using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectGovernanceMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "methodology",
                table: "projects",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "owner_user_id",
                table: "projects",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phase",
                table: "projects",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "planned_end_at",
                table: "projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "planned_start_at",
                table: "projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "project_type",
                table: "projects",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "sponsor_user_id",
                table: "projects",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status_reason",
                table: "projects",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_projects_owner_user_id",
                table: "projects",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_phase",
                table: "projects",
                column: "phase");

            migrationBuilder.CreateIndex(
                name: "IX_projects_project_type",
                table: "projects",
                column: "project_type");

            migrationBuilder.CreateIndex(
                name: "IX_projects_sponsor_user_id",
                table: "projects",
                column: "sponsor_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_projects_users_owner_user_id",
                table: "projects",
                column: "owner_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_projects_users_sponsor_user_id",
                table: "projects",
                column: "sponsor_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_projects_users_owner_user_id",
                table: "projects");

            migrationBuilder.DropForeignKey(
                name: "FK_projects_users_sponsor_user_id",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projects_owner_user_id",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projects_phase",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projects_project_type",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projects_sponsor_user_id",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "methodology",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "owner_user_id",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "phase",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "planned_end_at",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "planned_start_at",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "project_type",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "sponsor_user_id",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "status_reason",
                table: "projects");
        }
    }
}
