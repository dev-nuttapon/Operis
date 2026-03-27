import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { FormInstance } from "antd";
import { ApiOutlined, PlusOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateIntegrationReview, useIntegrationReviews, useUpdateIntegrationReview } from "../hooks/useGovernance";
import type { IntegrationReview, IntegrationReviewFormInput } from "../types/governance";

const { Title, Paragraph, Text } = Typography;

const integrationStatuses = ["draft", "in_review", "approved", "rejected", "applied"];
const integrationTypes = ["api", "event", "data_pipeline", "third_party", "identity"];

export function IntegrationReviewPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.governance.integrationReviewRead, permissions.governance.integrationReviewManage);
  const canManage = permissionState.hasPermission(permissions.governance.integrationReviewManage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", status: undefined as string | undefined, integrationType: undefined as string | undefined, reviewedBy: undefined as string | undefined, page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<IntegrationReview | null>(null);
  const [form] = Form.useForm<IntegrationReviewFormInput>();
  const query = useIntegrationReviews(filters, canRead);
  const createMutation = useCreateIntegrationReview();
  const updateMutation = useUpdateIntegrationReview();

  const columns = useMemo<ColumnsType<IntegrationReview>>(
    () => [
      { title: "Scope", dataIndex: "scopeRef" },
      { title: "Integration Type", dataIndex: "integrationType" },
      { title: "Reviewed By", dataIndex: "reviewedBy", render: (value) => value ?? "-" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Decision Reason", dataIndex: "decisionReason", render: (value) => value ?? "-" },
      { title: "Actions", key: "actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button> },
    ],
    [canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Integration review data is not available for this account." />;
  }

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      setCreateOpen(false);
      form.resetFields();
      void messageApi.success("Integration review created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create integration review");
      void messageApi.error(presentation.description);
    }
  };

  const submitUpdate = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({ id: editing.id, input: values });
      setEditing(null);
      form.resetFields();
      void messageApi.success("Integration review updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update integration review");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #7c2d12, #1d4ed8)", color: "#fff" }}>
            <ApiOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Integration Review</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Review dependency impact, integration risks, and evidence before approved integrations are applied to delivery flows.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search scope or impact" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
            <Select allowClear placeholder="Integration type" style={{ width: 200 }} options={integrationTypes.map((value) => ({ value, label: value }))} value={filters.integrationType} onChange={(value) => setFilters((current) => ({ ...current, integrationType: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={integrationStatuses.map((value) => ({ value, label: value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Input allowClear placeholder="Reviewed by" style={{ width: 220 }} value={filters.reviewedBy} onChange={(event) => setFilters((current) => ({ ...current, reviewedBy: event.target.value || undefined, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New integration review</Button>
        </Flex>

        <Table
          rowKey="id"
          loading={query.isLoading}
          columns={columns}
          dataSource={query.data?.items ?? []}
          expandable={{
            expandedRowRender: (item) => (
              <Space direction="vertical" size={8} style={{ width: "100%" }}>
                <Text strong>Risks</Text>
                <Text>{item.risks || "-"}</Text>
                <Text strong>Dependency Impact</Text>
                <Text>{item.dependencyImpact || "-"}</Text>
                <Text strong>Evidence</Text>
                <Text>{item.evidenceRef || "-"}</Text>
              </Space>
            ),
          }}
          pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }}
        />
      </Card>

      <Modal title="Create integration review" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <IntegrationReviewForm form={form} />
      </Modal>

      <Modal title="Edit integration review" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => { if (open && editing) form.setFieldsValue(editing); }}>
        <IntegrationReviewForm form={form} />
      </Modal>
    </Space>
  );
}

function IntegrationReviewForm({ form }: { form: FormInstance<IntegrationReviewFormInput> }) {
  return (
    <Form form={form} layout="vertical" initialValues={{ status: "draft", integrationType: "api" }}>
      <Form.Item label="Integration Scope" name="scopeRef" rules={[{ required: true, message: "Scope is required." }]}>
        <Input />
      </Form.Item>
      <Form.Item label="Integration Type" name="integrationType" rules={[{ required: true, message: "Integration type is required." }]}>
        <Select options={integrationTypes.map((value) => ({ value, label: value }))} />
      </Form.Item>
      <Form.Item label="Reviewed By" name="reviewedBy">
        <Input />
      </Form.Item>
      <Form.Item label="Decision Reason" name="decisionReason">
        <Input.TextArea rows={3} />
      </Form.Item>
      <Form.Item label="Risks" name="risks">
        <Input.TextArea rows={3} />
      </Form.Item>
      <Form.Item label="Dependency Impact" name="dependencyImpact">
        <Input.TextArea rows={3} />
      </Form.Item>
      <Form.Item label="Evidence Ref" name="evidenceRef">
        <Input />
      </Form.Item>
      <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}>
        <Select options={integrationStatuses.map((value) => ({ value, label: value }))} />
      </Form.Item>
    </Form>
  );
}
