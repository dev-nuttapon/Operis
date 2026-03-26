using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    public partial class Phase2DocumentGovernanceCore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "document_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    module_owner = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    classification_default = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    retention_class_default = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    approval_required = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "document_approvals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    step_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    reviewer_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    decision = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    decision_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    decided_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_approvals", x => x.id);
                    table.ForeignKey(
                        name: "FK_document_approvals_document_versions_document_version_id",
                        column: x => x.document_version_id,
                        principalTable: "document_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_entity_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    target_entity_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    link_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_links", x => x.id);
                    table.ForeignKey(
                        name: "FK_document_links_documents_source_document_id",
                        column: x => x.source_document_id,
                        principalTable: "documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.RenameColumn(name: "document_name", table: "documents", newName: "title");
            migrationBuilder.RenameColumn(name: "published_version_id", table: "documents", newName: "current_version_id");
            migrationBuilder.RenameColumn(name: "uploaded_by_user_id", table: "documents", newName: "owner_user_id");
            migrationBuilder.RenameColumn(name: "uploaded_at", table: "documents", newName: "created_at");

            migrationBuilder.RenameColumn(name: "revision", table: "document_versions", newName: "version_number");
            migrationBuilder.RenameColumn(name: "object_key", table: "document_versions", newName: "storage_key");
            migrationBuilder.RenameColumn(name: "size_bytes", table: "document_versions", newName: "file_size");
            migrationBuilder.RenameColumn(name: "content_type", table: "document_versions", newName: "mime_type");
            migrationBuilder.RenameColumn(name: "uploaded_by_user_id", table: "document_versions", newName: "uploaded_by");

            migrationBuilder.RenameIndex(name: "IX_documents_published_version_id", table: "documents", newName: "IX_documents_current_version_id");
            migrationBuilder.RenameIndex(name: "IX_document_versions_object_key", table: "document_versions", newName: "IX_document_versions_storage_key");
            migrationBuilder.RenameIndex(name: "IX_document_versions_document_id_revision", table: "document_versions", newName: "IX_document_versions_document_id_version_number");
            migrationBuilder.RenameIndex(name: "IX_document_versions_document_id_is_deleted_revision", table: "document_versions", newName: "IX_document_versions_document_id_is_deleted_version_number");

            migrationBuilder.AddColumn<Guid>(name: "document_type_id", table: "documents", type: "uuid", nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "project_id", table: "documents", type: "uuid", nullable: true);
            migrationBuilder.AddColumn<string>(name: "phase_code", table: "documents", type: "character varying(64)", maxLength: 64, nullable: true);
            migrationBuilder.AddColumn<string>(name: "status", table: "documents", type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "draft");
            migrationBuilder.AddColumn<string>(name: "classification", table: "documents", type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "internal");
            migrationBuilder.AddColumn<string>(name: "retention_class", table: "documents", type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "standard");
            migrationBuilder.AddColumn<string>(name: "tags_json", table: "documents", type: "jsonb", nullable: false, defaultValue: "[]");
            migrationBuilder.AddColumn<DateTimeOffset>(name: "updated_at", table: "documents", type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<string>(name: "status", table: "document_versions", type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "uploaded");

            migrationBuilder.AlterColumn<string>(name: "title", table: "documents", type: "character varying(512)", maxLength: 512, nullable: false, oldClrType: typeof(string), oldType: "character varying(256)", oldMaxLength: 256);
            migrationBuilder.AlterColumn<string>(name: "mime_type", table: "document_versions", type: "character varying(128)", maxLength: 128, nullable: false, oldClrType: typeof(string), oldType: "character varying(256)", oldMaxLength: 256);
            migrationBuilder.AlterColumn<string>(name: "file_name", table: "document_versions", type: "character varying(512)", maxLength: 512, nullable: false, oldClrType: typeof(string), oldType: "character varying(256)", oldMaxLength: 256);
            migrationBuilder.AlterColumn<string>(name: "uploaded_by", table: "document_versions", type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "unknown", oldClrType: typeof(string), oldType: "character varying(64)", oldMaxLength: 64, oldNullable: true);

            migrationBuilder.DropIndex(name: "IX_documents_uploaded_at", table: "documents");
            migrationBuilder.DropIndex(name: "IX_documents_is_deleted_uploaded_at", table: "documents");
            migrationBuilder.DropIndex(name: "IX_document_versions_document_id_version_code", table: "document_versions");

            migrationBuilder.DropColumn(name: "version_code", table: "document_versions");

            migrationBuilder.Sql(
                """
                UPDATE documents
                SET
                    owner_user_id = COALESCE(owner_user_id, 'unknown'),
                    phase_code = COALESCE(phase_code, 'GEN'),
                    updated_at = COALESCE(updated_at, created_at),
                    status = CASE
                        WHEN current_version_id IS NULL THEN 'draft'
                        ELSE 'approved'
                    END,
                    tags_json = COALESCE(tags_json, '[]'::jsonb);
                """);

            migrationBuilder.Sql(
                """
                UPDATE document_versions
                SET status = CASE
                    WHEN id IN (SELECT current_version_id FROM documents WHERE current_version_id IS NOT NULL) THEN 'approved'
                    ELSE 'superseded'
                END;
                """);

            migrationBuilder.CreateIndex(name: "IX_document_types_code", table: "document_types", column: "code", unique: true);
            migrationBuilder.CreateIndex(name: "IX_document_types_status", table: "document_types", column: "status");
            migrationBuilder.CreateIndex(name: "IX_document_approvals_document_version_id", table: "document_approvals", column: "document_version_id");
            migrationBuilder.CreateIndex(name: "IX_document_links_source_document_id", table: "document_links", column: "source_document_id");

            migrationBuilder.CreateIndex(name: "IX_documents_project_id_status_phase_code", table: "documents", columns: new[] { "project_id", "status", "phase_code" });
            migrationBuilder.CreateIndex(name: "IX_documents_document_type_id_status", table: "documents", columns: new[] { "document_type_id", "status" });
            migrationBuilder.CreateIndex(name: "IX_documents_owner_user_id_updated_at", table: "documents", columns: new[] { "owner_user_id", "updated_at" });

            migrationBuilder.AddForeignKey(
                name: "FK_documents_document_types_document_type_id",
                table: "documents",
                column: "document_type_id",
                principalTable: "document_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_documents_projects_project_id",
                table: "documents",
                column: "project_id",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_documents_document_types_document_type_id", table: "documents");
            migrationBuilder.DropForeignKey(name: "FK_documents_projects_project_id", table: "documents");

            migrationBuilder.DropTable(name: "document_approvals");
            migrationBuilder.DropTable(name: "document_links");
            migrationBuilder.DropTable(name: "document_types");

            migrationBuilder.DropIndex(name: "IX_documents_project_id_status_phase_code", table: "documents");
            migrationBuilder.DropIndex(name: "IX_documents_document_type_id_status", table: "documents");
            migrationBuilder.DropIndex(name: "IX_documents_owner_user_id_updated_at", table: "documents");

            migrationBuilder.AddColumn<string>(
                name: "version_code",
                table: "document_versions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "v1");

            migrationBuilder.Sql(
                """
                UPDATE document_versions
                SET version_code = 'v' || version_number::text;
                """);

            migrationBuilder.CreateIndex(name: "IX_documents_uploaded_at", table: "documents", column: "created_at");
            migrationBuilder.CreateIndex(name: "IX_documents_is_deleted_uploaded_at", table: "documents", columns: new[] { "is_deleted", "created_at" });
            migrationBuilder.CreateIndex(name: "IX_document_versions_document_id_version_code", table: "document_versions", columns: new[] { "document_id", "version_code" }, unique: true);

            migrationBuilder.DropColumn(name: "document_type_id", table: "documents");
            migrationBuilder.DropColumn(name: "project_id", table: "documents");
            migrationBuilder.DropColumn(name: "phase_code", table: "documents");
            migrationBuilder.DropColumn(name: "status", table: "documents");
            migrationBuilder.DropColumn(name: "classification", table: "documents");
            migrationBuilder.DropColumn(name: "retention_class", table: "documents");
            migrationBuilder.DropColumn(name: "tags_json", table: "documents");
            migrationBuilder.DropColumn(name: "updated_at", table: "documents");
            migrationBuilder.DropColumn(name: "status", table: "document_versions");

            migrationBuilder.RenameColumn(name: "title", table: "documents", newName: "document_name");
            migrationBuilder.RenameColumn(name: "current_version_id", table: "documents", newName: "published_version_id");
            migrationBuilder.RenameColumn(name: "owner_user_id", table: "documents", newName: "uploaded_by_user_id");
            migrationBuilder.RenameColumn(name: "created_at", table: "documents", newName: "uploaded_at");

            migrationBuilder.RenameColumn(name: "version_number", table: "document_versions", newName: "revision");
            migrationBuilder.RenameColumn(name: "storage_key", table: "document_versions", newName: "object_key");
            migrationBuilder.RenameColumn(name: "file_size", table: "document_versions", newName: "size_bytes");
            migrationBuilder.RenameColumn(name: "mime_type", table: "document_versions", newName: "content_type");
            migrationBuilder.RenameColumn(name: "uploaded_by", table: "document_versions", newName: "uploaded_by_user_id");

            migrationBuilder.RenameIndex(name: "IX_documents_current_version_id", table: "documents", newName: "IX_documents_published_version_id");
            migrationBuilder.RenameIndex(name: "IX_document_versions_storage_key", table: "document_versions", newName: "IX_document_versions_object_key");
            migrationBuilder.RenameIndex(name: "IX_document_versions_document_id_version_number", table: "document_versions", newName: "IX_document_versions_document_id_revision");
            migrationBuilder.RenameIndex(name: "IX_document_versions_document_id_is_deleted_version_number", table: "document_versions", newName: "IX_document_versions_document_id_is_deleted_revision");

            migrationBuilder.AlterColumn<string>(name: "document_name", table: "documents", type: "character varying(256)", maxLength: 256, nullable: false, oldClrType: typeof(string), oldType: "character varying(512)", oldMaxLength: 512);
            migrationBuilder.AlterColumn<string>(name: "content_type", table: "document_versions", type: "character varying(256)", maxLength: 256, nullable: false, oldClrType: typeof(string), oldType: "character varying(128)", oldMaxLength: 128);
            migrationBuilder.AlterColumn<string>(name: "file_name", table: "document_versions", type: "character varying(256)", maxLength: 256, nullable: false, oldClrType: typeof(string), oldType: "character varying(512)", oldMaxLength: 512);
            migrationBuilder.AlterColumn<string>(name: "uploaded_by_user_id", table: "document_versions", type: "character varying(64)", maxLength: 64, nullable: true, oldClrType: typeof(string), oldType: "character varying(64)", oldMaxLength: 64);
        }
    }
}
