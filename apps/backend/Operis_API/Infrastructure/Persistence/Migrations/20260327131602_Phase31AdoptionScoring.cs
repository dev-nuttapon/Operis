using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase31AdoptionScoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "adoption_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    rule_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    process_area = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    scope_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    threshold_percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adoption_rules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "adoption_anomalies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    adoption_rule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    process_area = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    summary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    detected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adoption_anomalies", x => x.id);
                    table.ForeignKey(
                        name: "FK_adoption_anomalies_adoption_rules_adoption_rule_id",
                        column: x => x.adoption_rule_id,
                        principalTable: "adoption_rules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_adoption_anomalies_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "adoption_scores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    adoption_rule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    process_area = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    score_percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    score_state = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    evidence_count = table.Column<int>(type: "integer", nullable: false),
                    expected_count = table.Column<int>(type: "integer", nullable: false),
                    calculated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adoption_scores", x => x.id);
                    table.ForeignKey(
                        name: "FK_adoption_scores_adoption_rules_adoption_rule_id",
                        column: x => x.adoption_rule_id,
                        principalTable: "adoption_rules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_adoption_scores_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_adoption_anomalies_adoption_rule_id",
                table: "adoption_anomalies",
                column: "adoption_rule_id");

            migrationBuilder.CreateIndex(
                name: "IX_adoption_anomalies_project_id_status_detected_at",
                table: "adoption_anomalies",
                columns: new[] { "project_id", "status", "detected_at" });

            migrationBuilder.CreateIndex(
                name: "IX_adoption_rules_process_area_status",
                table: "adoption_rules",
                columns: new[] { "process_area", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_adoption_rules_rule_code",
                table: "adoption_rules",
                column: "rule_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_adoption_scores_adoption_rule_id",
                table: "adoption_scores",
                column: "adoption_rule_id");

            migrationBuilder.CreateIndex(
                name: "IX_adoption_scores_project_id_adoption_rule_id_calculated_at",
                table: "adoption_scores",
                columns: new[] { "project_id", "adoption_rule_id", "calculated_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "adoption_anomalies");

            migrationBuilder.DropTable(
                name: "adoption_scores");

            migrationBuilder.DropTable(
                name: "adoption_rules");
        }
    }
}
