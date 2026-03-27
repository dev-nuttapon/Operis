using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations;

public partial class Phase23BackupRestoreDr : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "backup_evidence",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                backup_scope = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                executed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                executed_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                evidence_ref = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_backup_evidence", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "dr_drills",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                scope_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                planned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                executed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                finding_count = table.Column<int>(type: "integer", nullable: false),
                summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_dr_drills", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "legal_holds",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                scope_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                scope_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                placed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                placed_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                released_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                released_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                release_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_legal_holds", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "restore_verifications",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                backup_evidence_id = table.Column<Guid>(type: "uuid", nullable: false),
                executed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                executed_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                result_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_restore_verifications", x => x.id);
                table.ForeignKey(
                    name: "FK_restore_verifications_backup_evidence_backup_evidence_id",
                    column: x => x.backup_evidence_id,
                    principalTable: "backup_evidence",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_backup_evidence_backup_scope",
            table: "backup_evidence",
            column: "backup_scope");

        migrationBuilder.CreateIndex(
            name: "IX_backup_evidence_executed_at",
            table: "backup_evidence",
            column: "executed_at");

        migrationBuilder.CreateIndex(
            name: "IX_backup_evidence_status",
            table: "backup_evidence",
            column: "status");

        migrationBuilder.CreateIndex(
            name: "IX_backup_evidence_backup_scope_executed_at",
            table: "backup_evidence",
            columns: new[] { "backup_scope", "executed_at" });

        migrationBuilder.CreateIndex(
            name: "IX_dr_drills_scope_ref_planned_at",
            table: "dr_drills",
            columns: new[] { "scope_ref", "planned_at" });

        migrationBuilder.CreateIndex(
            name: "IX_dr_drills_status",
            table: "dr_drills",
            column: "status");

        migrationBuilder.CreateIndex(
            name: "IX_legal_holds_placed_at",
            table: "legal_holds",
            column: "placed_at");

        migrationBuilder.CreateIndex(
            name: "IX_legal_holds_status",
            table: "legal_holds",
            column: "status");

        migrationBuilder.CreateIndex(
            name: "IX_legal_holds_status_placed_at",
            table: "legal_holds",
            columns: new[] { "status", "placed_at" });

        migrationBuilder.CreateIndex(
            name: "IX_restore_verifications_backup_evidence_id",
            table: "restore_verifications",
            column: "backup_evidence_id");

        migrationBuilder.CreateIndex(
            name: "IX_restore_verifications_executed_at",
            table: "restore_verifications",
            column: "executed_at");

        migrationBuilder.CreateIndex(
            name: "IX_restore_verifications_status",
            table: "restore_verifications",
            column: "status");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "dr_drills");
        migrationBuilder.DropTable(name: "legal_holds");
        migrationBuilder.DropTable(name: "restore_verifications");
        migrationBuilder.DropTable(name: "backup_evidence");
    }
}
