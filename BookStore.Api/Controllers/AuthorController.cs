using BookStore.Application.Interface;
using BookStore.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStore.Api.Controllers
{
    [Route("api/[controller]")]
    public class AuthorController : Controller
    {
        private readonly IAuthorService _authorService;
        public AuthorController(IAuthorService authorService)
        {
            _authorService = authorService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAuthor(int page, int size,string? term)
        {
            var result = await _authorService.GetAllAuthorsAsync(page,size,term);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetAuthorById(Guid id)
        {
            var result = await _authorService.GetAuthorByIdAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddAuthor([FromBody] AddAuthorReq param)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _authorService.AddAuthorAsync(param,userName);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAuthor([FromBody] UpdateAuthorReq param)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _authorService.UpdateAuthorAsync(param, userName);
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
            var result = await _authorService.RemoveAuthorAsync(id,userName);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
