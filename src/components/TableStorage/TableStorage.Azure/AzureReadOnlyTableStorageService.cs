using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Laso.TableStorage.Azure.Extensions;
using Laso.TableStorage.Domain;

namespace Laso.TableStorage.Azure
{
    public class AzureReadOnlyTableStorageService : IReadOnlyTableStorageService
    {
        protected readonly ITableStorageContext Context;

        private const int MaxResultSize = 1000;

        public AzureReadOnlyTableStorageService(ITableStorageContext context)
        {
            Context = context;
        }

        public async Task<T> GetAsync<T>(string partitionKey) where T : TableStorageEntity, ITableEntity, new()
        {
            if (string.IsNullOrWhiteSpace(partitionKey))
                throw new InvalidOperationException($"{nameof(partitionKey)} must be specified.");

            return await Context.GetAsync<T>(partitionKey);
        }

        public Task<ICollection<T>> FindAllAsync<T>(Expression<Func<T, bool>> filter) where T : TableStorageEntity, ITableEntity, new()
        {
            var queryHelper = new TableStorageQueryHelper(Context.GetPropertyColumnMappers());
            var filterString = queryHelper.GetFilter(filter);

            var findResults =  FindAllInternalAsync<T>(filter);

            return findResults;//.Select(GetEntity<T>).ToList();
        }

        public async Task<T> GetAsync<T>(string partitionKey, Expression<Func<T, bool>> select) where T : TableStorageEntity, ITableEntity, new()
        {
            if (string.IsNullOrWhiteSpace(partitionKey))
                throw new InvalidOperationException($"{nameof(partitionKey)} must be specified.");

            var queryHelper = new TableStorageQueryHelper(Context.GetPropertyColumnMappers());
            var (selectString, project) = queryHelper.GetSelect(select);

            var findResults = await FindAllInternalAsync<T>(g=>g.PartitionKey == partitionKey && g.RowKey == partitionKey, 1, selectString);

            return findResults.FirstOrDefault();//.Select(x => project(GetEntity<T>(x))).SingleOrDefault();
        }

        public async Task<ICollection<T>> GetAllAsync<T>(int? limit = null) where T : TableStorageEntity, ITableEntity, new()
        {

            var findResults = await FindAllInternalAsync<T>(g=> true, limit);

            return findResults;
        }



        public async Task<ICollection<T>> GetAllAsync<T>(Expression<Func<T, bool>> select, int? limit = null) where T : TableStorageEntity, ITableEntity, new()
        {
            var queryHelper = new TableStorageQueryHelper(Context.GetPropertyColumnMappers());
            
            var (selectString, project) = queryHelper.GetSelect(select);

            var findResults = await FindAllInternalAsync<T>(select, limit, selectString);

            return findResults;//.Select(x => project(GetEntity<T>(x))).ToList();
        }

        public async Task<ICollection<T>> FindAllAsync<T>(Expression<Func<T, bool>> filter, int? limit = null) where T :  TableStorageEntity, ITableEntity, new()
        {
            var queryHelper = new TableStorageQueryHelper(Context.GetPropertyColumnMappers());
            var filterString = queryHelper.GetFilter(filter);

            var findResults = await FindAllInternalAsync<T>(filter, limit);

            return findResults;//.Select(GetEntity<T>).ToList();
        }

        public async Task<ICollection<T>> FindAllAsync<T>(Expression<Func<T, bool>> filter, Expression<Func<T, bool>> select, int? limit = null) where T : TableStorageEntity, ITableEntity, new()
        {
            var queryHelper = new TableStorageQueryHelper(Context.GetPropertyColumnMappers());

            var filterString = queryHelper.GetFilter(filter);
            var (selectString, project) = queryHelper.GetSelect(select);

            var findResults = await FindAllInternalAsync<T>(filter, limit, selectString);

            return findResults;//.Select(x => project(GetEntity<T>(x))).ToList();
        }

        private static string GetFilter(string partitionKey)
        {
            if (partitionKey == null)
                return null;

            var filter = $"{nameof(TableStorageEntity.PartitionKey)} eq '{partitionKey}'";

            if (!string.IsNullOrWhiteSpace(partitionKey))
                filter += $" and {nameof(TableStorageEntity.RowKey)} eq '{partitionKey}'";

            return filter;
        }

        //TODO: update signature to return async IAsyncEnumerable<DynamicTableEntity> once language feature available (as well as calling methods)
        protected async Task<ICollection<T>> FindAllInternalAsync<T>(Expression<Func<T, bool>> query, int? limit = null, IList<string> select = null) where T : TableStorageEntity, ITableEntity, new()
        {
            var result = new List<T>();
            var table = Context.GetTable(typeof(T));

            CancellationToken continuationToken = default;

            //do
            //{
            //    var diff = limit - result.Count;

            //    if (diff < MaxResultSize)
            //        query.TakeCount = diff;

                var queryResult =  table.ExecuteQuerySegmentedAsync<T>(query, continuationToken);

                await foreach (var entity in queryResult)
                {
                    result.Add(entity);

                    if (result.Count == limit)
                        break;
                }

            //    continuationToken = queryResult.ContinuationToken;
            //} while (continuationToken != null && result.Count < limit);

            return result;
        }

        private T GetEntity<T>(TableEntity tableEntity) where T : TableStorageEntity, new()
        {
            var entity = new T();
            var columns = tableEntity.ToDictionary(x => x.Key, x => x.Value);
            var propertyColumnMappers = Context.GetPropertyColumnMappers();

            typeof(T)
                .GetProperties()
                .Where(x => x.CanWrite)
                .ForEach(x =>
                {
                    var value = propertyColumnMappers.MapToProperty(x, columns);

                    x.SetValue(entity, value);
                });

            entity.SetValue(e => e.ETag, tableEntity.ETag);
            entity.SetValue(e => e.Timestamp, tableEntity.Timestamp);

            return entity;
        }

     
    }
}