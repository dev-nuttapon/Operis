using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Operis_API.Infrastructure.Persistence;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(OperisDbContext))]
    [Migration("20260320113000_AddWorkflowDefinitionTemplateAndStepDocument")]
    public partial class AddWorkflowDefinitionTemplateAndStepDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "document_template_id",
                table: "workflow_definitions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "document_id",
                table: "workflow_steps",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_steps_document_id",
                table: "workflow_steps",
                column: "document_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_workflow_steps_document_id",
                table: "workflow_steps");

            migrationBuilder.DropColumn(
                name: "document_template_id",
                table: "workflow_definitions");

            migrationBuilder.DropColumn(
                name: "document_id",
                table: "workflow_steps");
        }
    }
}
