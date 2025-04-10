using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BookStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "370ba9a5-af64-4cc7-9428-1caef4c130b4");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "39d1661f-e5bd-4468-b832-6e64a46d3140");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "771dfafe-c6f0-49a4-91d1-0aa9b5a45ee1");

            migrationBuilder.AlterColumn<string>(
                name: "ShippingStatus",
                table: "Order",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ShippingCost",
                table: "Order",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<string>(
                name: "ShippingStatus",
                table: "Order",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingCost",
                table: "Order",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "370ba9a5-af64-4cc7-9428-1caef4c130b4", "3", "User", "User" },
                    { "39d1661f-e5bd-4468-b832-6e64a46d3140", "2", "Librarian", "Librarian" },
                    { "771dfafe-c6f0-49a4-91d1-0aa9b5a45ee1", "1", "Admin", "Admin" }
                });
        }
    }
}
