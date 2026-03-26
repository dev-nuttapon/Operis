using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    public partial class Phase9MetricsQualityGates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "metric_definitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    metric_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    owner_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    target_value = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    threshold_value = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metric_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "quality_gate_results",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gate_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    evaluated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    override_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    evaluated_by_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    overridden_by_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quality_gate_results", x => x.id);
                    table.ForeignKey(
                        name: "FK_quality_gate_results_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "metric_collection_schedules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    metric_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    collection_frequency = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    collector_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    next_run_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metric_collection_schedules", x => x.id);
                    table.ForeignKey(
                        name: "FK_metric_collection_schedules_metric_definitions_metric_definition_id",
                        column: x => x.metric_definition_id,
                        principalTable: "metric_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "metric_results",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    metric_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quality_gate_result_id = table.Column<Guid>(type: "uuid", nullable: true),
                    measured_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    measured_value = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    source_ref = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metric_results", x => x.id);
                    table.ForeignKey(
                        name: "FK_metric_results_metric_definitions_metric_definition_id",
                        column: x => x.metric_definition_id,
                        principalTable: "metric_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_metric_results_quality_gate_results_quality_gate_result_id",
                        column: x => x.quality_gate_result_id,
                        principalTable: "quality_gate_results",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_metric_collection_schedules_metric_definition_id_status",
                table: "metric_collection_schedules",
                columns: new[] { "metric_definition_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_metric_collection_schedules_next_run_at",
                table: "metric_collection_schedules",
                column: "next_run_at");

            migrationBuilder.CreateIndex(
                name: "IX_metric_definitions_code",
                table: "metric_definitions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_metric_definitions_metric_type_status",
                table: "metric_definitions",
                columns: new[] { "metric_type", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_metric_definitions_owner_user_id",
                table: "metric_definitions",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_metric_results_metric_definition_id_measured_at",
                table: "metric_results",
                columns: new[] { "metric_definition_id", "measured_at" });

            migrationBuilder.CreateIndex(
                name: "IX_metric_results_quality_gate_result_id",
                table: "metric_results",
                column: "quality_gate_result_id");

            migrationBuilder.CreateIndex(
                name: "IX_metric_results_status",
                table: "metric_results",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_quality_gate_results_project_id_gate_type_evaluated_at",
                table: "quality_gate_results",
                columns: new[] { "project_id", "gate_type", "evaluated_at" });

            migrationBuilder.CreateIndex(
                name: "IX_quality_gate_results_result_evaluated_at",
                table: "quality_gate_results",
                columns: new[] { "result", "evaluated_at" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "metric_collection_schedules");
            migrationBuilder.DropTable(name: "metric_results");
            migrationBuilder.DropTable(name: "metric_definitions");
            migrationBuilder.DropTable(name: "quality_gate_results");
        }
    }
}
