import { useState, type ReactNode } from "react";
import {
  Alert,
  App,
  Button,
  Card,
  DatePicker,
  Divider,
  Form,
  Input,
  InputNumber,
  Modal,
  Select,
  Space,
  Table,
  Tag,
  Typography,
} from "antd";
import type { ColumnsType } from "antd/es/table";
import { CheckCircleOutlined, DeleteOutlined, EditOutlined, MailOutlined, TeamOutlined, UserAddOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { useLocation } from "react-router-dom";
import { useAuth } from "../../auth/hooks/useAuth";
import { useAdminUsers } from "../hooks/useAdminUsers";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import i18n from "../../../shared/i18n/config";
import type { Invitation, MasterDataItem, RegistrationRequest, User } from "../types/users";

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
    render: (status: Invitation["status"]) => (
      <Tag
        color={
          status === "Pending"
            ? "gold"
            : status === "Accepted"
              ? "green"
              : status === "Cancelled"
                ? "red"
                : "default"
        }
      >
        {status}
      </Tag>
    ),
  },
  {
    title: "Expires",
    dataIndex: "expiresAt",
    render: (value: string | null) => formatDate(value),
  },
  {
    title: "Link",
    key: "link",
    render: (_, record) => {
      const invitationUrl = `${window.location.origin}${record.invitationLink}`;
      return (
        <Space>
          <Button
            onClick={() => {
              void navigator.clipboard.writeText(invitationUrl);
            }}
          >
            Copy link
          </Button>
          <Button href={invitationUrl} target="_blank">
            Open
          </Button>
        </Space>
      );
    },
  },
  {
    title: "Actions",
    key: "actions",
    render: () => null,
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
    departmentsQuery,
    jobTitlesQuery,
    rolesQuery,
    createInvitationMutation,
    updateInvitationMutation,
    cancelInvitationMutation,
    createUserMutation,
    updateUserMutation,
    deleteUserMutation,
    approveRegistrationMutation,
    rejectRegistrationMutation,
    createDepartmentMutation,
    updateDepartmentMutation,
    deleteDepartmentMutation,
    createJobTitleMutation,
    updateJobTitleMutation,
    deleteJobTitleMutation,
  } = useAdminUsers();

  const [inviteForm] = Form.useForm();
  const [editInvitationForm] = Form.useForm();
  const [createUserForm] = Form.useForm();
  const [editUserForm] = Form.useForm();
  const [deleteUserForm] = Form.useForm();
  const [createDepartmentForm] = Form.useForm();
  const [editDepartmentForm] = Form.useForm();
  const [createJobTitleForm] = Form.useForm();
  const [editJobTitleForm] = Form.useForm();
  const [deleteDepartmentForm] = Form.useForm();
  const [deleteJobTitleForm] = Form.useForm();
  const [rejectReason, setRejectReason] = useState<Record<string, string>>({});
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [deletingUser, setDeletingUser] = useState<User | null>(null);
  const [creatingInvitation, setCreatingInvitation] = useState(false);
  const [editingInvitation, setEditingInvitation] = useState<Invitation | null>(null);
  const [editingDepartment, setEditingDepartment] = useState<MasterDataItem | null>(null);
  const [editingJobTitle, setEditingJobTitle] = useState<MasterDataItem | null>(null);
  const [creatingUser, setCreatingUser] = useState(false);
  const [creatingDepartment, setCreatingDepartment] = useState(false);
  const [deletingDepartment, setDeletingDepartment] = useState<MasterDataItem | null>(null);
  const [creatingJobTitle, setCreatingJobTitle] = useState(false);
  const [deletingJobTitle, setDeletingJobTitle] = useState<MasterDataItem | null>(null);

  const currentSection = location.pathname.includes("/admin/invitations")
    ? "invitations"
    : location.pathname.includes("/admin/master/departments")
      ? "master-departments"
    : location.pathname.includes("/admin/master/job-titles")
      ? "master-job-titles"
    : location.pathname.includes("/admin/registrations")
      ? "approvals"
      : "directory";

  const handleSuccess = (message: string) => {
    notification.success({ message });
  };

  const handleError = (message: string, error: unknown) => {
    const presentation = getApiErrorPresentation(error, message);
    notification.error({
      message: presentation.title,
      description: presentation.description,
    });
  };

  const userRoleOptions = (rolesQuery.data ?? []).map((item) => ({
    label: (
      <Space direction="vertical" size={0}>
        <Text>{item.name}</Text>
        {item.description ? <Text type="secondary">{item.description}</Text> : null}
      </Space>
    ),
    value: item.id,
  }));

  const masterColumns = (
    label: string,
    onEdit: (item: MasterDataItem) => void,
    onDelete: (item: MasterDataItem) => void,
    deleting: boolean
  ): ColumnsType<MasterDataItem> => [
    {
      title: label,
      dataIndex: "name",
    },
    {
      title: "ลำดับ",
      dataIndex: "displayOrder",
    },
    {
      title: "Actions",
      key: "actions",
      render: (_, record) => (
        <Space>
          <Button icon={<EditOutlined />} onClick={() => onEdit(record)}>
            Edit
          </Button>
          <Button danger icon={<DeleteOutlined />} loading={deleting} onClick={() => onDelete(record)}>
            Delete
          </Button>
        </Space>
      ),
    },
  ];

  const userColumns: ColumnsType<User> = [
    {
      title: "Name",
      key: "name",
      render: (_, record) => (
        <Space direction="vertical" size={0}>
          <Text strong>{`${record.keycloak?.firstName || "-"} ${record.keycloak?.lastName || ""}`.trim()}</Text>
          <Text type="secondary">{record.keycloak?.email || record.keycloak?.username || record.id || "-"}</Text>
        </Space>
      ),
    },
    {
      title: "Status",
      dataIndex: "status",
      render: (status: User["status"]) => (
        <Tag color={status === "Active" ? "green" : status === "Deleted" ? "red" : "default"}>
          {status}
        </Tag>
      ),
    },
    {
      title: "Department",
      dataIndex: "departmentName",
      render: (value: string | null) => value || "-",
    },
    {
      title: "Title",
      dataIndex: "jobTitleName",
      render: (value: string | null) => value || "-",
    },
    {
      title: "Roles",
      dataIndex: "roles",
      render: (roles: string[]) => (
        <Space wrap>
          {roles.length === 0 ? <Text type="secondary">-</Text> : roles.map((role) => <Tag key={role}>{role}</Tag>)}
        </Space>
      ),
    },
    {
      title: "Created",
      dataIndex: "createdAt",
      render: (value: string) => formatDate(value),
    },
    {
      title: "Actions",
      key: "actions",
      render: (_, record) => (
        <Space>
          <Button
            icon={<EditOutlined />}
            onClick={() => {
              const matchedRoleIds = (rolesQuery.data ?? [])
                .filter((item) => record.roles.includes(item.name))
                .map((item) => item.id);

              setEditingUser(record);
              editUserForm.setFieldsValue({
                email: record.keycloak?.email ?? "",
                firstName: record.keycloak?.firstName ?? "",
                lastName: record.keycloak?.lastName ?? "",
                departmentId: record.departmentId ?? undefined,
                jobTitleId: record.jobTitleId ?? undefined,
                roleIds: matchedRoleIds,
              });
            }}
          >
            Edit
          </Button>
          <Button
            danger
            icon={<DeleteOutlined />}
            loading={deleteUserMutation.isPending}
            onClick={() => {
              setDeletingUser(record);
              deleteUserForm.resetFields();
            }}
          >
            Delete
          </Button>
        </Space>
      ),
    },
  ];

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
                    onError: (error) => handleError(i18n.t("errors.approve_registration_failed"), error),
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
                    onError: (error) => handleError(i18n.t("errors.reject_registration_failed"), error),
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
  let pageDescription = "จัดการผู้ใช้งานจากฝั่งผู้ดูแลระบบ";
  let pageContent: ReactNode;

  if (currentSection === "invitations") {
    pageTitle = "เชิญผู้ใช้งาน";
    pageDescription = "สร้างคำเชิญสำหรับส่งให้ปลายทางยืนยันตัวตนและเข้าระบบภายหลัง";
    pageContent = (
      <Space direction="vertical" size={20} style={{ width: "100%" }}>
        <Card variant="borderless">
          <Typography.Title level={5}>เชิญผู้ใช้งาน</Typography.Title>
          <Paragraph type="secondary">
            ใช้สำหรับสร้างคำเชิญก่อน แล้วส่งลิงก์ให้ปลายทางยืนยันและตั้งค่าบัญชีด้วยตนเอง
          </Paragraph>
          <Button type="primary" icon={<MailOutlined />} onClick={() => setCreatingInvitation(true)}>
            เชิญผู้ใช้งาน
          </Button>
        </Card>

        <Card variant="borderless">
          <Typography.Title level={5}>รายการเชิญล่าสุด</Typography.Title>
          <Table
            rowKey="id"
            columns={invitationColumns.map((column) =>
              column.key !== "actions"
                ? column
                : {
                    ...column,
                    render: (_, record: Invitation) => (
                      <Space>
                        <Button
                          icon={<EditOutlined />}
                          disabled={record.status === "Accepted" || record.status === "Cancelled" || record.status === "Rejected"}
                          onClick={() => {
                            setEditingInvitation(record);
                            editInvitationForm.setFieldsValue({
                              email: record.email,
                              expiresAt: record.expiresAt ? dayjs(record.expiresAt) : undefined,
                            });
                          }}
                        >
                          Edit
                        </Button>
                        <Button
                          danger
                          disabled={record.status !== "Pending"}
                          loading={cancelInvitationMutation.isPending}
                          onClick={() => {
                            Modal.confirm({
                              title: `ยกเลิกคำเชิญ ${record.email}`,
                              content: "เมื่อยกเลิกแล้ว ลิงก์คำเชิญนี้จะใช้งานไม่ได้อีก",
                              okText: "ยืนยัน",
                              cancelText: "ยกเลิก",
                              okButtonProps: { danger: true },
                              onOk: () =>
                                new Promise<void>((resolve, reject) => {
                                  cancelInvitationMutation.mutate(record.id, {
                                    onSuccess: () => {
                                      handleSuccess(`Cancelled invitation for ${record.email}`);
                                      resolve();
                                    },
                                    onError: (error) => {
                                      handleError(i18n.t("errors.cancel_invitation_failed"), error);
                                      reject(error);
                                    },
                                  });
                                }),
                            });
                          }}
                        >
                          ยกเลิกคำเชิญ
                        </Button>
                      </Space>
                    ),
                  }
            )}
            dataSource={invitationsQuery.data ?? []}
            loading={invitationsQuery.isLoading}
            pagination={{ pageSize: 8 }}
          />
        </Card>
      </Space>
    );
  } else if (currentSection === "approvals") {
    pageTitle = "อนุมัติการลงทะเบียน";
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
    const departmentsContent = (
      <Card variant="borderless">
        <Typography.Title level={5}>Master: แผนก</Typography.Title>
        <Paragraph type="secondary">
          ใช้เพิ่ม ลบ แก้ไข master data ของแผนกก่อนนำไปผูกกับผู้ใช้งาน
        </Paragraph>
        <Space style={{ marginBottom: 16 }}>
          <Button type="primary" onClick={() => setCreatingDepartment(true)}>
            เพิ่มแผนก
          </Button>
        </Space>
        <Table
          rowKey="id"
          columns={masterColumns(
            "Department",
            (item) => {
              setEditingDepartment(item);
              editDepartmentForm.setFieldValue("editName", item.name);
              editDepartmentForm.setFieldValue("editDisplayOrder", item.displayOrder);
            },
            (item) => {
              setDeletingDepartment(item);
              deleteDepartmentForm.resetFields();
            },
            deleteDepartmentMutation.isPending
          )}
          dataSource={departmentsQuery.data ?? []}
          loading={departmentsQuery.isLoading}
          pagination={{ pageSize: 8 }}
        />
      </Card>
    );

    const jobTitlesContent = (
      <Card variant="borderless">
        <Typography.Title level={5}>Master: ตำแหน่ง</Typography.Title>
        <Paragraph type="secondary">
          ใช้เพิ่ม ลบ แก้ไข master data ของตำแหน่งก่อนนำไปผูกกับผู้ใช้งาน
        </Paragraph>
        <Space style={{ marginBottom: 16 }}>
          <Button type="primary" onClick={() => setCreatingJobTitle(true)}>
            เพิ่มตำแหน่ง
          </Button>
        </Space>
        <Table
          rowKey="id"
          columns={masterColumns(
            "Job title",
            (item) => {
              setEditingJobTitle(item);
              editJobTitleForm.setFieldValue("editName", item.name);
              editJobTitleForm.setFieldValue("editDisplayOrder", item.displayOrder);
            },
            (item) => {
              setDeletingJobTitle(item);
              deleteJobTitleForm.resetFields();
            },
            deleteJobTitleMutation.isPending
          )}
          dataSource={jobTitlesQuery.data ?? []}
          loading={jobTitlesQuery.isLoading}
          pagination={{ pageSize: 8 }}
        />
      </Card>
    );

    if (currentSection === "master-departments") {
      pageTitle = "จัดการข้อมูล Master";
      pageDescription = "จัดการข้อมูลหลักของระบบในหมวดแผนก";
      pageContent = departmentsContent;
    } else if (currentSection === "master-job-titles") {
      pageTitle = "จัดการข้อมูล Master";
      pageDescription = "จัดการข้อมูลหลักของระบบในหมวดตำแหน่ง";
      pageContent = jobTitlesContent;
    } else {
      pageContent = (
        <Space direction="vertical" size={20} style={{ width: "100%" }}>
          <Card variant="borderless">
            <Typography.Title level={5}>จัดการผู้ใช้งานโดยผู้ดูแล</Typography.Title>
            <Paragraph type="secondary">
              ใช้สำหรับเพิ่มผู้ใช้งานจากฝั่ง admin โดยตรง ตอนนี้รองรับการเพิ่มและดูรายการแล้ว ส่วนแก้ไขยังต้องเพิ่ม endpoint ฝั่ง API
            </Paragraph>
            <Button type="primary" icon={<UserAddOutlined />} onClick={() => setCreatingUser(true)}>
              เพิ่มผู้ใช้งาน
            </Button>
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
          message={i18n.t("errors.load_admin_data")}
          description={
            (() => {
              const sourceError = usersQuery.error ?? registrationRequestsQuery.error ?? invitationsQuery.error;
              const presentation = getApiErrorPresentation(sourceError, i18n.t("errors.load_admin_data"));
              return presentation.description;
            })()
          }
        />
      ) : null}

      {pageContent}

      <Modal
        title="เชิญผู้ใช้งาน"
        open={creatingInvitation}
        okText="ส่งคำเชิญ"
        cancelText="ยกเลิก"
        confirmLoading={createInvitationMutation.isPending}
        onCancel={() => {
          setCreatingInvitation(false);
          inviteForm.resetFields();
        }}
        onOk={() => {
          inviteForm
            .validateFields()
            .then((values: { email: string; expiresAt?: { endOf: (unit: string) => { toISOString: () => string } } }) => {
              createInvitationMutation.mutate(
                {
                  email: values.email,
                  invitedBy: actor,
                  expiresAt: values.expiresAt ? values.expiresAt.endOf("day").toISOString() : undefined,
                },
                {
                  onSuccess: () => {
                    setCreatingInvitation(false);
                    inviteForm.resetFields();
                    handleSuccess(`Invitation sent to ${values.email}`);
                  },
                  onError: (error) => handleError(i18n.t("errors.invite_user_failed"), error),
                }
              );
            })
            .catch(() => undefined);
        }}
      >
        <Form layout="vertical" form={inviteForm}>
          <Form.Item label="Email" name="email" rules={[{ required: true, type: "email" }]}>
            <Input prefix={<MailOutlined />} placeholder="invitee@company.com" />
          </Form.Item>
          <Form.Item label="วันหมดอายุ" name="expiresAt">
            <DatePicker
              style={{ width: "100%" }}
              format="DD/MM/YYYY"
              placeholder="เลือกวันหมดอายุ"
              disabledDate={(current) => Boolean(current && current.endOf("day").valueOf() <= Date.now())}
            />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title="แก้ไขคำเชิญ"
        open={editingInvitation !== null}
        okText="บันทึก"
        cancelText="ยกเลิก"
        confirmLoading={updateInvitationMutation.isPending}
        onCancel={() => {
          setEditingInvitation(null);
          editInvitationForm.resetFields();
        }}
        onOk={() => {
          if (!editingInvitation) {
            return;
          }

          editInvitationForm
            .validateFields()
            .then((values: { email: string; expiresAt?: { endOf: (unit: string) => { toISOString: () => string } } }) => {
              updateInvitationMutation.mutate(
                {
                  id: editingInvitation.id,
                  email: values.email,
                  expiresAt: values.expiresAt ? values.expiresAt.endOf("day").toISOString() : undefined,
                },
                {
                  onSuccess: () => {
                    setEditingInvitation(null);
                    editInvitationForm.resetFields();
                    handleSuccess(`Updated invitation for ${values.email}`);
                  },
                  onError: (error) => handleError(i18n.t("errors.update_invitation_failed"), error),
                }
              );
            })
            .catch(() => undefined);
        }}
      >
        <Form layout="vertical" form={editInvitationForm}>
          <Form.Item label="Email" name="email" rules={[{ required: true, type: "email" }]}>
            <Input prefix={<MailOutlined />} placeholder="invitee@company.com" />
          </Form.Item>
          <Form.Item label="วันหมดอายุ" name="expiresAt">
            <DatePicker
              style={{ width: "100%" }}
              format="DD/MM/YYYY"
              placeholder="เลือกวันหมดอายุ"
              disabledDate={(current) => Boolean(current && current.endOf("day").valueOf() <= Date.now())}
            />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title="เพิ่มผู้ใช้งาน"
        open={creatingUser}
        width={820}
        destroyOnHidden
        okText="บันทึก"
        cancelText="ยกเลิก"
        confirmLoading={createUserMutation.isPending}
        onCancel={() => {
          setCreatingUser(false);
          createUserForm.resetFields();
        }}
        onOk={() => {
          createUserForm
            .validateFields()
            .then((values: {
              email: string;
              firstName: string;
              lastName: string;
              password: string;
              confirmPassword: string;
              departmentId?: string;
              jobTitleId?: string;
              roles?: string[];
            }) => {
              createUserMutation.mutate(
                {
                  email: values.email,
                  firstName: values.firstName,
                  lastName: values.lastName,
                  password: values.password,
                  confirmPassword: values.confirmPassword,
                  createdBy: actor,
                  departmentId: values.departmentId,
                  jobTitleId: values.jobTitleId,
                  roleIds: values.roles,
                },
                {
                  onSuccess: () => {
                    setCreatingUser(false);
                    createUserForm.resetFields();
                    handleSuccess(`Created ${values.email}`);
                  },
                  onError: (error) => handleError(i18n.t("errors.create_user_failed"), error),
                }
              );
            })
            .catch(() => undefined);
        }}
      >
        <Form layout="vertical" form={createUserForm}>
          <Space direction="vertical" size={20} style={{ width: "100%" }}>
            <Card size="small" variant="borderless" style={{ background: "rgba(14, 165, 233, 0.06)" }}>
              <Typography.Title level={5} style={{ marginTop: 0 }}>
                ข้อมูลบัญชีผู้ใช้งาน
              </Typography.Title>
              <Form.Item label="อีเมล" name="email" rules={[{ required: true, type: "email" }]}>
                <Input placeholder="name@company.com" />
              </Form.Item>
              <Form.Item label="ชื่อ" name="firstName" rules={[{ required: true }]}>
                <Input placeholder="ชื่อ" />
              </Form.Item>
              <Form.Item label="นามสกุล" name="lastName" rules={[{ required: true }]}>
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
            </Card>

            <Card size="small" variant="borderless" style={{ background: "rgba(15, 23, 42, 0.03)" }}>
              <Typography.Title level={5} style={{ marginTop: 0 }}>
                โครงสร้างองค์กร
              </Typography.Title>
              <Form.Item label="แผนก" name="departmentId">
                <Select
                  allowClear
                  placeholder="เลือกแผนก"
                  loading={departmentsQuery.isLoading}
                  options={(departmentsQuery.data ?? []).map((item) => ({ label: item.name, value: item.id }))}
                />
              </Form.Item>
              <Form.Item label="ตำแหน่ง" name="jobTitleId">
                <Select
                  allowClear
                  placeholder="เลือกตำแหน่ง"
                  loading={jobTitlesQuery.isLoading}
                  options={(jobTitlesQuery.data ?? []).map((item) => ({ label: item.name, value: item.id }))}
                />
              </Form.Item>
            </Card>

            <Card size="small" variant="borderless" style={{ background: "rgba(22, 163, 74, 0.05)" }}>
              <Typography.Title level={5} style={{ marginTop: 0 }}>
                สิทธิ์การใช้งาน
              </Typography.Title>
              <Form.Item
                label="Roles"
                name="roles"
                extra="เลือกจากรายการ role ในฐานข้อมูลของระบบ แล้วระบบจะ map ไป Keycloak ให้อัตโนมัติ"
              >
                <Select
                  mode="multiple"
                  allowClear
                  placeholder="เลือกบทบาทผู้ใช้งาน"
                  loading={rolesQuery.isLoading}
                  options={userRoleOptions}
                />
              </Form.Item>
            </Card>
          </Space>
        </Form>
      </Modal>

      <Modal
        title="แก้ไขผู้ใช้งาน"
        open={editingUser !== null}
        width={820}
        destroyOnHidden
        okText="บันทึก"
        cancelText="ยกเลิก"
        confirmLoading={updateUserMutation.isPending}
        onCancel={() => {
          setEditingUser(null);
          editUserForm.resetFields();
        }}
        onOk={() => {
          editUserForm
            .validateFields()
            .then((values: {
              email: string;
              firstName: string;
              lastName: string;
              departmentId?: string;
              jobTitleId?: string;
              roleIds?: string[];
            }) => {
              if (!editingUser) {
                return;
              }

              updateUserMutation.mutate(
                {
                  id: editingUser.id,
                  email: values.email,
                  firstName: values.firstName,
                  lastName: values.lastName,
                  departmentId: values.departmentId,
                  jobTitleId: values.jobTitleId,
                  roleIds: values.roleIds,
                },
                {
                  onSuccess: () => {
                    handleSuccess(`Updated ${values.email}`);
                    setEditingUser(null);
                    editUserForm.resetFields();
                  },
                  onError: (error) => handleError(i18n.t("errors.update_user_failed"), error),
                }
              );
            })
            .catch(() => undefined);
        }}
      >
        <Form layout="vertical" form={editUserForm}>
          <Space direction="vertical" size={20} style={{ width: "100%" }}>
            <Card size="small" variant="borderless" style={{ background: "rgba(14, 165, 233, 0.06)" }}>
              <Typography.Title level={5} style={{ marginTop: 0 }}>
                ข้อมูลบัญชีผู้ใช้งาน
              </Typography.Title>
              <Form.Item label="อีเมล" name="email" rules={[{ required: true, type: "email" }]}>
                <Input placeholder="name@company.com" />
              </Form.Item>
              <Form.Item label="ชื่อ" name="firstName" rules={[{ required: true }]}>
                <Input placeholder="ชื่อ" />
              </Form.Item>
              <Form.Item label="นามสกุล" name="lastName" rules={[{ required: true }]}>
                <Input placeholder="นามสกุล" />
              </Form.Item>
            </Card>

            <Card size="small" variant="borderless" style={{ background: "rgba(15, 23, 42, 0.03)" }}>
              <Typography.Title level={5} style={{ marginTop: 0 }}>
                โครงสร้างองค์กร
              </Typography.Title>
              <Form.Item label="แผนก" name="departmentId">
                <Select
                  allowClear
                  placeholder="เลือกแผนก"
                  loading={departmentsQuery.isLoading}
                  options={(departmentsQuery.data ?? []).map((item) => ({ label: item.name, value: item.id }))}
                />
              </Form.Item>
              <Form.Item label="ตำแหน่ง" name="jobTitleId">
                <Select
                  allowClear
                  placeholder="เลือกตำแหน่ง"
                  loading={jobTitlesQuery.isLoading}
                  options={(jobTitlesQuery.data ?? []).map((item) => ({ label: item.name, value: item.id }))}
                />
              </Form.Item>
            </Card>

            <Card size="small" variant="borderless" style={{ background: "rgba(22, 163, 74, 0.05)" }}>
              <Typography.Title level={5} style={{ marginTop: 0 }}>
                สิทธิ์การใช้งาน
              </Typography.Title>
              <Form.Item
                label="Roles"
                name="roleIds"
                extra="เลือกจากรายการ role ในฐานข้อมูลของระบบ แล้วระบบจะ map ไป Keycloak ให้อัตโนมัติ"
              >
                <Select
                  mode="multiple"
                  allowClear
                  placeholder="เลือกบทบาทผู้ใช้งาน"
                  loading={rolesQuery.isLoading}
                  options={userRoleOptions}
                />
              </Form.Item>
            </Card>
          </Space>
        </Form>
      </Modal>

      <Modal
        title={deletingUser ? `ลบผู้ใช้งาน ${deletingUser.keycloak?.email || deletingUser.id}` : "ลบผู้ใช้งาน"}
        open={deletingUser !== null}
        okText="ยืนยันการลบ"
        cancelText="ยกเลิก"
        okButtonProps={{ danger: true }}
        confirmLoading={deleteUserMutation.isPending}
        onCancel={() => {
          setDeletingUser(null);
          deleteUserForm.resetFields();
        }}
        onOk={() => {
          if (!deletingUser) {
            return;
          }

          deleteUserForm
            .validateFields(["reason"])
            .then((values: { reason: string }) => {
              deleteUserMutation.mutate(
                { id: deletingUser.id, reason: values.reason },
                {
                  onSuccess: () => {
                    handleSuccess(`Deleted ${deletingUser.keycloak?.email || deletingUser.id}`);
                    setDeletingUser(null);
                    deleteUserForm.resetFields();
                  },
                  onError: (error) => handleError(i18n.t("errors.delete_user_failed"), error),
                }
              );
            })
            .catch(() => undefined);
        }}
      >
        <Form layout="vertical" form={deleteUserForm}>
          <Paragraph type="secondary">
            การดำเนินการนี้จะ soft delete ผู้ใช้งานในระบบ และปิดการใช้งานบัญชีที่ Keycloak
          </Paragraph>
          <Form.Item name="reason" label="เหตุผลในการลบ" rules={[{ required: true, message: "กรุณาระบุเหตุผลในการลบ" }]}>
            <Input.TextArea rows={4} placeholder="เช่น พนักงานลาออก / บัญชีสร้างผิด / ยกเลิกการใช้งาน" />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title="เพิ่มแผนก"
        open={creatingDepartment}
        okText="Create"
        confirmLoading={createDepartmentMutation.isPending}
        onCancel={() => {
          setCreatingDepartment(false);
          createDepartmentForm.resetFields();
        }}
        onOk={() => {
          createDepartmentForm
            .validateFields(["name", "displayOrder"])
            .then((values) => {
              createDepartmentMutation.mutate(
                { name: values.name, displayOrder: values.displayOrder },
                {
                  onSuccess: () => {
                    setCreatingDepartment(false);
                    createDepartmentForm.resetFields();
                    handleSuccess(`Created department ${values.name}`);
                  },
                  onError: (error) => handleError(i18n.t("errors.create_department_failed"), error),
                }
              );
            })
            .catch(() => undefined);
        }}
      >
        <Form form={createDepartmentForm} layout="vertical">
          <Form.Item name="name" label="Department name" rules={[{ required: true }]}>
            <Input placeholder="Operations" />
          </Form.Item>
          <Form.Item name="displayOrder" label="ลำดับ" rules={[{ required: true }]}>
            <InputNumber min={0} style={{ width: "100%" }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title="Edit department"
        open={editingDepartment !== null}
        okText="Save"
        confirmLoading={updateDepartmentMutation.isPending}
        onCancel={() => {
          setEditingDepartment(null);
          editDepartmentForm.resetFields();
        }}
        onOk={() => {
          editDepartmentForm
            .validateFields(["editName", "editDisplayOrder"])
            .then((values) => {
              if (!editingDepartment) {
                return;
              }

              updateDepartmentMutation.mutate(
                { id: editingDepartment.id, name: values.editName, displayOrder: values.editDisplayOrder },
                {
                  onSuccess: () => {
                    setEditingDepartment(null);
                    handleSuccess(`Updated department ${values.editName}`);
                  },
                  onError: (error) => handleError(i18n.t("errors.update_department_failed"), error),
                }
              );
            })
            .catch(() => undefined);
        }}
      >
        <Form form={editDepartmentForm} layout="vertical">
          <Form.Item name="editName" label="Department name" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="editDisplayOrder" label="ลำดับ" rules={[{ required: true }]}>
            <InputNumber min={0} style={{ width: "100%" }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={deletingDepartment ? `ลบแผนก ${deletingDepartment.name}` : "ลบแผนก"}
        open={deletingDepartment !== null}
        okText="Delete"
        okButtonProps={{ danger: true }}
        confirmLoading={deleteDepartmentMutation.isPending}
        onCancel={() => {
          setDeletingDepartment(null);
          deleteDepartmentForm.resetFields();
        }}
        onOk={() => {
          deleteDepartmentForm
            .validateFields(["reason"])
            .then((values) => {
              if (!deletingDepartment) {
                return;
              }

              deleteDepartmentMutation.mutate(
                { id: deletingDepartment.id, reason: values.reason },
                {
                  onSuccess: () => {
                    setDeletingDepartment(null);
                    deleteDepartmentForm.resetFields();
                    handleSuccess(`Deleted department ${deletingDepartment.name}`);
                  },
                  onError: (error) => handleError(i18n.t("errors.delete_department_failed"), error),
                }
              );
            })
            .catch(() => undefined);
        }}
      >
        <Form form={deleteDepartmentForm} layout="vertical">
          <Paragraph type="secondary">ยืนยันการลบแบบ soft delete และระบุเหตุผล</Paragraph>
          <Form.Item name="reason" label="เหตุผล" rules={[{ required: true, message: "กรุณาระบุเหตุผล" }]}>
            <Input.TextArea rows={4} placeholder="เช่น ยุบหน่วยงาน / สร้างรายการใหม่แทน" />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title="เพิ่มตำแหน่ง"
        open={creatingJobTitle}
        okText="Create"
        confirmLoading={createJobTitleMutation.isPending}
        onCancel={() => {
          setCreatingJobTitle(false);
          createJobTitleForm.resetFields();
        }}
        onOk={() => {
          createJobTitleForm
            .validateFields(["name", "displayOrder"])
            .then((values) => {
              createJobTitleMutation.mutate(
                { name: values.name, displayOrder: values.displayOrder },
                {
                  onSuccess: () => {
                    setCreatingJobTitle(false);
                    createJobTitleForm.resetFields();
                    handleSuccess(`Created job title ${values.name}`);
                  },
                  onError: (error) => handleError(i18n.t("errors.create_job_title_failed"), error),
                }
              );
            })
            .catch(() => undefined);
        }}
      >
        <Form form={createJobTitleForm} layout="vertical">
          <Form.Item name="name" label="Job title name" rules={[{ required: true }]}>
            <Input placeholder="Document Controller" />
          </Form.Item>
          <Form.Item name="displayOrder" label="ลำดับ" rules={[{ required: true }]}>
            <InputNumber min={0} style={{ width: "100%" }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title="Edit job title"
        open={editingJobTitle !== null}
        okText="Save"
        confirmLoading={updateJobTitleMutation.isPending}
        onCancel={() => {
          setEditingJobTitle(null);
          editJobTitleForm.resetFields();
        }}
        onOk={() => {
          editJobTitleForm
            .validateFields(["editName", "editDisplayOrder"])
            .then((values) => {
              if (!editingJobTitle) {
                return;
              }

              updateJobTitleMutation.mutate(
                { id: editingJobTitle.id, name: values.editName, displayOrder: values.editDisplayOrder },
                {
                  onSuccess: () => {
                    setEditingJobTitle(null);
                    handleSuccess(`Updated job title ${values.editName}`);
                  },
                  onError: (error) => handleError(i18n.t("errors.update_job_title_failed"), error),
                }
              );
            })
            .catch(() => undefined);
        }}
      >
        <Form form={editJobTitleForm} layout="vertical">
          <Form.Item name="editName" label="Job title name" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="editDisplayOrder" label="ลำดับ" rules={[{ required: true }]}>
            <InputNumber min={0} style={{ width: "100%" }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={deletingJobTitle ? `ลบตำแหน่ง ${deletingJobTitle.name}` : "ลบตำแหน่ง"}
        open={deletingJobTitle !== null}
        okText="Delete"
        okButtonProps={{ danger: true }}
        confirmLoading={deleteJobTitleMutation.isPending}
        onCancel={() => {
          setDeletingJobTitle(null);
          deleteJobTitleForm.resetFields();
        }}
        onOk={() => {
          deleteJobTitleForm
            .validateFields(["reason"])
            .then((values) => {
              if (!deletingJobTitle) {
                return;
              }

              deleteJobTitleMutation.mutate(
                { id: deletingJobTitle.id, reason: values.reason },
                {
                  onSuccess: () => {
                    setDeletingJobTitle(null);
                    deleteJobTitleForm.resetFields();
                    handleSuccess(`Deleted job title ${deletingJobTitle.name}`);
                  },
                  onError: (error) => handleError(i18n.t("errors.delete_job_title_failed"), error),
                }
              );
            })
            .catch(() => undefined);
        }}
      >
        <Form form={deleteJobTitleForm} layout="vertical">
          <Paragraph type="secondary">ยืนยันการลบแบบ soft delete และระบุเหตุผล</Paragraph>
          <Form.Item name="reason" label="เหตุผล" rules={[{ required: true, message: "กรุณาระบุเหตุผล" }]}>
            <Input.TextArea rows={4} placeholder="เช่น ปรับโครงสร้างตำแหน่ง / ยกเลิกรายการเดิม" />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
