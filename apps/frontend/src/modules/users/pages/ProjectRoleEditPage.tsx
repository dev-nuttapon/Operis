import { App, Alert, Button, Card, Form, Space, Typography, Skeleton, Flex, Grid } from "antd";
import { ArrowLeftOutlined, SaveOutlined, SolutionOutlined } from "@ant-design/icons";
import { useEffect, useMemo } from "react";
import { useLocation, useNavigate, useParams, useSearchParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import { useProjectOptions } from "../hooks/useProjectOptions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { ProjectRoleForm, toProjectRoleInitialValues, type ProjectRoleFormValues } from "../components/projectRoles/ProjectRoleForm";
import { useProjectRoleDetail } from "../hooks/useProjectRoleDetail";
import type { UpdateProjectRoleInput } from "../types/users";

type LocationState = { from?: string };

export function ProjectRoleEditPage() {
  const { t } = useTranslation();
  const { notification } = App.useApp();
  const navigate = useNavigate();
  const location = useLocation();
  const locationState = location.state as LocationState | null;
  const { projectRoleId } = useParams<{ projectRoleId: string }>();
  const [searchParams] = useSearchParams();
  const projectIdFromQuery = searchParams.get("projectId") ?? undefined;

  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;

  const permissionState = usePermissions();
  const canReadProjects = permissionState.hasPermission(permissions.projects.read);
  const canManageProjectRoles = permissionState.hasPermission(permissions.projects.manageRoles);

  const backTarget = locationState?.from ?? (projectIdFromQuery ? `/app/admin/project-roles?projectId=${projectIdFromQuery}` : "/app/admin/project-roles");

  const [form] = Form.useForm<ProjectRoleFormValues>();
  const detailQuery = useProjectRoleDetail(projectRoleId);
  const { updateProjectRoleMutation } = useProjectAdmin({
    projectsEnabled: false,
    projects: { page: 1, pageSize: 1 },
    projectRoles: { projectId: projectIdFromQuery, page: 1, pageSize: 10 },
    projectAssignments: null,
  });

  const projectOptionsState = useProjectOptions({ enabled: canReadProjects });
  const projectOptions = projectOptionsState.options;

  useEffect(() => {
    if (!detailQuery.data) return;
    form.setFieldsValue(toProjectRoleInitialValues(detailQuery.data, projectIdFromQuery));
  }, [detailQuery.data, form, projectIdFromQuery]);

  const handleSubmit = async () => {
    if (!projectRoleId) return;
    const values = await form.validateFields();
    const payload: UpdateProjectRoleInput = {
      id: projectRoleId,
      projectId: values.projectId,
      name: values.name,
      code: values.code,
      description: values.description,
      responsibilities: values.responsibilities,
      authorityScope: values.authorityScope,
      canCreateDocuments: values.canCreateDocuments,
      canReviewDocuments: values.canReviewDocuments,
      canApproveDocuments: values.canApproveDocuments,
      canReleaseDocuments: values.canReleaseDocuments,
      isPeerReviewRole: values.isPeerReviewRole,
      isReviewRole: values.isReviewRole,
      isApprovalRole: values.isApprovalRole,
      displayOrder: values.displayOrder,
    };

    updateProjectRoleMutation.mutate(payload, {
      onSuccess: () => {
        notification.success({ message: t("project_roles.messages.updated", { name: values.name }) });
        navigate(`/app/admin/project-roles?projectId=${values.projectId}`, { replace: true });
      },
      onError: (error) => {
        const presentation = getApiErrorPresentation(error, t("project_roles.messages.update_failed"));
        notification.error({ message: presentation.title, description: presentation.description });
      },
    });
  };

  const title = useMemo(() => t("project_roles.edit_page_title"), [t]);
  const description = useMemo(() => t("project_roles.edit_page_description"), [t]);

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Space style={{ width: "100%", justifyContent: "flex-start" }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate(backTarget)} block={isMobile}>
          {t("project_roles.edit_page_back")}
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
        ) : detailQuery.loading && !detailQuery.data ? (
          <Skeleton active paragraph={{ rows: 6 }} />
        ) : !detailQuery.data ? (
          <Alert type="error" showIcon message={t("errors.title_not_found")} />
        ) : (
          <>
            <ProjectRoleForm form={form} t={t} projectOptions={projectOptions} />
            <Flex gap={12} wrap={!isMobile} vertical={isMobile} align={isMobile ? "stretch" : "center"} justify="flex-start">
              <Button
                type="primary"
                icon={<SaveOutlined />}
                loading={updateProjectRoleMutation.isPending}
                onClick={() => void handleSubmit()}
                block={isMobile}
              >
                {t("project_roles.edit_page_submit")}
              </Button>
            </Flex>
          </>
        )}
      </Card>
    </Space>
  );
}
