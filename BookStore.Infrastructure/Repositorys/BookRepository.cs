using AutoMapper;
using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Infrastructure.Data;
using BookStore.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
namespace BookStore.Infrastructure.Repositorys
{
    public class BookRepository : Repository<Book>, IBookRepository
    {
        private readonly IMapper _mapper;
        public BookRepository(ApplicationDbContext dbContext, IMapper mapper) : base(dbContext)
        {
            _mapper = mapper;
        }
        public async Task AddNewCategoryAsync(Book book, Guid[] categoriesId, ICategoryRepository categoryData)
        {
            book.Categories.Clear();
            foreach (var Id in categoriesId)
            {
                Category category = await categoryData.GetAsync(Id);
                if (category is not null)
                {
                    book.Categories.Add(category);
                }
            }
        }
        public async Task<GetBookRes> GetBookByIdAsync(Guid Id)
        {
            var result = await _dbContext.Book.Where(c => c.BookId.Equals(Id))
                                    .ProjectTo<GetBookRes>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
            return result;
        }
        public void Update(Book book)
        {
            _dbContext.Update(book);
        }
    }
}
