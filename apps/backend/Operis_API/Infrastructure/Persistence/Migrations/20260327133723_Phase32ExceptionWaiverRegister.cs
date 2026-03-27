using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase32ExceptionWaiverRegister : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "waivers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    waiver_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    process_area = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    scope_summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    requested_by_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    justification = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    expires_at = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    decision_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    decision_by_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    decision_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    closure_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_waivers", x => x.id);
                    table.ForeignKey(
                        name: "FK_waivers_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "compensating_controls",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    waiver_id = table.Column<Guid>(type: "uuid", nullable: false),
                    control_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    owner_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compensating_controls", x => x.id);
                    table.ForeignKey(
                        name: "FK_compensating_controls_waivers_waiver_id",
                        column: x => x.waiver_id,
                        principalTable: "waivers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "waiver_reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    waiver_id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    outcome_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    reviewer_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    next_review_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_waiver_reviews", x => x.id);
                    table.ForeignKey(
                        name: "FK_waiver_reviews_waivers_waiver_id",
                        column: x => x.waiver_id,
                        principalTable: "waivers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compensating_controls_waiver_id_status",
                table: "compensating_controls",
                columns: new[] { "waiver_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_waiver_reviews_waiver_id_reviewed_at",
                table: "waiver_reviews",
                columns: new[] { "waiver_id", "reviewed_at" });

            migrationBuilder.CreateIndex(
                name: "IX_waivers_process_area_status_expires_at",
                table: "waivers",
                columns: new[] { "process_area", "status", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "IX_waivers_project_id",
                table: "waivers",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_waivers_waiver_code",
                table: "waivers",
                column: "waiver_code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compensating_controls");

            migrationBuilder.DropTable(
                name: "waiver_reviews");

            migrationBuilder.DropTable(
                name: "waivers");
        }
    }
}
