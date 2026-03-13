using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LinkPermanentOrgHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "department_id",
                table: "job_titles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_job_titles_department_id",
                table: "job_titles",
                column: "department_id");

            migrationBuilder.AddForeignKey(
                name: "FK_job_titles_departments_department_id",
                table: "job_titles",
                column: "department_id",
                principalTable: "departments",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_job_titles_departments_department_id",
                table: "job_titles");

            migrationBuilder.DropIndex(
                name: "IX_job_titles_department_id",
                table: "job_titles");

            migrationBuilder.DropColumn(
                name: "department_id",
                table: "job_titles");
        }
    }
}
