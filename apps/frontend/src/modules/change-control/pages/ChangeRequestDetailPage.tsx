import { Alert, Button, Card, Descriptions, Empty, Flex, Input, List, Modal, Space, Typography, message } from "antd";
import { useParams } from "react-router-dom";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useChangeRequest, useChangeRequestActions } from "../hooks/useChangeControl";

const { Title, Paragraph } = Typography;

export function ChangeRequestDetailPage() {
  const { changeRequestId = null } = useParams();
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.changeControl.read);
  const canManage = permissionState.hasPermission(permissions.changeControl.manage);
  const canApprove = permissionState.hasPermission(permissions.changeControl.approve);
  const [messageApi, contextHolder] = message.useMessage();
  const query = useChangeRequest(changeRequestId, canRead);
  const actions = useChangeRequestActions();
  const detail = query.data;

  if (!canRead) {
    return <Alert type="warning" showIcon message="Change request access is not available for this account." />;
  }

  if (!detail && !query.isLoading) {
    return <Empty description="Change request not found." />;
  }

  const requestReason = async (title: string, action: (reason: string) => Promise<unknown>) => {
    let reasonValue = "";
    Modal.confirm({
      title,
      content: <Input.TextArea rows={4} onChange={(event) => { reasonValue = event.target.value; }} placeholder="Provide rationale" />,
      onOk: async () => {
        try {
          await action(reasonValue);
          void messageApi.success(`${title} completed.`);
        } catch (error) {
          const presentation = getApiErrorPresentation(error, `Unable to ${title.toLowerCase()}`);
          void messageApi.error(presentation.description);
        }
      },
    });
  };

  const requestSummary = async (title: string, action: (summary: string) => Promise<unknown>) => {
    let summaryValue = "";
    Modal.confirm({
      title,
      content: <Input.TextArea rows={4} onChange={(event) => { summaryValue = event.target.value; }} placeholder="Provide implementation summary" />,
      onOk: async () => {
        try {
          await action(summaryValue);
          void messageApi.success(`${title} completed.`);
        } catch (error) {
          const presentation = getApiErrorPresentation(error, `Unable to ${title.toLowerCase()}`);
          void messageApi.error(presentation.description);
        }
      },
    });
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless" loading={query.isLoading}>
        <Flex justify="space-between" gap={16} wrap="wrap">
          <div>
            <Title level={3} style={{ margin: 0 }}>{detail?.code}</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>{detail?.projectName}</Paragraph>
          </div>
          <Flex gap={8} wrap>
            <Button disabled={!canManage || detail?.status !== "draft"} onClick={() => changeRequestId && void actions.submit.mutateAsync(changeRequestId)}>Submit</Button>
            <Button disabled={!canApprove || detail?.status !== "submitted"} onClick={() => void requestReason("Approve change request", async (reason) => { if (changeRequestId) { await actions.approve.mutateAsync({ id: changeRequestId, reason }); } })}>Approve</Button>
            <Button disabled={!canApprove || detail?.status !== "submitted"} onClick={() => void requestReason("Reject change request", async (reason) => { if (changeRequestId) { await actions.reject.mutateAsync({ id: changeRequestId, reason }); } })}>Reject</Button>
            <Button disabled={!canManage || detail?.status !== "approved"} onClick={() => void requestSummary("Implement change request", async (summary) => { if (changeRequestId) { await actions.implement.mutateAsync({ id: changeRequestId, summary }); } })}>Implement</Button>
            <Button disabled={!canManage || detail?.status !== "implemented"} onClick={() => void requestSummary("Close change request", async (summary) => { if (changeRequestId) { await actions.close.mutateAsync({ id: changeRequestId, summary }); } })}>Close</Button>
          </Flex>
        </Flex>
      </Card>

      <Card variant="borderless" title="Change Request Detail">
        <Descriptions bordered size="small" column={2}>
          <Descriptions.Item label="Title" span={2}>{detail?.title}</Descriptions.Item>
          <Descriptions.Item label="Reason" span={2}>{detail?.reason}</Descriptions.Item>
          <Descriptions.Item label="Priority">{detail?.priority}</Descriptions.Item>
          <Descriptions.Item label="Status">{detail?.status}</Descriptions.Item>
          <Descriptions.Item label="Requested By">{detail?.requestedBy}</Descriptions.Item>
          <Descriptions.Item label="Target Baseline">{detail?.targetBaselineName ?? "-"}</Descriptions.Item>
          <Descriptions.Item label="Decision Rationale" span={2}>{detail?.decisionRationale ?? "-"}</Descriptions.Item>
          <Descriptions.Item label="Implementation Summary" span={2}>{detail?.implementationSummary ?? "-"}</Descriptions.Item>
        </Descriptions>
      </Card>

      <Card variant="borderless" title="Impact Analysis">
        <Descriptions bordered size="small" column={1}>
          <Descriptions.Item label="Scope Impact">{detail?.impact.scopeImpact}</Descriptions.Item>
          <Descriptions.Item label="Schedule Impact">{detail?.impact.scheduleImpact}</Descriptions.Item>
          <Descriptions.Item label="Quality Impact">{detail?.impact.qualityImpact}</Descriptions.Item>
          <Descriptions.Item label="Security Impact">{detail?.impact.securityImpact}</Descriptions.Item>
          <Descriptions.Item label="Performance Impact">{detail?.impact.performanceImpact}</Descriptions.Item>
          <Descriptions.Item label="Risk Impact">{detail?.impact.riskImpact}</Descriptions.Item>
        </Descriptions>
      </Card>

      <Card variant="borderless" title="Linked Scope">
        <Descriptions bordered size="small" column={1}>
          <Descriptions.Item label="Linked Requirements">{detail?.linkedRequirementIds.length ? detail.linkedRequirementIds.join(", ") : "-"}</Descriptions.Item>
          <Descriptions.Item label="Linked Configuration Items">{detail?.linkedConfigurationItemIds.length ? detail.linkedConfigurationItemIds.join(", ") : "-"}</Descriptions.Item>
        </Descriptions>
      </Card>

      <Card variant="borderless" title="History">
        <List
          dataSource={detail?.history ?? []}
          renderItem={(item) => (
            <List.Item>
              <List.Item.Meta title={item.summary ?? item.eventType} description={`${item.actorUserId ?? "system"} · ${new Date(item.occurredAt).toLocaleString()}${item.reason ? ` · ${item.reason}` : ""}`} />
            </List.Item>
          )}
        />
      </Card>
    </Space>
  );
}
