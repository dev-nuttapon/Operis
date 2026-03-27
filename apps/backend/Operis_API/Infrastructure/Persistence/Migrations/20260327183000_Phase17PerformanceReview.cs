using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations;

public partial class Phase17PerformanceReview : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "metric_reviews",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                project_id = table.Column<Guid>(type: "uuid", nullable: false),
                review_period = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                reviewed_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                open_action_count = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_metric_reviews", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "trend_reports",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                project_id = table.Column<Guid>(type: "uuid", nullable: false),
                metric_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                period_from = table.Column<DateOnly>(type: "date", nullable: false),
                period_to = table.Column<DateOnly>(type: "date", nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                report_ref = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                trend_direction = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                variance = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                recommended_action = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_trend_reports", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_metric_reviews_project_id_status",
            table: "metric_reviews",
            columns: new[] { "project_id", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_trend_reports_project_id_metric_definition_id",
            table: "trend_reports",
            columns: new[] { "project_id", "metric_definition_id" });

        migrationBuilder.CreateIndex(
            name: "IX_trend_reports_status",
            table: "trend_reports",
            column: "status");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "metric_reviews");

        migrationBuilder.DropTable(
            name: "trend_reports");
    }
}
