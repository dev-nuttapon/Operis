using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    public partial class Phase11MasterDataOperationsSupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "master_data_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    domain = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_master_data_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "master_data_changes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    master_data_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    change_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    changed_by = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_master_data_changes", x => x.id);
                    table.ForeignKey(
                        name: "FK_master_data_changes_master_data_items_master_data_item_id",
                        column: x => x.master_data_item_id,
                        principalTable: "master_data_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_master_data_changes_master_data_item_id_changed_at",
                table: "master_data_changes",
                columns: new[] { "master_data_item_id", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "IX_master_data_items_domain_code",
                table: "master_data_items",
                columns: new[] { "domain", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_master_data_items_domain_status_display_order",
                table: "master_data_items",
                columns: new[] { "domain", "status", "display_order" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "master_data_changes");
            migrationBuilder.DropTable(name: "master_data_items");
        }
    }
}
