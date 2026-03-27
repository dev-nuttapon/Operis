using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase25ComplianceDashboardCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliance_dashboard_preferences",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    default_project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    default_process_area = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    default_period_days = table.Column<int>(type: "integer", nullable: false),
                    default_show_only_at_risk = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliance_dashboard_preferences", x => x.id);
                    table.ForeignKey(
                        name: "FK_compliance_dashboard_preferences_projects_default_project_id",
                        column: x => x.default_project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "compliance_snapshots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    process_area = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    period_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    period_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    readiness_score = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    missing_artifact_count = table.Column<int>(type: "integer", nullable: false),
                    overdue_approval_count = table.Column<int>(type: "integer", nullable: false),
                    stale_baseline_count = table.Column<int>(type: "integer", nullable: false),
                    open_capa_count = table.Column<int>(type: "integer", nullable: false),
                    open_audit_finding_count = table.Column<int>(type: "integer", nullable: false),
                    open_security_item_count = table.Column<int>(type: "integer", nullable: false),
                    details_json = table.Column<string>(type: "jsonb", nullable: false),
                    generated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    generated_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    superseded_by_snapshot_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliance_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "FK_compliance_snapshots_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliance_dashboard_preferences_default_project_id",
                table: "compliance_dashboard_preferences",
                column: "default_project_id");

            migrationBuilder.CreateIndex(
                name: "IX_compliance_dashboard_preferences_user_id",
                table: "compliance_dashboard_preferences",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliance_snapshots_generated_at",
                table: "compliance_snapshots",
                column: "generated_at");

            migrationBuilder.CreateIndex(
                name: "IX_compliance_snapshots_period_start_period_end",
                table: "compliance_snapshots",
                columns: new[] { "period_start", "period_end" });

            migrationBuilder.CreateIndex(
                name: "IX_compliance_snapshots_project_id_process_area_status",
                table: "compliance_snapshots",
                columns: new[] { "project_id", "process_area", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliance_dashboard_preferences");

            migrationBuilder.DropTable(
                name: "compliance_snapshots");
        }
    }
}
