using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase30TailoringGovernanceHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "deviation_reason",
                table: "tailoring_records",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "review_due_at",
                table: "tailoring_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "standard_reference",
                table: "tailoring_records",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "tailoring_criteria_id",
                table: "tailoring_records",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "tailoring_review_cycle_id",
                table: "tailoring_records",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tailoring_criteria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    criterion_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    standard_reference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tailoring_criteria", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tailoring_review_cycles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    owner_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    review_due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    approver_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    decision_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tailoring_review_cycles", x => x.id);
                    table.ForeignKey(
                        name: "FK_tailoring_review_cycles_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tailoring_records_tailoring_criteria_id",
                table: "tailoring_records",
                column: "tailoring_criteria_id");

            migrationBuilder.CreateIndex(
                name: "IX_tailoring_records_tailoring_review_cycle_id_status",
                table: "tailoring_records",
                columns: new[] { "tailoring_review_cycle_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_tailoring_criteria_criterion_code",
                table: "tailoring_criteria",
                column: "criterion_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tailoring_criteria_standard_reference_status",
                table: "tailoring_criteria",
                columns: new[] { "standard_reference", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_tailoring_review_cycles_project_id_status_review_due_at",
                table: "tailoring_review_cycles",
                columns: new[] { "project_id", "status", "review_due_at" });

            migrationBuilder.CreateIndex(
                name: "IX_tailoring_review_cycles_review_code",
                table: "tailoring_review_cycles",
                column: "review_code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tailoring_records_tailoring_criteria_tailoring_criteria_id",
                table: "tailoring_records",
                column: "tailoring_criteria_id",
                principalTable: "tailoring_criteria",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tailoring_records_tailoring_review_cycles_tailoring_review_~",
                table: "tailoring_records",
                column: "tailoring_review_cycle_id",
                principalTable: "tailoring_review_cycles",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tailoring_records_tailoring_criteria_tailoring_criteria_id",
                table: "tailoring_records");

            migrationBuilder.DropForeignKey(
                name: "FK_tailoring_records_tailoring_review_cycles_tailoring_review_~",
                table: "tailoring_records");

            migrationBuilder.DropTable(
                name: "tailoring_criteria");

            migrationBuilder.DropTable(
                name: "tailoring_review_cycles");

            migrationBuilder.DropIndex(
                name: "IX_tailoring_records_tailoring_criteria_id",
                table: "tailoring_records");

            migrationBuilder.DropIndex(
                name: "IX_tailoring_records_tailoring_review_cycle_id_status",
                table: "tailoring_records");

            migrationBuilder.DropColumn(
                name: "deviation_reason",
                table: "tailoring_records");

            migrationBuilder.DropColumn(
                name: "review_due_at",
                table: "tailoring_records");

            migrationBuilder.DropColumn(
                name: "standard_reference",
                table: "tailoring_records");

            migrationBuilder.DropColumn(
                name: "tailoring_criteria_id",
                table: "tailoring_records");

            migrationBuilder.DropColumn(
                name: "tailoring_review_cycle_id",
                table: "tailoring_records");
        }
    }
}
