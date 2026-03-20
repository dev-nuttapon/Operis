using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _20260320_PendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_workflow_step_roles_project_role_id"
                ON workflow_step_roles (project_role_id);
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                  IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint WHERE conname = 'FK_workflow_step_roles_project_roles_project_role_id'
                  ) THEN
                    ALTER TABLE workflow_step_roles
                    ADD CONSTRAINT "FK_workflow_step_roles_project_roles_project_role_id"
                    FOREIGN KEY (project_role_id) REFERENCES project_roles (id) ON DELETE CASCADE;
                  END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                  IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint WHERE conname = 'FK_workflow_step_roles_workflow_steps_workflow_step_id'
                  ) THEN
                    ALTER TABLE workflow_step_roles
                    ADD CONSTRAINT "FK_workflow_step_roles_workflow_steps_workflow_step_id"
                    FOREIGN KEY (workflow_step_id) REFERENCES workflow_steps (id) ON DELETE CASCADE;
                  END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                  IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint WHERE conname = 'FK_workflow_step_routes_workflow_steps_next_step_id'
                  ) THEN
                    ALTER TABLE workflow_step_routes
                    ADD CONSTRAINT "FK_workflow_step_routes_workflow_steps_next_step_id"
                    FOREIGN KEY (next_step_id) REFERENCES workflow_steps (id) ON DELETE SET NULL;
                  END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                  IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint WHERE conname = 'FK_workflow_step_routes_workflow_steps_workflow_step_id'
                  ) THEN
                    ALTER TABLE workflow_step_routes
                    ADD CONSTRAINT "FK_workflow_step_routes_workflow_steps_workflow_step_id"
                    FOREIGN KEY (workflow_step_id) REFERENCES workflow_steps (id) ON DELETE CASCADE;
                  END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE workflow_step_roles
                DROP CONSTRAINT IF EXISTS "FK_workflow_step_roles_project_roles_project_role_id";
                """);

            migrationBuilder.Sql("""
                ALTER TABLE workflow_step_roles
                DROP CONSTRAINT IF EXISTS "FK_workflow_step_roles_workflow_steps_workflow_step_id";
                """);

            migrationBuilder.Sql("""
                ALTER TABLE workflow_step_routes
                DROP CONSTRAINT IF EXISTS "FK_workflow_step_routes_workflow_steps_next_step_id";
                """);

            migrationBuilder.Sql("""
                ALTER TABLE workflow_step_routes
                DROP CONSTRAINT IF EXISTS "FK_workflow_step_routes_workflow_steps_workflow_step_id";
                """);

            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_workflow_step_roles_project_role_id";
                """);
        }
    }
}
