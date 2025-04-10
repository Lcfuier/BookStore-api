using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BookStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addReviewBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "567617d7-b74c-470b-9454-6ebbcefcd149");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5a3959f6-f0bd-4e8d-b43f-28cccb038ac3");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "88c94cdf-c188-4033-8649-8f937365ef5e");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "6bd1e637-3622-4417-97d1-48a1d7a8d573", "2", "Librarian", "Librarian" },
                    { "78e130e2-f883-41a7-8731-03107836a6cf", "3", "User", "User" },
                    { "dd076070-9422-4469-94ee-99656266079c", "1", "Admin", "Admin" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6bd1e637-3622-4417-97d1-48a1d7a8d573");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "78e130e2-f883-41a7-8731-03107836a6cf");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "dd076070-9422-4469-94ee-99656266079c");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "567617d7-b74c-470b-9454-6ebbcefcd149", "3", "User", "User" },
                    { "5a3959f6-f0bd-4e8d-b43f-28cccb038ac3", "1", "Admin", "Admin" },
                    { "88c94cdf-c188-4033-8649-8f937365ef5e", "2", "Librarian", "Librarian" }
                });
        }
    }
}
