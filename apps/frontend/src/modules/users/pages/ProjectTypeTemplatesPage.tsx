import { useEffect, useMemo, useState } from "react";
import { App, Button, Card, Form, Input, Modal, Select, Space, Table, Tag, Typography, Skeleton, Flex, Grid } from "antd";
import type { ColumnsType } from "antd/es/table";
import { DeleteOutlined, EditOutlined, PlusOutlined, ProfileOutlined } from "@ant-design/icons";
import { useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useProjectTemplates } from "../hooks/useProjectTemplates";
import type { ProjectTypeRoleRequirement, ProjectTypeTemplate, SoftDeleteInput } from "../types/users";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";

export function ProjectTypeTemplatesPage() {
  const { t } = useTranslation();
  const { notification } = App.useApp();
  const navigate = useNavigate();
  const location = useLocation();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const permissionState = usePermissions();
  const canManageTemplates = permissionState.hasPermission(permissions.projects.manageTemplates);
  const [templatePaging, setTemplatePaging] = useState({ page: 1, pageSize: 10, search: "", sortBy: "projectType", sortOrder: "asc" as "asc" | "desc" });
  const [requirementPaging, setRequirementPaging] = useState({ page: 1, pageSize: 10, search: "", sortBy: "displayOrder", sortOrder: "asc" as "asc" | "desc" });
  const [templateSearchInput, setTemplateSearchInput] = useState("");
  const [selectedTemplate, setSelectedTemplate] = useState<ProjectTypeTemplate | null>(null);
  const [deleteTemplateTarget, setDeleteTemplateTarget] = useState<ProjectTypeTemplate | null>(null);
  const [deleteRequirementTarget, setDeleteRequirementTarget] = useState<ProjectTypeRoleRequirement | null>(null);
  const [templateDeleteForm] = Form.useForm<{ reason: string }>();
  const [requirementDeleteForm] = Form.useForm<{ reason: string }>();

  const debouncedTemplateSearch = useDebouncedValue(templateSearchInput, 300);
  const debouncedRequirementSearch = useDebouncedValue(requirementPaging.search, 300);

  useEffect(() => {
    setTemplatePaging((current) => ({ ...current, page: 1, search: debouncedTemplateSearch }));
  }, [debouncedTemplateSearch, setTemplatePaging]);
  const {
    templatesQuery,
    roleRequirementsQuery,
    deleteTemplateMutation,
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
                <Button
                  icon={<EditOutlined />}
                  onClick={() =>
                    navigate(`/app/admin/project-type-templates/${record.id}/edit`, {
                      state: { from: `${location.pathname}${location.search}` },
                    })
                  }
                >
                  {t("common.actions.edit")}
                </Button>
                <Button danger icon={<DeleteOutlined />} onClick={() => { setDeleteTemplateTarget(record); templateDeleteForm.resetFields(); }}>{t("common.actions.delete")}</Button>
              </>
            ) : null}
          </Space>
        ),
      },
    ],
    [canManageTemplates, location.pathname, location.search, navigate, t, templateDeleteForm],
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
                <Button
                  icon={<EditOutlined />}
                  onClick={() =>
                    navigate(`/app/admin/project-type-templates/${record.projectTypeTemplateId}/role-requirements/${record.id}/edit`, {
                      state: { from: `${location.pathname}${location.search}` },
                    })
                  }
                >
                  {t("common.actions.edit")}
                </Button>
                <Button danger icon={<DeleteOutlined />} onClick={() => { setDeleteRequirementTarget(record); requirementDeleteForm.resetFields(); }}>{t("common.actions.delete")}</Button>
              </>
            ) : null}
          </Space>
        ),
      },
    ],
    [canManageTemplates, location.pathname, location.search, navigate, requirementDeleteForm, t],
  );

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
            <Button
              type="primary"
              icon={<PlusOutlined />}
              size="large"
              onClick={() =>
                navigate("/app/admin/project-type-templates/new", {
                  state: { from: `${location.pathname}${location.search}` },
                })
              }
              block={isMobile}
            >
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
            scroll={{ x: "max-content" }}
            onRow={(record) => ({ onClick: () => setSelectedTemplate(record) })}
            pagination={{ current: templatesQuery.data?.page ?? templatePaging.page, pageSize: templatesQuery.data?.pageSize ?? templatePaging.pageSize, total: templatesQuery.data?.total ?? 0, showSizeChanger: true, pageSizeOptions: [10,25,50,100], onChange: (page, pageSize) => setTemplatePaging((current) => ({ ...current, page, pageSize })) }}
          />
        )}
      </Card>

      <Card
        variant="borderless"
        title={t("project_type_templates.role_requirements.title")}
        extra={
          canManageTemplates ? (
            <Button
              type="primary"
              onClick={() =>
                selectedTemplate
                  ? navigate(`/app/admin/project-type-templates/${selectedTemplate.id}/role-requirements/new`, {
                      state: { from: `${location.pathname}${location.search}` },
                    })
                  : undefined
              }
              disabled={!selectedTemplate}
            >
              {t("project_type_templates.role_requirements.create_action")}
            </Button>
          ) : null
        }
      >
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
              scroll={{ x: "max-content" }}
              pagination={{ current: roleRequirementsQuery.data?.page ?? requirementPaging.page, pageSize: roleRequirementsQuery.data?.pageSize ?? requirementPaging.pageSize, total: roleRequirementsQuery.data?.total ?? 0, showSizeChanger: true, pageSizeOptions: [10,25,50,100], onChange: (page, pageSize) => setRequirementPaging((current) => ({ ...current, page, pageSize })) }}
            />
          )}
        </Space>
      </Card>

      <Modal open={Boolean(deleteTemplateTarget) && canManageTemplates} title={t("common.actions.delete")} onCancel={() => setDeleteTemplateTarget(null)} onOk={() => templateDeleteForm.submit()} confirmLoading={deleteTemplateMutation.isPending}>
        <Form form={templateDeleteForm} layout="vertical" onFinish={(values: { reason: string }) => deleteTemplateMutation.mutate({ id: deleteTemplateTarget!.id, input: { reason: values.reason } as SoftDeleteInput }, { onSuccess: () => { setDeleteTemplateTarget(null); notification.success({ message: t("project_type_templates.messages.deleted") }); }, onError: (error) => handleError(t("project_type_templates.messages.delete_failed"), error) })}>
          <Form.Item name="reason" label={t("admin_users.fields.delete_reason")} rules={[{ required: true }]}><Input.TextArea rows={3} /></Form.Item>
        </Form>
      </Modal>

      <Modal open={Boolean(deleteRequirementTarget) && canManageTemplates} title={t("common.actions.delete")} onCancel={() => setDeleteRequirementTarget(null)} onOk={() => requirementDeleteForm.submit()} confirmLoading={deleteRoleRequirementMutation.isPending}>
        <Form form={requirementDeleteForm} layout="vertical" onFinish={(values: { reason: string }) => deleteRoleRequirementMutation.mutate({ id: deleteRequirementTarget!.id, input: { reason: values.reason } as SoftDeleteInput }, { onSuccess: () => { setDeleteRequirementTarget(null); notification.success({ message: t("project_type_templates.role_requirements.messages.deleted") }); }, onError: (error) => handleError(t("project_type_templates.role_requirements.messages.delete_failed"), error) })}>
          <Form.Item name="reason" label={t("admin_users.fields.delete_reason")} rules={[{ required: true }]}><Input.TextArea rows={3} /></Form.Item>
        </Form>
      </Modal>

    </Space>
  );
}
