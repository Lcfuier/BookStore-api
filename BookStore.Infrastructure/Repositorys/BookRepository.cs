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
        public async Task<IEnumerable<BestSellerBookRes>> GetBestSellerBook()
        {
            var bestSellerBooks = await _dbContext.OrderItem
                    .GroupBy(od => od.BookId)
                    .Select(g => new {
                     BookId = g.Key,
                    TotalSold = g.Sum(x => x.Quantity)
                    })
                    .OrderByDescending(x => x.TotalSold)
                    .Take(10)
                    .Join(_dbContext.Book,
                    g => g.BookId,
                    b => b.BookId,
                    (g, b) => new BestSellerBookRes {
                        
                    Id=b.BookId,
                    Title=b.Title,
                    AuthorName=b.Author.AuthorFullName,
                    Price=b.Price,
                    ImageURL=b.ImageURL,
                    DiscountPercent=b.DiscountPercent,
                    TotalSold = g.TotalSold
                    })
                .ToListAsync();
            return bestSellerBooks;
        }
        public void Update(Book book)
        {
            _dbContext.Update(book);
        }
    }
}
