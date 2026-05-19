using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jobcv.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCvVersioningAndTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBaseCv",
                table: "Cvs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ParentCvId",
                table: "Cvs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetJobId",
                table: "Cvs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TemplateName",
                table: "Cvs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Cvs_ParentCvId",
                table: "Cvs",
                column: "ParentCvId");

            migrationBuilder.CreateIndex(
                name: "IX_Cvs_TargetJobId",
                table: "Cvs",
                column: "TargetJobId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cvs_Cvs_ParentCvId",
                table: "Cvs",
                column: "ParentCvId",
                principalTable: "Cvs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cvs_Jobs_TargetJobId",
                table: "Cvs",
                column: "TargetJobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cvs_Cvs_ParentCvId",
                table: "Cvs");

            migrationBuilder.DropForeignKey(
                name: "FK_Cvs_Jobs_TargetJobId",
                table: "Cvs");

            migrationBuilder.DropIndex(
                name: "IX_Cvs_ParentCvId",
                table: "Cvs");

            migrationBuilder.DropIndex(
                name: "IX_Cvs_TargetJobId",
                table: "Cvs");

            migrationBuilder.DropColumn(
                name: "IsBaseCv",
                table: "Cvs");

            migrationBuilder.DropColumn(
                name: "ParentCvId",
                table: "Cvs");

            migrationBuilder.DropColumn(
                name: "TargetJobId",
                table: "Cvs");

            migrationBuilder.DropColumn(
                name: "TemplateName",
                table: "Cvs");
        }
    }
}
