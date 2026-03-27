import { useState } from "react";
import { Alert, App, Button, Card, Flex, Form, Select, Space, Table, Typography } from "antd";
import { CalendarOutlined, PlusOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateMetricCollectionSchedule, useMetricCollectionSchedules, useMetricDefinitions } from "../hooks/useMetrics";
import type { CreateMetricCollectionScheduleInput } from "../types/metrics";

export function DataCollectionSchedulePage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.metrics.read);
  const canManage = permissionState.hasPermission(permissions.metrics.manage);
  const { notification } = App.useApp();
  const [filters, setFilters] = useState({ metricDefinitionId: undefined as string | undefined, status: undefined as string | undefined, collectorType: undefined as string | undefined, page: 1, pageSize: 25 });
  const [form] = Form.useForm<CreateMetricCollectionScheduleInput>();
  const schedulesQuery = useMetricCollectionSchedules(filters, canRead);
  const definitionsQuery = useMetricDefinitions({ page: 1, pageSize: 100 }, canRead);
  const createMutation = useCreateMetricCollectionSchedule();

  if (!canRead) {
    return <Alert type="warning" showIcon message="Metric schedules are not available for this account." />;
  }

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      form.resetFields();
      notification.success({ message: "Collection schedule created." });
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create schedule");
      notification.error({ message: presentation.title, description: presentation.description });
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0f766e, #0f172a)", color: "#fff" }}>
            <CalendarOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>Data Collection Schedule</Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Manage recurring collection schedules for governed metrics and verify who or what will collect them.
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless" title="Schedule Register">
        <Flex gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Select allowClear placeholder="Metric" style={{ width: 280 }} options={(definitionsQuery.data?.items ?? []).map((item) => ({ label: `${item.code} • ${item.name}`, value: item.id }))} value={filters.metricDefinitionId} onChange={(value) => setFilters((current) => ({ ...current, metricDefinitionId: value, page: 1 }))} />
          <Select allowClear placeholder="Collector Type" style={{ width: 180 }} options={["manual", "prometheus", "system"].map((value) => ({ label: value, value }))} value={filters.collectorType} onChange={(value) => setFilters((current) => ({ ...current, collectorType: value, page: 1 }))} />
          <Select allowClear placeholder="Status" style={{ width: 180 }} options={["draft", "active", "archived"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
        </Flex>
        <Table rowKey="id" loading={schedulesQuery.isLoading} dataSource={schedulesQuery.data?.items ?? []} pagination={{ current: schedulesQuery.data?.page ?? filters.page, pageSize: schedulesQuery.data?.pageSize ?? filters.pageSize, total: schedulesQuery.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} columns={[
          { title: "Metric", key: "metric", render: (_, item) => `${item.metricCode} • ${item.metricName}` },
          { title: "Frequency", dataIndex: "collectionFrequency" },
          { title: "Collector", dataIndex: "collectorType" },
          { title: "Next Run", dataIndex: "nextRunAt", render: (value: string) => new Date(value).toLocaleString() },
          { title: "Status", dataIndex: "status" },
        ]} />
      </Card>

      <Card variant="borderless" title="Create Schedule" extra={<PlusOutlined />}>
        <Form form={form} layout="vertical">
          <Form.Item label="Metric Definition" name="metricDefinitionId" rules={[{ required: true, message: "Metric is required." }]}><Select options={(definitionsQuery.data?.items ?? []).map((item) => ({ label: `${item.code} • ${item.name}`, value: item.id }))} /></Form.Item>
          <Form.Item label="Collection Frequency" name="collectionFrequency" rules={[{ required: true, message: "Frequency is required." }]}><Select options={["hourly", "daily", "weekly"].map((value) => ({ label: value, value }))} /></Form.Item>
          <Form.Item label="Collector Type" name="collectorType" rules={[{ required: true, message: "Collector type is required." }]}><Select options={["manual", "prometheus", "system"].map((value) => ({ label: value, value }))} /></Form.Item>
          <Form.Item label="Status" name="status" initialValue="draft"><Select options={["draft", "active", "archived"].map((value) => ({ label: value, value }))} /></Form.Item>
          <Button type="primary" onClick={() => void submitCreate()} disabled={!canManage} loading={createMutation.isPending}>Create Schedule</Button>
        </Form>
      </Card>
    </Space>
  );
}
