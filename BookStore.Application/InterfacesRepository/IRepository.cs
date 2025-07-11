﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BookStore.Domain.Queries;
namespace BookStore.Application.InterfacesRepository
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null, bool tracked = false);
        Task<T> GetByIdAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null, bool tracked = false);
        Task<int> CountAsync();
        Task<IEnumerable<T>> ListAllAsync(QueryOptions<T> options);

        Task<T?> GetAsync(Guid id);
        Task<T?> GetAsync(string id);
        Task<T?> GetAsync(QueryOptions<T> options);

        void Add(T entity);
        void AddRange(List<T> values);

        void Remove(T entity);
        void RemoveRange(T entity);
        void RemoveRange(List<T> entities);
    }
}
