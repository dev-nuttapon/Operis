using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    public partial class Phase4ChangeControlConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "baseline_registry",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    baseline_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    baseline_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    source_entity_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    source_entity_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    approved_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    change_request_id = table.Column<Guid>(type: "uuid", nullable: true),
                    superseded_by_baseline_id = table.Column<Guid>(type: "uuid", nullable: true),
                    override_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_baseline_registry", x => x.id);
                    table.ForeignKey(
                        name: "FK_baseline_registry_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_baseline_registry_baseline_registry_superseded_by_baseline_id",
                        column: x => x.superseded_by_baseline_id,
                        principalTable: "baseline_registry",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "configuration_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    item_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    owner_module = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    baseline_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuration_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_configuration_items_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "change_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    requested_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    reason = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    priority = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    target_baseline_id = table.Column<Guid>(type: "uuid", nullable: true),
                    linked_requirement_ids_json = table.Column<string>(type: "jsonb", nullable: false),
                    linked_configuration_item_ids_json = table.Column<string>(type: "jsonb", nullable: false),
                    decision_rationale = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    implementation_summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    approved_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_change_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_change_requests_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_change_requests_baseline_registry_target_baseline_id",
                        column: x => x.target_baseline_id,
                        principalTable: "baseline_registry",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "change_impacts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    change_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scope_impact = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    schedule_impact = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    quality_impact = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    security_impact = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    performance_impact = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    risk_impact = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_change_impacts", x => x.id);
                    table.ForeignKey(
                        name: "FK_change_impacts_change_requests_change_request_id",
                        column: x => x.change_request_id,
                        principalTable: "change_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_baseline_registry_project_id_baseline_type_status", table: "baseline_registry", columns: new[] { "project_id", "baseline_type", "status" });
            migrationBuilder.CreateIndex(name: "IX_baseline_registry_superseded_by_baseline_id", table: "baseline_registry", column: "superseded_by_baseline_id");
            migrationBuilder.CreateIndex(name: "IX_configuration_items_project_id_code", table: "configuration_items", columns: new[] { "project_id", "code" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_configuration_items_project_id_item_type_status", table: "configuration_items", columns: new[] { "project_id", "item_type", "status" });
            migrationBuilder.CreateIndex(name: "IX_change_requests_project_id_code", table: "change_requests", columns: new[] { "project_id", "code" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_change_requests_project_id_status_priority", table: "change_requests", columns: new[] { "project_id", "status", "priority" });
            migrationBuilder.CreateIndex(name: "IX_change_requests_target_baseline_id", table: "change_requests", column: "target_baseline_id");
            migrationBuilder.CreateIndex(name: "IX_change_impacts_change_request_id", table: "change_impacts", column: "change_request_id", unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_baseline_registry_change_requests_change_request_id",
                table: "baseline_registry",
                column: "change_request_id",
                principalTable: "change_requests",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.Sql(
                """
                INSERT INTO app_roles (id, name, keycloak_role_name, description, display_order, created_at)
                VALUES
                    ('66666666-0000-0000-0000-000000000001', 'Change Manager', 'operis:change_manager', 'Manages change requests and baseline creation.', 70, NOW()),
                    ('66666666-0000-0000-0000-000000000002', 'Configuration Controller', 'operis:configuration_controller', 'Manages configuration items.', 71, NOW())
                ON CONFLICT DO NOTHING;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM app_roles
                WHERE keycloak_role_name IN ('operis:change_manager', 'operis:configuration_controller');
                """);

            migrationBuilder.DropForeignKey(name: "FK_baseline_registry_change_requests_change_request_id", table: "baseline_registry");
            migrationBuilder.DropTable(name: "change_impacts");
            migrationBuilder.DropTable(name: "configuration_items");
            migrationBuilder.DropTable(name: "change_requests");
            migrationBuilder.DropTable(name: "baseline_registry");
        }
    }
}
