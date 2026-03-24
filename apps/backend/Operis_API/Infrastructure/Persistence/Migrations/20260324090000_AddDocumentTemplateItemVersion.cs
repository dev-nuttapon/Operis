using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Operis_API.Infrastructure.Persistence;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(OperisDbContext))]
    [Migration("20260324090000_AddDocumentTemplateItemVersion")]
    public partial class AddDocumentTemplateItemVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "document_version_id",
                table: "document_template_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_template_items_document_version_id",
                table: "document_template_items",
                column: "document_version_id");

            migrationBuilder.AddForeignKey(
                name: "FK_document_template_items_document_versions_document_version_id",
                table: "document_template_items",
                column: "document_version_id",
                principalTable: "document_versions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_template_items_document_versions_document_version_id",
                table: "document_template_items");

            migrationBuilder.DropIndex(
                name: "IX_document_template_items_document_version_id",
                table: "document_template_items");

            migrationBuilder.DropColumn(
                name: "document_version_id",
                table: "document_template_items");
        }
    }
}
