import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { StopOutlined, PlusOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateLegalHold, useLegalHolds, useReleaseLegalHold } from "../hooks/useOperations";
import type { CreateLegalHoldInput, LegalHold, ReleaseLegalHoldInput } from "../types/operations";

const { Title, Paragraph } = Typography;

export function LegalHoldRegisterPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.operations.read);
  const canManage = permissionState.hasPermission(permissions.operations.manage);
  const canApprove = permissionState.hasPermission(permissions.operations.approve);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", scopeType: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [releaseTarget, setReleaseTarget] = useState<LegalHold | null>(null);
  const [form] = Form.useForm<CreateLegalHoldInput>();
  const [releaseForm] = Form.useForm<ReleaseLegalHoldInput>();
  const query = useLegalHolds({ ...filters, sortBy: "placedAt", sortOrder: "desc" }, canRead);
  const createMutation = useCreateLegalHold();
  const releaseMutation = useReleaseLegalHold();

  const columns = useMemo<ColumnsType<LegalHold>>(
    () => [
      { title: "Scope", render: (_, item) => `${item.scopeType}: ${item.scopeRef}` },
      { title: "Placed At", dataIndex: "placedAt", render: (value) => new Date(value).toLocaleString() },
      { title: "Placed By", dataIndex: "placedBy" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "active" ? "red" : value === "released" ? "gold" : "default"}>{value}</Tag> },
      { title: "Reason", dataIndex: "reason" },
      { title: "Release", dataIndex: "releaseReason", render: (value) => value ?? "-" },
      { title: "Actions", render: (_, item) => <Button size="small" disabled={!canApprove || item.status !== "active"} onClick={() => setReleaseTarget(item)}>Release</Button> },
    ],
    [canApprove],
  );

  if (!canRead) return <Alert type="warning" showIcon message="Legal hold data is not available for this account." />;

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Legal hold created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create legal hold");
      void messageApi.error(presentation.description);
    }
  };

  const submitRelease = async () => {
    if (!releaseTarget) return;
    const values = await releaseForm.validateFields();
    try {
      await releaseMutation.mutateAsync({ id: releaseTarget.id, input: values });
      releaseForm.resetFields();
      setReleaseTarget(null);
      void messageApi.success("Legal hold released.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to release legal hold");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #dc2626, #7c2d12)", color: "#fff" }}>
            <StopOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Legal Hold Register</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Keep legal hold placement and governed release rationale visible to authorized audit and compliance users.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search scope or reason" style={{ width: 260 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
            <Select allowClear placeholder="Scope Type" style={{ width: 180 }} options={["project", "document", "record", "export"].map((value) => ({ label: value, value }))} value={filters.scopeType} onChange={(value) => setFilters((current) => ({ ...current, scopeType: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["active", "released", "archived"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New legal hold</Button>
        </Flex>

        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create legal hold" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <Form form={form} layout="vertical">
          <Form.Item label="Scope Type" name="scopeType" rules={[{ required: true, message: "Scope type is required." }]}>
            <Select options={["project", "document", "record", "export"].map((value) => ({ label: value, value }))} />
          </Form.Item>
          <Form.Item label="Scope Ref" name="scopeRef" rules={[{ required: true, message: "Scope reference is required." }]}>
            <Input placeholder="DOC-2026-0004" />
          </Form.Item>
          <Form.Item label="Reason" name="reason" rules={[{ required: true, message: "Reason is required." }]}>
            <Input.TextArea rows={4} placeholder="Preserve evidence pending investigation or regulatory request." />
          </Form.Item>
        </Form>
      </Modal>

      <Modal title="Release legal hold" open={Boolean(releaseTarget)} onOk={() => void submitRelease()} onCancel={() => { setReleaseTarget(null); releaseForm.resetFields(); }} confirmLoading={releaseMutation.isPending} destroyOnHidden>
        <Form form={releaseForm} layout="vertical">
          <Form.Item label="Release Rationale" name="reason" rules={[{ required: true, message: "Release rationale is required." }]}>
            <Input.TextArea rows={4} placeholder="Investigation closed and retention exception removed." />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
