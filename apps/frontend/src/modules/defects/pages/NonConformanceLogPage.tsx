import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useNavigate } from "react-router-dom";
import { useProjectOptions } from "../../users";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateNonConformance, useNonConformances } from "../hooks/useDefects";
import type { NonConformanceFormInput, NonConformanceListItem } from "../types/defects";

const { Title, Paragraph } = Typography;

export function NonConformanceLogPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.defects.read);
  const canManage = permissionState.hasPermission(permissions.defects.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const navigate = useNavigate();
  const [filters, setFilters] = useState({ projectId: undefined as string | undefined, status: undefined as string | undefined, ownerUserId: undefined as string | undefined, search: undefined as string | undefined, page: 1, pageSize: 10 });
  const [createOpen, setCreateOpen] = useState(false);
  const [form] = Form.useForm<NonConformanceFormInput & { linkedFindings?: string }>();

  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 50 });
  const itemsQuery = useNonConformances(filters, canRead);
  const createMutation = useCreateNonConformance();

  const columns = useMemo<ColumnsType<NonConformanceListItem>>(
    () => [
      { title: "Code", dataIndex: "code", key: "code" },
      { title: "Title", dataIndex: "title", key: "title" },
      { title: "Source", dataIndex: "sourceType", key: "sourceType" },
      { title: "Owner", dataIndex: "ownerUserId", key: "ownerUserId" },
      { title: "Status", dataIndex: "status", key: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Corrective Action", dataIndex: "correctiveActionRef", key: "correctiveActionRef", render: (value?: string | null) => value ?? "-" },
      { title: "Actions", key: "actions", render: (_, item) => <Button size="small" onClick={() => navigate(`/app/non-conformances/${item.id}`)}>Detail</Button> },
    ],
    [navigate],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Non-conformance access is not available for this account." />;
  }

  const handleCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync({
        projectId: values.projectId,
        code: values.code,
        title: values.title,
        description: values.description,
        sourceType: values.sourceType,
        ownerUserId: values.ownerUserId,
        correctiveActionRef: values.correctiveActionRef ?? null,
        rootCause: values.rootCause ?? null,
        linkedFindingRefs: values.linkedFindings ? values.linkedFindings.split(",").map((item) => item.trim()).filter(Boolean) : [],
      });
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Non-conformance created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create non-conformance");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Title level={3} style={{ margin: 0 }}>Non-Conformance Log</Title>
        <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
          Capture process and compliance deviations with root cause, corrective actions, and accepted disposition controls.
        </Paragraph>
      </Card>
      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Select allowClear showSearch placeholder="Project" style={{ width: 240 }} options={projectOptions.options} value={filters.projectId} onSearch={projectOptions.onSearch} onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["open", "in_review", "corrective_action", "closed"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Input allowClear placeholder="Owner user id" style={{ width: 180 }} value={filters.ownerUserId} onChange={(event) => setFilters((current) => ({ ...current, ownerUserId: event.target.value || undefined, page: 1 }))} />
            <Input allowClear placeholder="Search code or title" style={{ width: 220 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value || undefined, page: 1 }))} />
          </Flex>
          <Button type="primary" disabled={!canManage} onClick={() => setCreateOpen(true)}>Create non-conformance</Button>
        </Flex>
        <Table rowKey="id" loading={itemsQuery.isLoading} columns={columns} dataSource={itemsQuery.data?.items ?? []} pagination={{ current: itemsQuery.data?.page ?? filters.page, pageSize: itemsQuery.data?.pageSize ?? filters.pageSize, total: itemsQuery.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>
      <Modal title="Create non-conformance" open={createOpen} onOk={() => void handleCreate()} onCancel={() => setCreateOpen(false)} confirmLoading={createMutation.isPending} destroyOnHidden>
        <Form form={form} layout="vertical">
          <Form.Item label="Project" name="projectId" rules={[{ required: true }]}><Select showSearch options={projectOptions.options} onSearch={projectOptions.onSearch} /></Form.Item>
          <Form.Item label="Code" name="code" rules={[{ required: true }]}><Input placeholder="NC-2026-001" /></Form.Item>
          <Form.Item label="Title" name="title" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item label="Description" name="description" rules={[{ required: true }]}><Input.TextArea rows={4} /></Form.Item>
          <Form.Item label="Source Type" name="sourceType" rules={[{ required: true }]}><Input placeholder="audit" /></Form.Item>
          <Form.Item label="Owner User Id" name="ownerUserId" rules={[{ required: true }]}><Input placeholder="qa@example.com" /></Form.Item>
          <Form.Item label="Corrective Action Ref" name="correctiveActionRef"><Input placeholder="CAPA-001" /></Form.Item>
          <Form.Item label="Root Cause" name="rootCause"><Input.TextArea rows={3} /></Form.Item>
          <Form.Item label="Linked Findings" name="linkedFindings"><Input placeholder="AUD-001,AUD-002" /></Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
