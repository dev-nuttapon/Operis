import { Form, Input, InputNumber, Select } from "antd";
import type { TFunction } from "i18next";
import type { CreateProjectTypeRoleRequirementInput, ProjectTypeTemplate } from "../../types/users";

export function ProjectTypeRoleRequirementForm({
  form,
  t,
  selectedTemplate,
  onFinish,
  templateOptions,
  disableTemplateSelect,
}: {
  form: ReturnType<typeof Form.useForm<CreateProjectTypeRoleRequirementInput>>[0];
  t: TFunction;
  selectedTemplate?: ProjectTypeTemplate | null;
  onFinish: (values: CreateProjectTypeRoleRequirementInput) => void;
  templateOptions?: { label: string; value: string }[];
  disableTemplateSelect?: boolean;
}) {
  const templateSelectOptions =
    templateOptions ??
    (selectedTemplate ? [{ label: selectedTemplate.projectType, value: selectedTemplate.id }] : []);

  return (
    <Form
      form={form}
      layout="vertical"
      onFinish={onFinish}
      initialValues={selectedTemplate ? { projectTypeTemplateId: selectedTemplate.id, displayOrder: 100 } : { displayOrder: 100 }}
    >
      <Form.Item name="projectTypeTemplateId" label={t("project_type_templates.role_requirements.fields.template")} rules={[{ required: true }]}>
        <Select options={templateSelectOptions} disabled={disableTemplateSelect} />
      </Form.Item>
      <Form.Item name="roleName" label={t("project_type_templates.role_requirements.fields.role_name")} rules={[{ required: true }]}>
        <Input />
      </Form.Item>
      <Form.Item name="roleCode" label={t("project_type_templates.role_requirements.fields.role_code")}>
        <Input />
      </Form.Item>
      <Form.Item name="description" label={t("project_type_templates.role_requirements.fields.description")}>
        <Input.TextArea rows={3} />
      </Form.Item>
      <Form.Item name="displayOrder" label={t("project_type_templates.role_requirements.fields.display_order")} rules={[{ required: true }]}>
        <InputNumber min={1} style={{ width: "100%" }} />
      </Form.Item>
    </Form>
  );
}
