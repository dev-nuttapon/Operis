using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Operis_API.Infrastructure.Persistence;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(OperisDbContext))]
    [Migration("20260321100000_AddNotifications")]
    public partial class AddNotifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_recipient_user_id",
                table: "notifications",
                column: "recipient_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_recipient_user_id_status",
                table: "notifications",
                columns: new[] { "recipient_user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_recipient_user_id_created_at",
                table: "notifications",
                columns: new[] { "recipient_user_id", "created_at" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notifications");
        }
    }
}
