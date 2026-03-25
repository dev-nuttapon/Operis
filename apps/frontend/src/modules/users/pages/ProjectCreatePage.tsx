import { App, Button, Card, Form, Space, Typography, Alert, Flex, Grid, Divider, Table, Select } from "antd";
import type { ColumnsType } from "antd/es/table";
import { ArrowLeftOutlined, DeleteOutlined, ExclamationCircleOutlined, FolderOpenOutlined, SaveOutlined } from "@ant-design/icons";
import { useMemo, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import { useProjectTypeOptions } from "../hooks/useProjectTypeOptions";
import { ProjectForm, normalizeProjectPayload, type ProjectFormValues } from "../components/projects/ProjectForm";
import { useProjectUserOptions } from "../hooks/useProjectUserOptions";
import { useProjectRoleOptions } from "../hooks/useProjectRoleOptions";
import type { User } from "../types/users";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useWorkflowDefinition, useWorkflowDefinitionOptions } from "../../workflows";
import { useDocumentsByIds } from "../../documents";

type LocationState = {
  from?: string;
};

function toUserLabel(user: User) {
  const displayName = [user.keycloak?.firstName, user.keycloak?.lastName].filter(Boolean).join(" ").trim();
  const base = displayName || user.keycloak?.email || user.keycloak?.username || user.id;
  const jobTitle = user.jobTitleName?.trim();
  return jobTitle ? `${base} (${jobTitle})` : base;
}

export function ProjectCreatePage() {
  const { t } = useTranslation();
  const { notification, modal } = App.useApp();
  const navigate = useNavigate();
  const location = useLocation();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const locationState = location.state as LocationState | null;
  const permissionState = usePermissions();
  const canManageProjects = permissionState.hasPermission(permissions.projects.manage);
  const canManageProjectMembers = permissionState.hasPermission(permissions.projects.manageMembers);
  const canEditMembers = canManageProjects || canManageProjectMembers;
  const [createForm] = Form.useForm<ProjectFormValues>();
  const [memberTargetKeys, setMemberTargetKeys] = useState<string[]>([]);
  const [memberRoleByUserId, setMemberRoleByUserId] = useState<Record<string, string>>({});
  const [selectedWorkflowDefinitionId, setSelectedWorkflowDefinitionId] = useState<string | null>(null);

  const { createProjectMutation, createProjectAssignmentMutation } = useProjectAdmin({
    projectsEnabled: false,
    projects: { page: 1, pageSize: 10 },
    projectRoles: { page: 1, pageSize: 10 },
    projectAssignments: null,
  });
  const projectTypeOptionsState = useProjectTypeOptions({ enabled: canManageProjects });

  const userOptionsState = useProjectUserOptions(canManageProjects, toUserLabel);
  const projectRoleOptionsState = useProjectRoleOptions({ enabled: canEditMembers });
  const workflowDefinitionOptionsState = useWorkflowDefinitionOptions({ enabled: canManageProjects, status: "active" });
  const workflowDefinitionDetailQuery = useWorkflowDefinition(
    selectedWorkflowDefinitionId,
    Boolean(selectedWorkflowDefinitionId),
  );
  const roleLabelById = useMemo(
    () => new Map(projectRoleOptionsState.options.map((option) => [option.value, option.label] as const)),
    [projectRoleOptionsState.options],
  );
  // Page must remain usable even if dropdown option APIs fail.
  // Selects should degrade gracefully to empty options instead of crashing the route.
  const projectTypeOptions = useMemo(() => {
    const templateOptions = projectTypeOptionsState.options;
    return templateOptions.length > 0 ? templateOptions : [
      { label: t("projects.options.project_type.internal"), value: "Internal" },
      { label: t("projects.options.project_type.customer"), value: "Customer" },
      { label: t("projects.options.project_type.compliance"), value: "Compliance" },
      { label: t("projects.options.project_type.improvement"), value: "Improvement" },
    ];
  }, [projectTypeOptionsState.options, t]);

  const handleSubmit = async (redirectToMembers?: boolean) => {
    const values = await createForm.validateFields();
    const missingRole = memberTargetKeys.find((userId) => !memberRoleByUserId[userId]);
    if (missingRole) {
      notification.error({ message: t("projects.members.validation.role_required") });
      return;
    }
    const payload = {
      ...normalizeProjectPayload(values),
      workflowDefinitionId: selectedWorkflowDefinitionId ?? undefined,
    };
    createProjectMutation.mutate(payload, {
      onSuccess: async (project) => {
        if (canEditMembers && memberTargetKeys.length > 0) {
          await Promise.all(
            memberTargetKeys.map((userId) =>
              createProjectAssignmentMutation.mutateAsync({
                userId,
                projectId: project.id,
                projectRoleId: memberRoleByUserId[userId],
                isPrimary: false,
              }),
            ),
          );
        }
        notification.success({ message: t("projects.messages.created", { name: values.name }) });
        if (redirectToMembers) {
          navigate(`/app/admin/project-members?projectId=${project.id}`, { state: { from: "/app/projects/new" } });
          return;
        }
        navigate("/app/projects");
      },
      onError: (error) => {
        const { message } = getApiErrorPresentation(error);
        notification.error({ message: t("projects.messages.create_failed"), description: message });
      },
    });
  };

  const memberTransferData = useMemo(
    () =>
      userOptionsState.items.map((user) => ({
        key: user.id,
        title: toUserLabel(user),
        description: user.keycloak?.email ?? user.keycloak?.username ?? user.id,
        meta: user,
      })),
    [userOptionsState.items],
  );

  const [selectedMemberId, setSelectedMemberId] = useState<string | null>(null);

  const memberOptions = useMemo(() => {
    const selectedIds = new Set(memberTargetKeys);
    return memberTransferData
      .filter((item) => !selectedIds.has(String(item.key)))
      .map((item) => ({
        value: String(item.key),
        label: item.title,
        meta: item.meta,
      }));
  }, [memberTransferData, memberTargetKeys]);

  const selectedMembers = useMemo(() => {
    if (memberTargetKeys.length === 0) return [];
    const userById = new Map(memberTransferData.map((item) => [item.key, item.meta] as const));
    return memberTargetKeys.map((userId) => {
      const user = userById.get(userId);
      return {
        id: userId,
        name: user ? toUserLabel(user) : userId,
        email: user?.keycloak?.email ?? user?.keycloak?.username ?? "-",
        roleId: memberRoleByUserId[userId] ?? null,
      };
    });
  }, [memberRoleByUserId, memberTargetKeys, memberTransferData]);

  const hasProjectRoleOptions = projectRoleOptionsState.options.length > 0;

  const memberColumns = useMemo<ColumnsType<{ id: string; name: string; email: string; roleId: string | null }>>(
    () => [
      {
        title: t("projects.members.columns.name"),
        dataIndex: "name",
      },
      {
        title: t("projects.members.columns.email"),
        dataIndex: "email",
      },
      {
        title: t("projects.members.columns.role"),
        dataIndex: "roleId",
        render: (_value, record) => (
          <Select
            disabled={!canEditMembers}
            allowClear
            placeholder={t("projects.members.placeholders.role")}
            options={projectRoleOptionsState.options}
            notFoundContent={<Typography.Text type="secondary">{t("projects.members.messages.no_roles")}</Typography.Text>}
            value={memberRoleByUserId[record.id]}
            onChange={(value) =>
              setMemberRoleByUserId((current) => ({
                ...current,
                [record.id]: value ?? "",
              }))
            }
            dropdownRender={(menu) => (
              <>
                {menu}
                {projectRoleOptionsState.hasMore ? (
                  <div style={{ padding: 8 }}>
                    <Button type="link" onClick={() => projectRoleOptionsState.onLoadMore?.()}>
                      {t("projects.load_more_roles")}
                    </Button>
                  </div>
                ) : null}
              </>
            )}
          />
        ),
      },
      {
        title: t("admin_users.columns.actions"),
        key: "actions",
        align: "center",
        render: (_, record) => (
          <Button
            type="text"
            danger
            icon={<DeleteOutlined />}
            onClick={() => {
              modal.confirm({
                title: t("projects.members.actions.remove_confirm_title"),
                content: t("projects.members.actions.remove_confirm_description", { name: record.name }),
                icon: <ExclamationCircleOutlined />,
                okText: t("common.actions.confirm_delete"),
                cancelText: t("common.actions.cancel"),
                okButtonProps: { danger: true },
                onOk: () => {
                  setMemberTargetKeys((current) => current.filter((key) => key !== record.id));
                  setMemberRoleByUserId((current) => {
                    const next = { ...current };
                    delete next[record.id];
                    return next;
                  });
                },
              });
            }}
          >
            {t("projects.members.actions.remove")}
          </Button>
        ),
      },
    ],
    [canEditMembers, memberRoleByUserId, projectRoleOptionsState.options, t],
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

  const workflowSteps = workflowDefinitionDetailQuery.data?.steps ?? [];
  const workflowDocumentIds = useMemo(
    () => workflowSteps.map((step) => step.documentId).filter((value): value is string => Boolean(value)),
    [workflowSteps],
  );
  const workflowDocumentsQuery = useDocumentsByIds(
    workflowDocumentIds,
    Boolean(workflowDocumentIds.length),
  );
  const documentById = useMemo(
    () => new Map((workflowDocumentsQuery.data ?? []).map((doc) => [doc.id, doc] as const)),
    [workflowDocumentsQuery.data],
  );
  const stepLabelByOrder = useMemo(
    () => new Map(workflowSteps.map((step) => [step.displayOrder, `${step.displayOrder}. ${step.name}`] as const)),
    [workflowSteps],
  );

  const workflowStepColumns = useMemo<ColumnsType<{ id?: string; displayOrder: number; name: string; stepType: string; isRequired: boolean; roleIds?: string[]; documentId?: string | null; minApprovals?: number; routes?: { nextDisplayOrder?: number | null }[] }>>(
    () => [
      { title: t("workflow_definitions.steps.columns.order"), dataIndex: "displayOrder" },
      { title: t("workflow_definitions.steps.columns.name"), dataIndex: "name" },
      {
        title: t("workflow_definitions.steps.columns.type"),
        dataIndex: "stepType",
        render: (value: string) => stepTypeOptions.find((option) => option.value === value)?.label ?? value,
      },
      {
        title: t("workflow_definitions.steps.columns.roles"),
        dataIndex: "roleIds",
        render: (value?: string[]) => (value && value.length > 0
          ? value.map((roleId) => roleLabelById.get(roleId) ?? roleId).join(", ")
          : "-"),
      },
      {
        title: t("workflow_definitions.steps.columns.document"),
        dataIndex: "documentId",
        render: (value?: string | null) => (value ? documentById.get(value)?.documentName ?? value : "-"),
      },
      {
        title: t("workflow_definitions.steps.columns.published_version"),
        dataIndex: "documentId",
        render: (value?: string | null) => (value ? documentById.get(value)?.publishedVersionCode ?? "-" : "-"),
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
          const labels = (record.routes ?? [])
            .map((route) => route.nextDisplayOrder)
            .filter((value): value is number => Boolean(value))
            .map((order) => stepLabelByOrder.get(order) ?? `${order}`);
          return labels.length > 0 ? labels.join(", ") : "-";
        },
      },
    ],
    [documentById, roleLabelById, stepLabelByOrder, stepTypeOptions, t],
  );

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Space style={{ width: "100%", justifyContent: "flex-start" }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate(locationState?.from ?? "/app/projects")} block={isMobile}>
          {t("projects.create_page_back")}
        </Button>
      </Space>

      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)", color: "#fff" }}>
            <FolderOpenOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("projects.create_page_title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("projects.create_page_description")}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {!canManageProjects ? (
          <Alert type="info" showIcon message={t("errors.title_forbidden")} />
        ) : (
          <>
            <ProjectForm
              form={createForm}
              t={t}
              userOptions={userOptionsState.options}
              projectTypeOptions={projectTypeOptions}
              userOptionsLoading={userOptionsState.loading}
              onUserSearch={userOptionsState.onSearch}
              onUserLoadMore={userOptionsState.onLoadMore}
              userHasMore={userOptionsState.hasMore}
              projectTypeOptionsLoading={projectTypeOptionsState.loading}
              onProjectTypeSearch={projectTypeOptionsState.onSearch}
              onProjectTypeLoadMore={projectTypeOptionsState.onLoadMore}
              projectTypeHasMore={projectTypeOptionsState.hasMore}
            />
            <Divider style={{ margin: "8px 0 16px" }} />
            <Typography.Title level={5} style={{ marginBottom: 12 }}>
              {t("projects.members_section.title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ marginTop: 0 }}>
              {t("projects.members_section.create_hint")}
            </Typography.Paragraph>
            <Flex gap={12} vertical align="stretch">
              <Select
                allowClear
                showSearch
                filterOption={false}
                disabled={!canEditMembers}
                placeholder={t("projects.members.placeholders.member")}
                value={selectedMemberId}
                options={memberOptions}
                onSearch={userOptionsState.onSearch}
                onChange={(value) => setSelectedMemberId(value ?? null)}
                loading={userOptionsState.loading}
                style={{ width: "100%" }}
                dropdownRender={(menu) => (
                  <>
                    {menu}
                    {userOptionsState.hasMore ? (
                      <div style={{ padding: 8 }}>
                        <button
                          type="button"
                          onMouseDown={(event) => event.preventDefault()}
                          onClick={() => userOptionsState.onLoadMore?.()}
                          style={{
                            width: "100%",
                            border: "none",
                            background: "transparent",
                            color: "#1677ff",
                            cursor: "pointer",
                            padding: 4,
                          }}
                        >
                          {t("projects.load_more_users")}
                        </button>
                      </div>
                    ) : null}
                  </>
                )}
              />
              <div style={{ width: isMobile ? "100%" : "auto",marginBottom:16 }}>
                <Button
                  type="primary"
                  disabled={!canEditMembers || !selectedMemberId}
                  onClick={() => {
                    if (!selectedMemberId) return;
                    setMemberTargetKeys((current) =>
                      current.includes(selectedMemberId) ? current : [...current, selectedMemberId],
                    );
                    setSelectedMemberId(null);
                  }}
                  block={isMobile}
                >
                  {t("common.actions.add")}
                </Button>
              </div>
            </Flex>
            <Table
              rowKey="id"
              pagination={false}
              dataSource={selectedMembers}
              columns={memberColumns}
              locale={{ emptyText: t("projects.members.empty") }}
              scroll={{ x: "max-content" }}
            />
            <Divider style={{ margin: "16px 0" }} />
            <Typography.Title level={5} style={{ marginBottom: 12 }}>
              {t("projects.documents_section.title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ marginTop: 0 }}>
              {t("projects.documents_section.create_hint")}
            </Typography.Paragraph>
            <Form layout="vertical">
              <Form.Item label={t("projects.documents_section.workflow_label")}>
                <Select
                  allowClear
                  showSearch
                  placeholder={t("projects.documents_section.workflow_placeholder")}
                  options={workflowDefinitionOptionsState.options}
                  value={selectedWorkflowDefinitionId}
                  loading={workflowDefinitionOptionsState.loading}
                  onChange={(value) => setSelectedWorkflowDefinitionId(value ?? null)}
                  notFoundContent={<Typography.Text type="secondary">{t("projects.documents_section.no_workflows")}</Typography.Text>}
                />
              </Form.Item>
            </Form>
            {selectedWorkflowDefinitionId ? (
              <Table
                rowKey={(record) => record.id ?? `step-${record.displayOrder}`}
                pagination={false}
                dataSource={workflowDefinitionDetailQuery.data?.steps ?? []}
                loading={workflowDefinitionDetailQuery.isLoading}
                columns={workflowStepColumns}
                locale={{ emptyText: t("workflow_definitions.steps.empty") }}
                scroll={{ x: "max-content" }}
                size={isMobile ? "small" : "middle"}
              />
            ) : null}
            <Flex
              gap={12}
              wrap={!isMobile}
              vertical={isMobile}
              align={isMobile ? "stretch" : "center"}
              justify="flex-start"
              style={{ width: "100%" }}
            >
              <Button
                type="primary"
                icon={<SaveOutlined />}
                loading={createProjectMutation.isPending}
                onClick={() => void handleSubmit(false)}
                block={isMobile}
              >
                {t("projects.create_page_submit")}
              </Button>
              <Button
                onClick={() => navigate(locationState?.from ?? "/app/projects")}
                block={isMobile}
              >
                {t("common.actions.cancel")}
              </Button>
            </Flex>
          </>
        )}
      </Card>
    </Space>
  );
}
