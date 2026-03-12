using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitationDepartmentAndJobTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "department_id",
                table: "user_invitations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "job_title_id",
                table: "user_invitations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_invitations_department_id",
                table: "user_invitations",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_invitations_job_title_id",
                table: "user_invitations",
                column: "job_title_id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_invitations_departments_department_id",
                table: "user_invitations",
                column: "department_id",
                principalTable: "departments",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_user_invitations_job_titles_job_title_id",
                table: "user_invitations",
                column: "job_title_id",
                principalTable: "job_titles",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_invitations_departments_department_id",
                table: "user_invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_user_invitations_job_titles_job_title_id",
                table: "user_invitations");

            migrationBuilder.DropIndex(
                name: "IX_user_invitations_department_id",
                table: "user_invitations");

            migrationBuilder.DropIndex(
                name: "IX_user_invitations_job_title_id",
                table: "user_invitations");

            migrationBuilder.DropColumn(
                name: "department_id",
                table: "user_invitations");

            migrationBuilder.DropColumn(
                name: "job_title_id",
                table: "user_invitations");
        }
    }
}
