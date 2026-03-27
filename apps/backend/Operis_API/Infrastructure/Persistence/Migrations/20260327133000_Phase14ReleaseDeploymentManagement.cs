using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations;

public partial class Phase14ReleaseDeploymentManagement : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "releases",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                project_id = table.Column<Guid>(type: "uuid", nullable: false),
                release_code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                planned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                released_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                quality_gate_result_id = table.Column<Guid>(type: "uuid", nullable: true),
                quality_gate_override_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                approved_by_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_releases", x => x.id);
                table.ForeignKey(
                    name: "FK_releases_projects_project_id",
                    column: x => x.project_id,
                    principalTable: "projects",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_releases_quality_gate_results_quality_gate_result_id",
                    column: x => x.quality_gate_result_id,
                    principalTable: "quality_gate_results",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "deployment_checklists",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                release_id = table.Column<Guid>(type: "uuid", nullable: false),
                checklist_item = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                owner_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                evidence_ref = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_deployment_checklists", x => x.id);
                table.ForeignKey(
                    name: "FK_deployment_checklists_releases_release_id",
                    column: x => x.release_id,
                    principalTable: "releases",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "release_notes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                release_id = table.Column<Guid>(type: "uuid", nullable: false),
                summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                included_changes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                known_issues = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_release_notes", x => x.id);
                table.ForeignKey(
                    name: "FK_release_notes_releases_release_id",
                    column: x => x.release_id,
                    principalTable: "releases",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_releases_project_id_release_code",
            table: "releases",
            columns: new[] { "project_id", "release_code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_releases_project_id_status_planned_at",
            table: "releases",
            columns: new[] { "project_id", "status", "planned_at" });

        migrationBuilder.CreateIndex(
            name: "IX_releases_quality_gate_result_id",
            table: "releases",
            column: "quality_gate_result_id");

        migrationBuilder.CreateIndex(
            name: "IX_deployment_checklists_release_id_status",
            table: "deployment_checklists",
            columns: new[] { "release_id", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_release_notes_release_id_status",
            table: "release_notes",
            columns: new[] { "release_id", "status" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "deployment_checklists");
        migrationBuilder.DropTable(name: "release_notes");
        migrationBuilder.DropTable(name: "releases");
    }
}
