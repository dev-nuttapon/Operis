import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, InputNumber, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { FormInstance } from "antd";
import type { ColumnsType } from "antd/es/table";
import { ThunderboltOutlined, PlusOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateSlowOperationReview, useSlowOperationReviews, useUpdateSlowOperationReview } from "../hooks/useMetrics";
import type { CreateSlowOperationReviewInput, SlowOperationReviewItem, UpdateSlowOperationReviewInput } from "../types/metrics";

const { Title, Paragraph } = Typography;
const operationTypeOptions = ["query", "api", "job", "report"].map((value) => ({ label: value, value }));
const statusOptions = ["open", "investigating", "optimized", "verified", "closed"].map((value) => ({ label: value, value }));

export function SlowOperationReviewPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.metrics.read);
  const canManage = permissionState.hasPermission(permissions.metrics.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", operationType: undefined as string | undefined, ownerUserId: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 50 });
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<SlowOperationReviewItem | null>(null);
  const [form] = Form.useForm<CreateSlowOperationReviewInput>();
  const query = useSlowOperationReviews(filters, canRead);
  const createMutation = useCreateSlowOperationReview();
  const updateMutation = useUpdateSlowOperationReview();

  const columns = useMemo<ColumnsType<SlowOperationReviewItem>>(
    () => [
      { title: "Type", dataIndex: "operationType" },
      { title: "Operation Key", dataIndex: "operationKey" },
      { title: "Latency (ms)", dataIndex: "observedLatencyMs" },
      { title: "Frequency/hr", dataIndex: "frequencyPerHour", render: (value: number | null | undefined) => value ?? "-" },
      { title: "Owner", dataIndex: "ownerUserId" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button> },
    ],
    [canManage],
  );

  if (!canRead) return <Alert type="warning" showIcon message="Slow operation reviews are not available for this account." />;

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Slow operation review created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create slow operation review");
      void messageApi.error(presentation.description);
    }
  };

  const submitUpdate = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({ id: editing.id, input: values as UpdateSlowOperationReviewInput });
      form.resetFields();
      setEditing(null);
      void messageApi.success("Slow operation review updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update slow operation review");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless"><Space align="start" size={16}><div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #b45309, #dc2626)", color: "#fff" }}><ThunderboltOutlined /></div><div><Title level={3} style={{ margin: 0 }}>Slow Query / API Review</Title><Paragraph type="secondary" style={{ margin: "4px 0 0" }}>Track slow operational findings through investigation, optimization, verification, and closure.</Paragraph></div></Space></Card>
      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search operation key or owner" style={{ width: 260 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
            <Select allowClear placeholder="Operation Type" style={{ width: 180 }} options={operationTypeOptions} value={filters.operationType} onChange={(value) => setFilters((current) => ({ ...current, operationType: value, page: 1 }))} />
            <Input allowClear placeholder="Owner User" style={{ width: 180 }} value={filters.ownerUserId} onChange={(event) => setFilters((current) => ({ ...current, ownerUserId: event.target.value || undefined, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={statusOptions} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New review</Button>
        </Flex>
        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, defaultPageSize: 50, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>
      <Modal title="Create slow operation review" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden><SlowOperationForm form={form} /></Modal>
      <Modal title="Edit slow operation review" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => { if (open && editing) { form.setFieldsValue({ operationType: editing.operationType, operationKey: editing.operationKey, observedLatencyMs: editing.observedLatencyMs, frequencyPerHour: editing.frequencyPerHour ?? undefined, ownerUserId: editing.ownerUserId, optimizationSummary: editing.optimizationSummary ?? undefined, status: editing.status }); } }}><SlowOperationForm form={form} includeStatus /></Modal>
    </Space>
  );
}

function SlowOperationForm({ form, includeStatus = false }: { form: FormInstance<CreateSlowOperationReviewInput>; includeStatus?: boolean }) {
  return (
    <Form form={form} layout="vertical" initialValues={{ operationType: "query", status: "open" }}>
      <Form.Item label="Operation Type" name="operationType" rules={[{ required: true, message: "Operation type is required." }]}><Select options={operationTypeOptions} /></Form.Item>
      <Form.Item label="Operation Key" name="operationKey" rules={[{ required: true, message: "Operation key is required." }]}><Input placeholder="GET:/api/v1/documents" /></Form.Item>
      <Form.Item label="Observed Latency (ms)" name="observedLatencyMs" rules={[{ required: true, message: "Observed latency is required." }]}><InputNumber min={0} style={{ width: "100%" }} /></Form.Item>
      <Form.Item label="Frequency per Hour" name="frequencyPerHour"><InputNumber min={0} style={{ width: "100%" }} /></Form.Item>
      <Form.Item label="Owner User" name="ownerUserId" rules={[{ required: true, message: "Owner user is required." }]}><Input /></Form.Item>
      <Form.Item label="Optimization Summary" name="optimizationSummary"><Input.TextArea rows={4} /></Form.Item>
      {includeStatus ? <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}><Select options={statusOptions} /></Form.Item> : null}
    </Form>
  );
}
