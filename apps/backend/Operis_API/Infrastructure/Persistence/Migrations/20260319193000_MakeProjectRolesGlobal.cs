using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations;

public partial class MakeProjectRolesGlobal : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_project_roles_projects_project_id",
            table: "project_roles");

        migrationBuilder.DropIndex(
            name: "IX_project_roles_project_id",
            table: "project_roles");

        migrationBuilder.DropIndex(
            name: "IX_project_roles_project_id_name",
            table: "project_roles");

        migrationBuilder.DropIndex(
            name: "IX_project_roles_project_id_code",
            table: "project_roles");

        migrationBuilder.DropIndex(
            name: "IX_project_roles_project_id_deleted_at",
            table: "project_roles");

        migrationBuilder.DropIndex(
            name: "IX_project_roles_project_id_display_order_name",
            table: "project_roles");

        migrationBuilder.Sql("UPDATE project_roles SET project_id = NULL;");

        migrationBuilder.CreateIndex(
            name: "IX_project_roles_name",
            table: "project_roles",
            column: "name",
            unique: true,
            filter: "\"deleted_at\" IS NULL");

        migrationBuilder.CreateIndex(
            name: "IX_project_roles_code",
            table: "project_roles",
            column: "code",
            unique: true,
            filter: "\"deleted_at\" IS NULL AND \"code\" IS NOT NULL");

        migrationBuilder.AddForeignKey(
            name: "FK_project_roles_projects_project_id",
            table: "project_roles",
            column: "project_id",
            principalTable: "projects",
            principalColumn: "id",
            onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_project_roles_projects_project_id",
            table: "project_roles");

        migrationBuilder.DropIndex(
            name: "IX_project_roles_name",
            table: "project_roles");

        migrationBuilder.DropIndex(
            name: "IX_project_roles_code",
            table: "project_roles");

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

        migrationBuilder.CreateIndex(
            name: "IX_project_roles_project_id_code",
            table: "project_roles",
            columns: new[] { "project_id", "code" },
            unique: true,
            filter: "\"deleted_at\" IS NULL AND \"code\" IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_project_roles_project_id_deleted_at",
            table: "project_roles",
            columns: new[] { "project_id", "deleted_at" });

        migrationBuilder.CreateIndex(
            name: "IX_project_roles_project_id_display_order_name",
            table: "project_roles",
            columns: new[] { "project_id", "display_order", "name" },
            filter: "\"deleted_at\" IS NULL");

        migrationBuilder.AddForeignKey(
            name: "FK_project_roles_projects_project_id",
            table: "project_roles",
            column: "project_id",
            principalTable: "projects",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }
}
