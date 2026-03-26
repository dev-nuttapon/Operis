using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    public partial class Phase6MeetingsDecisions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "meeting_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    meeting_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    meeting_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    facilitator_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    agenda = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    discussion_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    is_restricted = table.Column<bool>(type: "boolean", nullable: false),
                    classification = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meeting_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_meeting_records_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "meeting_minutes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    meeting_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    decisions_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    actions_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meeting_minutes", x => x.id);
                    table.ForeignKey(
                        name: "FK_meeting_minutes_meeting_records_meeting_record_id",
                        column: x => x.meeting_record_id,
                        principalTable: "meeting_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "meeting_attendees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    meeting_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    attendance_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meeting_attendees", x => x.id);
                    table.ForeignKey(
                        name: "FK_meeting_attendees_meeting_records_meeting_record_id",
                        column: x => x.meeting_record_id,
                        principalTable: "meeting_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "decisions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    meeting_id = table.Column<Guid>(type: "uuid", nullable: true),
                    code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    decision_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    rationale = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    alternatives_considered = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    impacted_artifacts_json = table.Column<string>(type: "jsonb", nullable: false),
                    approved_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    is_restricted = table.Column<bool>(type: "boolean", nullable: false),
                    classification = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_decisions", x => x.id);
                    table.ForeignKey(
                        name: "FK_decisions_meeting_records_meeting_id",
                        column: x => x.meeting_id,
                        principalTable: "meeting_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_decisions_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_meeting_records_project_id_meeting_type_meeting_at", table: "meeting_records", columns: new[] { "project_id", "meeting_type", "meeting_at" });
            migrationBuilder.CreateIndex(name: "IX_meeting_records_project_id_status", table: "meeting_records", columns: new[] { "project_id", "status" });
            migrationBuilder.CreateIndex(name: "IX_meeting_records_is_restricted", table: "meeting_records", column: "is_restricted");
            migrationBuilder.CreateIndex(name: "IX_meeting_minutes_meeting_record_id", table: "meeting_minutes", column: "meeting_record_id", unique: true);
            migrationBuilder.CreateIndex(name: "IX_meeting_attendees_meeting_record_id_user_id", table: "meeting_attendees", columns: new[] { "meeting_record_id", "user_id" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_decisions_project_id_code", table: "decisions", columns: new[] { "project_id", "code" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_decisions_project_id_status_decision_type", table: "decisions", columns: new[] { "project_id", "status", "decision_type" });
            migrationBuilder.CreateIndex(name: "IX_decisions_meeting_id", table: "decisions", column: "meeting_id");
            migrationBuilder.CreateIndex(name: "IX_decisions_is_restricted", table: "decisions", column: "is_restricted");

            migrationBuilder.Sql(
                """
                INSERT INTO app_roles (id, name, keycloak_role_name, description, display_order, created_at)
                VALUES
                    ('88888888-0000-0000-0000-000000000001', 'Meeting Manager', 'operis:meeting_manager', 'Manages meetings, minutes, and decision drafting.', 90, NOW()),
                    ('88888888-0000-0000-0000-000000000002', 'Meeting Approver', 'operis:meeting_approver', 'Approves meetings and decisions.', 91, NOW()),
                    ('88888888-0000-0000-0000-000000000003', 'Meeting Viewer', 'operis:meeting_viewer', 'Reads non-restricted meetings and decisions.', 92, NOW())
                ON CONFLICT DO NOTHING;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM app_roles
                WHERE keycloak_role_name IN ('operis:meeting_manager', 'operis:meeting_approver', 'operis:meeting_viewer');
                """);

            migrationBuilder.DropTable(name: "meeting_attendees");
            migrationBuilder.DropTable(name: "meeting_minutes");
            migrationBuilder.DropTable(name: "decisions");
            migrationBuilder.DropTable(name: "meeting_records");
        }
    }
}
