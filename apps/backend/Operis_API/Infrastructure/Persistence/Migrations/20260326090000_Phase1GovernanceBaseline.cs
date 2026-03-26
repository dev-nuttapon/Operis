using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    public partial class Phase1GovernanceBaseline : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "process_assets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    category = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    owner_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    effective_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    effective_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    current_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_process_assets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qa_checklists",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    scope = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    owner_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    items_json = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_qa_checklists", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "process_asset_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    process_asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    content_ref = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    change_summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    approved_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_process_asset_versions", x => x.id);
                    table.ForeignKey(
                        name: "FK_process_asset_versions_process_assets_process_asset_id",
                        column: x => x.process_asset_id,
                        principalTable: "process_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    scope_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    lifecycle_model = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    target_end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    owner_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    milestones_json = table.Column<string>(type: "jsonb", nullable: false),
                    roles_json = table.Column<string>(type: "jsonb", nullable: false),
                    risk_approach = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    quality_approach = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    approval_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    approved_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_plans", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_plans_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stakeholders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    role_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    influence_level = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    contact_channel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stakeholders", x => x.id);
                    table.ForeignKey(
                        name: "FK_stakeholders_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tailoring_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requester_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    requested_change = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    impact_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    approver_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    impacted_process_asset_id = table.Column<Guid>(type: "uuid", nullable: true),
                    approval_rationale = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tailoring_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_tailoring_records_process_assets_impacted_process_asset_id",
                        column: x => x.impacted_process_asset_id,
                        principalTable: "process_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tailoring_records_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_process_assets_code", table: "process_assets", column: "code", unique: true);
            migrationBuilder.CreateIndex(name: "IX_process_assets_category_status", table: "process_assets", columns: new[] { "category", "status" });
            migrationBuilder.CreateIndex(name: "IX_process_assets_owner_user_id", table: "process_assets", column: "owner_user_id");
            migrationBuilder.CreateIndex(name: "IX_process_asset_versions_process_asset_id_version_number", table: "process_asset_versions", columns: new[] { "process_asset_id", "version_number" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_process_asset_versions_process_asset_id_status", table: "process_asset_versions", columns: new[] { "process_asset_id", "status" });
            migrationBuilder.CreateIndex(name: "IX_qa_checklists_code", table: "qa_checklists", column: "code", unique: true);
            migrationBuilder.CreateIndex(name: "IX_qa_checklists_scope_status", table: "qa_checklists", columns: new[] { "scope", "status" });
            migrationBuilder.CreateIndex(name: "IX_project_plans_project_id_status", table: "project_plans", columns: new[] { "project_id", "status" });
            migrationBuilder.CreateIndex(name: "IX_project_plans_status_owner_user_id", table: "project_plans", columns: new[] { "status", "owner_user_id" });
            migrationBuilder.CreateIndex(name: "IX_stakeholders_project_id_status", table: "stakeholders", columns: new[] { "project_id", "status" });
            migrationBuilder.CreateIndex(name: "IX_tailoring_records_project_id_status", table: "tailoring_records", columns: new[] { "project_id", "status" });
            migrationBuilder.CreateIndex(name: "IX_tailoring_records_requester_user_id_status", table: "tailoring_records", columns: new[] { "requester_user_id", "status" });
            migrationBuilder.CreateIndex(name: "IX_tailoring_records_impacted_process_asset_id", table: "tailoring_records", column: "impacted_process_asset_id");

            migrationBuilder.Sql(
                """
                INSERT INTO app_roles (id, name, keycloak_role_name, description, display_order, created_at)
                VALUES
                    ('44444444-0000-0000-0000-000000000001', 'Compliance Admin', 'operis:compliance_admin', 'Approves and manages process governance assets.', 50, NOW()),
                    ('44444444-0000-0000-0000-000000000002', 'Project Manager', 'operis:pm', 'Owns project plans and tailoring submissions.', 51, NOW()),
                    ('44444444-0000-0000-0000-000000000003', 'Business Analyst', 'operis:ba', 'Reads governance baselines and stakeholder coverage.', 52, NOW()),
                    ('44444444-0000-0000-0000-000000000004', 'Quality Analyst', 'operis:qa', 'Reviews QA checklists and governance readiness.', 53, NOW()),
                    ('44444444-0000-0000-0000-000000000005', 'Approver', 'operis:approver', 'Approves project plan and tailoring transitions.', 54, NOW())
                ON CONFLICT DO NOTHING;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM app_roles
                WHERE keycloak_role_name IN ('operis:compliance_admin', 'operis:pm', 'operis:ba', 'operis:qa', 'operis:approver');
                """);

            migrationBuilder.DropTable(name: "tailoring_records");
            migrationBuilder.DropTable(name: "stakeholders");
            migrationBuilder.DropTable(name: "project_plans");
            migrationBuilder.DropTable(name: "process_asset_versions");
            migrationBuilder.DropTable(name: "qa_checklists");
            migrationBuilder.DropTable(name: "process_assets");
        }
    }
}
