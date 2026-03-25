import { App, Alert, Button, Card, Flex, Grid, Space, Typography } from "antd";
import {
  ApartmentOutlined,
  FileTextOutlined,
  ReloadOutlined,
  SettingOutlined,
  SolutionOutlined,
  TeamOutlined,
  TagsOutlined,
  UnorderedListOutlined,
} from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useQueryClient } from "@tanstack/react-query";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import { useRefreshKeycloakUsersCache } from "../hooks/useRefreshKeycloakUsersCache";
import { useRefreshWorkflowDefinitionsCache } from "../hooks/useRefreshWorkflowDefinitionsCache";
import { useRefreshDocumentTemplateCache } from "../hooks/useRefreshDocumentTemplateCache";
import { useRefreshDepartmentsCache } from "../hooks/useRefreshDepartmentsCache";
import { useRefreshJobTitlesCache } from "../hooks/useRefreshJobTitlesCache";
import { useRefreshDivisionsCache } from "../hooks/useRefreshDivisionsCache";
import { useRefreshProjectRolesCache } from "../hooks/useRefreshProjectRolesCache";

export function AdminSettingsPage() {
  const { t } = useTranslation();
  const { notification } = App.useApp();
  const queryClient = useQueryClient();
  const permissionState = usePermissions();
  const canManageUsers = permissionState.hasPermission(permissions.users.update);
  const canManageWorkflows =
    permissionState.hasPermission(permissions.workflows.manageDefinitions)
    || permissionState.hasPermission(permissions.users.update);
  const canManageTemplates =
    permissionState.hasPermission(permissions.documents.upload)
    || permissionState.hasPermission(permissions.users.update);
  const hasAnyPermission = canManageUsers || canManageWorkflows || canManageTemplates;
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const refreshMutation = useRefreshKeycloakUsersCache();
  const refreshWorkflowMutation = useRefreshWorkflowDefinitionsCache();
  const refreshTemplateMutation = useRefreshDocumentTemplateCache();
  const refreshDepartmentsMutation = useRefreshDepartmentsCache();
  const refreshJobTitlesMutation = useRefreshJobTitlesCache();
  const refreshDivisionsMutation = useRefreshDivisionsCache();
  const refreshProjectRolesMutation = useRefreshProjectRolesCache();

  const buttonStyle = {
    height: 44,
    borderRadius: 12,
    paddingInline: 16,
  } as const;

  const ghostStyle = {
    borderColor: "rgba(148, 163, 184, 0.3)",
    color: "#e2e8f0",
  } as const;

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
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
            <SettingOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("admin_settings.title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("admin_settings.description")}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card
        variant="borderless"
        style={{
          background: "linear-gradient(135deg, rgba(15, 23, 42, 0.95), rgba(30, 41, 59, 0.95))",
          border: "1px solid rgba(148, 163, 184, 0.2)",
        }}
      >
        {!hasAnyPermission ? (
          <Alert type="warning" showIcon message={t("errors.title_forbidden")} />
        ) : (
          <Flex gap={12} wrap={!isMobile} vertical={isMobile} align={isMobile ? "stretch" : "center"}>
            <Button
              type="primary"
              icon={<TeamOutlined />}
              loading={refreshMutation.isPending}
              disabled={!canManageUsers}
              style={buttonStyle}
              onClick={() => {
                refreshMutation.mutate(undefined, {
                  onSuccess: (result) => {
                    notification.success({
                      message: t("admin_settings.messages.refresh_success", {
                        refreshed: result.refreshed,
                        missing: result.missing,
                      }),
                    });
                  },
                  onError: () => {
                    notification.error({ message: t("admin_settings.messages.refresh_failed") });
                  },
                });
              }}
            >
              {t("admin_settings.actions.refresh_keycloak_cache")}
            </Button>
            <Button
              icon={<UnorderedListOutlined />}
              loading={refreshWorkflowMutation.isPending}
              disabled={!canManageWorkflows}
              style={ghostStyle}
              onClick={() => {
                refreshWorkflowMutation.mutate(undefined, {
                  onSuccess: (result) => {
                    void queryClient.invalidateQueries({ queryKey: ["workflows", "definitions"] });
                    notification.success({
                      message: t("admin_settings.messages.refresh_workflows_success", {
                        total: result.total,
                      }),
                    });
                  },
                  onError: () => {
                    notification.error({ message: t("admin_settings.messages.refresh_workflows_failed") });
                  },
                });
              }}
            >
              {t("admin_settings.actions.refresh_workflows_cache")}
            </Button>
            <Button
              icon={<FileTextOutlined />}
              loading={refreshTemplateMutation.isPending}
              disabled={!canManageTemplates}
              style={ghostStyle}
              onClick={() => {
                refreshTemplateMutation.mutate(undefined, {
                  onSuccess: (result) => {
                    void queryClient.invalidateQueries({ queryKey: ["documents", "templates"] });
                    notification.success({
                      message: t("admin_settings.messages.refresh_templates_success", {
                        total: result.total,
                      }),
                    });
                  },
                  onError: () => {
                    notification.error({ message: t("admin_settings.messages.refresh_templates_failed") });
                  },
                });
              }}
            >
              {t("admin_settings.actions.refresh_templates_cache")}
            </Button>
            <Button
              icon={<ApartmentOutlined />}
              loading={refreshDepartmentsMutation.isPending}
              disabled={!canManageUsers}
              style={ghostStyle}
              onClick={() => {
                refreshDepartmentsMutation.mutate(undefined, {
                  onSuccess: (result) => {
                    void queryClient.invalidateQueries({ queryKey: ["admin", "departments"] });
                    void queryClient.invalidateQueries({ queryKey: ["department-options"] });
                    notification.success({
                      message: t("admin_settings.messages.refresh_departments_success", {
                        total: result.total,
                      }),
                    });
                  },
                  onError: () => {
                    notification.error({ message: t("admin_settings.messages.refresh_departments_failed") });
                  },
                });
              }}
            >
              {t("admin_settings.actions.refresh_departments_cache")}
            </Button>
            <Button
              icon={<TagsOutlined />}
              loading={refreshDivisionsMutation.isPending}
              disabled={!canManageUsers}
              style={ghostStyle}
              onClick={() => {
                refreshDivisionsMutation.mutate(undefined, {
                  onSuccess: (result) => {
                    void queryClient.invalidateQueries({ queryKey: ["admin", "divisions"] });
                    void queryClient.invalidateQueries({ queryKey: ["division-options"] });
                    notification.success({
                      message: t("admin_settings.messages.refresh_divisions_success", {
                        total: result.total,
                      }),
                    });
                  },
                  onError: () => {
                    notification.error({ message: t("admin_settings.messages.refresh_divisions_failed") });
                  },
                });
              }}
            >
              {t("admin_settings.actions.refresh_divisions_cache")}
            </Button>
            <Button
              icon={<SolutionOutlined />}
              loading={refreshJobTitlesMutation.isPending}
              disabled={!canManageUsers}
              style={ghostStyle}
              onClick={() => {
                refreshJobTitlesMutation.mutate(undefined, {
                  onSuccess: (result) => {
                    void queryClient.invalidateQueries({ queryKey: ["admin", "job-titles"] });
                    void queryClient.invalidateQueries({ queryKey: ["job-title-options"] });
                    notification.success({
                      message: t("admin_settings.messages.refresh_job_titles_success", {
                        total: result.total,
                      }),
                    });
                  },
                  onError: () => {
                    notification.error({ message: t("admin_settings.messages.refresh_job_titles_failed") });
                  },
                });
              }}
            >
              {t("admin_settings.actions.refresh_job_titles_cache")}
            </Button>
            <Button
              icon={<ReloadOutlined />}
              loading={refreshProjectRolesMutation.isPending}
              disabled={!canManageUsers}
              style={ghostStyle}
              onClick={() => {
                refreshProjectRolesMutation.mutate(undefined, {
                  onSuccess: (result) => {
                    void queryClient.invalidateQueries({ queryKey: ["admin", "project-roles"] });
                    void queryClient.invalidateQueries({ queryKey: ["project-role-options"] });
                    notification.success({
                      message: t("admin_settings.messages.refresh_project_roles_success", {
                        total: result.total,
                      }),
                    });
                  },
                  onError: () => {
                    notification.error({ message: t("admin_settings.messages.refresh_project_roles_failed") });
                  },
                });
              }}
            >
              {t("admin_settings.actions.refresh_project_roles_cache")}
            </Button>
          </Flex>
        )}
      </Card>
    </Space>
  );
}
