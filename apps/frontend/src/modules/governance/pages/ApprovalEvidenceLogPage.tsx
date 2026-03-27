import { useState } from "react";
import { Alert, Card, DatePicker, Input, Select, Space, Table, Tag, Typography } from "antd";
import { AuditOutlined } from "@ant-design/icons";
import dayjs, { type Dayjs } from "dayjs";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useApprovalEvidence } from "../hooks/useGovernance";

const { Title, Paragraph } = Typography;

export function ApprovalEvidenceLogPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.governance.approvalEvidenceRead);
  const [approvedRange, setApprovedRange] = useState<[Dayjs | null, Dayjs | null] | null>(null);
  const [filters, setFilters] = useState({ entityType: undefined as string | undefined, actorUserId: "", outcome: undefined as string | undefined, page: 1, pageSize: 50 });
  const query = useApprovalEvidence({
    ...filters,
    approvedFrom: approvedRange?.[0]?.format("YYYY-MM-DD"),
    approvedTo: approvedRange?.[1]?.format("YYYY-MM-DD"),
  }, canRead);

  if (!canRead) {
    return <Alert type="warning" showIcon message="Approval evidence access is not available for this account." />;
  }

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #1d4ed8, #0f172a)", color: "#fff" }}>
            <AuditOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Approval Evidence Log</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Review immutable approval evidence by entity, approver, outcome, and approval date.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Space wrap style={{ marginBottom: 16 }}>
          <Input.Search allowClear placeholder="Approver" style={{ width: 220 }} value={filters.actorUserId} onChange={(event) => setFilters((current) => ({ ...current, actorUserId: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, actorUserId: value, page: 1 }))} />
          <Select allowClear placeholder="Entity type" style={{ width: 180 }} options={["raci_map", "sla_rule", "retention_policy"].map((value) => ({ label: value, value }))} value={filters.entityType} onChange={(value) => setFilters((current) => ({ ...current, entityType: value, page: 1 }))} />
          <Select allowClear placeholder="Outcome" style={{ width: 160 }} options={[{ label: "approved", value: "approved" }]} value={filters.outcome} onChange={(value) => setFilters((current) => ({ ...current, outcome: value, page: 1 }))} />
          <DatePicker.RangePicker value={approvedRange} onChange={(value) => setApprovedRange(value as [Dayjs | null, Dayjs | null] | null)} />
        </Space>

        <Table
          rowKey="id"
          loading={query.isLoading}
          dataSource={query.data?.items ?? []}
          columns={[
            { title: "Entity", key: "entity", render: (_, item) => `${item.entityType}: ${item.entityId}` },
            { title: "Approver", dataIndex: "approverUserId" },
            { title: "Outcome", dataIndex: "outcome", render: (value: string) => <Tag color="green">{value}</Tag> },
            { title: "Reason", dataIndex: "reason" },
            { title: "Approved At", dataIndex: "approvedAt", render: (value: string) => dayjs(value).format("YYYY-MM-DD HH:mm") },
          ]}
          pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }}
        />
      </Card>
    </Space>
  );
}
