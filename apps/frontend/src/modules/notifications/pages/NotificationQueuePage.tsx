import { useState } from "react";
import { Alert, App, Button, Card, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography } from "antd";
import { MailOutlined, PlusOutlined, RedoOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useNotificationActions } from "../hooks/useNotificationActions";
import { useNotificationQueue } from "../hooks/useNotifications";
import type { CreateNotificationQueueInput, NotificationQueueItem } from "../types/notifications";

export function NotificationQueuePage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.notifications.read, permissions.notifications.manage);
  const canManage = permissionState.hasPermission(permissions.notifications.manage);
  const { notification } = App.useApp();
  const [filters, setFilters] = useState({ search: "", channel: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [form] = Form.useForm<CreateNotificationQueueInput>();
  const query = useNotificationQueue(filters, canRead);
  const actions = useNotificationActions();

  if (!canRead) {
    return <Alert type="warning" showIcon message="Notification queue is not available for this account." />;
  }

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await actions.enqueueMutation.mutateAsync(values);
      setCreateOpen(false);
      form.resetFields();
      notification.success({ message: "Notification queued." });
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to queue notification");
      notification.error({ message: presentation.title, description: presentation.description });
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0f766e, #1d4ed8)", color: "#fff" }}>
            <MailOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>Notification Queue</Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Review queued delivery attempts, failure reasons, retries, and outbound notification channels.
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search target, payload, or error" style={{ width: 260 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
            <Select allowClear placeholder="Channel" style={{ width: 180 }} options={["email", "chat", "webhook"].map((value) => ({ label: value, value }))} value={filters.channel} onChange={(value) => setFilters((current) => ({ ...current, channel: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["queued", "sent", "failed", "retried", "closed"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New queue item</Button>
        </Flex>

        <Table<NotificationQueueItem> rowKey="id" loading={query.isLoading} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} columns={[
          { title: "Channel", dataIndex: "channel", render: (value: string) => <Tag>{value}</Tag> },
          { title: "Target", dataIndex: "targetRef" },
          { title: "Payload", dataIndex: "payloadRef" },
          { title: "Queued At", dataIndex: "queuedAt", render: (value: string) => new Date(value).toLocaleString() },
          { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "failed" ? "red" : value === "sent" ? "green" : value === "retried" ? "blue" : "gold"}>{value}</Tag> },
          { title: "Retries", dataIndex: "retryCount" },
          { title: "Actions", key: "actions", render: (_, item) => (
            <Button size="small" icon={<RedoOutlined />} disabled={!canManage || item.status !== "failed" || actions.retryMutation.isPending} onClick={async () => {
              try {
                await actions.retryMutation.mutateAsync(item.id);
                notification.success({ message: "Notification retry queued." });
              } catch (error) {
                const presentation = getApiErrorPresentation(error, "Unable to retry notification");
                notification.error({ message: presentation.title, description: presentation.description });
              }
            }}>Retry</Button>
          ) },
        ]} />
      </Card>

      <Modal title="Create Notification Queue Item" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={actions.enqueueMutation.isPending} destroyOnHidden>
        <Form form={form} layout="vertical" initialValues={{ channel: "email", status: "queued" }}>
          <Form.Item label="Channel" name="channel" rules={[{ required: true, message: "Channel is required." }]}><Select options={["email", "chat", "webhook"].map((value) => ({ label: value, value }))} /></Form.Item>
          <Form.Item label="Target Reference" name="targetRef" rules={[{ required: true, message: "Target reference is required." }]}><Input placeholder="ops@example.com" /></Form.Item>
          <Form.Item label="Payload Reference" name="payloadRef" rules={[{ required: true, message: "Payload reference is required." }]}><Input placeholder="minio://notifications/capa-24.json" /></Form.Item>
          <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}><Select options={["queued", "sent", "failed", "retried", "closed"].map((value) => ({ label: value, value }))} /></Form.Item>
          <Form.Item label="Last Error" name="lastError"><Input.TextArea rows={3} /></Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
