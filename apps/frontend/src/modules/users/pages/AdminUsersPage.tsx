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
import type { SortOrder, SorterResult } from "antd/es/table/interface";
import { CheckCircleOutlined, DeleteOutlined, EditOutlined, EyeOutlined, MailOutlined, PlusOutlined, TeamOutlined, UserAddOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import type { Dayjs } from "dayjs";
import { useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../auth/hooks/useAuth";
import { useAdminUsers } from "../hooks/useAdminUsers";
import { ApiError, getApiErrorPresentation } from "../../../shared/lib/apiClient";
import type { Invitation, InvitationStatus, MasterDataItem, RegistrationRequest, RegistrationRequestStatus, User, UserStatus } from "../types/users";

const { Paragraph, Text } = Typography;

function formatDate(value: string | null, language: string) {
  if (!value) {
    return "-";
  }

  return new Intl.DateTimeFormat(language.startsWith("th") ? "th-TH" : "en-US", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

function getDisplayActor(user: { name?: string | null; email?: string | null } | null | undefined) {
  return user?.email || user?.name || "admin@operis.local";
}

function toApiSortOrder(order?: SortOrder): "asc" | "desc" | undefined {
  if (order === "ascend") return "asc";
  if (order === "descend") return "desc";
  return undefined;
}

function toRange(range?: [Dayjs | null, Dayjs | null]) {
  return {
    from: range?.[0] ? range[0].startOf("day").toISOString() : undefined,
    to: range?.[1] ? range[1].endOf("day").toISOString() : undefined,
  };
}

export function AdminUsersPage() {
  const { t, i18n: i18nInstance } = useTranslation();
  const { notification } = App.useApp();
  const location = useLocation();
  const { user } = useAuth();
  const currentLanguage = i18nInstance.language;
  const actor = getDisplayActor(user);
  const [usersPaging, setUsersPaging] = useState({
    page: 1,
    pageSize: 10,
    search: "",
    status: undefined as UserStatus | undefined,
    from: undefined as string | undefined,
    to: undefined as string | undefined,
    sortBy: "createdAt",
    sortOrder: "desc" as "asc" | "desc",
  });
  const [registrationPaging, setRegistrationPaging] = useState({
    page: 1,
    pageSize: 10,
    search: "",
    status: undefined as RegistrationRequestStatus | undefined,
    from: undefined as string | undefined,
    to: undefined as string | undefined,
    sortBy: "requestedAt",
    sortOrder: "desc" as "asc" | "desc",
  });
  const [invitationPaging, setInvitationPaging] = useState({
    page: 1,
    pageSize: 10,
    search: "",
    status: undefined as InvitationStatus | undefined,
    from: undefined as string | undefined,
    to: undefined as string | undefined,
    sortBy: "invitedAt",
    sortOrder: "desc" as "asc" | "desc",
  });
  const [departmentPaging, setDepartmentPaging] = useState({ page: 1, pageSize: 10, search: "", sortBy: "displayOrder", sortOrder: "asc" as "asc" | "desc" });
  const [jobTitlePaging, setJobTitlePaging] = useState({ page: 1, pageSize: 10, search: "", sortBy: "displayOrder", sortOrder: "asc" as "asc" | "desc" });
  const {
    usersQuery,
    registrationRequestsQuery,
    invitationsQuery,
    departmentsQuery,
    jobTitlesQuery,
    departmentOptionsQuery,
    jobTitleOptionsQuery,
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
  } = useAdminUsers({
    users: usersPaging,
    registrationRequests: registrationPaging,
    invitations: invitationPaging,
    departments: departmentPaging,
    jobTitles: jobTitlePaging,
  });

  const [inviteForm] = Form.useForm();
  const [editInvitationForm] = Form.useForm();
  const [createUserForm] = Form.useForm();
  const [editUserForm] = Form.useForm();
  const [deleteUserForm] = Form.useForm();
  const [reviewRegistrationForm] = Form.useForm();
  const [createDepartmentForm] = Form.useForm();
  const [editDepartmentForm] = Form.useForm();
  const [createJobTitleForm] = Form.useForm();
  const [editJobTitleForm] = Form.useForm();
  const [deleteDepartmentForm] = Form.useForm();
  const [deleteJobTitleForm] = Form.useForm();
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [deletingUser, setDeletingUser] = useState<User | null>(null);
  const [creatingInvitation, setCreatingInvitation] = useState(false);
  const [editingInvitation, setEditingInvitation] = useState<Invitation | null>(null);
  const [viewingInvitation, setViewingInvitation] = useState<Invitation | null>(null);
  const [viewingRegistrationLink, setViewingRegistrationLink] = useState<RegistrationRequest | null>(null);
  const [editingDepartment, setEditingDepartment] = useState<MasterDataItem | null>(null);
  const [editingJobTitle, setEditingJobTitle] = useState<MasterDataItem | null>(null);
  const [creatingUser, setCreatingUser] = useState(false);
  const [creatingDepartment, setCreatingDepartment] = useState(false);
  const [deletingDepartment, setDeletingDepartment] = useState<MasterDataItem | null>(null);
  const [creatingJobTitle, setCreatingJobTitle] = useState(false);
  const [deletingJobTitle, setDeletingJobTitle] = useState<MasterDataItem | null>(null);
  const [managingRegistration, setManagingRegistration] = useState<RegistrationRequest | null>(null);

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
    const presentation = getAdminErrorNotification(error, message);
    notification.error({
      message: presentation.title,
      description: presentation.description,
    });
  };

  const getAdminErrorNotification = (error: unknown, fallbackTitle: string) => {
    const presentation = getApiErrorPresentation(error, fallbackTitle);

    if (!(error instanceof ApiError)) {
      return presentation;
    }

    if (error.message === t("errors.user_exists") || error.message === t("errors.keycloak_user_exists")) {
      return {
        title: t("admin_users.notifications.email_in_use_title"),
        description: t("admin_users.notifications.email_in_use_description"),
      };
    }

    if (error.message === t("errors.pending_invitation_exists")) {
      return {
        title: t("admin_users.notifications.pending_invitation_title"),
        description: t("admin_users.notifications.pending_invitation_description"),
      };
    }

    if (error.message === t("errors.pending_registration_exists")) {
      return {
        title: t("admin_users.notifications.pending_request_title"),
        description: t("admin_users.notifications.pending_request_description"),
      };
    }

    if (error.message === t("errors.invitation_accepted")) {
      return {
        title: t("admin_users.notifications.invitation_accepted_title"),
        description: t("admin_users.notifications.invitation_accepted_description"),
      };
    }

    if (error.message === t("errors.invitation_expired")) {
      return {
        title: t("admin_users.notifications.invitation_expired_title"),
        description: t("admin_users.notifications.invitation_expired_description"),
      };
    }

    if (error.category === "network") {
      return {
        title: t("admin_users.notifications.server_unavailable_title"),
        description: t("admin_users.notifications.server_unavailable_description"),
      };
    }

    return presentation;
  };

  const copyToClipboard = async (value: string, successMessage: string) => {
    await navigator.clipboard.writeText(value);
    handleSuccess(successMessage);
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
  const departmentOptions = (departmentOptionsQuery.data?.items ?? []).map((item) => ({ label: item.name, value: item.id }));
  const jobTitleOptions = (jobTitleOptionsQuery.data?.items ?? []).map((item) => ({ label: item.name, value: item.id }));

  const invitationColumns: ColumnsType<Invitation> = [
    {
      title: t("admin_users.columns.email"),
      dataIndex: "email",
      sorter: true,
    },
    {
      title: t("admin_users.columns.invited_by"),
      dataIndex: "invitedBy",
    },
    {
      title: t("admin_users.columns.department"),
      dataIndex: "departmentName",
      render: (value: string | null) => value || "-",
    },
    {
      title: t("admin_users.columns.job_title"),
      dataIndex: "jobTitleName",
      render: (value: string | null) => value || "-",
    },
    {
      title: t("admin_users.columns.status"),
      dataIndex: "status",
      sorter: true,
      render: (status: Invitation["status"]) => (
        <Tag
          color={
            status === "Pending"
              ? "gold"
              : status === "Accepted"
                ? "green"
                : status === "Cancelled"
                  ? "volcano"
                  : status === "Rejected"
                    ? "red"
                    : "default"
          }
        >
          {status}
        </Tag>
      ),
    },
    {
      title: t("admin_users.columns.expires_at"),
      dataIndex: "expiresAt",
      sorter: true,
      render: (value: string | null) => formatDate(value, currentLanguage),
    },
    {
      title: t("admin_users.columns.actions"),
      key: "actions",
      render: () => null,
    },
  ];

  const userStatusOptions = [
    { value: "Active", label: "Active" },
    { value: "Rejected", label: "Rejected" },
    { value: "Deleted", label: "Deleted" },
  ];

  const invitationStatusOptions = [
    { value: "Pending", label: "Pending" },
    { value: "Accepted", label: "Accepted" },
    { value: "Rejected", label: "Rejected" },
    { value: "Expired", label: "Expired" },
    { value: "Cancelled", label: "Cancelled" },
  ];

  const registrationStatusOptions = [
    { value: "Pending", label: "Pending" },
    { value: "Approved", label: "Approved" },
    { value: "Rejected", label: "Rejected" },
  ];

  const masterColumns = (
    label: string,
    onEdit: (item: MasterDataItem) => void,
    onDelete: (item: MasterDataItem) => void,
    deleting: boolean
  ): ColumnsType<MasterDataItem> => [
    {
      title: label,
      dataIndex: "name",
      sorter: true,
    },
    {
      title: t("admin_users.master.display_order"),
      dataIndex: "displayOrder",
      sorter: true,
    },
    {
      title: t("admin_users.columns.actions"),
      key: "actions",
      render: (_, record) => (
        <Space>
          <Button icon={<EditOutlined />} onClick={() => onEdit(record)}>
            {t("common.actions.edit")}
          </Button>
          <Button danger icon={<DeleteOutlined />} loading={deleting} onClick={() => onDelete(record)}>
            {t("common.actions.delete")}
          </Button>
        </Space>
      ),
    },
  ];

  const userColumns: ColumnsType<User> = [
    {
      title: t("admin_users.columns.name"),
      key: "name",
      render: (_, record) => (
        <Space direction="vertical" size={0}>
          <Text strong>{`${record.keycloak?.firstName || "-"} ${record.keycloak?.lastName || ""}`.trim()}</Text>
          <Text type="secondary">{record.keycloak?.email || record.keycloak?.username || record.id || "-"}</Text>
        </Space>
      ),
    },
    {
      title: t("admin_users.columns.status"),
      dataIndex: "status",
      sorter: true,
      render: (status: User["status"]) => (
        <Tag color={status === "Active" ? "green" : status === "Deleted" ? "red" : "default"}>
          {status}
        </Tag>
      ),
    },
    {
      title: t("admin_users.columns.department"),
      dataIndex: "departmentName",
      render: (value: string | null) => value || "-",
    },
    {
      title: t("admin_users.columns.job_title"),
      dataIndex: "jobTitleName",
      render: (value: string | null) => value || "-",
    },
    {
      title: t("admin_users.columns.roles"),
      dataIndex: "roles",
      render: (roles: string[]) => (
        <Space wrap>
          {roles.length === 0 ? <Text type="secondary">-</Text> : roles.map((role) => <Tag key={role}>{role}</Tag>)}
        </Space>
      ),
    },
    {
      title: t("admin_users.columns.created_at"),
      dataIndex: "createdAt",
      sorter: true,
      render: (value: string) => formatDate(value, currentLanguage),
    },
    {
      title: t("admin_users.columns.actions"),
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
            {t("common.actions.edit")}
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
            {t("common.actions.delete")}
          </Button>
        </Space>
      ),
    },
  ];

  const reviewColumns: ColumnsType<RegistrationRequest> = [
    {
      title: t("admin_users.columns.applicant"),
      key: "applicant",
      render: (_, record) => (
        <Space direction="vertical" size={0}>
          <Text strong>{`${record.firstName} ${record.lastName}`}</Text>
          <Text type="secondary">{record.email}</Text>
        </Space>
      ),
    },
    {
      title: t("admin_users.columns.requested_at"),
      dataIndex: "requestedAt",
      sorter: true,
      render: (value: string) => formatDate(value, currentLanguage),
    },
    {
      title: t("admin_users.columns.department"),
      dataIndex: "departmentName",
      render: (value: string | null) => value || "-",
    },
    {
      title: t("admin_users.columns.job_title"),
      dataIndex: "jobTitleName",
      render: (value: string | null) => value || "-",
    },
    {
      title: t("admin_users.columns.status"),
      dataIndex: "status",
      sorter: true,
      render: (status: RegistrationRequest["status"]) => (
        <Tag color={status === "Pending" ? "gold" : status === "Approved" ? "green" : "red"}>
          {status}
        </Tag>
      ),
    },
    {
      title: t("admin_users.columns.actions"),
      key: "actions",
      render: (_, record) => (
        record.status === "Pending" ? (
          <Button
            icon={<CheckCircleOutlined />}
            onClick={() => {
              setManagingRegistration(record);
              reviewRegistrationForm.setFieldsValue({
                action: "approve",
                reason: "",
              });
            }}
          >
            {t("common.actions.manage")}
          </Button>
        ) : record.status === "Approved" && record.passwordSetupLink && !record.passwordSetupCompletedAt ? (
          <Button
            icon={<EyeOutlined />}
            onClick={() => {
              setViewingRegistrationLink(record);
            }}
          >
            {t("admin_users.registration.view_setup_link")}
          </Button>
        ) : (
          <Text type="secondary">-</Text>
        )
      ),
    },
  ];

  let pageTitle = t("admin_users.directory.page_title");
  let pageDescription = t("admin_users.directory.page_description");
  let pageContent: ReactNode;

  if (currentSection === "invitations") {
    pageTitle = t("admin_users.invitations.page_title");
    pageDescription = t("admin_users.invitations.page_description");
    pageContent = (
      <Space direction="vertical" size={20} style={{ width: "100%" }}>
        <Card variant="borderless">
          <Typography.Title level={5}>{t("admin_users.invitations.latest_title")}</Typography.Title>
          <Space
            wrap
            size={[12, 12]}
            style={{ width: "100%", marginBottom: 16, justifyContent: "space-between" }}
          >
            <Input.Search
              allowClear
              style={{ width: 320 }}
              placeholder={t("admin_users.placeholders.search_invitations")}
              onSearch={(value) => setInvitationPaging((current) => ({ ...current, page: 1, search: value }))}
              onChange={(event) => {
                if (event.target.value === "") {
                  setInvitationPaging((current) => ({ ...current, page: 1, search: "" }));
                }
              }}
            />
            <Select
              allowClear
              style={{ width: 180 }}
              placeholder={t("admin_users.placeholders.select_status")}
              options={invitationStatusOptions}
              value={invitationPaging.status}
              onChange={(value) => setInvitationPaging((current) => ({ ...current, page: 1, status: value }))}
            />
            <DatePicker.RangePicker
              value={[
                invitationPaging.from ? dayjs(invitationPaging.from) : null,
                invitationPaging.to ? dayjs(invitationPaging.to) : null,
              ]}
              onChange={(range) => {
                const normalized = toRange(range as [Dayjs | null, Dayjs | null] | undefined);
                setInvitationPaging((current) => ({ ...current, page: 1, ...normalized }));
              }}
            />
            <Button type="primary" icon={<MailOutlined />} size="large" onClick={() => setCreatingInvitation(true)}>
              {t("admin_users.invitations.open_create")}
            </Button>
          </Space>
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
                          icon={<EyeOutlined />}
                          disabled={record.status === "Accepted"}
                          onClick={() => {
                            setViewingInvitation(record);
                          }}
                        >
                          {t("common.actions.view")}
                        </Button>
                        <Button
                          icon={<EditOutlined />}
                          disabled={record.status === "Accepted" || record.status === "Cancelled" || record.status === "Rejected"}
                          onClick={() => {
                            setEditingInvitation(record);
                            editInvitationForm.setFieldsValue({
                              email: record.email,
                              departmentId: record.departmentId ?? undefined,
                              jobTitleId: record.jobTitleId ?? undefined,
                              expiresAt: record.expiresAt ? dayjs(record.expiresAt) : undefined,
                            });
                          }}
                        >
                          {t("common.actions.edit")}
                        </Button>
                        <Button
                          danger
                          disabled={record.status === "Accepted"}
                          loading={cancelInvitationMutation.isPending}
                          onClick={() => {
                            Modal.confirm({
                              title: t("admin_users.invitations.cancel_title", { email: record.email }),
                              content: t("admin_users.invitations.cancel_description"),
                              okText: t("common.actions.confirm"),
                              cancelText: t("common.actions.cancel"),
                              okButtonProps: { danger: true },
                              onOk: () =>
                                new Promise<void>((resolve, reject) => {
                                  cancelInvitationMutation.mutate(record.id, {
                                    onSuccess: () => {
                                      handleSuccess(t("admin_users.messages.invitation_cancelled", { email: record.email }));
                                      resolve();
                                    },
                                    onError: (error) => {
                                      handleError(t("errors.cancel_invitation_failed"), error);
                                      reject(error);
                                    },
                                  });
                                }),
                            });
                          }}
                        >
                          {t("admin_users.invitations.cancel_action")}
                        </Button>
                      </Space>
                    ),
                  }
            )}
            dataSource={invitationsQuery.data?.items ?? []}
            loading={invitationsQuery.isLoading}
            pagination={{
              current: invitationsQuery.data?.page ?? invitationPaging.page,
              pageSize: invitationsQuery.data?.pageSize ?? invitationPaging.pageSize,
              total: invitationsQuery.data?.total ?? 0,
              showSizeChanger: true,
              pageSizeOptions: [10, 25, 50, 100],
            }}
            onChange={(pagination, _, sorter) => {
              const sort = sorter as SorterResult<Invitation>;
              setInvitationPaging((current) => ({
                ...current,
                page: pagination.current ?? current.page,
                pageSize: pagination.pageSize ?? current.pageSize,
                sortBy: typeof sort.field === "string" ? sort.field : current.sortBy,
                sortOrder: toApiSortOrder(sort.order) ?? current.sortOrder,
              }));
            }}
          />
        </Card>
      </Space>
    );
  } else if (currentSection === "approvals") {
    pageTitle = t("admin_users.registration.page_title");
    pageDescription = t("admin_users.registration.page_description");
    pageContent = (
      <Card variant="borderless">
        <Typography.Title level={5}>{t("admin_users.registration.pending_title")}</Typography.Title>
        <Divider />
        <Space wrap size={12} style={{ marginBottom: 16 }}>
          <Input.Search
            allowClear
            style={{ width: 320 }}
            placeholder={t("admin_users.placeholders.search_registrations")}
            onSearch={(value) => setRegistrationPaging((current) => ({ ...current, page: 1, search: value }))}
            onChange={(event) => {
              if (event.target.value === "") {
                setRegistrationPaging((current) => ({ ...current, page: 1, search: "" }));
              }
            }}
          />
          <Select
            allowClear
            style={{ width: 180 }}
            placeholder={t("admin_users.placeholders.select_status")}
            options={registrationStatusOptions}
            value={registrationPaging.status}
            onChange={(value) => setRegistrationPaging((current) => ({ ...current, page: 1, status: value }))}
          />
          <DatePicker.RangePicker
            value={[
              registrationPaging.from ? dayjs(registrationPaging.from) : null,
              registrationPaging.to ? dayjs(registrationPaging.to) : null,
            ]}
            onChange={(range) => {
              const normalized = toRange(range as [Dayjs | null, Dayjs | null] | undefined);
              setRegistrationPaging((current) => ({ ...current, page: 1, ...normalized }));
            }}
          />
        </Space>
        <Table
          rowKey="id"
          columns={reviewColumns}
          dataSource={registrationRequestsQuery.data?.items ?? []}
          loading={registrationRequestsQuery.isLoading}
          pagination={{
            current: registrationRequestsQuery.data?.page ?? registrationPaging.page,
            pageSize: registrationRequestsQuery.data?.pageSize ?? registrationPaging.pageSize,
            total: registrationRequestsQuery.data?.total ?? 0,
            showSizeChanger: true,
            pageSizeOptions: [10, 25, 50, 100],
          }}
          onChange={(pagination, _, sorter) => {
            const sort = sorter as SorterResult<RegistrationRequest>;
            setRegistrationPaging((current) => ({
              ...current,
              page: pagination.current ?? current.page,
              pageSize: pagination.pageSize ?? current.pageSize,
              sortBy: typeof sort.field === "string" ? sort.field : current.sortBy,
              sortOrder: toApiSortOrder(sort.order) ?? current.sortOrder,
            }));
          }}
        />
      </Card>
    );
  } else {
    const departmentsContent = (
      <Card variant="borderless">
        <Typography.Title level={5}>{t("admin_users.master.departments_title")}</Typography.Title>
        <Paragraph type="secondary">
          {t("admin_users.master.departments_description")}
        </Paragraph>
        <Space
          wrap
          style={{ width: "100%", marginBottom: 16, justifyContent: "space-between" }}
          size={[12, 12]}
        >
          <Input.Search
            allowClear
            style={{ width: 360, maxWidth: "100%" }}
            placeholder={t("admin_users.placeholders.search_departments")}
            onSearch={(value) => setDepartmentPaging((current) => ({ ...current, page: 1, search: value }))}
          />
          <Button type="primary" icon={<PlusOutlined />} size="large" onClick={() => setCreatingDepartment(true)}>
            {t("admin_users.master.create_department")}
          </Button>
        </Space>
        <Table
          rowKey="id"
          columns={masterColumns(
            t("admin_users.columns.department"),
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
          dataSource={departmentsQuery.data?.items ?? []}
          loading={departmentsQuery.isLoading}
          pagination={{
            current: departmentsQuery.data?.page ?? departmentPaging.page,
            pageSize: departmentsQuery.data?.pageSize ?? departmentPaging.pageSize,
            total: departmentsQuery.data?.total ?? 0,
            showSizeChanger: true,
            pageSizeOptions: [10, 25, 50, 100],
          }}
          onChange={(pagination, _, sorter) => {
            const sort = sorter as SorterResult<MasterDataItem>;
            setDepartmentPaging((current) => ({
              ...current,
              page: pagination.current ?? current.page,
              pageSize: pagination.pageSize ?? current.pageSize,
              sortBy: typeof sort.field === "string" ? sort.field : current.sortBy,
              sortOrder: toApiSortOrder(sort.order) ?? current.sortOrder,
            }));
          }}
        />
      </Card>
    );

    const jobTitlesContent = (
      <Card variant="borderless">
        <Typography.Title level={5}>{t("admin_users.master.job_titles_title")}</Typography.Title>
        <Paragraph type="secondary">
          {t("admin_users.master.job_titles_description")}
        </Paragraph>
        <Space
          wrap
          style={{ width: "100%", marginBottom: 16, justifyContent: "space-between" }}
          size={[12, 12]}
        >
          <Input.Search
            allowClear
            style={{ width: 360, maxWidth: "100%" }}
            placeholder={t("admin_users.placeholders.search_job_titles")}
            onSearch={(value) => setJobTitlePaging((current) => ({ ...current, page: 1, search: value }))}
          />
          <Button type="primary" icon={<PlusOutlined />} size="large" onClick={() => setCreatingJobTitle(true)}>
            {t("admin_users.master.create_job_title")}
          </Button>
        </Space>
        <Table
          rowKey="id"
          columns={masterColumns(
            t("admin_users.columns.job_title"),
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
          dataSource={jobTitlesQuery.data?.items ?? []}
          loading={jobTitlesQuery.isLoading}
          pagination={{
            current: jobTitlesQuery.data?.page ?? jobTitlePaging.page,
            pageSize: jobTitlesQuery.data?.pageSize ?? jobTitlePaging.pageSize,
            total: jobTitlesQuery.data?.total ?? 0,
            showSizeChanger: true,
            pageSizeOptions: [10, 25, 50, 100],
          }}
          onChange={(pagination, _, sorter) => {
            const sort = sorter as SorterResult<MasterDataItem>;
            setJobTitlePaging((current) => ({
              ...current,
              page: pagination.current ?? current.page,
              pageSize: pagination.pageSize ?? current.pageSize,
              sortBy: typeof sort.field === "string" ? sort.field : current.sortBy,
              sortOrder: toApiSortOrder(sort.order) ?? current.sortOrder,
            }));
          }}
        />
      </Card>
    );

    if (currentSection === "master-departments") {
      pageTitle = t("admin_users.master.page_title");
      pageDescription = t("admin_users.master.departments_page_description");
      pageContent = departmentsContent;
    } else if (currentSection === "master-job-titles") {
      pageTitle = t("admin_users.master.page_title");
      pageDescription = t("admin_users.master.job_titles_page_description");
      pageContent = jobTitlesContent;
    } else {
      pageContent = (
        <Space direction="vertical" size={20} style={{ width: "100%" }}>
          <Card variant="borderless">
            <Typography.Title level={5}>{t("admin_users.directory.list_title")}</Typography.Title>
            <Space
              wrap
              size={[12, 12]}
              style={{ width: "100%", marginBottom: 16, justifyContent: "space-between" }}
            >
              <Input.Search
                allowClear
                style={{ width: 320 }}
                placeholder={t("admin_users.placeholders.search_users")}
                onSearch={(value) => setUsersPaging((current) => ({ ...current, page: 1, search: value }))}
                onChange={(event) => {
                  if (event.target.value === "") {
                    setUsersPaging((current) => ({ ...current, page: 1, search: "" }));
                  }
                }}
              />
              <Select
                allowClear
                style={{ width: 180 }}
                placeholder={t("admin_users.placeholders.select_status")}
                options={userStatusOptions}
                value={usersPaging.status}
                onChange={(value) => setUsersPaging((current) => ({ ...current, page: 1, status: value }))}
              />
              <DatePicker.RangePicker
                value={[
                  usersPaging.from ? dayjs(usersPaging.from) : null,
                  usersPaging.to ? dayjs(usersPaging.to) : null,
                ]}
                onChange={(range) => {
                  const normalized = toRange(range as [Dayjs | null, Dayjs | null] | undefined);
                  setUsersPaging((current) => ({ ...current, page: 1, ...normalized }));
                }}
              />
              <Button type="primary" icon={<UserAddOutlined />} size="large" onClick={() => setCreatingUser(true)}>
                {t("admin_users.directory.create_user")}
              </Button>
            </Space>
            <Table
              rowKey="id"
              columns={userColumns}
              dataSource={usersQuery.data?.items ?? []}
              loading={usersQuery.isLoading}
              pagination={{
                current: usersQuery.data?.page ?? usersPaging.page,
                pageSize: usersQuery.data?.pageSize ?? usersPaging.pageSize,
                total: usersQuery.data?.total ?? 0,
                showSizeChanger: true,
                pageSizeOptions: [10, 25, 50, 100],
              }}
              onChange={(pagination, _, sorter) => {
                const sort = sorter as SorterResult<User>;
                setUsersPaging((current) => ({
                  ...current,
                  page: pagination.current ?? current.page,
                  pageSize: pagination.pageSize ?? current.pageSize,
                  sortBy: typeof sort.field === "string" ? sort.field : current.sortBy,
                  sortOrder: toApiSortOrder(sort.order) ?? current.sortOrder,
                }));
              }}
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
          message={t("errors.load_admin_data")}
          description={
            (() => {
              const sourceError = usersQuery.error ?? registrationRequestsQuery.error ?? invitationsQuery.error;
              const presentation = getApiErrorPresentation(sourceError, t("errors.load_admin_data"));
              return presentation.description;
            })()
          }
        />
      ) : null}

      {pageContent}

      <Modal
        title={managingRegistration ? t("admin_users.registration.manage_modal_title_with_email", { email: managingRegistration.email }) : t("admin_users.registration.manage_modal_title")}
        open={managingRegistration !== null}
        okText={t("common.actions.confirm")}
        cancelText={t("common.actions.cancel")}
        confirmLoading={approveRegistrationMutation.isPending || rejectRegistrationMutation.isPending}
        onCancel={() => {
          setManagingRegistration(null);
          reviewRegistrationForm.resetFields();
        }}
        onOk={() => {
          if (!managingRegistration) {
            return;
          }

          reviewRegistrationForm
            .validateFields()
            .then((values: { action: "approve" | "reject"; reason?: string }) => {
              if (values.action === "approve") {
                approveRegistrationMutation.mutate(
                  { requestId: managingRegistration.id, input: { reviewedBy: actor } },
                  {
                    onSuccess: (approvedRequest) => {
                      handleSuccess(t("admin_users.messages.registration_approved", { email: managingRegistration.email }));
                      setViewingRegistrationLink(approvedRequest);
                      setManagingRegistration(null);
                      reviewRegistrationForm.resetFields();
                    },
                    onError: (error) => handleError(t("errors.approve_registration_failed"), error),
                  }
                );
                return;
              }

              const reason = values.reason?.trim() || t("admin_users.registration.default_rejection_reason");
              rejectRegistrationMutation.mutate(
                { requestId: managingRegistration.id, input: { reviewedBy: actor, reason } },
                {
                  onSuccess: () => {
                    handleSuccess(t("admin_users.messages.registration_rejected", { email: managingRegistration.email }));
                    setManagingRegistration(null);
                    reviewRegistrationForm.resetFields();
                  },
                  onError: (error) => handleError(t("errors.reject_registration_failed"), error),
                }
              );
            })
            .catch(() => undefined);
        }}
      >
        <Form form={reviewRegistrationForm} layout="vertical" initialValues={{ action: "approve", reason: "" }}>
          <Form.Item label={t("admin_users.registration.manage_action_label")} name="action" rules={[{ required: true }]}>
            <Select
              options={[
                { value: "approve", label: t("common.actions.approve") },
                { value: "reject", label: t("common.actions.reject") },
              ]}
            />
          </Form.Item>
          <Form.Item
            noStyle
            shouldUpdate={(prevValues, currentValues) => prevValues.action !== currentValues.action}
          >
            {({ getFieldValue }) => (
              <Form.Item
                label={t("admin_users.fields.reason")}
                name="reason"
                rules={
                  getFieldValue("action") === "reject"
                    ? [{ required: true, message: t("admin_users.validation.reason_required") }]
                    : []
                }
                extra={
                  getFieldValue("action") === "approve"
                    ? t("admin_users.registration.reason_optional_for_approve")
                    : undefined
                }
              >
                <Input.TextArea rows={4} placeholder={t("admin_users.registration.reject_reason_placeholder")} />
              </Form.Item>
            )}
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={t("admin_users.registration.setup_link_title")}
        open={viewingRegistrationLink !== null}
        onCancel={() => {
          setViewingRegistrationLink(null);
        }}
        footer={[
          <Button
            key="copy"
            type="primary"
            onClick={() => {
              if (!viewingRegistrationLink?.passwordSetupLink) {
                return;
              }

              const registrationUrl = `${window.location.origin}${viewingRegistrationLink.passwordSetupLink}`;
              void copyToClipboard(
                registrationUrl,
                t("admin_users.messages.registration_link_copied", { email: viewingRegistrationLink.email })
              );
            }}
          >
            {t("common.actions.copy_link")}
          </Button>,
          <Button
            key="close"
            onClick={() => {
              setViewingRegistrationLink(null);
            }}
          >
            {t("common.actions.close")}
          </Button>,
        ]}
      >
        <Space direction="vertical" size={12} style={{ width: "100%" }}>
          <Text strong>{viewingRegistrationLink?.email}</Text>
          <Text type="secondary">{t("admin_users.view.department")}: {viewingRegistrationLink?.departmentName || "-"}</Text>
          <Text type="secondary">{t("admin_users.view.job_title")}: {viewingRegistrationLink?.jobTitleName || "-"}</Text>
          <Text type="secondary">
            {t("admin_users.registration.password_setup_expires_at")}: {formatDate(viewingRegistrationLink?.passwordSetupExpiresAt ?? null, currentLanguage)}
          </Text>
          <Input.TextArea
            value={viewingRegistrationLink?.passwordSetupLink ? `${window.location.origin}${viewingRegistrationLink.passwordSetupLink}` : ""}
            autoSize={{ minRows: 3, maxRows: 5 }}
            readOnly
          />
        </Space>
      </Modal>

      <Modal
        title={t("admin_users.invitations.view_title")}
        open={viewingInvitation !== null}
        onCancel={() => {
          setViewingInvitation(null);
        }}
        footer={[
          <Button
            key="copy"
            type="primary"
            onClick={() => {
              if (!viewingInvitation) {
                return;
              }

              const invitationUrl = `${window.location.origin}${viewingInvitation.invitationLink}`;
              void copyToClipboard(
                invitationUrl,
                t("admin_users.messages.invitation_link_copied", { email: viewingInvitation.email })
              );
            }}
          >
            {t("common.actions.copy_link")}
          </Button>,
          <Button
            key="close"
            onClick={() => {
              setViewingInvitation(null);
            }}
          >
            {t("common.actions.close")}
          </Button>,
        ]}
      >
        <Space direction="vertical" size={12} style={{ width: "100%" }}>
          <Text strong>{viewingInvitation?.email}</Text>
          <Text type="secondary">{t("admin_users.view.department")}: {viewingInvitation?.departmentName || "-"}</Text>
          <Text type="secondary">{t("admin_users.view.job_title")}: {viewingInvitation?.jobTitleName || "-"}</Text>
          <Input.TextArea
            value={viewingInvitation ? `${window.location.origin}${viewingInvitation.invitationLink}` : ""}
            autoSize={{ minRows: 3, maxRows: 5 }}
            readOnly
          />
        </Space>
      </Modal>

      <Modal
        title={t("admin_users.invitations.create_modal_title")}
        open={creatingInvitation}
        okText={t("common.actions.send_invitation")}
        cancelText={t("common.actions.cancel")}
        confirmLoading={createInvitationMutation.isPending}
        onCancel={() => {
          setCreatingInvitation(false);
          inviteForm.resetFields();
        }}
        onOk={() => {
          inviteForm
            .validateFields()
            .then((values: { email: string; departmentId?: string; jobTitleId?: string; expiresAt?: { endOf: (unit: string) => { toISOString: () => string } } }) => {
              createInvitationMutation.mutate(
                {
                  email: values.email,
                  invitedBy: actor,
                  departmentId: values.departmentId,
                  jobTitleId: values.jobTitleId,
                  expiresAt: values.expiresAt ? values.expiresAt.endOf("day").toISOString() : undefined,
                },
                {
                  onSuccess: () => {
                    setCreatingInvitation(false);
                    inviteForm.resetFields();
                    handleSuccess(t("admin_users.messages.invitation_sent", { email: values.email }));
                  },
                  onError: (error) => handleError(t("errors.invite_user_failed"), error),
                }
              );
            })
            .catch(() => undefined);
        }}
      >
        <Form layout="vertical" form={inviteForm}>
          <Form.Item label={t("admin_users.fields.email")} name="email" rules={[{ required: true, type: "email" }]}>
            <Input prefix={<MailOutlined />} placeholder={t("admin_users.placeholders.invitee_email")} />
          </Form.Item>
          <Form.Item label={t("admin_users.fields.department")} name="departmentId">
            <Select
              allowClear
              placeholder={t("admin_users.placeholders.select_department")}
              options={departmentOptions}
            />
          </Form.Item>
          <Form.Item label={t("admin_users.fields.job_title")} name="jobTitleId">
            <Select
              allowClear
              placeholder={t("admin_users.placeholders.select_job_title")}
              options={jobTitleOptions}
            />
          </Form.Item>
          <Form.Item label={t("admin_users.fields.expires_at")} name="expiresAt">
            <DatePicker
              style={{ width: "100%" }}
              format="DD/MM/YYYY"
              placeholder={t("admin_users.placeholders.select_expiration_date")}
              disabledDate={(current) => Boolean(current && current.endOf("day").valueOf() <= Date.now())}
            />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={t("admin_users.invitations.edit_modal_title")}
        open={editingInvitation !== null}
        okText={t("common.actions.save")}
        cancelText={t("common.actions.cancel")}
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
            .then((values: { email: string; departmentId?: string; jobTitleId?: string; expiresAt?: { endOf: (unit: string) => { toISOString: () => string } } }) => {
              updateInvitationMutation.mutate(
                {
                  id: editingInvitation.id,
                  email: values.email,
                  departmentId: values.departmentId,
                  jobTitleId: values.jobTitleId,
                  expiresAt: values.expiresAt ? values.expiresAt.endOf("day").toISOString() : undefined,
                },
                {
                  onSuccess: () => {
                    setEditingInvitation(null);
                    editInvitationForm.resetFields();
                    handleSuccess(t("admin_users.messages.invitation_updated", { email: values.email }));
                  },
                  onError: (error) => handleError(t("errors.update_invitation_failed"), error),
                }
              );
            })
            .catch(() => undefined);
        }}
      >
        <Form layout="vertical" form={editInvitationForm}>
          <Form.Item label={t("admin_users.fields.email")} name="email" rules={[{ required: true, type: "email" }]}>
            <Input prefix={<MailOutlined />} placeholder={t("admin_users.placeholders.invitee_email")} />
          </Form.Item>
          <Form.Item label={t("admin_users.fields.department")} name="departmentId">
            <Select
              allowClear
              placeholder={t("admin_users.placeholders.select_department")}
              options={departmentOptions}
            />
          </Form.Item>
          <Form.Item label={t("admin_users.fields.job_title")} name="jobTitleId">
            <Select
              allowClear
              placeholder={t("admin_users.placeholders.select_job_title")}
              options={jobTitleOptions}
            />
          </Form.Item>
          <Form.Item label={t("admin_users.fields.expires_at")} name="expiresAt">
            <DatePicker
              style={{ width: "100%" }}
              format="DD/MM/YYYY"
              placeholder={t("admin_users.placeholders.select_expiration_date")}
              disabledDate={(current) => Boolean(current && current.endOf("day").valueOf() <= Date.now())}
            />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={t("admin_users.directory.create_modal_title")}
        open={creatingUser}
        width={820}
        destroyOnHidden
        okText={t("common.actions.save")}
        cancelText={t("common.actions.cancel")}
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
                    handleSuccess(t("admin_users.messages.user_created", { email: values.email }));
                  },
                  onError: (error) => handleError(t("errors.create_user_failed"), error),
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
                {t("admin_users.sections.account_information")}
              </Typography.Title>
              <Form.Item label={t("admin_users.fields.email")} name="email" rules={[{ required: true, type: "email" }]}>
                <Input placeholder={t("admin_users.placeholders.user_email")} />
              </Form.Item>
              <Form.Item label={t("admin_users.fields.first_name")} name="firstName" rules={[{ required: true, message: t("invitation_page.first_name_required") }]}>
                <Input placeholder={t("invitation_page.first_name_placeholder")} />
              </Form.Item>
              <Form.Item label={t("admin_users.fields.last_name")} name="lastName" rules={[{ required: true, message: t("invitation_page.last_name_required") }]}>
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
            </Card>

            <Card size="small" variant="borderless" style={{ background: "rgba(15, 23, 42, 0.03)" }}>
              <Typography.Title level={5} style={{ marginTop: 0 }}>
                {t("admin_users.sections.organization_structure")}
              </Typography.Title>
              <Form.Item label={t("admin_users.fields.department")} name="departmentId">
                <Select
                  allowClear
                  placeholder={t("admin_users.placeholders.select_department")}
                  loading={departmentsQuery.isLoading}
                  options={departmentOptions}
                />
              </Form.Item>
              <Form.Item label={t("admin_users.fields.job_title")} name="jobTitleId">
                <Select
                  allowClear
                  placeholder={t("admin_users.placeholders.select_job_title")}
                  loading={jobTitlesQuery.isLoading}
                  options={jobTitleOptions}
                />
              </Form.Item>
            </Card>

            <Card size="small" variant="borderless" style={{ background: "rgba(22, 163, 74, 0.05)" }}>
              <Typography.Title level={5} style={{ marginTop: 0 }}>
                {t("admin_users.sections.access_rights")}
              </Typography.Title>
              <Form.Item
                label={t("admin_users.fields.roles")}
                name="roles"
                extra={t("admin_users.fields.roles_help")}
              >
                <Select
                  mode="multiple"
                  allowClear
                  placeholder={t("admin_users.placeholders.select_roles")}
                  loading={rolesQuery.isLoading}
                  options={userRoleOptions}
                />
              </Form.Item>
            </Card>
          </Space>
        </Form>
      </Modal>

      <Modal
        title={t("admin_users.directory.edit_modal_title")}
        open={editingUser !== null}
        width={820}
        destroyOnHidden
        okText={t("common.actions.save")}
        cancelText={t("common.actions.cancel")}
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
                    handleSuccess(t("admin_users.messages.user_updated", { email: values.email }));
                    setEditingUser(null);
                    editUserForm.resetFields();
                  },
                  onError: (error) => handleError(t("errors.update_user_failed"), error),
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
                {t("admin_users.sections.account_information")}
              </Typography.Title>
              <Form.Item label={t("admin_users.fields.email")} name="email" rules={[{ required: true, type: "email" }]}>
                <Input placeholder={t("admin_users.placeholders.user_email")} />
              </Form.Item>
              <Form.Item label={t("admin_users.fields.first_name")} name="firstName" rules={[{ required: true, message: t("invitation_page.first_name_required") }]}>
                <Input placeholder={t("invitation_page.first_name_placeholder")} />
              </Form.Item>
              <Form.Item label={t("admin_users.fields.last_name")} name="lastName" rules={[{ required: true, message: t("invitation_page.last_name_required") }]}>
                <Input placeholder={t("invitation_page.last_name_placeholder")} />
              </Form.Item>
            </Card>

            <Card size="small" variant="borderless" style={{ background: "rgba(15, 23, 42, 0.03)" }}>
              <Typography.Title level={5} style={{ marginTop: 0 }}>
                {t("admin_users.sections.organization_structure")}
              </Typography.Title>
              <Form.Item label={t("admin_users.fields.department")} name="departmentId">
                <Select
                  allowClear
                  placeholder={t("admin_users.placeholders.select_department")}
                  loading={departmentsQuery.isLoading}
                  options={departmentOptions}
                />
              </Form.Item>
              <Form.Item label={t("admin_users.fields.job_title")} name="jobTitleId">
                <Select
                  allowClear
                  placeholder={t("admin_users.placeholders.select_job_title")}
                  loading={jobTitlesQuery.isLoading}
                  options={jobTitleOptions}
                />
              </Form.Item>
            </Card>

            <Card size="small" variant="borderless" style={{ background: "rgba(22, 163, 74, 0.05)" }}>
              <Typography.Title level={5} style={{ marginTop: 0 }}>
                {t("admin_users.sections.access_rights")}
              </Typography.Title>
              <Form.Item
                label={t("admin_users.fields.roles")}
                name="roleIds"
                extra={t("admin_users.fields.roles_help")}
              >
                <Select
                  mode="multiple"
                  allowClear
                  placeholder={t("admin_users.placeholders.select_roles")}
                  loading={rolesQuery.isLoading}
                  options={userRoleOptions}
                />
              </Form.Item>
            </Card>
          </Space>
        </Form>
      </Modal>

      <Modal
        title={deletingUser ? t("admin_users.directory.delete_modal_title_with_email", { email: deletingUser.keycloak?.email || deletingUser.id }) : t("admin_users.directory.delete_modal_title")}
        open={deletingUser !== null}
        okText={t("common.actions.confirm_delete")}
        cancelText={t("common.actions.cancel")}
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
                    handleSuccess(t("admin_users.messages.user_deleted", { email: deletingUser.keycloak?.email || deletingUser.id }));
                    setDeletingUser(null);
                    deleteUserForm.resetFields();
                  },
                  onError: (error) => handleError(t("errors.delete_user_failed"), error),
                }
              );
            })
            .catch(() => undefined);
        }}
      >
        <Form layout="vertical" form={deleteUserForm}>
          <Paragraph type="secondary">
            {t("admin_users.directory.delete_description")}
          </Paragraph>
          <Form.Item name="reason" label={t("admin_users.fields.delete_reason")} rules={[{ required: true, message: t("admin_users.validation.delete_reason_required") }]}>
            <Input.TextArea rows={4} placeholder={t("admin_users.placeholders.user_delete_reason")} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={t("admin_users.master.create_department_modal_title")}
        open={creatingDepartment}
        okText={t("common.actions.create")}
        cancelText={t("common.actions.cancel")}
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
                    handleSuccess(t("admin_users.messages.department_created", { name: values.name }));
                  },
                  onError: (error) => handleError(t("errors.create_department_failed"), error),
                }
              );
            })
            .catch(() => undefined);
        }}
      >
        <Form form={createDepartmentForm} layout="vertical">
          <Form.Item name="name" label={t("admin_users.master.department_name")} rules={[{ required: true, message: t("errors.department_required") }]}>
            <Input placeholder={t("admin_users.placeholders.department_name")} />
          </Form.Item>
          <Form.Item name="displayOrder" label={t("admin_users.master.display_order")} rules={[{ required: true }]}>
            <InputNumber min={0} style={{ width: "100%" }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={t("admin_users.master.edit_department_modal_title")}
        open={editingDepartment !== null}
        okText={t("common.actions.save")}
        cancelText={t("common.actions.cancel")}
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
                    handleSuccess(t("admin_users.messages.department_updated", { name: values.editName }));
                  },
                  onError: (error) => handleError(t("errors.update_department_failed"), error),
                }
              );
            })
            .catch(() => undefined);
        }}
      >
        <Form form={editDepartmentForm} layout="vertical">
          <Form.Item name="editName" label={t("admin_users.master.department_name")} rules={[{ required: true, message: t("errors.department_required") }]}>
            <Input />
          </Form.Item>
          <Form.Item name="editDisplayOrder" label={t("admin_users.master.display_order")} rules={[{ required: true }]}>
            <InputNumber min={0} style={{ width: "100%" }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={deletingDepartment ? t("admin_users.master.delete_department_title_with_name", { name: deletingDepartment.name }) : t("admin_users.master.delete_department_title")}
        open={deletingDepartment !== null}
        okText={t("common.actions.delete")}
        cancelText={t("common.actions.cancel")}
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
                    handleSuccess(t("admin_users.messages.department_deleted", { name: deletingDepartment.name }));
                  },
                  onError: (error) => handleError(t("errors.delete_department_failed"), error),
                }
              );
            })
            .catch(() => undefined);
        }}
      >
        <Form form={deleteDepartmentForm} layout="vertical">
          <Paragraph type="secondary">{t("admin_users.master.delete_soft_delete_description")}</Paragraph>
          <Form.Item name="reason" label={t("admin_users.fields.reason")} rules={[{ required: true, message: t("admin_users.validation.reason_required") }]}>
            <Input.TextArea rows={4} placeholder={t("admin_users.placeholders.department_delete_reason")} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={t("admin_users.master.create_job_title_modal_title")}
        open={creatingJobTitle}
        okText={t("common.actions.create")}
        cancelText={t("common.actions.cancel")}
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
                    handleSuccess(t("admin_users.messages.job_title_created", { name: values.name }));
                  },
                  onError: (error) => handleError(t("errors.create_job_title_failed"), error),
                }
              );
            })
            .catch(() => undefined);
        }}
      >
        <Form form={createJobTitleForm} layout="vertical">
          <Form.Item name="name" label={t("admin_users.master.job_title_name")} rules={[{ required: true, message: t("errors.job_title_required") }]}>
            <Input placeholder={t("admin_users.placeholders.job_title_name")} />
          </Form.Item>
          <Form.Item name="displayOrder" label={t("admin_users.master.display_order")} rules={[{ required: true }]}>
            <InputNumber min={0} style={{ width: "100%" }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={t("admin_users.master.edit_job_title_modal_title")}
        open={editingJobTitle !== null}
        okText={t("common.actions.save")}
        cancelText={t("common.actions.cancel")}
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
                    handleSuccess(t("admin_users.messages.job_title_updated", { name: values.editName }));
                  },
                  onError: (error) => handleError(t("errors.update_job_title_failed"), error),
                }
              );
            })
            .catch(() => undefined);
        }}
      >
        <Form form={editJobTitleForm} layout="vertical">
          <Form.Item name="editName" label={t("admin_users.master.job_title_name")} rules={[{ required: true, message: t("errors.job_title_required") }]}>
            <Input />
          </Form.Item>
          <Form.Item name="editDisplayOrder" label={t("admin_users.master.display_order")} rules={[{ required: true }]}>
            <InputNumber min={0} style={{ width: "100%" }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={deletingJobTitle ? t("admin_users.master.delete_job_title_title_with_name", { name: deletingJobTitle.name }) : t("admin_users.master.delete_job_title_title")}
        open={deletingJobTitle !== null}
        okText={t("common.actions.delete")}
        cancelText={t("common.actions.cancel")}
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
                    handleSuccess(t("admin_users.messages.job_title_deleted", { name: deletingJobTitle.name }));
                  },
                  onError: (error) => handleError(t("errors.delete_job_title_failed"), error),
                }
              );
            })
            .catch(() => undefined);
        }}
      >
        <Form form={deleteJobTitleForm} layout="vertical">
          <Paragraph type="secondary">{t("admin_users.master.delete_soft_delete_description")}</Paragraph>
          <Form.Item name="reason" label={t("admin_users.fields.reason")} rules={[{ required: true, message: t("admin_users.validation.reason_required") }]}>
            <Input.TextArea rows={4} placeholder={t("admin_users.placeholders.job_title_delete_reason")} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
