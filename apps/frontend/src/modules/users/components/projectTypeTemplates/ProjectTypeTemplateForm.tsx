import { Form, Select, Switch } from "antd";
import type { TFunction } from "i18next";
import type { CreateProjectTypeTemplateInput } from "../../types/users";

const PROJECT_TYPE_OPTIONS = ["Internal", "Customer", "Compliance", "Improvement"].map((value) => ({ value, label: value }));

export function ProjectTypeTemplateForm({
  form,
  t,
  onFinish,
}: {
  form: ReturnType<typeof Form.useForm<CreateProjectTypeTemplateInput>>[0];
  t: TFunction;
  onFinish: (values: CreateProjectTypeTemplateInput) => void;
}) {
  return (
    <Form
      form={form}
      layout="vertical"
      onFinish={onFinish}
      initialValues={{
        requirePlannedPeriod: true,
        requireActiveTeam: true,
        requirePrimaryAssignment: true,
        requireReportingRoot: true,
        requireDocumentCreator: true,
        requireReviewer: true,
        requireApprover: true,
      }}
    >
      <Form.Item name="projectType" label={t("project_type_templates.fields.project_type")} rules={[{ required: true }]}>
        <Select options={PROJECT_TYPE_OPTIONS} />
      </Form.Item>
      <Form.Item name="requireSponsor" label={t("project_type_templates.fields.require_sponsor")} valuePropName="checked">
        <Switch />
      </Form.Item>
      <Form.Item name="requirePlannedPeriod" label={t("project_type_templates.fields.require_planned_period")} valuePropName="checked">
        <Switch />
      </Form.Item>
      <Form.Item name="requireActiveTeam" label={t("project_type_templates.fields.require_active_team")} valuePropName="checked">
        <Switch />
      </Form.Item>
      <Form.Item name="requirePrimaryAssignment" label={t("project_type_templates.fields.require_primary_assignment")} valuePropName="checked">
        <Switch />
      </Form.Item>
      <Form.Item name="requireReportingRoot" label={t("project_type_templates.fields.require_reporting_root")} valuePropName="checked">
        <Switch />
      </Form.Item>
      <Form.Item name="requireDocumentCreator" label={t("project_type_templates.fields.require_document_creator")} valuePropName="checked">
        <Switch />
      </Form.Item>
      <Form.Item name="requireReviewer" label={t("project_type_templates.fields.require_reviewer")} valuePropName="checked">
        <Switch />
      </Form.Item>
      <Form.Item name="requireApprover" label={t("project_type_templates.fields.require_approver")} valuePropName="checked">
        <Switch />
      </Form.Item>
      <Form.Item name="requireReleaseRole" label={t("project_type_templates.fields.require_release_role")} valuePropName="checked">
        <Switch />
      </Form.Item>
    </Form>
  );
}
