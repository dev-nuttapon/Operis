import { App, Alert, Button, Card, Form, Space, Typography, Flex, Grid, Skeleton } from "antd";
import { ArrowLeftOutlined, EditOutlined, SaveOutlined } from "@ant-design/icons";
import { useEffect, useRef } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useProjectTemplates } from "../hooks/useProjectTemplates";
import { useProjectTypeRoleRequirementDetail } from "../hooks/useProjectTypeRoleRequirementDetail";
import { useProjectTypeTemplateDetail } from "../hooks/useProjectTypeTemplateDetail";
import { ProjectTypeRoleRequirementForm } from "../components/projectTypeTemplates/ProjectTypeRoleRequirementForm";
import type { CreateProjectTypeRoleRequirementInput } from "../types/users";

type LocationState = {
  from?: string;
};

export function ProjectTypeRoleRequirementEditPage() {
  const { t } = useTranslation();
  const { notification } = App.useApp();
  const { templateId, requirementId } = useParams<{ templateId: string; requirementId: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const locationState = location.state as LocationState | null;
  const backTarget = locationState?.from ?? "/app/admin/project-type-templates";

  const permissionState = usePermissions();
  const canManageTemplates = permissionState.hasPermission(permissions.projects.manageTemplates);

  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;

  const [form] = Form.useForm<CreateProjectTypeRoleRequirementInput>();
  const requirementQuery = useProjectTypeRoleRequirementDetail(requirementId);
  const templateQuery = useProjectTypeTemplateDetail(templateId ?? requirementQuery.data?.projectTypeTemplateId);

  const { updateRoleRequirementMutation } = useProjectTemplates({
    templates: { page: 1, pageSize: 10 },
    roleRequirements: { page: 1, pageSize: 10 },
  });

  const initializedRef = useRef(false);
  useEffect(() => {
    if (initializedRef.current) return;
    if (!requirementQuery.data) return;
    form.setFieldsValue({
      projectTypeTemplateId: requirementQuery.data.projectTypeTemplateId,
      roleName: requirementQuery.data.roleName,
      roleCode: requirementQuery.data.roleCode ?? undefined,
      description: requirementQuery.data.description ?? undefined,
      displayOrder: requirementQuery.data.displayOrder,
    });
    initializedRef.current = true;
  }, [form, requirementQuery.data]);

  const handleSubmit = (values: CreateProjectTypeRoleRequirementInput) => {
    if (!requirementId) return;

    updateRoleRequirementMutation.mutate(
      { id: requirementId, ...values },
      {
        onSuccess: () => {
          notification.success({ message: t("project_type_templates.role_requirements.messages.updated") });
          navigate(backTarget);
        },
        onError: (error) => {
          const presentation = getApiErrorPresentation(error, t("project_type_templates.role_requirements.messages.save_failed"));
          notification.error({ message: presentation.title, description: presentation.description });
        },
      },
    );
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Space style={{ width: "100%", justifyContent: "flex-start" }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate(backTarget)} block={isMobile}>
          {t("project_type_templates.role_requirements.edit_page_back")}
        </Button>
      </Space>

      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)", color: "#fff" }}>
            <EditOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("project_type_templates.role_requirements.edit_page_title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("project_type_templates.role_requirements.edit_page_description")}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {!canManageTemplates ? (
          <Alert type="warning" showIcon message={t("errors.title_forbidden")} />
        ) : requirementQuery.isLoading && !requirementQuery.data ? (
          <Skeleton active paragraph={{ rows: 6 }} />
        ) : !requirementQuery.data ? (
          <Alert type="error" showIcon message={t("errors.title_not_found")} />
        ) : (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <ProjectTypeRoleRequirementForm form={form} t={t} selectedTemplate={templateQuery.data} onFinish={handleSubmit} disableTemplateSelect />
            <Flex justify="flex-start">
              <Button type="primary" icon={<SaveOutlined />} onClick={() => form.submit()} loading={updateRoleRequirementMutation.isPending} block={isMobile}>
                {t("project_type_templates.role_requirements.edit_page_submit")}
              </Button>
            </Flex>
          </Space>
        )}
      </Card>
    </Space>
  );
}
