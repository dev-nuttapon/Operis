import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, InputNumber, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { FormInstance } from "antd";
import type { ColumnsType } from "antd/es/table";
import { CloudServerOutlined, PlusOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCapacityReviews, useCreateCapacityReview, useUpdateCapacityReview } from "../hooks/useMetrics";
import type { CapacityReviewItem, CreateCapacityReviewInput, UpdateCapacityReviewInput } from "../types/metrics";

const { Title, Paragraph } = Typography;
const statusOptions = ["planned", "reviewed", "actioned", "closed"].map((value) => ({ label: value, value }));

export function CapacityReviewPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.metrics.read);
  const canManage = permissionState.hasPermission(permissions.metrics.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", scopeRef: undefined as string | undefined, reviewedBy: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<CapacityReviewItem | null>(null);
  const [form] = Form.useForm<CreateCapacityReviewInput & { status?: string }>();
  const query = useCapacityReviews(filters, canRead);
  const createMutation = useCreateCapacityReview();
  const updateMutation = useUpdateCapacityReview();

  const columns = useMemo<ColumnsType<CapacityReviewItem>>(
    () => [
      { title: "Scope", dataIndex: "scopeRef" },
      { title: "Period", dataIndex: "reviewPeriod" },
      { title: "Reviewed By", dataIndex: "reviewedBy" },
      { title: "Actions", dataIndex: "actionCount" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button> },
    ],
    [canManage],
  );

  if (!canRead) return <Alert type="warning" showIcon message="Capacity reviews are not available for this account." />;

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Capacity review created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create capacity review");
      void messageApi.error(presentation.description);
    }
  };

  const submitUpdate = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({ id: editing.id, input: { ...values, status: values.status ?? editing.status } as UpdateCapacityReviewInput });
      form.resetFields();
      setEditing(null);
      void messageApi.success("Capacity review updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update capacity review");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless"><Space align="start" size={16}><div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0369a1, #155e75)", color: "#fff" }}><CloudServerOutlined /></div><div><Title level={3} style={{ margin: 0 }}>Capacity Review</Title><Paragraph type="secondary" style={{ margin: "4px 0 0" }}>Review scoped capacity posture, action counts, and closure readiness.</Paragraph></div></Space></Card>
      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search scope, period, or reviewer" style={{ width: 260 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
            <Input allowClear placeholder="Scope Ref" style={{ width: 180 }} value={filters.scopeRef} onChange={(event) => setFilters((current) => ({ ...current, scopeRef: event.target.value || undefined, page: 1 }))} />
            <Input allowClear placeholder="Reviewed By" style={{ width: 180 }} value={filters.reviewedBy} onChange={(event) => setFilters((current) => ({ ...current, reviewedBy: event.target.value || undefined, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={statusOptions} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New review</Button>
        </Flex>
        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>
      <Modal title="Create capacity review" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden><CapacityReviewForm form={form} /></Modal>
      <Modal title="Edit capacity review" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => { if (open && editing) { form.setFieldsValue({ scopeRef: editing.scopeRef, reviewPeriod: editing.reviewPeriod, reviewedBy: editing.reviewedBy, summary: editing.summary, actionCount: editing.actionCount, status: editing.status }); } }}><CapacityReviewForm form={form} includeStatus /></Modal>
    </Space>
  );
}

function CapacityReviewForm({ form, includeStatus = false }: { form: FormInstance<CreateCapacityReviewInput & { status?: string }>; includeStatus?: boolean }) {
  return (
    <Form form={form} layout="vertical">
      <Form.Item label="Scope Ref" name="scopeRef" rules={[{ required: true, message: "Scope reference is required." }]}><Input /></Form.Item>
      <Form.Item label="Review Period" name="reviewPeriod" rules={[{ required: true, message: "Review period is required." }]}><Input placeholder="2026-Q2" /></Form.Item>
      <Form.Item label="Reviewed By" name="reviewedBy" rules={[{ required: true, message: "Reviewer is required." }]}><Input /></Form.Item>
      <Form.Item label="Summary" name="summary" rules={[{ required: true, message: "Summary is required." }]}><Input.TextArea rows={4} /></Form.Item>
      <Form.Item label="Action Count" name="actionCount" initialValue={0}><InputNumber min={0} style={{ width: "100%" }} /></Form.Item>
      {includeStatus ? <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}><Select options={statusOptions} /></Form.Item> : null}
    </Form>
  );
}
