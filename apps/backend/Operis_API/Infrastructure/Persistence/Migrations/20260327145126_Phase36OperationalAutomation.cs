using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase36OperationalAutomation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "automation_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    job_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    scope_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    schedule_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    latest_run_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    latest_run_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    failure_summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_automation_jobs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "automation_job_runs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    triggered_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    trigger_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    queued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    error_summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    remediation_path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_automation_job_runs", x => x.id);
                    table.ForeignKey(
                        name: "FK_automation_job_runs_automation_jobs_job_id",
                        column: x => x.job_id,
                        principalTable: "automation_jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "automation_job_evidence_refs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    route = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    evidence_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_automation_job_evidence_refs", x => x.id);
                    table.ForeignKey(
                        name: "FK_automation_job_evidence_refs_automation_job_runs_job_run_id",
                        column: x => x.job_run_id,
                        principalTable: "automation_job_runs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_automation_job_evidence_refs_job_run_id",
                table: "automation_job_evidence_refs",
                column: "job_run_id");

            migrationBuilder.CreateIndex(
                name: "IX_automation_job_runs_job_id_queued_at",
                table: "automation_job_runs",
                columns: new[] { "job_id", "queued_at" });

            migrationBuilder.CreateIndex(
                name: "IX_automation_job_runs_status_queued_at",
                table: "automation_job_runs",
                columns: new[] { "status", "queued_at" });

            migrationBuilder.CreateIndex(
                name: "IX_automation_jobs_job_name",
                table: "automation_jobs",
                column: "job_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_automation_jobs_job_type_status",
                table: "automation_jobs",
                columns: new[] { "job_type", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "automation_job_evidence_refs");

            migrationBuilder.DropTable(
                name: "automation_job_runs");

            migrationBuilder.DropTable(
                name: "automation_jobs");
        }
    }
}
