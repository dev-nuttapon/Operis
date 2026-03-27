import { useMemo, useState } from "react";
import { Alert, Button, Card, Descriptions, Empty, Flex, Input, Modal, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useNavigate, useParams } from "react-router-dom";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useManagementReview, useTransitionManagementReview } from "../hooks/useGovernance";
import type { ManagementReviewAction, ManagementReviewItem } from "../types/governance";

const { Title, Paragraph, Text } = Typography;

export function ManagementReviewDetailPage() {
  const { reviewId } = useParams<{ reviewId: string }>();
  const navigate = useNavigate();
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(
    permissions.governance.managementReviewRead,
    permissions.governance.managementReviewManage,
    permissions.governance.managementReviewApprove,
  );
  const canManage = permissionState.hasPermission(permissions.governance.managementReviewManage);
  const canApprove = permissionState.hasPermission(permissions.governance.managementReviewApprove);
  const [messageApi, contextHolder] = message.useMessage();
  const [transitionTarget, setTransitionTarget] = useState<string | null>(null);
  const [reason, setReason] = useState("");
  const query = useManagementReview(reviewId ?? null, canRead && Boolean(reviewId));
  const transitionMutation = useTransitionManagementReview();

  const itemColumns = useMemo<ColumnsType<ManagementReviewItem>>(
    () => [
      { title: "Type", dataIndex: "itemType", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Title", dataIndex: "title" },
      { title: "Decision", dataIndex: "decision", render: (value?: string | null) => value ?? "-" },
      { title: "Owner", dataIndex: "ownerUserId", render: (value?: string | null) => value ?? "-" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "closed" ? "green" : "default"}>{value}</Tag> },
    ],
    [],
  );

  const actionColumns = useMemo<ColumnsType<ManagementReviewAction>>(
    () => [
      { title: "Action", dataIndex: "title" },
      { title: "Owner", dataIndex: "ownerUserId" },
      { title: "Due", dataIndex: "dueAt", render: (value?: string | null) => value ? new Date(value).toLocaleString() : "-" },
      { title: "Mandatory", dataIndex: "isMandatory", render: (value: boolean) => <Tag color={value ? "red" : "default"}>{value ? "mandatory" : "optional"}</Tag> },
      { title: "Linked", key: "linked", render: (_, item) => item.linkedEntityType && item.linkedEntityId ? `${item.linkedEntityType}:${item.linkedEntityId}` : "-" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "closed" ? "green" : value === "in_progress" ? "blue" : "gold"}>{value}</Tag> },
    ],
    [],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Management review data is not available for this account." />;
  }

  if (!query.isLoading && !query.data) {
    return (
      <Card variant="borderless">
        <Empty description="Management review not found.">
          <Button onClick={() => navigate("/app/governance/management-reviews")}>Back to reviews</Button>
        </Empty>
      </Card>
    );
  }

  const review = query.data;

  const transition = async () => {
    if (!review || !reviewId || !transitionTarget) return;
    try {
      await transitionMutation.mutateAsync({ id: reviewId, input: { targetStatus: transitionTarget, reason } });
      setTransitionTarget(null);
      setReason("");
      void messageApi.success(`Management review moved to ${transitionTarget}.`);
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to transition management review");
      void messageApi.error(presentation.description);
    }
  };

  const availableTransitions = review ? getAvailableTransitions(review.status, canManage, canApprove) : [];

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless" loading={query.isLoading}>
        <Flex justify="space-between" align="start" gap={16} wrap="wrap">
          <div>
            <Title level={3} style={{ margin: 0 }}>{review?.reviewCode ?? "Management Review"}</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {review?.title}
            </Paragraph>
          </div>
          <Space wrap>
            {availableTransitions.map((target) => (
              <Button key={target} onClick={() => setTransitionTarget(target)}>{target}</Button>
            ))}
          </Space>
        </Flex>
      </Card>

      <Card variant="borderless" loading={query.isLoading}>
        <Descriptions column={2} bordered size="small">
          <Descriptions.Item label="Project">{review?.projectName ?? review?.projectId ?? "-"}</Descriptions.Item>
          <Descriptions.Item label="Period">{review?.reviewPeriod ?? "-"}</Descriptions.Item>
          <Descriptions.Item label="Scheduled">{review?.scheduledAt ? new Date(review.scheduledAt).toLocaleString() : "-"}</Descriptions.Item>
          <Descriptions.Item label="Facilitator">{review?.facilitatorUserId ?? "-"}</Descriptions.Item>
          <Descriptions.Item label="Status"><Tag>{review?.status}</Tag></Descriptions.Item>
          <Descriptions.Item label="Escalation Link">{review?.escalationEntityType && review?.escalationEntityId ? `${review.escalationEntityType}:${review.escalationEntityId}` : "-"}</Descriptions.Item>
          <Descriptions.Item label="Agenda Summary" span={2}>{review?.agendaSummary ?? "-"}</Descriptions.Item>
          <Descriptions.Item label="Minutes Summary" span={2}>{review?.minutesSummary ?? "-"}</Descriptions.Item>
          <Descriptions.Item label="Decision Summary" span={2}>{review?.decisionSummary ?? "-"}</Descriptions.Item>
        </Descriptions>
      </Card>

      <Card variant="borderless" title="Agenda & Decision Items" loading={query.isLoading}>
        <Table rowKey="id" columns={itemColumns} dataSource={review?.items ?? []} pagination={false} />
      </Card>

      <Card variant="borderless" title="Follow-up Actions" loading={query.isLoading}>
        <Table rowKey="id" columns={actionColumns} dataSource={review?.actions ?? []} pagination={false} />
      </Card>

      <Card variant="borderless" title="Audit History" loading={query.isLoading}>
        {(review?.history?.length ?? 0) === 0 ? (
          <Empty description="No audit history recorded." />
        ) : (
          <Space direction="vertical" size={12} style={{ width: "100%" }}>
            {review?.history.map((entry) => (
              <Card key={entry.id} size="small">
                <Space direction="vertical" size={2}>
                  <Text strong>{entry.reason || "Change recorded"}</Text>
                  <Text type="secondary">{entry.requestedBy} • {new Date(entry.occurredAt).toLocaleString()}</Text>
                </Space>
              </Card>
            ))}
          </Space>
        )}
      </Card>

      <Modal
        title={transitionTarget ? `Move to ${transitionTarget}` : "Transition Review"}
        open={Boolean(transitionTarget)}
        onOk={() => void transition()}
        onCancel={() => { setTransitionTarget(null); setReason(""); }}
        confirmLoading={transitionMutation.isPending}
        destroyOnHidden
      >
        <Space direction="vertical" size={12} style={{ width: "100%" }}>
          <Paragraph type="secondary" style={{ margin: 0 }}>
            Closing requires minutes, decision summary, and all mandatory actions closed.
          </Paragraph>
          <Input.TextArea rows={4} value={reason} onChange={(event) => setReason(event.target.value)} placeholder="Reason for this transition" />
        </Space>
      </Modal>
    </Space>
  );
}

function getAvailableTransitions(status: string, canManage: boolean, canApprove: boolean) {
  const normalized = status.toLowerCase();
  if (normalized === "draft" && canManage) return ["scheduled"];
  if (normalized === "scheduled" && canManage) return ["in_review"];
  if (normalized === "in_review" && canApprove) return ["closed"];
  if (normalized === "closed" && canApprove) return ["archived"];
  return [];
}
