using System.Linq;
using System.Threading.Tasks;
using Laso.DataImport.Domain.Entities;
using Microsoft.Azure.Cosmos.Table;

namespace Laso.DataImport.Services.Persistence.Azure
{
    public class AzureTableStorageTable : ITable
    {
        private readonly CloudTable _table;
        private readonly AzureTableStorageContext _context;
        private readonly IPropertyColumnMapper[] _propertyColumnMappers;

        internal AzureTableStorageTable(CloudTable table, AzureTableStorageContext context, IPropertyColumnMapper[] propertyColumnMappers)
        {
            _table = table;
            _context = context;
            _propertyColumnMappers = propertyColumnMappers;
        }

        public void Insert<TEntity>(TEntity entity) where TEntity : TableStorageEntity
        {
            var tableEntity = GetTableEntity(entity);
            _context.Insert(entity.GetType(), tableEntity);
        }

        public void InsertOrReplace<TEntity>(TEntity entity) where TEntity : TableStorageEntity
        {
            var tableEntity = GetTableEntity(entity);
            _context.InsertOrReplace(entity.GetType(), tableEntity);
        }

        public void InsertOrMerge<TEntity>(TEntity entity) where TEntity : TableStorageEntity
        {
            var tableEntity = GetTableEntity(entity);
            _context.InsertOrMerge(entity.GetType(), tableEntity);
        }

        public void Replace<TEntity>(TEntity entity) where TEntity : TableStorageEntity
        {
            var tableEntity = GetTableEntity(entity);
            _context.Replace(entity.GetType(), tableEntity);
        }

        public void Merge<TEntity>(TEntity entity) where TEntity : TableStorageEntity
        {
            var tableEntity = GetTableEntity(entity);
            _context.Merge(entity.GetType(), tableEntity);
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : TableStorageEntity
        {
            var tableEntity = GetTableEntity(entity);
            _context.Delete(entity.GetType(), tableEntity);
        }

        public async Task<TableQuerySegment<DynamicTableEntity>> ExecuteQuerySegmentedAsync(TableQuery query, TableContinuationToken continuationToken)
        {
            return await _table.ExecuteQuerySegmentedAsync(query, continuationToken);
        }

        private ITableEntity GetTableEntity<T>(T entity) where T : TableStorageEntity
        {
            var properties = typeof(T)
                .GetProperties()
                .SelectMany(x => _propertyColumnMappers.MapToColumns(x, x.GetValue(entity)))
                .ToDictionary(a => a.Key, a => EntityProperty.CreateEntityPropertyFromObject(a.Value));

            return new DynamicTableEntity(entity.PartitionKey, entity.RowKey, entity.ETag, properties);
        }
    }
}