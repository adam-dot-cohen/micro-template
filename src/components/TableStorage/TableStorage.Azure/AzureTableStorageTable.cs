using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Laso.TableStorage.Azure.Extensions;
using Laso.TableStorage.Domain;
using Microsoft.Azure.Cosmos.Table;

namespace Laso.TableStorage.Azure
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

            var tableEntity = new DynamicTableEntity(entity.PartitionKey, entity.RowKey, entity.ETag, properties);

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

            public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
            {
                _tableEntity.ReadEntity(properties, operationContext);
            }

            public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
            {
                return _tableEntity.WriteEntity(operationContext);
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

            public DateTimeOffset Timestamp
            {
                get => _tableEntity.Timestamp;
                set
                {
                    _tableEntity.Timestamp = value;
                    _entity.SetValue(e => e.Timestamp, value);
                }
            }

            public string ETag
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