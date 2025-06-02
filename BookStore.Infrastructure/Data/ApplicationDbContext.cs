using BookStore.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext     (DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Book> Book { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<Author> Author { get; set; }
        public DbSet<Publisher> Publisher { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Chat> Chat { get; set; }
        public DbSet<Message> Message { get; set; }
        public DbSet<OrderItem> OrderItem { get; set; }
        public DbSet<PaymentProfile> PaymentProfile { get; set; }
        public DbSet<CartItem> CartItem { get; set; }
        public DbSet<Cart> Cart { get; set; }
        public DbSet<ApplicationUser> Customer { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasMany(d => d.Books).WithMany(p => p.Categories)
                    .UsingEntity<Dictionary<string, object>>(
                        "BookCategory",
                        r => r.HasOne<Book>().WithMany()
                            .HasForeignKey("BookId")
                            .HasConstraintName("FK_BOOKCATE_BOOKCATEG_BOOKS"),
                        l => l.HasOne<Category>().WithMany()
                            .HasForeignKey("CategoryId")
                            .HasConstraintName("FK_BOOKCATE_BOOKCATEG_CATEGORI"),
                        j =>
                        {
                            j.HasKey("CategoryId", "BookId").HasName("PK_BOOKCATEGORY");
                            j.ToTable("BookCategory");
                            j.HasIndex(new[] { "BookId" }, "BOOKCATEGORY2_FK");
                            j.HasIndex(new[] { "CategoryId" }, "BOOKCATEGORY_FK");
                        });
            });
            //SeedRoles(modelBuilder);
        }
        /*private void SeedRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityRole>().HasData(
               new IdentityRole() { Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" },
               new IdentityRole() { Name = "Librarian", ConcurrencyStamp = "2", NormalizedName = "Librarian" },
               new IdentityRole() { Name = "User", ConcurrencyStamp = "3", NormalizedName = "User" }
               );
        }*/
    }
}
