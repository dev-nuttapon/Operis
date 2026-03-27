using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations;

public partial class Phase16SupplierAgreement : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "supplier_id",
            table: "external_dependencies",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "suppliers",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                supplier_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                owner_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                criticality = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                review_due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_suppliers", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "supplier_agreements",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                supplier_id = table.Column<Guid>(type: "uuid", nullable: false),
                agreement_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                sla_terms = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                evidence_ref = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_supplier_agreements", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_external_dependencies_supplier_id",
            table: "external_dependencies",
            column: "supplier_id");

        migrationBuilder.CreateIndex(
            name: "IX_suppliers_name",
            table: "suppliers",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_suppliers_supplier_type",
            table: "suppliers",
            column: "supplier_type");

        migrationBuilder.CreateIndex(
            name: "IX_suppliers_owner_user_id",
            table: "suppliers",
            column: "owner_user_id");

        migrationBuilder.CreateIndex(
            name: "IX_suppliers_criticality_status",
            table: "suppliers",
            columns: new[] { "criticality", "status" });

        migrationBuilder.CreateIndex(
            name: "IX_supplier_agreements_supplier_id_status",
            table: "supplier_agreements",
            columns: new[] { "supplier_id", "status" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "supplier_agreements");

        migrationBuilder.DropTable(
            name: "suppliers");

        migrationBuilder.DropIndex(
            name: "IX_external_dependencies_supplier_id",
            table: "external_dependencies");

        migrationBuilder.DropColumn(
            name: "supplier_id",
            table: "external_dependencies");
    }
}
