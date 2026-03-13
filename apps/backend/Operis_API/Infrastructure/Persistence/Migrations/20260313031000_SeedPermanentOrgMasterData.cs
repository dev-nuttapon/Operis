using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Operis_API.Infrastructure.Persistence;

#nullable disable

namespace Operis_API.Infrastructure.Persistence.Migrations;

[DbContext(typeof(OperisDbContext))]
[Migration("20260313031000_SeedPermanentOrgMasterData")]
public partial class SeedPermanentOrgMasterData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DO $$
            BEGIN
                UPDATE divisions
                SET
                    display_order = 10,
                    deleted_reason = NULL,
                    deleted_by = NULL,
                    deleted_at = NULL
                WHERE name = 'Operations';

                IF NOT EXISTS (SELECT 1 FROM divisions WHERE name = 'Operations' AND deleted_at IS NULL) THEN
                    INSERT INTO divisions (id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('0f4ec2f3-19de-45a4-9953-4db2f72f0c11', 'Operations', 10, NOW(), NULL, NULL, NULL, NULL);
                END IF;

                UPDATE divisions
                SET
                    display_order = 20,
                    deleted_reason = NULL,
                    deleted_by = NULL,
                    deleted_at = NULL
                WHERE name = 'Finance';

                IF NOT EXISTS (SELECT 1 FROM divisions WHERE name = 'Finance' AND deleted_at IS NULL) THEN
                    INSERT INTO divisions (id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('6e4f9ab4-1969-4f66-87d6-eac7184ddb32', 'Finance', 20, NOW(), NULL, NULL, NULL, NULL);
                END IF;

                UPDATE divisions
                SET
                    display_order = 30,
                    deleted_reason = NULL,
                    deleted_by = NULL,
                    deleted_at = NULL
                WHERE name = 'Human Resources';

                IF NOT EXISTS (SELECT 1 FROM divisions WHERE name = 'Human Resources' AND deleted_at IS NULL) THEN
                    INSERT INTO divisions (id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('7ae4892f-6baa-4771-8d43-64f2dc2acf43', 'Human Resources', 30, NOW(), NULL, NULL, NULL, NULL);
                END IF;

                UPDATE divisions
                SET
                    display_order = 40,
                    deleted_reason = NULL,
                    deleted_by = NULL,
                    deleted_at = NULL
                WHERE name = 'Information Technology';

                IF NOT EXISTS (SELECT 1 FROM divisions WHERE name = 'Information Technology' AND deleted_at IS NULL) THEN
                    INSERT INTO divisions (id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('1ff98ca8-6d75-4531-876a-c749de6cd954', 'Information Technology', 40, NOW(), NULL, NULL, NULL, NULL);
                END IF;

                UPDATE divisions
                SET
                    display_order = 50,
                    deleted_reason = NULL,
                    deleted_by = NULL,
                    deleted_at = NULL
                WHERE name = 'Quality & Compliance';

                IF NOT EXISTS (SELECT 1 FROM divisions WHERE name = 'Quality & Compliance' AND deleted_at IS NULL) THEN
                    INSERT INTO divisions (id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('1d934dd3-6771-4d8d-b6c0-6d3934a0b665', 'Quality & Compliance', 50, NOW(), NULL, NULL, NULL, NULL);
                END IF;
            END
            $$;
            """);

        migrationBuilder.Sql(
            """
            DO $$
            DECLARE
                operations_id uuid := (SELECT id FROM divisions WHERE name = 'Operations' AND deleted_at IS NULL LIMIT 1);
                finance_id uuid := (SELECT id FROM divisions WHERE name = 'Finance' AND deleted_at IS NULL LIMIT 1);
                hr_id uuid := (SELECT id FROM divisions WHERE name = 'Human Resources' AND deleted_at IS NULL LIMIT 1);
                it_id uuid := (SELECT id FROM divisions WHERE name = 'Information Technology' AND deleted_at IS NULL LIMIT 1);
                qc_id uuid := (SELECT id FROM divisions WHERE name = 'Quality & Compliance' AND deleted_at IS NULL LIMIT 1);
            BEGIN
                UPDATE departments SET division_id = operations_id, display_order = 10, deleted_reason = NULL, deleted_by = NULL, deleted_at = NULL WHERE name = 'Document Control';
                IF NOT EXISTS (SELECT 1 FROM departments WHERE name = 'Document Control' AND deleted_at IS NULL) THEN
                    INSERT INTO departments (id, division_id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('2fd070d5-d2f6-42b4-9d77-8d05b778ef01', operations_id, 'Document Control', 10, NOW(), NULL, NULL, NULL, NULL);
                END IF;

                UPDATE departments SET division_id = operations_id, display_order = 20, deleted_reason = NULL, deleted_by = NULL, deleted_at = NULL WHERE name = 'Procurement';
                IF NOT EXISTS (SELECT 1 FROM departments WHERE name = 'Procurement' AND deleted_at IS NULL) THEN
                    INSERT INTO departments (id, division_id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('4cce0d22-a7be-45b3-9f54-8f3daf0179d2', operations_id, 'Procurement', 20, NOW(), NULL, NULL, NULL, NULL);
                END IF;

                UPDATE departments SET division_id = finance_id, display_order = 30, deleted_reason = NULL, deleted_by = NULL, deleted_at = NULL WHERE name = 'Accounting';
                IF NOT EXISTS (SELECT 1 FROM departments WHERE name = 'Accounting' AND deleted_at IS NULL) THEN
                    INSERT INTO departments (id, division_id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('ca5e1a88-8b6c-43db-a9cb-d8c6f140dfe3', finance_id, 'Accounting', 30, NOW(), NULL, NULL, NULL, NULL);
                END IF;

                UPDATE departments SET division_id = finance_id, display_order = 40, deleted_reason = NULL, deleted_by = NULL, deleted_at = NULL WHERE name = 'Budget Planning';
                IF NOT EXISTS (SELECT 1 FROM departments WHERE name = 'Budget Planning' AND deleted_at IS NULL) THEN
                    INSERT INTO departments (id, division_id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('9fbd28fb-a28b-42fb-b415-6550ae495894', finance_id, 'Budget Planning', 40, NOW(), NULL, NULL, NULL, NULL);
                END IF;

                UPDATE departments SET division_id = hr_id, display_order = 50, deleted_reason = NULL, deleted_by = NULL, deleted_at = NULL WHERE name = 'People Operations';
                IF NOT EXISTS (SELECT 1 FROM departments WHERE name = 'People Operations' AND deleted_at IS NULL) THEN
                    INSERT INTO departments (id, division_id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('7cf963d8-1a9f-40a1-b9a7-60094f4f6c25', hr_id, 'People Operations', 50, NOW(), NULL, NULL, NULL, NULL);
                END IF;

                UPDATE departments SET division_id = hr_id, display_order = 60, deleted_reason = NULL, deleted_by = NULL, deleted_at = NULL WHERE name = 'Talent Acquisition';
                IF NOT EXISTS (SELECT 1 FROM departments WHERE name = 'Talent Acquisition' AND deleted_at IS NULL) THEN
                    INSERT INTO departments (id, division_id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('1a482687-d8bc-490c-89ef-d007a065a9c6', hr_id, 'Talent Acquisition', 60, NOW(), NULL, NULL, NULL, NULL);
                END IF;

                UPDATE departments SET division_id = it_id, display_order = 70, deleted_reason = NULL, deleted_by = NULL, deleted_at = NULL WHERE name = 'Infrastructure & Support';
                IF NOT EXISTS (SELECT 1 FROM departments WHERE name = 'Infrastructure & Support' AND deleted_at IS NULL) THEN
                    INSERT INTO departments (id, division_id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('453f9412-64ca-46d1-a65f-4260d0b36a17', it_id, 'Infrastructure & Support', 70, NOW(), NULL, NULL, NULL, NULL);
                END IF;

                UPDATE departments SET division_id = it_id, display_order = 80, deleted_reason = NULL, deleted_by = NULL, deleted_at = NULL WHERE name = 'Application Development';
                IF NOT EXISTS (SELECT 1 FROM departments WHERE name = 'Application Development' AND deleted_at IS NULL) THEN
                    INSERT INTO departments (id, division_id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('cc277432-e13b-4c18-90b3-59693f18dd88', it_id, 'Application Development', 80, NOW(), NULL, NULL, NULL, NULL);
                END IF;

                UPDATE departments SET division_id = qc_id, display_order = 90, deleted_reason = NULL, deleted_by = NULL, deleted_at = NULL WHERE name = 'Internal Audit';
                IF NOT EXISTS (SELECT 1 FROM departments WHERE name = 'Internal Audit' AND deleted_at IS NULL) THEN
                    INSERT INTO departments (id, division_id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('246f95f8-e38d-4273-85dd-f1a4f9580499', qc_id, 'Internal Audit', 90, NOW(), NULL, NULL, NULL, NULL);
                END IF;

                UPDATE departments SET division_id = qc_id, display_order = 100, deleted_reason = NULL, deleted_by = NULL, deleted_at = NULL WHERE name = 'Compliance Management';
                IF NOT EXISTS (SELECT 1 FROM departments WHERE name = 'Compliance Management' AND deleted_at IS NULL) THEN
                    INSERT INTO departments (id, division_id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('b70d2d25-cb93-4782-ab6d-8e5b2405070a', qc_id, 'Compliance Management', 100, NOW(), NULL, NULL, NULL, NULL);
                END IF;
            END
            $$;
            """);

        migrationBuilder.Sql(
            """
            DO $$
            DECLARE
                document_control_id uuid := (SELECT id FROM departments WHERE name = 'Document Control' AND deleted_at IS NULL LIMIT 1);
                procurement_id uuid := (SELECT id FROM departments WHERE name = 'Procurement' AND deleted_at IS NULL LIMIT 1);
                accounting_id uuid := (SELECT id FROM departments WHERE name = 'Accounting' AND deleted_at IS NULL LIMIT 1);
                budget_id uuid := (SELECT id FROM departments WHERE name = 'Budget Planning' AND deleted_at IS NULL LIMIT 1);
                people_ops_id uuid := (SELECT id FROM departments WHERE name = 'People Operations' AND deleted_at IS NULL LIMIT 1);
                talent_id uuid := (SELECT id FROM departments WHERE name = 'Talent Acquisition' AND deleted_at IS NULL LIMIT 1);
                infra_id uuid := (SELECT id FROM departments WHERE name = 'Infrastructure & Support' AND deleted_at IS NULL LIMIT 1);
                app_dev_id uuid := (SELECT id FROM departments WHERE name = 'Application Development' AND deleted_at IS NULL LIMIT 1);
                audit_id uuid := (SELECT id FROM departments WHERE name = 'Internal Audit' AND deleted_at IS NULL LIMIT 1);
                compliance_id uuid := (SELECT id FROM departments WHERE name = 'Compliance Management' AND deleted_at IS NULL LIMIT 1);
            BEGIN
                UPDATE job_titles SET department_id = document_control_id, display_order = 10, deleted_reason = NULL, deleted_by = NULL, deleted_at = NULL WHERE name = 'Document Controller';
                IF NOT EXISTS (SELECT 1 FROM job_titles WHERE name = 'Document Controller' AND deleted_at IS NULL) THEN
                    INSERT INTO job_titles (id, department_id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('866da8d5-cc62-4f7b-bda5-148fb4c63691', document_control_id, 'Document Controller', 10, NOW(), NULL, NULL, NULL, NULL);
                END IF;

                UPDATE job_titles SET department_id = procurement_id, display_order = 20, deleted_reason = NULL, deleted_by = NULL, deleted_at = NULL WHERE name = 'Procurement Officer';
                IF NOT EXISTS (SELECT 1 FROM job_titles WHERE name = 'Procurement Officer' AND deleted_at IS NULL) THEN
                    INSERT INTO job_titles (id, department_id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('bf20a56f-468a-4657-bfa3-f4f73cc3a992', procurement_id, 'Procurement Officer', 20, NOW(), NULL, NULL, NULL, NULL);
                END IF;

                UPDATE job_titles SET department_id = accounting_id, display_order = 30, deleted_reason = NULL, deleted_by = NULL, deleted_at = NULL WHERE name = 'Accountant';
                IF NOT EXISTS (SELECT 1 FROM job_titles WHERE name = 'Accountant' AND deleted_at IS NULL) THEN
                    INSERT INTO job_titles (id, department_id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('c55e475f-3e22-4770-8dfa-0dfb7140ca13', accounting_id, 'Accountant', 30, NOW(), NULL, NULL, NULL, NULL);
                END IF;

                UPDATE job_titles SET department_id = budget_id, display_order = 40, deleted_reason = NULL, deleted_by = NULL, deleted_at = NULL WHERE name = 'Financial Analyst';
                IF NOT EXISTS (SELECT 1 FROM job_titles WHERE name = 'Financial Analyst' AND deleted_at IS NULL) THEN
                    INSERT INTO job_titles (id, department_id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('b484fc3f-1840-4065-96d1-82f39b55c544', budget_id, 'Financial Analyst', 40, NOW(), NULL, NULL, NULL, NULL);
                END IF;

                UPDATE job_titles SET department_id = people_ops_id, display_order = 50, deleted_reason = NULL, deleted_by = NULL, deleted_at = NULL WHERE name = 'HR Officer';
                IF NOT EXISTS (SELECT 1 FROM job_titles WHERE name = 'HR Officer' AND deleted_at IS NULL) THEN
                    INSERT INTO job_titles (id, department_id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('8d3f2f1f-a1a9-4188-8ec8-909c43f30c85', people_ops_id, 'HR Officer', 50, NOW(), NULL, NULL, NULL, NULL);
                END IF;

                UPDATE job_titles SET department_id = talent_id, display_order = 60, deleted_reason = NULL, deleted_by = NULL, deleted_at = NULL WHERE name = 'Recruiter';
                IF NOT EXISTS (SELECT 1 FROM job_titles WHERE name = 'Recruiter' AND deleted_at IS NULL) THEN
                    INSERT INTO job_titles (id, department_id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('d9f37c9b-5d4d-4237-aedf-692260610386', talent_id, 'Recruiter', 60, NOW(), NULL, NULL, NULL, NULL);
                END IF;

                UPDATE job_titles SET department_id = infra_id, display_order = 70, deleted_reason = NULL, deleted_by = NULL, deleted_at = NULL WHERE name = 'IT Support Specialist';
                IF NOT EXISTS (SELECT 1 FROM job_titles WHERE name = 'IT Support Specialist' AND deleted_at IS NULL) THEN
                    INSERT INTO job_titles (id, department_id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('5f17af1d-18cb-4dd1-bf01-70d308042bc7', infra_id, 'IT Support Specialist', 70, NOW(), NULL, NULL, NULL, NULL);
                END IF;

                UPDATE job_titles SET department_id = app_dev_id, display_order = 80, deleted_reason = NULL, deleted_by = NULL, deleted_at = NULL WHERE name = 'Software Engineer';
                IF NOT EXISTS (SELECT 1 FROM job_titles WHERE name = 'Software Engineer' AND deleted_at IS NULL) THEN
                    INSERT INTO job_titles (id, department_id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('e43a0dc1-a466-41bb-a619-c0f62006fcd8', app_dev_id, 'Software Engineer', 80, NOW(), NULL, NULL, NULL, NULL);
                END IF;

                UPDATE job_titles SET department_id = audit_id, display_order = 90, deleted_reason = NULL, deleted_by = NULL, deleted_at = NULL WHERE name = 'Internal Auditor';
                IF NOT EXISTS (SELECT 1 FROM job_titles WHERE name = 'Internal Auditor' AND deleted_at IS NULL) THEN
                    INSERT INTO job_titles (id, department_id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('5f1af090-09c7-4f48-bb68-5ca2b49cca29', audit_id, 'Internal Auditor', 90, NOW(), NULL, NULL, NULL, NULL);
                END IF;

                UPDATE job_titles SET department_id = compliance_id, display_order = 100, deleted_reason = NULL, deleted_by = NULL, deleted_at = NULL WHERE name = 'Compliance Officer';
                IF NOT EXISTS (SELECT 1 FROM job_titles WHERE name = 'Compliance Officer' AND deleted_at IS NULL) THEN
                    INSERT INTO job_titles (id, department_id, name, display_order, created_at, updated_at, deleted_reason, deleted_by, deleted_at)
                    VALUES ('9eff5780-41c2-4544-b48c-c5c7192a6d2a', compliance_id, 'Compliance Officer', 100, NOW(), NULL, NULL, NULL, NULL);
                END IF;
            END
            $$;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM job_titles WHERE id IN (
                '866da8d5-cc62-4f7b-bda5-148fb4c63691',
                'bf20a56f-468a-4657-bfa3-f4f73cc3a992',
                'c55e475f-3e22-4770-8dfa-0dfb7140ca13',
                'b484fc3f-1840-4065-96d1-82f39b55c544',
                '8d3f2f1f-a1a9-4188-8ec8-909c43f30c85',
                'd9f37c9b-5d4d-4237-aedf-692260610386',
                '5f17af1d-18cb-4dd1-bf01-70d308042bc7',
                'e43a0dc1-a466-41bb-a619-c0f62006fcd8',
                '5f1af090-09c7-4f48-bb68-5ca2b49cca29',
                '9eff5780-41c2-4544-b48c-c5c7192a6d2a'
            );

            DELETE FROM departments WHERE id IN (
                '2fd070d5-d2f6-42b4-9d77-8d05b778ef01',
                '4cce0d22-a7be-45b3-9f54-8f3daf0179d2',
                'ca5e1a88-8b6c-43db-a9cb-d8c6f140dfe3',
                '9fbd28fb-a28b-42fb-b415-6550ae495894',
                '7cf963d8-1a9f-40a1-b9a7-60094f4f6c25',
                '1a482687-d8bc-490c-89ef-d007a065a9c6',
                '453f9412-64ca-46d1-a65f-4260d0b36a17',
                'cc277432-e13b-4c18-90b3-59693f18dd88',
                '246f95f8-e38d-4273-85dd-f1a4f9580499',
                'b70d2d25-cb93-4782-ab6d-8e5b2405070a'
            );

            DELETE FROM divisions WHERE id IN (
                '0f4ec2f3-19de-45a4-9953-4db2f72f0c11',
                '6e4f9ab4-1969-4f66-87d6-eac7184ddb32',
                '7ae4892f-6baa-4771-8d43-64f2dc2acf43',
                '1ff98ca8-6d75-4531-876a-c749de6cd954',
                '1d934dd3-6771-4d8d-b6c0-6d3934a0b665'
            );
            """);
    }
}
