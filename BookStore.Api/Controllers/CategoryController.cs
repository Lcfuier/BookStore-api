using BookStore.Application.Interface;
using BookStore.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStore.Api.Controllers
{
    [Route("api/[controller]")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService= categoryService;  
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCategory(int page, int size, string? term)
        {
            var result = await _categoryService.GetAllCategoriesAsync(page, size, term);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetCategoryById(Guid id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] AddCategoryReq param)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _categoryService.AddCategoryAsync(param, userName);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAuthor([FromBody] UpdateCategoryReq param)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _categoryService.UpdateCategoryAsync(param, userName);
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
            var result = await _categoryService.RemoveCategoryAsync(id,userName);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
