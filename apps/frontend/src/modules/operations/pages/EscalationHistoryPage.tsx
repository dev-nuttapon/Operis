import { useState } from "react";
import { Alert, App, Button, Card, DatePicker, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography } from "antd";
import { NotificationOutlined, PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateEscalationEvent, useEscalationEvents } from "../hooks/useOperations";
import type { CreateEscalationEventInput } from "../types/operations";

type EscalationFormValues = Omit<CreateEscalationEventInput, "triggeredAt"> & { triggeredAt: dayjs.Dayjs };

export function EscalationHistoryPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.operations.read, permissions.operations.manage);
  const canManage = permissionState.hasPermission(permissions.operations.manage);
  const { notification } = App.useApp();
  const [filters, setFilters] = useState({ search: "", scopeType: undefined as string | undefined, escalatedTo: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [form] = Form.useForm<EscalationFormValues>();
  const query = useEscalationEvents({ ...filters, sortBy: "triggeredAt", sortOrder: "desc" }, canRead);
  const createMutation = useCreateEscalationEvent();

  if (!canRead) {
    return <Alert type="warning" showIcon message="Escalation history is not available for this account." />;
  }

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync({ ...values, triggeredAt: values.triggeredAt.toISOString() });
      setCreateOpen(false);
      form.resetFields();
      notification.success({ message: "Escalation event created." });
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create escalation event");
      notification.error({ message: presentation.title, description: presentation.description });
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #dc2626, #7c3aed)", color: "#fff" }}>
            <NotificationOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>Escalation History</Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Track when governed reviews, findings, or exceptions escalate and who they were escalated to.
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search scope, target, or reason" style={{ width: 260 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
            <Select allowClear placeholder="Scope type" style={{ width: 180 }} options={["sla_rule", "capa", "incident", "access_review"].map((value) => ({ label: value, value }))} value={filters.scopeType} onChange={(value) => setFilters((current) => ({ ...current, scopeType: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["triggered", "acknowledged", "closed"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New escalation</Button>
        </Flex>

        <Table rowKey="id" loading={query.isLoading} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} columns={[
          { title: "Scope", key: "scope", render: (_, item) => `${item.scopeType}: ${item.scopeRef}` },
          { title: "Triggered At", dataIndex: "triggeredAt", render: (value: string) => new Date(value).toLocaleString() },
          { title: "Escalated To", dataIndex: "escalatedTo" },
          { title: "Reason", dataIndex: "triggerReason" },
          { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "closed" ? "green" : value === "acknowledged" ? "blue" : "red"}>{value}</Tag> },
        ]} />
      </Card>

      <Modal title="Create Escalation Event" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <Form form={form} layout="vertical" initialValues={{ status: "triggered", triggeredAt: dayjs() }}>
          <Form.Item label="Scope Type" name="scopeType" rules={[{ required: true, message: "Scope type is required." }]}><Select options={["sla_rule", "capa", "incident", "access_review"].map((value) => ({ label: value, value }))} /></Form.Item>
          <Form.Item label="Scope Reference" name="scopeRef" rules={[{ required: true, message: "Scope reference is required." }]}><Input /></Form.Item>
          <Form.Item label="Triggered At" name="triggeredAt" rules={[{ required: true, message: "Triggered time is required." }]}><DatePicker showTime style={{ width: "100%" }} /></Form.Item>
          <Form.Item label="Trigger Reason" name="triggerReason" rules={[{ required: true, message: "Trigger reason is required." }]}><Input.TextArea rows={4} /></Form.Item>
          <Form.Item label="Escalated To" name="escalatedTo" rules={[{ required: true, message: "Escalated target is required." }]}><Input placeholder="compliance-board@example.com" /></Form.Item>
          <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}><Select options={["triggered", "acknowledged", "closed"].map((value) => ({ label: value, value }))} /></Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
