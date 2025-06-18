using BookStore.Api.Controllers;
using BookStore.Application.Interface;
using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Domain.Queries;
using BookStore.Domain.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.UnitTest.Controllers
{
    public class CategoryControllerTests
    {
        private readonly ICategoryService _categoryService;
        private readonly CategoryController _controller;

        public CategoryControllerTests()
        {
            _categoryService = A.Fake<ICategoryService>();
            _controller = new CategoryController(_categoryService);
        }
        [Fact]
        public async Task GetAllCategory_ReturnsOk_WhenSuccess()
        {
            // Arrange
            var result = new Result<PaginationResponse<Category>>
            {
                Success = true,
                Data = new PaginationResponse<Category>
                {
                    Items = new List<Category>(),
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 0
                }
            };
            A.CallTo(() => _categoryService.GetAllCategoriesAsync(1, 10, null)).Returns(result);

            // Act
            var response = await _controller.GetAllCategory(1, 10, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            okResult.Value.Should().Be(result);
        }
        [Fact]
        public async Task GetCategoryById_ReturnsBadRequest_WhenFailed()
        {
            var id = Guid.NewGuid();
            var result = new Result<Category> { Success = false, Message = "Not found" };
            A.CallTo(() => _categoryService.GetCategoryByIdAsync(id)).Returns(result);

            var response = await _controller.GetCategoryById(id);

            Assert.IsType<BadRequestObjectResult>(response);
        }
        [Fact]
        public async Task AddCategory_ReturnsOk_WhenSuccess()
        {
            var req = new AddCategoryReq { Name = "Test" };
            var result = new Result<Category> { Success = true, Data = new Category() };
            var userName = "admin";

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.Name, userName)
    }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            A.CallTo(() => _categoryService.AddCategoryAsync(req, userName)).Returns(result);

            var response = await _controller.AddCategory(req);

            var ok = Assert.IsType<OkObjectResult>(response);
            ok.Value.Should().Be(result);
        }
        [Fact]
        public async Task DeleteCategory_ReturnsOk_WhenSuccess()
        {
            var id = Guid.NewGuid();
            var result = new Result<Category> { Success = true };
            var userName = "admin";

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.Name, userName)
    }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            A.CallTo(() => _categoryService.RemoveCategoryAsync(id, userName)).Returns(result);

            var response = await _controller.DeleteCategory(id);

            var ok = Assert.IsType<OkObjectResult>(response);
            ok.Value.Should().Be(result);
        }

    }

}
