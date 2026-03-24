import { useEffect, useMemo, useState } from "react";
import { App, Alert, Button, Card, Flex, Form, Input, Select, Space, Switch, Table, Typography, Grid, Skeleton, InputNumber } from "antd";
import { ArrowLeftOutlined, DeleteOutlined, SaveOutlined, EditOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router-dom";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";
import { useProjectRoleOptions } from "../../users";
import { useWorkflowDefinition } from "../hooks/useWorkflowDefinition";
import { useUpdateWorkflowDefinition } from "../hooks/useUpdateWorkflowDefinition";
import type { WorkflowStep, WorkflowStepType } from "../types/workflows";
import { useDocumentTemplate, useDocumentTemplates, useDocumentsByIds } from "../../documents";

type StepDraft = {
  name: string;
  stepType: WorkflowStepType;
  roleIds: string[];
  isRequired: boolean;
  documentId?: string | null;
  minApprovals?: number;
};

export function WorkflowDefinitionEditPage() {
  const { t } = useTranslation();
  const { notification, modal } = App.useApp();
  const navigate = useNavigate();
  const { workflowDefinitionId } = useParams<{ workflowDefinitionId: string }>();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const permissionState = usePermissions();
  const canManage = permissionState.hasPermission(permissions.workflows.manageDefinitions);
  const canReadRoles = permissionState.hasPermission(permissions.projects.read);
  const definitionQuery = useWorkflowDefinition(workflowDefinitionId ?? null, Boolean(workflowDefinitionId));
  const updateMutation = useUpdateWorkflowDefinition();
  const [form] = Form.useForm<{ name: string }>();
  const [stepForm] = Form.useForm<StepDraft>();
  const [steps, setSteps] = useState<WorkflowStep[]>([]);
  const [editingIndex, setEditingIndex] = useState<number | null>(null);
  const [selectedTemplateId, setSelectedTemplateId] = useState<string | null>(null);
  const [templateSearch, setTemplateSearch] = useState("");
  const debouncedTemplateSearch = useDebouncedValue(templateSearch, 300);

  const roleOptionsState = useProjectRoleOptions({ enabled: canReadRoles });
  const roleLabelById = useMemo(
    () => new Map(roleOptionsState.items.map((item) => [item.id, item.name] as const)),
    [roleOptionsState.items],
  );

  const stepTypeOptions = useMemo(
    () => [
      { value: "submit", label: t("workflow_definitions.steps.types.submit") },
      { value: "peer_review", label: t("workflow_definitions.steps.types.peer_review") },
      { value: "review", label: t("workflow_definitions.steps.types.review") },
      { value: "approve", label: t("workflow_definitions.steps.types.approve") },
    ],
    [t],
  );

  const templateListState = useDocumentTemplates(
    { page: 1, pageSize: 25, search: debouncedTemplateSearch || undefined },
    canManage,
  );
  const templateDetailState = useDocumentTemplate(selectedTemplateId, Boolean(selectedTemplateId));
  const templateItems = templateDetailState.data?.items ?? [];
  const templateDocumentIds =
    (templateDetailState.data?.documentIds
      ?? templateItems.map((item) => item.documentId)
      ?? (templateDetailState.data as { DocumentIds?: string[] } | null)?.DocumentIds
      ?? []) as string[];
  const templateDocumentsState = useDocumentsByIds(
    templateDocumentIds,
    Boolean(templateDocumentIds.length),
  );
  const formatPublishedVersion = useMemo(
    () => (doc: { publishedVersionCode?: string | null; publishedRevision?: number | null }) =>
      doc.publishedVersionCode
        ? `${doc.publishedVersionCode}${doc.publishedRevision ? ` r${doc.publishedRevision}` : ""}`
        : t("workflow_definitions.steps.document_unpublished"),
    [t],
  );
  const baseDocumentOptions = useMemo(
    () => (templateDocumentsState.data ?? []).map((doc) => ({ label: doc.documentName, value: doc.id })),
    [templateDocumentsState.data],
  );
  const selectedDocumentIds = useMemo(
    () => new Set(steps.map((step) => step.documentId).filter((value): value is string => Boolean(value))),
    [steps],
  );
  const editingDocumentId = editingIndex !== null ? steps[editingIndex]?.documentId ?? null : null;
  const documentOptions = useMemo(
    () =>
      baseDocumentOptions.filter((option) => option.value === editingDocumentId || !selectedDocumentIds.has(option.value)),
    [baseDocumentOptions, editingDocumentId, selectedDocumentIds],
  );
  const documentLabelById = useMemo(
    () => new Map((templateDocumentsState.data ?? []).map((doc) => [doc.id, doc.documentName] as const)),
    [templateDocumentsState.data],
  );
  const documentPublishedById = useMemo(() => {
    if (templateItems.length > 0) {
      return new Map(
        templateItems.map((item) => [
          item.documentId,
          item.versionCode
            ? `${item.versionCode}${item.revision ? ` r${item.revision}` : ""}`
            : t("workflow_definitions.steps.document_unpublished"),
        ] as const),
      );
    }
    return new Map((templateDocumentsState.data ?? []).map((doc) => [doc.id, formatPublishedVersion(doc)] as const));
  }, [formatPublishedVersion, templateDocumentsState.data, templateItems, t]);

  useEffect(() => {
    if (!definitionQuery.data) return;
    form.setFieldsValue({ name: definitionQuery.data.name });
    setSelectedTemplateId(definitionQuery.data.documentTemplateId ?? null);
    setSteps(
      definitionQuery.data.steps
        .slice()
        .sort((a, b) => a.displayOrder - b.displayOrder)
        .map((step, index) => ({
          ...step,
          displayOrder: index + 1,
        })),
    );
  }, [definitionQuery.data, form]);

  const handleAddStep = async () => {
    const values = await stepForm.validateFields();
    const minApprovals = Math.max(1, Number(values.minApprovals ?? 1));
    if (editingIndex !== null) {
      setSteps((current) =>
        current.map((step, index) =>
          index === editingIndex
            ? {
                ...step,
                name: values.name.trim(),
                stepType: values.stepType,
                roleIds: values.roleIds,
                isRequired: values.isRequired,
                documentId: values.documentId ?? null,
                minApprovals,
                routes: step.routes?.map((route) => ({
                  ...route,
                  action: values.stepType,
                })),
              }
            : step,
        ),
      );
      setEditingIndex(null);
      stepForm.resetFields();
      return;
    }

    const nextOrder = steps.length + 1;
    setSteps((current) => [
      ...current,
      {
        name: values.name.trim(),
        stepType: values.stepType,
        roleIds: values.roleIds,
        isRequired: values.isRequired,
        displayOrder: nextOrder,
        documentId: values.documentId ?? null,
        minApprovals,
        routes: [],
      },
    ]);
    stepForm.resetFields();
  };

  const handleRemoveStep = (index: number) => {
    setSteps((current) => {
      const remaining = current.filter((_, idx) => idx !== index);
      const orderMap = new Map<number, number>();
      remaining.forEach((step, idx) => orderMap.set(step.displayOrder, idx + 1));
      return remaining.map((step, idx) => ({
        ...step,
        displayOrder: idx + 1,
        routes: step.routes?.map((route) => ({
          ...route,
          nextDisplayOrder: route.nextDisplayOrder ? orderMap.get(route.nextDisplayOrder) ?? null : null,
        })),
      }));
    });
    if (editingIndex === index) {
      setEditingIndex(null);
      stepForm.resetFields();
    }
  };

  const handleSubmit = async () => {
    if (!workflowDefinitionId) return;
    const values = await form.validateFields();
    if (steps.length === 0) {
      notification.error({ message: t("workflow_definitions.steps.validation.required") });
      return;
    }

    updateMutation.mutate(
      { workflowDefinitionId, name: values.name.trim(), documentTemplateId: selectedTemplateId, steps },
      {
        onSuccess: () => {
          notification.success({ message: t("workflow_definitions.notifications.updated") });
          navigate("/app/steps");
        },
        onError: (error) => {
          const presentation = getApiErrorPresentation(error, t("workflow_definitions.notifications.update_failed_title"));
          notification.error({ message: presentation.title, description: presentation.description });
        },
      },
    );
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Button icon={<ArrowLeftOutlined />} onClick={() => navigate("/app/steps")} block={isMobile}>
        {t("workflow_definitions.actions.back")}
      </Button>

      <Card variant="borderless">
        <Space align="start" size={16}>
          <div
            style={{
              width: 48,
              height: 48,
              borderRadius: 14,
              display: "grid",
              placeItems: "center",
              background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)",
              color: "#fff",
            }}
          >
            <SaveOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("workflow_definitions.edit_page.title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("workflow_definitions.edit_page.description")}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {!canManage ? (
          <Alert type="warning" showIcon message={t("errors.title_forbidden")} />
        ) : definitionQuery.isLoading ? (
          <Skeleton active paragraph={{ rows: 6 }} />
        ) : !definitionQuery.data ? (
          <Alert type="error" showIcon message={t("errors.title_not_found")} />
        ) : (
          <>
            <Form form={form} layout="vertical">
              <Form.Item
                label={t("workflow_definitions.form.name")}
                name="name"
                rules={[
                  { required: true, message: t("workflow_definitions.validation.name_required") },
                  { max: 200, message: t("workflow_definitions.validation.name_max_length") },
                ]}
              >
                <Input placeholder={t("workflow_definitions.placeholders.name")} />
              </Form.Item>
              <Form.Item label={t("workflow_definitions.form.template_label")}>
                <Select
                  allowClear
                  showSearch
                  filterOption={false}
                  placeholder={t("workflow_definitions.form.template_placeholder")}
                  options={(templateListState.data?.items ?? []).map((item) => ({ label: item.name, value: item.id }))}
                  value={selectedTemplateId}
                  loading={templateListState.isLoading}
                  onSearch={setTemplateSearch}
                  onChange={(value) => setSelectedTemplateId(value ?? null)}
                  notFoundContent={<Typography.Text type="secondary">{t("workflow_definitions.form.no_templates")}</Typography.Text>}
                />
              </Form.Item>
            </Form>

            <Typography.Title level={5} style={{ marginBottom: 12 }}>
              {t("workflow_definitions.steps.title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ marginTop: 0 }}>
              {t("workflow_definitions.steps.description")}
            </Typography.Paragraph>

            <Form form={stepForm} layout="vertical" initialValues={{ stepType: "submit", isRequired: true, minApprovals: 1 }}>
              <Flex gap={12} wrap={!isMobile} vertical={isMobile} align={isMobile ? "stretch" : "center"}>
                <Form.Item
                  label={t("workflow_definitions.steps.fields.name")}
                  name="name"
                  rules={[{ required: true, message: t("workflow_definitions.steps.validation.name_required") }]}
                  style={{ flex: 1, minWidth: isMobile ? "100%" : 200 }}
                >
                  <Input placeholder={t("workflow_definitions.steps.placeholders.name")} />
                </Form.Item>
                <Form.Item
                  label={t("workflow_definitions.steps.fields.type")}
                  name="stepType"
                  rules={[{ required: true, message: t("workflow_definitions.steps.validation.type_required") }]}
                  style={{ flex: 1, minWidth: isMobile ? "100%" : 180 }}
                >
                  <Select options={stepTypeOptions} />
                </Form.Item>
                <Form.Item
                  label={t("workflow_definitions.steps.fields.document")}
                  name="documentId"
                  rules={[
                    {
                      validator: async (_, value) => {
                        if (selectedTemplateId && !value) {
                          throw new Error(t("workflow_definitions.steps.validation.document_required"));
                        }
                      },
                    },
                  ]}
                  style={{ flex: 2, minWidth: isMobile ? "100%" : 240 }}
                >
                  <Select
                    allowClear
                    showSearch
                    options={documentOptions}
                    placeholder={t("workflow_definitions.steps.placeholders.document")}
                    disabled={!selectedTemplateId}
                    loading={templateDocumentsState.isLoading}
                  />
                </Form.Item>
                <Form.Item
                  label={t("workflow_definitions.steps.fields.min_approvals")}
                  name="minApprovals"
                  rules={[{ required: true, message: t("workflow_definitions.steps.validation.min_approvals_required") }]}
                  style={{ flex: 1, minWidth: isMobile ? "100%" : 140 }}
                >
                  <InputNumber
                    min={1}
                    placeholder={t("workflow_definitions.steps.placeholders.min_approvals")}
                    style={{ width: "100%" }}
                  />
                </Form.Item>
                <Form.Item
                  label={t("workflow_definitions.steps.fields.roles")}
                  name="roleIds"
                  rules={[{ required: true, message: t("workflow_definitions.steps.validation.roles_required") }]}
                  style={{ flex: 2, minWidth: isMobile ? "100%" : 260 }}
                >
                  <Select
                    mode="multiple"
                    allowClear
                    showSearch
                    filterOption={false}
                    options={roleOptionsState.options}
                    onSearch={roleOptionsState.onSearch}
                    loading={roleOptionsState.loading}
                    dropdownRender={(menu) => (
                      <>
                        {menu}
                        {roleOptionsState.hasMore ? (
                          <div style={{ padding: 8 }}>
                            <button
                              type="button"
                              onMouseDown={(event) => event.preventDefault()}
                              onClick={() => roleOptionsState.onLoadMore?.()}
                              style={{
                                width: "100%",
                                border: "none",
                                background: "transparent",
                                color: "#1677ff",
                                cursor: "pointer",
                                padding: 4,
                              }}
                            >
                              {t("workflow_definitions.steps.actions.load_more_roles")}
                            </button>
                          </div>
                        ) : null}
                      </>
                    )}
                  />
                </Form.Item>
                <Form.Item
                  label={t("workflow_definitions.steps.fields.required")}
                  name="isRequired"
                  valuePropName="checked"
                  style={{ marginTop: 30 }}
                >
                  <Switch />
                </Form.Item>
              </Flex>
              <Button type="primary" onClick={() => void handleAddStep()} disabled={!canManage} block={isMobile}>
                {editingIndex !== null ? t("workflow_definitions.steps.actions.update") : t("workflow_definitions.steps.actions.add")}
              </Button>
            </Form>

            <Table
              rowKey={(_record, index) => `step-${index}`}
              pagination={false}
              dataSource={steps}
              size={isMobile ? "small" : "middle"}
              scroll={{ x: "max-content" }}
              locale={{ emptyText: t("workflow_definitions.steps.empty") }}
              columns={[
                { title: t("workflow_definitions.steps.columns.order"), dataIndex: "displayOrder" },
                { title: t("workflow_definitions.steps.columns.name"), dataIndex: "name" },
                {
                  title: t("workflow_definitions.steps.columns.type"),
                  dataIndex: "stepType",
                  render: (value: WorkflowStepType) =>
                    stepTypeOptions.find((option) => option.value === value)?.label ?? value,
                },
                {
                  title: t("workflow_definitions.steps.columns.roles"),
                  dataIndex: "roleIds",
                  render: (value: string[]) =>
                    value.map((roleId) => roleLabelById.get(roleId) ?? roleId).join(", "),
                },
                {
                  title: t("workflow_definitions.steps.columns.document"),
                  dataIndex: "documentId",
                  render: (value?: string | null) => (value ? documentLabelById.get(value) ?? value : "-"),
                },
                {
                  title: t("workflow_definitions.steps.columns.published_version"),
                  dataIndex: "documentId",
                  render: (value?: string | null) => (value ? documentPublishedById.get(value) ?? "-" : "-"),
                },
                {
                  title: t("workflow_definitions.steps.columns.min_approvals"),
                  dataIndex: "minApprovals",
                  align: "center",
                  render: (value?: number) => value ?? 1,
                },
                {
                  title: t("workflow_definitions.steps.columns.required"),
                  dataIndex: "isRequired",
                  render: (value: boolean) => (value ? t("common.actions.yes") : t("common.actions.no")),
                },
                {
                  title: t("workflow_definitions.steps.columns.next_step"),
                  key: "nextStep",
                  render: (_value, record) => {
                    const options = steps
                      .filter((step) => step.displayOrder !== record.displayOrder)
                      .map((step) => ({
                        value: step.displayOrder,
                        label: `${step.displayOrder}. ${step.name}`,
                      }));
                    const currentNext = record.routes?.[0]?.nextDisplayOrder ?? null;
                    return (
                      <Select
                        allowClear
                        placeholder={t("workflow_definitions.steps.placeholders.next_step")}
                        value={currentNext ?? undefined}
                        options={options}
                        onChange={(value) => {
                          setSteps((current) =>
                            current.map((step) =>
                              step.displayOrder === record.displayOrder
                                ? {
                                    ...step,
                                    routes: value
                                      ? [{ action: step.stepType, nextDisplayOrder: value }]
                                      : [],
                                  }
                                : step,
                            ),
                          );
                        }}
                        style={{ minWidth: 180 }}
                      />
                    );
                  },
                },
                {
                  title: t("admin_users.columns.actions"),
                  key: "actions",
                  render: (_value, _record, index) => (
                    <Space size={4}>
                      <Button
                        type="text"
                        icon={<EditOutlined />}
                        onClick={() => {
                          const step = steps[index];
                          stepForm.setFieldsValue({
                            name: step.name,
                            stepType: step.stepType,
                            roleIds: step.roleIds,
                            isRequired: step.isRequired,
                            documentId: step.documentId ?? null,
                            minApprovals: step.minApprovals ?? 1,
                          });
                          setEditingIndex(index);
                        }}
                      >
                        {t("common.actions.edit")}
                      </Button>
                      <Button
                        type="text"
                        danger
                        icon={<DeleteOutlined />}
                        onClick={() => {
                          modal.confirm({
                            title: t("common.actions.delete"),
                            content: t("workflow_definitions.steps.confirm_delete"),
                            okText: t("common.actions.delete"),
                            okButtonProps: { danger: true },
                            cancelText: t("common.actions.cancel"),
                            onOk: () => handleRemoveStep(index),
                          });
                        }}
                      >
                        {t("common.actions.delete")}
                      </Button>
                    </Space>
                  ),
                },
              ]}
              style={{ marginTop: 16 }}
            />

            <Flex gap={12} wrap={!isMobile} vertical={isMobile} align={isMobile ? "stretch" : "center"} justify="flex-start" style={{ marginTop: 16 }}>
              <Button
                type="primary"
                icon={<SaveOutlined />}
                loading={updateMutation.isPending}
                onClick={() => void handleSubmit()}
                block={isMobile}
              >
                {t("workflow_definitions.actions.save")}
              </Button>
              <Button onClick={() => navigate("/app/steps")} block={isMobile}>
                {t("common.actions.cancel")}
              </Button>
            </Flex>
          </>
        )}
      </Card>
    </Space>
  );
}
