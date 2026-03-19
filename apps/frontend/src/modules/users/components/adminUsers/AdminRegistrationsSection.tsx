import { useEffect, useState } from "react";
import { Button, Card, Divider, Input, Space, Table, Tag, Typography, Skeleton, Flex, Grid } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SorterResult } from "antd/es/table/interface";
import { CheckCircleOutlined, EyeOutlined } from "@ant-design/icons";
import { permissions } from "../../../../shared/authz/permissions";
import { usePermissions } from "../../../../shared/authz/usePermissions";
import { useDebouncedValue } from "../../../../shared/hooks/useDebouncedValue";
import { ActionMenu } from "../../../../shared/components/ActionMenu";
import {
  formatDate,
  toApiSortOrder,
} from "../../utils/adminUsersPresentation";
import type { RegistrationRequest, RegistrationRequestStatus } from "../../types/users";

interface AdminRegistrationsSectionProps {
  currentLanguage: string;
  data: RegistrationRequest[];
  loading: boolean;
  paging: {
    page: number;
    pageSize: number;
    search: string;
    status: RegistrationRequestStatus | undefined;
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
  setManagingRegistration: (request: RegistrationRequest | null) => void;
  setPaging: (
    updater: (current: AdminRegistrationsSectionProps["paging"]) => AdminRegistrationsSectionProps["paging"]
  ) => void;
  setViewingRegistrationLink: (request: RegistrationRequest | null) => void;
  t: (key: string, options?: Record<string, unknown>) => string;
  reviewRegistrationForm: {
    setFieldsValue: (values: { action: "approve"; reason: string }) => void;
  };
}

export function AdminRegistrationsSection({
  currentLanguage,
  data,
  loading,
  paging,
  pagination,
  reviewRegistrationForm,
  setManagingRegistration,
  setPaging,
  setViewingRegistrationLink,
  t,
}: AdminRegistrationsSectionProps) {
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const permissionState = usePermissions();
  const canReviewRegistrations = permissionState.hasPermission(permissions.users.reviewRegistrations);
  const [searchInput, setSearchInput] = useState(paging.search);
  const debouncedSearch = useDebouncedValue(searchInput, 300);

  useEffect(() => {
    setPaging((current) => ({ ...current, page: 1, search: debouncedSearch }));
  }, [debouncedSearch, setPaging]);

  useEffect(() => {
    setSearchInput(paging.search);
  }, [paging.search]);
  const columns: ColumnsType<RegistrationRequest> = [
    {
      title: t("admin_users.columns.applicant"),
      key: "applicant",
      render: (_, record) => (
        <Space direction="vertical" size={0}>
          <Typography.Text strong>{`${record.firstName} ${record.lastName}`}</Typography.Text>
          <Typography.Text type="secondary">{record.email}</Typography.Text>
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
      render: (_, record) => {
        if (record.status === "Pending" && canReviewRegistrations) {
          return (
            <ActionMenu
              items={[
                {
                  key: "manage",
                  icon: <CheckCircleOutlined />,
                  label: t("common.actions.manage"),
                  onClick: () => {
                    setManagingRegistration(record);
                    reviewRegistrationForm.setFieldsValue({
                      action: "approve",
                      reason: "",
                    });
                  },
                },
              ]}
            />
          );
        }

        if (record.status === "Approved" && record.passwordSetupLink && !record.passwordSetupCompletedAt) {
          return (
            <ActionMenu
              items={[
                {
                  key: "view",
                  icon: <EyeOutlined />,
                  label: t("admin_users.registration.view_setup_link"),
                  onClick: () => setViewingRegistrationLink(record),
                },
              ]}
            />
          );
        }

        return <Typography.Text type="secondary">-</Typography.Text>;
      },
    },
  ];

  return (
    <Card variant="borderless">
      <Typography.Title level={5}>{t("admin_users.registration.pending_title")}</Typography.Title>
      <Divider />
      <Flex
        gap={12}
        wrap={!isMobile}
        vertical={isMobile}
        align={isMobile ? "stretch" : "center"}
        style={{ marginBottom: 16 }}
      >
        <Input.Search
          allowClear
          style={{ width: isMobile ? "100%" : 320 }}
          placeholder={t("admin_users.placeholders.search_registrations")}
          value={searchInput}
          onChange={(event) => setSearchInput(event.target.value)}
          onSearch={(value) => setSearchInput(value)}
        />
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
            const sort = sorter as SorterResult<RegistrationRequest>;
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
  );
}
