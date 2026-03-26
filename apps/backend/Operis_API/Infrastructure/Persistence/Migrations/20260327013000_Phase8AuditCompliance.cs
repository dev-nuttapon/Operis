using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    public partial class Phase8AuditCompliance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    scope = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    criteria = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    planned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    owner_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_plans", x => x.id);
                    table.ForeignKey(
                        name: "FK_audit_plans_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "evidence_exports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    scope_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    scope_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    requested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    output_ref = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    from_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    to_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    included_artifact_types_json = table.Column<string>(type: "jsonb", nullable: false),
                    failure_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evidence_exports", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_findings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    owner_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    resolution_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_findings", x => x.id);
                    table.ForeignKey(
                        name: "FK_audit_findings_audit_plans_audit_plan_id",
                        column: x => x.audit_plan_id,
                        principalTable: "audit_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_findings_audit_plan_id_code",
                table: "audit_findings",
                columns: new[] { "audit_plan_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_findings_audit_plan_id_status",
                table: "audit_findings",
                columns: new[] { "audit_plan_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_findings_owner_user_id",
                table: "audit_findings",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_plans_owner_user_id",
                table: "audit_plans",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_plans_project_id_status_planned_at",
                table: "audit_plans",
                columns: new[] { "project_id", "status", "planned_at" });

            migrationBuilder.CreateIndex(
                name: "IX_evidence_exports_requested_by_requested_at",
                table: "evidence_exports",
                columns: new[] { "requested_by", "requested_at" });

            migrationBuilder.CreateIndex(
                name: "IX_evidence_exports_status_requested_at",
                table: "evidence_exports",
                columns: new[] { "status", "requested_at" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_findings");

            migrationBuilder.DropTable(
                name: "evidence_exports");

            migrationBuilder.DropTable(
                name: "audit_plans");
        }
    }
}
