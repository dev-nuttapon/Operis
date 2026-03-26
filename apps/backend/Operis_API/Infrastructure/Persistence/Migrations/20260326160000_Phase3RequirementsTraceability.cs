using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    public partial class Phase3RequirementsTraceability : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "requirement_baselines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    baseline_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    requirement_ids_json = table.Column<string>(type: "jsonb", nullable: false),
                    reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    approved_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_requirement_baselines", x => x.id);
                    table.ForeignKey(
                        name: "FK_requirement_baselines_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "requirements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    description = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    priority = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    owner_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    current_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_requirements", x => x.id);
                    table.ForeignKey(
                        name: "FK_requirements_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "traceability_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    source_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    target_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    target_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    link_rule = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_traceability_links", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "requirement_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    requirement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    business_reason = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    acceptance_criteria = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    security_impact = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    performance_impact = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_requirement_versions", x => x.id);
                    table.ForeignKey(
                        name: "FK_requirement_versions_requirements_requirement_id",
                        column: x => x.requirement_id,
                        principalTable: "requirements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_requirement_baselines_project_id_status",
                table: "requirement_baselines",
                columns: new[] { "project_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_requirements_owner_user_id",
                table: "requirements",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_requirements_current_version_id",
                table: "requirements",
                column: "current_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_requirements_project_id_code",
                table: "requirements",
                columns: new[] { "project_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_requirements_project_id_status_priority",
                table: "requirements",
                columns: new[] { "project_id", "status", "priority" });

            migrationBuilder.CreateIndex(
                name: "IX_requirement_versions_requirement_id_version_number",
                table: "requirement_versions",
                columns: new[] { "requirement_id", "version_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_traceability_links_source_id",
                table: "traceability_links",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_traceability_links_status_source_type_target_type",
                table: "traceability_links",
                columns: new[] { "status", "source_type", "target_type" });

            migrationBuilder.CreateIndex(
                name: "IX_traceability_links_target_id",
                table: "traceability_links",
                column: "target_id");

            migrationBuilder.CreateIndex(
                name: "IX_traceability_links_source_type_source_id_target_type_target_id_link_rule",
                table: "traceability_links",
                columns: new[] { "source_type", "source_id", "target_type", "target_id", "link_rule" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_requirements_requirement_versions_current_version_id",
                table: "requirements",
                column: "current_version_id",
                principalTable: "requirement_versions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.Sql(
                """
                INSERT INTO app_roles (id, name, keycloak_role_name, description, display_order, created_at)
                VALUES
                    ('55555555-0000-0000-0000-000000000001', 'Requirements Manager', 'operis:requirements_manager', 'Manages requirement register and traceability.', 60, NOW()),
                    ('55555555-0000-0000-0000-000000000002', 'Requirements Approver', 'operis:requirements_approver', 'Approves and baselines requirements.', 61, NOW())
                ON CONFLICT DO NOTHING;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM app_roles
                WHERE keycloak_role_name IN ('operis:requirements_manager', 'operis:requirements_approver');
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_requirements_requirement_versions_current_version_id",
                table: "requirements");

            migrationBuilder.DropTable(name: "requirement_baselines");
            migrationBuilder.DropTable(name: "traceability_links");
            migrationBuilder.DropTable(name: "requirement_versions");
            migrationBuilder.DropTable(name: "requirements");
        }
    }
}
