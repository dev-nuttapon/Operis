using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Operis_API.Infrastructure.Persistence;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(OperisDbContext))]
    [Migration("20260320103000_AddWorkflowInstances")]
    public partial class AddWorkflowInstances : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS workflow_steps (
    id uuid NOT NULL,
    workflow_definition_id uuid NOT NULL,
    name character varying(200) NOT NULL,
    step_type character varying(32) NOT NULL,
    display_order integer NOT NULL,
    is_required boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone,
    CONSTRAINT ""PK_workflow_steps"" PRIMARY KEY (id),
    CONSTRAINT ""FK_workflow_steps_workflow_definitions_workflow_definition_id"" FOREIGN KEY (workflow_definition_id) REFERENCES workflow_definitions (id) ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS workflow_step_roles (
    id uuid NOT NULL,
    workflow_step_id uuid NOT NULL,
    project_role_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    CONSTRAINT ""PK_workflow_step_roles"" PRIMARY KEY (id),
    CONSTRAINT ""FK_workflow_step_roles_project_roles_project_role_id"" FOREIGN KEY (project_role_id) REFERENCES project_roles (id) ON DELETE CASCADE,
    CONSTRAINT ""FK_workflow_step_roles_workflow_steps_workflow_step_id"" FOREIGN KEY (workflow_step_id) REFERENCES workflow_steps (id) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS ""IX_workflow_step_roles_project_role_id"" ON workflow_step_roles (project_role_id);
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_workflow_step_roles_workflow_step_id_project_role_id"" ON workflow_step_roles (workflow_step_id, project_role_id);
CREATE INDEX IF NOT EXISTS ""IX_workflow_steps_workflow_definition_id_display_order"" ON workflow_steps (workflow_definition_id, display_order);
");
        migrationBuilder.CreateTable(
            name: "workflow_instances",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                project_id = table.Column<Guid>(type: "uuid", nullable: false),
                document_id = table.Column<Guid>(type: "uuid", nullable: false),
                workflow_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                current_step_order = table.Column<int>(type: "integer", nullable: false),
                started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_workflow_instances", x => x.id);
                table.ForeignKey(
                    name: "FK_workflow_instances_documents_document_id",
                    column: x => x.document_id,
                    principalTable: "documents",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_workflow_instances_projects_project_id",
                    column: x => x.project_id,
                    principalTable: "projects",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_workflow_instances_workflow_definitions_workflow_definition_id",
                    column: x => x.workflow_definition_id,
                    principalTable: "workflow_definitions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "workflow_instance_steps",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                workflow_instance_id = table.Column<Guid>(type: "uuid", nullable: false),
                workflow_step_id = table.Column<Guid>(type: "uuid", nullable: false),
                step_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                display_order = table.Column<int>(type: "integer", nullable: false),
                is_required = table.Column<bool>(type: "boolean", nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_workflow_instance_steps", x => x.id);
                table.ForeignKey(
                    name: "FK_workflow_instance_steps_workflow_instances_workflow_instance_id",
                    column: x => x.workflow_instance_id,
                    principalTable: "workflow_instances",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_workflow_instance_steps_workflow_steps_workflow_step_id",
                    column: x => x.workflow_step_id,
                    principalTable: "workflow_steps",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "workflow_instance_actions",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                workflow_instance_step_id = table.Column<Guid>(type: "uuid", nullable: false),
                action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                actor_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                actor_email = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                actor_display_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                comment = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_workflow_instance_actions", x => x.id);
                table.ForeignKey(
                    name: "FK_workflow_instance_actions_workflow_instance_steps_workflow_instance_step_id",
                    column: x => x.workflow_instance_step_id,
                    principalTable: "workflow_instance_steps",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_workflow_instances_document_id",
            table: "workflow_instances",
            column: "document_id");

        migrationBuilder.CreateIndex(
            name: "IX_workflow_instances_project_id",
            table: "workflow_instances",
            column: "project_id");

        migrationBuilder.CreateIndex(
            name: "IX_workflow_instances_workflow_definition_id",
            table: "workflow_instances",
            column: "workflow_definition_id");

        migrationBuilder.CreateIndex(
            name: "IX_workflow_instances_project_id_status_created_at",
            table: "workflow_instances",
            columns: new[] { "project_id", "status", "created_at" });

        migrationBuilder.CreateIndex(
            name: "IX_workflow_instance_steps_workflow_instance_id_display_order",
            table: "workflow_instance_steps",
            columns: new[] { "workflow_instance_id", "display_order" });

        migrationBuilder.CreateIndex(
            name: "IX_workflow_instance_steps_workflow_step_id",
            table: "workflow_instance_steps",
            column: "workflow_step_id");

        migrationBuilder.CreateIndex(
            name: "IX_workflow_instance_actions_workflow_instance_step_id",
            table: "workflow_instance_actions",
            column: "workflow_instance_step_id");

        migrationBuilder.CreateIndex(
            name: "IX_workflow_instance_actions_actor_user_id_created_at",
            table: "workflow_instance_actions",
            columns: new[] { "actor_user_id", "created_at" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        migrationBuilder.DropTable(
            name: "workflow_instance_actions");

        migrationBuilder.DropTable(
            name: "workflow_instance_steps");

        migrationBuilder.DropTable(
            name: "workflow_instances");
        }
    }
}
