using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Laso.TableStorage.Domain;

namespace Laso.TableStorage.Azure
{
    public interface ITableStorageContext
    {
        void Insert<T>(T tableEntity) where T : ITableEntity;
        void InsertOrReplace<T>(T tableEntity) where T : ITableEntity;
        void InsertOrMerge<T>(T tableEntity) where T : ITableEntity;
        void Replace<T>(T tableEntity) where T : ITableEntity;
        void Merge<T>(T tableEntity) where T : ITableEntity;
        void Delete<T>(T tableEntity) where T : ITableEntity;
        T Get<T>(string partitionKey) where T : TableStorageEntity, new();
        Task<T> GetAsync<T>(string partitionKey) where T : TableStorageEntity, new();
        void DeleteTable(TableItem table);
        ITable GetTable(Type entityType);
        Task SaveChangesAsync();

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
        Task<TEntity> GetAsync<TEntity>(string partitionKey) where TEntity : TableStorageEntity, new();
        AsyncPageable<T> ExecuteQuerySegmentedAsync<T>(Expression<Func<T, bool>> query,
            CancellationToken continuationToken) where T : class, ITableEntity, new();
    }

    public class TableEntityState
    {
        public ITableEntity Entity { get; set; }
        public Type EntityType { get; set; }
        public string EntityKey { get; set; }
    }
}