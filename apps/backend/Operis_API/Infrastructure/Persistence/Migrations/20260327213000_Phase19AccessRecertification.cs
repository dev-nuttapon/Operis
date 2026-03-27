using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations;

public partial class Phase19AccessRecertification : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "access_recertification_schedules",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                scope_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                scope_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                planned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                review_owner_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                subject_users_json = table.Column<string>(type: "jsonb", nullable: true),
                exception_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_access_recertification_schedules", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "access_recertification_decisions",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                subject_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                decision = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                decided_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                decided_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_access_recertification_decisions", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_access_recertification_schedules_review_owner_user_id",
            table: "access_recertification_schedules",
            column: "review_owner_user_id");

        migrationBuilder.CreateIndex(
            name: "IX_access_recertification_schedules_status_planned_at",
            table: "access_recertification_schedules",
            columns: new[] { "status", "planned_at" });

        migrationBuilder.CreateIndex(
            name: "IX_access_recertification_decisions_schedule_id_decision",
            table: "access_recertification_decisions",
            columns: new[] { "schedule_id", "decision" });

        migrationBuilder.CreateIndex(
            name: "IX_access_recertification_decisions_subject_user_id",
            table: "access_recertification_decisions",
            column: "subject_user_id");

        migrationBuilder.CreateIndex(
            name: "IX_access_recertification_decisions_schedule_id_subject_user_id",
            table: "access_recertification_decisions",
            columns: new[] { "schedule_id", "subject_user_id" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "access_recertification_decisions");

        migrationBuilder.DropTable(
            name: "access_recertification_schedules");
    }
}
