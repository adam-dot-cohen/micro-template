using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Laso.Provisioning.Core.Extensions;
using Laso.Provisioning.Core.Persistence;
using Laso.Provisioning.Domain.Entities;
using Microsoft.Azure.Cosmos.Table;

namespace Laso.Provisioning.Infrastructure.Persistence.Azure
{
    public class AzureReadOnlyTableStorageService : IReadOnlyTableStorageService
    {
        public readonly ITableStorageContext Context;

        private const int MaxResultSize = 1000;

        public AzureReadOnlyTableStorageService(ITableStorageContext context)
        {
            Context = context;
        }

        public async Task<T> GetAsync<T>(string partitionKey, string rowKey = null) where T : TableStorageEntity, new()
        {
            if (string.IsNullOrWhiteSpace(partitionKey))
                throw new InvalidOperationException($"{nameof(partitionKey)} must be specified.");

            var filter = $"PartitionKey eq '{partitionKey}'";

            if (string.IsNullOrWhiteSpace(rowKey))
                filter += $" and RowKey eq '{rowKey}'";

            return (await FindAllAsync<T>(filter, 1)).SingleOrDefault();
        }

        public async Task<ICollection<T>> GetAllAsync<T>(string partitionKey = null, int? limit = null) where T : TableStorageEntity, new()
        {
            var filter = partitionKey != null ? $"PartitionKey eq '{partitionKey}'" : null;

            return await FindAllAsync<T>(filter, limit);
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

        private static T GetEntity<T>(DynamicTableEntity tableEntity) where T : TableStorageEntity, new()
        {
            var entity = new T();
            var properties = typeof(T).GetProperties().ToDictionary(x => x.Name);

            tableEntity.Properties.ForEach(x =>
            {
                if (!properties.ContainsKey(x.Key))
                    return;

                var value = GetValue(properties[x.Key].PropertyType, x.Value.PropertyAsObject);
                properties[x.Key].SetValue(entity, value);
            });

            entity.SetValue(e => e.ETag, tableEntity.ETag);
            entity.SetValue(e => e.Timestamp, tableEntity.Timestamp);
            return entity;
        }

        private static object GetValue(Type type, object value)
        {
            var propertyType = type.GetNonNullableType();

            if (propertyType.IsEnum)
            {
                if (value != null && Enum.IsDefined(propertyType, value))
                    value = Enum.Parse(propertyType, (string)value);

                //Add new custom serializations here
            }

            return value;
        }
    }
}