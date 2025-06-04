using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Domain.Queries;
using BookStore.Domain.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Application.Interface
{
    public interface IAuthorService
    {
        Task<Result<PaginationResponse<Author>>> GetAllAuthorsAsync(int page, int size, string? term);
        Task<Result<Author>> GetAuthorByIdAsync(Guid? id);
        Task<Result<IEnumerable<Author>>> GetAuthorByTermAsync(string term);
        Task<Result<Author>> AddAuthorAsync(AddAuthorReq authorDto, string userName);
        Task<Result<Author>> UpdateAuthorAsync(UpdateAuthorReq authorDto, string userName);
        Task<Result<Author>> RemoveAuthorAsync(Guid authorId, string userName);

    }
}
