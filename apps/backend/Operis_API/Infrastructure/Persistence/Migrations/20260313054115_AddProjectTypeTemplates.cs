using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectTypeTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "project_type_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    require_sponsor = table.Column<bool>(type: "boolean", nullable: false),
                    require_planned_period = table.Column<bool>(type: "boolean", nullable: false),
                    require_active_team = table.Column<bool>(type: "boolean", nullable: false),
                    require_primary_assignment = table.Column<bool>(type: "boolean", nullable: false),
                    require_reporting_root = table.Column<bool>(type: "boolean", nullable: false),
                    require_document_creator = table.Column<bool>(type: "boolean", nullable: false),
                    require_reviewer = table.Column<bool>(type: "boolean", nullable: false),
                    require_approver = table.Column<bool>(type: "boolean", nullable: false),
                    require_release_role = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_type_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "project_type_role_requirements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_type_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    role_code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_type_role_requirements", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_type_role_requirements_project_type_templates_proje~",
                        column: x => x.project_type_template_id,
                        principalTable: "project_type_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_project_type_role_requirements_project_type_template_id_ro~1",
                table: "project_type_role_requirements",
                columns: new[] { "project_type_template_id", "role_name" },
                filter: "\"deleted_at\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_project_type_role_requirements_project_type_template_id_rol~",
                table: "project_type_role_requirements",
                columns: new[] { "project_type_template_id", "role_code" },
                filter: "\"deleted_at\" IS NULL AND \"role_code\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_project_type_templates_project_type",
                table: "project_type_templates",
                column: "project_type",
                unique: true,
                filter: "\"deleted_at\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_type_role_requirements");

            migrationBuilder.DropTable(
                name: "project_type_templates");
        }
    }
}
