import { useEffect, useMemo, useState } from "react";
import { App, Button, Card, Form, Input, InputNumber, Modal, Select, Space, Switch, Table, Tag, Typography, Skeleton, Flex, Grid } from "antd";
import type { ColumnsType } from "antd/es/table";
import { DeleteOutlined, EditOutlined, PlusOutlined, ProfileOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useProjectTemplates } from "../hooks/useProjectTemplates";
import type {
  CreateProjectTypeRoleRequirementInput,
  CreateProjectTypeTemplateInput,
  ProjectTypeRoleRequirement,
  ProjectTypeTemplate,
  SoftDeleteInput,
  UpdateProjectTypeRoleRequirementInput,
  UpdateProjectTypeTemplateInput,
} from "../types/users";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";

type TemplateFormValues = CreateProjectTypeTemplateInput;
type RequirementFormValues = CreateProjectTypeRoleRequirementInput;

const PROJECT_TYPE_OPTIONS = ["Internal", "Customer", "Compliance", "Improvement"].map((value) => ({ value, label: value }));

export function ProjectTypeTemplatesPage() {
  const { t } = useTranslation();
  const { notification } = App.useApp();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const permissionState = usePermissions();
  const canManageTemplates = permissionState.hasPermission(permissions.projects.manageTemplates);
  const [templatePaging, setTemplatePaging] = useState({ page: 1, pageSize: 10, search: "", sortBy: "projectType", sortOrder: "asc" as "asc" | "desc" });
  const [requirementPaging, setRequirementPaging] = useState({ page: 1, pageSize: 10, search: "", sortBy: "displayOrder", sortOrder: "asc" as "asc" | "desc" });
  const [templateSearchInput, setTemplateSearchInput] = useState("");
  const [selectedTemplate, setSelectedTemplate] = useState<ProjectTypeTemplate | null>(null);
  const [createTemplateOpen, setCreateTemplateOpen] = useState(false);
  const [editTemplateTarget, setEditTemplateTarget] = useState<ProjectTypeTemplate | null>(null);
  const [deleteTemplateTarget, setDeleteTemplateTarget] = useState<ProjectTypeTemplate | null>(null);
  const [createRequirementOpen, setCreateRequirementOpen] = useState(false);
  const [editRequirementTarget, setEditRequirementTarget] = useState<ProjectTypeRoleRequirement | null>(null);
  const [deleteRequirementTarget, setDeleteRequirementTarget] = useState<ProjectTypeRoleRequirement | null>(null);
  const [templateForm] = Form.useForm<TemplateFormValues>();
  const [editTemplateForm] = Form.useForm<TemplateFormValues>();
  const [templateDeleteForm] = Form.useForm<{ reason: string }>();
  const [requirementForm] = Form.useForm<RequirementFormValues>();
  const [editRequirementForm] = Form.useForm<RequirementFormValues>();
  const [requirementDeleteForm] = Form.useForm<{ reason: string }>();

  const debouncedTemplateSearch = useDebouncedValue(templateSearchInput, 300);
  const debouncedRequirementSearch = useDebouncedValue(requirementPaging.search, 300);

  useEffect(() => {
    setTemplatePaging((current) => ({ ...current, page: 1, search: debouncedTemplateSearch }));
  }, [debouncedTemplateSearch, setTemplatePaging]);
  const {
    templatesQuery,
    roleRequirementsQuery,
    createTemplateMutation,
    updateTemplateMutation,
    deleteTemplateMutation,
    createRoleRequirementMutation,
    updateRoleRequirementMutation,
    deleteRoleRequirementMutation,
  } = useProjectTemplates({
    templates: { ...templatePaging, search: debouncedTemplateSearch },
    roleRequirements: { ...requirementPaging, search: debouncedRequirementSearch, templateId: selectedTemplate?.id },
  });

  const handleError = (fallbackTitle: string, error: unknown) => {
    const presentation = getApiErrorPresentation(error, fallbackTitle);
    notification.error({ message: presentation.title, description: presentation.description });
  };

  const templateColumns: ColumnsType<ProjectTypeTemplate> = useMemo(
    () => [
      { title: t("project_type_templates.columns.project_type"), dataIndex: "projectType" },
      { title: t("project_type_templates.columns.sponsor"), dataIndex: "requireSponsor", render: (value: boolean) => (value ? t("common.actions.yes") : t("common.actions.no")) },
      { title: t("project_type_templates.columns.review"), dataIndex: "requireReviewer", render: (value: boolean) => (value ? t("common.actions.yes") : t("common.actions.no")) },
      { title: t("project_type_templates.columns.approve"), dataIndex: "requireApprover", render: (value: boolean) => (value ? t("common.actions.yes") : t("common.actions.no")) },
      {
        title: t("admin_users.columns.actions"),
        key: "actions",
        render: (_, record) => (
          <Space>
            <Button onClick={() => setSelectedTemplate(record)}>{t("common.actions.view")}</Button>
            {canManageTemplates ? (
              <>
                <Button icon={<EditOutlined />} onClick={() => { setEditTemplateTarget(record); editTemplateForm.setFieldsValue(record); }}>{t("common.actions.edit")}</Button>
                <Button danger icon={<DeleteOutlined />} onClick={() => { setDeleteTemplateTarget(record); templateDeleteForm.resetFields(); }}>{t("common.actions.delete")}</Button>
              </>
            ) : null}
          </Space>
        ),
      },
    ],
    [canManageTemplates, editTemplateForm, t, templateDeleteForm],
  );

  const requirementColumns: ColumnsType<ProjectTypeRoleRequirement> = useMemo(
    () => [
      { title: t("project_type_templates.role_requirements.columns.role_name"), dataIndex: "roleName" },
      { title: t("project_type_templates.role_requirements.columns.role_code"), dataIndex: "roleCode", render: (value: string | null) => value ?? "-" },
      { title: t("project_type_templates.role_requirements.columns.display_order"), dataIndex: "displayOrder" },
      {
        title: t("admin_users.columns.actions"),
        key: "actions",
        render: (_, record) => (
          <Space>
            {canManageTemplates ? (
              <>
                <Button icon={<EditOutlined />} onClick={() => { setEditRequirementTarget(record); editRequirementForm.setFieldsValue({ projectTypeTemplateId: record.projectTypeTemplateId, roleName: record.roleName, roleCode: record.roleCode ?? undefined, description: record.description ?? undefined, displayOrder: record.displayOrder }); }}>{t("common.actions.edit")}</Button>
                <Button danger icon={<DeleteOutlined />} onClick={() => { setDeleteRequirementTarget(record); requirementDeleteForm.resetFields(); }}>{t("common.actions.delete")}</Button>
              </>
            ) : null}
          </Space>
        ),
      },
    ],
    [canManageTemplates, editRequirementForm, requirementDeleteForm, t],
  );

  const saveTemplate = (values: TemplateFormValues, target?: ProjectTypeTemplate | null) => {
    const onSuccess = () => {
      setCreateTemplateOpen(false);
      setEditTemplateTarget(null);
      templateForm.resetFields();
      editTemplateForm.resetFields();
      notification.success({ message: target ? t("project_type_templates.messages.updated") : t("project_type_templates.messages.created") });
    };
    const onError = (error: unknown) => handleError(t("project_type_templates.messages.save_failed"), error);
    if (target) {
      updateTemplateMutation.mutate({ id: target.id, ...values } as UpdateProjectTypeTemplateInput, { onSuccess, onError });
    } else {
      createTemplateMutation.mutate(values, { onSuccess, onError });
    }
  };

  const saveRequirement = (values: RequirementFormValues, target?: ProjectTypeRoleRequirement | null) => {
    const onSuccess = () => {
      setCreateRequirementOpen(false);
      setEditRequirementTarget(null);
      requirementForm.resetFields();
      editRequirementForm.resetFields();
      notification.success({ message: target ? t("project_type_templates.role_requirements.messages.updated") : t("project_type_templates.role_requirements.messages.created") });
    };
    const onError = (error: unknown) => handleError(t("project_type_templates.role_requirements.messages.save_failed"), error);
    if (target) {
      updateRoleRequirementMutation.mutate({ id: target.id, ...values } as UpdateProjectTypeRoleRequirementInput, { onSuccess, onError });
    } else {
      createRoleRequirementMutation.mutate(values, { onSuccess, onError });
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)", color: "#fff" }}>
            <ProfileOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>{t("project_type_templates.page_title")}</Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>{t("project_type_templates.page_description")}</Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex
          gap={12}
          wrap={!isMobile}
          vertical={isMobile}
          align={isMobile ? "stretch" : "center"}
          justify="space-between"
          style={{ width: "100%", marginBottom: 16 }}
        >
          <Input.Search
            allowClear
            placeholder={t("project_type_templates.search_placeholder")}
            style={{ width: isMobile ? "100%" : undefined, maxWidth: isMobile ? undefined : 360 }}
            value={templateSearchInput}
            onChange={(event) => setTemplateSearchInput(event.target.value)}
            onSearch={(value) => setTemplateSearchInput(value)}
          />
          {canManageTemplates ? (
            <Button type="primary" icon={<PlusOutlined />} size="large" onClick={() => setCreateTemplateOpen(true)} block={isMobile}>
              {t("project_type_templates.create_action")}
            </Button>
          ) : null}
        </Flex>
        {templatesQuery.isLoading && (templatesQuery.data?.items?.length ?? 0) === 0 ? (
          <Skeleton active paragraph={{ rows: 6 }} />
        ) : (
          <Table
            rowKey="id"
            columns={templateColumns}
            dataSource={templatesQuery.data?.items ?? []}
            loading={templatesQuery.isLoading}
            onRow={(record) => ({ onClick: () => setSelectedTemplate(record) })}
            pagination={{ current: templatesQuery.data?.page ?? templatePaging.page, pageSize: templatesQuery.data?.pageSize ?? templatePaging.pageSize, total: templatesQuery.data?.total ?? 0, showSizeChanger: true, pageSizeOptions: [10,25,50,100], onChange: (page, pageSize) => setTemplatePaging((current) => ({ ...current, page, pageSize })) }}
          />
        )}
      </Card>

      <Card variant="borderless" title={t("project_type_templates.role_requirements.title")} extra={canManageTemplates ? <Button type="primary" onClick={() => {
        if (!selectedTemplate) return;
        requirementForm.setFieldsValue({ projectTypeTemplateId: selectedTemplate.id, displayOrder: 100 } as RequirementFormValues);
        setCreateRequirementOpen(true);
      }} disabled={!selectedTemplate}>{t("project_type_templates.role_requirements.create_action")}</Button> : null}>
        <Space direction="vertical" size={16} style={{ width: "100%" }}>
          <Select
            placeholder={t("project_type_templates.role_requirements.select_template")}
            value={selectedTemplate?.id}
            options={(templatesQuery.data?.items ?? []).map((item) => ({ label: item.projectType, value: item.id }))}
            onChange={(value) => {
              const next = (templatesQuery.data?.items ?? []).find((item) => item.id === value) ?? null;
              setSelectedTemplate(next);
            }}
          />
          {selectedTemplate ? <Tag color="blue">{selectedTemplate.projectType}</Tag> : null}
          {roleRequirementsQuery.isLoading && (roleRequirementsQuery.data?.items?.length ?? 0) === 0 ? (
            <Skeleton active paragraph={{ rows: 6 }} />
          ) : (
            <Table
              rowKey="id"
              columns={requirementColumns}
              dataSource={roleRequirementsQuery.data?.items ?? []}
              loading={roleRequirementsQuery.isLoading}
              pagination={{ current: roleRequirementsQuery.data?.page ?? requirementPaging.page, pageSize: roleRequirementsQuery.data?.pageSize ?? requirementPaging.pageSize, total: roleRequirementsQuery.data?.total ?? 0, showSizeChanger: true, pageSizeOptions: [10,25,50,100], onChange: (page, pageSize) => setRequirementPaging((current) => ({ ...current, page, pageSize })) }}
            />
          )}
        </Space>
      </Card>

      <Modal open={createTemplateOpen && canManageTemplates} title={t("project_type_templates.create_action")} onCancel={() => setCreateTemplateOpen(false)} onOk={() => templateForm.submit()} confirmLoading={createTemplateMutation.isPending}>
        <ProjectTypeTemplateForm form={templateForm} t={t} onFinish={(values) => saveTemplate(values)} />
      </Modal>

      <Modal open={Boolean(editTemplateTarget) && canManageTemplates} title={t("common.actions.edit")} onCancel={() => setEditTemplateTarget(null)} onOk={() => editTemplateForm.submit()} confirmLoading={updateTemplateMutation.isPending}>
        <ProjectTypeTemplateForm form={editTemplateForm} t={t} onFinish={(values) => saveTemplate(values, editTemplateTarget)} />
      </Modal>

      <Modal open={Boolean(deleteTemplateTarget) && canManageTemplates} title={t("common.actions.delete")} onCancel={() => setDeleteTemplateTarget(null)} onOk={() => templateDeleteForm.submit()} confirmLoading={deleteTemplateMutation.isPending}>
        <Form form={templateDeleteForm} layout="vertical" onFinish={(values: { reason: string }) => deleteTemplateMutation.mutate({ id: deleteTemplateTarget!.id, input: { reason: values.reason } as SoftDeleteInput }, { onSuccess: () => { setDeleteTemplateTarget(null); notification.success({ message: t("project_type_templates.messages.deleted") }); }, onError: (error) => handleError(t("project_type_templates.messages.delete_failed"), error) })}>
          <Form.Item name="reason" label={t("admin_users.fields.delete_reason")} rules={[{ required: true }]}><Input.TextArea rows={3} /></Form.Item>
        </Form>
      </Modal>

      <Modal open={createRequirementOpen && canManageTemplates} title={t("project_type_templates.role_requirements.create_action")} onCancel={() => setCreateRequirementOpen(false)} onOk={() => requirementForm.submit()} confirmLoading={createRoleRequirementMutation.isPending}>
        <ProjectTypeRoleRequirementForm form={requirementForm} t={t} selectedTemplate={selectedTemplate} onFinish={(values) => saveRequirement(values)} />
      </Modal>

      <Modal open={Boolean(editRequirementTarget) && canManageTemplates} title={t("common.actions.edit")} onCancel={() => setEditRequirementTarget(null)} onOk={() => editRequirementForm.submit()} confirmLoading={updateRoleRequirementMutation.isPending}>
        <ProjectTypeRoleRequirementForm form={editRequirementForm} t={t} selectedTemplate={selectedTemplate} onFinish={(values) => saveRequirement(values, editRequirementTarget)} />
      </Modal>

      <Modal open={Boolean(deleteRequirementTarget) && canManageTemplates} title={t("common.actions.delete")} onCancel={() => setDeleteRequirementTarget(null)} onOk={() => requirementDeleteForm.submit()} confirmLoading={deleteRoleRequirementMutation.isPending}>
        <Form form={requirementDeleteForm} layout="vertical" onFinish={(values: { reason: string }) => deleteRoleRequirementMutation.mutate({ id: deleteRequirementTarget!.id, input: { reason: values.reason } as SoftDeleteInput }, { onSuccess: () => { setDeleteRequirementTarget(null); notification.success({ message: t("project_type_templates.role_requirements.messages.deleted") }); }, onError: (error) => handleError(t("project_type_templates.role_requirements.messages.delete_failed"), error) })}>
          <Form.Item name="reason" label={t("admin_users.fields.delete_reason")} rules={[{ required: true }]}><Input.TextArea rows={3} /></Form.Item>
        </Form>
      </Modal>

    </Space>
  );
}

function ProjectTypeTemplateForm({ form, t, onFinish }: { form: ReturnType<typeof Form.useForm<TemplateFormValues>>[0]; t: (key: string, options?: Record<string, unknown>) => string; onFinish: (values: TemplateFormValues) => void }) {
  return (
    <Form form={form} layout="vertical" onFinish={onFinish} initialValues={{ requirePlannedPeriod: true, requireActiveTeam: true, requirePrimaryAssignment: true, requireReportingRoot: true, requireDocumentCreator: true, requireReviewer: true, requireApprover: true }}>
      <Form.Item name="projectType" label={t("project_type_templates.fields.project_type")} rules={[{ required: true }]}>
        <Select options={PROJECT_TYPE_OPTIONS} />
      </Form.Item>
      <Form.Item name="requireSponsor" label={t("project_type_templates.fields.require_sponsor")} valuePropName="checked"><Switch /></Form.Item>
      <Form.Item name="requirePlannedPeriod" label={t("project_type_templates.fields.require_planned_period")} valuePropName="checked"><Switch /></Form.Item>
      <Form.Item name="requireActiveTeam" label={t("project_type_templates.fields.require_active_team")} valuePropName="checked"><Switch /></Form.Item>
      <Form.Item name="requirePrimaryAssignment" label={t("project_type_templates.fields.require_primary_assignment")} valuePropName="checked"><Switch /></Form.Item>
      <Form.Item name="requireReportingRoot" label={t("project_type_templates.fields.require_reporting_root")} valuePropName="checked"><Switch /></Form.Item>
      <Form.Item name="requireDocumentCreator" label={t("project_type_templates.fields.require_document_creator")} valuePropName="checked"><Switch /></Form.Item>
      <Form.Item name="requireReviewer" label={t("project_type_templates.fields.require_reviewer")} valuePropName="checked"><Switch /></Form.Item>
      <Form.Item name="requireApprover" label={t("project_type_templates.fields.require_approver")} valuePropName="checked"><Switch /></Form.Item>
      <Form.Item name="requireReleaseRole" label={t("project_type_templates.fields.require_release_role")} valuePropName="checked"><Switch /></Form.Item>
    </Form>
  );
}

function ProjectTypeRoleRequirementForm({ form, t, selectedTemplate, onFinish }: { form: ReturnType<typeof Form.useForm<RequirementFormValues>>[0]; t: (key: string, options?: Record<string, unknown>) => string; selectedTemplate: ProjectTypeTemplate | null; onFinish: (values: RequirementFormValues) => void }) {
  return (
    <Form form={form} layout="vertical" onFinish={onFinish} initialValues={selectedTemplate ? { projectTypeTemplateId: selectedTemplate.id, displayOrder: 100 } : { displayOrder: 100 }}>
      <Form.Item name="projectTypeTemplateId" label={t("project_type_templates.role_requirements.fields.template")} rules={[{ required: true }]}>
        <Select options={selectedTemplate ? [{ label: selectedTemplate.projectType, value: selectedTemplate.id }] : []} />
      </Form.Item>
      <Form.Item name="roleName" label={t("project_type_templates.role_requirements.fields.role_name")} rules={[{ required: true }]}><Input /></Form.Item>
      <Form.Item name="roleCode" label={t("project_type_templates.role_requirements.fields.role_code")}><Input /></Form.Item>
      <Form.Item name="description" label={t("project_type_templates.role_requirements.fields.description")}><Input.TextArea rows={3} /></Form.Item>
      <Form.Item name="displayOrder" label={t("project_type_templates.role_requirements.fields.display_order")} rules={[{ required: true }]}><InputNumber min={1} style={{ width: "100%" }} /></Form.Item>
    </Form>
  );
}
