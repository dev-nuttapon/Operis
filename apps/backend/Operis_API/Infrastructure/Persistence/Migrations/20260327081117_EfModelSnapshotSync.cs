using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations;

public partial class EfModelSnapshotSync : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Snapshot-only migration. Historical phase migrations were authored manually
        // before dotnet-ef tooling was aligned, so this migration intentionally makes
        // no schema changes and only advances the EF model snapshot.
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // No-op reverse path. This migration exists only to align the EF model snapshot.
    }
}
