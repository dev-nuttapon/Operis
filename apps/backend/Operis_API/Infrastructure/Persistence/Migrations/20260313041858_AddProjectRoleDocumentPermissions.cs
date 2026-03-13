using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectRoleDocumentPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
