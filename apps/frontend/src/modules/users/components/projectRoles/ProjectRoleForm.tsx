import { Checkbox, Form, Input, InputNumber, Select, Space } from "antd";
import type { FormInstance } from "antd";
import type { ProjectRole } from "../../types/users";
import type { useTranslation } from "react-i18next";

export type ProjectRoleFormValues = {
  projectId: string;
  name: string;
  code?: string;
  description?: string;
  responsibilities?: string;
  authorityScope?: string;
  canCreateDocuments: boolean;
  canReviewDocuments: boolean;
  canApproveDocuments: boolean;
  canReleaseDocuments: boolean;
  isPeerReviewRole: boolean;
  isReviewRole: boolean;
  isApprovalRole: boolean;
  displayOrder: number;
};

export function toProjectRoleInitialValues(record: ProjectRole, projectId?: string): ProjectRoleFormValues {
  return {
    projectId: record.projectId ?? projectId ?? "",
    name: record.name,
    code: record.code ?? undefined,
    description: record.description ?? undefined,
    responsibilities: record.responsibilities ?? undefined,
    authorityScope: record.authorityScope ?? undefined,
    canCreateDocuments: record.canCreateDocuments,
    canReviewDocuments: record.canReviewDocuments,
    canApproveDocuments: record.canApproveDocuments,
    canReleaseDocuments: record.canReleaseDocuments,
    isPeerReviewRole: record.isPeerReviewRole ?? false,
    isReviewRole: record.isReviewRole,
    isApprovalRole: record.isApprovalRole,
    displayOrder: record.displayOrder,
  };
}

export function ProjectRoleForm({
  form,
  t,
  projectOptions,
}: {
  form: FormInstance<ProjectRoleFormValues>;
  t: ReturnType<typeof useTranslation>["t"];
  projectOptions: { label: string; value: string }[];
}) {
  return (
    <Form
      form={form}
      layout="vertical"
      initialValues={{
        isPeerReviewRole: false,
        isReviewRole: false,
        isApprovalRole: false,
        canCreateDocuments: false,
        canReviewDocuments: false,
        canApproveDocuments: false,
        canReleaseDocuments: false,
      }}
    >
      <Form.Item name="projectId" label={t("project_roles.fields.project")} rules={[{ required: true }]}>
        <Select options={projectOptions} />
      </Form.Item>
      <Form.Item name="name" label={t("project_roles.fields.name")} rules={[{ required: true }]}>
        <Input placeholder={t("project_roles.placeholders.name")} />
      </Form.Item>
      <Form.Item name="code" label={t("project_roles.fields.code")}>
        <Input placeholder={t("project_roles.placeholders.code")} />
      </Form.Item>
      <Form.Item name="description" label={t("project_roles.fields.description")}>
        <Input.TextArea rows={3} placeholder={t("project_roles.placeholders.description")} />
      </Form.Item>
      <Form.Item name="responsibilities" label={t("project_roles.fields.responsibilities")}>
        <Input.TextArea rows={4} placeholder={t("project_roles.placeholders.responsibilities")} />
      </Form.Item>
      <Form.Item name="authorityScope" label={t("project_roles.fields.authority_scope")}>
        <Input.TextArea rows={3} placeholder={t("project_roles.placeholders.authority_scope")} />
      </Form.Item>
      <Form.Item label={t("project_roles.fields.document_permissions")}>
        <Space direction="vertical">
          <Form.Item name="canCreateDocuments" valuePropName="checked" noStyle>
            <Checkbox>{t("project_roles.fields.can_create_documents")}</Checkbox>
          </Form.Item>
          <Form.Item name="canReviewDocuments" valuePropName="checked" noStyle>
            <Checkbox>{t("project_roles.fields.can_review_documents")}</Checkbox>
          </Form.Item>
          <Form.Item name="canApproveDocuments" valuePropName="checked" noStyle>
            <Checkbox>{t("project_roles.fields.can_approve_documents")}</Checkbox>
          </Form.Item>
          <Form.Item name="canReleaseDocuments" valuePropName="checked" noStyle>
            <Checkbox>{t("project_roles.fields.can_release_documents")}</Checkbox>
          </Form.Item>
        </Space>
      </Form.Item>
      <Form.Item name="displayOrder" label={t("project_roles.fields.display_order")} rules={[{ required: true }]}>
        <InputNumber min={0} style={{ width: "100%" }} />
      </Form.Item>
      <Form.Item name="isPeerReviewRole" valuePropName="checked">
        <Checkbox>{t("project_roles.fields.is_peer_review_role")}</Checkbox>
      </Form.Item>
      <Form.Item name="isReviewRole" valuePropName="checked">
        <Checkbox>{t("project_roles.fields.is_review_role")}</Checkbox>
      </Form.Item>
      <Form.Item name="isApprovalRole" valuePropName="checked">
        <Checkbox>{t("project_roles.fields.is_approval_role")}</Checkbox>
      </Form.Item>
    </Form>
  );
}
