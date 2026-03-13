import { Button, Card, DatePicker, Input, Select, Space, Table, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SorterResult } from "antd/es/table/interface";
import { ApartmentOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import type { Dayjs } from "dayjs";
import { formatDate, toApiSortOrder, toRange, userStatusOptions } from "../../utils/adminUsersPresentation";
import type { User, UserStatus } from "../../types/users";

interface AdminUserAffiliationsSectionProps {
  currentLanguage: string;
  data: User[];
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
  setEditingUser: (user: User | null) => void;
  setPaging: (
    updater: (current: AdminUserAffiliationsSectionProps["paging"]) => AdminUserAffiliationsSectionProps["paging"]
  ) => void;
  t: (key: string, options?: Record<string, unknown>) => string;
}

export function AdminUserAffiliationsSection({
  currentLanguage,
  data,
  loading,
  paging,
  pagination,
  setEditingUser,
  setPaging,
  t,
}: AdminUserAffiliationsSectionProps) {
  const columns: ColumnsType<User> = [
    {
      title: t("admin_users.columns.name"),
      key: "name",
      render: (_, record) => (
        <Space direction="vertical" size={0}>
          <Typography.Text strong>{`${record.keycloak?.firstName || "-"} ${record.keycloak?.lastName || ""}`.trim()}</Typography.Text>
          <Typography.Text type="secondary">{record.keycloak?.email || record.id}</Typography.Text>
        </Space>
      ),
    },
    {
      title: t("admin_users.columns.status"),
      dataIndex: "status",
      sorter: true,
      render: (status: User["status"]) => <Tag color={status === "Active" ? "green" : status === "Deleted" ? "red" : "default"}>{status}</Tag>,
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
      title: t("admin_users.columns.position"),
      dataIndex: "jobTitleName",
      render: (value: string | null) => value || "-",
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
        <Button icon={<ApartmentOutlined />} onClick={() => setEditingUser(record)}>
          {t("common.affiliation_manage")}
        </Button>
      ),
    },
  ];

  return (
    <Card variant="borderless">
      <Typography.Title level={5}>{t("admin_users.affiliations.list_title")}</Typography.Title>
      <Space wrap size={[12, 12]} style={{ width: "100%", marginBottom: 16, justifyContent: "space-between" }}>
        <Input.Search
          allowClear
          style={{ width: 320 }}
          placeholder={t("admin_users.placeholders.search_users")}
          onSearch={(value) => setPaging((current) => ({ ...current, page: 1, search: value }))}
          onChange={(event) => {
            if (event.target.value === "") {
              setPaging((current) => ({ ...current, page: 1, search: "" }));
            }
          }}
        />
        <Select
          allowClear
          style={{ width: 180 }}
          placeholder={t("admin_users.placeholders.select_status")}
          options={userStatusOptions}
          value={paging.status}
          onChange={(value) => setPaging((current) => ({ ...current, page: 1, status: value }))}
        />
        <DatePicker.RangePicker
          value={[paging.from ? dayjs(paging.from) : null, paging.to ? dayjs(paging.to) : null]}
          onChange={(range) => {
            const normalized = toRange(range as [Dayjs | null, Dayjs | null] | undefined);
            setPaging((current) => ({ ...current, page: 1, ...normalized }));
          }}
        />
      </Space>
      <Table
        rowKey="id"
        columns={columns}
        dataSource={data}
        loading={loading}
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
    </Card>
  );
}
