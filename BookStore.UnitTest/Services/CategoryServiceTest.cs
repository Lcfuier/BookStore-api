using AutoFixture;
using AutoMapper;
using BookStore.Application.Interface;
using BookStore.Application.InterfacesRepository;
using BookStore.Application.Services;
using BookStore.Domain.Constants;
using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Domain.Queries;
using BookStore.UnitTest.Mocks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.UnitTest.Services
{
    public class CategoryServiceTest
    {
        private readonly Fixture _fixture;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICategoryService _sut;
        private readonly UserManager<ApplicationUser> _userManager;
        public CategoryServiceTest()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _userManager = A.Fake<UserManager<ApplicationUser>>();
            var context = MockDbContext.CreateMockDbContext();
            _categoryRepository = A.Fake<ICategoryRepository>();

            // Setup the UnitOfWork to return the mocked repository
            _unitOfWork = A.Fake<IUnitOfWork>();
            A.CallTo(() => _unitOfWork.Category).Returns(_categoryRepository);

            _mapper = A.Fake<IMapper>();
            _sut = new CategoryService(_unitOfWork,_userManager);
        }

        [Fact]
        public async Task GetCategoriesAsync_WhenSuccessful_ShouldReturnCategories()
        {
            // Arrange
            var page = 1;
            var size = 10;
            var term = "";
            var categories = _fixture.CreateMany<Category>(3).ToList();

            A.CallTo(() => _unitOfWork.Category.ListAllAsync(A<QueryOptions<Category>>._))
                .Returns(categories);

            A.CallTo(() => _unitOfWork.Category.CountAsync())
                .Returns(categories.Count);

            // Act
            var result = await _sut.GetAllCategoriesAsync(page, size, term);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Items.Should().BeEquivalentTo(categories);
            result.Data.TotalRecords.Should().Be(categories.Count);

            A.CallTo(() => _unitOfWork.Category.ListAllAsync(A<QueryOptions<Category>>._))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.Category.CountAsync())
                .MustHaveHappenedOnceExactly();
        }


        [Fact]
        public async Task AddCategoryAsync_WithValidAdminUser_ShouldAddAndSaveCategory()
        {
            // Arrange
            var categoryDto = new AddCategoryReq { Name = "Khoa học" };
            var userName = "adminuser";
            var user = _fixture.Create<ApplicationUser>();

            A.CallTo(() => _userManager.FindByNameAsync(userName)).Returns(user);
            A.CallTo(() => _userManager.GetRolesAsync(user)).Returns(new List<string> { Roles.Admin });

            A.CallTo(() => _unitOfWork.Category).Returns(_categoryRepository);
            A.CallTo(() => _unitOfWork.SaveAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.AddCategoryAsync(categoryDto, userName);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Name.Should().Be(categoryDto.Name);

            A.CallTo(() => _categoryRepository.Add(A<Category>.That.Matches(c =>
                c.Name == categoryDto.Name &&
                c.CreatedBy == userName
            ))).MustHaveHappenedOnceExactly();

            A.CallTo(() => _unitOfWork.SaveAsync()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task DeleteCategoryAsync_WhenSuccessful_ShouldDeleteCategory()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var userName = "adminuser";
            var category = _fixture.Build<Category>().With(c => c.CategoryId, categoryId).Create();
            var user = _fixture.Create<ApplicationUser>();

            A.CallTo(() => _userManager.FindByNameAsync(userName)).Returns(user);
            A.CallTo(() => _userManager.GetRolesAsync(user)).Returns(new List<string> { Roles.Admin });
            A.CallTo(() => _categoryRepository.GetAsync(A<QueryOptions<Category>>._)).Returns(category);
            A.CallTo(() => _unitOfWork.Category).Returns(_categoryRepository);

            // Act
            var result = await _sut.RemoveCategoryAsync(categoryId, userName);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Xóa thể loại thành công!");

            A.CallTo(() => _categoryRepository.Remove(A<Category>.That.Matches(c => c.CategoryId == categoryId)))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _unitOfWork.SaveAsync()).MustHaveHappenedOnceExactly();
        }

    }
}
