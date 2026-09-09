using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SaaS.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "Description", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { "Admin", new DateTime(2026, 9, 9, 6, 12, 41, 991, DateTimeKind.Utc).AddTicks(9359), "Administrator role", "Admin", null },
                    { "User", new DateTime(2026, 9, 9, 6, 12, 41, 992, DateTimeKind.Utc).AddTicks(1472), "Regular user role", "User", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "Admin");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "User");
        }
    }
}
