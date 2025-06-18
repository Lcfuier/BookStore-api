using BookStore.Domain.Models;
using BookStore.Domain.Queries;
using BookStore.Infrastructure.Data;
using BookStore.Infrastructure.Repositorys;
using BookStore.UnitTest.Mocks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.UnitTest.Repositories
{
    public class AuthorRepositoryTests
    {
        private async Task<ApplicationDbContext> SeedDatabaseContext()
        {
            var context = MockDbContext.CreateMockDbContext();
            var author1 = new Author
            {
                Id = new Guid("cf7dd825-4ae5-4cb9-b399-e48fffcfc2c0"),
                AuthorFullName = "Nguyen A",
               CreatedTime=DateTime.UtcNow,
            };
            var author2 = new Author
            {
                Id = new Guid("ae480964-1458-4de2-90d5-c08ef090fb25"),
                AuthorFullName = "Tran B",
                CreatedTime = DateTime.UtcNow,
            };
            var author3 = new Author
            {
                Id = new Guid("eb6577bf-e5e8-46d6-bca2-fc72bca57b8f"),
                AuthorFullName = "Pham C",
                CreatedTime = DateTime.UtcNow,
            };
            await context.Author.AddAsync(author1);
            await context.Author.AddAsync(author2);
            await context.Author.AddAsync(author3);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            return context;
        }
        [Fact]
        public async Task GetAuthorsAsync_WhenSuccessful_ShouldReturnAuthors()
        {
            // Arrange
            var context = await SeedDatabaseContext();
            var sut = new AuthorRepository(context);

            // Act
            var actual = await sut.GetAll();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<Author>>(actual);
            Assert.Equal(context.Author.Count(), actual.Count());
        }
        [Fact]
        public async Task GetAuthorByIdAsync_WhenSuccessful_ShouldReturnAuthor()
        {
            // Arrange
            var id = new Guid("cf7dd825-4ae5-4cb9-b399-e48fffcfc2c0");
            var context = await SeedDatabaseContext();
            var sut = new AuthorRepository(context);

            // Act
            var actual = await sut.GetAsync(new QueryOptions<Author>
            {
                Where = c => c.Id == id
            });

            // Assert
            Assert.IsType<Author>(actual);
        }
        [Fact]
        public async Task AddAuthorAsync_WhenSuccessful_ShouldAddAuthor()
        {
            // Arrange
            var author = new Author
            {
                Id = new Guid("424f8543-b34b-4e7a-90a3-5b5271fd3224"),
                AuthorFullName = "Tran D",
                CreatedTime = DateTime.UtcNow,
            };
            var context = await SeedDatabaseContext();
            var sut = new AuthorRepository(context);

            // Act
            sut.Add(author);
            await context.SaveChangesAsync();

            // Assert
            Assert.NotNull(await context.Author.FirstOrDefaultAsync(x => x.Id == author.Id));
        }
        [Fact]
        public async Task DeleteAuthorAsync_WhenSuccessful_ShouldUpdateAuthor()
        {
            // Arrange
            var id = new Guid("cf7dd825-4ae5-4cb9-b399-e48fffcfc2c0");
            var context = await SeedDatabaseContext();
            var sut = new AuthorRepository(context);

            // Act
            var actual = await sut.GetAsync(new QueryOptions<Author>
            {
                Includes = "Books",
                Where = c => c.Id == id
            });
            if (actual != null)
            {
                sut.Remove(actual);
            }
            await context.SaveChangesAsync();

            // Assert
            Assert.Null(await context.Author.FindAsync(id));
        }
    }
}
