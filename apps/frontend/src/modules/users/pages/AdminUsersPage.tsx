import { useState } from "react";
import {
  Alert,
  App,
  Button,
  Card,
  Col,
  Divider,
  Form,
  Input,
  InputNumber,
  Row,
  Space,
  Table,
  Tag,
  Typography,
} from "antd";
import type { ColumnsType } from "antd/es/table";
import { CheckCircleOutlined, MailOutlined, TeamOutlined, UserAddOutlined } from "@ant-design/icons";
import { useLocation } from "react-router-dom";
import { useAuth } from "../../auth/hooks/useAuth";
import { useAdminUsers } from "../hooks/useAdminUsers";
import type { Invitation, RegistrationRequest, User } from "../types/users";

const { Paragraph, Text } = Typography;

function formatDate(value: string | null) {
  if (!value) {
    return "-";
  }

  return new Intl.DateTimeFormat("th-TH", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

function getDisplayActor(user: { name?: string | null; email?: string | null } | null | undefined) {
  return user?.email || user?.name || "admin@operis.local";
}

const userColumns: ColumnsType<User> = [
  {
    title: "Name",
    key: "name",
    render: (_, record) => (
      <Space direction="vertical" size={0}>
        <Text strong>{`${record.firstName} ${record.lastName}`}</Text>
        <Text type="secondary">{record.email}</Text>
      </Space>
    ),
  },
  {
    title: "Status",
    dataIndex: "status",
    render: (status: User["status"]) => <Tag color={status === "Active" ? "green" : "default"}>{status}</Tag>,
  },
  {
    title: "Created",
    dataIndex: "createdAt",
    render: (value: string) => formatDate(value),
  },
  {
    title: "Identity",
    key: "identity",
    render: (_, record) => record.keycloak ? <Tag color="blue">{record.keycloak.username}</Tag> : <Text type="secondary">Pending sync</Text>,
  },
];

const invitationColumns: ColumnsType<Invitation> = [
  {
    title: "Email",
    dataIndex: "email",
  },
  {
    title: "Invited By",
    dataIndex: "invitedBy",
  },
  {
    title: "Status",
    dataIndex: "status",
    render: (status: Invitation["status"]) => <Tag color={status === "Pending" ? "gold" : "default"}>{status}</Tag>,
  },
  {
    title: "Expires",
    dataIndex: "expiresAt",
    render: (value: string | null) => formatDate(value),
  },
];

export function AdminUsersPage() {
  const { notification } = App.useApp();
  const location = useLocation();
  const { user } = useAuth();
  const actor = getDisplayActor(user);
  const {
    usersQuery,
    registrationRequestsQuery,
    invitationsQuery,
    createInvitationMutation,
    createUserMutation,
    approveRegistrationMutation,
    rejectRegistrationMutation,
  } = useAdminUsers();

  const [inviteForm] = Form.useForm();
  const [createUserForm] = Form.useForm();
  const [rejectReason, setRejectReason] = useState<Record<string, string>>({});

  const pendingRequests = (registrationRequestsQuery.data ?? []).filter((item) => item.status === "Pending");
  const currentSection = location.pathname.includes("/admin/invitations")
    ? "invitations"
    : location.pathname.includes("/admin/registrations")
      ? "approvals"
      : "directory";

  const handleSuccess = (message: string) => {
    notification.success({ message });
  };

  const handleError = (message: string, error: unknown) => {
    notification.error({
      message,
      description: error instanceof Error ? error.message : "Unknown error",
    });
  };

  const reviewColumns: ColumnsType<RegistrationRequest> = [
    {
      title: "Applicant",
      key: "applicant",
      render: (_, record) => (
        <Space direction="vertical" size={0}>
          <Text strong>{`${record.firstName} ${record.lastName}`}</Text>
          <Text type="secondary">{record.email}</Text>
        </Space>
      ),
    },
    {
      title: "Requested",
      dataIndex: "requestedAt",
      render: (value: string) => formatDate(value),
    },
    {
      title: "Status",
      dataIndex: "status",
      render: (status: RegistrationRequest["status"]) => <Tag color={status === "Pending" ? "gold" : status === "Approved" ? "green" : "red"}>{status}</Tag>,
    },
    {
      title: "Actions",
      key: "actions",
      render: (_, record) => (
        <Space direction="vertical" style={{ width: 280 }}>
          <Space wrap>
            <Button
              type="primary"
              icon={<CheckCircleOutlined />}
              loading={approveRegistrationMutation.isPending}
              onClick={() => {
                approveRegistrationMutation.mutate(
                  { requestId: record.id, input: { reviewedBy: actor } },
                  {
                    onSuccess: () => handleSuccess(`Approved ${record.email}`),
                    onError: (error) => handleError(`Failed to approve ${record.email}`, error),
                  }
                );
              }}
            >
              Approve
            </Button>
            <Button
              danger
              loading={rejectRegistrationMutation.isPending}
              onClick={() => {
                const reason = rejectReason[record.id]?.trim() || "Not approved";
                rejectRegistrationMutation.mutate(
                  { requestId: record.id, input: { reviewedBy: actor, reason } },
                  {
                    onSuccess: () => handleSuccess(`Rejected ${record.email}`),
                    onError: (error) => handleError(`Failed to reject ${record.email}`, error),
                  }
                );
              }}
            >
              Reject
            </Button>
          </Space>
          <Input
            placeholder="Reason for rejection"
            value={rejectReason[record.id] ?? ""}
            onChange={(event) => {
              setRejectReason((current) => ({
                ...current,
                [record.id]: event.target.value,
              }));
            }}
          />
        </Space>
      ),
    },
  ];

  let pageTitle = "จัดการผู้ใช้งาน";
  let pageDescription = "แยก 3 กระบวนการชัดเจน: เพิ่มจากผู้ดูแล, เชิญผู้ใช้งาน, และลงทะเบียนพร้อมอนุมัติ";
  let pageContent: JSX.Element;

  if (currentSection === "invitations") {
    pageTitle = "เชิญผู้ใช้งาน";
    pageDescription = "สร้างคำเชิญสำหรับส่งให้ปลายทางยืนยันตัวตนและเข้าระบบภายหลัง";
    pageContent = (
      <Space direction="vertical" size={20} style={{ width: "100%" }}>
        <Card variant="borderless">
          <Typography.Title level={5}>เชิญผู้ใช้งาน</Typography.Title>
          <Paragraph type="secondary">
            ใช้สำหรับสร้างคำเชิญก่อน ตอนนี้ระบบบันทึกรายการเชิญแล้ว แต่ขั้นสร้างลิงก์ยืนยันเฉพาะรายและหน้าตอบรับยังต้องเพิ่มต่อ
          </Paragraph>
          <Form
            layout="vertical"
            form={inviteForm}
            onFinish={(values: { email: string; expiresInDays?: number }) => {
              createInvitationMutation.mutate(
                { ...values, invitedBy: actor },
                {
                  onSuccess: () => {
                    inviteForm.resetFields();
                    handleSuccess(`Invitation sent to ${values.email}`);
                  },
                  onError: (error) => handleError(`Failed to invite ${values.email}`, error),
                }
              );
            }}
          >
            <Row gutter={16}>
              <Col xs={24} md={12}>
                <Form.Item label="Email" name="email" rules={[{ required: true, type: "email" }]}>
                  <Input prefix={<MailOutlined />} placeholder="invitee@company.com" />
                </Form.Item>
              </Col>
              <Col xs={24} md={6}>
                <Form.Item label="Expires in days" name="expiresInDays">
                  <InputNumber min={1} max={90} style={{ width: "100%" }} placeholder="7" />
                </Form.Item>
              </Col>
              <Col xs={24} md={6}>
                <Form.Item label=" " colon={false}>
                  <Button
                    htmlType="submit"
                    type="primary"
                    icon={<MailOutlined />}
                    loading={createInvitationMutation.isPending}
                    block
                  >
                    Send invite
                  </Button>
                </Form.Item>
              </Col>
            </Row>
          </Form>
        </Card>

        <Card variant="borderless">
          <Typography.Title level={5}>รายการเชิญล่าสุด</Typography.Title>
          <Table
            rowKey="id"
            columns={invitationColumns}
            dataSource={invitationsQuery.data ?? []}
            loading={invitationsQuery.isLoading}
            pagination={{ pageSize: 8 }}
          />
        </Card>
      </Space>
    );
  } else if (currentSection === "approvals") {
    pageTitle = "ลงทะเบียนและอนุมัติการลงทะเบียน";
    pageDescription = "ตรวจคำขอลงทะเบียนที่ผู้ใช้ส่งเข้ามา แล้วอนุมัติหรือปฏิเสธตามนโยบาย";
    pageContent = (
      <Card variant="borderless">
        <Typography.Title level={5}>คำขอลงทะเบียนที่รออนุมัติ</Typography.Title>
        <Paragraph type="secondary">
          ใช้สำหรับคำขอที่ผู้ใช้ลงทะเบียนเข้ามาเอง แล้วให้ผู้ดูแลตรวจและอนุมัติหรือปฏิเสธ
        </Paragraph>
        <Divider />
        <Table
          rowKey="id"
          columns={reviewColumns}
          dataSource={registrationRequestsQuery.data ?? []}
          loading={registrationRequestsQuery.isLoading}
          pagination={{ pageSize: 8 }}
        />
      </Card>
    );
  } else {
    pageContent = (
      <Space direction="vertical" size={20} style={{ width: "100%" }}>
        <Card variant="borderless">
          <Typography.Title level={5}>เพิ่มผู้ใช้งานโดยผู้ดูแล</Typography.Title>
          <Paragraph type="secondary">
            ใช้สำหรับเพิ่มผู้ใช้งานจากฝั่ง admin โดยตรง ตอนนี้รองรับการเพิ่มและดูรายการแล้ว ส่วนแก้ไขและลบยังต้องเพิ่ม endpoint ฝั่ง API
          </Paragraph>
          <Form
            layout="vertical"
            form={createUserForm}
            onFinish={(values: { email: string; firstName: string; lastName: string }) => {
              createUserMutation.mutate(
                { ...values, createdBy: actor },
                {
                  onSuccess: () => {
                    createUserForm.resetFields();
                    handleSuccess(`Created ${values.email}`);
                  },
                  onError: (error) => handleError(`Failed to create ${values.email}`, error),
                }
              );
            }}
          >
            <Row gutter={16}>
              <Col xs={24} md={8}>
                <Form.Item label="Email" name="email" rules={[{ required: true, type: "email" }]}>
                  <Input placeholder="name@company.com" />
                </Form.Item>
              </Col>
              <Col xs={24} md={6}>
                <Form.Item label="First name" name="firstName" rules={[{ required: true }]}>
                  <Input placeholder="First name" />
                </Form.Item>
              </Col>
              <Col xs={24} md={6}>
                <Form.Item label="Last name" name="lastName" rules={[{ required: true }]}>
                  <Input placeholder="Last name" />
                </Form.Item>
              </Col>
              <Col xs={24} md={4}>
                <Form.Item label=" " colon={false}>
                  <Button
                    htmlType="submit"
                    type="primary"
                    icon={<UserAddOutlined />}
                    loading={createUserMutation.isPending}
                    block
                  >
                    Create
                  </Button>
                </Form.Item>
              </Col>
            </Row>
          </Form>
        </Card>

        <Card variant="borderless">
          <Typography.Title level={5}>รายการผู้ใช้งาน</Typography.Title>
          <Table
            rowKey="id"
            columns={userColumns}
            dataSource={usersQuery.data ?? []}
            loading={usersQuery.isLoading}
            pagination={{ pageSize: 8 }}
          />
        </Card>
      </Space>
    );
  }

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
            <TeamOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {pageTitle}
            </Typography.Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {pageDescription}
            </Paragraph>
          </div>
        </Space>
      </Card>

      {(usersQuery.error || registrationRequestsQuery.error || invitationsQuery.error) ? (
        <Alert
          type="error"
          showIcon
          message="Unable to load admin data"
          description={
            usersQuery.error instanceof Error
              ? usersQuery.error.message
              : registrationRequestsQuery.error instanceof Error
                ? registrationRequestsQuery.error.message
                : invitationsQuery.error instanceof Error
                  ? invitationsQuery.error.message
                  : "Unknown error"
          }
        />
      ) : null}

      {pageContent}
    </Space>
  );
}
