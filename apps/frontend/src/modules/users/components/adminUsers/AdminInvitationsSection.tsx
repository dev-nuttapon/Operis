import { useEffect, useState } from "react";
import { Button, Card, DatePicker, Input, Select, Space, Table, Typography, Skeleton, Flex, Grid } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SorterResult } from "antd/es/table/interface";
import { EditOutlined, EyeOutlined, MailOutlined } from "@ant-design/icons";
import { permissions } from "../../../../shared/authz/permissions";
import { usePermissions } from "../../../../shared/authz/usePermissions";
import { useDebouncedValue } from "../../../../shared/hooks/useDebouncedValue";
import dayjs from "dayjs";
import type { Dayjs } from "dayjs";
import { invitationStatusOptions, toApiSortOrder, toRange } from "../../utils/adminUsersPresentation";
import type { Invitation, InvitationStatus } from "../../types/users";

interface AdminInvitationsSectionProps {
  cancelInvitationLoading: boolean;
  columns: ColumnsType<Invitation>;
  data: Invitation[];
  loading: boolean;
  paging: {
    page: number;
    pageSize: number;
    search: string;
    status: InvitationStatus | undefined;
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
  setCreatingInvitation: (open: boolean) => void;
  setEditingInvitation: (invitation: Invitation | null) => void;
  setPaging: (
    updater: (current: AdminInvitationsSectionProps["paging"]) => AdminInvitationsSectionProps["paging"]
  ) => void;
  setViewingInvitation: (invitation: Invitation | null) => void;
  t: (key: string, options?: Record<string, unknown>) => string;
  onCancelInvitation: (record: Invitation) => void;
  onPrefillInvitationEdit: (record: Invitation) => void;
}

export function AdminInvitationsSection({
  cancelInvitationLoading,
  columns,
  data,
  loading,
  paging,
  pagination,
  setCreatingInvitation,
  setEditingInvitation,
  setPaging,
  setViewingInvitation,
  t,
  onCancelInvitation,
  onPrefillInvitationEdit,
}: AdminInvitationsSectionProps) {
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const permissionState = usePermissions();
  const canInviteUsers = permissionState.hasPermission(permissions.users.invite);
  const [searchInput, setSearchInput] = useState(paging.search);
  const debouncedSearch = useDebouncedValue(searchInput, 300);

  useEffect(() => {
    setPaging((current) => ({ ...current, page: 1, search: debouncedSearch }));
  }, [debouncedSearch, setPaging]);

  useEffect(() => {
    setSearchInput(paging.search);
  }, [paging.search]);
  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Typography.Title level={5}>{t("admin_users.invitations.latest_title")}</Typography.Title>
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
            placeholder={t("admin_users.placeholders.search_invitations")}
            value={searchInput}
            onChange={(event) => setSearchInput(event.target.value)}
            onSearch={(value) => setSearchInput(value)}
          />
          <Select
            allowClear
            style={{ width: isMobile ? "100%" : 180 }}
            placeholder={t("admin_users.placeholders.select_status")}
            options={invitationStatusOptions}
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
          {canInviteUsers ? (
            <Button type="primary" icon={<MailOutlined />} size="large" onClick={() => setCreatingInvitation(true)} block={isMobile}>
              {t("admin_users.invitations.open_create")}
            </Button>
          ) : null}
        </Flex>
        {loading && data.length === 0 ? (
          <Skeleton active paragraph={{ rows: 6 }} />
        ) : (
          <Table
            rowKey="id"
            columns={columns.map((column) =>
              column.key !== "actions"
                ? column
                : {
                    ...column,
                    render: (_: unknown, record: Invitation) => (
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
                        {canInviteUsers ? (
                          <>
                            <Button
                              icon={<EditOutlined />}
                              disabled={record.status === "Accepted" || record.status === "Cancelled" || record.status === "Rejected"}
                              onClick={() => {
                                setEditingInvitation(record);
                                onPrefillInvitationEdit(record);
                              }}
                            >
                              {t("common.actions.edit")}
                            </Button>
                            <Button
                              danger
                              disabled={record.status === "Accepted"}
                              loading={cancelInvitationLoading}
                              onClick={() => {
                                onCancelInvitation(record);
                              }}
                            >
                              {t("admin_users.invitations.cancel_action")}
                            </Button>
                          </>
                        ) : null}
                      </Space>
                    ),
                  }
            )}
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
              const sort = sorter as SorterResult<Invitation>;
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
