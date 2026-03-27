import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Input, Select, Space, Table, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { BarChartOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useProjectOptions } from "../../users";
import { useControlCoverage } from "../hooks/useAssessment";
import type { ControlCoverageItem } from "../types/assessment";

const { Title, Paragraph, Text } = Typography;

const controlSetOptions = ["cmmi", "iso9001", "security", "internal"].map((value) => ({ value, label: value }));
const processAreaOptions = [
  "project_governance",
  "requirements_traceability",
  "document_governance",
  "change_control",
  "verification",
  "audit_capa",
  "security_resilience",
].map((value) => ({ value, label: value }));
const coverageStatusOptions = ["sufficient", "partial", "gap"].map((value) => ({ value, label: value }));

export function ControlCoveragePage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.assessment.controlsRead, permissions.assessment.controlsManage);
  const [filters, setFilters] = useState({ projectId: undefined as string | undefined, controlSet: undefined as string | undefined, processArea: undefined as string | undefined, coverageStatus: undefined as string | undefined, search: "", page: 1, pageSize: 25 });
  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 50 });
  const coverageQuery = useControlCoverage(filters, canRead);

  const columns = useMemo<ColumnsType<ControlCoverageItem>>(
    () => [
      { title: "Control", render: (_, item) => <Space direction="vertical" size={0}><Text strong>{item.controlCode}</Text><Text type="secondary">{item.title}</Text></Space> },
      { title: "Set", dataIndex: "controlSet" },
      { title: "Project", dataIndex: "projectName", render: (value) => value ?? "Global" },
      { title: "Coverage", dataIndex: "coverageStatus", render: (value: string) => <Tag color={value === "sufficient" ? "green" : value === "partial" ? "gold" : "red"}>{value}</Tag> },
      { title: "Active Mappings", dataIndex: "activeMappingCount" },
      { title: "Evidence", dataIndex: "evidenceCount" },
      { title: "Gaps", dataIndex: "gapCount" },
      { title: "Generated", dataIndex: "generatedAt", render: (value: string) => new Date(value).toLocaleString() },
    ],
    [],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Control coverage is not available for this account." />;
  }

  const rows = coverageQuery.data?.items ?? [];
  const totals = rows.reduce(
    (accumulator, item) => ({
      sufficient: accumulator.sufficient + (item.coverageStatus === "sufficient" ? 1 : 0),
      partial: accumulator.partial + (item.coverageStatus === "partial" ? 1 : 0),
      gap: accumulator.gap + (item.coverageStatus === "gap" ? 1 : 0),
    }),
    { sufficient: 0, partial: 0, gap: 0 },
  );

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #2563eb, #0f766e)", color: "#fff" }}>
            <BarChartOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Control Coverage</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Review control-to-evidence coverage snapshots, isolate gaps by project or process area, and use the latest generated totals for readiness review.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Flex gap={12} wrap="wrap">
        <Card size="small" style={{ minWidth: 180 }}><Text type="secondary">Sufficient</Text><Title level={4} style={{ margin: "6px 0 0" }}>{totals.sufficient}</Title></Card>
        <Card size="small" style={{ minWidth: 180 }}><Text type="secondary">Partial</Text><Title level={4} style={{ margin: "6px 0 0" }}>{totals.partial}</Title></Card>
        <Card size="small" style={{ minWidth: 180 }}><Text type="secondary">Gap</Text><Title level={4} style={{ margin: "6px 0 0" }}>{totals.gap}</Title></Card>
      </Flex>

      <Card
        variant="borderless"
        title="Coverage Snapshots"
        extra={<Button onClick={() => coverageQuery.refetch()}>Refresh snapshot</Button>}
      >
        <Flex gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Select allowClear showSearch placeholder="Project" style={{ width: 220 }} options={projectOptions.options} onSearch={projectOptions.onSearch} value={filters.projectId} onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))} />
          <Select allowClear placeholder="Control Set" style={{ width: 180 }} options={controlSetOptions} value={filters.controlSet} onChange={(value) => setFilters((current) => ({ ...current, controlSet: value, page: 1 }))} />
          <Select allowClear placeholder="Process Area" style={{ width: 220 }} options={processAreaOptions} value={filters.processArea} onChange={(value) => setFilters((current) => ({ ...current, processArea: value, page: 1 }))} />
          <Select allowClear placeholder="Coverage" style={{ width: 180 }} options={coverageStatusOptions} value={filters.coverageStatus} onChange={(value) => setFilters((current) => ({ ...current, coverageStatus: value, page: 1 }))} />
          <Input.Search allowClear placeholder="Search control code or title" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
        </Flex>
        <Table rowKey={(row) => `${row.controlId}-${row.generatedAt}`} loading={coverageQuery.isLoading} columns={columns} dataSource={rows} pagination={{ current: coverageQuery.data?.page ?? filters.page, pageSize: coverageQuery.data?.pageSize ?? filters.pageSize, total: coverageQuery.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>
    </Space>
  );
}
