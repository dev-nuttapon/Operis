import { App, Button, Card, Form, Space, Typography, Alert, Flex, Grid } from "antd";
import { ArrowLeftOutlined, CloseOutlined, FolderOpenOutlined, SaveOutlined, TeamOutlined } from "@ant-design/icons";
import { useMemo } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import { useProjectTypeOptions } from "../hooks/useProjectTypeOptions";
import { ProjectForm, normalizeProjectPayload, type ProjectFormValues } from "../components/projects/ProjectForm";
import { useProjectUserOptions } from "../hooks/useProjectUserOptions";
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
  const canManageProjectRoles = permissionState.hasPermission(permissions.projects.manageRoles);
  const canManageProjectMembers = permissionState.hasPermission(permissions.projects.manageMembers);
  const [createForm] = Form.useForm<ProjectFormValues>();

  const { createProjectMutation } = useProjectAdmin({
    projectsEnabled: false,
    projects: { page: 1, pageSize: 10 },
    projectRoles: { page: 1, pageSize: 10 },
    projectAssignments: null,
  });
  const projectTypeOptionsState = useProjectTypeOptions({ enabled: canManageProjects });

  const userOptionsState = useProjectUserOptions(canManageProjects, toUserLabel);
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
    createProjectMutation.mutate(normalizeProjectPayload(values), {
      onSuccess: (project) => {
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

      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 40, height: 40, borderRadius: 12, display: "grid", placeItems: "center", background: "rgba(14, 165, 233, 0.15)", color: "#38bdf8" }}>
            <TeamOutlined />
          </div>
          <div style={{ flex: 1 }}>
            <Typography.Title level={4} style={{ margin: 0 }}>
              {t("projects.members_section.title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("projects.members_section.description")}
            </Typography.Paragraph>
          </div>
          {canManageProjectMembers ? (
            <Button
              icon={<TeamOutlined />}
              loading={createProjectMutation.isPending}
              onClick={() => void handleSubmit(true)}
              block={isMobile}
            >
              {t("projects.actions.manage_members")}
            </Button>
          ) : null}
        </Space>
      </Card>
    </Space>
  );
}
