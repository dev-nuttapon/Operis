using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations;

public partial class Phase24CapaExecutionAndNotificationQueue : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "touchpoint",
            table: "secret_rotations",
            type: "character varying(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "custom");

        migrationBuilder.AddColumn<string>(
            name: "evidence_ref",
            table: "secret_rotations",
            type: "character varying(512)",
            maxLength: 512,
            nullable: true);

        migrationBuilder.CreateTable(
            name: "notification_queue",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                channel = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                target_ref = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                payload_ref = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                queued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                retry_count = table.Column<int>(type: "integer", nullable: false),
                last_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                last_retried_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_notification_queue", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "capa_records",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                source_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                source_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                owner_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                root_cause_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                verified_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                closed_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_capa_records", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "escalation_events",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                scope_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                scope_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                triggered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                trigger_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                escalated_to = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_escalation_events", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "capa_actions",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                capa_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                action_description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                assigned_to = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                due_date = table.Column<DateOnly>(type: "date", nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_capa_actions", x => x.id);
                table.ForeignKey(
                    name: "FK_capa_actions_capa_records_capa_record_id",
                    column: x => x.capa_record_id,
                    principalTable: "capa_records",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_secret_rotations_touchpoint_status",
            table: "secret_rotations",
            columns: new[] { "touchpoint", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_notification_queue_status_queued_at",
            table: "notification_queue",
            columns: new[] { "status", "queued_at" });

        migrationBuilder.CreateIndex(
            name: "IX_notification_queue_channel_status",
            table: "notification_queue",
            columns: new[] { "channel", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_capa_records_source_type_status",
            table: "capa_records",
            columns: new[] { "source_type", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_capa_records_owner_user_id_status",
            table: "capa_records",
            columns: new[] { "owner_user_id", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_capa_actions_capa_record_id_status",
            table: "capa_actions",
            columns: new[] { "capa_record_id", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_capa_actions_assigned_to_due_date",
            table: "capa_actions",
            columns: new[] { "assigned_to", "due_date" });

        migrationBuilder.CreateIndex(
            name: "IX_escalation_events_scope_type_status",
            table: "escalation_events",
            columns: new[] { "scope_type", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_escalation_events_escalated_to_triggered_at",
            table: "escalation_events",
            columns: new[] { "escalated_to", "triggered_at" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "capa_actions");
        migrationBuilder.DropTable(name: "escalation_events");
        migrationBuilder.DropTable(name: "notification_queue");
        migrationBuilder.DropTable(name: "capa_records");

        migrationBuilder.DropIndex(
            name: "IX_secret_rotations_touchpoint_status",
            table: "secret_rotations");

        migrationBuilder.DropColumn(
            name: "touchpoint",
            table: "secret_rotations");

        migrationBuilder.DropColumn(
            name: "evidence_ref",
            table: "secret_rotations");
    }
}
