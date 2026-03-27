import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, InputNumber, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { FormInstance } from "antd";
import type { ColumnsType } from "antd/es/table";
import { DashboardOutlined, PlusOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreatePerformanceBaseline, usePerformanceBaselines, useUpdatePerformanceBaseline } from "../hooks/useMetrics";
import type { CreatePerformanceBaselineInput, PerformanceBaselineItem, UpdatePerformanceBaselineInput } from "../types/metrics";

const { Title, Paragraph } = Typography;
const scopeTypeOptions = ["project", "module", "service", "platform"].map((value) => ({ label: value, value }));
const statusOptions = ["draft", "approved", "active", "superseded"].map((value) => ({ label: value, value }));

export function PerformanceBaselinePage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.metrics.read);
  const canManage = permissionState.hasPermission(permissions.metrics.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", scopeType: undefined as string | undefined, metricName: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<PerformanceBaselineItem | null>(null);
  const [form] = Form.useForm<CreatePerformanceBaselineInput & { status?: string }>();
  const query = usePerformanceBaselines(filters, canRead);
  const createMutation = useCreatePerformanceBaseline();
  const updateMutation = useUpdatePerformanceBaseline();

  const columns = useMemo<ColumnsType<PerformanceBaselineItem>>(
    () => [
      { title: "Scope", render: (_, item) => `${item.scopeType}: ${item.scopeRef}` },
      { title: "Metric", dataIndex: "metricName" },
      { title: "Target", dataIndex: "targetValue" },
      { title: "Threshold", dataIndex: "thresholdValue" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button> },
    ],
    [canManage],
  );

  if (!canRead) return <Alert type="warning" showIcon message="Performance baselines are not available for this account." />;

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Performance baseline created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create performance baseline");
      void messageApi.error(presentation.description);
    }
  };

  const submitUpdate = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({ id: editing.id, input: { ...values, status: values.status ?? editing.status } as UpdatePerformanceBaselineInput });
      form.resetFields();
      setEditing(null);
      void messageApi.success("Performance baseline updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update performance baseline");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless"><Space align="start" size={16}><div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0f766e, #1d4ed8)", color: "#fff" }}><DashboardOutlined /></div><div><Title level={3} style={{ margin: 0 }}>Performance Baseline</Title><Paragraph type="secondary" style={{ margin: "4px 0 0" }}>Define governed target and threshold baselines for measured system performance.</Paragraph></div></Space></Card>
      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search scope or metric" style={{ width: 260 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
            <Select allowClear placeholder="Scope Type" style={{ width: 180 }} options={scopeTypeOptions} value={filters.scopeType} onChange={(value) => setFilters((current) => ({ ...current, scopeType: value, page: 1 }))} />
            <Input allowClear placeholder="Metric Name" style={{ width: 180 }} value={filters.metricName} onChange={(event) => setFilters((current) => ({ ...current, metricName: event.target.value || undefined, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={statusOptions} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New baseline</Button>
        </Flex>
        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>
      <Modal title="Create performance baseline" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden><PerformanceBaselineForm form={form} /></Modal>
      <Modal title="Edit performance baseline" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => { if (open && editing) { form.setFieldsValue({ scopeType: editing.scopeType, scopeRef: editing.scopeRef, metricName: editing.metricName, targetValue: editing.targetValue, thresholdValue: editing.thresholdValue, status: editing.status }); } }}><PerformanceBaselineForm form={form} includeStatus /></Modal>
    </Space>
  );
}

function PerformanceBaselineForm({ form, includeStatus = false }: { form: FormInstance<CreatePerformanceBaselineInput & { status?: string }>; includeStatus?: boolean }) {
  return (
    <Form form={form} layout="vertical">
      <Form.Item label="Scope Type" name="scopeType" rules={[{ required: true, message: "Scope type is required." }]}><Select options={scopeTypeOptions} /></Form.Item>
      <Form.Item label="Scope Ref" name="scopeRef" rules={[{ required: true, message: "Scope reference is required." }]}><Input /></Form.Item>
      <Form.Item label="Metric Name" name="metricName" rules={[{ required: true, message: "Metric name is required." }]}><Input /></Form.Item>
      <Form.Item label="Target Value" name="targetValue" rules={[{ required: true, message: "Target value is required." }]}><InputNumber style={{ width: "100%" }} /></Form.Item>
      <Form.Item label="Threshold Value" name="thresholdValue" rules={[{ required: true, message: "Threshold value is required." }]}><InputNumber style={{ width: "100%" }} /></Form.Item>
      {includeStatus ? <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}><Select options={statusOptions} /></Form.Item> : null}
    </Form>
  );
}
