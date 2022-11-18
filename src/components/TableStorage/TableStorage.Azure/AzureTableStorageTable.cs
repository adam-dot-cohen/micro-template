using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Laso.TableStorage.Azure.Extensions;
using Laso.TableStorage.Domain;

namespace Laso.TableStorage.Azure
{
    public class AzureTableStorageTable : ITable
    {
        private readonly TableClient _table;
        private readonly AzureTableStorageContext _context;
        private readonly IPropertyColumnMapper[] _propertyColumnMappers;

        internal AzureTableStorageTable(TableClient table, AzureTableStorageContext context, IPropertyColumnMapper[] propertyColumnMappers)
        {
            _table = table;
            _context = context;
            _propertyColumnMappers = propertyColumnMappers;
        }

        public void Insert<TEntity>(TEntity entity) where TEntity : TableStorageEntity
        {
            _context.Insert(entity);
        }

        public void InsertOrReplace<TEntity>(TEntity entity) where TEntity : TableStorageEntity
        {
            _context.InsertOrReplace(entity);
        }

        public void InsertOrMerge<TEntity>(TEntity entity) where TEntity : TableStorageEntity
        {
            _context.InsertOrMerge(entity);
        }

        public void Replace<TEntity>(TEntity entity) where TEntity : TableStorageEntity
        {
            _context.Replace(entity);
        }

        public void Merge<TEntity>(TEntity entity) where TEntity : TableStorageEntity
        {
            _context.Merge(entity);
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : TableStorageEntity
        {
            _context.Delete(entity);
        }

        public TEntity Get<TEntity>(string partitionKey) where TEntity : TableStorageEntity, new()
        {
            return _table.GetEntity<TEntity>(partitionKey, partitionKey);
        }
        public async Task<TEntity> GetAsync<TEntity>(string partitionKey) where TEntity : TableStorageEntity, new()
        {
            return await _table.GetEntityAsync<TEntity>(partitionKey, partitionKey);
        }

        public AsyncPageable<T> ExecuteQuerySegmentedAsync<T>(Expression<Func<T,bool>> query, CancellationToken continuationToken) where T : class, ITableEntity, new()
        {
            return _table.QueryAsync<T>(query,cancellationToken: continuationToken);
        }

        private ITableEntity GetTableEntity<T>(T entity) where T : TableStorageEntity
        {
            var properties = typeof(T)
                .GetProperties()
                .SelectMany(x => _propertyColumnMappers.MapToColumns(x, x.GetValue(entity)))
                .ToDictionary(a => a.Key, a =>a.Value);

            var tableEntity = new TableEntity(properties);

            return new MappedTableEntity<T>(entity, tableEntity);
        }

        /// <summary>
        /// Maps Timestamp and Etag back to entity on save
        /// </summary>
        private class MappedTableEntity<T> : ITableEntity where T : TableStorageEntity
        {
            private readonly T _entity;
            private readonly ITableEntity _tableEntity;

            public MappedTableEntity(T entity, ITableEntity tableEntity)
            {
                _entity = entity;
                _tableEntity = tableEntity;
            }

         

            public string PartitionKey
            {
                get => _tableEntity.PartitionKey;
                set => _tableEntity.PartitionKey = value;
            }

            public string RowKey
            {
                get => _tableEntity.RowKey;
                set => _tableEntity.RowKey = value;
            }

            DateTimeOffset? ITableEntity.Timestamp
            
            {
                get => _tableEntity.Timestamp;
                set
                {
                    _tableEntity.Timestamp = value;
                    _entity.SetValue(e => e.Timestamp, value);
                }
            }

            public ETag ETag 
            {
                get => _tableEntity.ETag;
                set
                {
                    _tableEntity.ETag = value;
                    _entity.SetValue(e => e.ETag, value);
                }
            }
            
        }
    }
}