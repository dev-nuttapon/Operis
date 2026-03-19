import { App, Alert, Button, Card, Form, Space, Typography, Skeleton, Flex, Grid } from "antd";
import { ArrowLeftOutlined, EditOutlined, SaveOutlined, TeamOutlined } from "@ant-design/icons";
import { useEffect, useMemo } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import { useProjectTypeOptions } from "../hooks/useProjectTypeOptions";
import { ProjectForm, normalizeProjectPayload, toInitialValues, type ProjectFormValues } from "../components/projects/ProjectForm";
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

export function ProjectEditPage() {
  const { t } = useTranslation();
  const { notification } = App.useApp();
  const navigate = useNavigate();
  const location = useLocation();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const locationState = location.state as LocationState | null;
  const { projectId } = useParams<{ projectId: string }>();

  const permissionState = usePermissions();
  const canManageProjects = permissionState.hasPermission(permissions.projects.manage);
  const canManageProjectMembers = permissionState.hasPermission(permissions.projects.manageMembers);

  const defaultBackTarget = "/app/projects";
  const backTarget = locationState?.from ?? defaultBackTarget;

  const [editForm] = Form.useForm<ProjectFormValues>();

  const { projectDetailQuery, updateProjectMutation } = useProjectAdmin({
    projectsEnabled: false,
    projects: { page: 1, pageSize: 1 },
    projectDetailId: projectId,
    projectRoles: { page: 1, pageSize: 1 },
    projectAssignments: null,
  });

  const projectTypeOptionsState = useProjectTypeOptions({ enabled: canManageProjects });
  const userOptionsState = useProjectUserOptions(canManageProjects, toUserLabel);
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
    }
  }, [editForm, projectDetailQuery.data]);

  const handleSubmit = async () => {
    if (!projectId) return;
    const values = await editForm.validateFields();
    updateProjectMutation.mutate(
      { id: projectId, ...normalizeProjectPayload(values) },
      {
        onSuccess: () => {
          notification.success({ message: t("projects.messages.updated", { name: values.name }) });
          navigate(backTarget);
        },
        onError: (error) => {
          const presentation = getApiErrorPresentation(error, t("projects.messages.update_failed"));
          notification.error({ message: presentation.title, description: presentation.description });
        },
      },
    );
  };

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
              onClick={() =>
                navigate(`/app/admin/project-members?projectId=${projectId}`, { state: { from: `${location.pathname}${location.search}` } })
              }
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
