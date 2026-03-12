import { App, Button, Card, Form, Input, Result, Space, Spin, Typography } from "antd";
import { useMutation, useQuery } from "@tanstack/react-query";
import { useNavigate, useParams } from "react-router-dom";
import { acceptInvitation, getInvitationByToken } from "../api/usersApi";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import i18n from "../../../shared/i18n/config";

const { Paragraph, Title, Text } = Typography;

export function InvitationAcceptPage() {
  const { notification } = App.useApp();
  const navigate = useNavigate();
  const { token } = useParams<{ token: string }>();
  const [form] = Form.useForm();

  const invitationQuery = useQuery({
    queryKey: ["public", "invitation", token],
    queryFn: () => getInvitationByToken(token ?? ""),
    enabled: Boolean(token),
  });

  const acceptInvitationMutation = useMutation({
    mutationFn: (values: { firstName: string; lastName: string; password: string; confirmPassword: string }) =>
      acceptInvitation(token ?? "", values),
    onSuccess: () => {
      notification.success({ message: "ยืนยันคำเชิญสำเร็จ" });
      navigate("/login", { replace: true });
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
    return <Result status="404" title="Invitation not found" />;
  }

  if (invitationQuery.isLoading) {
    return (
      <div style={{ minHeight: "100vh", display: "grid", placeItems: "center" }}>
        <Spin size="large" />
      </div>
    );
  }

  if (invitationQuery.isError || !invitationQuery.data) {
    return <Result status="error" title="Invitation not found" subTitle="ลิงก์คำเชิญไม่ถูกต้องหรือไม่มีอยู่แล้ว" />;
  }

  if (invitationQuery.data.status === "Accepted") {
    return <Result status="success" title="Invitation already accepted" subTitle="คำเชิญนี้ถูกยืนยันไปแล้ว" />;
  }

  if (invitationQuery.data.status === "Expired") {
    return <Result status="warning" title="Invitation expired" subTitle="คำเชิญนี้หมดอายุแล้ว กรุณาติดต่อผู้ดูแลระบบ" />;
  }

  if (invitationQuery.data.status === "Rejected") {
    return <Result status="warning" title="Invitation unavailable" subTitle="คำเชิญนี้ไม่สามารถใช้งานได้แล้ว" />;
  }

  return (
    <div
      style={{
        minHeight: "100vh",
        display: "grid",
        placeItems: "center",
        padding: 24,
        background: "linear-gradient(180deg, #f8fafc 0%, #e2e8f0 100%)",
      }}
    >
      <Card style={{ width: "100%", maxWidth: 560, borderRadius: 20 }}>
        <Space direction="vertical" size={20} style={{ width: "100%" }}>
          <div>
            <Title level={3} style={{ marginBottom: 8 }}>
              ยืนยันคำเชิญผู้ใช้งาน
            </Title>
            <Paragraph type="secondary" style={{ marginBottom: 0 }}>
              ตั้งค่าบัญชีสำหรับ <Text strong>{invitationQuery.data.email}</Text>
            </Paragraph>
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
            <Form.Item label="ชื่อ" name="firstName" rules={[{ required: true, message: "กรุณากรอกชื่อ" }]}>
              <Input placeholder="ชื่อ" />
            </Form.Item>
            <Form.Item label="นามสกุล" name="lastName" rules={[{ required: true, message: "กรุณากรอกนามสกุล" }]}>
              <Input placeholder="นามสกุล" />
            </Form.Item>
            <Form.Item
              label="รหัสผ่าน"
              name="password"
              rules={[
                { required: true, message: "กรุณากรอกรหัสผ่าน" },
                { min: 8, message: "รหัสผ่านต้องมีอย่างน้อย 8 ตัวอักษร" },
              ]}
            >
              <Input.Password placeholder="อย่างน้อย 8 ตัวอักษร" />
            </Form.Item>
            <Form.Item
              label="ยืนยันรหัสผ่าน"
              name="confirmPassword"
              dependencies={["password"]}
              rules={[
                { required: true, message: "กรุณายืนยันรหัสผ่าน" },
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
              <Input.Password placeholder="กรอกรหัสผ่านอีกครั้ง" />
            </Form.Item>
            <Button type="primary" htmlType="submit" block loading={acceptInvitationMutation.isPending}>
              ยืนยันคำเชิญ
            </Button>
          </Form>
        </Space>
      </Card>
    </div>
  );
}
