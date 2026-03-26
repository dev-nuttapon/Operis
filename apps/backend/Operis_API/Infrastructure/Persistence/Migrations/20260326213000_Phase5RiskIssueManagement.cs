using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    public partial class Phase5RiskIssueManagement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "issues",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    owner_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    root_issue = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    dependencies = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    resolution_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    is_sensitive = table.Column<bool>(type: "boolean", nullable: false),
                    sensitive_context = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_issues", x => x.id);
                    table.ForeignKey(
                        name: "FK_issues_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "risks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    probability = table.Column<int>(type: "integer", nullable: false),
                    impact = table.Column<int>(type: "integer", nullable: false),
                    owner_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    mitigation_plan = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    cause = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    effect = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    contingency_plan = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    next_review_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_risks", x => x.id);
                    table.ForeignKey(
                        name: "FK_risks_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "issue_actions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    issue_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action_description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    assigned_to = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    verification_note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_issue_actions", x => x.id);
                    table.ForeignKey(
                        name: "FK_issue_actions_issues_issue_id",
                        column: x => x.issue_id,
                        principalTable: "issues",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "risk_reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    risk_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewed_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    decision = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_risk_reviews", x => x.id);
                    table.ForeignKey(
                        name: "FK_risk_reviews_risks_risk_id",
                        column: x => x.risk_id,
                        principalTable: "risks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_issues_project_id_code", table: "issues", columns: new[] { "project_id", "code" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_issues_project_id_status_severity", table: "issues", columns: new[] { "project_id", "status", "severity" });
            migrationBuilder.CreateIndex(name: "IX_issues_owner_user_id", table: "issues", column: "owner_user_id");
            migrationBuilder.CreateIndex(name: "IX_issues_due_date", table: "issues", column: "due_date");
            migrationBuilder.CreateIndex(name: "IX_issues_is_sensitive", table: "issues", column: "is_sensitive");

            migrationBuilder.CreateIndex(name: "IX_risks_project_id_code", table: "risks", columns: new[] { "project_id", "code" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_risks_project_id_status_next_review_at", table: "risks", columns: new[] { "project_id", "status", "next_review_at" });
            migrationBuilder.CreateIndex(name: "IX_risks_owner_user_id", table: "risks", column: "owner_user_id");

            migrationBuilder.CreateIndex(name: "IX_issue_actions_issue_id_status", table: "issue_actions", columns: new[] { "issue_id", "status" });
            migrationBuilder.CreateIndex(name: "IX_issue_actions_assigned_to_status", table: "issue_actions", columns: new[] { "assigned_to", "status" });

            migrationBuilder.CreateIndex(name: "IX_risk_reviews_risk_id_reviewed_at", table: "risk_reviews", columns: new[] { "risk_id", "reviewed_at" });

            migrationBuilder.Sql(
                """
                INSERT INTO app_roles (id, name, keycloak_role_name, description, display_order, created_at)
                VALUES
                    ('77777777-0000-0000-0000-000000000001', 'Risk Manager', 'operis:risk_manager', 'Manages risk and issue workflows including sensitive issue handling.', 80, NOW()),
                    ('77777777-0000-0000-0000-000000000002', 'Risk Viewer', 'operis:risk_viewer', 'Reads non-sensitive risk and issue registers.', 81, NOW())
                ON CONFLICT DO NOTHING;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM app_roles
                WHERE keycloak_role_name IN ('operis:risk_manager', 'operis:risk_viewer');
                """);

            migrationBuilder.DropTable(name: "issue_actions");
            migrationBuilder.DropTable(name: "risk_reviews");
            migrationBuilder.DropTable(name: "issues");
            migrationBuilder.DropTable(name: "risks");
        }
    }
}
