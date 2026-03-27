using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase35ControlMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "control_catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    control_code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    control_set = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    process_area = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_control_catalog", x => x.id);
                    table.ForeignKey(
                        name: "FK_control_catalog_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "control_coverage_snapshots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    control_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    coverage_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    active_mapping_count = table.Column<int>(type: "integer", nullable: false),
                    evidence_count = table.Column<int>(type: "integer", nullable: false),
                    gap_count = table.Column<int>(type: "integer", nullable: false),
                    generated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_control_coverage_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "FK_control_coverage_snapshots_control_catalog_control_id",
                        column: x => x.control_id,
                        principalTable: "control_catalog",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_control_coverage_snapshots_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "control_mappings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    control_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    target_module = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    target_entity_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    target_entity_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    target_route = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    evidence_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_by_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    activated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    retired_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_control_mappings", x => x.id);
                    table.ForeignKey(
                        name: "FK_control_mappings_control_catalog_control_id",
                        column: x => x.control_id,
                        principalTable: "control_catalog",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_control_mappings_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_control_catalog_control_code",
                table: "control_catalog",
                column: "control_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_control_catalog_project_id_control_set_status",
                table: "control_catalog",
                columns: new[] { "project_id", "control_set", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_control_catalog_updated_at",
                table: "control_catalog",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_control_coverage_snapshots_control_id_generated_at",
                table: "control_coverage_snapshots",
                columns: new[] { "control_id", "generated_at" });

            migrationBuilder.CreateIndex(
                name: "IX_control_coverage_snapshots_project_id_generated_at",
                table: "control_coverage_snapshots",
                columns: new[] { "project_id", "generated_at" });

            migrationBuilder.CreateIndex(
                name: "IX_control_mappings_control_id_status",
                table: "control_mappings",
                columns: new[] { "control_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_control_mappings_project_id",
                table: "control_mappings",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_control_mappings_target_module_target_entity_type_target_en~",
                table: "control_mappings",
                columns: new[] { "target_module", "target_entity_type", "target_entity_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "control_coverage_snapshots");

            migrationBuilder.DropTable(
                name: "control_mappings");

            migrationBuilder.DropTable(
                name: "control_catalog");
        }
    }
}
