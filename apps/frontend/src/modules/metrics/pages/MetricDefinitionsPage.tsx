import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, InputNumber, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { LineChartOutlined, PlusOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateMetricCollectionSchedule, useCreateMetricDefinition, useMetricCollectionSchedules, useMetricDefinitions, useUpdateMetricDefinition } from "../hooks/useMetrics";
import type { CreateMetricCollectionScheduleInput, CreateMetricDefinitionInput, MetricCollectionScheduleItem, MetricDefinitionItem, UpdateMetricDefinitionInput } from "../types/metrics";

const { Title, Paragraph, Text } = Typography;

export function MetricDefinitionsPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.metrics.read);
  const canManage = permissionState.hasPermission(permissions.metrics.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", metricType: undefined as string | undefined, status: undefined as string | undefined, ownerUserId: undefined as string | undefined, page: 1, pageSize: 10 });
  const [selectedMetric, setSelectedMetric] = useState<MetricDefinitionItem | null>(null);
  const [createOpen, setCreateOpen] = useState(false);
  const [scheduleOpen, setScheduleOpen] = useState(false);
  const [metricForm] = Form.useForm<CreateMetricDefinitionInput>();
  const [scheduleForm] = Form.useForm<CreateMetricCollectionScheduleInput>();
  const definitionsQuery = useMetricDefinitions(filters, canRead);
  const schedulesQuery = useMetricCollectionSchedules({ page: 1, pageSize: 20 }, canRead);
  const createMutation = useCreateMetricDefinition();
  const updateMutation = useUpdateMetricDefinition();
  const createScheduleMutation = useCreateMetricCollectionSchedule();

  const columns = useMemo<ColumnsType<MetricDefinitionItem>>(
    () => [
      { title: "Code", dataIndex: "code", key: "code" },
      { title: "Name", dataIndex: "name", key: "name" },
      { title: "Type", dataIndex: "metricType", key: "metricType", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Owner", dataIndex: "ownerUserId", key: "ownerUserId" },
      { title: "Target", dataIndex: "targetValue", key: "targetValue" },
      { title: "Threshold", dataIndex: "thresholdValue", key: "thresholdValue" },
      { title: "Status", dataIndex: "status", key: "status", render: (value: string) => <Tag color={value === "active" ? "green" : "blue"}>{value}</Tag> },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => (
          <Flex gap={8} wrap>
            <Button size="small" disabled={!canManage} onClick={() => setSelectedMetric(item)}>Edit</Button>
            <Button size="small" disabled={!canManage} onClick={() => { scheduleForm.setFieldValue("metricDefinitionId", item.id); setScheduleOpen(true); }}>Schedule</Button>
          </Flex>
        ),
      },
    ],
    [canManage, scheduleForm],
  );

  const scheduleColumns = useMemo<ColumnsType<MetricCollectionScheduleItem>>(
    () => [
      { title: "Metric", key: "metric", render: (_, item) => <Space direction="vertical" size={0}><Text strong>{item.metricCode}</Text><Text type="secondary">{item.metricName}</Text></Space> },
      { title: "Frequency", dataIndex: "collectionFrequency", key: "collectionFrequency" },
      { title: "Collector", dataIndex: "collectorType", key: "collectorType" },
      { title: "Next Run", dataIndex: "nextRunAt", key: "nextRunAt", render: (value: string) => new Date(value).toLocaleString() },
      { title: "Status", dataIndex: "status", key: "status", render: (value: string) => <Tag>{value}</Tag> },
    ],
    [],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Metrics access is not available for this account." />;
  }

  const handleCreate = async () => {
    const values = await metricForm.validateFields();
    try {
      await createMutation.mutateAsync(values);
      metricForm.resetFields();
      setCreateOpen(false);
      void messageApi.success("Metric definition created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create metric definition");
      void messageApi.error(presentation.description);
    }
  };

  const handleUpdate = async () => {
    if (!selectedMetric) return;
    const values = await metricForm.validateFields();
    try {
      await updateMutation.mutateAsync({ id: selectedMetric.id, input: { ...values, status: selectedMetric.status } as UpdateMetricDefinitionInput });
      setSelectedMetric(null);
      metricForm.resetFields();
      void messageApi.success("Metric definition updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update metric definition");
      void messageApi.error(presentation.description);
    }
  };

  const handleScheduleCreate = async () => {
    const values = await scheduleForm.validateFields();
    try {
      await createScheduleMutation.mutateAsync(values);
      scheduleForm.resetFields();
      setScheduleOpen(false);
      void messageApi.success("Collection schedule created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create schedule");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0f766e, #0f172a)", color: "#fff" }}>
            <LineChartOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Metric Definitions</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Define measurable quality and performance indicators, owners, thresholds, and collection schedules.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search metric" style={{ width: 220 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
            <Select allowClear placeholder="Type" style={{ width: 180 }} options={["quality", "performance", "delivery"].map((value) => ({ label: value, value }))} value={filters.metricType} onChange={(value) => setFilters((current) => ({ ...current, metricType: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["draft", "approved", "active", "deprecated"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New metric</Button>
        </Flex>

        <Table rowKey="id" loading={definitionsQuery.isLoading} columns={columns} dataSource={definitionsQuery.data?.items ?? []} pagination={{ current: definitionsQuery.data?.page ?? filters.page, pageSize: definitionsQuery.data?.pageSize ?? filters.pageSize, total: definitionsQuery.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Card variant="borderless" title="Collection Schedules">
        <Table rowKey="id" loading={schedulesQuery.isLoading} columns={scheduleColumns} dataSource={schedulesQuery.data?.items ?? []} pagination={false} />
      </Card>

      <Modal title="Create metric definition" open={createOpen} onOk={() => void handleCreate()} onCancel={() => setCreateOpen(false)} confirmLoading={createMutation.isPending} destroyOnHidden>
        <MetricDefinitionForm form={metricForm} />
      </Modal>

      <Modal title="Edit metric definition" open={Boolean(selectedMetric)} onOk={() => void handleUpdate()} onCancel={() => { setSelectedMetric(null); metricForm.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => { if (open && selectedMetric) { metricForm.setFieldsValue(selectedMetric); } }}>
        <MetricDefinitionForm form={metricForm} />
      </Modal>

      <Modal title="Create collection schedule" open={scheduleOpen} onOk={() => void handleScheduleCreate()} onCancel={() => setScheduleOpen(false)} confirmLoading={createScheduleMutation.isPending} destroyOnHidden>
        <Form form={scheduleForm} layout="vertical">
          <Form.Item label="Metric Definition" name="metricDefinitionId" rules={[{ required: true, message: "Metric is required." }]}>
            <Select options={(definitionsQuery.data?.items ?? []).map((item) => ({ label: `${item.code} • ${item.name}`, value: item.id }))} />
          </Form.Item>
          <Form.Item label="Frequency" name="collectionFrequency" rules={[{ required: true, message: "Frequency is required." }]}>
            <Select options={["hourly", "daily", "weekly"].map((value) => ({ label: value, value }))} />
          </Form.Item>
          <Form.Item label="Collector Type" name="collectorType" rules={[{ required: true, message: "Collector is required." }]}>
            <Select options={["manual", "prometheus", "system"].map((value) => ({ label: value, value }))} />
          </Form.Item>
          <Form.Item label="Status" name="status" initialValue="draft">
            <Select options={["draft", "active", "archived"].map((value) => ({ label: value, value }))} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}

function MetricDefinitionForm({ form }: { form: ReturnType<typeof Form.useForm<CreateMetricDefinitionInput>>[0] }) {
  return (
    <Form form={form} layout="vertical">
      <Form.Item label="Code" name="code" rules={[{ required: true, message: "Code is required." }]}><Input placeholder="DEFECT_DENSITY" /></Form.Item>
      <Form.Item label="Name" name="name" rules={[{ required: true, message: "Name is required." }]}><Input /></Form.Item>
      <Form.Item label="Type" name="metricType" rules={[{ required: true, message: "Type is required." }]}><Select options={["quality", "performance", "delivery"].map((value) => ({ label: value, value }))} /></Form.Item>
      <Form.Item label="Owner" name="ownerUserId" rules={[{ required: true, message: "Owner is required." }]}><Input placeholder="qa@example.com" /></Form.Item>
      <Form.Item label="Target Value" name="targetValue" rules={[{ required: true, message: "Target is required." }]}><InputNumber style={{ width: "100%" }} /></Form.Item>
      <Form.Item label="Threshold Value" name="thresholdValue" rules={[{ required: true, message: "Threshold is required." }]}><InputNumber style={{ width: "100%" }} /></Form.Item>
    </Form>
  );
}
