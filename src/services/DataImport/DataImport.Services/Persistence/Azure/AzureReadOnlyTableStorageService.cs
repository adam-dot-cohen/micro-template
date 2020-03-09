using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Laso.DataImport.Core.Extensions;
using Laso.DataImport.Domain.Entities;
using Microsoft.Azure.Cosmos.Table;

namespace Laso.DataImport.Services.Persistence.Azure
{
    public class AzureReadOnlyTableStorageService : IReadOnlyTableStorageService
    {
        protected readonly ITableStorageContext Context;

        private const int MaxResultSize = 1000;

        public AzureReadOnlyTableStorageService(ITableStorageContext context)
        {
            Context = context;
        }

        public async Task<T> GetAsync<T>(string partitionKey, string rowKey = null) where T : TableStorageEntity, new()
        {
            if (string.IsNullOrWhiteSpace(partitionKey))
                throw new InvalidOperationException($"{nameof(partitionKey)} must be specified.");

            var filter = $"{nameof(TableStorageEntity.PartitionKey)} eq '{partitionKey}'";

            if (string.IsNullOrWhiteSpace(rowKey))
                filter += $" and {nameof(TableStorageEntity.RowKey)} eq '{rowKey}'";

            return (await FindAllAsync<T>(filter, 1)).SingleOrDefault();
        }

        public async Task<ICollection<T>> GetAllAsync<T>(string partitionKey = null, int? limit = null) where T : TableStorageEntity, new()
        {
            var filter = partitionKey != null ? $"{nameof(TableStorageEntity.PartitionKey)} eq '{partitionKey}'" : null;

            return await FindAllAsync<T>(filter, limit);
        }

        public async Task<ICollection<T>> FindAllAsync<T>(Expression<Func<T, bool>> filter, int? limit = null) where T : TableStorageEntity, new()
        {
            return await FindAllAsync<T>(new TableStorageQueryHelper(Context.GetPropertyColumnMappers()).GetFilter(filter), limit);
        }

        public async Task<ICollection<T>> FindAllAsync<T>(string filter = null, int? limit = null) where T : TableStorageEntity, new()
        {
            return (await FindAllInternalAsync<T>(filter, limit)).Select(GetEntity<T>).ToList();
        }

        protected async Task<ICollection<DynamicTableEntity>> FindAllInternalAsync<T>(string filter = null, int? limit = null) where T : TableStorageEntity, new()
        {
            var result = new List<DynamicTableEntity>();
            var table = Context.GetTable(typeof(T));

            var query = new TableQuery();

            if (!string.IsNullOrWhiteSpace(filter))
                query.FilterString = filter;

            TableContinuationToken continuationToken = null;
            var count = 0;

            do
            {
                var diff = limit - count;

                if (diff < MaxResultSize)
                    query.TakeCount = diff;

                var queryResult = await table.ExecuteQuerySegmentedAsync(query, continuationToken);

                foreach (var entity in queryResult)
                {
                    result.Add(entity);

                    if (++count == limit)
                        break;
                }

                continuationToken = queryResult.ContinuationToken;
            } while (continuationToken != null);

            return result;
        }

        private T GetEntity<T>(DynamicTableEntity tableEntity) where T : TableStorageEntity, new()
        {
            var entity = new T();
            var columns = tableEntity.Properties.ToDictionary(x => x.Key, x => x.Value.PropertyAsObject);
            var propertyColumnMappers = Context.GetPropertyColumnMappers();

            typeof(T)
                .GetProperties()
                .Where(x => x.CanWrite)
                .ForEach(x => x.SetValue(entity, propertyColumnMappers.MapToProperty(x, columns)));

            entity.SetValue(e => e.ETag, tableEntity.ETag);
            entity.SetValue(e => e.Timestamp, tableEntity.Timestamp);

            return entity;
        }
    }
}