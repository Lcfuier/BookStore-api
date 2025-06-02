using BookStore.Application.Services;
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
     public interface IReviewService
    {
        Task<Result<PaginationResponse<ReviewRes>>> GetReviewsByBookIdAsync(int page, int size, Guid bookId);
        Task<Result<PaginationResponse<Review>>> GetReviewsByUserIdAsync(int page, int size, string UserId);
        Task<Result<Review>> AddReviewAsync(AddReviewReq req, string userName);
        Task<Result<Review>> UpdateReviewAsync(UpdateReviewReq req, string userName, string userRole);
        Task<Result<Review>> RemoveReviewAsync(Guid ReviewId, string userName, string userRole);
    }
}
