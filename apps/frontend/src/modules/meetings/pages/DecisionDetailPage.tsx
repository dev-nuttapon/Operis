import { useParams } from "react-router-dom";
import { Alert, Button, Card, Checkbox, Descriptions, Form, Input, List, Select, Space, Tag, Typography, message } from "antd";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useDecision, useDecisionActions, useUpdateDecision } from "../hooks/useMeetings";
import type { DecisionUpdateInput } from "../types/meetings";

const { Title, Paragraph, Text } = Typography;

export function DecisionDetailPage() {
  const { decisionId } = useParams<{ decisionId: string }>();
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.meetings.read);
  const canManage = permissionState.hasPermission(permissions.meetings.manage);
  const canApprove = permissionState.hasPermission(permissions.meetings.approve);
  const canReadRestricted = permissionState.hasPermission(permissions.meetings.readRestricted);
  const [messageApi, contextHolder] = message.useMessage();
  const [form] = Form.useForm<DecisionUpdateInput>();
  const decisionQuery = useDecision(decisionId ?? null, canRead);
  const updateMutation = useUpdateDecision();
  const actions = useDecisionActions();

  if (!canRead) {
    return <Alert type="warning" showIcon message="Decision access is not available for this account." />;
  }

  if (!decisionQuery.data) {
    return <Alert type="info" showIcon message={decisionQuery.isLoading ? "Loading decision..." : "Decision not found or restricted."} />;
  }

  const decision = decisionQuery.data;

  const handleSave = async () => {
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({ id: decision.id, input: values });
      void messageApi.success("Decision updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update decision");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Title level={3} style={{ margin: 0 }}>{decision.code} · {decision.title}</Title>
        <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
          {decision.decisionType} · <Tag>{decision.status}</Tag>
        </Paragraph>
      </Card>

      <Card variant="borderless">
        <Descriptions bordered size="small" column={2}>
          <Descriptions.Item label="Project">{decision.projectName}</Descriptions.Item>
          <Descriptions.Item label="Meeting">{decision.meetingTitle ?? "standalone"}</Descriptions.Item>
          <Descriptions.Item label="Approved By">{decision.approvedBy ?? "pending"}</Descriptions.Item>
          <Descriptions.Item label="Restricted">{decision.isRestricted ? "yes" : "no"}</Descriptions.Item>
        </Descriptions>
      </Card>

      <Card variant="borderless" title="Decision detail">
        <Form form={form} layout="vertical" initialValues={{ title: decision.title, decisionType: decision.decisionType, rationale: decision.rationale, alternativesConsidered: decision.alternativesConsidered ?? undefined, impactedArtifacts: decision.impactedArtifacts, isRestricted: decision.isRestricted, classification: decision.classification ?? undefined }}>
          <Form.Item label="Title" name="title" rules={[{ required: true, message: "Title is required." }]}><Input disabled={!canManage} /></Form.Item>
          <Form.Item label="Decision Type" name="decisionType" rules={[{ required: true, message: "Decision type is required." }]}><Select disabled={!canManage} options={["approval", "architecture", "governance", "change"].map((value) => ({ label: value, value }))} /></Form.Item>
          <Form.Item label="Rationale" name="rationale" rules={[{ required: true, message: "Rationale is required." }]}><Input.TextArea rows={4} disabled={!canManage} /></Form.Item>
          <Form.Item label="Alternatives Considered" name="alternativesConsidered"><Input.TextArea rows={3} disabled={!canManage} /></Form.Item>
          <Form.Item label="Impacted Artifacts" name="impactedArtifacts"><Select mode="tags" disabled={!canManage} tokenSeparators={[","]} /></Form.Item>
          {canReadRestricted ? (
            <>
              <Form.Item name="isRestricted" valuePropName="checked"><Checkbox disabled={!canManage}>Restricted decision</Checkbox></Form.Item>
              <Form.Item shouldUpdate noStyle>
                {() => form.getFieldValue("isRestricted") ? <Form.Item label="Classification" name="classification" rules={[{ required: true, message: "Classification is required." }]}><Input disabled={!canManage} /></Form.Item> : null}
              </Form.Item>
            </>
          ) : null}
          <Space>
            <Button type="primary" disabled={!canManage} loading={updateMutation.isPending} onClick={() => void handleSave()}>Save decision</Button>
            <Button disabled={!canApprove || decision.status !== "proposed"} onClick={() => void actions.approve.mutateAsync({ id: decision.id, input: { reason: "Approved from detail" } })}>Approve</Button>
            <Button disabled={!canManage || decision.status !== "approved"} onClick={() => void actions.apply.mutateAsync({ id: decision.id, input: { reason: "Applied from detail" } })}>Apply</Button>
          </Space>
        </Form>
      </Card>

      <Card variant="borderless" title="History">
        <List dataSource={decision.history} renderItem={(item) => <List.Item><Space direction="vertical" size={0}><Text strong>{item.eventType}</Text><Text type="secondary">{item.actorUserId ?? "system"} · {item.occurredAt}</Text>{item.reason ? <Text>{item.reason}</Text> : null}</Space></List.Item>} />
      </Card>
    </Space>
  );
}
