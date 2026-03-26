import { App, Alert, Button, Card, Form, Space, Typography, Skeleton, Flex, Grid, Divider, Table, Select } from "antd";
import type { ColumnsType } from "antd/es/table";
import { ArrowLeftOutlined, EditOutlined, SaveOutlined, DeleteOutlined, ExclamationCircleOutlined } from "@ant-design/icons";
import { useEffect, useMemo, useRef, useState } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import { useProjectTypeOptions } from "../hooks/useProjectTypeOptions";
import { ProjectForm, normalizeProjectPayload, toInitialValues, type ProjectFormValues } from "../components/projects/ProjectForm";
import { useProjectUserOptions } from "../hooks/useProjectUserOptions";
import { useProjectRoleOptions } from "../hooks/useProjectRoleOptions";
import { useRefreshKeycloakUsersCache } from "../hooks/useRefreshKeycloakUsersCache";
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

export function ProjectEditPage() {
  const { t } = useTranslation();
  const { notification, modal } = App.useApp();
  const navigate = useNavigate();
  const location = useLocation();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const locationState = location.state as LocationState | null;
  const { projectId } = useParams<{ projectId: string }>();

  const permissionState = usePermissions();
  const canManageProjects = permissionState.hasPermission(permissions.projects.manage);
  const canManageProjectMembers = permissionState.hasPermission(permissions.projects.manageMembers);
  const canEditMembers = canManageProjectMembers || canManageProjects;
  const canLoadProjectRoles = canEditMembers;

  const defaultBackTarget = "/app/projects";
  const backTarget = locationState?.from ?? defaultBackTarget;

  const [editForm] = Form.useForm<ProjectFormValues>();
  const [memberTargetKeys, setMemberTargetKeys] = useState<string[]>([]);
  const [memberRoleByUserId, setMemberRoleByUserId] = useState<Record<string, string>>({});
  const [selectedWorkflowDefinitionId, setSelectedWorkflowDefinitionId] = useState<string | null>(null);

  const { projectDetailQuery, updateProjectMutation, projectAssignmentsQuery, createProjectAssignmentMutation, updateProjectAssignmentMutation, deleteProjectAssignmentMutation } = useProjectAdmin({
    projectsEnabled: false,
    projects: { page: 1, pageSize: 1 },
    projectDetailId: projectId,
    projectRoles: { page: 1, pageSize: 1 },
    projectAssignments: projectId ? { projectId, page: 1, pageSize: 200 } : null,
  });

  const projectTypeOptionsState = useProjectTypeOptions({ enabled: canManageProjects });
  const userOptionsState = useProjectUserOptions(canEditMembers, toUserLabel);
  const projectRoleOptionsState = useProjectRoleOptions({ enabled: canLoadProjectRoles });
  const workflowDefinitionOptionsState = useWorkflowDefinitionOptions({ enabled: canManageProjects, status: "active" });
  const workflowDefinitionDetailQuery = useWorkflowDefinition(
    selectedWorkflowDefinitionId,
    Boolean(selectedWorkflowDefinitionId),
  );
  const roleLabelById = useMemo(
    () => new Map(projectRoleOptionsState.options.map((option) => [option.value, option.label] as const)),
    [projectRoleOptionsState.options],
  );
  const refreshKeycloakUsersCache = useRefreshKeycloakUsersCache();
  const hasTriggeredUserRefresh = useRef(false);
  const projectTypeOptions = useMemo(() => {
    const templateOptions = projectTypeOptionsState.options;
    return templateOptions.length > 0 ? templateOptions : [
      { label: t("projects.options.project_type.internal"), value: "Internal" },
      { label: t("projects.options.project_type.customer"), value: "Customer" },
      { label: t("projects.options.project_type.compliance"), value: "Compliance" },
      { label: t("projects.options.project_type.improvement"), value: "Improvement" },
    ];
  }, [projectTypeOptionsState.options, t]);

  useEffect(() => {
    if (projectDetailQuery.data) {
      editForm.setFieldsValue(toInitialValues(projectDetailQuery.data));
      setSelectedWorkflowDefinitionId(projectDetailQuery.data.workflowDefinitionId ?? null);
    }
  }, [editForm, projectDetailQuery.data]);

  useEffect(() => {
    if (!projectAssignmentsQuery.data) {
      return;
    }
    const assignments = projectAssignmentsQuery.data.items ?? [];
    setMemberTargetKeys(assignments.map((assignment) => assignment.userId));
    setMemberRoleByUserId(() => {
      const next: Record<string, string> = {};
      assignments.forEach((assignment) => {
        next[assignment.userId] = assignment.projectRoleId;
      });
      return next;
    });

    if (
      canEditMembers
      && assignments.length > 0
      && !hasTriggeredUserRefresh.current
      && assignments.some((assignment) =>
        !assignment.userDisplayName
        || assignment.userDisplayName === assignment.userId
        || !assignment.userEmail
        || assignment.userEmail === assignment.userId)
    ) {
      hasTriggeredUserRefresh.current = true;
      refreshKeycloakUsersCache.mutate(undefined, {
        onSuccess: () => {
          void projectAssignmentsQuery.refetch();
        },
        onError: () => {
          hasTriggeredUserRefresh.current = false;
        },
      });
    }
  }, [canEditMembers, projectAssignmentsQuery, projectAssignmentsQuery.data, refreshKeycloakUsersCache]);

  const handleSubmit = async () => {
    if (!projectId) return;
    const values = await editForm.validateFields();
    const missingRole = memberTargetKeys.find((userId) => !memberRoleByUserId[userId]);
    if (missingRole) {
      notification.error({ message: t("projects.members.validation.role_required") });
      return;
    }
    try {
      await updateProjectMutation.mutateAsync({
        id: projectId,
        ...normalizeProjectPayload(values),
        workflowDefinitionId: selectedWorkflowDefinitionId ?? undefined,
      });

      if (canEditMembers) {
        const currentAssignments = projectAssignmentsQuery.data?.items ?? [];
        const assignmentByUserId = new Map(currentAssignments.map((assignment) => [assignment.userId, assignment]));

        const desiredUserIds = new Set(memberTargetKeys);
        const createPayloads = memberTargetKeys
          .filter((userId) => !assignmentByUserId.has(userId))
          .map((userId) => ({
            userId,
            projectId,
            projectRoleId: memberRoleByUserId[userId],
            isPrimary: false,
          }));

        const updatePayloads = memberTargetKeys
          .filter((userId) => assignmentByUserId.has(userId))
          .map((userId) => {
            const assignment = assignmentByUserId.get(userId)!;
            return {
              id: assignment.id,
              userId,
              projectId,
              projectRoleId: memberRoleByUserId[userId],
              reportsToUserId: assignment.reportsToUserId ?? undefined,
              isPrimary: assignment.isPrimary,
              startAt: assignment.startAt ?? undefined,
              endAt: assignment.endAt ?? undefined,
              reason: t("projects.members.reasons.updated"),
            };
          })
          .filter((payload) => {
            const assignment = assignmentByUserId.get(payload.userId)!;
            return assignment.projectRoleId !== payload.projectRoleId;
          });

        const deletePayloads = currentAssignments.filter((assignment) => !desiredUserIds.has(assignment.userId));

        await Promise.all(createPayloads.map((payload) => createProjectAssignmentMutation.mutateAsync(payload)));
        await Promise.all(updatePayloads.map((payload) => updateProjectAssignmentMutation.mutateAsync(payload)));
        await Promise.all(deletePayloads.map((assignment) =>
          deleteProjectAssignmentMutation.mutateAsync({ id: assignment.id, input: { reason: t("projects.members.reasons.removed") } })
        ));
      }

      notification.success({ message: t("projects.messages.updated", { name: values.name }) });
      navigate(backTarget);
    } catch (error) {
      const presentation = getApiErrorPresentation(error, t("projects.messages.update_failed"));
      notification.error({ message: presentation.title, description: presentation.description });
    }
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
    const assignmentByUserId = new Map(
      (projectAssignmentsQuery.data?.items ?? []).map((assignment) => [assignment.userId, assignment] as const),
    );
    return memberTargetKeys.map((userId) => {
      const assignment = assignmentByUserId.get(userId);
      const user = userById.get(userId);
      const name = assignment?.userDisplayName
        ?? assignment?.userEmail
        ?? (user ? toUserLabel(user) : userId);
      const email = assignment?.userEmail
        ?? user?.keycloak?.email
        ?? user?.keycloak?.username
        ?? "-";
      return {
        id: userId,
        name,
        email,
        roleId: memberRoleByUserId[userId] ?? null,
      };
    });
  }, [memberRoleByUserId, memberTargetKeys, memberTransferData, projectAssignmentsQuery.data]);

  const memberColumns = useMemo<ColumnsType<{ id: string; name: string; email: string; roleId: string | null }>>(
    () => [
      { title: t("projects.members.columns.name"), dataIndex: "name" },
      { title: t("projects.members.columns.email"), dataIndex: "email" },
      {
        title: t("projects.members.columns.role"),
        dataIndex: "roleId",
        render: (_value, record) => (
          <Select
            allowClear
            disabled={!canEditMembers}
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
        title: "",
        key: "remove",
        align: "center",
        render: (_, record) => (
          <Button
            type="text"
            danger
            size="small"
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
                  const nextKeys = memberTargetKeys.filter((key) => key !== record.id);
                  setMemberTargetKeys(nextKeys);
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
    [canEditMembers, memberRoleByUserId, memberTargetKeys, projectRoleOptionsState, t],
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
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate(backTarget)} block={isMobile}>
          {t("projects.edit_page_back")}
        </Button>
      </Space>

      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)", color: "#fff" }}>
            <EditOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("projects.edit_page_title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("projects.edit_page_description")}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {!canManageProjects ? (
          <Alert type="info" showIcon message={t("errors.title_forbidden")} />
        ) : projectDetailQuery.isLoading && !projectDetailQuery.data ? (
          <Skeleton active paragraph={{ rows: 6 }} />
        ) : !projectDetailQuery.data ? (
          <Alert type="error" showIcon message={t("errors.title_not_found")} />
        ) : (
          <>
            <ProjectForm
              form={editForm}
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
              {t("projects.members_section.description")}
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
              <div style={{ width: isMobile ? "100%" : "auto", marginBottom:16 }}>
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
              {t("projects.documents_section.description")}
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
                loading={updateProjectMutation.isPending}
                onClick={() => void handleSubmit()}
                block={isMobile}
              >
                {t("projects.edit_page_submit")}
              </Button>
              <Button onClick={() => navigate(backTarget)} block={isMobile}>
                {t("projects.edit_page_cancel")}
              </Button>
            </Flex>
          </>
        )}
      </Card>
    </Space>
  );
}
