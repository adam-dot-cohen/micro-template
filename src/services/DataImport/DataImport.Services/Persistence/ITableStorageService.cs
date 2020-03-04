using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Laso.DataImport.Domain.Entities;

namespace Laso.DataImport.Services.Persistence
{
    public interface IReadOnlyTableStorageService
    {
        Task<T> GetAsync<T>(string partitionKey, string rowKey = null) where T : TableStorageEntity, new();
        Task<ICollection<T>> GetAllAsync<T>(string partitionKey = null, int? limit = null) where T : TableStorageEntity, new();
        Task<ICollection<T>> FindAllAsync<T>(Expression<Func<T, bool>> filter, int? limit = null) where T : TableStorageEntity, new();
        Task<ICollection<T>> FindAllAsync<T>(string filter = null, int? limit = null) where T : TableStorageEntity, new();
    }

    public interface ITableStorageService : IReadOnlyTableStorageService
    {
        Task InsertAsync<T>(T entity) where T : TableStorageEntity;
        Task InsertAsync<T>(IEnumerable<T> entities) where T : TableStorageEntity;
        Task InsertOrReplaceAsync<T>(T entity) where T : TableStorageEntity;
        Task InsertOrReplaceAsync<T>(IEnumerable<T> entities) where T : TableStorageEntity;

        Task DeleteAsync<T>(string partitionKey, string rowKey = null) where T : TableStorageEntity, new();
        Task DeleteAsync<T>(T entity) where T : TableStorageEntity;
        Task DeleteAsync<T>(IEnumerable<T> entities) where T : TableStorageEntity;
    }
}
