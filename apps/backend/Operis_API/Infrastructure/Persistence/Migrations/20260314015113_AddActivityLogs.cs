using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "activity_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    module = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    action = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    actor_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    actor_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    actor_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    actor_display_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    department_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    request_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    trace_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    http_method = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    request_path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    status_code = table.Column<int>(type: "integer", nullable: true),
                    error_code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    reason = table.Column<string>(type: "text", nullable: true),
                    source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    before_json = table.Column<string>(type: "jsonb", nullable: true),
                    after_json = table.Column<string>(type: "jsonb", nullable: true),
                    changes_json = table.Column<string>(type: "jsonb", nullable: true),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true),
                    is_sensitive = table.Column<bool>(type: "boolean", nullable: false),
                    retention_class = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_logs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_action_occurred_at",
                table: "activity_logs",
                columns: new[] { "action", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_actor_email_occurred_at",
                table: "activity_logs",
                columns: new[] { "actor_email", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_actor_user_id_occurred_at",
                table: "activity_logs",
                columns: new[] { "actor_user_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_department_id_occurred_at",
                table: "activity_logs",
                columns: new[] { "department_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_entity_type_entity_id",
                table: "activity_logs",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_module_occurred_at",
                table: "activity_logs",
                columns: new[] { "module", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_occurred_at",
                table: "activity_logs",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_request_id",
                table: "activity_logs",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_status_occurred_at",
                table: "activity_logs",
                columns: new[] { "status", "occurred_at" });

            migrationBuilder.Sql(
                """
                INSERT INTO activity_logs (
                    id, occurred_at, module, action, entity_type, entity_id, actor_type, actor_user_id, actor_email,
                    actor_display_name, department_id, tenant_id, request_id, trace_id, correlation_id, http_method,
                    request_path, ip_address, user_agent, status, status_code, error_code, error_message, reason,
                    source, before_json, after_json, changes_json, metadata_json, is_sensitive, retention_class, created_at
                )
                SELECT
                    id, occurred_at, module, action, entity_type, entity_id, actor_type, actor_user_id, actor_email,
                    actor_display_name, department_id, tenant_id, request_id, trace_id, correlation_id, http_method,
                    request_path, ip_address, user_agent, status, status_code, error_code, error_message, reason,
                    source, before_json, after_json, changes_json, metadata_json, is_sensitive, retention_class, created_at
                FROM audit_logs;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_logs");
        }
    }
}
