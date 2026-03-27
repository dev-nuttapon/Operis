import { Alert, Button, Card, Descriptions, Form, Input, Select, Space, Typography, message } from "antd";
import { useParams } from "react-router-dom";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCloseNonConformance, useNonConformance, useUpdateNonConformance } from "../hooks/useDefects";

const { Title, Paragraph, Text } = Typography;

export function NonConformanceDetailPage() {
  const { nonConformanceId } = useParams<{ nonConformanceId: string }>();
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.defects.read);
  const canManage = permissionState.hasPermission(permissions.defects.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const itemQuery = useNonConformance(nonConformanceId ?? null, canRead);
  const updateMutation = useUpdateNonConformance();
  const closeMutation = useCloseNonConformance();
  const [form] = Form.useForm<{ title: string; description: string; sourceType: string; ownerUserId: string; correctiveActionRef?: string; rootCause?: string; resolutionSummary?: string; acceptedDisposition?: string; status: string }>();

  if (!canRead) {
    return <Alert type="warning" showIcon message="Non-conformance access is not available for this account." />;
  }

  if (!itemQuery.data) {
    return <Alert type="info" showIcon message={itemQuery.isLoading ? "Loading non-conformance..." : "Non-conformance not found."} />;
  }

  const item = itemQuery.data;

  const handleUpdate = async () => {
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({ id: item.id, input: { ...values, linkedFindingRefs: item.linkedFindingRefs } });
      void messageApi.success("Non-conformance updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update non-conformance");
      void messageApi.error(presentation.description);
    }
  };

  const handleClose = async () => {
    try {
      await closeMutation.mutateAsync({ id: item.id, input: { correctiveActionRef: item.correctiveActionRef ?? null, acceptedDisposition: item.acceptedDisposition ?? null, resolutionSummary: item.resolutionSummary ?? null } });
      void messageApi.success("Non-conformance closed.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to close non-conformance");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Title level={3} style={{ margin: 0 }}>{item.code} · {item.title}</Title>
        <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
          Non-conformance detail with root cause, linked findings, and governed closure evidence.
        </Paragraph>
      </Card>
      <Card variant="borderless">
        <Descriptions column={2} size="small">
          <Descriptions.Item label="Project">{item.projectName}</Descriptions.Item>
          <Descriptions.Item label="Status">{item.status}</Descriptions.Item>
          <Descriptions.Item label="Source">{item.sourceType}</Descriptions.Item>
          <Descriptions.Item label="Owner">{item.ownerUserId}</Descriptions.Item>
          <Descriptions.Item label="Corrective Action">{item.correctiveActionRef ?? "-"}</Descriptions.Item>
          <Descriptions.Item label="Accepted Disposition">{item.acceptedDisposition ?? "-"}</Descriptions.Item>
          <Descriptions.Item label="Root Cause" span={2}>{item.rootCause ?? "-"}</Descriptions.Item>
          <Descriptions.Item label="Resolution" span={2}>{item.resolutionSummary ?? "-"}</Descriptions.Item>
        </Descriptions>
        <Paragraph style={{ marginTop: 16 }}>{item.description}</Paragraph>
        <Paragraph type="secondary">Linked findings: {item.linkedFindingRefs.length > 0 ? item.linkedFindingRefs.join(", ") : "none"}</Paragraph>
      </Card>
      <Card variant="borderless">
        <Form form={form} layout="vertical" initialValues={{ title: item.title, description: item.description, sourceType: item.sourceType, ownerUserId: item.ownerUserId, correctiveActionRef: item.correctiveActionRef ?? undefined, rootCause: item.rootCause ?? undefined, resolutionSummary: item.resolutionSummary ?? undefined, acceptedDisposition: item.acceptedDisposition ?? undefined, status: item.status }}>
          <Form.Item label="Title" name="title" rules={[{ required: true }]}><Input disabled={!canManage} /></Form.Item>
          <Form.Item label="Description" name="description" rules={[{ required: true }]}><Input.TextArea rows={4} disabled={!canManage} /></Form.Item>
          <Form.Item label="Source Type" name="sourceType" rules={[{ required: true }]}><Input disabled={!canManage} /></Form.Item>
          <Form.Item label="Owner User Id" name="ownerUserId" rules={[{ required: true }]}><Input disabled={!canManage} /></Form.Item>
          <Form.Item label="Corrective Action Ref" name="correctiveActionRef"><Input disabled={!canManage} /></Form.Item>
          <Form.Item label="Root Cause" name="rootCause"><Input.TextArea rows={3} disabled={!canManage} /></Form.Item>
          <Form.Item label="Resolution Summary" name="resolutionSummary"><Input.TextArea rows={3} disabled={!canManage} /></Form.Item>
          <Form.Item label="Accepted Disposition" name="acceptedDisposition"><Input.TextArea rows={3} disabled={!canManage} /></Form.Item>
          <Form.Item label="Status" name="status" rules={[{ required: true }]}><Select disabled={!canManage} options={["open", "in_review", "corrective_action", "closed"].map((value) => ({ label: value, value }))} /></Form.Item>
        </Form>
        <Space>
          <Button onClick={() => void handleUpdate()} disabled={!canManage} loading={updateMutation.isPending}>Save</Button>
          <Button type="primary" onClick={() => void handleClose()} disabled={!canManage || item.status !== "corrective_action"} loading={closeMutation.isPending}>Close</Button>
        </Space>
        {item.status === "corrective_action" && !item.correctiveActionRef && !item.acceptedDisposition ? <Text type="danger">Corrective action reference or accepted disposition is required before closure.</Text> : null}
      </Card>
    </Space>
  );
}
