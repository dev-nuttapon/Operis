import { useState } from "react";
import { Alert, Card, Drawer, Input, Select, Space, Table, Tag, Timeline, Typography } from "antd";
import { HistoryOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useChangeRequest, useChangeRequests } from "../hooks/useChangeControl";

export function ChangeLogPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.changeControl.read, permissions.changeControl.manage, permissions.changeControl.approve);
  const [filters, setFilters] = useState({ search: "", status: undefined as string | undefined, page: 1, pageSize: 25 });
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const listQuery = useChangeRequests(filters, canRead);
  const detailQuery = useChangeRequest(selectedId, Boolean(selectedId));

  if (!canRead) {
    return <Alert type="warning" showIcon message="Change log is not available for this account." />;
  }

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0f766e, #1d4ed8)", color: "#fff" }}>
            <HistoryOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>Change Log</Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Review governed change requests and inspect their request-to-close event history in one place.
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Space style={{ marginBottom: 16 }} wrap>
          <Input.Search allowClear placeholder="Search code, title, or requester" style={{ width: 260 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
          <Select allowClear placeholder="Status" style={{ width: 180 }} options={["draft", "submitted", "approved", "rejected", "implemented", "closed"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
        </Space>
        <Table rowKey="id" loading={listQuery.isLoading} dataSource={listQuery.data?.items ?? []} pagination={{ current: listQuery.data?.page ?? filters.page, pageSize: listQuery.data?.pageSize ?? filters.pageSize, total: listQuery.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} onRow={(record) => ({ onClick: () => setSelectedId(record.id) })} columns={[
          { title: "Code", dataIndex: "code" },
          { title: "Title", dataIndex: "title" },
          { title: "Requester", dataIndex: "requestedBy" },
          { title: "Priority", dataIndex: "priority" },
          { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
          { title: "Updated", dataIndex: "updatedAt", render: (value: string | null) => value ? new Date(value).toLocaleString() : "-" },
        ]} />
      </Card>

      <Drawer open={Boolean(selectedId)} onClose={() => setSelectedId(null)} title="Change History" width={620}>
        {detailQuery.data ? (
          <Timeline items={(detailQuery.data.history ?? []).map((item) => ({
            children: (
              <Space direction="vertical" size={0}>
                <Typography.Text strong>{item.eventType}</Typography.Text>
                <Typography.Text type="secondary">{item.occurredAt ? new Date(item.occurredAt).toLocaleString() : "-"}</Typography.Text>
                <Typography.Text>{item.summary || item.reason || "-"}</Typography.Text>
              </Space>
            ),
          }))} />
        ) : null}
      </Drawer>
    </Space>
  );
}
