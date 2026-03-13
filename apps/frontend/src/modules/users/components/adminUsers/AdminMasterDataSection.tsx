import { Button, Card, Input, Space, Table, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SorterResult } from "antd/es/table/interface";
import { DeleteOutlined, EditOutlined, PlusOutlined } from "@ant-design/icons";
import { toApiSortOrder } from "../../utils/adminUsersPresentation";
import type { MasterDataItem } from "../../types/users";

interface PagingState {
  page: number;
  pageSize: number;
  search: string;
  sortBy: string;
  sortOrder: "asc" | "desc";
}

interface AdminMasterDataSectionProps {
  createLabel: string;
  data: MasterDataItem[];
  deleting: boolean;
  description: string;
  extraColumns?: ColumnsType<MasterDataItem>;
  itemLabel: string;
  loading: boolean;
  paging: PagingState;
  pagination?: {
    page?: number;
    pageSize?: number;
    total?: number;
  };
  searchPlaceholder: string;
  setCreating: (open: boolean) => void;
  setDeleting: (item: MasterDataItem | null) => void;
  setEditing: (item: MasterDataItem | null) => void;
  setPaging: (updater: (current: PagingState) => PagingState) => void;
  t: (key: string, options?: Record<string, unknown>) => string;
  title: string;
  onEdit: (item: MasterDataItem) => void;
  onDeletePrepare: () => void;
}

export function AdminMasterDataSection({
  createLabel,
  data,
  deleting,
  description,
  extraColumns = [],
  itemLabel,
  loading,
  paging,
  pagination,
  searchPlaceholder,
  setCreating,
  setDeleting,
  setEditing,
  setPaging,
  t,
  title,
  onDeletePrepare,
  onEdit,
}: AdminMasterDataSectionProps) {
  const columns: ColumnsType<MasterDataItem> = [
    {
      title: itemLabel,
      dataIndex: "name",
      sorter: true,
    },
    ...extraColumns,
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
          <Button
            icon={<EditOutlined />}
            onClick={() => {
              setEditing(record);
              onEdit(record);
            }}
          >
            {t("common.actions.edit")}
          </Button>
          <Button
            danger
            icon={<DeleteOutlined />}
            loading={deleting}
            onClick={() => {
              setDeleting(record);
              onDeletePrepare();
            }}
          >
            {t("common.actions.delete")}
          </Button>
        </Space>
      ),
    },
  ];

  return (
    <Card variant="borderless">
      <Typography.Title level={5}>{title}</Typography.Title>
      <Typography.Paragraph type="secondary">{description}</Typography.Paragraph>
      <Space
        wrap
        style={{ width: "100%", marginBottom: 16, justifyContent: "space-between" }}
        size={[12, 12]}
      >
        <Input.Search
          allowClear
          style={{ width: 360, maxWidth: "100%" }}
          placeholder={searchPlaceholder}
          onSearch={(value) => setPaging((current) => ({ ...current, page: 1, search: value }))}
        />
        <Button type="primary" icon={<PlusOutlined />} size="large" onClick={() => setCreating(true)}>
          {createLabel}
        </Button>
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
          const sort = sorter as SorterResult<MasterDataItem>;
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
