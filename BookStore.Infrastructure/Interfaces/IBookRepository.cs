using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Infrastructure.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Infrastructure.Interfaces
{
    public interface IBookRepository : IRepository<Book>
    {
        Task AddNewCategoryAsync(Book book, Guid[] categoriesId, ICategoryRepository categoryData);
        Task<GetBookRes> GetBookByIdAsync(Guid Id);
        void Update(Book book);
    }
}
