using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitationTokenFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "invitation_token",
                table: "user_invitations",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE user_invitations
                SET invitation_token = md5(id::text || clock_timestamp()::text || random()::text)
                WHERE invitation_token IS NULL OR invitation_token = '';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "invitation_token",
                table: "user_invitations",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_invitations_invitation_token",
                table: "user_invitations",
                column: "invitation_token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_invitations_invitation_token",
                table: "user_invitations");

            migrationBuilder.DropColumn(
                name: "invitation_token",
                table: "user_invitations");
        }
    }
}
