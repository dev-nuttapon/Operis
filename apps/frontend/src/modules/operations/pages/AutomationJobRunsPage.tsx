import { useMemo, useState } from "react";
import { Alert, Button, Card, Descriptions, Drawer, Flex, Input, Select, Space, Table, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { HistoryOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useAutomationJobRuns } from "../hooks/useOperations";
import type { AutomationJobRun } from "../types/operations";

const { Title, Paragraph, Text } = Typography;

const statusOptions = ["queued", "running", "succeeded", "failed"].map((value) => ({ value, label: value }));
const jobTypeOptions = ["backup", "retention", "export", "secret_rotation", "alert"].map((value) => ({ value, label: value }));

export function AutomationJobRunsPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.operations.automationRead, permissions.operations.automationManage, permissions.operations.automationExecute);
  const [filters, setFilters] = useState({ jobType: undefined as string | undefined, status: undefined as string | undefined, triggeredBy: undefined as string | undefined, search: "", page: 1, pageSize: 25 });
  const [selectedRun, setSelectedRun] = useState<AutomationJobRun | null>(null);
  const runsQuery = useAutomationJobRuns(filters, canRead);

  const columns = useMemo<ColumnsType<AutomationJobRun>>(
    () => [
      { title: "Job", render: (_, item) => <Space direction="vertical" size={0}><Text strong>{item.jobName}</Text><Text type="secondary">{item.jobType}</Text></Space> },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "failed" ? "red" : value === "succeeded" ? "green" : "blue"}>{value}</Tag> },
      { title: "Triggered By", dataIndex: "triggeredBy" },
      { title: "Queued At", dataIndex: "queuedAt", render: (value: string) => new Date(value).toLocaleString() },
      { title: "Evidence", render: (_, item) => item.evidenceRefs.length },
      { title: "Failure", dataIndex: "errorSummary", render: (value) => value ?? "-" },
      { title: "Actions", render: (_, item) => <Button size="small" onClick={() => setSelectedRun(item)}>Open</Button> },
    ],
    [],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Automation job runs are not available for this account." />;
  }

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #2563eb, #0f766e)", color: "#fff" }}>
            <HistoryOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Automation Runs</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Review queued, running, successful, and failed operational job runs together with evidence links and remediation paths.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless" title="Run History">
        <Flex gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Select allowClear placeholder="Job Type" style={{ width: 180 }} options={jobTypeOptions} value={filters.jobType} onChange={(value) => setFilters((current) => ({ ...current, jobType: value, page: 1 }))} />
          <Select allowClear placeholder="Status" style={{ width: 180 }} options={statusOptions} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          <Input allowClear placeholder="Triggered By" style={{ width: 220 }} value={filters.triggeredBy} onChange={(event) => setFilters((current) => ({ ...current, triggeredBy: event.target.value || undefined, page: 1 }))} />
          <Input.Search allowClear placeholder="Search job or failure" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
        </Flex>
        <Table rowKey="id" loading={runsQuery.isLoading} columns={columns} dataSource={runsQuery.data?.items ?? []} pagination={{ current: runsQuery.data?.page ?? filters.page, pageSize: runsQuery.data?.pageSize ?? filters.pageSize, total: runsQuery.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Drawer title="Automation Run Detail" open={Boolean(selectedRun)} width={780} onClose={() => setSelectedRun(null)} destroyOnHidden>
        {selectedRun ? (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <Descriptions bordered size="small" column={1}>
              <Descriptions.Item label="Job">{selectedRun.jobName}</Descriptions.Item>
              <Descriptions.Item label="Job Type">{selectedRun.jobType}</Descriptions.Item>
              <Descriptions.Item label="Status"><Tag>{selectedRun.status}</Tag></Descriptions.Item>
              <Descriptions.Item label="Triggered By">{selectedRun.triggeredBy}</Descriptions.Item>
              <Descriptions.Item label="Trigger Reason">{selectedRun.triggerReason ?? "-"}</Descriptions.Item>
              <Descriptions.Item label="Error Summary">{selectedRun.errorSummary ?? "-"}</Descriptions.Item>
              <Descriptions.Item label="Remediation Path">{selectedRun.remediationPath ?? "-"}</Descriptions.Item>
            </Descriptions>

            <Card size="small" title={`Evidence Links (${selectedRun.evidenceRefs.length})`}>
              <Table
                rowKey="id"
                size="small"
                pagination={false}
                dataSource={selectedRun.evidenceRefs}
                columns={[
                  { title: "Entity", render: (_, item) => `${item.entityType}:${item.entityId}` },
                  { title: "Evidence Ref", dataIndex: "evidenceRef" },
                  { title: "Route", dataIndex: "route" },
                  {
                    title: "Open",
                    render: (_, item) => <Button type="link" onClick={() => window.location.assign(item.route)}>Open source</Button>,
                  },
                ]}
              />
            </Card>
          </Space>
        ) : null}
      </Drawer>
    </Space>
  );
}
