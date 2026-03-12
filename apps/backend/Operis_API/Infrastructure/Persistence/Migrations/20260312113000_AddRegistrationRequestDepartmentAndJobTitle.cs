using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    public partial class AddRegistrationRequestDepartmentAndJobTitle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "department_id",
                table: "user_registration_requests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "job_title_id",
                table: "user_registration_requests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_registration_requests_department_id",
                table: "user_registration_requests",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_registration_requests_job_title_id",
                table: "user_registration_requests",
                column: "job_title_id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_registration_requests_departments_department_id",
                table: "user_registration_requests",
                column: "department_id",
                principalTable: "departments",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_user_registration_requests_job_titles_job_title_id",
                table: "user_registration_requests",
                column: "job_title_id",
                principalTable: "job_titles",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_registration_requests_departments_department_id",
                table: "user_registration_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_user_registration_requests_job_titles_job_title_id",
                table: "user_registration_requests");

            migrationBuilder.DropIndex(
                name: "IX_user_registration_requests_department_id",
                table: "user_registration_requests");

            migrationBuilder.DropIndex(
                name: "IX_user_registration_requests_job_title_id",
                table: "user_registration_requests");

            migrationBuilder.DropColumn(
                name: "department_id",
                table: "user_registration_requests");

            migrationBuilder.DropColumn(
                name: "job_title_id",
                table: "user_registration_requests");
        }
    }
}
