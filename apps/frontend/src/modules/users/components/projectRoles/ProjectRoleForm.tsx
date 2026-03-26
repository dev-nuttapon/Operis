import { Form, Input, InputNumber, Select } from "antd";
import type { FormInstance } from "antd";
import type { ProjectRole } from "../../types/users";
import type { useTranslation } from "react-i18next";

export type ProjectRoleFormValues = {
  projectId?: string;
  name: string;
  code?: string;
  status: string;
  description?: string;
  responsibilities?: string;
  authorityScope?: string;
  displayOrder: number;
};

export function toProjectRoleInitialValues(record: ProjectRole): ProjectRoleFormValues {
  return {
    projectId: record.projectId ?? undefined,
    name: record.name,
    code: record.code ?? undefined,
    status: record.status,
    description: record.description ?? undefined,
    responsibilities: record.responsibilities ?? undefined,
    authorityScope: record.authorityScope ?? undefined,
    displayOrder: record.displayOrder,
  };
}

export function ProjectRoleForm({
  form,
  t,
  projectOptions,
  projectOptionsLoading,
  onProjectSearch,
  onProjectLoadMore,
  hasMoreProjects,
}: {
  form: FormInstance<ProjectRoleFormValues>;
  t: ReturnType<typeof useTranslation>["t"];
  projectOptions: Array<{ label: string; value: string }>;
  projectOptionsLoading: boolean;
  onProjectSearch: (value: string) => void;
  onProjectLoadMore: () => void;
  hasMoreProjects: boolean;
}) {
  return (
    <Form
      form={form}
      layout="vertical"
      initialValues={{ status: "Active" }}
    >
      <Form.Item name="projectId" label={t("project_roles.fields.project")} rules={[{ required: true }]}>
        <Select
          showSearch
          filterOption={false}
          options={projectOptions}
          onSearch={onProjectSearch}
          loading={projectOptionsLoading}
          dropdownRender={(menu) => (
            <>
              {menu}
              {hasMoreProjects ? (
                <div style={{ padding: 8 }}>
                  <button
                    type="button"
                    onMouseDown={(event) => event.preventDefault()}
                    onClick={onProjectLoadMore}
                    style={{ width: "100%", border: "none", background: "transparent", color: "#1677ff", cursor: "pointer", padding: 4 }}
                  >
                    {t("projects.load_more_projects")}
                  </button>
                </div>
              ) : null}
            </>
          )}
          placeholder={t("project_roles.placeholders.project")}
        />
      </Form.Item>
      <Form.Item name="name" label={t("project_roles.fields.name")} rules={[{ required: true }]}>
        <Input placeholder={t("project_roles.placeholders.name")} />
      </Form.Item>
      <Form.Item name="code" label={t("project_roles.fields.code")}>
        <Input placeholder={t("project_roles.placeholders.code")} />
      </Form.Item>
      <Form.Item name="status" label={t("project_roles.fields.status")} rules={[{ required: true }]}>
        <Select
          options={[
            { label: t("project_roles.status.active"), value: "Active" },
            { label: t("project_roles.status.archived"), value: "Archived" },
          ]}
        />
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
      <Form.Item name="displayOrder" label={t("project_roles.fields.display_order")} rules={[{ required: true }]}>
        <InputNumber min={0} style={{ width: "100%" }} />
      </Form.Item>
    </Form>
  );
}
