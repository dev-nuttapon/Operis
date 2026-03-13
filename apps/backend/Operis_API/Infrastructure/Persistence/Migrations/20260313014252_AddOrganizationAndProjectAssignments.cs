using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationAndProjectAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "division_id",
                table: "departments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    start_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    end_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reporting_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    reports_to_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    department_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    effective_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    effective_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reporting_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_reporting_lines_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_reporting_lines_users_reports_to_user_id",
                        column: x => x.reports_to_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reporting_lines_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_org_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    division_id = table.Column<Guid>(type: "uuid", nullable: true),
                    department_id = table.Column<Guid>(type: "uuid", nullable: true),
                    position_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    is_division_head = table.Column<bool>(type: "boolean", nullable: false),
                    is_department_head = table.Column<bool>(type: "boolean", nullable: false),
                    start_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_org_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_org_assignments_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_user_org_assignments_divisions_division_id",
                        column: x => x.division_id,
                        principalTable: "divisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_user_org_assignments_job_titles_position_id",
                        column: x => x.position_id,
                        principalTable: "job_titles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_user_org_assignments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_project_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    start_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_project_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_project_assignments_project_roles_project_role_id",
                        column: x => x.project_role_id,
                        principalTable: "project_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_project_assignments_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_project_assignments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_departments_division_id",
                table: "departments",
                column: "division_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_code",
                table: "projects",
                column: "code",
                unique: true,
                filter: "\"deleted_at\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_projects_deleted_at_status_created_at",
                table: "projects",
                columns: new[] { "deleted_at", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_reporting_lines_department_id",
                table: "reporting_lines",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_reporting_lines_reports_to_user_id",
                table: "reporting_lines",
                column: "reports_to_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_reporting_lines_user_id_is_primary",
                table: "reporting_lines",
                columns: new[] { "user_id", "is_primary" });

            migrationBuilder.CreateIndex(
                name: "IX_user_org_assignments_department_id",
                table: "user_org_assignments",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_org_assignments_department_id_is_department_head",
                table: "user_org_assignments",
                columns: new[] { "department_id", "is_department_head" });

            migrationBuilder.CreateIndex(
                name: "IX_user_org_assignments_division_id",
                table: "user_org_assignments",
                column: "division_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_org_assignments_division_id_is_division_head",
                table: "user_org_assignments",
                columns: new[] { "division_id", "is_division_head" });

            migrationBuilder.CreateIndex(
                name: "IX_user_org_assignments_position_id",
                table: "user_org_assignments",
                column: "position_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_org_assignments_user_id_is_primary",
                table: "user_org_assignments",
                columns: new[] { "user_id", "is_primary" });

            migrationBuilder.CreateIndex(
                name: "IX_user_project_assignments_project_id_is_primary",
                table: "user_project_assignments",
                columns: new[] { "project_id", "is_primary" });

            migrationBuilder.CreateIndex(
                name: "IX_user_project_assignments_project_role_id",
                table: "user_project_assignments",
                column: "project_role_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_project_assignments_user_id_project_id_project_role_id",
                table: "user_project_assignments",
                columns: new[] { "user_id", "project_id", "project_role_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_departments_divisions_division_id",
                table: "departments",
                column: "division_id",
                principalTable: "divisions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_departments_divisions_division_id",
                table: "departments");

            migrationBuilder.DropTable(
                name: "reporting_lines");

            migrationBuilder.DropTable(
                name: "user_org_assignments");

            migrationBuilder.DropTable(
                name: "user_project_assignments");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropIndex(
                name: "IX_departments_division_id",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "division_id",
                table: "departments");
        }
    }
}
