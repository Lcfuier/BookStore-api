using BookStore.Application.Interface;
using BookStore.Application.Services;
using BookStore.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStore.Api.Controllers
{
    [Route("api/[controller]s")]
    public class BookController : Controller
    {
        private readonly IBookService _bookService;
        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllBook([FromQuery] GetAllBookReq req)
        {
            var result = await _bookService.GetAllBooksAsync(req);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpGet("best-sellers")]
        public async Task<IActionResult> GetBestSellerBook()
        {
            var result = await _bookService.GetBestSellerBookAsync();
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetBookById(Guid id)
        {
            var result = await _bookService.GetBookByIdAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddBook([FromForm] AddBookReq param, IFormFile Image)
        {
            if (Image == null || Image.Length == 0)
                return BadRequest();
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _bookService.AddBookAsync(param,Image, userName);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateBook([FromForm] UpdateBookReq param, IFormFile? newImage,Guid id)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _bookService.UpdateBookAsync(param, newImage,userName,id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAuthor(Guid id)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _bookService.RemoveBookAsync(id, userName);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
