using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Operis_API.Infrastructure.Persistence;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(OperisDbContext))]
    [Migration("20260318103000_AddProjectEvidenceIndexes")]
    public partial class AddProjectEvidenceIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_project_roles_project_id_deleted_at",
                table: "project_roles",
                columns: new[] { "project_id", "deleted_at" });

            migrationBuilder.CreateIndex(
                name: "IX_project_roles_project_id_display_order_name",
                table: "project_roles",
                columns: new[] { "project_id", "display_order", "name" },
                filter: "\"deleted_at\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_user_project_assignments_project_id_created_at",
                table: "user_project_assignments",
                columns: new[] { "project_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_user_project_assignments_project_role_id_status",
                table: "user_project_assignments",
                columns: new[] { "project_role_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_user_project_assignments_project_id_status_created_at",
                table: "user_project_assignments",
                columns: new[] { "project_id", "status", "created_at" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_project_roles_project_id_deleted_at",
                table: "project_roles");

            migrationBuilder.DropIndex(
                name: "IX_project_roles_project_id_display_order_name",
                table: "project_roles");

            migrationBuilder.DropIndex(
                name: "IX_user_project_assignments_project_id_created_at",
                table: "user_project_assignments");

            migrationBuilder.DropIndex(
                name: "IX_user_project_assignments_project_role_id_status",
                table: "user_project_assignments");

            migrationBuilder.DropIndex(
                name: "IX_user_project_assignments_project_id_status_created_at",
                table: "user_project_assignments");
        }
    }
}
