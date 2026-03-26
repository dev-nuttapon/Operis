import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { FormInstance } from "antd";
import type { ColumnsType } from "antd/es/table";
import { SafetyCertificateOutlined, PlusOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useAccessReviews, useApproveAccessReview, useCreateAccessReview, useUpdateAccessReview } from "../hooks/useOperations";
import type { AccessReview, ApproveAccessReviewInput, CreateAccessReviewInput, UpdateAccessReviewInput } from "../types/operations";

const { Title, Paragraph } = Typography;

export function AccessReviewsPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.operations.read);
  const canManage = permissionState.hasPermission(permissions.operations.manage);
  const canApprove = permissionState.hasPermission(permissions.operations.approve);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", scopeType: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 25 });
  const [editing, setEditing] = useState<AccessReview | null>(null);
  const [approveTarget, setApproveTarget] = useState<AccessReview | null>(null);
  const [createOpen, setCreateOpen] = useState(false);
  const [form] = Form.useForm<CreateAccessReviewInput>();
  const [approveForm] = Form.useForm<ApproveAccessReviewInput>();
  const query = useAccessReviews({ ...filters, sortBy: "createdAt", sortOrder: "desc" }, canRead);
  const createMutation = useCreateAccessReview();
  const updateMutation = useUpdateAccessReview();
  const approveMutation = useApproveAccessReview();

  const columns = useMemo<ColumnsType<AccessReview>>(
    () => [
      { title: "Scope", key: "scope", render: (_, item) => `${item.scopeType}: ${item.scopeRef}` },
      { title: "Review Cycle", dataIndex: "reviewCycle" },
      { title: "Assigned Reviewer", dataIndex: "reviewedBy", render: (value) => value ?? "-" },
      { title: "Decision", dataIndex: "decision", render: (value) => value ?? "-" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "Approved" ? "green" : "blue"}>{value}</Tag> },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => (
          <Flex gap={8} wrap>
            <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button>
            <Button size="small" disabled={!canApprove || item.status === "Approved" || item.status === "Archived"} onClick={() => setApproveTarget(item)}>Approve</Button>
          </Flex>
        ),
      },
    ],
    [canApprove, canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Access review data is not available for this account." />;
  }

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Access review created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create access review");
      void messageApi.error(presentation.description);
    }
  };

  const submitUpdate = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({ id: editing.id, input: { ...values, status: editing.status } as UpdateAccessReviewInput });
      form.resetFields();
      setEditing(null);
      void messageApi.success("Access review updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update access review");
      void messageApi.error(presentation.description);
    }
  };

  const submitApprove = async () => {
    if (!approveTarget) return;
    const values = await approveForm.validateFields();
    try {
      await approveMutation.mutateAsync({ id: approveTarget.id, input: values });
      approveForm.resetFields();
      setApproveTarget(null);
      void messageApi.success("Access review approved.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to approve access review");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0f766e, #1d4ed8)", color: "#fff" }}>
            <SafetyCertificateOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Access Review</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Track scheduled access recertification reviews, assigned reviewers, and approval outcomes with rationale.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search scope" style={{ width: 220 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
            <Select allowClear placeholder="Scope type" style={{ width: 180 }} options={["role", "group", "application"].map((value) => ({ label: value, value }))} value={filters.scopeType} onChange={(value) => setFilters((current) => ({ ...current, scopeType: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["Scheduled", "In Review", "Approved", "Archived"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New access review</Button>
        </Flex>

        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, defaultPageSize: 25, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create access review" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <AccessReviewForm form={form} />
      </Modal>

      <Modal title="Edit access review" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => { if (open && editing) { form.setFieldsValue(editing); } }}>
        <AccessReviewForm form={form} />
      </Modal>

      <Modal title="Approve access review" open={Boolean(approveTarget)} onOk={() => void submitApprove()} onCancel={() => { setApproveTarget(null); approveForm.resetFields(); }} confirmLoading={approveMutation.isPending} destroyOnHidden>
        <Form form={approveForm} layout="vertical">
          <Form.Item label="Decision" name="decision" rules={[{ required: true, message: "Decision is required." }]}>
            <Select options={[{ label: "approve", value: "approve" }, { label: "reject", value: "reject" }]} />
          </Form.Item>
          <Form.Item label="Rationale" name="decisionRationale" rules={[{ required: true, message: "Rationale is required." }]}>
            <Input.TextArea rows={4} placeholder="State the review evidence and outcome." />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}

function AccessReviewForm({ form }: { form: FormInstance<CreateAccessReviewInput> }) {
  return (
    <Form form={form} layout="vertical">
      <Form.Item label="Scope Type" name="scopeType" rules={[{ required: true, message: "Scope type is required." }]}>
        <Select options={["role", "group", "application"].map((value) => ({ label: value, value }))} />
      </Form.Item>
      <Form.Item label="Scope Reference" name="scopeRef" rules={[{ required: true, message: "Scope reference is required." }]}>
        <Input placeholder="finance-approver" />
      </Form.Item>
      <Form.Item label="Review Cycle" name="reviewCycle" rules={[{ required: true, message: "Review cycle is required." }]}>
        <Input placeholder="Q2-2026" />
      </Form.Item>
      <Form.Item label="Assigned Reviewer" name="reviewedBy">
        <Input placeholder="reviewer@example.com" />
      </Form.Item>
    </Form>
  );
}
