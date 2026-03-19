import { useEffect, useState } from "react";
import { Button, Card, DatePicker, Input, Select, Space, Table, Tag, Typography, Skeleton, Flex, Grid } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SorterResult } from "antd/es/table/interface";
import { DeleteOutlined, EditOutlined, UserAddOutlined } from "@ant-design/icons";
import { permissions } from "../../../../shared/authz/permissions";
import { usePermissions } from "../../../../shared/authz/usePermissions";
import { useDebouncedValue } from "../../../../shared/hooks/useDebouncedValue";
import dayjs from "dayjs";
import type { Dayjs } from "dayjs";
import {
  formatDate,
  toApiSortOrder,
  toRange,
  userStatusOptions,
} from "../../utils/adminUsersPresentation";
import type { User, UserStatus } from "../../types/users";

interface AdminUsersDirectorySectionProps {
  currentLanguage: string;
  data: User[];
  deleteUserLoading: boolean;
  editUserForm: {
    setFieldsValue: (values: Record<string, unknown>) => void;
  };
  loading: boolean;
  paging: {
    page: number;
    pageSize: number;
    search: string;
    status: UserStatus | undefined;
    from: string | undefined;
    to: string | undefined;
    sortBy: string;
    sortOrder: "asc" | "desc";
  };
  pagination?: {
    page?: number;
    pageSize?: number;
    total?: number;
  };
  roleItems: Array<{ id: string; name: string }>;
  setCreatingUser: (open: boolean) => void;
  setDeletingUser: (user: User | null) => void;
  setPaging: (
    updater: (current: AdminUsersDirectorySectionProps["paging"]) => AdminUsersDirectorySectionProps["paging"]
  ) => void;
  setEditingUser: (user: User | null) => void;
  t: (key: string, options?: Record<string, unknown>) => string;
  deleteUserForm: {
    resetFields: () => void;
  };
}

export function AdminUsersDirectorySection({
  currentLanguage,
  data,
  deleteUserForm,
  deleteUserLoading,
  editUserForm,
  loading,
  paging,
  pagination,
  roleItems,
  setCreatingUser,
  setDeletingUser,
  setEditingUser,
  setPaging,
  t,
}: AdminUsersDirectorySectionProps) {
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const permissionState = usePermissions();
  const canCreateUsers = permissionState.hasPermission(permissions.users.create);
  const canUpdateUsers = permissionState.hasPermission(permissions.users.update);
  const canDeleteUsers = permissionState.hasPermission(permissions.users.delete);
  const [searchInput, setSearchInput] = useState(paging.search);
  const debouncedSearch = useDebouncedValue(searchInput, 300);

  useEffect(() => {
    setPaging((current) => ({ ...current, page: 1, search: debouncedSearch }));
  }, [debouncedSearch, setPaging]);

  useEffect(() => {
    setSearchInput(paging.search);
  }, [paging.search]);
  const columns: ColumnsType<User> = [
    {
      title: t("admin_users.columns.name"),
      key: "name",
      render: (_, record) => (
        <Space direction="vertical" size={0}>
          <Typography.Text strong>{`${record.keycloak?.firstName || "-"} ${record.keycloak?.lastName || ""}`.trim()}</Typography.Text>
          <Typography.Text type="secondary">{record.keycloak?.email || record.keycloak?.username || record.id || "-"}</Typography.Text>
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
      title: t("admin_users.columns.division"),
      dataIndex: "divisionName",
      render: (value: string | null) => value || "-",
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
          {roles.length === 0 ? <Typography.Text type="secondary">-</Typography.Text> : roles.map((role) => <Tag key={role}>{role}</Tag>)}
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
          {canUpdateUsers ? (
            <Button
              icon={<EditOutlined />}
              onClick={() => {
                const matchedRoleIds = roleItems
                  .filter((item) => record.roles.includes(item.name))
                  .map((item) => item.id);

                setEditingUser(record);
                editUserForm.setFieldsValue({
                  email: record.keycloak?.email ?? "",
                  firstName: record.keycloak?.firstName ?? "",
                  lastName: record.keycloak?.lastName ?? "",
                  divisionId: record.divisionId ?? undefined,
                  departmentId: record.departmentId ?? undefined,
                  jobTitleId: record.jobTitleId ?? undefined,
                  roleIds: matchedRoleIds,
                });
              }}
            >
              {t("common.actions.edit")}
            </Button>
          ) : null}
          {canDeleteUsers ? (
            <Button
              danger
              icon={<DeleteOutlined />}
              loading={deleteUserLoading}
              onClick={() => {
                setDeletingUser(record);
                deleteUserForm.resetFields();
              }}
            >
              {t("common.actions.delete")}
            </Button>
          ) : null}
        </Space>
      ),
    },
  ];

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Typography.Title level={5}>{t("admin_users.directory.list_title")}</Typography.Title>
        <Flex
          gap={12}
          wrap={!isMobile}
          vertical={isMobile}
          align={isMobile ? "stretch" : "center"}
          justify="space-between"
          style={{ width: "100%", marginBottom: 16 }}
        >
          <Input.Search
            allowClear
            style={{ width: isMobile ? "100%" : 320 }}
            placeholder={t("admin_users.placeholders.search_users")}
            value={searchInput}
            onChange={(event) => setSearchInput(event.target.value)}
            onSearch={(value) => setSearchInput(value)}
          />
          <Select
            allowClear
            style={{ width: isMobile ? "100%" : 180 }}
            placeholder={t("admin_users.placeholders.select_status")}
            options={userStatusOptions}
            value={paging.status}
            onChange={(value) => setPaging((current) => ({ ...current, page: 1, status: value }))}
          />
          <DatePicker.RangePicker
            style={{ width: isMobile ? "100%" : undefined }}
            value={[
              paging.from ? dayjs(paging.from) : null,
              paging.to ? dayjs(paging.to) : null,
            ]}
            onChange={(range) => {
              const normalized = toRange(range as [Dayjs | null, Dayjs | null] | undefined);
              setPaging((current) => ({ ...current, page: 1, ...normalized }));
            }}
          />
          {canCreateUsers ? (
            <Button type="primary" icon={<UserAddOutlined />} size="large" onClick={() => setCreatingUser(true)} block={isMobile}>
              {t("admin_users.directory.create_user")}
            </Button>
          ) : null}
        </Flex>
        {loading && data.length === 0 ? (
          <Skeleton active paragraph={{ rows: 6 }} />
        ) : (
          <Table
            rowKey="id"
            columns={columns}
            dataSource={data}
            loading={loading}
            scroll={{ x: "max-content" }}
            pagination={{
              current: pagination?.page ?? paging.page,
              pageSize: pagination?.pageSize ?? paging.pageSize,
              total: pagination?.total ?? 0,
              showSizeChanger: true,
              pageSizeOptions: [10, 25, 50, 100],
            }}
            onChange={(nextPagination, _, sorter) => {
              const sort = sorter as SorterResult<User>;
              setPaging((current) => ({
                ...current,
                page: nextPagination.current ?? current.page,
                pageSize: nextPagination.pageSize ?? current.pageSize,
                sortBy: typeof sort.field === "string" ? sort.field : current.sortBy,
                sortOrder: toApiSortOrder(sort.order) ?? current.sortOrder,
              }));
            }}
          />
        )}
      </Card>
    </Space>
  );
}
