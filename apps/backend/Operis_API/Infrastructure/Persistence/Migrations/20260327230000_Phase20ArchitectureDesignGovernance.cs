using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations;

public partial class Phase20ArchitectureDesignGovernance : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "architecture_records",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                project_id = table.Column<Guid>(type: "uuid", nullable: false),
                title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                architecture_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                owner_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                current_version_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                security_impact = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                evidence_ref = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                approved_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_architecture_records", x => x.id);
                table.ForeignKey(
                    name: "FK_architecture_records_projects_project_id",
                    column: x => x.project_id,
                    principalTable: "projects",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "integration_reviews",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                scope_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                integration_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                reviewed_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                decision_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                risks = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                dependency_impact = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                evidence_ref = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                decided_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                applied_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_integration_reviews", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "design_reviews",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                architecture_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                review_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                reviewed_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                decision_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                design_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                concerns = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                evidence_ref = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                decided_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_design_reviews", x => x.id);
                table.ForeignKey(
                    name: "FK_design_reviews_architecture_records_architecture_record_id",
                    column: x => x.architecture_record_id,
                    principalTable: "architecture_records",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_architecture_records_owner_user_id",
            table: "architecture_records",
            column: "owner_user_id");

        migrationBuilder.CreateIndex(
            name: "IX_architecture_records_project_id_architecture_type_status",
            table: "architecture_records",
            columns: new[] { "project_id", "architecture_type", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_design_reviews_architecture_record_id_status",
            table: "design_reviews",
            columns: new[] { "architecture_record_id", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_design_reviews_reviewed_by",
            table: "design_reviews",
            column: "reviewed_by");

        migrationBuilder.CreateIndex(
            name: "IX_integration_reviews_reviewed_by",
            table: "integration_reviews",
            column: "reviewed_by");

        migrationBuilder.CreateIndex(
            name: "IX_integration_reviews_scope_ref_integration_type_status",
            table: "integration_reviews",
            columns: new[] { "scope_ref", "integration_type", "status" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "design_reviews");

        migrationBuilder.DropTable(
            name: "integration_reviews");

        migrationBuilder.DropTable(
            name: "architecture_records");
    }
}
