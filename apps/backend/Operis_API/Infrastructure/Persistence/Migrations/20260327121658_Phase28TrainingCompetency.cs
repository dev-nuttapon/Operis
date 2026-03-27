using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase28TrainingCompetency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "competency_reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    review_period = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    reviewer_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    planned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_competency_reviews", x => x.id);
                    table.ForeignKey(
                        name: "FK_competency_reviews_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "training_courses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    course_code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    provider = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    delivery_mode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    audience_scope = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    validity_months = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    activated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    retired_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_courses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role_training_requirements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    course_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    required_within_days = table.Column<int>(type: "integer", nullable: false),
                    renewal_interval_months = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_training_requirements", x => x.id);
                    table.ForeignKey(
                        name: "FK_role_training_requirements_project_roles_project_role_id",
                        column: x => x.project_role_id,
                        principalTable: "project_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_role_training_requirements_training_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "training_courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "training_completions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    course_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    assigned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completion_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    expiry_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    evidence_ref = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_completions", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_completions_project_roles_project_role_id",
                        column: x => x.project_role_id,
                        principalTable: "project_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_training_completions_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_training_completions_training_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "training_courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_competency_reviews_project_id_status",
                table: "competency_reviews",
                columns: new[] { "project_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_competency_reviews_user_id_status_planned_at",
                table: "competency_reviews",
                columns: new[] { "user_id", "status", "planned_at" });

            migrationBuilder.CreateIndex(
                name: "IX_role_training_requirements_course_id_project_role_id",
                table: "role_training_requirements",
                columns: new[] { "course_id", "project_role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_role_training_requirements_project_role_id_status",
                table: "role_training_requirements",
                columns: new[] { "project_role_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_training_completions_course_id_project_role_id_project_id_u~",
                table: "training_completions",
                columns: new[] { "course_id", "project_role_id", "project_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_training_completions_project_id",
                table: "training_completions",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_training_completions_project_role_id",
                table: "training_completions",
                column: "project_role_id");

            migrationBuilder.CreateIndex(
                name: "IX_training_completions_status_due_at",
                table: "training_completions",
                columns: new[] { "status", "due_at" });

            migrationBuilder.CreateIndex(
                name: "IX_training_completions_user_id_status",
                table: "training_completions",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_training_courses_course_code",
                table: "training_courses",
                column: "course_code",
                unique: true,
                filter: "\"course_code\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_training_courses_status_title",
                table: "training_courses",
                columns: new[] { "status", "title" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "competency_reviews");

            migrationBuilder.DropTable(
                name: "role_training_requirements");

            migrationBuilder.DropTable(
                name: "training_completions");

            migrationBuilder.DropTable(
                name: "training_courses");
        }
    }
}
