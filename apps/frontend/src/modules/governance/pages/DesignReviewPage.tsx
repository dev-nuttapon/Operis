import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { FormInstance } from "antd";
import { AuditOutlined, PlusOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useArchitectureRecords, useCreateDesignReview, useDesignReviews, useUpdateDesignReview } from "../hooks/useGovernance";
import type { DesignReview, DesignReviewFormInput } from "../types/governance";

const { Title, Paragraph, Text } = Typography;

const designStatuses = ["draft", "in_review", "approved", "rejected", "baseline"];
const reviewTypes = ["architecture", "security", "peer", "design_authority"];

export function DesignReviewPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.governance.designReviewRead, permissions.governance.designReviewManage);
  const canManage = permissionState.hasPermission(permissions.governance.designReviewManage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", status: undefined as string | undefined, architectureRecordId: undefined as string | undefined, reviewType: undefined as string | undefined, reviewedBy: undefined as string | undefined, page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<DesignReview | null>(null);
  const [form] = Form.useForm<DesignReviewFormInput>();
  const query = useDesignReviews(filters, canRead);
  const architecturesQuery = useArchitectureRecords({ page: 1, pageSize: 100 }, canRead);
  const createMutation = useCreateDesignReview();
  const updateMutation = useUpdateDesignReview();

  const columns = useMemo<ColumnsType<DesignReview>>(
    () => [
      { title: "Architecture", dataIndex: "architectureTitle", render: (value, item) => value ?? item.architectureRecordId },
      { title: "Review Type", dataIndex: "reviewType" },
      { title: "Reviewed By", dataIndex: "reviewedBy", render: (value) => value ?? "-" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Decision Reason", dataIndex: "decisionReason", render: (value) => value ?? "-" },
      { title: "Actions", key: "actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button> },
    ],
    [canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Design review data is not available for this account." />;
  }

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      setCreateOpen(false);
      form.resetFields();
      void messageApi.success("Design review created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create design review");
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
      void messageApi.success("Design review updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update design review");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0f766e, #155e75)", color: "#fff" }}>
            <AuditOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Design Review</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Record design concerns, review outcomes, and evidence before architecture decisions become a baseline.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search architecture or decision" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
            <Select allowClear placeholder="Architecture" style={{ width: 220 }} options={(architecturesQuery.data?.items ?? []).map((item) => ({ value: item.id, label: item.title }))} value={filters.architectureRecordId} onChange={(value) => setFilters((current) => ({ ...current, architectureRecordId: value, page: 1 }))} />
            <Select allowClear placeholder="Review type" style={{ width: 180 }} options={reviewTypes.map((value) => ({ value, label: value }))} value={filters.reviewType} onChange={(value) => setFilters((current) => ({ ...current, reviewType: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={designStatuses.map((value) => ({ value, label: value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Input allowClear placeholder="Reviewed by" style={{ width: 220 }} value={filters.reviewedBy} onChange={(event) => setFilters((current) => ({ ...current, reviewedBy: event.target.value || undefined, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New design review</Button>
        </Flex>

        <Table
          rowKey="id"
          loading={query.isLoading}
          columns={columns}
          dataSource={query.data?.items ?? []}
          expandable={{
            expandedRowRender: (item) => (
              <Space direction="vertical" size={8} style={{ width: "100%" }}>
                <Text strong>Design Summary</Text>
                <Text>{item.designSummary || "-"}</Text>
                <Text strong>Concerns</Text>
                <Text>{item.concerns || "-"}</Text>
                <Text strong>Evidence</Text>
                <Text>{item.evidenceRef || "-"}</Text>
              </Space>
            ),
          }}
          pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }}
        />
      </Card>

      <Modal title="Create design review" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <DesignReviewForm form={form} architectureOptions={(architecturesQuery.data?.items ?? []).map((item) => ({ value: item.id, label: item.title }))} />
      </Modal>

      <Modal title="Edit design review" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => { if (open && editing) form.setFieldsValue(editing); }}>
        <DesignReviewForm form={form} architectureOptions={(architecturesQuery.data?.items ?? []).map((item) => ({ value: item.id, label: item.title }))} />
      </Modal>
    </Space>
  );
}

function DesignReviewForm({ form, architectureOptions }: { form: FormInstance<DesignReviewFormInput>; architectureOptions: Array<{ value: string; label: string }> }) {
  return (
    <Form form={form} layout="vertical" initialValues={{ status: "draft", reviewType: "architecture" }}>
      <Form.Item label="Architecture Record" name="architectureRecordId" rules={[{ required: true, message: "Architecture record is required." }]}>
        <Select options={architectureOptions} showSearch optionFilterProp="label" />
      </Form.Item>
      <Form.Item label="Review Type" name="reviewType" rules={[{ required: true, message: "Review type is required." }]}>
        <Select options={reviewTypes.map((value) => ({ value, label: value }))} />
      </Form.Item>
      <Form.Item label="Reviewed By" name="reviewedBy">
        <Input />
      </Form.Item>
      <Form.Item label="Decision Reason" name="decisionReason">
        <Input.TextArea rows={3} />
      </Form.Item>
      <Form.Item label="Design Summary" name="designSummary">
        <Input.TextArea rows={3} />
      </Form.Item>
      <Form.Item label="Concerns" name="concerns">
        <Input.TextArea rows={3} />
      </Form.Item>
      <Form.Item label="Evidence Ref" name="evidenceRef">
        <Input />
      </Form.Item>
      <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}>
        <Select options={designStatuses.map((value) => ({ value, label: value }))} />
      </Form.Item>
    </Form>
  );
}
