using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Laso.TableStorage.Azure.Extensions;

namespace Laso.TableStorage.Azure
{
    public class AzureTableStorageService : AzureReadOnlyTableStorageService, ITableStorageService
    {
        public AzureTableStorageService(ITableStorageContext context) : base(context) { }

        public async Task InsertAsync<T>(T entity) where T : TableStorageEntity
        {
            var table = Context.GetTable(typeof(T));
            table.Insert(entity);
            await Context.SaveChangesAsync();
        }

        public async Task InsertAsync<T>(IEnumerable<T> entities) where T : TableStorageEntity
        {
            var table = Context.GetTable(typeof(T));
            entities.ForEach(table.Insert);
            await Context.SaveChangesAsync();
        }

        public async Task InsertOrReplaceAsync<T>(T entity) where T : TableStorageEntity
        {
            var table = Context.GetTable(typeof(T));
            table.InsertOrReplace(entity);
            await Context.SaveChangesAsync();
        }

        public async Task InsertOrReplaceAsync<T>(IEnumerable<T> entities) where T : TableStorageEntity
        {
            var table = Context.GetTable(typeof(T));
            entities.ForEach(table.InsertOrReplace);
            await Context.SaveChangesAsync();
        }

        public async Task DeleteAsync<T>(string partitionKey, string rowKey = null) where T : TableStorageEntity, new()
        {
            if (string.IsNullOrWhiteSpace(partitionKey))
                throw new InvalidOperationException($"{nameof(partitionKey)} must be specified.");

            var filter = $"{nameof(TableStorageEntity.PartitionKey)} eq '{partitionKey}'";

            if (string.IsNullOrWhiteSpace(rowKey))
                filter += $" and {nameof(TableStorageEntity.RowKey)} eq '{rowKey}'";

            var entities = await FindAllInternalAsync<T>(filter);

            entities.ForEach(x => Context.Delete(typeof(T), x));
            await Context.SaveChangesAsync();
        }

        public async Task DeleteAsync<T>(T entity) where T : TableStorageEntity
        {
            var table = Context.GetTable(typeof(T));
            table.Delete(entity);
            await Context.SaveChangesAsync();
        }

        public async Task DeleteAsync<T>(IEnumerable<T> entities) where T : TableStorageEntity
        {
            var table = Context.GetTable(typeof(T));
            entities.ForEach(table.Delete);
            await Context.SaveChangesAsync();
        }

        public async Task TruncateAsync<T>() where T : TableStorageEntity, new()
        {
            var entities = await FindAllInternalAsync<T>();

            entities.ForEach(e => Context.Delete(typeof(T), e));

            await Context.SaveChangesAsync();
        }
    }
}