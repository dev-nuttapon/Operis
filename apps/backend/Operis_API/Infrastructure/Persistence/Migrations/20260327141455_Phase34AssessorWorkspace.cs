using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase34AssessorWorkspace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assessment_packages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    package_code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    process_area = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    scope_summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    evidence_references_json = table.Column<string>(type: "jsonb", nullable: false),
                    created_by_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    prepared_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    prepared_by_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    shared_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    shared_by_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    archived_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    archived_by_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assessment_packages", x => x.id);
                    table.ForeignKey(
                        name: "FK_assessment_packages_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assessment_findings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    package_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    evidence_entity_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    evidence_entity_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    evidence_route = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    owner_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    acceptance_summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    closure_summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_by_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    accepted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    accepted_by_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    closed_by_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assessment_findings", x => x.id);
                    table.ForeignKey(
                        name: "FK_assessment_findings_assessment_packages_package_id",
                        column: x => x.package_id,
                        principalTable: "assessment_packages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "assessment_notes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    package_id = table.Column<Guid>(type: "uuid", nullable: false),
                    note_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    note = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    created_by_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assessment_notes", x => x.id);
                    table.ForeignKey(
                        name: "FK_assessment_notes_assessment_packages_package_id",
                        column: x => x.package_id,
                        principalTable: "assessment_packages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assessment_findings_evidence_entity_type_evidence_entity_id",
                table: "assessment_findings",
                columns: new[] { "evidence_entity_type", "evidence_entity_id" });

            migrationBuilder.CreateIndex(
                name: "IX_assessment_findings_package_id_status",
                table: "assessment_findings",
                columns: new[] { "package_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_assessment_notes_package_id_created_at",
                table: "assessment_notes",
                columns: new[] { "package_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_assessment_packages_package_code",
                table: "assessment_packages",
                column: "package_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assessment_packages_project_id_process_area_status",
                table: "assessment_packages",
                columns: new[] { "project_id", "process_area", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_assessment_packages_updated_at",
                table: "assessment_packages",
                column: "updated_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assessment_findings");

            migrationBuilder.DropTable(
                name: "assessment_notes");

            migrationBuilder.DropTable(
                name: "assessment_packages");
        }
    }
}
