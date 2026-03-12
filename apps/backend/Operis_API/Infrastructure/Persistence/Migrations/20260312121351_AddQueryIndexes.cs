using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_users_deleted_at_created_at",
                table: "users",
                columns: new[] { "deleted_at", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_user_registration_requests_status_requested_at",
                table: "user_registration_requests",
                columns: new[] { "status", "requested_at" });

            migrationBuilder.CreateIndex(
                name: "IX_user_invitations_status_invited_at",
                table: "user_invitations",
                columns: new[] { "status", "invited_at" });

            migrationBuilder.CreateIndex(
                name: "IX_job_titles_deleted_at_display_order_name",
                table: "job_titles",
                columns: new[] { "deleted_at", "display_order", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_documents_uploaded_at",
                table: "documents",
                column: "uploaded_at");

            migrationBuilder.CreateIndex(
                name: "IX_departments_deleted_at_display_order_name",
                table: "departments",
                columns: new[] { "deleted_at", "display_order", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_action_occurred_at",
                table: "audit_logs",
                columns: new[] { "action", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_actor_email_occurred_at",
                table: "audit_logs",
                columns: new[] { "actor_email", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_app_roles_deleted_at_display_order_name",
                table: "app_roles",
                columns: new[] { "deleted_at", "display_order", "name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_deleted_at_created_at",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_user_registration_requests_status_requested_at",
                table: "user_registration_requests");

            migrationBuilder.DropIndex(
                name: "IX_user_invitations_status_invited_at",
                table: "user_invitations");

            migrationBuilder.DropIndex(
                name: "IX_job_titles_deleted_at_display_order_name",
                table: "job_titles");

            migrationBuilder.DropIndex(
                name: "IX_documents_uploaded_at",
                table: "documents");

            migrationBuilder.DropIndex(
                name: "IX_departments_deleted_at_display_order_name",
                table: "departments");

            migrationBuilder.DropIndex(
                name: "IX_audit_logs_action_occurred_at",
                table: "audit_logs");

            migrationBuilder.DropIndex(
                name: "IX_audit_logs_actor_email_occurred_at",
                table: "audit_logs");

            migrationBuilder.DropIndex(
                name: "IX_app_roles_deleted_at_display_order_name",
                table: "app_roles");
        }
    }
}
