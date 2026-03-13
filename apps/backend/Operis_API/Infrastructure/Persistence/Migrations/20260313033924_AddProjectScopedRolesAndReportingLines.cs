using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectScopedRolesAndReportingLines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_project_roles_name",
                table: "project_roles");

            migrationBuilder.AddColumn<string>(
                name: "reports_to_user_id",
                table: "user_project_assignments",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "project_id",
                table: "project_roles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_project_assignments_reports_to_user_id",
                table: "user_project_assignments",
                column: "reports_to_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_roles_project_id",
                table: "project_roles",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_roles_project_id_name",
                table: "project_roles",
                columns: new[] { "project_id", "name" },
                unique: true,
                filter: "\"deleted_at\" IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_project_roles_projects_project_id",
                table: "project_roles",
                column: "project_id",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_project_assignments_users_reports_to_user_id",
                table: "user_project_assignments",
                column: "reports_to_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_project_roles_projects_project_id",
                table: "project_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_user_project_assignments_users_reports_to_user_id",
                table: "user_project_assignments");

            migrationBuilder.DropIndex(
                name: "IX_user_project_assignments_reports_to_user_id",
                table: "user_project_assignments");

            migrationBuilder.DropIndex(
                name: "IX_project_roles_project_id",
                table: "project_roles");

            migrationBuilder.DropIndex(
                name: "IX_project_roles_project_id_name",
                table: "project_roles");

            migrationBuilder.DropColumn(
                name: "reports_to_user_id",
                table: "user_project_assignments");

            migrationBuilder.DropColumn(
                name: "project_id",
                table: "project_roles");

            migrationBuilder.CreateIndex(
                name: "IX_project_roles_name",
                table: "project_roles",
                column: "name",
                unique: true,
                filter: "\"deleted_at\" IS NULL");
        }
    }
}
