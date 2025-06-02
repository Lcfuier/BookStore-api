using BookStore.Application.Interface;
using BookStore.Domain.Constants;
using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Domain.Queries;
using BookStore.Domain.Result;
using BookStore.Infrastructure.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BookStore.Application.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly IUnitOfWork _data;
        private readonly UserManager<ApplicationUser> _userManager;
        public AuthorService(IUnitOfWork data,UserManager<ApplicationUser> userManager)
        {
            _data = data;
            _userManager = userManager;
        }
        public async Task<Result<PaginationResponse<Author>>> GetAllAuthorsAsync(int page, int size,string? term)
        {
            IEnumerable<Author> data;
            QueryOptions<Author> options = new QueryOptions<Author>
            {
            };
            if (term != null)
            {
                options.Where = mi => mi.AuthorFullName.ToLower().Contains(term.ToLower());
            }
            if (page < 1 )
            {
                data= await _data.Author.ListAllAsync(options);
            }
            else
            {
                options.PageNumber=page;
                options.PageSize=size;
                data=await _data.Author.ListAllAsync(options);
            }
            
            PaginationResponse<Author> paginationResponse = new PaginationResponse<Author>
            {
                PageNumber = page,
                PageSize = size,
                // must be above the TotalRecords bc it has multiple Where clauses
                Items = data,
                TotalRecords = await _data.Author.CountAsync()
            };
            return new Result<PaginationResponse<Author>>
            {
                Data = paginationResponse,
                Success=true
            };
        }
        public async Task<Result<Author>> GetAuthorByIdAsync(Guid? id)
        {
            QueryOptions<Author> options = new()
            {
                Where = a => a.Id == id,
            };
            var data = await _data.Author.GetAsync(options);
            if(data != null)
            {
                return new Result<Author>
                {
                    Success = true,
                    Data = data,
                };
            }
            else
            {
                return new Result<Author>
                {
                    Success = false,
                    Data = data,
                    Message = "Không tìm thấy tác giả!"
                };
            }
            
        }
        public async Task<Result<IEnumerable<Author>>> GetAuthorByTermAsync(string term)
        {
            QueryOptions<Author> options = new QueryOptions<Author>
            {
            };
            if (term != null)
            {
                options.Where = mi => mi.AuthorFullName.Contains(term);
            }
            var data = await _data.Author.ListAllAsync(options);
            
            return new Result<IEnumerable<Author>>
            {
                Data=data,
                Success = true,
            };
        }
        public async Task<Result<Author>> AddAuthorAsync(AddAuthorReq authorDto,string userName)
        {
            Result<Author> result = new Result<Author>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return new Result<Author>
                {
                    Success = false,
                    Data = null,
                    Message = "Người dùng không tồn tại !"
                };
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.FirstOrDefault()?.ToLower() != Roles.Admin.ToLower() || roles.FirstOrDefault()?.ToLower() != Roles.Librarian.ToLower())
            {
                return new Result<Author>
                {
                    Success = false,
                    Data = null,
                    Message = "Không thể truy cập!"
                };
            }
            var author = new Author()
            {
                AuthorFullName = authorDto.AuthorFullName,
                CreatedTime = DateTime.UtcNow,
                CreatedBy=userName,
            };
            _data.Author.Add(author);
            await _data.SaveAsync();
            result.Success = true;
            result.Message = "Thêm tác giả thành công!";
            result.Data = author;
            return result;
        }

        public async Task<Result<Author>> UpdateAuthorAsync(UpdateAuthorReq authorDto, string userName)
        {
            Result<Author> result = new Result<Author>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return new Result<Author>
                {
                    Success = false,
                    Data = null,
                    Message = "Người dùng không tồn tại !"
                };
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.FirstOrDefault()?.ToLower() != Roles.Admin.ToLower() || roles.FirstOrDefault()?.ToLower() != Roles.Librarian.ToLower())
            {
                return new Result<Author>
                {
                    Success = false,
                    Data = null,
                    Message = "Không thể truy cập!"
                };
            }
            var author = await _data.Author.GetAsync(new QueryOptions<Author>
            {
                Where=c=>c.Id.Equals(authorDto.AuthorId)
            });
            if (author == null)
            {
                result.Success = false;
                result.Message = "Tác giả không tồn tại!";
                result.Data = null;
                return result;
            }
            author.AuthorFullName = authorDto.AuthorFullName;
            author.ModifiedTime= DateTime.UtcNow;
            author.ModifiedBy = userName;
            _data.Author.Update(author);
            await _data.SaveAsync();
            result.Success = true;
            result.Message = "Cập nhật tác giả thành công !";
            result.Data = author;
            return result;
        }

        public async Task<Result<Author>> RemoveAuthorAsync(Guid authorId)
        {
            Result<Author> result = new Result<Author>();
            var author = await _data.Author.GetAsync(new QueryOptions<Author>
            {
                Where = c => c.Id.Equals(authorId)
            });
            if (author == null)
            {
                result.Success = false;
                result.Message = "Tác giả không tồn tại !";
                result.Data = null;
                return result;
            }
            _data.Author.Remove(author);
            await _data.SaveAsync();
            result.Success= true;
            result.Message = "Xóa tác giả thành công!";
            return result;
        }
    }
}

