import { useMemo, useState } from "react";
import { Alert, Card, DatePicker, Flex, Input, Select, Space, Table, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import dayjs from "dayjs";
import { SafetyCertificateOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useAuditEvents } from "../hooks/useAuditLogs";
import type { AuditEventItem } from "../types/audits";

const { RangePicker } = DatePicker;
const { Title, Paragraph, Text } = Typography;

export function AuditLogsPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.auditLogs.read);
  const [filters, setFilters] = useState({
    entityType: undefined as string | undefined,
    action: undefined as string | undefined,
    actorUserId: undefined as string | undefined,
    outcome: undefined as string | undefined,
    from: undefined as string | undefined,
    to: undefined as string | undefined,
    page: 1,
    pageSize: 10,
  });
  const auditEventsQuery = useAuditEvents(filters, canRead);

  const columns = useMemo<ColumnsType<AuditEventItem>>(
    () => [
      {
        title: "Occurred At",
        dataIndex: "occurredAt",
        key: "occurredAt",
        render: (value: string) => dayjs(value).format("YYYY-MM-DD HH:mm"),
      },
      {
        title: "Action",
        dataIndex: "action",
        key: "action",
      },
      {
        title: "Entity",
        key: "entity",
        render: (_, item) => (
          <Space direction="vertical" size={0}>
            <Text>{item.entityType}</Text>
            <Text type="secondary">{item.entityId ?? "-"}</Text>
          </Space>
        ),
      },
      {
        title: "Actor",
        key: "actor",
        render: (_, item) => item.actorEmail ?? item.actorDisplayName ?? item.actorUserId ?? "-",
      },
      {
        title: "Outcome",
        dataIndex: "outcome",
        key: "outcome",
        render: (value: string) => <Tag color={value === "success" ? "green" : value === "failed" ? "red" : "gold"}>{value}</Tag>,
      },
      {
        title: "Reason",
        dataIndex: "reason",
        key: "reason",
        render: (value: string | null) => value ?? "-",
      },
    ],
    [],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Audit log access is not available for this account." />;
  }

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Flex gap={16} align="flex-start" wrap="wrap">
          <div
            style={{
              width: 48,
              height: 48,
              borderRadius: 14,
              display: "grid",
              placeItems: "center",
              background: "linear-gradient(135deg, #1d4ed8, #0f766e)",
              color: "#fff",
            }}
          >
            <SafetyCertificateOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Audit Log</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Search immutable audit events across modules, actors, outcomes, and time ranges for compliance review.
            </Paragraph>
          </div>
        </Flex>
      </Card>

      <Card variant="borderless">
        <Flex gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Input allowClear placeholder="Entity Type" style={{ width: 180 }} value={filters.entityType} onChange={(event) => setFilters((current) => ({ ...current, entityType: event.target.value || undefined, page: 1 }))} />
          <Input allowClear placeholder="Action" style={{ width: 180 }} value={filters.action} onChange={(event) => setFilters((current) => ({ ...current, action: event.target.value || undefined, page: 1 }))} />
          <Input allowClear placeholder="Actor" style={{ width: 220 }} value={filters.actorUserId} onChange={(event) => setFilters((current) => ({ ...current, actorUserId: event.target.value || undefined, page: 1 }))} />
          <Select allowClear placeholder="Outcome" style={{ width: 180 }} options={["success", "failed", "denied"].map((value) => ({ label: value, value }))} value={filters.outcome} onChange={(value) => setFilters((current) => ({ ...current, outcome: value, page: 1 }))} />
          <RangePicker
            onChange={(value) =>
              setFilters((current) => ({
                ...current,
                from: value?.[0]?.startOf("day").toISOString(),
                to: value?.[1]?.endOf("day").toISOString(),
                page: 1,
              }))
            }
          />
        </Flex>

        <Table
          rowKey="id"
          loading={auditEventsQuery.isLoading}
          columns={columns}
          dataSource={auditEventsQuery.data?.items ?? []}
          pagination={{
            current: auditEventsQuery.data?.page ?? filters.page,
            pageSize: auditEventsQuery.data?.pageSize ?? filters.pageSize,
            total: auditEventsQuery.data?.total ?? 0,
            showSizeChanger: true,
            onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })),
          }}
        />
      </Card>
    </Space>
  );
}
