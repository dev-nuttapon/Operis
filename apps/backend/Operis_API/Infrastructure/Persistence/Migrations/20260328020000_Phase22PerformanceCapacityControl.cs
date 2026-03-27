using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations;

public partial class Phase22PerformanceCapacityControl : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "capacity_reviews",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                scope_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                review_period = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                reviewed_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                action_count = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_capacity_reviews", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "performance_baselines",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                scope_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                scope_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                metric_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                target_value = table.Column<decimal>(type: "numeric", nullable: false),
                threshold_value = table.Column<decimal>(type: "numeric", nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_performance_baselines", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "performance_gate_results",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                scope_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                evaluated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                override_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                evidence_ref = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                evaluated_by_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                overridden_by_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_performance_gate_results", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "slow_operation_reviews",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                operation_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                operation_key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                observed_latency_ms = table.Column<decimal>(type: "numeric", nullable: false),
                frequency_per_hour = table.Column<decimal>(type: "numeric", nullable: true),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                owner_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                optimization_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_slow_operation_reviews", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_capacity_reviews_scope_ref_status",
            table: "capacity_reviews",
            columns: new[] { "scope_ref", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_capacity_reviews_reviewed_by",
            table: "capacity_reviews",
            column: "reviewed_by");

        migrationBuilder.CreateIndex(
            name: "IX_performance_baselines_metric_name",
            table: "performance_baselines",
            column: "metric_name");

        migrationBuilder.CreateIndex(
            name: "IX_performance_baselines_scope_type_metric_name_status",
            table: "performance_baselines",
            columns: new[] { "scope_type", "metric_name", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_performance_gate_results_scope_ref_evaluated_at",
            table: "performance_gate_results",
            columns: new[] { "scope_ref", "evaluated_at" });

        migrationBuilder.CreateIndex(
            name: "IX_performance_gate_results_result_evaluated_at",
            table: "performance_gate_results",
            columns: new[] { "result", "evaluated_at" });

        migrationBuilder.CreateIndex(
            name: "IX_slow_operation_reviews_operation_type_status",
            table: "slow_operation_reviews",
            columns: new[] { "operation_type", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_slow_operation_reviews_owner_user_id_status",
            table: "slow_operation_reviews",
            columns: new[] { "owner_user_id", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_slow_operation_reviews_operation_key",
            table: "slow_operation_reviews",
            column: "operation_key");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "capacity_reviews");
        migrationBuilder.DropTable(name: "performance_baselines");
        migrationBuilder.DropTable(name: "performance_gate_results");
        migrationBuilder.DropTable(name: "slow_operation_reviews");
    }
}
