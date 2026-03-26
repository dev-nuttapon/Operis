import { App, Alert, Button, Card, Form, Space, Typography, Flex, Grid, Skeleton } from "antd";
import { ArrowLeftOutlined, PlusOutlined, SaveOutlined } from "@ant-design/icons";
import { useEffect, useMemo } from "react";
import { useLocation, useNavigate, useSearchParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import { useProjectOptions } from "../hooks/useProjectOptions";
import { useProjectRoleOptions } from "../hooks/useProjectRoleOptions";
import { useProjectUserOptions } from "../hooks/useProjectUserOptions";
import type { User } from "../types/users";
import { ProjectMemberForm, type ProjectMemberFormValues } from "../components/projectMembers/ProjectMemberForm";

type LocationState = {
  from?: string;
};

function toUserLabel(user: User) {
  return user.keycloak?.email ?? user.keycloak?.username ?? user.id;
}

export function ProjectMemberCreatePage() {
  const { t } = useTranslation();
  const { notification } = App.useApp();
  const navigate = useNavigate();
  const location = useLocation();
  const locationState = location.state as LocationState | null;
  const backTarget = locationState?.from ?? "/app/admin/project-members";
  const [searchParams] = useSearchParams();

  const permissionState = usePermissions();
  const canReadProjects = permissionState.hasPermission(permissions.projects.read);
  const canManageProjectMembers = permissionState.hasPermission(permissions.projects.manageMembers);

  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;

  const [form] = Form.useForm<ProjectMemberFormValues>();

  useEffect(() => {
    const initialProjectId = searchParams.get("projectId") ?? undefined;
    if (initialProjectId) {
      form.setFieldValue("projectId", initialProjectId);
    }
  }, [form, searchParams]);

  const { createProjectAssignmentMutation } = useProjectAdmin({
    projectsEnabled: false,
    projects: { page: 1, pageSize: 1 },
    projectRoles: { page: 1, pageSize: 1 },
    projectAssignments: null,
  });

  const projectOptionsState = useProjectOptions({ enabled: canReadProjects });
  const projectRoleOptionsState = useProjectRoleOptions({ enabled: canManageProjectMembers });
  const userOptionsState = useProjectUserOptions(canManageProjectMembers, toUserLabel);

  const projectOptions = projectOptionsState.options;
  const projectRoleOptions = projectRoleOptionsState.options;
  const userOptions = useMemo(() => [...userOptionsState.options], [userOptionsState.options]);
  const reportingOptions = userOptions;

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      if (!values.projectId) return;

      createProjectAssignmentMutation.mutate(
        {
          userId: values.userId,
          projectId: values.projectId,
          projectRoleId: values.projectRoleId,
          reportsToUserId: values.reportsToUserId,
          isPrimary: Boolean(values.isPrimary),
          startAt: values.period?.[0]?.startOf("day").toISOString(),
          endAt: values.period?.[1]?.endOf("day").toISOString(),
        },
        {
          onSuccess: () => {
            notification.success({ message: t("project_members.messages.created") });
            navigate(backTarget);
          },
          onError: (error) => {
            const presentation = getApiErrorPresentation(error, t("project_members.messages.create_failed"));
            notification.error({ message: presentation.title, description: presentation.description });
          },
        },
      );
    } catch {
      // validation errors
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Space style={{ width: "100%", justifyContent: "flex-start" }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate(backTarget)} block={isMobile}>
          {t("project_members.create_page_back")}
        </Button>
      </Space>

      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)", color: "#fff" }}>
            <PlusOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("project_members.create_page_title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("project_members.create_page_description")}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {!canManageProjectMembers ? (
          <Alert type="warning" showIcon message={t("errors.title_forbidden")} />
        ) : projectOptionsState.loading && projectOptions.length === 0 ? (
          <Skeleton active paragraph={{ rows: 6 }} />
        ) : (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <ProjectMemberForm
              form={form}
              t={t}
              showProjectField
              projectOptions={projectOptions}
              projectOptionsLoading={projectOptionsState.loading}
              onProjectSearch={projectOptionsState.onSearch}
              onProjectLoadMore={projectOptionsState.onLoadMore}
              projectHasMore={projectOptionsState.hasMore}
              userOptions={userOptions}
              projectRoleOptions={projectRoleOptions}
              reportingOptions={reportingOptions}
              includeReason={false}
              userOptionsLoading={userOptionsState.loading}
              onUserSearch={userOptionsState.onSearch}
              onUserLoadMore={userOptionsState.onLoadMore}
              userHasMore={userOptionsState.hasMore}
              roleOptionsLoading={projectRoleOptionsState.loading}
              onRoleSearch={projectRoleOptionsState.onSearch}
              onRoleLoadMore={projectRoleOptionsState.onLoadMore}
              roleHasMore={projectRoleOptionsState.hasMore}
            />

            <Flex justify="flex-start">
              <Button type="primary" icon={<SaveOutlined />} onClick={handleSubmit} loading={createProjectAssignmentMutation.isPending} block={isMobile}>
                {t("project_members.create_page_submit")}
              </Button>
            </Flex>
          </Space>
        )}
      </Card>
    </Space>
  );
}
