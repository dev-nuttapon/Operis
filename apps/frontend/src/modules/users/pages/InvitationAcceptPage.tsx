import { App, Button, Card, Flex, Form, Input, Select, Space, Typography, theme as antdTheme } from "antd";
import { Suspense, lazy, useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useInvitationAcceptance } from "../hooks/useInvitationAcceptance";
import { ApiError, getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useThemeStore, type ThemeMode } from "../../../shared/store/useThemeStore";
import { useTranslation } from "react-i18next";
import { CenteredLoader } from "../../../shared/components/feedback/CenteredLoader";
import { StatusPanel } from "../../../shared/components/feedback/StatusPanel";

const { Title } = Typography;
const PublicActionSuccessModal = lazy(() =>
  import("../components/publicUsers/PublicActionSuccessModal").then((module) => ({ default: module.PublicActionSuccessModal }))
);

export function InvitationAcceptPage() {
  const { notification } = App.useApp();
  const { token: designToken } = antdTheme.useToken();
  const { theme, setTheme } = useThemeStore();
  const { t, i18n: i18nInstance } = useTranslation();
  const navigate = useNavigate();
  const { token } = useParams<{ token: string }>();
  const [form] = Form.useForm();
  const [successModalOpen, setSuccessModalOpen] = useState(false);
  const { acceptInvitationMutation, invitationQuery } = useInvitationAcceptance(token);
  const currentLanguage = i18nInstance.language.startsWith("th") ? "th" : "en";
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

  const getInvitationErrorNotification = (error: unknown) => {
    const presentation = getApiErrorPresentation(error, t("invitation_page.notifications.submit_failed_title"));

    if (!(error instanceof ApiError)) {
      return presentation;
    }

    if (error.message === t("errors.invitation_expired")) {
      return {
        title: t("invitation_page.notifications.expired_title"),
        description: t("invitation_page.notifications.expired_description"),
      };
    }

    if (error.message === t("errors.invitation_cancelled")) {
      return {
        title: t("invitation_page.notifications.cancelled_title"),
        description: t("invitation_page.notifications.cancelled_description"),
      };
    }

    if (error.message === t("errors.invitation_accepted")) {
      return {
        title: t("invitation_page.notifications.accepted_title"),
        description: t("invitation_page.notifications.accepted_description"),
      };
    }

    if (error.message === t("errors.user_exists") || error.message === t("errors.keycloak_user_exists")) {
      return {
        title: t("invitation_page.notifications.email_in_use_title"),
        description: t("invitation_page.notifications.email_in_use_description"),
      };
    }

    if (error.category === "network") {
      return {
        title: t("invitation_page.notifications.server_unavailable_title"),
        description: t("invitation_page.notifications.server_unavailable_description"),
      };
    }

    return presentation;
  };

  useEffect(() => {
    if (!acceptInvitationMutation.isSuccess) {
      return;
    }

    setSuccessModalOpen(true);
    form.resetFields();
  }, [acceptInvitationMutation.isSuccess, form]);

  useEffect(() => {
    if (!acceptInvitationMutation.isError) {
      return;
    }

    const presentation = getInvitationErrorNotification(acceptInvitationMutation.error);
    notification.error({
      message: presentation.title,
      description: presentation.description,
    });
  }, [acceptInvitationMutation.error, acceptInvitationMutation.isError, notification]);

  if (!token) {
    return <StatusPanel title={t("invitation_page.not_found_title")} />;
  }

  if (invitationQuery.isLoading) {
    return <CenteredLoader />;
  }

  if (invitationQuery.isError || !invitationQuery.data) {
    return <StatusPanel status="error" title={t("invitation_page.not_found_title")} subtitle={t("invitation_page.not_found_subtitle")} />;
  }

  if (invitationQuery.data.status === "Accepted") {
    return <StatusPanel status="success" title={t("invitation_page.accepted_title")} subtitle={t("invitation_page.accepted_subtitle")} />;
  }

  if (invitationQuery.data.status === "Expired") {
    return <StatusPanel status="warning" title={t("invitation_page.expired_title")} subtitle={t("invitation_page.expired_subtitle")} />;
  }

  if (invitationQuery.data.status === "Rejected") {
    return <StatusPanel status="warning" title={t("invitation_page.unavailable_title")} subtitle={t("invitation_page.unavailable_subtitle")} />;
  }

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
      <Flex justify="flex-end" align="center" style={{ width: "100%", marginBottom: 24 }} gap="middle">
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
                void i18nInstance.changeLanguage(value);
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
                {t("invitation_page.page_title")}
              </Title>
            </div>

            <Form
              form={form}
              layout="vertical"
              onFinish={(values) => {
                acceptInvitationMutation.mutate(values);
              }}
            >
              <Form.Item label={t("admin_users.fields.email")}>
                <Input value={invitationQuery.data.email} disabled />
              </Form.Item>
              <Form.Item label={t("invitation_page.department_label")}>
                <Input value={invitationQuery.data.departmentName || "-"} disabled />
              </Form.Item>
              <Form.Item label={t("invitation_page.job_title_label")}>
                <Input value={invitationQuery.data.jobTitleName || "-"} disabled />
              </Form.Item>
              <Form.Item label={t("invitation_page.first_name_label")} name="firstName" rules={[{ required: true, message: t("invitation_page.first_name_required") }]}>
                <Input placeholder={t("invitation_page.first_name_placeholder")} />
              </Form.Item>
              <Form.Item label={t("invitation_page.last_name_label")} name="lastName" rules={[{ required: true, message: t("invitation_page.last_name_required") }]}>
                <Input placeholder={t("invitation_page.last_name_placeholder")} />
              </Form.Item>
              <Form.Item
                label={t("invitation_page.password_label")}
                name="password"
                rules={[
                  { required: true, message: t("errors.password_required") },
                  { min: 8, message: t("errors.password_min_length") },
                ]}
              >
                <Input.Password placeholder={t("invitation_page.password_placeholder")} />
              </Form.Item>
              <Form.Item
                label={t("invitation_page.confirm_password_label")}
                name="confirmPassword"
                dependencies={["password"]}
                rules={[
                  { required: true, message: t("invitation_page.confirm_password_required") },
                  ({ getFieldValue }) => ({
                    validator(_, value) {
                      if (!value || getFieldValue("password") === value) {
                        return Promise.resolve();
                      }

                      return Promise.reject(new Error(t("errors.password_mismatch")));
                    },
                  }),
                ]}
              >
                <Input.Password placeholder={t("invitation_page.confirm_password_placeholder")} />
              </Form.Item>
              <Button type="primary" htmlType="submit" block loading={acceptInvitationMutation.isPending}>
                {t("invitation_page.submit")}
              </Button>
            </Form>
          </Space>
        </Card>
      </div>

      {successModalOpen ? (
        <Suspense fallback={null}>
          <PublicActionSuccessModal
            actionLabel={t("invitation_page.go_login")}
            description={t("invitation_page.success_description")}
            onConfirm={() => {
              setSuccessModalOpen(false);
              navigate("/login", { replace: true });
            }}
            open={successModalOpen}
            successColor={designToken.colorSuccess}
            title={t("invitation_page.success_title")}
          />
        </Suspense>
      ) : null}
    </div>
  );
}
