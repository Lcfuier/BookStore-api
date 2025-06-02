using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BookStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cart_Cart_CartId1",
                table: "Cart");

            migrationBuilder.DropIndex(
                name: "IX_Cart_CartId1",
                table: "Cart");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "899f43cc-36d9-477a-b24a-0ab78a095663");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c2020658-1184-4a32-beed-cdcdddb3de60");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cf3c63c9-7d4d-4ecb-ac32-797eb817c0af");

            migrationBuilder.DropColumn(
                name: "CartId1",
                table: "Cart");

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedTime",
                table: "CartItem",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2677b2d2-4cdc-4270-8582-90323c2205d0", "1", "Admin", "Admin" },
                    { "64663f85-e6f7-48af-8e97-2bf0103fa543", "2", "Librarian", "Librarian" },
                    { "72cd916a-c24f-4c0a-9093-74bd8a6de0a2", "3", "User", "User" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2677b2d2-4cdc-4270-8582-90323c2205d0");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "64663f85-e6f7-48af-8e97-2bf0103fa543");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "72cd916a-c24f-4c0a-9093-74bd8a6de0a2");

            migrationBuilder.DropColumn(
                name: "ModifiedTime",
                table: "CartItem");

            migrationBuilder.AddColumn<Guid>(
                name: "CartId1",
                table: "Cart",
                type: "uuid",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "899f43cc-36d9-477a-b24a-0ab78a095663", "2", "Librarian", "Librarian" },
                    { "c2020658-1184-4a32-beed-cdcdddb3de60", "3", "User", "User" },
                    { "cf3c63c9-7d4d-4ecb-ac32-797eb817c0af", "1", "Admin", "Admin" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cart_CartId1",
                table: "Cart",
                column: "CartId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Cart_Cart_CartId1",
                table: "Cart",
                column: "CartId1",
                principalTable: "Cart",
                principalColumn: "CartId");
        }
    }
}
