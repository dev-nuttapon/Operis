import { useState } from "react";
import { Alert, Card, DatePicker, Input, Select, Space, Table, Typography } from "antd";
import { WarningOutlined } from "@ant-design/icons";
import dayjs, { type Dayjs } from "dayjs";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useWorkflowOverrides } from "../hooks/useGovernance";

const { Title, Paragraph } = Typography;

export function WorkflowOverrideLogPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.governance.overrideLogRead);
  const [occurredRange, setOccurredRange] = useState<[Dayjs | null, Dayjs | null] | null>(null);
  const [filters, setFilters] = useState({ entityType: undefined as string | undefined, requestedBy: "", approvedBy: "", page: 1, pageSize: 50 });
  const query = useWorkflowOverrides({
    ...filters,
    occurredFrom: occurredRange?.[0]?.format("YYYY-MM-DD"),
    occurredTo: occurredRange?.[1]?.format("YYYY-MM-DD"),
  }, canRead);

  if (!canRead) {
    return <Alert type="warning" showIcon message="Workflow override log access is not available for this account." />;
  }

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #b45309, #7c2d12)", color: "#fff" }}>
            <WarningOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Workflow Override Log</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Search read-only override events by actor, entity, and occurrence date.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Space wrap style={{ marginBottom: 16 }}>
          <Input.Search allowClear placeholder="Requested by" style={{ width: 220 }} value={filters.requestedBy} onChange={(event) => setFilters((current) => ({ ...current, requestedBy: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, requestedBy: value, page: 1 }))} />
          <Input.Search allowClear placeholder="Approved by" style={{ width: 220 }} value={filters.approvedBy} onChange={(event) => setFilters((current) => ({ ...current, approvedBy: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, approvedBy: value, page: 1 }))} />
          <Select allowClear placeholder="Entity type" style={{ width: 180 }} options={["workflow", "change_request", "quality_gate"].map((value) => ({ label: value, value }))} value={filters.entityType} onChange={(value) => setFilters((current) => ({ ...current, entityType: value, page: 1 }))} />
          <DatePicker.RangePicker value={occurredRange} onChange={(value) => setOccurredRange(value as [Dayjs | null, Dayjs | null] | null)} />
        </Space>

        <Table
          rowKey="id"
          loading={query.isLoading}
          dataSource={query.data?.items ?? []}
          columns={[
            { title: "Entity", key: "entity", render: (_, item) => `${item.entityType}: ${item.entityId}` },
            { title: "Requested By", dataIndex: "requestedBy" },
            { title: "Approved By", dataIndex: "approvedBy" },
            { title: "Reason", dataIndex: "reason" },
            { title: "Occurred At", dataIndex: "occurredAt", render: (value: string) => dayjs(value).format("YYYY-MM-DD HH:mm") },
          ]}
          pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }}
        />
      </Card>
    </Space>
  );
}
