using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Laso.TableStorage.Azure.Extensions;
using Laso.TableStorage.Domain;

namespace Laso.TableStorage.Azure
{
    public class AzureTableStorageService : AzureReadOnlyTableStorageService, ITableStorageService
    {
        public AzureTableStorageService(ITableStorageContext context) : base(context) { }

        public T Get<T>(string partitionKey) where T : TableStorageEntity, new()
        {
            return Context.Get<T>(partitionKey);
        }

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

        public async Task ReplaceAsync<T>(T entity) where T : TableStorageEntity
        {
            var table = Context.GetTable(typeof(T));
            table.Replace(entity);
            await Context.SaveChangesAsync();
        }

        public async Task ReplaceAsync<T>(IEnumerable<T> entities) where T : TableStorageEntity
        {
            var table = Context.GetTable(typeof(T));
            entities.ForEach(table.Replace);
            await Context.SaveChangesAsync();
        }

        public async Task MergeAsync<T>(T entity) where T : TableStorageEntity
        {
            var table = Context.GetTable(typeof(T));
            table.Merge(entity);
            await Context.SaveChangesAsync();
        }

        public async Task MergeAsync<T>(IEnumerable<T> entities) where T : TableStorageEntity
        {
            var table = Context.GetTable(typeof(T));
            entities.ForEach(table.Merge);
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

        public async Task InsertOrMergeAsync<T>(T entity) where T : TableStorageEntity
        {
            var table = Context.GetTable(typeof(T));
            table.InsertOrMerge(entity);
            await Context.SaveChangesAsync();
        }

        public async Task InsertOrMergeAsync<T>(IEnumerable<T> entities) where T : TableStorageEntity
        {
            var table = Context.GetTable(typeof(T));
            entities.ForEach(table.InsertOrMerge);
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

        public async Task TruncateAsync<T>() where T : TableStorageEntity, ITableEntity, new()
        {
            var entities = await FindAllInternalAsync<T>(x=> true);

            entities.ForEach(e => Context.Delete( e));

            await Context.SaveChangesAsync();
        }
    }
}