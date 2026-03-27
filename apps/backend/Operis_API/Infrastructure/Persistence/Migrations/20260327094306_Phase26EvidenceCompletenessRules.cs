using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase26EvidenceCompletenessRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "evidence_rule_results",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    scope_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    scope_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    process_area = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    evaluated_rule_count = table.Column<int>(type: "integer", nullable: false),
                    missing_item_count = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    superseded_by_result_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evidence_rule_results", x => x.id);
                    table.ForeignKey(
                        name: "FK_evidence_rule_results_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "evidence_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    rule_code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    process_area = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    artifact_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    expression_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evidence_rules", x => x.id);
                    table.ForeignKey(
                        name: "FK_evidence_rules_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "evidence_missing_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    result_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    process_area = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    artifact_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    reason_code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    module = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    route = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    scope = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    entity_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    metadata = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    detected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evidence_missing_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_evidence_missing_items_evidence_rule_results_result_id",
                        column: x => x.result_id,
                        principalTable: "evidence_rule_results",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_evidence_missing_items_evidence_rules_rule_id",
                        column: x => x.rule_id,
                        principalTable: "evidence_rules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_evidence_missing_items_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_evidence_missing_items_project_id_process_area_reason_code",
                table: "evidence_missing_items",
                columns: new[] { "project_id", "process_area", "reason_code" });

            migrationBuilder.CreateIndex(
                name: "IX_evidence_missing_items_result_id_process_area",
                table: "evidence_missing_items",
                columns: new[] { "result_id", "process_area" });

            migrationBuilder.CreateIndex(
                name: "IX_evidence_missing_items_rule_id",
                table: "evidence_missing_items",
                column: "rule_id");

            migrationBuilder.CreateIndex(
                name: "IX_evidence_rule_results_completed_at",
                table: "evidence_rule_results",
                column: "completed_at");

            migrationBuilder.CreateIndex(
                name: "IX_evidence_rule_results_project_id_process_area_status",
                table: "evidence_rule_results",
                columns: new[] { "project_id", "process_area", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_evidence_rule_results_scope_type_scope_ref_status",
                table: "evidence_rule_results",
                columns: new[] { "scope_type", "scope_ref", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_evidence_rules_project_id",
                table: "evidence_rules",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_evidence_rules_rule_code",
                table: "evidence_rules",
                column: "rule_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_evidence_rules_status_process_area_artifact_type",
                table: "evidence_rules",
                columns: new[] { "status", "process_area", "artifact_type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "evidence_missing_items");

            migrationBuilder.DropTable(
                name: "evidence_rule_results");

            migrationBuilder.DropTable(
                name: "evidence_rules");
        }
    }
}
