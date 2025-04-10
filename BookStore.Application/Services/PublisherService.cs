using BookStore.Application.Interface;
using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Domain.Queries;
using BookStore.Domain.Result;
using BookStore.Infrastructure.Interface;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Application.Services
{
    public class PublisherService : IPublisherService
    {
        private readonly IUnitOfWork _data;
        private readonly UserManager<ApplicationUser> _userManager;
        public PublisherService(IUnitOfWork data, UserManager<ApplicationUser> userManager)
        {
            _data = data;
            _userManager = userManager;
        }
        public async Task<Result<PaginationResponse<Publisher>>> GetAllPublishersAsync(int page, int size, string? term)
        {
            IEnumerable<Publisher> data;
            QueryOptions<Publisher> options = new QueryOptions<Publisher>
            {
            };
            if (term != null)
            {
                options.Where = mi => mi.Name.ToLower().Contains(term.ToLower());
            }
            if (page < 1)
            {
                data = await _data.Publisher.ListAllAsync(options);
            }
            else
            {
                options.PageNumber = page;
                options.PageSize = size;
                data = await _data.Publisher.ListAllAsync(options);
            }

            PaginationResponse<Publisher> paginationResponse = new PaginationResponse<Publisher>
            {
                PageNumber = page,
                PageSize = size,
                // must be above the TotalRecords bc it has multiple Where clauses
                Items = data,
                TotalRecords = await _data.Publisher.CountAsync()
            };
            return new Result<PaginationResponse<Publisher>>
            {
                Data = paginationResponse,
                Message = "Successful",
                Success = true
            };
        }
        public async Task<Result<Publisher>> GetPublisherByIdAsync(Guid? id)
        {
            QueryOptions<Publisher> options = new()
            {
                Where = a => a.PublisherId == id,
            };
            var data = await _data.Publisher.GetAsync(options);
            if (data != null)
            {
                return new Result<Publisher>
                {
                    Success = true,
                    Data = data,
                    Message = "Successful"
                };
            }
            else
            {
                return new Result<Publisher>
                {
                    Success = false,
                    Data = data,
                    Message = "Not found Publisher"
                };
            }

        }
        public async Task<Result<IEnumerable<Publisher>>> GetPublisherByTermAsync(string term)
        {
            QueryOptions<Publisher> options = new QueryOptions<Publisher>
            {
            };
            if (term != null)
            {
                options.Where = mi => mi.Name.Contains(term);
            }
            var data = await _data.Publisher.ListAllAsync(options);

            return new Result<IEnumerable<Publisher>>
            {
                Data = data,
                Success = true,
                Message = "Successful"
            };
        }
        public async Task<Result<Publisher>> AddPublisherAsync(AddPublisherReq publisherDto, string userName)
        {
            Result<Publisher> result = new Result<Publisher>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                result.Success = false;
                result.Message = "User not exist";
                result.Data = null;
                return result;
            }
            var publisher = new Publisher()
            {
                Name = publisherDto.Name,
                CreatedTime = DateTime.UtcNow,
                CreatedBy = userName,
            };
            _data.Publisher .Add(publisher);
            await _data.SaveAsync();
            result.Success = true;
            result.Message = "Successfull";
            result.Data = publisher;
            return result;
        }

        public async Task<Result<Publisher>> UpdatePublisherAsync(UpdatePublisherReq publisherDto, string userName)
        {
            Result<Publisher> result = new Result<Publisher>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                result.Success = false;
                result.Message = "User not exist";
                result.Data = null;
                return result;
            }
            var publisher = await _data.Publisher  .GetAsync(new QueryOptions<Publisher>
            {
                Where = c => c.PublisherId.Equals(publisherDto.PublisherId)
            });
            if (publisher == null)
            {
                result.Success = false;
                result.Message = "Publisher not exist";
                result.Data = null;
                return result;
            }
            publisher.Name = publisherDto.Name;
            publisher.ModifiedTime = DateTime.UtcNow;
            publisher.ModifiedBy = userName;
            _data.Publisher.Update(publisher);
            await _data.SaveAsync();
            result.Success = true;
            result.Message = "Successfull";
            result.Data = publisher;
            return result;
        }

        public async Task<Result<Publisher>> RemovePublisherAsync(Guid Id)
        {
            Result<Publisher> result = new Result<Publisher>();
            var publisher = await _data.Publisher.GetAsync(new QueryOptions<Publisher>
            {
                Where = c => c.PublisherId.Equals(Id)
            });
            if (publisher == null)
            {
                result.Success = false;
                result.Message = "Publisher not exist";
                result.Data = null;
                return result;
            }
            _data.Publisher.Remove(publisher);
            await _data.SaveAsync();
            result.Success = true;
            result.Message = "Successfull";
            return result;
        }
    }
}
