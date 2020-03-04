using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Laso.Identity.Core.Extensions;
using Laso.Identity.Core.Persistence;
using Laso.Identity.Domain.Entities;
using Laso.Identity.Infrastructure.Extensions;
using Microsoft.Azure.Cosmos.Table;

namespace Laso.Identity.Infrastructure.Persistence.Azure
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

            var filterString = GetFilter(partitionKey, rowKey);

            var findResults = await FindAllInternalAsync<T>(filterString, 1);

            return findResults.Select(GetEntity<T>).SingleOrDefault();
        }

        public async Task<TResult> GetAsync<T, TResult>(string partitionKey, Expression<Func<T, TResult>> select, string rowKey = null) where T : TableStorageEntity, new()
        {
            if (string.IsNullOrWhiteSpace(partitionKey))
                throw new InvalidOperationException($"{nameof(partitionKey)} must be specified.");

            var queryHelper = new TableStorageQueryHelper(Context.GetPropertyColumnMappers());
            var filterString = GetFilter(partitionKey, rowKey);
            var (selectString, project) = queryHelper.GetSelect(select);

            var findResults = await FindAllInternalAsync<T>(filterString, 1, selectString);

            return findResults.Select(x => project(GetEntity<T>(x))).SingleOrDefault();
        }

        public async Task<ICollection<T>> GetAllAsync<T>(string partitionKey = null, int? limit = null) where T : TableStorageEntity, new()
        {
            var filterString = GetFilter(partitionKey);

            var findResults = await FindAllInternalAsync<T>(filterString, limit);

            return findResults.Select(GetEntity<T>).ToList();
        }

        public async Task<ICollection<TResult>> GetAllAsync<T, TResult>(Expression<Func<T, TResult>> select, string partitionKey = null, int? limit = null) where T : TableStorageEntity, new()
        {
            var queryHelper = new TableStorageQueryHelper(Context.GetPropertyColumnMappers());

            var filterString = GetFilter(partitionKey);
            var (selectString, project) = queryHelper.GetSelect(select);

            var findResults = await FindAllInternalAsync<T>(filterString, limit, selectString);

            return findResults.Select(x => project(GetEntity<T>(x))).ToList();
        }

        public async Task<ICollection<T>> FindAllAsync<T>(Expression<Func<T, bool>> filter, int? limit = null) where T : TableStorageEntity, new()
        {
            var queryHelper = new TableStorageQueryHelper(Context.GetPropertyColumnMappers());
            var filterString = queryHelper.GetFilter(filter);

            var findResults = await FindAllInternalAsync<T>(filterString, limit);

            return findResults.Select(GetEntity<T>).ToList();
        }

        public async Task<ICollection<TResult>> FindAllAsync<T, TResult>(Expression<Func<T, bool>> filter, Expression<Func<T, TResult>> select, int? limit = null) where T : TableStorageEntity, new()
        {
            var queryHelper = new TableStorageQueryHelper(Context.GetPropertyColumnMappers());

            var filterString = queryHelper.GetFilter(filter);
            var (selectString, project) = queryHelper.GetSelect(select);

            var findResults = await FindAllInternalAsync<T>(filterString, limit, selectString);

            return findResults.Select(x => project(GetEntity<T>(x))).ToList();
        }

        private static string GetFilter(string partitionKey, string rowKey = null)
        {
            if (partitionKey == null)
                return null;

            var filter = $"{nameof(TableStorageEntity.PartitionKey)} eq '{partitionKey}'";

            if (string.IsNullOrWhiteSpace(rowKey))
                filter += $" and {nameof(TableStorageEntity.RowKey)} eq '{rowKey}'";

            return filter;
        }

        protected async Task<ICollection<DynamicTableEntity>> FindAllInternalAsync<T>(string filter = null, int? limit = null, IList<string> select = null) where T : TableStorageEntity, new()
        {
            var result = new List<DynamicTableEntity>();
            var table = Context.GetTable(typeof(T));

            var query = new TableQuery();

            if (!string.IsNullOrWhiteSpace(filter))
                query.FilterString = filter;

            if (select != null)
                query.SelectColumns = select;

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