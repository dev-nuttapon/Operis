using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectRoleGovernanceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "authority_scope",
                table: "project_roles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "project_roles",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "project_roles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_approval_role",
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

            migrationBuilder.AddColumn<string>(
                name: "responsibilities",
                table: "project_roles",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_project_roles_project_id_code",
                table: "project_roles",
                columns: new[] { "project_id", "code" },
                unique: true,
                filter: "\"deleted_at\" IS NULL AND \"code\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_project_roles_project_id_code",
                table: "project_roles");

            migrationBuilder.DropColumn(
                name: "authority_scope",
                table: "project_roles");

            migrationBuilder.DropColumn(
                name: "code",
                table: "project_roles");

            migrationBuilder.DropColumn(
                name: "description",
                table: "project_roles");

            migrationBuilder.DropColumn(
                name: "is_approval_role",
                table: "project_roles");

            migrationBuilder.DropColumn(
                name: "is_review_role",
                table: "project_roles");

            migrationBuilder.DropColumn(
                name: "responsibilities",
                table: "project_roles");
        }
    }
}
