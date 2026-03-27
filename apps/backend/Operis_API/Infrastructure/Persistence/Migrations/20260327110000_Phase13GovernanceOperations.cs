using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    public partial class Phase13GovernanceOperations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "approval_evidence_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    approver_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_approval_evidence_logs", x => x.id));

            migrationBuilder.CreateTable(
                name: "raci_maps",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    process_code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    role_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    responsibility_type = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_raci_maps", x => x.id));

            migrationBuilder.CreateTable(
                name: "retention_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    applies_to = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    retention_period_days = table.Column<int>(type: "integer", nullable: false),
                    archive_rule = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_retention_policies", x => x.id));

            migrationBuilder.CreateTable(
                name: "sla_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    scope_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    scope_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    target_duration_hours = table.Column<int>(type: "integer", nullable: false),
                    escalation_policy_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_sla_rules", x => x.id));

            migrationBuilder.CreateTable(
                name: "workflow_override_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    requested_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    approved_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_workflow_override_logs", x => x.id));

            migrationBuilder.CreateIndex(
                name: "IX_approval_evidence_logs_approver_user_id_approved_at",
                table: "approval_evidence_logs",
                columns: new[] { "approver_user_id", "approved_at" });

            migrationBuilder.CreateIndex(
                name: "IX_approval_evidence_logs_entity_type_outcome_approved_at",
                table: "approval_evidence_logs",
                columns: new[] { "entity_type", "outcome", "approved_at" });

            migrationBuilder.CreateIndex(
                name: "IX_raci_maps_process_code_role_name_responsibility_type",
                table: "raci_maps",
                columns: new[] { "process_code", "role_name", "responsibility_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_raci_maps_process_code_status",
                table: "raci_maps",
                columns: new[] { "process_code", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_retention_policies_applies_to_status",
                table: "retention_policies",
                columns: new[] { "applies_to", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_retention_policies_policy_code",
                table: "retention_policies",
                column: "policy_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sla_rules_scope_type_status",
                table: "sla_rules",
                columns: new[] { "scope_type", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_override_logs_entity_type_occurred_at",
                table: "workflow_override_logs",
                columns: new[] { "entity_type", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_override_logs_requested_by_occurred_at",
                table: "workflow_override_logs",
                columns: new[] { "requested_by", "occurred_at" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "approval_evidence_logs");
            migrationBuilder.DropTable(name: "raci_maps");
            migrationBuilder.DropTable(name: "retention_policies");
            migrationBuilder.DropTable(name: "sla_rules");
            migrationBuilder.DropTable(name: "workflow_override_logs");
        }
    }
}
