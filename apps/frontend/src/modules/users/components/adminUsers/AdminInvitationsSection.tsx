import { Button, Card, DatePicker, Input, Select, Space, Table, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SorterResult } from "antd/es/table/interface";
import { EditOutlined, EyeOutlined, MailOutlined } from "@ant-design/icons";
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
  return (
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
            options={invitationStatusOptions}
            value={paging.status}
            onChange={(value) => setPaging((current) => ({ ...current, page: 1, status: value }))}
          />
          <DatePicker.RangePicker
            value={[
              paging.from ? dayjs(paging.from) : null,
              paging.to ? dayjs(paging.to) : null,
            ]}
            onChange={(range) => {
              const normalized = toRange(range as [Dayjs | null, Dayjs | null] | undefined);
              setPaging((current) => ({ ...current, page: 1, ...normalized }));
            }}
          />
          <Button type="primary" icon={<MailOutlined />} size="large" onClick={() => setCreatingInvitation(true)}>
            {t("admin_users.invitations.open_create")}
          </Button>
        </Space>
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
                    </Space>
                  ),
                }
          )}
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
      </Card>
    </Space>
  );
}
