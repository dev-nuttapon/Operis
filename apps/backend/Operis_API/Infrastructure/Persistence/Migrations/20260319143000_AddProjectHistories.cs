using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Operis_API.Infrastructure.Persistence;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(OperisDbContext))]
    [Migration("20260319143000_AddProjectHistories")]
    public partial class AddProjectHistories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "project_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    summary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    actor_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    actor_email = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    actor_display_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    before_json = table.Column<string>(type: "text", nullable: true),
                    after_json = table.Column<string>(type: "text", nullable: true),
                    metadata_json = table.Column<string>(type: "text", nullable: true),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_histories", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_project_histories_project_id",
                table: "project_histories",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_histories_event_type",
                table: "project_histories",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "IX_project_histories_occurred_at",
                table: "project_histories",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "IX_project_histories_project_id_occurred_at",
                table: "project_histories",
                columns: new[] { "project_id", "occurred_at" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_histories");
        }
    }
}
