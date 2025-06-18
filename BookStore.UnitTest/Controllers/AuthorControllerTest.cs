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
    public class AuthorControllerTests
    {
        private readonly IAuthorService _authorService;
        private readonly AuthorController _controller;

        public AuthorControllerTests()
        {
            _authorService = A.Fake<IAuthorService>();
            _controller = new AuthorController(_authorService);
        }
        [Fact]
        public async Task GetAllAuthor_ReturnsOk_WhenSuccess()
        {
            // Arrange
            var page = 1;
            var size = 10;
            var term = "";
            var data = new Result<PaginationResponse<Author>>
            {
                Success = true,
                Data = new PaginationResponse<Author>
                {
                    Items = new List<Author>(),
                    PageNumber = page,
                    PageSize = size,
                    TotalRecords = 0
                }
            };

            A.CallTo(() => _authorService.GetAllAuthorsAsync(page, size, term)).Returns(data);

            // Act
            var result = await _controller.GetAllAuthor(page, size, term);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().BeEquivalentTo(data);
        }
        [Fact]
        public async Task GetAuthorById_ReturnsBadRequest_WhenFail()
        {
            // Arrange
            var id = Guid.NewGuid();
            var data = new Result<Author> { Success = false, Message = "Not Found" };

            A.CallTo(() => _authorService.GetAuthorByIdAsync(id)).Returns(data);

            // Act
            var result = await _controller.GetAuthorById(id);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
        [Fact]
        public async Task AddAuthor_ReturnsOk_WhenSuccess()
        {
            // Arrange
            var dto = new AddAuthorReq { AuthorFullName = "Test Author" };
            var resultObj = new Result<Author> { Success = true, Data = new Author() };

            A.CallTo(() => _authorService.AddAuthorAsync(dto, "testUser")).Returns(resultObj);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.Name, "testUser")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.AddAuthor(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().BeEquivalentTo(resultObj);
        }
        [Fact]
        public async Task DeleteAuthor_ReturnsBadRequest_WhenFail()
        {
            // Arrange
            var id = Guid.NewGuid();
            var resultObj = new Result<Author> { Success = false, Message = "Không tìm thấy" };

            A.CallTo(() => _authorService.RemoveAuthorAsync(id, "admin")).Returns(resultObj);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.Name, "admin")
    }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.DeleteAuthor(id);

            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            badResult.Value.Should().Be(resultObj);
        }
    }
}
