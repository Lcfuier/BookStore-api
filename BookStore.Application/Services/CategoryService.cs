using BookStore.Application.Interface;
using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Domain.Queries;
using BookStore.Domain.Result;
using BookStore.Infrastructure.Interface;
using BookStore.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _data;
        private readonly UserManager<ApplicationUser> _userManager;
        public CategoryService(IUnitOfWork data, UserManager<ApplicationUser> userManager)
        {
            _data = data;
            _userManager = userManager;
        }
        public async Task<Result<PaginationResponse<Category>>> GetAllCategoriesAsync(int page, int size, string? term)
        {
            IEnumerable<Category> data;
            QueryOptions<Category> options = new QueryOptions<Category>
            {
            };
            if (term != null)
            {
                options.Where = mi => mi.Name.ToLower().Contains(term.ToLower());
            }
            if (page < 1)
            {
                data = await _data.Category.ListAllAsync(options);
            }
            else
            {
                options.PageNumber = page;
                options.PageSize = size;
                data = await _data.Category.ListAllAsync(options);
            }

            PaginationResponse<Category> paginationResponse = new PaginationResponse<Category>
            {
                PageNumber = page,
                PageSize = size,
                // must be above the TotalRecords bc it has multiple Where clauses
                Items = data,
                TotalRecords = await _data.Category.CountAsync()
            };
            return new Result<PaginationResponse<Category>>
            {
                Data = paginationResponse,
                Message = "Successful",
                Success = true
            };
        }
        public async Task<Result<Category>> GetCategoryByIdAsync(Guid? id)
        {
            QueryOptions<Category> options = new()
            {
                Where = a => a.CategoryId == id,
            };
            var data = await _data.Category.GetAsync(options);
            if (data != null)
            {
                return new Result<Category>
                {
                    Success = true,
                    Data = data,
                    Message = "Successful"
                };
            }
            else
            {
                return new Result<Category>
                {
                    Success = false,
                    Data = data,
                    Message = "Not found Author"
                };
            }

        }
        public async Task<Result<IEnumerable<Category>>> GetCategoryByTermAsync(string term)
        {
            QueryOptions<Category> options = new QueryOptions<Category>
            {
            };
            if (term != null)
            {
                options.Where = mi => mi.Name.Contains(term);
            }
            var data = await _data.Category.ListAllAsync(options);

            return new Result<IEnumerable<Category>>
            {
                Data = data,
                Success = true,
                Message = "Successful"
            };
        }
        public async Task<Result<Category>> AddCategoryAsync(AddCategoryReq CategoryDto, string userName)
        {
            Result<Category> result = new Result<Category>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                result.Success = false;
                result.Message = "User not exist";
                result.Data = null;
                return result;
            }
            var category = new Category()
            {
                Name = CategoryDto.Name,
                CreatedTime = DateTime.UtcNow,
                CreatedBy = userName,
            };
            _data.Category.Add(category);
            await _data.SaveAsync();
            result.Success = true;
            result.Message = "Successfull";
            result.Data = category;
            return result;
        }

        public async Task<Result<Category>> UpdateCategoryAsync(UpdateCategoryReq categoryDto, string userName)
        {
            Result<Category> result = new Result<Category>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                result.Success = false;
                result.Message = "User not exist";
                result.Data = null;
                return result;
            }
            var category = await _data.Category.GetAsync(new QueryOptions<Category>
            {
                Where = c => c.CategoryId.Equals(categoryDto.CategoryId)
            });
            if (category == null)
            {
                result.Success = false;
                result.Message = "Category not exist";
                result.Data = null;
                return result;
            }
            category.Name = category.Name;
            category.ModifiedTime = DateTime.UtcNow;
            category.ModifiedBy = userName;
            _data.Category.Update(category);
            await _data.SaveAsync();
            result.Success = true;
            result.Message = "Successfull";
            result.Data = category;
            return result;
        }

        public async Task<Result<Category>> RemoveCategoryAsync(Guid Id)
        {
            Result<Category> result = new Result<Category>();
            var category = await _data.Category.GetAsync(new QueryOptions<Category>
            {
                Where = c => c.CategoryId.Equals(Id)
            });
            if (category == null)
            {
                result.Success = false;
                result.Message = "Category not exist";
                result.Data = null;
                return result;
            }
            _data.Category.Remove(category);
            await _data.SaveAsync();
            result.Success = true;
            result.Message = "Successfull";
            return result;
        }
    }
}
