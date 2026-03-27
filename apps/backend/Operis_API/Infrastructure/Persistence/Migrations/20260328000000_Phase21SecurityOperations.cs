using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations;

public partial class Phase21SecurityOperations : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "data_classification_policies",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                policy_code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                classification_level = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                scope = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                handling_rule = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_data_classification_policies", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "privileged_access_events",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                requested_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                approved_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                used_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                requested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_privileged_access_events", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "secret_rotations",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                secret_scope = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                planned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                rotated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                verified_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_secret_rotations", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "security_incidents",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                project_id = table.Column<Guid>(type: "uuid", nullable: true),
                code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                reported_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                owner_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                resolution_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_security_incidents", x => x.id);
                table.ForeignKey(
                    name: "FK_security_incidents_projects_project_id",
                    column: x => x.project_id,
                    principalTable: "projects",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "vulnerability_records",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                asset_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                identified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                patch_due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                owner_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                verification_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_vulnerability_records", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_data_classification_policies_policy_code",
            table: "data_classification_policies",
            column: "policy_code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_data_classification_policies_classification_level_status",
            table: "data_classification_policies",
            columns: new[] { "classification_level", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_privileged_access_events_requested_by_status",
            table: "privileged_access_events",
            columns: new[] { "requested_by", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_privileged_access_events_approved_by",
            table: "privileged_access_events",
            column: "approved_by");

        migrationBuilder.CreateIndex(
            name: "IX_privileged_access_events_used_by",
            table: "privileged_access_events",
            column: "used_by");

        migrationBuilder.CreateIndex(
            name: "IX_secret_rotations_secret_scope_status",
            table: "secret_rotations",
            columns: new[] { "secret_scope", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_secret_rotations_verified_by",
            table: "secret_rotations",
            column: "verified_by");

        migrationBuilder.CreateIndex(
            name: "IX_security_incidents_code",
            table: "security_incidents",
            column: "code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_security_incidents_severity_status_reported_at",
            table: "security_incidents",
            columns: new[] { "severity", "status", "reported_at" });

        migrationBuilder.CreateIndex(
            name: "IX_security_incidents_owner_user_id",
            table: "security_incidents",
            column: "owner_user_id");

        migrationBuilder.CreateIndex(
            name: "IX_security_incidents_project_id",
            table: "security_incidents",
            column: "project_id");

        migrationBuilder.CreateIndex(
            name: "IX_vulnerability_records_asset_ref_status",
            table: "vulnerability_records",
            columns: new[] { "asset_ref", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_vulnerability_records_severity_status",
            table: "vulnerability_records",
            columns: new[] { "severity", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_vulnerability_records_owner_user_id",
            table: "vulnerability_records",
            column: "owner_user_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "data_classification_policies");
        migrationBuilder.DropTable(name: "privileged_access_events");
        migrationBuilder.DropTable(name: "secret_rotations");
        migrationBuilder.DropTable(name: "security_incidents");
        migrationBuilder.DropTable(name: "vulnerability_records");
    }
}
