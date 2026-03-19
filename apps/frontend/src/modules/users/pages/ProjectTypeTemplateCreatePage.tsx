import { App, Alert, Button, Card, Form, Space, Typography, Flex, Grid } from "antd";
import { ArrowLeftOutlined, PlusOutlined, SaveOutlined } from "@ant-design/icons";
import { useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useProjectTemplates } from "../hooks/useProjectTemplates";
import { ProjectTypeTemplateForm } from "../components/projectTypeTemplates/ProjectTypeTemplateForm";
import type { CreateProjectTypeTemplateInput } from "../types/users";

type LocationState = {
  from?: string;
};

export function ProjectTypeTemplateCreatePage() {
  const { t } = useTranslation();
  const { notification } = App.useApp();
  const navigate = useNavigate();
  const location = useLocation();
  const locationState = location.state as LocationState | null;
  const backTarget = locationState?.from ?? "/app/admin/project-type-templates";

  const permissionState = usePermissions();
  const canManageTemplates = permissionState.hasPermission(permissions.projects.manageTemplates);

  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;

  const [form] = Form.useForm<CreateProjectTypeTemplateInput>();
  const { createTemplateMutation } = useProjectTemplates({
    templates: { page: 1, pageSize: 10 },
    roleRequirements: { page: 1, pageSize: 10 },
  });

  const handleSubmit = (values: CreateProjectTypeTemplateInput) => {
    createTemplateMutation.mutate(values, {
      onSuccess: () => {
        notification.success({ message: t("project_type_templates.messages.created") });
        navigate(backTarget);
      },
      onError: (error) => {
        const presentation = getApiErrorPresentation(error, t("project_type_templates.messages.save_failed"));
        notification.error({ message: presentation.title, description: presentation.description });
      },
    });
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Space style={{ width: "100%", justifyContent: "flex-start" }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate(backTarget)} block={isMobile}>
          {t("project_type_templates.create_page_back")}
        </Button>
      </Space>

      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)", color: "#fff" }}>
            <PlusOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("project_type_templates.create_page_title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("project_type_templates.create_page_description")}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {!canManageTemplates ? (
          <Alert type="warning" showIcon message={t("errors.title_forbidden")} />
        ) : (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <ProjectTypeTemplateForm form={form} t={t} onFinish={handleSubmit} />
            <Flex justify="flex-start">
              <Button type="primary" icon={<SaveOutlined />} onClick={() => form.submit()} loading={createTemplateMutation.isPending} block={isMobile}>
                {t("project_type_templates.create_page_submit")}
              </Button>
            </Flex>
          </Space>
        )}
      </Card>
    </Space>
  );
}
