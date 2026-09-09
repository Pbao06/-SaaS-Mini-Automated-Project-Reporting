using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaS.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldStatusForOtpCodeSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "OtpCodes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "OtpCodes");
        }
    }
}
