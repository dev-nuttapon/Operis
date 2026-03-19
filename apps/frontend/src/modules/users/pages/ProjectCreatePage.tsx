import { App, Button, Card, Form, Space, Typography, Alert, Flex, Grid, Divider, Table, Transfer, Select } from "antd";
import type { ColumnsType } from "antd/es/table";
import { ArrowLeftOutlined, CloseOutlined, FolderOpenOutlined, SaveOutlined } from "@ant-design/icons";
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
  const { notification } = App.useApp();
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

  const { createProjectMutation, createProjectAssignmentMutation } = useProjectAdmin({
    projectsEnabled: false,
    projects: { page: 1, pageSize: 10 },
    projectRoles: { page: 1, pageSize: 10 },
    projectAssignments: null,
  });
  const projectTypeOptionsState = useProjectTypeOptions({ enabled: canManageProjects });

  const userOptionsState = useProjectUserOptions(canManageProjects, toUserLabel);
  const projectRoleOptionsState = useProjectRoleOptions({ enabled: canEditMembers, allowWithoutProjectId: true });
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
    createProjectMutation.mutate(normalizeProjectPayload(values), {
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
      { title: t("projects.members.columns.name"), dataIndex: "name" },
      { title: t("projects.members.columns.email"), dataIndex: "email" },
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
          />
        ),
      },
    ],
    [canEditMembers, memberRoleByUserId, projectRoleOptionsState.options, t],
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
            <Transfer
              dataSource={memberTransferData}
              titles={[t("projects.members.transfer.available"), t("projects.members.transfer.selected")]}
              targetKeys={memberTargetKeys}
              onChange={(nextTargetKeys) => {
                const nextKeys = nextTargetKeys.map((key) => String(key));
                setMemberTargetKeys(nextKeys);
                setMemberRoleByUserId((current) => {
                  const next = { ...current };
                  Object.keys(next).forEach((key) => {
                    if (!nextKeys.includes(key)) {
                      delete next[key];
                    }
                  });
                  return next;
                });
              }}
              render={(item) => item.title}
              listStyle={{ width: 260, height: 320 }}
              showSearch
              disabled={!canEditMembers}
              onSearch={(direction, value) => {
                if (direction === "left") {
                  userOptionsState.onSearch(value);
                }
              }}
              footer={(props) =>
                props.direction === "left" && userOptionsState.hasMore ? (
                  <Button type="link" onClick={() => userOptionsState.onLoadMore?.()}>
                    {t("projects.load_more_users")}
                  </Button>
                ) : null
              }
            />
            <Table
              rowKey="id"
              pagination={false}
              dataSource={selectedMembers}
              columns={memberColumns}
              locale={{ emptyText: t("projects.members.empty") }}
              scroll={{ x: "max-content" }}
            />
            <Flex
              gap={12}
              wrap={!isMobile}
              vertical={isMobile}
              align={isMobile ? "stretch" : "center"}
              justify="flex-start"
              style={{ width: "100%" }}
            >
              <Button
                icon={<CloseOutlined />}
                onClick={() => navigate(locationState?.from ?? "/app/projects")}
                block={isMobile}
              >
                {t("common.actions.cancel")}
              </Button>
              <Button
                type="primary"
                icon={<SaveOutlined />}
                loading={createProjectMutation.isPending}
                onClick={() => void handleSubmit(false)}
                block={isMobile}
              >
                {t("projects.create_page_submit")}
              </Button>
            </Flex>
          </>
        )}
      </Card>
    </Space>
  );
}
