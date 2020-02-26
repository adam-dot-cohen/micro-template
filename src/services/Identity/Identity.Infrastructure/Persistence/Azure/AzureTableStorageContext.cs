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
        private const int MaxTableNameLength = 63;
        private const int MaxTableBatchSize = 100;              // TableConstants.TableServiceBatchMaximumOperations;
        private const int MaxStringPropertySizeInBytes = 65536; // TableConstants.TableServiceMaxStringPropertySizeInBytes

        private readonly Lazy<CloudTableClient> _client;
        private readonly string _tablePrefix;
        private readonly ISaveChangesDecorator[] _saveChangesDecorators;
        private readonly IPropertyColumnMapper[] _propertyColumnMappers;

        private readonly Inflector.Inflector _inflector = new Inflector.Inflector(CultureInfo.CurrentCulture);
        private readonly ConcurrentDictionary<Type, TableContext> _entityTypeTableContexts = new ConcurrentDictionary<Type, TableContext>();

        private class TableContext
        {
            public ITable Table { get; set; }
            public CloudTable CloudTable { get; set; }
            public ConcurrentDictionary<string, ICollection<TableOperation>> PartitionOperations { get; set; }
        }

        public AzureTableStorageContext(string connectionString, string tablePrefix = null, ISaveChangesDecorator[] saveChangesDecorators = null, IPropertyColumnMapper[] propertyColumnMappers = null)
        {
            _client = new Lazy<CloudTableClient>(() => CreateCloudTableClient(connectionString));
            _tablePrefix = tablePrefix;
            _saveChangesDecorators = saveChangesDecorators;
            _propertyColumnMappers = propertyColumnMappers;
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

        private TableContext GetTableContext(Type entityType)
        {
            return _entityTypeTableContexts.GetOrAdd(entityType, x =>
            {
                var tableName = ((_tablePrefix ?? "").ToLower() + _inflector.Pluralize(x.Name)).Truncate(MaxTableNameLength);
                var cloudTable = _client.Value.GetTableReference(tableName);
                cloudTable.CreateIfNotExists(); //TODO: async?

                return new TableContext
                {
                    CloudTable = cloudTable,
                    Table = new AzureTableStorageTable(cloudTable, this, _propertyColumnMappers),
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

            foreach (var decorator in _saveChangesDecorators ?? Enumerable.Empty<ISaveChangesDecorator>())
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

        public IEnumerable<TableEntityState> GetEntityStates()
        {
            var entityStates = _entityTypeTableContexts
                .Select(c => new
                {
                    EntityType = c.Key,
                    Operations = c.Value.PartitionOperations.SelectMany(o => o.Value)
                })
                .SelectMany(a => a.Operations.Select(o => new TableEntityState
                {
                    Entity = o.Entity,
                    EntityType = a.EntityType,
                    EntityKey = $"{o.Entity.PartitionKey}:{o.Entity.RowKey}",
                    OperationType = o.OperationType
                }));

            return entityStates;
        }

        public IPropertyColumnMapper[] GetPropertyColumnMappers()
        {
            return _propertyColumnMappers;
        }
    }
}