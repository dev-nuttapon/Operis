using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    public partial class Phase12SecurityDependencyConfigurationAudit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "access_reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    scope_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    scope_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    review_cycle = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    reviewed_by = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    decision = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    decision_rationale = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_access_reviews", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "configuration_audits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    scope_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    planned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    finding_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuration_audits", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "external_dependencies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    dependency_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    owner_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    criticality = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    review_due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_dependencies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "security_reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    scope_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    scope_ref = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    controls_reviewed = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    findings_summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_security_reviews", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_access_reviews_scope_type_status",
                table: "access_reviews",
                columns: new[] { "scope_type", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_configuration_audits_status_planned_at",
                table: "configuration_audits",
                columns: new[] { "status", "planned_at" });

            migrationBuilder.CreateIndex(
                name: "IX_external_dependencies_criticality_status_review_due_at",
                table: "external_dependencies",
                columns: new[] { "criticality", "status", "review_due_at" });

            migrationBuilder.CreateIndex(
                name: "IX_external_dependencies_dependency_type",
                table: "external_dependencies",
                column: "dependency_type");

            migrationBuilder.CreateIndex(
                name: "IX_external_dependencies_owner_user_id",
                table: "external_dependencies",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_security_reviews_scope_type_status",
                table: "security_reviews",
                columns: new[] { "scope_type", "status" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "access_reviews");
            migrationBuilder.DropTable(name: "configuration_audits");
            migrationBuilder.DropTable(name: "external_dependencies");
            migrationBuilder.DropTable(name: "security_reviews");
        }
    }
}
