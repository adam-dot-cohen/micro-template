using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Laso.TableStorage.Domain;

namespace Laso.TableStorage
{
    public interface IReadOnlyTableStorageService
    {
        Task<T> GetAsync<T>(string partitionKey) where T : TableStorageEntity, ITableEntity, new();
        Task<ICollection<T>> GetAllAsync<T>(int? limit = null) where T : TableStorageEntity, ITableEntity, new();
        Task<ICollection<T>> FindAllAsync<T>(Expression<Func<T, bool>> filter, int? limit = null) where T : TableStorageEntity, ITableEntity, new();
        Task<ICollection<T>> FindAllAsync<T>(Expression<Func<T, bool>> filter) where T : TableStorageEntity, ITableEntity, new();
        Task<T> GetAsync<T>(string partitionKey, Expression<Func<T, bool>> select) where T : TableStorageEntity, ITableEntity, new();
        Task<ICollection<T>> GetAllAsync<T>(Expression<Func<T, bool>> select, int? limit = null) where T : TableStorageEntity, ITableEntity, new();
        Task<ICollection<T>> FindAllAsync<T>(Expression<Func<T, bool>> filter, Expression<Func<T, bool>> select, int? limit = null) where T : TableStorageEntity, ITableEntity, new();
        //Task<TResult> GetAsync<T, TResult>(string partitionKey, Expression<Func<T, TResult>> select, string rowKey = null) where T : TableStorageEntity, ITableEntity, new();
        //Task<ICollection<TResult>> GetAllAsync<T, TResult>(Expression<Func<T, TResult>> select, string partitionKey = null, int? limit = null) where T : TableStorageEntity, ITableEntity, new();
        //Task<ICollection<TResult>> FindAllAsync<T, TResult>(Expression<Func<T, bool>> filter, Expression<Func<T, TResult>> select, int? limit = null) where T : TableStorageEntity, ITableEntity, new();
    }

    public interface ITableStorageService : IReadOnlyTableStorageService
    {
        Task InsertAsync<T>(T entity) where T : TableStorageEntity;
        Task InsertAsync<T>(IEnumerable<T> entities) where T : TableStorageEntity;
        Task ReplaceAsync<T>(T entity) where T : TableStorageEntity;
        Task ReplaceAsync<T>(IEnumerable<T> entities) where T : TableStorageEntity;
        Task MergeAsync<T>(T entity) where T : TableStorageEntity;
        Task MergeAsync<T>(IEnumerable<T> entities) where T : TableStorageEntity;
        Task InsertOrReplaceAsync<T>(T entity) where T : TableStorageEntity;
        Task InsertOrReplaceAsync<T>(IEnumerable<T> entities) where T : TableStorageEntity;
        Task InsertOrMergeAsync<T>(T entity) where T : TableStorageEntity;
        Task InsertOrMergeAsync<T>(IEnumerable<T> entities) where T : TableStorageEntity;
        Task DeleteAsync<T>(T entity) where T : TableStorageEntity;
        Task DeleteAsync<T>(IEnumerable<T> entities) where T : TableStorageEntity;
    }
}
