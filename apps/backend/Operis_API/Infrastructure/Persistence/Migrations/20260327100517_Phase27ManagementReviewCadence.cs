using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase27ManagementReviewCadence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "management_reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    review_code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    review_period = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    scheduled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    facilitator_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    agenda_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    minutes_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    decision_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    escalation_entity_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    escalation_entity_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    closed_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_management_reviews", x => x.id);
                    table.ForeignKey(
                        name: "FK_management_reviews_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "management_review_actions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    owner_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    is_mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    linked_entity_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    linked_entity_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_management_review_actions", x => x.id);
                    table.ForeignKey(
                        name: "FK_management_review_actions_management_reviews_review_id",
                        column: x => x.review_id,
                        principalTable: "management_reviews",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "management_review_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    decision = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    owner_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_management_review_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_management_review_items_management_reviews_review_id",
                        column: x => x.review_id,
                        principalTable: "management_reviews",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_management_review_actions_owner_user_id",
                table: "management_review_actions",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_management_review_actions_review_id_status_is_mandatory",
                table: "management_review_actions",
                columns: new[] { "review_id", "status", "is_mandatory" });

            migrationBuilder.CreateIndex(
                name: "IX_management_review_items_review_id_item_type_status",
                table: "management_review_items",
                columns: new[] { "review_id", "item_type", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_management_reviews_facilitator_user_id",
                table: "management_reviews",
                column: "facilitator_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_management_reviews_project_id_status_scheduled_at",
                table: "management_reviews",
                columns: new[] { "project_id", "status", "scheduled_at" });

            migrationBuilder.CreateIndex(
                name: "IX_management_reviews_review_code",
                table: "management_reviews",
                column: "review_code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "management_review_actions");

            migrationBuilder.DropTable(
                name: "management_review_items");

            migrationBuilder.DropTable(
                name: "management_reviews");
        }
    }
}
