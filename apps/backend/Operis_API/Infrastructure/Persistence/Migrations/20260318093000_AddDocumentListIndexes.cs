using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Operis_API.Infrastructure.Persistence;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(OperisDbContext))]
    [Migration("20260318093000_AddDocumentListIndexes")]
    public partial class AddDocumentListIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_documents_is_deleted_uploaded_at",
                table: "documents",
                columns: new[] { "is_deleted", "uploaded_at" });

            migrationBuilder.CreateIndex(
                name: "IX_document_versions_document_id_is_deleted_revision",
                table: "document_versions",
                columns: new[] { "document_id", "is_deleted", "revision" });

            migrationBuilder.CreateIndex(
                name: "IX_document_versions_document_id_is_deleted_uploaded_at",
                table: "document_versions",
                columns: new[] { "document_id", "is_deleted", "uploaded_at" });

            migrationBuilder.CreateIndex(
                name: "IX_document_histories_document_id_occurred_at",
                table: "document_histories",
                columns: new[] { "document_id", "occurred_at" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_documents_is_deleted_uploaded_at",
                table: "documents");

            migrationBuilder.DropIndex(
                name: "IX_document_versions_document_id_is_deleted_revision",
                table: "document_versions");

            migrationBuilder.DropIndex(
                name: "IX_document_versions_document_id_is_deleted_uploaded_at",
                table: "document_versions");

            migrationBuilder.DropIndex(
                name: "IX_document_histories_document_id_occurred_at",
                table: "document_histories");
        }
    }
}
