using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaS.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOtpCodeSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VerifiedToken",
                table: "OtpCodes",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedTokenDate",
                table: "OtpCodes",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerifiedToken",
                table: "OtpCodes");

            migrationBuilder.DropColumn(
                name: "VerifiedTokenDate",
                table: "OtpCodes");
        }
    }
}
