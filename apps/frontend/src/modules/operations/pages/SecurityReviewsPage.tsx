import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { FormInstance } from "antd";
import type { ColumnsType } from "antd/es/table";
import { SafetyOutlined, PlusOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateSecurityReview, useSecurityReviews, useUpdateSecurityReview } from "../hooks/useOperations";
import type { CreateSecurityReviewInput, SecurityReview, UpdateSecurityReviewInput } from "../types/operations";

const { Title, Paragraph } = Typography;

export function SecurityReviewsPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.operations.read);
  const canManage = permissionState.hasPermission(permissions.operations.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", scopeType: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 25 });
  const [editing, setEditing] = useState<SecurityReview | null>(null);
  const [createOpen, setCreateOpen] = useState(false);
  const [form] = Form.useForm<CreateSecurityReviewInput>();
  const query = useSecurityReviews({ ...filters, sortBy: "createdAt", sortOrder: "desc" }, canRead);
  const createMutation = useCreateSecurityReview();
  const updateMutation = useUpdateSecurityReview();

  const columns = useMemo<ColumnsType<SecurityReview>>(
    () => [
      { title: "Scope", key: "scope", render: (_, item) => `${item.scopeType}: ${item.scopeRef}` },
      { title: "Controls Reviewed", dataIndex: "controlsReviewed" },
      { title: "Findings", dataIndex: "findingsSummary", render: (value) => value ?? "-" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "Closed" ? "green" : "blue"}>{value}</Tag> },
      { title: "Actions", key: "actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button> },
    ],
    [canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Security review data is not available for this account." />;
  }

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Security review created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create security review");
      void messageApi.error(presentation.description);
    }
  };

  const submitUpdate = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({ id: editing.id, input: values as UpdateSecurityReviewInput });
      form.resetFields();
      setEditing(null);
      void messageApi.success("Security review updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update security review");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #111827, #dc2626)", color: "#fff" }}>
            <SafetyOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Security Review</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Capture scope-based control reviews, findings summaries, and closeout status for internal security checkpoints.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search scope or control" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
            <Select allowClear placeholder="Scope type" style={{ width: 180 }} options={["application", "environment", "project"].map((value) => ({ label: value, value }))} value={filters.scopeType} onChange={(value) => setFilters((current) => ({ ...current, scopeType: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["Planned", "In Review", "Findings Issued", "Closed"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New security review</Button>
        </Flex>

        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, defaultPageSize: 25, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create security review" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <SecurityReviewForm form={form} />
      </Modal>

      <Modal title="Edit security review" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => { if (open && editing) { form.setFieldsValue(editing); } }}>
        <SecurityReviewForm form={form} />
      </Modal>
    </Space>
  );
}

function SecurityReviewForm({ form }: { form: FormInstance<CreateSecurityReviewInput> }) {
  return (
    <Form form={form} layout="vertical" initialValues={{ status: "Planned" }}>
      <Form.Item label="Scope Type" name="scopeType" rules={[{ required: true, message: "Scope type is required." }]}>
        <Select options={["application", "environment", "project"].map((value) => ({ label: value, value }))} />
      </Form.Item>
      <Form.Item label="Scope Reference" name="scopeRef" rules={[{ required: true, message: "Scope reference is required." }]}>
        <Input placeholder="prod-cluster" />
      </Form.Item>
      <Form.Item label="Controls Reviewed" name="controlsReviewed" rules={[{ required: true, message: "Controls reviewed is required." }]}>
        <Input.TextArea rows={4} placeholder="Access control, secret handling, and backup integrity." />
      </Form.Item>
      <Form.Item label="Findings Summary" name="findingsSummary">
        <Input.TextArea rows={4} placeholder="Open findings or observations." />
      </Form.Item>
      <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}>
        <Select options={["Planned", "In Review", "Findings Issued", "Closed"].map((value) => ({ label: value, value }))} />
      </Form.Item>
    </Form>
  );
}
