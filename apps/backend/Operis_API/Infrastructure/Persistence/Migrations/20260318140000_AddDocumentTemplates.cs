using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Operis_API.Infrastructure.Persistence;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(OperisDbContext))]
    [Migration("20260318140000_AddDocumentTemplates")]
    public partial class AddDocumentTemplates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        migrationBuilder.CreateTable(
            name: "document_templates",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                created_by_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                deleted_by_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                deleted_reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_document_templates", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "document_template_items",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                template_id = table.Column<Guid>(type: "uuid", nullable: false),
                document_id = table.Column<Guid>(type: "uuid", nullable: false),
                display_order = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_document_template_items", x => x.id);
                table.ForeignKey(
                    name: "FK_document_template_items_document_templates_template_id",
                    column: x => x.template_id,
                    principalTable: "document_templates",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_document_template_items_documents_document_id",
                    column: x => x.document_id,
                    principalTable: "documents",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_document_template_items_document_id",
            table: "document_template_items",
            column: "document_id");

        migrationBuilder.CreateIndex(
            name: "IX_document_template_items_template_id",
            table: "document_template_items",
            column: "template_id");

        migrationBuilder.CreateIndex(
            name: "IX_document_template_items_template_id_document_id",
            table: "document_template_items",
            columns: new[] { "template_id", "document_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_document_templates_is_deleted",
            table: "document_templates",
            column: "is_deleted");

        migrationBuilder.CreateIndex(
            name: "IX_document_templates_is_deleted_created_at",
            table: "document_templates",
            columns: new[] { "is_deleted", "created_at" });

        migrationBuilder.CreateIndex(
            name: "IX_document_templates_name",
            table: "document_templates",
            column: "name");
    }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_template_items");

            migrationBuilder.DropTable(
                name: "document_templates");
        }
    }
}
