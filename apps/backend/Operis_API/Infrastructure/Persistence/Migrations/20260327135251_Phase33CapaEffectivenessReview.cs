using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase33CapaEffectivenessReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "capa_effectiveness_reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    capa_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    effectiveness_result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    evidence_ref = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    review_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    reviewed_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reopened_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    reopened_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    reopen_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_capa_effectiveness_reviews", x => x.id);
                    table.ForeignKey(
                        name: "FK_capa_effectiveness_reviews_capa_records_capa_record_id",
                        column: x => x.capa_record_id,
                        principalTable: "capa_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_capa_effectiveness_reviews_capa_record_id_reviewed_at",
                table: "capa_effectiveness_reviews",
                columns: new[] { "capa_record_id", "reviewed_at" });

            migrationBuilder.CreateIndex(
                name: "IX_capa_effectiveness_reviews_status_effectiveness_result",
                table: "capa_effectiveness_reviews",
                columns: new[] { "status", "effectiveness_result" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "capa_effectiveness_reviews");
        }
    }
}
