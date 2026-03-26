import { App, Alert, Button, Card, Form, Space, Typography, Flex, Grid } from "antd";
import { ArrowLeftOutlined, SaveOutlined, SolutionOutlined } from "@ant-design/icons";
import { useMemo } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { ProjectRoleForm, type ProjectRoleFormValues } from "../components/projectRoles/ProjectRoleForm";
import type { CreateProjectRoleInput } from "../types/users";
import { useProjectOptions } from "../hooks/useProjectOptions";

type LocationState = { from?: string };

export function ProjectRoleCreatePage() {
  const { t } = useTranslation();
  const { notification } = App.useApp();
  const navigate = useNavigate();
  const location = useLocation();
  const locationState = location.state as LocationState | null;

  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;

  const permissionState = usePermissions();
  const canManageProjectRoles = permissionState.hasPermission(permissions.projects.manageRoles);

  const backTarget = locationState?.from ?? "/app/projects/roles";

  const [form] = Form.useForm<ProjectRoleFormValues>();
  const projectOptionsState = useProjectOptions({ enabled: canManageProjectRoles });
  const { createProjectRoleMutation } = useProjectAdmin({
    projectsEnabled: false,
    projects: { page: 1, pageSize: 1 },
    projectRoles: { page: 1, pageSize: 10 },
    projectAssignments: null,
  });

  const handleSubmit = async () => {
    const values = await form.validateFields();
    const payload: CreateProjectRoleInput = {
      projectId: values.projectId,
      name: values.name,
      code: values.code,
      status: values.status,
      description: values.description,
      responsibilities: values.responsibilities,
      authorityScope: values.authorityScope,
      displayOrder: values.displayOrder,
    };

    createProjectRoleMutation.mutate(payload, {
      onSuccess: () => {
        notification.success({ message: t("project_roles.messages.created", { name: values.name }) });
        navigate("/app/projects/roles", { replace: true });
      },
      onError: (error) => {
        const presentation = getApiErrorPresentation(error, t("project_roles.messages.create_failed"));
        notification.error({ message: presentation.title, description: presentation.description });
      },
    });
  };

  const title = useMemo(() => t("project_roles.create_page_title"), [t]);
  const description = useMemo(() => t("project_roles.create_page_description"), [t]);

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Space style={{ width: "100%", justifyContent: "flex-start" }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate(backTarget)} block={isMobile}>
          {t("project_roles.create_page_back")}
        </Button>
      </Space>

      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)", color: "#fff" }}>
            <SolutionOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {title}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {description}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {!canManageProjectRoles ? (
          <Alert type="info" showIcon message={t("errors.title_forbidden")} />
        ) : (
          <>
            <ProjectRoleForm
              form={form}
              t={t}
              projectOptions={projectOptionsState.options}
              projectOptionsLoading={projectOptionsState.loading}
              onProjectSearch={projectOptionsState.onSearch}
              onProjectLoadMore={projectOptionsState.onLoadMore}
              hasMoreProjects={projectOptionsState.hasMore}
            />
            <Flex gap={12} wrap={!isMobile} vertical={isMobile} align={isMobile ? "stretch" : "center"} justify="flex-start">
              <Button
                type="primary"
                icon={<SaveOutlined />}
                loading={createProjectRoleMutation.isPending}
                onClick={() => void handleSubmit()}
                block={isMobile}
              >
                {t("project_roles.create_page_submit")}
              </Button>
            </Flex>
          </>
        )}
      </Card>
    </Space>
  );
}
