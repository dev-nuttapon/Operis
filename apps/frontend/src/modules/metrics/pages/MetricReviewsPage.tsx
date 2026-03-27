import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, InputNumber, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { FormInstance } from "antd";
import type { ColumnsType } from "antd/es/table";
import { FundProjectionScreenOutlined, PlusOutlined } from "@ant-design/icons";
import { useProjectOptions } from "../../users";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateMetricReview, useMetricReviews, useUpdateMetricReview } from "../hooks/useMetrics";
import type { CreateMetricReviewInput, MetricReviewItem, UpdateMetricReviewInput } from "../types/metrics";

const { Title, Paragraph } = Typography;

export function MetricReviewsPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.metrics.read);
  const canManage = permissionState.hasPermission(permissions.metrics.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ projectId: undefined as string | undefined, status: undefined as string | undefined, reviewedBy: undefined as string | undefined, search: "", page: 1, pageSize: 25 });
  const [editing, setEditing] = useState<MetricReviewItem | null>(null);
  const [createOpen, setCreateOpen] = useState(false);
  const [form] = Form.useForm<CreateMetricReviewInput & { status?: string }>();
  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 50 });
  const query = useMetricReviews(filters, canRead);
  const createMutation = useCreateMetricReview();
  const updateMutation = useUpdateMetricReview();

  const columns = useMemo<ColumnsType<MetricReviewItem>>(
    () => [
      { title: "Project", dataIndex: "projectName" },
      { title: "Review Period", dataIndex: "reviewPeriod" },
      { title: "Reviewer", dataIndex: "reviewedBy" },
      { title: "Open Actions", dataIndex: "openActionCount" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Actions", key: "actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button> },
    ],
    [canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Metric review data is not available for this account." />;
  }

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync({
        projectId: values.projectId,
        reviewPeriod: values.reviewPeriod,
        reviewedBy: values.reviewedBy,
        summary: values.summary ?? null,
        openActionCount: values.openActionCount ?? 0,
      });
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Metric review created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create metric review");
      void messageApi.error(presentation.description);
    }
  };

  const submitUpdate = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({
        id: editing.id,
        input: {
          projectId: values.projectId,
          reviewPeriod: values.reviewPeriod,
          reviewedBy: values.reviewedBy,
          summary: values.summary ?? null,
          openActionCount: values.openActionCount ?? 0,
          status: values.status ?? editing.status,
        } as UpdateMetricReviewInput,
      });
      form.resetFields();
      setEditing(null);
      void messageApi.success("Metric review updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update metric review");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0f766e, #0f172a)", color: "#fff" }}>
            <FundProjectionScreenOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Metrics Review Log</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Track project review periods, reviewer accountability, and whether follow-up actions are still open.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Select allowClear showSearch placeholder="Project" style={{ width: 240 }} options={projectOptions.options} value={filters.projectId} onSearch={projectOptions.onSearch} onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["planned", "reviewed", "actions_tracked", "closed"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Input allowClear placeholder="Reviewer" style={{ width: 180 }} value={filters.reviewedBy} onChange={(event) => setFilters((current) => ({ ...current, reviewedBy: event.target.value || undefined, page: 1 }))} />
            <Input.Search allowClear placeholder="Search period or project" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New review</Button>
        </Flex>
        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, defaultPageSize: 25, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create metrics review" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <MetricReviewForm form={form} projectOptions={projectOptions.options} />
      </Modal>

      <Modal title="Edit metrics review" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => { if (open && editing) { form.setFieldsValue({ projectId: editing.projectId, reviewPeriod: editing.reviewPeriod, reviewedBy: editing.reviewedBy, summary: editing.summary ?? undefined, openActionCount: editing.openActionCount, status: editing.status }); } }}>
        <MetricReviewForm form={form} projectOptions={projectOptions.options} includeStatus />
      </Modal>
    </Space>
  );
}

function MetricReviewForm({ form, projectOptions, includeStatus = false }: { form: FormInstance<CreateMetricReviewInput & { status?: string }>; projectOptions: Array<{ label: string; value: string }>; includeStatus?: boolean }) {
  return (
    <Form form={form} layout="vertical">
      <Form.Item label="Project" name="projectId" rules={[{ required: true, message: "Project is required." }]}>
        <Select showSearch options={projectOptions} />
      </Form.Item>
      <Form.Item label="Review Period" name="reviewPeriod" rules={[{ required: true, message: "Review period is required." }]}>
        <Input placeholder="2026-Q1" />
      </Form.Item>
      <Form.Item label="Reviewed By" name="reviewedBy" rules={[{ required: true, message: "Reviewer is required." }]}>
        <Input placeholder="qa-manager@example.com" />
      </Form.Item>
      <Form.Item label="Summary" name="summary">
        <Input.TextArea rows={4} />
      </Form.Item>
      <Form.Item label="Open Action Count" name="openActionCount" initialValue={0}>
        <InputNumber min={0} style={{ width: "100%" }} />
      </Form.Item>
      {includeStatus ? (
        <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}>
          <Select options={["planned", "reviewed", "actions_tracked", "closed"].map((value) => ({ label: value, value }))} />
        </Form.Item>
      ) : null}
    </Form>
  );
}
