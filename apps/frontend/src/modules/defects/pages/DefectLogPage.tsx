import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useNavigate } from "react-router-dom";
import { useProjectOptions } from "../../users";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateDefect, useDefects } from "../hooks/useDefects";
import type { DefectFormInput, DefectListItem } from "../types/defects";

const { Title, Paragraph } = Typography;

export function DefectLogPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.defects.read);
  const canManage = permissionState.hasPermission(permissions.defects.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const navigate = useNavigate();
  const [filters, setFilters] = useState({ projectId: undefined as string | undefined, severity: undefined as string | undefined, status: undefined as string | undefined, ownerUserId: undefined as string | undefined, search: undefined as string | undefined, page: 1, pageSize: 10 });
  const [createOpen, setCreateOpen] = useState(false);
  const [form] = Form.useForm<DefectFormInput & { affectedArtifacts?: string }>();

  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 50 });
  const defectsQuery = useDefects(filters, canRead);
  const createMutation = useCreateDefect();

  const columns = useMemo<ColumnsType<DefectListItem>>(
    () => [
      { title: "Code", dataIndex: "code", key: "code" },
      { title: "Title", dataIndex: "title", key: "title" },
      { title: "Severity", dataIndex: "severity", key: "severity", render: (value: string) => <Tag color={value === "critical" ? "red" : value === "high" ? "volcano" : "gold"}>{value}</Tag> },
      { title: "Owner", dataIndex: "ownerUserId", key: "ownerUserId" },
      { title: "Status", dataIndex: "status", key: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Phase Found", dataIndex: "detectedInPhase", key: "detectedInPhase", render: (value?: string | null) => value ?? "-" },
      { title: "Actions", key: "actions", render: (_, item) => <Button size="small" onClick={() => navigate(`/app/defects/${item.id}`)}>Detail</Button> },
    ],
    [navigate],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Defect access is not available for this account." />;
  }

  const handleCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync({
        projectId: values.projectId,
        code: values.code,
        title: values.title,
        description: values.description,
        severity: values.severity,
        ownerUserId: values.ownerUserId,
        detectedInPhase: values.detectedInPhase ?? null,
        correctiveActionRef: values.correctiveActionRef ?? null,
        affectedArtifactRefs: values.affectedArtifacts ? values.affectedArtifacts.split(",").map((item) => item.trim()).filter(Boolean) : [],
      });
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Defect created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create defect");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Title level={3} style={{ margin: 0 }}>Defect Log</Title>
        <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
          Track product and process defects with owner, severity, affected artifacts, and governed closure controls.
        </Paragraph>
      </Card>
      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Select allowClear showSearch placeholder="Project" style={{ width: 240 }} options={projectOptions.options} value={filters.projectId} onSearch={projectOptions.onSearch} onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))} />
            <Select allowClear placeholder="Severity" style={{ width: 160 }} options={["low", "medium", "high", "critical"].map((value) => ({ label: value, value }))} value={filters.severity} onChange={(value) => setFilters((current) => ({ ...current, severity: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 160 }} options={["open", "in_progress", "resolved", "closed"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Input allowClear placeholder="Owner user id" style={{ width: 180 }} value={filters.ownerUserId} onChange={(event) => setFilters((current) => ({ ...current, ownerUserId: event.target.value || undefined, page: 1 }))} />
            <Input allowClear placeholder="Search code or title" style={{ width: 220 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value || undefined, page: 1 }))} />
          </Flex>
          <Button type="primary" disabled={!canManage} onClick={() => setCreateOpen(true)}>Create defect</Button>
        </Flex>
        <Table rowKey="id" loading={defectsQuery.isLoading} columns={columns} dataSource={defectsQuery.data?.items ?? []} pagination={{ current: defectsQuery.data?.page ?? filters.page, pageSize: defectsQuery.data?.pageSize ?? filters.pageSize, total: defectsQuery.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>
      <Modal title="Create defect" open={createOpen} onOk={() => void handleCreate()} onCancel={() => setCreateOpen(false)} confirmLoading={createMutation.isPending} destroyOnHidden>
        <Form form={form} layout="vertical">
          <Form.Item label="Project" name="projectId" rules={[{ required: true }]}><Select showSearch options={projectOptions.options} onSearch={projectOptions.onSearch} /></Form.Item>
          <Form.Item label="Code" name="code" rules={[{ required: true }]}><Input placeholder="DEF-2026-001" /></Form.Item>
          <Form.Item label="Title" name="title" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item label="Description" name="description" rules={[{ required: true }]}><Input.TextArea rows={4} /></Form.Item>
          <Form.Item label="Severity" name="severity" rules={[{ required: true }]}><Select options={["low", "medium", "high", "critical"].map((value) => ({ label: value, value }))} /></Form.Item>
          <Form.Item label="Owner User Id" name="ownerUserId" rules={[{ required: true }]}><Input placeholder="qa@example.com" /></Form.Item>
          <Form.Item label="Detected In Phase" name="detectedInPhase"><Input placeholder="uat" /></Form.Item>
          <Form.Item label="Corrective Action Ref" name="correctiveActionRef"><Input placeholder="CAPA-001" /></Form.Item>
          <Form.Item label="Affected Artifacts" name="affectedArtifacts"><Input placeholder="REQ-001,DOC-002,REL-003" /></Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
