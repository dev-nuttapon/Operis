using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations;

public partial class Phase15DefectNonConformance : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "defects",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                project_id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                owner_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                detected_in_phase = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                resolution_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                corrective_action_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                affected_artifact_refs_json = table.Column<string>(type: "jsonb", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_defects", x => x.id);
                table.ForeignKey(
                    name: "FK_defects_projects_project_id",
                    column: x => x.project_id,
                    principalTable: "projects",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "non_conformances",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                project_id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                source_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                owner_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                corrective_action_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                root_cause = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                resolution_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                accepted_disposition = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                linked_finding_refs_json = table.Column<string>(type: "jsonb", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_non_conformances", x => x.id);
                table.ForeignKey(
                    name: "FK_non_conformances_projects_project_id",
                    column: x => x.project_id,
                    principalTable: "projects",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_defects_project_id_code",
            table: "defects",
            columns: new[] { "project_id", "code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_defects_project_id_severity_status",
            table: "defects",
            columns: new[] { "project_id", "severity", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_defects_owner_user_id",
            table: "defects",
            column: "owner_user_id");

        migrationBuilder.CreateIndex(
            name: "IX_non_conformances_project_id_code",
            table: "non_conformances",
            columns: new[] { "project_id", "code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_non_conformances_project_id_status",
            table: "non_conformances",
            columns: new[] { "project_id", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_non_conformances_owner_user_id",
            table: "non_conformances",
            column: "owner_user_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "defects");
        migrationBuilder.DropTable(name: "non_conformances");
    }
}
