using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations;

public partial class RemoveProjectIdFromProjectRoles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_project_roles_projects_project_id",
            table: "project_roles");

        migrationBuilder.DropIndex(
            name: "IX_project_roles_project_id",
            table: "project_roles");

        migrationBuilder.DropColumn(
            name: "project_id",
            table: "project_roles");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "project_id",
            table: "project_roles",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_project_roles_project_id",
            table: "project_roles",
            column: "project_id");

        migrationBuilder.AddForeignKey(
            name: "FK_project_roles_projects_project_id",
            table: "project_roles",
            column: "project_id",
            principalTable: "projects",
            principalColumn: "id",
            onDelete: ReferentialAction.SetNull);
    }
}
