using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Operis_API.Infrastructure.Persistence;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(OperisDbContext))]
    [Migration("20260319152000_AddProjectRolePeerReview")]
    public partial class AddProjectRolePeerReview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_peer_review_role",
                table: "project_roles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_peer_review_role",
                table: "project_roles");
        }
    }
}
