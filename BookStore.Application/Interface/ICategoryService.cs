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
    public interface ICategoryService
    {
        Task<Result<PaginationResponse<Category>>> GetAllCategoriesAsync(int page, int size, string? term);
        Task<Result<Category>> GetCategoryByIdAsync(Guid? id);
        Task<Result<IEnumerable<Category>>> GetCategoryByTermAsync(string term);
        Task<Result<Category>> AddCategoryAsync(AddCategoryReq CategoryDto, string userName);
        Task<Result<Category>> UpdateCategoryAsync(UpdateCategoryReq categoryDto, string userName);
        Task<Result<Category>> RemoveCategoryAsync(Guid Id);

    }
}
