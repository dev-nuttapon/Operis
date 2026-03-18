using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Operis_API.Infrastructure.Persistence;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(OperisDbContext))]
    [Migration("20260318113000_AddBusinessAuditEvents")]
    public partial class AddBusinessAuditEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "business_audit_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    module = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    event_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    summary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    actor_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    actor_email = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    actor_display_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_business_audit_events", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_business_audit_events_entity_id",
                table: "business_audit_events",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_business_audit_events_entity_type",
                table: "business_audit_events",
                column: "entity_type");

            migrationBuilder.CreateIndex(
                name: "IX_business_audit_events_event_type",
                table: "business_audit_events",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "IX_business_audit_events_module",
                table: "business_audit_events",
                column: "module");

            migrationBuilder.CreateIndex(
                name: "IX_business_audit_events_entity_type_entity_id_occurred_at",
                table: "business_audit_events",
                columns: new[] { "entity_type", "entity_id", "occurred_at" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "business_audit_events");
        }
    }
}
