import { App, Alert, Button, Card, Form, Space, Typography, Flex, Grid, Skeleton } from "antd";
import { ArrowLeftOutlined, PlusOutlined, SaveOutlined } from "@ant-design/icons";
import { useEffect } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useProjectTemplates } from "../hooks/useProjectTemplates";
import { useProjectTypeTemplateDetail } from "../hooks/useProjectTypeTemplateDetail";
import { ProjectTypeRoleRequirementForm } from "../components/projectTypeTemplates/ProjectTypeRoleRequirementForm";
import type { CreateProjectTypeRoleRequirementInput } from "../types/users";

type LocationState = {
  from?: string;
};

export function ProjectTypeRoleRequirementCreatePage() {
  const { t } = useTranslation();
  const { notification } = App.useApp();
  const { templateId } = useParams<{ templateId: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const locationState = location.state as LocationState | null;
  const backTarget = locationState?.from ?? "/app/admin/project-type-templates";

  const permissionState = usePermissions();
  const canManageTemplates = permissionState.hasPermission(permissions.projects.manageTemplates);

  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;

  const [form] = Form.useForm<CreateProjectTypeRoleRequirementInput>();
  const templateQuery = useProjectTypeTemplateDetail(templateId);

  const { createRoleRequirementMutation } = useProjectTemplates({
    templates: { page: 1, pageSize: 10 },
    roleRequirements: { page: 1, pageSize: 10 },
  });

  useEffect(() => {
    if (!templateQuery.data) return;
    form.setFieldValue("projectTypeTemplateId", templateQuery.data.id);
    if (!form.getFieldValue("displayOrder")) {
      form.setFieldValue("displayOrder", 100);
    }
  }, [form, templateQuery.data]);

  const handleSubmit = (values: CreateProjectTypeRoleRequirementInput) => {
    createRoleRequirementMutation.mutate(values, {
      onSuccess: () => {
        notification.success({ message: t("project_type_templates.role_requirements.messages.created") });
        navigate(backTarget);
      },
      onError: (error) => {
        const presentation = getApiErrorPresentation(error, t("project_type_templates.role_requirements.messages.save_failed"));
        notification.error({ message: presentation.title, description: presentation.description });
      },
    });
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Space style={{ width: "100%", justifyContent: "flex-start" }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate(backTarget)} block={isMobile}>
          {t("project_type_templates.role_requirements.create_page_back")}
        </Button>
      </Space>

      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)", color: "#fff" }}>
            <PlusOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("project_type_templates.role_requirements.create_page_title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("project_type_templates.role_requirements.create_page_description")}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {!canManageTemplates ? (
          <Alert type="warning" showIcon message={t("errors.title_forbidden")} />
        ) : templateQuery.isLoading && !templateQuery.data ? (
          <Skeleton active paragraph={{ rows: 6 }} />
        ) : !templateQuery.data ? (
          <Alert type="error" showIcon message={t("errors.title_not_found")} />
        ) : (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <ProjectTypeRoleRequirementForm form={form} t={t} selectedTemplate={templateQuery.data} onFinish={handleSubmit} disableTemplateSelect />
            <Flex justify="flex-start">
              <Button type="primary" icon={<SaveOutlined />} onClick={() => form.submit()} loading={createRoleRequirementMutation.isPending} block={isMobile}>
                {t("project_type_templates.role_requirements.create_page_submit")}
              </Button>
            </Flex>
          </Space>
        )}
      </Card>
    </Space>
  );
}
