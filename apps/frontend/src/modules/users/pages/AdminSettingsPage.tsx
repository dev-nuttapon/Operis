import { App, Alert, Button, Card, Flex, Grid, Space, Typography } from "antd";
import { ReloadOutlined, SettingOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useQueryClient } from "@tanstack/react-query";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import { useRefreshKeycloakUsersCache } from "../hooks/useRefreshKeycloakUsersCache";
import { useRefreshWorkflowDefinitionsCache } from "../hooks/useRefreshWorkflowDefinitionsCache";
import { useRefreshDocumentTemplateCache } from "../hooks/useRefreshDocumentTemplateCache";

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

      <Card variant="borderless">
        {!hasAnyPermission ? (
          <Alert type="warning" showIcon message={t("errors.title_forbidden")} />
        ) : (
          <Flex gap={12} wrap={!isMobile} vertical={isMobile} align={isMobile ? "stretch" : "center"}>
            <Button
              type="primary"
              icon={<ReloadOutlined />}
              loading={refreshMutation.isPending}
              disabled={!canManageUsers}
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
              icon={<ReloadOutlined />}
              loading={refreshWorkflowMutation.isPending}
              disabled={!canManageWorkflows}
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
              icon={<ReloadOutlined />}
              loading={refreshTemplateMutation.isPending}
              disabled={!canManageTemplates}
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
          </Flex>
        )}
      </Card>
    </Space>
  );
}
