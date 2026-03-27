import { useMemo, useState } from "react";
import { Alert, Button, Card, DatePicker, Flex, Form, Input, InputNumber, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { FormInstance } from "antd";
import type { ColumnsType } from "antd/es/table";
import { LineChartOutlined, PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { useProjectOptions } from "../../users";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateTrendReport, useMetricDefinitions, useTrendReports, useUpdateTrendReport } from "../hooks/useMetrics";
import type { CreateTrendReportInput, TrendReportItem, UpdateTrendReportInput } from "../types/metrics";

const { Title, Paragraph } = Typography;

type TrendReportFormValues = Omit<CreateTrendReportInput, "periodFrom" | "periodTo"> & {
  periodFrom?: dayjs.Dayjs;
  periodTo?: dayjs.Dayjs;
};

export function TrendReportsPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.metrics.read);
  const canManage = permissionState.hasPermission(permissions.metrics.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ projectId: undefined as string | undefined, metricDefinitionId: undefined as string | undefined, status: undefined as string | undefined, search: "", page: 1, pageSize: 25 });
  const [editing, setEditing] = useState<TrendReportItem | null>(null);
  const [createOpen, setCreateOpen] = useState(false);
  const [form] = Form.useForm<TrendReportFormValues>();
  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 50 });
  const metricDefinitionsQuery = useMetricDefinitions({ page: 1, pageSize: 100 }, canRead);
  const metricOptions = (metricDefinitionsQuery.data?.items ?? []).map((item) => ({ label: `${item.code} · ${item.name}`, value: item.id }));
  const query = useTrendReports(filters, canRead);
  const createMutation = useCreateTrendReport();
  const updateMutation = useUpdateTrendReport();

  const columns = useMemo<ColumnsType<TrendReportItem>>(
    () => [
      { title: "Project", dataIndex: "projectName" },
      { title: "Metric", render: (_, item) => `${item.metricCode} · ${item.metricName}` },
      { title: "Period", render: (_, item) => `${item.periodFrom} -> ${item.periodTo}` },
      { title: "Direction", dataIndex: "trendDirection", render: (value?: string | null) => value ?? "-" },
      { title: "Variance", dataIndex: "variance", render: (value?: number | null) => value ?? "-" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Actions", key: "actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button> },
    ],
    [canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Trend report data is not available for this account." />;
  }

  const mapValues = (values: TrendReportFormValues): CreateTrendReportInput => ({
    projectId: values.projectId,
    metricDefinitionId: values.metricDefinitionId ?? null,
    periodFrom: values.periodFrom?.format("YYYY-MM-DD") ?? null,
    periodTo: values.periodTo?.format("YYYY-MM-DD") ?? null,
    status: values.status,
    reportRef: values.reportRef ?? null,
    trendDirection: values.trendDirection ?? null,
    variance: values.variance ?? null,
    recommendedAction: values.recommendedAction ?? null,
  });

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(mapValues(values));
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Trend report created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create trend report");
      void messageApi.error(presentation.description);
    }
  };

  const submitUpdate = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({ id: editing.id, input: mapValues(values) as UpdateTrendReportInput });
      form.resetFields();
      setEditing(null);
      void messageApi.success("Trend report updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update trend report");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #1d4ed8, #0f172a)", color: "#fff" }}>
            <LineChartOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Trend Analysis Report</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Review directional trends, variance, and recommended actions for approved metric periods.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Select allowClear showSearch placeholder="Project" style={{ width: 240 }} options={projectOptions.options} value={filters.projectId} onSearch={projectOptions.onSearch} onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))} />
            <Select allowClear showSearch placeholder="Metric" style={{ width: 280 }} options={metricOptions} value={filters.metricDefinitionId} onChange={(value) => setFilters((current) => ({ ...current, metricDefinitionId: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["draft", "approved", "archived"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Input.Search allowClear placeholder="Search project or metric" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New report</Button>
        </Flex>
        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, defaultPageSize: 25, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create trend report" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <TrendReportForm form={form} projectOptions={projectOptions.options} metricOptions={metricOptions} />
      </Modal>

      <Modal title="Edit trend report" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => { if (open && editing) { form.setFieldsValue({ projectId: editing.projectId, metricDefinitionId: editing.metricDefinitionId, periodFrom: editing.periodFrom ? dayjs(editing.periodFrom) : undefined, periodTo: editing.periodTo ? dayjs(editing.periodTo) : undefined, status: editing.status, reportRef: editing.reportRef ?? undefined, trendDirection: editing.trendDirection ?? undefined, variance: editing.variance ?? undefined, recommendedAction: editing.recommendedAction ?? undefined }); } }}>
        <TrendReportForm form={form} projectOptions={projectOptions.options} metricOptions={metricOptions} />
      </Modal>
    </Space>
  );
}

function TrendReportForm({ form, projectOptions, metricOptions }: { form: FormInstance<TrendReportFormValues>; projectOptions: Array<{ label: string; value: string }>; metricOptions: Array<{ label: string; value: string }>; }) {
  return (
    <Form form={form} layout="vertical" initialValues={{ status: "draft" }}>
      <Form.Item label="Project" name="projectId" rules={[{ required: true, message: "Project is required." }]}>
        <Select showSearch options={projectOptions} />
      </Form.Item>
      <Form.Item label="Metric" name="metricDefinitionId" rules={[{ required: true, message: "Metric is required." }]}>
        <Select showSearch options={metricOptions} />
      </Form.Item>
      <Form.Item label="Period From" name="periodFrom" rules={[{ required: true, message: "Period start is required." }]}>
        <DatePicker style={{ width: "100%" }} />
      </Form.Item>
      <Form.Item label="Period To" name="periodTo" rules={[{ required: true, message: "Period end is required." }]}>
        <DatePicker style={{ width: "100%" }} />
      </Form.Item>
      <Form.Item label="Trend Direction" name="trendDirection">
        <Select allowClear options={["up", "down", "flat", "volatile"].map((value) => ({ label: value, value }))} />
      </Form.Item>
      <Form.Item label="Variance" name="variance">
        <InputNumber style={{ width: "100%" }} />
      </Form.Item>
      <Form.Item label="Recommended Action" name="recommendedAction">
        <Input.TextArea rows={4} />
      </Form.Item>
      <Form.Item label="Report Ref" name="reportRef">
        <Input placeholder="minio://metrics/trend-report-q1.pdf" />
      </Form.Item>
      <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}>
        <Select options={["draft", "approved", "archived"].map((value) => ({ label: value, value }))} />
      </Form.Item>
    </Form>
  );
}
