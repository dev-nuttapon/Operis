using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Operis_API.Infrastructure.Persistence;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(OperisDbContext))]
    [Migration("20260320093000_AddProjectWorkflowAndTemplate")]
    public partial class AddProjectWorkflowAndTemplate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        migrationBuilder.AddColumn<Guid>(
            name: "workflow_definition_id",
            table: "projects",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "document_template_id",
            table: "projects",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_projects_document_template_id",
            table: "projects",
            column: "document_template_id");

        migrationBuilder.CreateIndex(
            name: "IX_projects_workflow_definition_id",
            table: "projects",
            column: "workflow_definition_id");

        migrationBuilder.AddForeignKey(
            name: "FK_projects_document_templates_document_template_id",
            table: "projects",
            column: "document_template_id",
            principalTable: "document_templates",
            principalColumn: "id",
            onDelete: ReferentialAction.SetNull);

        migrationBuilder.AddForeignKey(
            name: "FK_projects_workflow_definitions_workflow_definition_id",
            table: "projects",
            column: "workflow_definition_id",
            principalTable: "workflow_definitions",
            principalColumn: "id",
            onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        migrationBuilder.DropForeignKey(
            name: "FK_projects_document_templates_document_template_id",
            table: "projects");

        migrationBuilder.DropForeignKey(
            name: "FK_projects_workflow_definitions_workflow_definition_id",
            table: "projects");

        migrationBuilder.DropIndex(
            name: "IX_projects_document_template_id",
            table: "projects");

        migrationBuilder.DropIndex(
            name: "IX_projects_workflow_definition_id",
            table: "projects");

        migrationBuilder.DropColumn(
            name: "document_template_id",
            table: "projects");

        migrationBuilder.DropColumn(
            name: "workflow_definition_id",
            table: "projects");
        }
    }
}
