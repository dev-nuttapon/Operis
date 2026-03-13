import { App, Button, Card, Flex, Form, Input, Select, Space, Typography, theme as antdTheme } from "antd";
import { Suspense, lazy, useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { usePublicRegistration } from "../hooks/usePublicRegistration";
import { useOrgStructureOptions } from "../hooks/useOrgStructureOptions";
import { ApiError, getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useThemeStore, type ThemeMode } from "../../../shared/store/useThemeStore";

const { Paragraph, Title } = Typography;
const PublicActionSuccessModal = lazy(() =>
  import("../components/publicUsers/PublicActionSuccessModal").then((module) => ({ default: module.PublicActionSuccessModal }))
);

export function PublicRegistrationPage() {
  const { notification } = App.useApp();
  const { token: designToken } = antdTheme.useToken();
  const { theme, setTheme } = useThemeStore();
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const [form] = Form.useForm();
  const [successModalOpen, setSuccessModalOpen] = useState(false);
  const { registerMutation, divisionsQuery } = usePublicRegistration();
  const currentLanguage = i18n.language.startsWith("th") ? "th" : "en";
  const selectedDivisionId = Form.useWatch("divisionId", form) as string | undefined;
  const selectedDepartmentId = Form.useWatch("departmentId", form) as string | undefined;
  const cascade = useOrgStructureOptions({ divisionId: selectedDivisionId, departmentId: selectedDepartmentId, publicAccess: true });
  const isDarkMode = designToken.colorBgBase.toLowerCase() === "#020617";
  const pageBackground = isDarkMode
    ? "radial-gradient(circle at top, rgba(14, 165, 233, 0.16) 0%, rgba(15, 23, 42, 0.98) 42%, rgba(2, 6, 23, 1) 100%)"
    : "linear-gradient(180deg, #f8fafc 0%, #e2e8f0 100%)";

  useEffect(() => {
    let actualTheme = theme;
    if (theme === "system") {
      actualTheme = window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
    }

    document.documentElement.setAttribute("data-theme", actualTheme);
  }, [theme]);

  const getRegistrationErrorNotification = (error: unknown) => {
    const presentation = getApiErrorPresentation(error, t("public_registration.notifications.submit_failed_title"));

    if (!(error instanceof ApiError)) {
      return {
        title: t("public_registration.notifications.submit_failed_title"),
        description: t("public_registration.notifications.submit_failed_description"),
      };
    }

    if (error.message === t("errors.user_exists") || error.message === t("errors.keycloak_user_exists")) {
      return {
        title: t("public_registration.notifications.email_in_use_title"),
        description: t("public_registration.notifications.email_in_use_description"),
      };
    }

    if (error.message === t("errors.pending_registration_exists")) {
      return {
        title: t("public_registration.notifications.pending_request_title"),
        description: t("public_registration.notifications.pending_request_description"),
      };
    }

    if (error.message === t("errors.pending_invitation_exists")) {
      return {
        title: t("public_registration.notifications.pending_invitation_title"),
        description: t("public_registration.notifications.pending_invitation_description"),
      };
    }

    if (error.category === "network") {
      return {
        title: t("public_registration.notifications.server_unavailable_title"),
        description: t("public_registration.notifications.server_unavailable_description"),
      };
    }

    if (error.category === "conflict") {
      return {
        title: t("public_registration.notifications.conflict_title"),
        description: t("public_registration.notifications.conflict_description"),
      };
    }

    if (error.category === "bad_request") {
      return {
        title: t("public_registration.notifications.invalid_data_title"),
        description: error.message || t("public_registration.notifications.invalid_data_description"),
      };
    }

    if (error.category === "unauthorized" || error.category === "forbidden") {
      return {
        title: t("public_registration.notifications.access_denied_title"),
        description: t("public_registration.notifications.access_denied_description"),
      };
    }

    if (error.category === "server") {
      return {
        title: t("public_registration.notifications.server_error_title"),
        description: t("public_registration.notifications.server_error_description"),
      };
    }

    return {
      title: t("public_registration.notifications.submit_failed_title"),
      description: presentation.description || t("public_registration.notifications.submit_failed_description"),
    };
  };

  useEffect(() => {
    if (!registerMutation.isSuccess) {
      return;
    }

    setSuccessModalOpen(true);
    form.resetFields();
  }, [form, registerMutation.isSuccess]);

  useEffect(() => {
    if (!registerMutation.isError) {
      return;
    }

    const presentation = getRegistrationErrorNotification(registerMutation.error);
    notification.error({
      message: presentation.title,
      description: presentation.description,
    });
  }, [notification, registerMutation.error, registerMutation.isError]);

  const divisionOptions = useMemo(() => (divisionsQuery.data?.items ?? []).map((item) => ({ label: item.name, value: item.id })), [divisionsQuery.data?.items]);
  const departmentOptions = useMemo(
    () => (cascade.departmentsQuery.data?.items ?? []).map((item) => ({ label: item.name, value: item.id })),
    [cascade.departmentsQuery.data?.items]
  );
  const jobTitleOptions = useMemo(
    () => (cascade.jobTitlesQuery.data?.items ?? []).map((item) => ({ label: item.name, value: item.id })),
    [cascade.jobTitlesQuery.data?.items]
  );

  return (
    <div
      style={{
        minHeight: "100vh",
        display: "flex",
        flexDirection: "column",
        padding: 24,
        background: pageBackground,
      }}
    >
      <Flex justify="space-between" align="center" style={{ width: "100%", marginBottom: 24 }} gap="middle" wrap>
        <Button type="text" onClick={() => navigate("/login")}>
          {t("public_registration.back_to_login")}
        </Button>
        <Flex
          gap={12}
          align="center"
          style={{
            padding: 12,
            borderRadius: 18,
            background: isDarkMode ? "rgba(15, 23, 42, 0.72)" : "rgba(255, 255, 255, 0.82)",
            border: `1px solid ${designToken.colorBorder}`,
            boxShadow: isDarkMode
              ? "0 16px 32px rgba(2, 6, 23, 0.32)"
              : "0 16px 32px rgba(148, 163, 184, 0.18)",
            backdropFilter: "blur(14px)",
          }}
        >
          <Space size={8}>
            <span style={{ color: designToken.colorTextSecondary, fontSize: 13, fontWeight: 600 }}>
              {t("common.language")}
            </span>
            <Select
              value={currentLanguage}
              variant="filled"
              popupMatchSelectWidth={false}
              style={{ width: 128 }}
              onChange={(value: "th" | "en") => {
                void i18n.changeLanguage(value);
              }}
              options={[
                { value: "en", label: t("common.language_en") },
                { value: "th", label: t("common.language_th") },
              ]}
            />
          </Space>
          <Select
            value={theme}
            variant="filled"
            popupMatchSelectWidth={false}
            style={{ width: 148 }}
            onChange={(value: ThemeMode) => setTheme(value)}
            options={[
              { value: "light", label: t("common.theme.light") },
              { value: "dark", label: t("common.theme.dark") },
              { value: "system", label: t("common.theme.system") },
            ]}
          />
        </Flex>
      </Flex>

      <div style={{ flex: 1, display: "grid", placeItems: "center" }}>
        <Card
          style={{
            width: "100%",
            maxWidth: 560,
            borderRadius: 20,
            background: designToken.colorBgContainer,
            borderColor: designToken.colorBorder,
            boxShadow: isDarkMode
              ? "0 24px 60px rgba(2, 6, 23, 0.55)"
              : "0 24px 60px rgba(148, 163, 184, 0.22)",
          }}
        >
          <Space direction="vertical" size={20} style={{ width: "100%" }}>
            <div>
              <Title level={3} style={{ marginBottom: 8 }}>
                {t("public_registration.page_title")}
              </Title>
              <Paragraph type="secondary" style={{ marginBottom: 0 }}>
                {t("public_registration.page_description")}
              </Paragraph>
            </div>

            <Form
              form={form}
              layout="vertical"
              onFinish={(values) => {
                registerMutation.mutate(values);
              }}
            >
              <Form.Item
                label={t("admin_users.fields.email")}
                name="email"
                rules={[
                  { required: true, message: t("errors.email_required") },
                  { type: "email", message: t("documents.form.owner_email_invalid") },
                ]}
              >
                <Input placeholder={t("public_registration.email_placeholder")} />
              </Form.Item>
              <Form.Item
                label={t("admin_users.fields.first_name")}
                name="firstName"
                rules={[{ required: true, message: t("invitation_page.first_name_required") }]}
              >
                <Input placeholder={t("invitation_page.first_name_placeholder")} />
              </Form.Item>
              <Form.Item
                label={t("admin_users.fields.last_name")}
                name="lastName"
                rules={[{ required: true, message: t("invitation_page.last_name_required") }]}
              >
                <Input placeholder={t("invitation_page.last_name_placeholder")} />
              </Form.Item>
              <Form.Item label={t("admin_users.fields.division")} name="divisionId">
                <Select
                  allowClear
                  placeholder={t("admin_users.placeholders.select_division")}
                  loading={divisionsQuery.isLoading}
                  options={divisionOptions}
                  onChange={() => {
                    form.setFieldValue("departmentId", undefined);
                    form.setFieldValue("jobTitleId", undefined);
                  }}
                />
              </Form.Item>
              <Form.Item label={t("admin_users.fields.department")} name="departmentId">
                <Select
                  allowClear
                  disabled={!selectedDivisionId}
                  placeholder={t("admin_users.placeholders.select_department")}
                  loading={cascade.departmentsQuery.isLoading}
                  options={departmentOptions}
                  onChange={() => {
                    form.setFieldValue("jobTitleId", undefined);
                  }}
                />
              </Form.Item>
              <Form.Item label={t("admin_users.fields.job_title")} name="jobTitleId">
                <Select
                  allowClear
                  disabled={!selectedDepartmentId}
                  placeholder={t("admin_users.placeholders.select_job_title")}
                  loading={cascade.jobTitlesQuery.isLoading}
                  options={jobTitleOptions}
                />
              </Form.Item>
              <Button type="primary" htmlType="submit" block loading={registerMutation.isPending}>
                {t("public_registration.submit")}
              </Button>
            </Form>
          </Space>
        </Card>
      </div>

      {successModalOpen ? (
        <Suspense fallback={null}>
          <PublicActionSuccessModal
            actionLabel={t("public_registration.go_login")}
            description={t("public_registration.success_description")}
            onConfirm={() => {
              setSuccessModalOpen(false);
              navigate("/login", { replace: true });
            }}
            open={successModalOpen}
            successColor={designToken.colorSuccess}
            title={t("public_registration.success_title")}
          />
        </Suspense>
      ) : null}
    </div>
  );
}
