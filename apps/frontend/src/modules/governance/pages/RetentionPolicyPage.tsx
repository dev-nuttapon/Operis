import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, InputNumber, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { FormInstance } from "antd";
import { InboxOutlined, PlusOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateRetentionPolicy, useRetentionPolicies, useUpdateRetentionPolicy } from "../hooks/useGovernance";
import type { RetentionPolicy, RetentionPolicyFormInput } from "../types/governance";

const { Title, Paragraph } = Typography;

export function RetentionPolicyPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.governance.retentionRead);
  const canManage = permissionState.hasPermission(permissions.governance.retentionManage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", appliesTo: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 25 });
  const [editing, setEditing] = useState<RetentionPolicy | null>(null);
  const [createOpen, setCreateOpen] = useState(false);
  const [form] = Form.useForm<RetentionPolicyFormInput>();
  const query = useRetentionPolicies(filters, canRead);
  const createMutation = useCreateRetentionPolicy();
  const updateMutation = useUpdateRetentionPolicy();

  const columns = useMemo<ColumnsType<RetentionPolicy>>(
    () => [
      { title: "Policy Code", dataIndex: "policyCode" },
      { title: "Applies To", dataIndex: "appliesTo" },
      { title: "Retention Days", dataIndex: "retentionPeriodDays" },
      { title: "Archive Rule", dataIndex: "archiveRule", render: (value) => value ?? "-" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Actions", key: "actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button> },
    ],
    [canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Retention policy access is not available for this account." />;
  }

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      setCreateOpen(false);
      form.resetFields();
      void messageApi.success("Retention policy created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create retention policy");
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
      void messageApi.success("Retention policy updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update retention policy");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #4338ca, #0f172a)", color: "#fff" }}>
            <InboxOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Data Retention Policy</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Manage retention periods, archive rules, and lifecycle status for governed record classes.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search policy or scope" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
            <Select allowClear placeholder="Applies to" style={{ width: 180 }} options={["documents", "audit_logs", "project_records"].map((value) => ({ label: value, value }))} value={filters.appliesTo} onChange={(value) => setFilters((current) => ({ ...current, appliesTo: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["draft", "approved", "active", "archived"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New retention policy</Button>
        </Flex>

        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create retention policy" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <RetentionPolicyForm form={form} />
      </Modal>

      <Modal title="Edit retention policy" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => { if (open && editing) form.setFieldsValue(editing); }}>
        <RetentionPolicyForm form={form} />
      </Modal>
    </Space>
  );
}

function RetentionPolicyForm({ form }: { form: FormInstance<RetentionPolicyFormInput> }) {
  return (
    <Form form={form} layout="vertical" initialValues={{ status: "draft", retentionPeriodDays: 365 }}>
      <Form.Item label="Policy Code" name="policyCode" rules={[{ required: true, message: "Policy code is required." }]}>
        <Input placeholder="DOC-RET-001" />
      </Form.Item>
      <Form.Item label="Applies To" name="appliesTo" rules={[{ required: true, message: "Scope is required." }]}>
        <Select options={["documents", "audit_logs", "project_records"].map((value) => ({ label: value, value }))} />
      </Form.Item>
      <Form.Item label="Retention Period Days" name="retentionPeriodDays" rules={[{ required: true, message: "Retention period is required." }]}>
        <InputNumber min={1} style={{ width: "100%" }} />
      </Form.Item>
      <Form.Item label="Archive Rule" name="archiveRule">
        <Input placeholder="archive_to_cold_storage" />
      </Form.Item>
      <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}>
        <Select options={["draft", "approved", "active", "archived"].map((value) => ({ label: value, value }))} />
      </Form.Item>
      <Form.Item label="Reason" name="reason">
        <Input.TextArea rows={3} placeholder="Optional note for approved or archived changes." />
      </Form.Item>
    </Form>
  );
}
