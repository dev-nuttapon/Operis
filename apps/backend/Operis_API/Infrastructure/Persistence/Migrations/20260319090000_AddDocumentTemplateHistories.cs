using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Operis_API.Infrastructure.Persistence;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(OperisDbContext))]
    [Migration("20260319090000_AddDocumentTemplateHistories")]
    public partial class AddDocumentTemplateHistories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "document_template_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_document_template_histories", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_template_histories_template_id",
                table: "document_template_histories",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_template_histories_event_type",
                table: "document_template_histories",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "IX_document_template_histories_occurred_at",
                table: "document_template_histories",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "IX_document_template_histories_template_id_occurred_at",
                table: "document_template_histories",
                columns: new[] { "template_id", "occurred_at" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_template_histories");
        }
    }
}
