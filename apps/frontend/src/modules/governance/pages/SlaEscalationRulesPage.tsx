import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, InputNumber, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { FormInstance } from "antd";
import { ClockCircleOutlined, PlusOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateSlaRule, useSlaRules, useUpdateSlaRule } from "../hooks/useGovernance";
import type { SlaRule, SlaRuleFormInput } from "../types/governance";

const { Title, Paragraph } = Typography;

export function SlaEscalationRulesPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.governance.slaRead);
  const canManage = permissionState.hasPermission(permissions.governance.slaManage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", scopeType: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 25 });
  const [editing, setEditing] = useState<SlaRule | null>(null);
  const [createOpen, setCreateOpen] = useState(false);
  const [form] = Form.useForm<SlaRuleFormInput>();
  const query = useSlaRules(filters, canRead);
  const createMutation = useCreateSlaRule();
  const updateMutation = useUpdateSlaRule();

  const columns = useMemo<ColumnsType<SlaRule>>(
    () => [
      { title: "Scope", key: "scope", render: (_, item) => `${item.scopeType}: ${item.scopeRef}` },
      { title: "Target Duration (hrs)", dataIndex: "targetDurationHours" },
      { title: "Escalation Policy", dataIndex: "escalationPolicyId" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Actions", key: "actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button> },
    ],
    [canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="SLA rule access is not available for this account." />;
  }

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      setCreateOpen(false);
      form.resetFields();
      void messageApi.success("SLA rule created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create SLA rule");
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
      void messageApi.success("SLA rule updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update SLA rule");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #2563eb, #4338ca)", color: "#fff" }}>
            <ClockCircleOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>SLA & Escalation Rules</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Define governed response targets and escalation policies for controlled workflow scopes.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search scope or policy" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
            <Select allowClear placeholder="Scope type" style={{ width: 180 }} options={["project", "workflow", "approval"].map((value) => ({ label: value, value }))} value={filters.scopeType} onChange={(value) => setFilters((current) => ({ ...current, scopeType: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["draft", "approved", "active", "archived"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New SLA rule</Button>
        </Flex>

        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create SLA rule" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <SlaRuleForm form={form} />
      </Modal>

      <Modal title="Edit SLA rule" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => { if (open && editing) form.setFieldsValue(editing); }}>
        <SlaRuleForm form={form} />
      </Modal>
    </Space>
  );
}

function SlaRuleForm({ form }: { form: FormInstance<SlaRuleFormInput> }) {
  return (
    <Form form={form} layout="vertical" initialValues={{ status: "draft", targetDurationHours: 4 }}>
      <Form.Item label="Scope Type" name="scopeType" rules={[{ required: true, message: "Scope type is required." }]}>
        <Select options={["project", "workflow", "approval"].map((value) => ({ label: value, value }))} />
      </Form.Item>
      <Form.Item label="Scope Reference" name="scopeRef" rules={[{ required: true, message: "Scope reference is required." }]}>
        <Input placeholder="workflow:document-approval" />
      </Form.Item>
      <Form.Item label="Target Duration Hours" name="targetDurationHours" rules={[{ required: true, message: "Target duration is required." }]}>
        <InputNumber min={1} style={{ width: "100%" }} />
      </Form.Item>
      <Form.Item label="Escalation Policy" name="escalationPolicyId" rules={[{ required: true, message: "Escalation policy is required." }]}>
        <Input placeholder="policy-ops-01" />
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
