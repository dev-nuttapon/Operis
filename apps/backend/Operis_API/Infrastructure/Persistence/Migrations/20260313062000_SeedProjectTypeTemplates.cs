using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedProjectTypeTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                DECLARE
                    internal_template_id uuid := '11111111-1111-1111-1111-111111111111';
                    customer_template_id uuid := '22222222-2222-2222-2222-222222222222';
                    compliance_template_id uuid := '33333333-3333-3333-3333-333333333333';
                    improvement_template_id uuid := '44444444-4444-4444-4444-444444444444';
                BEGIN
                    INSERT INTO project_type_templates (
                        id,
                        project_type,
                        require_sponsor,
                        require_planned_period,
                        require_active_team,
                        require_primary_assignment,
                        require_reporting_root,
                        require_document_creator,
                        require_reviewer,
                        require_approver,
                        require_release_role,
                        created_at,
                        updated_at,
                        deleted_reason,
                        deleted_by,
                        deleted_at
                    )
                    VALUES
                        (internal_template_id, 'Internal', false, true, true, true, true, true, true, true, false, NOW(), NULL, NULL, NULL, NULL),
                        (customer_template_id, 'Customer', true, true, true, true, true, true, true, true, true, NOW(), NULL, NULL, NULL, NULL),
                        (compliance_template_id, 'Compliance', true, true, true, true, true, true, true, true, true, NOW(), NULL, NULL, NULL, NULL),
                        (improvement_template_id, 'Improvement', false, true, true, true, true, true, true, false, false, NOW(), NULL, NULL, NULL, NULL)
                    ON CONFLICT DO NOTHING;

                    INSERT INTO project_type_role_requirements (
                        id,
                        project_type_template_id,
                        role_name,
                        role_code,
                        description,
                        display_order,
                        created_at,
                        updated_at,
                        deleted_reason,
                        deleted_by,
                        deleted_at
                    )
                    VALUES
                        ('11111111-aaaa-1111-aaaa-111111111111', internal_template_id, 'Project Manager', 'PM', 'Owns internal project delivery and coordination.', 1, NOW(), NULL, NULL, NULL, NULL),
                        ('11111111-bbbb-1111-bbbb-111111111111', internal_template_id, 'Reviewer', 'REVIEWER', 'Reviews internal project deliverables.', 2, NOW(), NULL, NULL, NULL, NULL),
                        ('11111111-cccc-1111-cccc-111111111111', internal_template_id, 'Approver', 'APPROVER', 'Approves internal project outputs.', 3, NOW(), NULL, NULL, NULL, NULL),
                        ('22222222-aaaa-2222-aaaa-222222222222', customer_template_id, 'Project Manager', 'PM', 'Owns customer-facing project execution.', 1, NOW(), NULL, NULL, NULL, NULL),
                        ('22222222-bbbb-2222-bbbb-222222222222', customer_template_id, 'Delivery Lead', 'DELIVERY_LEAD', 'Leads delivery planning and execution for the customer project.', 2, NOW(), NULL, NULL, NULL, NULL),
                        ('22222222-cccc-2222-cccc-222222222222', customer_template_id, 'Quality Reviewer', 'QA_REVIEW', 'Performs quality review before approval.', 3, NOW(), NULL, NULL, NULL, NULL),
                        ('22222222-dddd-2222-dddd-222222222222', customer_template_id, 'Release Approver', 'REL_APPROVER', 'Approves release to customer or production scope.', 4, NOW(), NULL, NULL, NULL, NULL),
                        ('33333333-aaaa-3333-aaaa-333333333333', compliance_template_id, 'Compliance Lead', 'COMPLIANCE_LEAD', 'Leads compliance implementation and evidence readiness.', 1, NOW(), NULL, NULL, NULL, NULL),
                        ('33333333-bbbb-3333-bbbb-333333333333', compliance_template_id, 'Quality Reviewer', 'QA_REVIEW', 'Reviews compliance deliverables and controls.', 2, NOW(), NULL, NULL, NULL, NULL),
                        ('33333333-cccc-3333-cccc-333333333333', compliance_template_id, 'Approval Authority', 'APPROVER', 'Approves compliance milestones and findings closure.', 3, NOW(), NULL, NULL, NULL, NULL),
                        ('33333333-dddd-3333-dddd-333333333333', compliance_template_id, 'Release Controller', 'RELEASE', 'Controls release of approved compliance artefacts.', 4, NOW(), NULL, NULL, NULL, NULL),
                        ('44444444-aaaa-4444-aaaa-444444444444', improvement_template_id, 'Improvement Lead', 'IMPROVEMENT_LEAD', 'Leads process or operational improvement work.', 1, NOW(), NULL, NULL, NULL, NULL),
                        ('44444444-bbbb-4444-bbbb-444444444444', improvement_template_id, 'Reviewer', 'REVIEWER', 'Reviews improvement outcomes.', 2, NOW(), NULL, NULL, NULL, NULL)
                    ON CONFLICT DO NOTHING;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM project_type_role_requirements
                WHERE project_type_template_id IN (
                    '11111111-1111-1111-1111-111111111111',
                    '22222222-2222-2222-2222-222222222222',
                    '33333333-3333-3333-3333-333333333333',
                    '44444444-4444-4444-4444-444444444444'
                );

                DELETE FROM project_type_templates
                WHERE id IN (
                    '11111111-1111-1111-1111-111111111111',
                    '22222222-2222-2222-2222-222222222222',
                    '33333333-3333-3333-3333-333333333333',
                    '44444444-4444-4444-4444-444444444444'
                );
                """);
        }
    }
}
