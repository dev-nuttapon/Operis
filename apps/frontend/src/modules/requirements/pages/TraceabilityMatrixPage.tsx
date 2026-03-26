import { Alert, Card, Flex, Select, Space, Table, Tag, Typography } from "antd";
import { useState } from "react";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useProjectOptions } from "../../users";
import { useTraceabilityMatrix } from "../hooks/useRequirements";

const { Title, Paragraph, Text } = Typography;

export function TraceabilityMatrixPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.requirements.read);
  const [filters, setFilters] = useState({
    projectId: undefined as string | undefined,
    baselineStatus: undefined as string | undefined,
    missingCoverage: undefined as boolean | undefined,
    page: 1,
    pageSize: 20,
  });
  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 20 });
  const traceabilityQuery = useTraceabilityMatrix(filters, canRead);

  if (!canRead) {
    return <Alert type="warning" showIcon message="Traceability access is not available for this account." />;
  }

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Title level={3} style={{ margin: 0 }}>Traceability Matrix</Title>
        <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
          Read-only traceability coverage across requirements, documents, tests, change requests, and releases.
        </Paragraph>
      </Card>

      <Card variant="borderless">
        <Flex gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Select
            allowClear
            showSearch
            placeholder="Project"
            style={{ width: 220 }}
            options={projectOptions.options}
            onSearch={projectOptions.onSearch}
            onPopupScroll={(event) => {
              const target = event.target as HTMLDivElement;
              if (target.scrollTop + target.clientHeight >= target.scrollHeight - 24) {
                projectOptions.onLoadMore();
              }
            }}
            onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))}
          />
          <Select allowClear placeholder="Baseline" style={{ width: 180 }} options={[{ label: "locked", value: "locked" }]} onChange={(value) => setFilters((current) => ({ ...current, baselineStatus: value, page: 1 }))} />
          <Select
            allowClear
            placeholder="Coverage"
            style={{ width: 180 }}
            options={[{ label: "Missing coverage", value: "missing" }, { label: "Coverage complete", value: "complete" }]}
            onChange={(value) => setFilters((current) => ({ ...current, missingCoverage: value === undefined ? undefined : value === "missing", page: 1 }))}
          />
        </Flex>

        <Table
          rowKey="requirementId"
          loading={traceabilityQuery.isLoading}
          dataSource={traceabilityQuery.data?.items ?? []}
          pagination={{
            current: traceabilityQuery.data?.page ?? filters.page,
            pageSize: traceabilityQuery.data?.pageSize ?? filters.pageSize,
            total: traceabilityQuery.data?.total ?? 0,
            showSizeChanger: true,
            onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })),
          }}
          columns={[
            {
              title: "Requirement",
              key: "requirement",
              render: (_, item) => (
                <Space direction="vertical" size={0}>
                  <Text strong>{item.requirementCode}</Text>
                  <Text>{item.requirementTitle}</Text>
                </Space>
              ),
            },
            { title: "Project", dataIndex: "projectName", key: "projectName" },
            { title: "Status", dataIndex: "requirementStatus", key: "requirementStatus", render: (value: string) => <Tag>{value}</Tag> },
            { title: "Baseline", dataIndex: "baselineStatus", key: "baselineStatus", render: (value?: string | null) => value ? <Tag color="blue">{value}</Tag> : "-" },
            { title: "Missing Links", dataIndex: "missingLinkCount", key: "missingLinkCount" },
            {
              title: "Coverage",
              key: "links",
              render: (_, item) => (
                <Flex gap={4} wrap>
                  {item.links.length === 0 ? <Text type="secondary">No links</Text> : item.links.map((link) => (
                    <Tag key={link.id}>{`${link.targetType}:${link.targetId}`}</Tag>
                  ))}
                </Flex>
              ),
            },
          ]}
        />
      </Card>
    </Space>
  );
}
