using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Operis_API.Infrastructure.Persistence;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(OperisDbContext))]
    [Migration("20260319165000_RemoveProjectRoleWorkflowFlags")]
    public partial class RemoveProjectRoleWorkflowFlags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "can_approve_documents",
                table: "project_roles");

            migrationBuilder.DropColumn(
                name: "can_create_documents",
                table: "project_roles");

            migrationBuilder.DropColumn(
                name: "can_release_documents",
                table: "project_roles");

            migrationBuilder.DropColumn(
                name: "can_review_documents",
                table: "project_roles");

            migrationBuilder.DropColumn(
                name: "is_peer_review_role",
                table: "project_roles");

            migrationBuilder.DropColumn(
                name: "is_review_role",
                table: "project_roles");

            migrationBuilder.DropColumn(
                name: "is_approval_role",
                table: "project_roles");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "can_approve_documents",
                table: "project_roles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "can_create_documents",
                table: "project_roles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "can_release_documents",
                table: "project_roles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "can_review_documents",
                table: "project_roles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_peer_review_role",
                table: "project_roles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_review_role",
                table: "project_roles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_approval_role",
                table: "project_roles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
