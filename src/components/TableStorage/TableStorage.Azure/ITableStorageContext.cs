using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace Laso.TableStorage.Azure
{
    public interface ITableStorageContext
    {
        void Insert(Type entityType, ITableEntity tableEntity);
        void InsertOrReplace(Type entityType, ITableEntity tableEntity);
        void InsertOrMerge(Type entityType, ITableEntity tableEntity);
        void Replace(Type entityType, ITableEntity tableEntity);
        void Merge(Type entityType, ITableEntity tableEntity);
        void Delete(Type entityType, ITableEntity tableEntity);

        ITable GetTable(Type entityType);
        Task<ICollection<TableResult>> SaveChangesAsync();

        IEnumerable<TableEntityState> GetEntityStates();
        IPropertyColumnMapper[] GetPropertyColumnMappers();
    }

    public interface ITable
    {
        void Insert<TEntity>(TEntity entity) where TEntity : TableStorageEntity;
        void InsertOrReplace<TEntity>(TEntity entity) where TEntity : TableStorageEntity;
        void InsertOrMerge<TEntity>(TEntity entity) where TEntity : TableStorageEntity;
        void Replace<TEntity>(TEntity entity) where TEntity : TableStorageEntity;
        void Merge<TEntity>(TEntity entity) where TEntity : TableStorageEntity;
        void Delete<TEntity>(TEntity entity) where TEntity : TableStorageEntity;

        Task<TableQuerySegment<DynamicTableEntity>> ExecuteQuerySegmentedAsync(TableQuery query, TableContinuationToken continuationToken);
    }

    public class TableEntityState
    {
        public ITableEntity Entity { get; set; }
        public Type EntityType { get; set; }
        public string EntityKey { get; set; }
        public TableOperationType OperationType { get; set; }
    }
}