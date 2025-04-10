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
    public interface IPublisherService
    {
        Task<Result<PaginationResponse<Publisher>>> GetAllPublishersAsync(int page, int size, string? term);
        Task<Result<Publisher>> GetPublisherByIdAsync(Guid? id);
        Task<Result<IEnumerable<Publisher>>> GetPublisherByTermAsync(string term);
        Task<Result<Publisher>> AddPublisherAsync(AddPublisherReq publisherDto, string userName);
        Task<Result<Publisher>> UpdatePublisherAsync(UpdatePublisherReq publisherDto, string userName);
        Task<Result<Publisher>> RemovePublisherAsync(Guid Id);

    }
}
