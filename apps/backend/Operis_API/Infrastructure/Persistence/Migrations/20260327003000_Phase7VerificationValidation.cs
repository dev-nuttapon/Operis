using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    public partial class Phase7VerificationValidation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "test_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    scope_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    owner_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    entry_criteria = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    exit_criteria = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    linked_requirement_ids_json = table.Column<string>(type: "jsonb", nullable: false),
                    approval_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    approved_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    baselined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_plans", x => x.id);
                    table.ForeignKey(
                        name: "FK_test_plans_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "uat_signoffs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    release_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    scope_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    submitted_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    approved_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    decision_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    evidence_refs_json = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_uat_signoffs", x => x.id);
                    table.ForeignKey(
                        name: "FK_uat_signoffs_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "test_cases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    test_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    preconditions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    steps_json = table.Column<string>(type: "jsonb", nullable: false),
                    expected_result = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    requirement_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_cases", x => x.id);
                    table.ForeignKey(
                        name: "FK_test_cases_requirements_requirement_id",
                        column: x => x.requirement_id,
                        principalTable: "requirements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_test_cases_test_plans_test_plan_id",
                        column: x => x.test_plan_id,
                        principalTable: "test_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "test_executions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    test_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    executed_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    executed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    evidence_ref = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    is_sensitive_evidence = table.Column<bool>(type: "boolean", nullable: false),
                    evidence_classification = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_executions", x => x.id);
                    table.ForeignKey(
                        name: "FK_test_executions_test_cases_test_case_id",
                        column: x => x.test_case_id,
                        principalTable: "test_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_test_plans_project_id_code", table: "test_plans", columns: new[] { "project_id", "code" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_test_plans_project_id_status", table: "test_plans", columns: new[] { "project_id", "status" });
            migrationBuilder.CreateIndex(name: "IX_test_plans_owner_user_id", table: "test_plans", column: "owner_user_id");

            migrationBuilder.CreateIndex(name: "IX_test_cases_test_plan_id_code", table: "test_cases", columns: new[] { "test_plan_id", "code" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_test_cases_test_plan_id_status", table: "test_cases", columns: new[] { "test_plan_id", "status" });
            migrationBuilder.CreateIndex(name: "IX_test_cases_requirement_id", table: "test_cases", column: "requirement_id");

            migrationBuilder.CreateIndex(name: "IX_test_executions_test_case_id_executed_at", table: "test_executions", columns: new[] { "test_case_id", "executed_at" });
            migrationBuilder.CreateIndex(name: "IX_test_executions_result_executed_at", table: "test_executions", columns: new[] { "result", "executed_at" });
            migrationBuilder.CreateIndex(name: "IX_test_executions_executed_by", table: "test_executions", column: "executed_by");
            migrationBuilder.CreateIndex(name: "IX_test_executions_is_sensitive_evidence", table: "test_executions", column: "is_sensitive_evidence");

            migrationBuilder.CreateIndex(name: "IX_uat_signoffs_project_id_status", table: "uat_signoffs", columns: new[] { "project_id", "status" });
            migrationBuilder.CreateIndex(name: "IX_uat_signoffs_release_id", table: "uat_signoffs", column: "release_id");
            migrationBuilder.CreateIndex(name: "IX_uat_signoffs_submitted_by", table: "uat_signoffs", column: "submitted_by");

            migrationBuilder.Sql(
                """
                INSERT INTO app_roles (id, name, keycloak_role_name, description, display_order, created_at)
                VALUES
                    ('99999999-0000-0000-0000-000000000001', 'Verification Manager', 'operis:verification_manager', 'Manages test plans, cases, executions, and evidence export.', 93, NOW()),
                    ('99999999-0000-0000-0000-000000000002', 'Verification Approver', 'operis:verification_approver', 'Approves verification workflows and UAT outcomes.', 94, NOW()),
                    ('99999999-0000-0000-0000-000000000003', 'Verification Viewer', 'operis:verification_viewer', 'Reads verification and validation records.', 95, NOW()),
                    ('99999999-0000-0000-0000-000000000004', 'UAT Submitter', 'operis:uat_submitter', 'Submits UAT sign-off records for approval.', 96, NOW())
                ON CONFLICT DO NOTHING;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM app_roles
                WHERE keycloak_role_name IN ('operis:verification_manager', 'operis:verification_approver', 'operis:verification_viewer', 'operis:uat_submitter');
                """);

            migrationBuilder.DropTable(name: "test_executions");
            migrationBuilder.DropTable(name: "uat_signoffs");
            migrationBuilder.DropTable(name: "test_cases");
            migrationBuilder.DropTable(name: "test_plans");
        }
    }
}
