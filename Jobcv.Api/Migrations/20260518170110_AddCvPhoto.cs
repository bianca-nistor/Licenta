using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jobcv.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCvPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoContentType",
                table: "Cvs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhotoFileName",
                table: "Cvs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "Cvs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoContentType",
                table: "Cvs");

            migrationBuilder.DropColumn(
                name: "PhotoFileName",
                table: "Cvs");

            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "Cvs");
        }
    }
}
