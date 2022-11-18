using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Laso.TableStorage.Azure.Extensions;
using Laso.TableStorage.Domain;

namespace Laso.TableStorage.Azure
{
    public class AzureTableStorageContext : ITableStorageContext
    {
        private const int MaxTableNameLength = 63;
        private const int MaxTableBatchSize = 100;              // TableConstants.TableServiceBatchMaximumOperations;
        private const int MaxStringPropertySizeInBytes = 65536; // TableConstants.TableServiceMaxStringPropertySizeInBytes

        private readonly Lazy<TableServiceClient> _client;
        private readonly string _tablePrefix;
        private readonly ISaveChangesDecorator[] _saveChangesDecorators;
        private readonly IPropertyColumnMapper[] _propertyColumnMappers;

        private readonly Inflector.Inflector _inflector = new Inflector.Inflector(new CultureInfo("en-us"));
        private readonly ConcurrentDictionary<Type, TableContext> _entityTypeTableContexts = new ConcurrentDictionary<Type, TableContext>();

        private class TableContext
        {
            public ITable Table { get; set; }
            public TableClient CloudTable { get; set; }
        }

        public AzureTableStorageContext(string connectionString, string tablePrefix = null, ISaveChangesDecorator[] saveChangesDecorators = null, IPropertyColumnMapper[] propertyColumnMappers = null)
        {
            _client = new Lazy<TableServiceClient>(() => CreateCloudTableClient(connectionString));
            _tablePrefix = tablePrefix;
            _saveChangesDecorators = saveChangesDecorators;
            _propertyColumnMappers = propertyColumnMappers;
        }

        private static TableServiceClient CreateCloudTableClient(string connectionString)
        {
            // var account = CloudStorageAccount.Parse(connectionString); 
            //todo:  var servicePoint = ServicePointManager. account.TableEndpoint);

            //servicePoint.UseNagleAlgorithm = false;
            //servicePoint.Expect100Continue = false;
            //servicePoint.ConnectionLimit = 1000;
            return new TableServiceClient(connectionString);
            //return account.CreateCloudTableClient();
        }
        public void Insert<T>(T tableEntity) where T : ITableEntity
        {
            if (_entityTypeTableContexts.TryGetValue(typeof(T), out var context))
                context.CloudTable.AddEntity(tableEntity);
            else
                throw new Exception($"Database operation failed for {typeof(T).Name}");
        }
        
        public void InsertOrReplace<T>(T tableEntity) where T : ITableEntity
        {
            if (_entityTypeTableContexts.TryGetValue(typeof(T), out var context))
                context.CloudTable.UpsertEntity(tableEntity, TableUpdateMode.Replace);
            else
                throw new Exception($"Database operation failed for {typeof(T).Name}");
        }

        public void InsertOrMerge<T>(T tableEntity) where T : ITableEntity
        {
            if (_entityTypeTableContexts.TryGetValue(typeof(T), out var context))
                context.CloudTable.UpsertEntity(tableEntity, TableUpdateMode.Merge);
            else
                throw new Exception($"Database operation failed for {typeof(T).Name}");
        }

        public void Replace<T>(T tableEntity) where T : ITableEntity
        {
            if (_entityTypeTableContexts.TryGetValue(typeof(T), out var context))
                context.CloudTable.UpdateEntity(tableEntity, tableEntity.ETag, TableUpdateMode.Replace);
            else
                throw new Exception($"Database operation failed for {typeof(T).Name}");
        }

        public void Merge<T>(T tableEntity) where T : ITableEntity
        {
            if (_entityTypeTableContexts.TryGetValue(typeof(T), out var context))
                context.CloudTable.UpsertEntity(tableEntity, TableUpdateMode.Merge);
            else
                throw new Exception($"Database operation failed for {typeof(T).Name}");
        }

        public void Delete<T>(T tableEntity) where T : ITableEntity
        {
            if (_entityTypeTableContexts.TryGetValue(typeof(T), out var context))
                context.CloudTable.DeleteEntity(tableEntity.PartitionKey, tableEntity.PartitionKey);
            else
                throw new Exception($"Database operation failed for {typeof(T).Name}");

        }
        public T Get<T>(string partitionKey) where T : TableStorageEntity, new()
        {
            if (_entityTypeTableContexts.TryGetValue(typeof(T), out var context))
                return context.CloudTable.GetEntity<T>(partitionKey, partitionKey);
       
            throw new Exception($"Database operation failed for {typeof(T).Name}");
        }
        public async Task<T> GetAsync<T>(string partitionKey) where T : TableStorageEntity, new()
        {
            if (_entityTypeTableContexts.TryGetValue(typeof(T), out var context))
            {
                var result = await context.CloudTable.GetEntityIfExistsAsync<T>(partitionKey, partitionKey);
                return (result.HasValue ? result.Value : default);
            }
       
            throw new Exception($"Database operation failed for {typeof(T).Name}");
        }
        public void DeleteTable(TableItem table)
        {
            _client.Value.DeleteTable(table.Name);

        }
        private void AddTableOperation(Type entityType, ITableEntity tableEntity)
        {
            ValidatePartitionAndRowKeys(tableEntity);
            var tableContext = GetTableContext(entityType);
            _client.Value.CreateTable(entityType.Name);
        }

        private static void ValidatePartitionAndRowKeys(ITableEntity tableEntity)
        {
            if (HasIllegalCharacters(tableEntity.PartitionKey))
                throw new Exception($"{nameof(tableEntity.PartitionKey)} contains illegal characters: \"{tableEntity.PartitionKey}\"");

            if (HasIllegalCharacters(tableEntity.RowKey))
                throw new Exception($"{nameof(tableEntity.RowKey)} contains illegal characters: \"{tableEntity.RowKey}\"");
        }
        private static bool HasIllegalCharacters(string key)
        {
            //ReadOnlySpan<char> keySpan = key.AsSpan();
            //keySpan.
            foreach (var c in key.Where(x =>
                     {
                         var code = (int)x;

                         return x is '/' or '\\' or '#' or '?' || code is >= 0 and <= 31 or >= 127 and <= 159;
                     }))
                return true;
            return false;
        }

        public class Test : IComparable<char>
        {
            public int CompareTo(char other)
            {
                throw new NotImplementedException();
            }
        }

        private TableContext GetTableContext(Type entityType)
            => _entityTypeTableContexts.GetOrAdd(entityType, x =>
            {
                var tableName = //((_tablePrefix ?? "").ToLower() + 
                    _inflector.Pluralize(x.Name)//)
                    .Truncate(MaxTableNameLength);
                var tableItem = _client.Value.CreateTableIfNotExistsAsync(tableName).GetAwaiter().GetResult();
                if (!tableItem.HasValue) throw new Exception("failed to create table");
                var cloudTable = _client.Value.GetTableClient(tableItem.Value.Name);

                return new TableContext
                {
                    CloudTable = cloudTable,
                    Table = new AzureTableStorageTable(cloudTable, this, _propertyColumnMappers)
                };
            });


        public ITable GetTable(Type entityType)
        {
            var tableContext = GetTableContext(entityType);

            return tableContext.Table;
        }

        public async Task SaveChangesAsync()
        {
            //Func<Task<ICollection<TableResult>>> saveChanges = InternalSaveChangesAsync;

            //foreach (var decorator in _saveChangesDecorators ?? Enumerable.Empty<ISaveChangesDecorator>())
            //{
            //    var newContext = new SaveChangesContext(this, saveChanges);

            //    var localDecorator = decorator;
            //    saveChanges = () => localDecorator.DecorateAsync(newContext);
            //}

            //return await saveChanges();
            await Task.CompletedTask;
        }

        //private async Task<ICollection<TableResult>> InternalSaveChangesAsync()
        //{
        //    var results = new List<TableResult>();

        //    foreach (var tableContext in _entityTypeTableContexts.Values)
        //    {
        //        var partitionOperations = tableContext.PartitionOperations;

        //        foreach (var operations in partitionOperations)
        //        {
        //            foreach (var batch in operations.Value.Batch(MaxTableBatchSize))
        //            {
        //                var batchOperation = new TableBatchOperation();
        //                batchOperation.AddRange(batch);

        //                results.AddRange(await tableContext.CloudTable.ExecuteBatchAsync(batchOperation));
        //            }
        //        }

        //        partitionOperations.Clear();
        //    }

        //    return results;
        //}

        public IEnumerable<TableEntityState> GetEntityStates()
        {
            //var entityStates = _entityTypeTableContexts
            //    .Select(c => new
            //    {
            //        EntityType = c.Key,
            //        Operations = c.Value.PartitionOperations.SelectMany(o => o.Value)
            //    })
            //    .SelectMany(a => a.Operations.Select(o => new TableEntityState
            //    {
            //        Entity = o.Entity,
            //        EntityType = a.EntityType,
            //        EntityKey = $"{o.Entity.PartitionKey}:{o.Entity.RowKey}",
            //        OperationType = o.OperationType
            //    }));

            //return entityStates;
            return Enumerable.Empty<TableEntityState>();
        }

        public IPropertyColumnMapper[] GetPropertyColumnMappers()
        {
            return _propertyColumnMappers;
        }

        protected IEnumerable<TableItem> GetTables()
        {
            return _client.Value.Query();
        }
    }
}