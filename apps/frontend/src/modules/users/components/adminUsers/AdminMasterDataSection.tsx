import { useEffect, useState } from "react";
import { Button, Card, Input, Space, Table, Typography, Skeleton, Flex, Grid } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SorterResult } from "antd/es/table/interface";
import { DeleteOutlined, EditOutlined, PlusOutlined } from "@ant-design/icons";
import { permissions } from "../../../../shared/authz/permissions";
import { usePermissions } from "../../../../shared/authz/usePermissions";
import { useDebouncedValue } from "../../../../shared/hooks/useDebouncedValue";
import { ActionMenu } from "../../../../shared/components/ActionMenu";
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
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const permissionState = usePermissions();
  const canManagePermanentOrg = permissionState.hasPermission(permissions.masterData.managePermanentOrg);
  const [searchInput, setSearchInput] = useState(paging.search);
  const debouncedSearch = useDebouncedValue(searchInput, 300);

  useEffect(() => {
    setPaging((current) => ({ ...current, page: 1, search: debouncedSearch }));
  }, [debouncedSearch, setPaging]);

  useEffect(() => {
    setSearchInput(paging.search);
  }, [paging.search]);
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
      render: (_, record) =>
        canManagePermanentOrg ? (
          <ActionMenu
            items={[
              {
                key: "edit",
                icon: <EditOutlined />,
                label: t("common.actions.edit"),
                onClick: () => {
                  setEditing(record);
                  onEdit(record);
                },
              },
              {
                key: "delete",
                icon: <DeleteOutlined />,
                label: t("common.actions.delete"),
                danger: true,
                onClick: () => {
                  setDeleting(record);
                  onDeletePrepare();
                },
              },
            ]}
            loading={deleting}
          />
        ) : null,
    },
  ];

  return (
    <Card variant="borderless">
      <Typography.Title level={5}>{title}</Typography.Title>
      <Typography.Paragraph type="secondary">{description}</Typography.Paragraph>
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
          style={{ width: isMobile ? "100%" : undefined, maxWidth: isMobile ? undefined : 360 }}
          placeholder={searchPlaceholder}
          value={searchInput}
          onChange={(event) => setSearchInput(event.target.value)}
          onSearch={(value) => setSearchInput(value)}
        />
        {canManagePermanentOrg ? (
          <Button type="primary" icon={<PlusOutlined />} size="large" onClick={() => setCreating(true)} block={isMobile}>
            {createLabel}
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
      )}
    </Card>
  );
}
