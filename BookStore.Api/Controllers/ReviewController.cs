using BookStore.Application.Interface;
using BookStore.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStore.Api.Controllers
{
    [Route("api/books")]
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;
        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }
        [HttpGet("{id:guid}/reviews")]
        public async Task<IActionResult> GetAllReviewByBookId(int page, int size, Guid id)
        {
            var result = await _reviewService.GetReviewsByBookIdAsync(page, size, id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize]
        [HttpPost("{id:guid}/reviews")]
        public async Task<IActionResult> AddReview([FromBody] AddReviewReq param)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _reviewService.AddReviewAsync(param, userName);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize]
        [HttpPut("{id:guid}/reviews/{reviewId:guid}")]
        public async Task<IActionResult> UpdateReview([FromBody] UpdateReviewReq param,Guid reviewId,Guid bookId)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            string role = claimsIdentity?.FindFirst(ClaimTypes.Role)?.Value ?? "";
            var result = await _reviewService.UpdateReviewAsync(param, userName,role);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize]
        [HttpDelete("{id:guid}/reviews/{reviewId:guid}")]
        public async Task<IActionResult> DeleteReview(Guid reviewId)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            string role = claimsIdentity?.FindFirst(ClaimTypes.Role)?.Value ?? "";
            var result = await _reviewService.RemoveReviewAsync(reviewId,userName,role);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
