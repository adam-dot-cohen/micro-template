using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Laso.Identity.Core.Extensions;
using Microsoft.Azure.Cosmos.Table;

namespace Laso.Identity.Infrastructure.Persistence.Azure
{
    public class AzureTableStorageContext : ITableStorageContext
    {
        private const int MaxTableBatchSize = 100;              // TableConstants.TableServiceBatchMaximumOperations;
        private const int MaxStringPropertySizeInBytes = 65536; // TableConstants.TableServiceMaxStringPropertySizeInBytes

        private readonly Inflector.Inflector _inflector;
        private readonly Lazy<CloudTableClient> _client;
        private readonly ISaveChangesDecorator[] _saveChangesDecorators;

        private readonly ConcurrentDictionary<Type, TableContext> _entityTypeTableContexts = new ConcurrentDictionary<Type, TableContext>();

        private class TableContext
        {
            public ITable Table { get; set; }
            public CloudTable CloudTable { get; set; }
            public ConcurrentDictionary<string, ICollection<TableOperation>> PartitionOperations { get; set; }
        }

        public AzureTableStorageContext(string connectionString, ISaveChangesDecorator[] saveChangesDecorators)
        {
            _saveChangesDecorators = saveChangesDecorators;
            _inflector = new Inflector.Inflector(CultureInfo.CurrentCulture);
            _client = new Lazy<CloudTableClient>(() => CreateCloudTableClient(connectionString));
        }

        public IEnumerable<TableEntityState> GetEntityStates()
        {
            var entityStates = _entityTypeTableContexts
                .Select(c => new
                {
                    EntityType = c.Key,
                    Operations = c.Value.PartitionOperations.SelectMany(o => o.Value)
                })
                .SelectMany(a => a.Operations.Select(o =>
                    new TableEntityState
                    {
                        Entity = o.Entity,
                        EntityType = a.EntityType,
                        EntityKey = $"{o.Entity.PartitionKey}:{o.Entity.RowKey}",
                        OperationType = o.OperationType
                    }));

            return entityStates;
        }

        private static CloudTableClient CreateCloudTableClient(string connectionString)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var servicePoint = ServicePointManager.FindServicePoint(account.TableEndpoint);

            servicePoint.UseNagleAlgorithm = false;
            servicePoint.Expect100Continue = false;
            servicePoint.ConnectionLimit = 1000;

            return account.CreateCloudTableClient();
        }

        public void Insert(Type entityType, ITableEntity tableEntity)
        {
            AddTableOperation(entityType, tableEntity, TableOperation.Insert);
        }

        public void InsertOrReplace(Type entityType, ITableEntity tableEntity)
        {
            AddTableOperation(entityType, tableEntity, TableOperation.InsertOrReplace);
        }

        public void InsertOrMerge(Type entityType, ITableEntity tableEntity)
        {
            AddTableOperation(entityType, tableEntity, TableOperation.InsertOrMerge);
        }

        public void Replace(Type entityType, ITableEntity tableEntity)
        {
            AddTableOperation(entityType, tableEntity, TableOperation.Replace);
        }

        public void Merge(Type entityType, ITableEntity tableEntity)
        {
            AddTableOperation(entityType, tableEntity, TableOperation.Merge);
        }

        public void Delete(Type entityType, ITableEntity tableEntity)
        {
            AddTableOperation(entityType, tableEntity, TableOperation.Delete);
        }

        private void AddTableOperation(Type entityType, ITableEntity tableEntity, Func<ITableEntity, TableOperation> operation)
        {
            var tableContext = GetTableContext(entityType);
            var partitionOperations = tableContext.PartitionOperations.GetOrAdd(tableEntity.PartitionKey, new List<TableOperation>());
            partitionOperations.Add(operation(tableEntity));
        }

        //TODO: async?
        private TableContext GetTableContext(Type entityType)
        {
            return _entityTypeTableContexts.GetOrAdd(entityType, entityType1 =>
            {
                var tableName = _inflector.Pluralize(entityType1.Name);
                var cloudTable = _client.Value.GetTableReference(tableName);
                cloudTable.CreateIfNotExists();

                return new TableContext
                {
                    CloudTable = cloudTable,
                    Table = new AzureTableStorageTable(cloudTable, this),
                    PartitionOperations = new ConcurrentDictionary<string, ICollection<TableOperation>>()
                };
            });
        }

        public ITable GetTable(Type entityType)
        {
            var tableContext = GetTableContext(entityType);

            return tableContext.Table;
        }

        public async Task<ICollection<TableResult>> SaveChangesAsync()
        {
            Func<Task<ICollection<TableResult>>> saveChanges = InternalSaveChangesAsync;

            foreach (var decorator in _saveChangesDecorators)
            {
                var newContext = new SaveChangesContext(this, saveChanges);

                var localDecorator = decorator;
                saveChanges = () => localDecorator.DecorateAsync(newContext);
            }

            return await saveChanges();
        }

        private async Task<ICollection<TableResult>> InternalSaveChangesAsync()
        {
            var results = new List<TableResult>();

            foreach (var tableContext in _entityTypeTableContexts.Values)
            {
                var partitionOperations = tableContext.PartitionOperations;

                foreach (var operations in partitionOperations)
                {
                    foreach (var batch in operations.Value.Batch(MaxTableBatchSize))
                    {
                        var batchOperation = new TableBatchOperation();
                        batchOperation.AddRange(batch);

                        results.AddRange(await tableContext.CloudTable.ExecuteBatchAsync(batchOperation));
                    }
                }

                partitionOperations.Clear();
            }

            return results;
        }
    }
}