import { useParams } from "react-router-dom";
import { Alert, Button, Card, Descriptions, Flex, Form, Input, InputNumber, List, Space, Tag, Typography, message } from "antd";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useRisk, useRiskActions, useUpdateRisk } from "../hooks/useRisks";
import type { RiskUpdateInput } from "../types/risks";

const { Title, Paragraph, Text } = Typography;

export function RiskDetailPage() {
  const { riskId } = useParams<{ riskId: string }>();
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.risks.read);
  const canManage = permissionState.hasPermission(permissions.risks.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [form] = Form.useForm<RiskUpdateInput>();
  const riskQuery = useRisk(riskId ?? null, canRead);
  const updateRiskMutation = useUpdateRisk();
  const actions = useRiskActions();

  if (!canRead) {
    return <Alert type="warning" showIcon message="Risk access is not available for this account." />;
  }

  if (!riskQuery.data) {
    return <Alert type="info" showIcon message={riskQuery.isLoading ? "Loading risk..." : "Risk not found."} />;
  }

  const risk = riskQuery.data;

  const handleSave = async () => {
    const values = await form.validateFields();
    try {
      await updateRiskMutation.mutateAsync({ id: risk.id, input: values });
      void messageApi.success("Risk updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update risk");
      void messageApi.error(presentation.description);
    }
  };

  const runRiskAction = async (action: "assess" | "mitigate" | "close") => {
    try {
      if (action === "assess") {
        await actions.assess.mutateAsync({ id: risk.id, input: {} });
      } else if (action === "mitigate") {
        await actions.mitigate.mutateAsync({ id: risk.id, input: { mitigationPlan: risk.mitigationPlan ?? undefined } });
      } else {
        await actions.close.mutateAsync({ id: risk.id, input: {} });
      }

      void messageApi.success(`Risk ${action}ed.`);
    } catch (error) {
      const presentation = getApiErrorPresentation(error, `Unable to ${action} risk`);
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Flex justify="space-between" gap={16} wrap="wrap">
          <div>
            <Title level={3} style={{ margin: 0 }}>{risk.code} · {risk.title}</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Current status: <Tag>{risk.status}</Tag>
            </Paragraph>
          </div>
          <Flex gap={8} wrap>
            <Button disabled={!canManage || risk.status !== "draft"} onClick={() => void runRiskAction("assess")}>Assess</Button>
            <Button disabled={!canManage || risk.status !== "assessed"} onClick={() => void runRiskAction("mitigate")}>Mitigate</Button>
            <Button disabled={!canManage || risk.status !== "mitigated"} onClick={() => void runRiskAction("close")}>Close</Button>
          </Flex>
        </Flex>
      </Card>

      <Card variant="borderless">
        <Descriptions column={2} bordered size="small">
          <Descriptions.Item label="Project">{risk.projectName}</Descriptions.Item>
          <Descriptions.Item label="Owner">{risk.ownerUserId}</Descriptions.Item>
          <Descriptions.Item label="Probability">{risk.probability}</Descriptions.Item>
          <Descriptions.Item label="Impact">{risk.impact}</Descriptions.Item>
          <Descriptions.Item label="Next review">{risk.nextReviewAt ? new Date(risk.nextReviewAt).toLocaleString() : "not set"}</Descriptions.Item>
          <Descriptions.Item label="Updated">{new Date(risk.updatedAt).toLocaleString()}</Descriptions.Item>
        </Descriptions>
      </Card>

      <Card variant="borderless" title="Update risk">
        <Form
          form={form}
          layout="vertical"
          initialValues={{
            title: risk.title,
            description: risk.description,
            probability: risk.probability,
            impact: risk.impact,
            ownerUserId: risk.ownerUserId,
            mitigationPlan: risk.mitigationPlan ?? undefined,
            cause: risk.cause ?? undefined,
            effect: risk.effect ?? undefined,
            contingencyPlan: risk.contingencyPlan ?? undefined,
            nextReviewAt: risk.nextReviewAt ?? undefined,
          }}
        >
          <Form.Item label="Title" name="title" rules={[{ required: true, message: "Title is required." }]}>
            <Input disabled={!canManage} />
          </Form.Item>
          <Form.Item label="Description" name="description" rules={[{ required: true, message: "Description is required." }]}>
            <Input.TextArea rows={4} disabled={!canManage} />
          </Form.Item>
          <Flex gap={12}>
            <Form.Item label="Probability" name="probability" rules={[{ required: true, message: "Probability is required." }]} style={{ flex: 1 }}>
              <InputNumber min={1} max={5} style={{ width: "100%" }} disabled={!canManage} />
            </Form.Item>
            <Form.Item label="Impact" name="impact" rules={[{ required: true, message: "Impact is required." }]} style={{ flex: 1 }}>
              <InputNumber min={1} max={5} style={{ width: "100%" }} disabled={!canManage} />
            </Form.Item>
          </Flex>
          <Form.Item label="Owner" name="ownerUserId" rules={[{ required: true, message: "Owner is required." }]}>
            <Input disabled={!canManage} />
          </Form.Item>
          <Form.Item label="Cause" name="cause">
            <Input.TextArea rows={3} disabled={!canManage} />
          </Form.Item>
          <Form.Item label="Effect" name="effect">
            <Input.TextArea rows={3} disabled={!canManage} />
          </Form.Item>
          <Form.Item label="Mitigation plan" name="mitigationPlan">
            <Input.TextArea rows={3} disabled={!canManage} />
          </Form.Item>
          <Form.Item label="Contingency plan" name="contingencyPlan">
            <Input.TextArea rows={3} disabled={!canManage} />
          </Form.Item>
          <Button type="primary" disabled={!canManage} loading={updateRiskMutation.isPending} onClick={() => void handleSave()}>
            Save changes
          </Button>
        </Form>
      </Card>

      <Card variant="borderless" title="Review history">
        <List
          dataSource={risk.reviews}
          renderItem={(item) => (
            <List.Item>
              <Space direction="vertical" size={0}>
                <Text strong>{item.decision}</Text>
                <Text type="secondary">{item.reviewedBy} · {new Date(item.reviewedAt).toLocaleString()}</Text>
                {item.notes ? <Text>{item.notes}</Text> : null}
              </Space>
            </List.Item>
          )}
        />
      </Card>
    </Space>
  );
}
