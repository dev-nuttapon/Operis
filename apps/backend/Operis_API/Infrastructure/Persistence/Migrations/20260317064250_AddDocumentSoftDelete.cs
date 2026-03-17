using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "documents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by_user_id",
                table: "documents",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "documents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "document_versions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by_user_id",
                table: "document_versions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "document_versions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_documents_is_deleted",
                table: "documents",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_document_versions_is_deleted",
                table: "document_versions",
                column: "is_deleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_documents_is_deleted",
                table: "documents");

            migrationBuilder.DropIndex(
                name: "IX_document_versions_is_deleted",
                table: "document_versions");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "deleted_by_user_id",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "document_versions");

            migrationBuilder.DropColumn(
                name: "deleted_by_user_id",
                table: "document_versions");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "document_versions");
        }
    }
}
