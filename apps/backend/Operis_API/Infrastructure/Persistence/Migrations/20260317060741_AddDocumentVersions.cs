using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "document_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    revision = table.Column<int>(type: "integer", nullable: false),
                    version_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    file_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    object_key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    bucket_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    content_type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    uploaded_by_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    uploaded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_versions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_versions_document_id",
                table: "document_versions",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_versions_document_id_revision",
                table: "document_versions",
                columns: new[] { "document_id", "revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_versions_document_id_version_code",
                table: "document_versions",
                columns: new[] { "document_id", "version_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_versions_object_key",
                table: "document_versions",
                column: "object_key",
                unique: true,
                filter: "\"object_key\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_versions");
        }
    }
}
