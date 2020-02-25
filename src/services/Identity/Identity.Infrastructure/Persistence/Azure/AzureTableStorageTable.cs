using System.Linq;
using System.Threading.Tasks;
using Laso.Identity.Domain.Entities;
using Microsoft.Azure.Cosmos.Table;

namespace Laso.Identity.Infrastructure.Persistence.Azure
{
    public class AzureTableStorageTable : ITable
    {
        private readonly CloudTable _table;
        private readonly AzureTableStorageContext _context;

        internal AzureTableStorageTable(CloudTable table, AzureTableStorageContext context)
        {
            _table = table;
            _context = context;
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

        private static ITableEntity GetTableEntity<T>(T entity) where T : TableStorageEntity
        {
            var properties = typeof(T)
                .GetProperties()
                .SelectMany(x => PropertyColumnMapper.MapToColumns(x, x.GetValue(entity)))
                .ToDictionary(a => a.Key, a => EntityProperty.CreateEntityPropertyFromObject(a.Value));

            return new DynamicTableEntity(entity.PartitionKey, entity.RowKey, entity.ETag, properties);
        }
    }
}