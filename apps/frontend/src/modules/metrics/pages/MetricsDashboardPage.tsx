import { Alert, Card, Col, Row, Space, Statistic, Table, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useMetricResults } from "../hooks/useMetrics";
import type { MetricCurrentVsTargetItem, MetricResultItem } from "../types/metrics";

const { Title, Paragraph } = Typography;

export function MetricsDashboardPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.metrics.read);
  const resultsQuery = useMetricResults({ page: 1, pageSize: 25 }, canRead);

  const columns: ColumnsType<MetricResultItem> = [
    { title: "Metric", key: "metric", render: (_, item) => `${item.metricCode} • ${item.metricName}` },
    { title: "Measured", dataIndex: "measuredValue", key: "measuredValue" },
    { title: "Target", dataIndex: "targetValue", key: "targetValue" },
    { title: "Threshold", dataIndex: "thresholdValue", key: "thresholdValue" },
    { title: "Status", dataIndex: "status", key: "status", render: (value: string) => <Tag color={value === "threshold_breached" ? "red" : "green"}>{value}</Tag> },
    { title: "Measured At", dataIndex: "measuredAt", key: "measuredAt", render: (value: string) => new Date(value).toLocaleString() },
  ];

  const currentVsTargetColumns: ColumnsType<MetricCurrentVsTargetItem> = [
    { title: "Metric", key: "metric", render: (_, item) => `${item.metricCode} • ${item.metricName}` },
    { title: "Current", dataIndex: "currentValue", key: "currentValue" },
    { title: "Target", dataIndex: "targetValue", key: "targetValue" },
    { title: "Threshold", dataIndex: "thresholdValue", key: "thresholdValue" },
    { title: "Status", dataIndex: "status", key: "status", render: (value: string) => <Tag color={value === "threshold_breached" ? "red" : "green"}>{value}</Tag> },
  ];

  if (!canRead) {
    return <Alert type="warning" showIcon message="Metrics dashboard access is not available for this account." />;
  }

  const summary = resultsQuery.data?.summary;

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Title level={3} style={{ margin: 0 }}>Metrics Dashboard</Title>
        <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
          Review trend, current vs target, breach count, and open quality gate actions without per-widget API fan-out.
        </Paragraph>
      </Card>

      <Row gutter={[16, 16]}>
        <Col xs={24} md={8}><Card><Statistic title="Breach Count" value={summary?.breachCount ?? 0} /></Card></Col>
        <Col xs={24} md={8}><Card><Statistic title="Open Actions" value={summary?.openActions ?? 0} /></Card></Col>
        <Col xs={24} md={8}><Card><Statistic title="Tracked Metrics" value={summary?.currentVsTarget.length ?? 0} /></Card></Col>
      </Row>

      <Card variant="borderless" title="Current vs Target">
        <Table rowKey="metricDefinitionId" columns={currentVsTargetColumns} dataSource={summary?.currentVsTarget ?? []} pagination={false} />
      </Card>

      <Card variant="borderless" title="Recent Metric Results">
        <Table rowKey="id" loading={resultsQuery.isLoading} columns={columns} dataSource={resultsQuery.data?.items ?? []} pagination={false} />
      </Card>
    </Space>
  );
}
