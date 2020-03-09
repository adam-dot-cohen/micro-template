﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Laso.Identity.Domain.Entities;

namespace Laso.Identity.Core.Persistence
{
    public interface IReadOnlyTableStorageService
    {
        Task<T> GetAsync<T>(string partitionKey, string rowKey = null) where T : TableStorageEntity, new();
        Task<TResult> GetAsync<T, TResult>(string partitionKey, Expression<Func<T, TResult>> select, string rowKey = null) where T : TableStorageEntity, new();
        Task<ICollection<T>> GetAllAsync<T>(string partitionKey = null, int? limit = null) where T : TableStorageEntity, new();
        Task<ICollection<TResult>> GetAllAsync<T, TResult>(Expression<Func<T, TResult>> select, string partitionKey = null, int? limit = null) where T : TableStorageEntity, new();
        Task<ICollection<T>> FindAllAsync<T>(Expression<Func<T, bool>> filter, int? limit = null) where T : TableStorageEntity, new();
        Task<ICollection<TResult>> FindAllAsync<T, TResult>(Expression<Func<T, bool>> filter, Expression<Func<T, TResult>> select, int? limit = null) where T : TableStorageEntity, new();
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