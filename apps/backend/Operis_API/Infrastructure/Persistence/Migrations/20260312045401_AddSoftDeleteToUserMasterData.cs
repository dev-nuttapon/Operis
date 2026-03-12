using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToUserMasterData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_job_titles_name",
                table: "job_titles");

            migrationBuilder.DropIndex(
                name: "IX_departments_name",
                table: "departments");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "job_titles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by",
                table: "job_titles",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "departments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by",
                table: "departments",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_job_titles_name",
                table: "job_titles",
                column: "name",
                unique: true,
                filter: "\"deleted_at\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_departments_name",
                table: "departments",
                column: "name",
                unique: true,
                filter: "\"deleted_at\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_job_titles_name",
                table: "job_titles");

            migrationBuilder.DropIndex(
                name: "IX_departments_name",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "job_titles");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "job_titles");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "departments");

            migrationBuilder.CreateIndex(
                name: "IX_job_titles_name",
                table: "job_titles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_departments_name",
                table: "departments",
                column: "name",
                unique: true);
        }
    }
}
