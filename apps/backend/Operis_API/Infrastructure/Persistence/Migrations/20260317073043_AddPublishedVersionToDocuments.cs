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
