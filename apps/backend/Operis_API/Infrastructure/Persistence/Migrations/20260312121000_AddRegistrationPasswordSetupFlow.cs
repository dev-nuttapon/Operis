using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    public partial class AddRegistrationPasswordSetupFlow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "password_setup_completed_at",
                table: "user_registration_requests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "password_setup_expires_at",
                table: "user_registration_requests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "password_setup_token",
                table: "user_registration_requests",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provisioned_user_id",
                table: "user_registration_requests",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_registration_requests_password_setup_token",
                table: "user_registration_requests",
                column: "password_setup_token",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_registration_requests_password_setup_token",
                table: "user_registration_requests");

            migrationBuilder.DropColumn(
                name: "password_setup_completed_at",
                table: "user_registration_requests");

            migrationBuilder.DropColumn(
                name: "password_setup_expires_at",
                table: "user_registration_requests");

            migrationBuilder.DropColumn(
                name: "password_setup_token",
                table: "user_registration_requests");

            migrationBuilder.DropColumn(
                name: "provisioned_user_id",
                table: "user_registration_requests");
        }
    }
}
