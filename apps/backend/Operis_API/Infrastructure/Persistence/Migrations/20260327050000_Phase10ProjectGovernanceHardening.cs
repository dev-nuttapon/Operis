using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    public partial class Phase10ProjectGovernanceHardening : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "project_id",
                table: "project_roles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "project_roles",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.Sql("UPDATE project_roles SET status = 'Active' WHERE status IS NULL;");

            migrationBuilder.CreateTable(
                name: "phase_approval_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    phase_code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    entry_criteria_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    required_evidence_refs_json = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    submitted_by = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    decision = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    decision_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    decided_by = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    decided_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    baseline_by = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    baselined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phase_approval_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_phase_approval_requests_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_phase_approval_requests_users_baseline_by",
                        column: x => x.baseline_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_phase_approval_requests_users_decided_by",
                        column: x => x.decided_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_phase_approval_requests_users_submitted_by",
                        column: x => x.submitted_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_project_roles_project_id",
                table: "project_roles",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_roles_project_id_code",
                table: "project_roles",
                columns: new[] { "project_id", "code" },
                unique: true,
                filter: "\"deleted_at\" IS NULL AND \"code\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_project_roles_project_id_name",
                table: "project_roles",
                columns: new[] { "project_id", "name" },
                filter: "\"deleted_at\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_project_roles_project_id_status_display_order_name",
                table: "project_roles",
                columns: new[] { "project_id", "status", "display_order", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_phase_approval_requests_baseline_by",
                table: "phase_approval_requests",
                column: "baseline_by");

            migrationBuilder.CreateIndex(
                name: "IX_phase_approval_requests_decided_by",
                table: "phase_approval_requests",
                column: "decided_by");

            migrationBuilder.CreateIndex(
                name: "IX_phase_approval_requests_project_id_phase_code_created_at",
                table: "phase_approval_requests",
                columns: new[] { "project_id", "phase_code", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_phase_approval_requests_project_id_status",
                table: "phase_approval_requests",
                columns: new[] { "project_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_phase_approval_requests_submitted_by",
                table: "phase_approval_requests",
                column: "submitted_by");

            migrationBuilder.AddForeignKey(
                name: "FK_project_roles_projects_project_id",
                table: "project_roles",
                column: "project_id",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_project_roles_projects_project_id",
                table: "project_roles");

            migrationBuilder.DropTable(
                name: "phase_approval_requests");

            migrationBuilder.DropIndex(
                name: "IX_project_roles_project_id",
                table: "project_roles");

            migrationBuilder.DropIndex(
                name: "IX_project_roles_project_id_code",
                table: "project_roles");

            migrationBuilder.DropIndex(
                name: "IX_project_roles_project_id_name",
                table: "project_roles");

            migrationBuilder.DropIndex(
                name: "IX_project_roles_project_id_status_display_order_name",
                table: "project_roles");

            migrationBuilder.DropColumn(
                name: "project_id",
                table: "project_roles");

            migrationBuilder.DropColumn(
                name: "status",
                table: "project_roles");
        }
    }
}
