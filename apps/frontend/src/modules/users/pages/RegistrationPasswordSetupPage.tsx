import { App, Button, Card, Flex, Form, Input, Result, Select, Space, Spin, Typography, theme as antdTheme } from "antd";
import { BulbFilled, BulbOutlined, GlobalOutlined } from "@ant-design/icons";
import { Suspense, lazy, useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useRegistrationPasswordSetup } from "../hooks/useRegistrationPasswordSetup";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useThemeStore, type ThemeMode } from "../../../shared/store/useThemeStore";
import { useTranslation } from "react-i18next";

const { Paragraph, Title } = Typography;
const PublicActionSuccessModal = lazy(() =>
  import("../components/publicUsers/PublicActionSuccessModal").then((module) => ({ default: module.PublicActionSuccessModal }))
);

export function RegistrationPasswordSetupPage() {
  const { notification } = App.useApp();
  const { token: designToken } = antdTheme.useToken();
  const { theme, setTheme } = useThemeStore();
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const { token } = useParams<{ token: string }>();
  const [form] = Form.useForm();
  const [successModalOpen, setSuccessModalOpen] = useState(false);
  const { completeMutation, setupQuery } = useRegistrationPasswordSetup(token);
  const currentLanguage = i18n.language.startsWith("th") ? "th" : "en";
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

  useEffect(() => {
    if (!completeMutation.isSuccess) {
      return;
    }

    setSuccessModalOpen(true);
    form.resetFields();
  }, [completeMutation.isSuccess, form]);

  useEffect(() => {
    if (!completeMutation.isError) {
      return;
    }

    const presentation = getApiErrorPresentation(completeMutation.error, t("errors.complete_registration_password_setup_failed"));
    notification.error({
      message: presentation.title,
      description: presentation.description,
    });
  }, [completeMutation.error, completeMutation.isError, notification, t]);

  if (!token) {
    return <Result status="404" title={t("registration_password_setup.not_found_title")} />;
  }

  if (setupQuery.isLoading) {
    return (
      <div style={{ minHeight: "100vh", display: "grid", placeItems: "center" }}>
        <Spin size="large" />
      </div>
    );
  }

  if (setupQuery.isError || !setupQuery.data) {
    return <Result status="error" title={t("registration_password_setup.not_found_title")} subTitle={t("registration_password_setup.not_found_subtitle")} />;
  }

  if (setupQuery.data.isCompleted) {
    return <Result status="success" title={t("registration_password_setup.completed_title")} subTitle={t("registration_password_setup.completed_subtitle")} />;
  }

  if (setupQuery.data.isExpired) {
    return <Result status="warning" title={t("registration_password_setup.expired_title")} subTitle={t("registration_password_setup.expired_subtitle")} />;
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
            <GlobalOutlined style={{ fontSize: 16, color: designToken.colorTextSecondary }} />
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
              { value: "light", label: <Space><BulbOutlined /> {t("common.theme.light")}</Space> },
              { value: "dark", label: <Space><BulbFilled /> {t("common.theme.dark")}</Space> },
              { value: "system", label: <Space><BulbOutlined /> {t("common.theme.system")}</Space> },
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
                {t("registration_password_setup.page_title")}
              </Title>
              <Paragraph type="secondary" style={{ marginBottom: 0 }}>
                {t("registration_password_setup.page_description")}
              </Paragraph>
            </div>

            <Form
              form={form}
              layout="vertical"
              onFinish={(values) => {
                completeMutation.mutate(values);
              }}
            >
              <Form.Item label={t("admin_users.fields.email")}>
                <Input value={setupQuery.data.email} disabled />
              </Form.Item>
              <Form.Item label={t("admin_users.fields.department")}>
                <Input value={setupQuery.data.departmentName || "-"} disabled />
              </Form.Item>
              <Form.Item label={t("admin_users.fields.job_title")}>
                <Input value={setupQuery.data.jobTitleName || "-"} disabled />
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
              <Button type="primary" htmlType="submit" block loading={completeMutation.isPending}>
                {t("registration_password_setup.submit")}
              </Button>
            </Form>
          </Space>
        </Card>
      </div>

      {successModalOpen ? (
        <Suspense fallback={null}>
          <PublicActionSuccessModal
            actionLabel={t("registration_password_setup.go_login")}
            description={t("registration_password_setup.success_description")}
            onConfirm={() => {
              setSuccessModalOpen(false);
              navigate("/login", { replace: true });
            }}
            open={successModalOpen}
            successColor={designToken.colorSuccess}
            title={t("registration_password_setup.success_title")}
          />
        </Suspense>
      ) : null}
    </div>
  );
}
