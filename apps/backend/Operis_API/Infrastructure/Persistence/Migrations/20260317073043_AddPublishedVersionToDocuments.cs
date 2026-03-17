using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPublishedVersionToDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "published_version_id",
                table: "documents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_documents_published_version_id",
                table: "documents",
                column: "published_version_id");

            migrationBuilder.AddForeignKey(
                name: "FK_documents_document_versions_published_version_id",
                table: "documents",
                column: "published_version_id",
                principalTable: "document_versions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.Sql("""
                UPDATE documents AS d
                SET published_version_id = v.id
                FROM (
                    SELECT DISTINCT ON (document_id) id, document_id
                    FROM document_versions
                    WHERE is_deleted = false
                    ORDER BY document_id, revision DESC, uploaded_at DESC
                ) AS v
                WHERE d.id = v.document_id
                  AND d.published_version_id IS NULL
                  AND d.is_deleted = false;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_documents_document_versions_published_version_id",
                table: "documents");

            migrationBuilder.DropIndex(
                name: "IX_documents_published_version_id",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "published_version_id",
                table: "documents");
        }
    }
}
