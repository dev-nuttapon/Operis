using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations;

public partial class Phase18KnowledgeBase : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "lessons_learned",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                project_id = table.Column<Guid>(type: "uuid", nullable: false),
                title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                lesson_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                owner_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                source_ref = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                context = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                what_happened = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                what_to_repeat = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                what_to_avoid = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                linked_evidence_json = table.Column<string>(type: "jsonb", nullable: true),
                published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_lessons_learned", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_lessons_learned_owner_user_id",
            table: "lessons_learned",
            column: "owner_user_id");

        migrationBuilder.CreateIndex(
            name: "IX_lessons_learned_project_id_lesson_type_status",
            table: "lessons_learned",
            columns: new[] { "project_id", "lesson_type", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_lessons_learned_published_at",
            table: "lessons_learned",
            column: "published_at");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "lessons_learned");
    }
}
