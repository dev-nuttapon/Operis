import { App, Button, Card, Flex, Form, Input, Modal, Result, Select, Space, Spin, Typography, theme as antdTheme } from "antd";
import { useMutation, useQuery } from "@tanstack/react-query";
import { BulbFilled, BulbOutlined, CheckCircleFilled, GlobalOutlined } from "@ant-design/icons";
import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { acceptInvitation, getInvitationByToken } from "../api/usersApi";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import i18n from "../../../shared/i18n/config";
import { useThemeStore, type ThemeMode } from "../../../shared/store/useThemeStore";
import { useTranslation } from "react-i18next";

const { Paragraph, Title } = Typography;

export function InvitationAcceptPage() {
  const { notification } = App.useApp();
  const { token: designToken } = antdTheme.useToken();
  const { theme, setTheme } = useThemeStore();
  const { t, i18n: i18nInstance } = useTranslation();
  const navigate = useNavigate();
  const { token } = useParams<{ token: string }>();
  const [form] = Form.useForm();
  const [successModalOpen, setSuccessModalOpen] = useState(false);
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

  const invitationQuery = useQuery({
    queryKey: ["public", "invitation", token],
    queryFn: () => getInvitationByToken(token ?? ""),
    enabled: Boolean(token),
  });

  const acceptInvitationMutation = useMutation({
    mutationFn: (values: { firstName: string; lastName: string; password: string; confirmPassword: string }) =>
      acceptInvitation(token ?? "", values),
    onSuccess: () => {
      setSuccessModalOpen(true);
    },
    onError: (error) => {
      const presentation = getApiErrorPresentation(error, i18n.t("errors.accept_invitation_failed"));
      notification.error({
        message: presentation.title,
        description: presentation.description,
      });
    },
  });

  if (!token) {
    return <Result status="404" title={t("invitation_page.not_found_title")} />;
  }

  if (invitationQuery.isLoading) {
    return (
      <div style={{ minHeight: "100vh", display: "grid", placeItems: "center" }}>
        <Spin size="large" />
      </div>
    );
  }

  if (invitationQuery.isError || !invitationQuery.data) {
    return <Result status="error" title={t("invitation_page.not_found_title")} subTitle={t("invitation_page.not_found_subtitle")} />;
  }

  if (invitationQuery.data.status === "Accepted") {
    return <Result status="success" title={t("invitation_page.accepted_title")} subTitle={t("invitation_page.accepted_subtitle")} />;
  }

  if (invitationQuery.data.status === "Expired") {
    return <Result status="warning" title={t("invitation_page.expired_title")} subTitle={t("invitation_page.expired_subtitle")} />;
  }

  if (invitationQuery.data.status === "Rejected") {
    return <Result status="warning" title={t("invitation_page.unavailable_title")} subTitle={t("invitation_page.unavailable_subtitle")} />;
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
            <GlobalOutlined style={{ fontSize: 16, color: designToken.colorTextSecondary }} />
            <Select
              value={currentLanguage}
              variant="filled"
              popupMatchSelectWidth={false}
              style={{ width: 128 }}
              onChange={(value: "th" | "en") => {
                void i18nInstance.changeLanguage(value);
              }}
              options={[
                { value: "en", label: t("common.language_en", { defaultValue: "English" }) },
                { value: "th", label: t("common.language_th", { defaultValue: "Thai" }) },
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
              <Form.Item label="Email">
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
                  { required: true, message: "กรุณากรอกรหัสผ่าน" },
                  { min: 8, message: "รหัสผ่านต้องมีอย่างน้อย 8 ตัวอักษร" },
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

                      return Promise.reject(new Error("รหัสผ่านและยืนยันรหัสผ่านไม่ตรงกัน"));
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

      <Modal
        title={t("invitation_page.success_title")}
        open={successModalOpen}
        closable={false}
        maskClosable={false}
        keyboard={false}
        footer={[
          <Flex key="actions" justify="center">
            <Button
              type="primary"
              onClick={() => {
                setSuccessModalOpen(false);
                navigate("/login", { replace: true });
              }}
            >
              {t("invitation_page.go_login")}
            </Button>
          </Flex>,
        ]}
      >
        <Flex vertical align="center" gap={12} style={{ textAlign: "center", padding: "8px 0" }}>
          <CheckCircleFilled style={{ fontSize: 56, color: designToken.colorSuccess }} />
          <Title level={4} style={{ margin: 0 }}>
            {t("invitation_page.success_title")}
          </Title>
          <Paragraph style={{ marginBottom: 0, maxWidth: 360, whiteSpace: "pre-line" }}>
            {t("invitation_page.success_description")}
          </Paragraph>
        </Flex>
      </Modal>
    </div>
  );
}
