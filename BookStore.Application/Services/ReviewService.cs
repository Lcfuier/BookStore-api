using AutoMapper;
using BookStore.Application.Interface;
using BookStore.Domain.Constants;
using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Domain.Queries;
using BookStore.Domain.Result;
using BookStore.Infrastructure.Interface;
using BookStore.Infrastructure.Migrations;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _data;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        public ReviewService(IUnitOfWork data, UserManager<ApplicationUser> userManager,IMapper mapper)
        {
            _data = data;
            _userManager = userManager;
            _mapper = mapper;
        }
        public async Task<Result<PaginationResponse<ReviewRes>>> GetReviewsByBookIdAsync(int page, int size, Guid bookId)
        {
            IEnumerable<Review> data;
            QueryOptions<Review> options = new QueryOptions<Review>
            {
                Where = c => c.BookId.Equals(bookId),
                Includes="User"
            };
            if (page < 1)
            {
                data = await _data.Review.ListAllAsync(options);
            }
            else
            {
                options.PageNumber = page;
                options.PageSize = size;
                options.OrderBy = c => c.CreatedTime;
                options.OrderByDirection = "desc";
                data = await _data.Review.ListAllAsync(options);
            }
            
            PaginationResponse<ReviewRes> paginationResponse = new PaginationResponse<ReviewRes>
            {
                PageNumber = page,
                PageSize = size,
                // must be above the TotalRecords bc it has multiple Where clauses
                Items = _mapper.Map<List<ReviewRes>>(data),
                TotalRecords = await _data.Review.CountAsync()
            };
            return new Result<PaginationResponse<ReviewRes>>
            {
                Data = paginationResponse,
                Success = true
            };
        }
        public async Task<Result<PaginationResponse<Review>>> GetReviewsByUserIdAsync(int page, int size, string UserId)
        {
            IEnumerable<Review> data;
            QueryOptions<Review> options = new QueryOptions<Review>
            {
                Where = c => c.UserId.Equals(UserId)
            };
            if (page < 1)
            {
                data = await _data.Review.ListAllAsync(options);
            }
            else
            {
                options.PageNumber = page;
                options.PageSize = size;
                data = await _data.Review.ListAllAsync(options);
            }

            PaginationResponse<Review> paginationResponse = new PaginationResponse<Review>
            {
                PageNumber = page,
                PageSize = size,
                // must be above the TotalRecords bc it has multiple Where clauses
                Items = data,
                TotalRecords = await _data.Review.CountAsync()
            };
            return new Result<PaginationResponse<Review>>
            {
                Data = paginationResponse,
                Success = true
            };
        }
        public async Task<Result<Review>> AddReviewAsync(AddReviewReq req, string userName)
        {
            Result<Review> result = new Result<Review>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                result.Success = false;
                result.Message = "Người dùng không tồn tại!";
                result.Data = null;
                return result;
            }
            var review = new Review()
            {
                BookId=req.BookId,
                Comment=req.Comment,
                Rating=req.Rating,
                User=user,
                UserId=user.Id,
                CreatedTime = DateTime.UtcNow,
                CreatedBy = userName,
            };
            _data.Review.Add(review);
            await _data.SaveAsync();
            result.Success = true;
            result.Message = "Thêm đánh giá thành công!";
            result.Data = review;
            return result;
        }
        public async Task<Result<Review>> UpdateReviewAsync(UpdateReviewReq req, string userName,string userRole)
        {
            Result<Review> result = new Result<Review>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                result.Success = false;
                result.Message = "Người dùng không tồn tại!";
                result.Data = null;
                return result;
            }
            var review = await _data.Review.GetAsync(new QueryOptions<Review>
            {
                Where = c => c.Id.Equals(req.ReviewId)
            });
            if (review == null)
            {
                result.Success = false;
                result.Message = "Không tìm thấy đánh giá";
                result.Data = null;
                return result;
            }
            if(!userRole.Equals(Roles.Admin)||!userRole.Equals(Roles.Librarian)|| !review.UserId.Equals(user.Id))
            {
                result.Success = false;
                result.Message = "Lỗi khi thực hiện!";
                result.Data = null;
                return result;
            }
            review.Comment = req.Comment;
            review.Rating = req.Rating;
            review.ModifiedTime = DateTime.UtcNow;
            review.ModifiedBy = userName;
            _data.Review.Update(review);
            await _data.SaveAsync();
            result.Success = true;
            result.Message = "Cập nhật đánh giá thành công!";
            result.Data = review;
            return result;
        }
        public async Task<Result<Review>> RemoveReviewAsync(Guid ReviewId, string userName, string userRole)
        {
            Result<Review> result = new Result<Review>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                result.Success = false;
                result.Message = "Người dùng không tồn tại!";
                result.Data = null;
                return result;
            }
            var review = await _data.Review.GetAsync(new QueryOptions<Review>
            {
                Where = c => c.Id.Equals(ReviewId)
            });
            if (review == null)
            {
                result.Success = false;
                result.Message = "Đánh giá không tồn tại";
                result.Data = null;
                return result;
            }
            if (userRole.ToLower()==Roles.User.ToLower())
            {
                if (review.UserId != user.Id)
                {
                    result.Success = false;
                    result.Message = "Lỗi khi thực hiện";
                    result.Data = null;
                    return result;
                }
            }
            _data.Review.Remove(review);
            await _data.SaveAsync();
            result.Success = true;
            result.Message = "Xóa đánh giá thành công!";
            return result;
        }
    }
}
