using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Domain.Queries;
using BookStore.Domain.Result;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Application.Interface
{
    public interface IBookService
    {
        Task<Result<PaginationResponse<Book>>> GetAllBooksAsync(GetAllBookReq req);
        Task<Result<Book>> GetBookByIdAsync(Guid id);
        Task<Result<Book>> RemoveBookAsync(Guid Id);
        Task<Result<Book>> AddBookAsync(AddBookReq bookDto, IFormFile image, string userName);
        Task<Result<Book>> UpdateBookAsync(UpdateBookReq bookDto, IFormFile? newImage, string userName, Guid Id);
    }
}
