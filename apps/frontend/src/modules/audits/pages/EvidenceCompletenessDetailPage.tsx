import { Alert, Button, Card, Space, Table, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useNavigate, useParams } from "react-router-dom";
import { useEvidenceRuleResult } from "../hooks/useAuditLogs";
import type { EvidenceMissingItem } from "../types/audits";

const { Title, Paragraph, Text } = Typography;

export function EvidenceCompletenessDetailPage() {
  const { resultId } = useParams<{ resultId: string }>();
  const navigate = useNavigate();
  const resultQuery = useEvidenceRuleResult(resultId ?? null, Boolean(resultId));

  if (!resultId) {
    return <Alert type="error" showIcon message="Evidence result id is required." />;
  }

  if (resultQuery.isLoading) {
    return <Card loading variant="borderless" />;
  }

  if (!resultQuery.data) {
    return <Alert type="warning" showIcon message="Evidence result was not found." />;
  }

  const columns: ColumnsType<EvidenceMissingItem> = [
    { title: "Title", dataIndex: "title" },
    { title: "Process Area", dataIndex: "processArea" },
    { title: "Artifact", dataIndex: "artifactType" },
    { title: "Reason", dataIndex: "reasonCode", render: (value: string) => <Tag color="red">{value}</Tag> },
    { title: "Scope", dataIndex: "scope" },
    { title: "Detected", dataIndex: "detectedAt", render: (value: string) => new Date(value).toLocaleString() },
    {
      title: "Source",
      key: "source",
      render: (_, item) => <Button type="link" onClick={() => navigate(item.route)}>Open source</Button>,
    },
  ];

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Title level={3} style={{ margin: 0 }}>Evidence Result Detail</Title>
        <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
          Review exact missing evidence items captured by this evaluation run.
        </Paragraph>
        <Space wrap>
          <Text strong>Status:</Text>
          <Tag color={resultQuery.data.status === "completed" ? "green" : "default"}>{resultQuery.data.status}</Tag>
          <Text strong>Missing:</Text>
          <Text>{resultQuery.data.missingItemCount}</Text>
          <Text strong>Completed:</Text>
          <Text>{new Date(resultQuery.data.completedAt).toLocaleString()}</Text>
        </Space>
      </Card>

      <Card variant="borderless" title="Missing Evidence Items">
        <Table rowKey="id" columns={columns} dataSource={resultQuery.data.missingItems} pagination={false} />
      </Card>
    </Space>
  );
}
