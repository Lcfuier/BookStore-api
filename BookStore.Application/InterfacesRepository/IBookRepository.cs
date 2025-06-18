using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Application.InterfacesRepository
{
    public interface IBookRepository : IRepository<Book>
    {
        Task AddNewCategoryAsync(Book book, Guid[] categoriesId, ICategoryRepository categoryData);
        Task<IEnumerable<BestSellerBookRes>> GetBestSellerBook();
        void Update(Book book);
    }
}
