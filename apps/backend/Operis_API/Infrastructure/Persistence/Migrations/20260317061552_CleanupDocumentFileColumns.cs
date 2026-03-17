using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CleanupDocumentFileColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_documents_object_key",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "bucket_name",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "content_type",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "file_name",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "object_key",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "size_bytes",
                table: "documents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "bucket_name",
                table: "documents",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "content_type",
                table: "documents",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "file_name",
                table: "documents",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "object_key",
                table: "documents",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "size_bytes",
                table: "documents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_documents_object_key",
                table: "documents",
                column: "object_key",
                unique: true,
                filter: "\"object_key\" IS NOT NULL");
        }
    }
}
