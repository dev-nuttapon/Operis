import { Alert, Button, Card, Descriptions, Form, Input, Select, Space, Typography, message } from "antd";
import { useParams } from "react-router-dom";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCloseDefect, useDefect, useResolveDefect, useUpdateDefect } from "../hooks/useDefects";

const { Title, Paragraph, Text } = Typography;

export function DefectDetailPage() {
  const { defectId } = useParams<{ defectId: string }>();
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.defects.read);
  const canManage = permissionState.hasPermission(permissions.defects.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const defectQuery = useDefect(defectId ?? null, canRead);
  const updateMutation = useUpdateDefect();
  const resolveMutation = useResolveDefect();
  const closeMutation = useCloseDefect();
  const [updateForm] = Form.useForm<{ title: string; description: string; severity: string; ownerUserId: string; detectedInPhase?: string; correctiveActionRef?: string; status: string }>();

  if (!canRead) {
    return <Alert type="warning" showIcon message="Defect access is not available for this account." />;
  }

  if (!defectQuery.data) {
    return <Alert type="info" showIcon message={defectQuery.isLoading ? "Loading defect..." : "Defect not found."} />;
  }

  const defect = defectQuery.data;

  const handleUpdate = async () => {
    const values = await updateForm.validateFields();
    try {
      await updateMutation.mutateAsync({
        id: defect.id,
        input: { ...values, affectedArtifactRefs: defect.affectedArtifactRefs },
      });
      void messageApi.success("Defect updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update defect");
      void messageApi.error(presentation.description);
    }
  };

  const handleResolve = async () => {
    try {
      await resolveMutation.mutateAsync({ id: defect.id, input: { resolutionSummary: defect.resolutionSummary ?? "Resolved via managed workflow.", correctiveActionRef: defect.correctiveActionRef ?? null } });
      void messageApi.success("Defect resolved.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to resolve defect");
      void messageApi.error(presentation.description);
    }
  };

  const handleClose = async () => {
    try {
      await closeMutation.mutateAsync({ id: defect.id, input: { resolutionSummary: defect.resolutionSummary ?? "" } });
      void messageApi.success("Defect closed.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to close defect");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Title level={3} style={{ margin: 0 }}>{defect.code} · {defect.title}</Title>
        <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
          Defect detail, corrective references, and governed resolve/close actions.
        </Paragraph>
      </Card>
      <Card variant="borderless">
        <Descriptions column={2} size="small">
          <Descriptions.Item label="Project">{defect.projectName}</Descriptions.Item>
          <Descriptions.Item label="Status">{defect.status}</Descriptions.Item>
          <Descriptions.Item label="Severity">{defect.severity}</Descriptions.Item>
          <Descriptions.Item label="Owner">{defect.ownerUserId}</Descriptions.Item>
          <Descriptions.Item label="Phase Found">{defect.detectedInPhase ?? "-"}</Descriptions.Item>
          <Descriptions.Item label="Corrective Action">{defect.correctiveActionRef ?? "-"}</Descriptions.Item>
          <Descriptions.Item label="Resolution" span={2}>{defect.resolutionSummary ?? "-"}</Descriptions.Item>
        </Descriptions>
        <Paragraph style={{ marginTop: 16 }}>{defect.description}</Paragraph>
        <Paragraph type="secondary">Affected artifacts: {defect.affectedArtifactRefs.length > 0 ? defect.affectedArtifactRefs.join(", ") : "none"}</Paragraph>
      </Card>
      <Card variant="borderless">
        <Form form={updateForm} layout="vertical" initialValues={{ title: defect.title, description: defect.description, severity: defect.severity, ownerUserId: defect.ownerUserId, detectedInPhase: defect.detectedInPhase ?? undefined, correctiveActionRef: defect.correctiveActionRef ?? undefined, status: defect.status }}>
          <Form.Item label="Title" name="title" rules={[{ required: true }]}><Input disabled={!canManage} /></Form.Item>
          <Form.Item label="Description" name="description" rules={[{ required: true }]}><Input.TextArea rows={4} disabled={!canManage} /></Form.Item>
          <Form.Item label="Severity" name="severity" rules={[{ required: true }]}><Select disabled={!canManage} options={["low", "medium", "high", "critical"].map((value) => ({ label: value, value }))} /></Form.Item>
          <Form.Item label="Owner User Id" name="ownerUserId" rules={[{ required: true }]}><Input disabled={!canManage} /></Form.Item>
          <Form.Item label="Detected In Phase" name="detectedInPhase"><Input disabled={!canManage} /></Form.Item>
          <Form.Item label="Corrective Action Ref" name="correctiveActionRef"><Input disabled={!canManage} /></Form.Item>
          <Form.Item label="Status" name="status" rules={[{ required: true }]}><Select disabled={!canManage} options={["open", "in_progress", "resolved", "closed"].map((value) => ({ label: value, value }))} /></Form.Item>
        </Form>
        <Space>
          <Button onClick={() => void handleUpdate()} disabled={!canManage} loading={updateMutation.isPending}>Save</Button>
          <Button onClick={() => void handleResolve()} disabled={!canManage || defect.status !== "in_progress"} loading={resolveMutation.isPending}>Resolve</Button>
          <Button type="primary" onClick={() => void handleClose()} disabled={!canManage || defect.status !== "resolved"} loading={closeMutation.isPending}>Close</Button>
        </Space>
        {defect.status === "resolved" && !defect.resolutionSummary ? <Text type="danger">Resolution summary is required before closure.</Text> : null}
      </Card>
    </Space>
  );
}
