import { App, Button, Card, Form, Input, Space, Typography } from "antd";
import { useMutation } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { changeCurrentUserPassword } from "../api/usersApi";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import i18n from "../../../shared/i18n/config";
import { useI18nLanguage } from "../../../shared/i18n/hooks/useI18nLanguage";

const { Title, Paragraph } = Typography;

export function ChangePasswordPage() {
  const [form] = Form.useForm();
  const { notification } = App.useApp();
  const navigate = useNavigate();
  const language = useI18nLanguage();
  const t = (key: string) => i18n.t(key, { lng: language });

  const changeMutation = useMutation({
    mutationFn: changeCurrentUserPassword,
    onSuccess: () => {
      notification.success({ message: t("profile.change_password_success") });
      form.resetFields();
      navigate("/app/profile");
    },
    onError: (error) => {
      const presentation = getApiErrorPresentation(error, t("profile.change_password_failed"));
      notification.error({ message: presentation.title, description: presentation.description });
    },
  });

  return (
    <div style={{ width: "100%", display: "flex", justifyContent: "center" }}>
      <Card
        variant="borderless"
        style={{
          borderRadius: 16,
          width: "100%",
          maxWidth: 520,
        }}
      >
      <Space direction="vertical" size={16} style={{ width: "100%" }}>
        <div>
          <Title level={3} style={{ marginBottom: 4 }}>
            {t("profile.change_password_title")}
          </Title>
          <Paragraph type="secondary" style={{ marginBottom: 0 }}>
            {t("profile.change_password_description")}
          </Paragraph>
        </div>

        <Form
          form={form}
          layout="vertical"
          onFinish={(values) => changeMutation.mutate(values)}
        >
          <Form.Item
            label={t("profile.current_password_label")}
            name="currentPassword"
            rules={[{ required: true, message: t("errors.password_required") }]}
          >
            <Input.Password placeholder={t("profile.current_password_placeholder")} />
          </Form.Item>
          <Form.Item
            label={t("profile.new_password_label")}
            name="newPassword"
            rules={[
              { required: true, message: t("errors.password_required") },
              { min: 8, message: t("errors.password_min_length") },
            ]}
          >
            <Input.Password placeholder={t("profile.new_password_placeholder")} />
          </Form.Item>
          <Form.Item
            label={t("profile.confirm_password_label")}
            name="confirmPassword"
            dependencies={["newPassword"]}
            rules={[
              { required: true, message: t("invitation_page.confirm_password_required") },
              ({ getFieldValue }) => ({
                validator(_, value) {
                  if (!value || getFieldValue("newPassword") === value) {
                    return Promise.resolve();
                  }

                  return Promise.reject(new Error(t("errors.password_mismatch")));
                },
              }),
            ]}
          >
            <Input.Password placeholder={t("profile.confirm_password_placeholder")} />
          </Form.Item>
          <Button type="primary" htmlType="submit" block loading={changeMutation.isPending}>
            {t("common.change_password")}
          </Button>
        </Form>
      </Space>
      </Card>
    </div>
  );
}
