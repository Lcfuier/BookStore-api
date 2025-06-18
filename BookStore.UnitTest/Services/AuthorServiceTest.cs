using AutoFixture;
using AutoMapper;
using BookStore.Application.Interface;
using BookStore.Application.InterfacesRepository;
using BookStore.Application.Services;
using BookStore.Domain.Constants;
using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Domain.Queries;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.UnitTest.Services
{
    public class AuthorServiceTest
    {
        private readonly Fixture _fixture;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthorService _sut;
        private readonly IAuthorRepository _authorRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public AuthorServiceTest()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
               .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _unitOfWork = A.Fake<IUnitOfWork>();
            _authorRepository = A.Fake<IAuthorRepository>();
            _userManager=A.Fake<UserManager<ApplicationUser>>();
            A.CallTo(() => _unitOfWork.Author).Returns(_authorRepository);
            _sut = new AuthorService(_unitOfWork,_userManager);
        }

        // Test methods go here...
        [Fact]
        public async Task GetAllAuthorsAsync_WhenCalled_ShouldReturnAllAuthors()
        {
            // Arrange
            var authors = _fixture.CreateMany<Author>(3);
            A.CallTo(() => _authorRepository.ListAllAsync(A<QueryOptions<Author>>._)).Returns(Task.FromResult(authors));

            // Act
            var result = await _sut.GetAllAuthorsAsync(0,1,"");

            // Assert
            A.CallTo(() => _authorRepository.ListAllAsync(A<QueryOptions<Author>>._)).MustHaveHappenedOnceExactly();
            Assert.Equal(authors, result.Data.Items);
        }
        [Fact]
        public async Task GetAuthorByIdAsync_WhenCalled_ShouldReturnAuthorWithBooks()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var expectedAuthor = _fixture.Build<Author>()
                                         .With(a => a.Id, authorId)
                                         .Create();

            // Set up the query options to match the expected options in the service method
            var options = new QueryOptions<Author>
            {
                Where = a => a.Id == authorId,
                Includes = "Books"
            };

            A.CallTo(() => _authorRepository.GetAsync(A<QueryOptions<Author>>._))
                .Returns(expectedAuthor);

            // Act
            var result = await _sut.GetAuthorByIdAsync(authorId);

            // Assert
            A.CallTo(() => _authorRepository.GetAsync(A<QueryOptions<Author>>._)).MustHaveHappenedOnceExactly();

            Assert.Equal(expectedAuthor, result.Data);
        }
        [Fact]
        public async Task GetAuthorsByTermAsync_WhenCalled_ShouldReturnFilteredAuthors()
        {
            string searchTerm = "John";
            var authors = _fixture.CreateMany<Author>(5).ToList();

            // Setup query options to match the filtering logic
            var options = new QueryOptions<Author>
            {
                Where = a => a.AuthorFullName.Contains(searchTerm) 
            };

            A.CallTo(() => _authorRepository.ListAllAsync(A<QueryOptions<Author>>._))
                .Returns(authors);

            // Act
            var result = await _sut.GetAllAuthorsAsync(0,1,searchTerm);

            // Assert
            A.CallTo(() => _authorRepository.ListAllAsync(A<QueryOptions<Author>>._)).MustHaveHappenedOnceExactly();

            Assert.Equal(authors, result.Data.Items);
        }
        [Fact]
        public async Task AddAuthorAsync_WithValidAdminUser_ShouldAddAuthorAndSave()
        {
            // Arrange
            var authorDto = new AddAuthorReq { AuthorFullName = "Nguyễn Nhật Ánh" };
            var userName = "adminuser";

            var user = _fixture.Create<ApplicationUser>();
            var authorRepository = A.Fake<IAuthorRepository>();
            var addedAuthor = new Author { AuthorFullName = authorDto.AuthorFullName };

            // Fake UserManager behavior
            A.CallTo(() => _userManager.FindByNameAsync(userName)).Returns(user);
            A.CallTo(() => _userManager.GetRolesAsync(user)).Returns(new List<string> { Roles.Admin });

            // Giả lập Add và SaveAsync
            A.CallTo(() => _authorRepository.Add(A<Author>._))
                .Invokes((Author a) => addedAuthor = a); // Giả lập lưu
            A.CallTo(() => _unitOfWork.SaveAsync()).Returns(Task.CompletedTask);
            A.CallTo(() => _unitOfWork.Author).Returns(_authorRepository);

            // Act
            var result = await _sut.AddAuthorAsync(authorDto, userName);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.AuthorFullName.Should().Be(authorDto.AuthorFullName);

            A.CallTo(() => _authorRepository.Add(A<Author>.That.Matches(
                a => a.AuthorFullName == authorDto.AuthorFullName && a.CreatedBy == userName
            ))).MustHaveHappenedOnceExactly();

            A.CallTo(() => _unitOfWork.SaveAsync()).MustHaveHappenedOnceExactly();
        }
        [Fact]
        public async Task RemoveAuthorAsync_WithValidAdminUserAndExistingAuthor_ShouldRemoveAndSave()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var userName = "adminuser";
            var author = _fixture.Build<Author>()
                                 .With(a => a.Id, authorId)
                                 .Create();

            var user = _fixture.Create<ApplicationUser>();

            // Giả lập tìm thấy user và có vai trò Admin
            A.CallTo(() => _userManager.FindByNameAsync(userName)).Returns(user);
            A.CallTo(() => _userManager.GetRolesAsync(user)).Returns(new List<string> { Roles.Admin });

            // Giả lập tìm thấy tác giả
            A.CallTo(() => _authorRepository.GetAsync(A<QueryOptions<Author>>._)).Returns(author);

            // Giả lập các call cần thiết
            A.CallTo(() => _unitOfWork.Author).Returns(_authorRepository);
            A.CallTo(() => _unitOfWork.SaveAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.RemoveAuthorAsync(authorId, userName);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Xóa tác giả thành công!");

            A.CallTo(() => _authorRepository.Remove(A<Author>.That.Matches(a => a.Id == authorId))).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveAsync()).MustHaveHappenedOnceExactly();
        }

    }

}
