using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectAssignmentHistoryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "change_reason",
                table: "user_project_assignments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "replaced_by_assignment_id",
                table: "user_project_assignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "user_project_assignments",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.Sql("""
                UPDATE user_project_assignments
                SET status = 'Active'
                WHERE status IS NULL OR status = '';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_user_project_assignments_project_id_status_start_at",
                table: "user_project_assignments",
                columns: new[] { "project_id", "status", "start_at" });

            migrationBuilder.CreateIndex(
                name: "IX_user_project_assignments_replaced_by_assignment_id",
                table: "user_project_assignments",
                column: "replaced_by_assignment_id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_project_assignments_user_project_assignments_replaced_~",
                table: "user_project_assignments",
                column: "replaced_by_assignment_id",
                principalTable: "user_project_assignments",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_project_assignments_user_project_assignments_replaced_~",
                table: "user_project_assignments");

            migrationBuilder.DropIndex(
                name: "IX_user_project_assignments_project_id_status_start_at",
                table: "user_project_assignments");

            migrationBuilder.DropIndex(
                name: "IX_user_project_assignments_replaced_by_assignment_id",
                table: "user_project_assignments");

            migrationBuilder.DropColumn(
                name: "change_reason",
                table: "user_project_assignments");

            migrationBuilder.DropColumn(
                name: "replaced_by_assignment_id",
                table: "user_project_assignments");

            migrationBuilder.DropColumn(
                name: "status",
                table: "user_project_assignments");
        }
    }
}
