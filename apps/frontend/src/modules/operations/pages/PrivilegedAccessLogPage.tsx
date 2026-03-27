import { useMemo, useState } from "react";
import { Alert, Button, Card, DatePicker, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { FormInstance } from "antd";
import type { ColumnsType } from "antd/es/table";
import { LockOutlined, PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreatePrivilegedAccessEvent, usePrivilegedAccessEvents, useUpdatePrivilegedAccessEvent } from "../hooks/useOperations";
import type { CreatePrivilegedAccessEventInput, PrivilegedAccessEvent, UpdatePrivilegedAccessEventInput } from "../types/operations";

const { Title, Paragraph } = Typography;
const statusOptions = ["requested", "approved", "used", "reviewed", "closed"].map((value) => ({ label: value, value }));
type PrivilegedAccessFormValues = Omit<CreatePrivilegedAccessEventInput, "requestedAt" | "approvedAt" | "usedAt" | "reviewedAt"> & { requestedAtDay?: dayjs.Dayjs; approvedAtDay?: dayjs.Dayjs; usedAtDay?: dayjs.Dayjs; reviewedAtDay?: dayjs.Dayjs };

export function PrivilegedAccessLogPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.operations.read, permissions.operations.manage);
  const canManage = permissionState.hasPermission(permissions.operations.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", status: undefined as string | undefined, requestedBy: undefined as string | undefined, approvedBy: undefined as string | undefined, usedBy: undefined as string | undefined, page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<PrivilegedAccessEvent | null>(null);
  const [form] = Form.useForm<PrivilegedAccessFormValues>();
  const query = usePrivilegedAccessEvents({ ...filters, sortBy: "requestedAt", sortOrder: "desc" }, canRead);
  const createMutation = useCreatePrivilegedAccessEvent();
  const updateMutation = useUpdatePrivilegedAccessEvent();

  const columns = useMemo<ColumnsType<PrivilegedAccessEvent>>(
    () => [
      { title: "Requested By", dataIndex: "requestedBy" },
      { title: "Approved By", dataIndex: "approvedBy", render: (value: string | null) => value ?? "-" },
      { title: "Used By", dataIndex: "usedBy", render: (value: string | null) => value ?? "-" },
      { title: "Requested At", dataIndex: "requestedAt", render: (value: string) => dayjs(value).format("YYYY-MM-DD HH:mm") },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Reason", dataIndex: "reason", ellipsis: true },
      { title: "Actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button> },
    ],
    [canManage],
  );

  if (!canRead) return <Alert type="warning" showIcon message="Privileged access events are not available for this account." />;

  const toInput = (values: PrivilegedAccessFormValues, fallbackRequestedAt?: string): CreatePrivilegedAccessEventInput => ({
    requestedBy: values.requestedBy,
    approvedBy: values.approvedBy ?? null,
    usedBy: values.usedBy ?? null,
    requestedAt: (values.requestedAtDay ?? dayjs(fallbackRequestedAt)).toISOString(),
    approvedAt: values.approvedAtDay ? values.approvedAtDay.toISOString() : null,
    usedAt: values.usedAtDay ? values.usedAtDay.toISOString() : null,
    reviewedAt: values.reviewedAtDay ? values.reviewedAtDay.toISOString() : null,
    status: values.status,
    reason: values.reason,
  });

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(toInput(values, dayjs().toISOString()));
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Privileged access event created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create privileged access event");
      void messageApi.error(presentation.description);
    }
  };

  const submitUpdate = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({ id: editing.id, input: toInput(values, editing.requestedAt) as UpdatePrivilegedAccessEventInput });
      form.resetFields();
      setEditing(null);
      void messageApi.success("Privileged access event updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update privileged access event");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless"><Space align="start" size={16}><div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #111827, #374151)", color: "#fff" }}><LockOutlined /></div><div><Title level={3} style={{ margin: 0 }}>Privileged Access Log</Title><Paragraph type="secondary" style={{ margin: "4px 0 0" }}>Track break-glass and privileged sessions from request to approval, use, review, and closure.</Paragraph></div></Space></Card>
      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search requester, approver, user, or reason" style={{ width: 300 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={statusOptions} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Input allowClear placeholder="Requested By" style={{ width: 180 }} value={filters.requestedBy} onChange={(event) => setFilters((current) => ({ ...current, requestedBy: event.target.value || undefined, page: 1 }))} />
            <Input allowClear placeholder="Approved By" style={{ width: 180 }} value={filters.approvedBy} onChange={(event) => setFilters((current) => ({ ...current, approvedBy: event.target.value || undefined, page: 1 }))} />
            <Input allowClear placeholder="Used By" style={{ width: 180 }} value={filters.usedBy} onChange={(event) => setFilters((current) => ({ ...current, usedBy: event.target.value || undefined, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New event</Button>
        </Flex>
        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>
      <Modal title="Create privileged access event" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden><PrivilegedAccessForm form={form} /></Modal>
      <Modal title="Edit privileged access event" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => {
        if (open && editing) {
          form.setFieldsValue({ requestedBy: editing.requestedBy, approvedBy: editing.approvedBy ?? undefined, usedBy: editing.usedBy ?? undefined, requestedAtDay: dayjs(editing.requestedAt), approvedAtDay: editing.approvedAt ? dayjs(editing.approvedAt) : undefined, usedAtDay: editing.usedAt ? dayjs(editing.usedAt) : undefined, reviewedAtDay: editing.reviewedAt ? dayjs(editing.reviewedAt) : undefined, status: editing.status, reason: editing.reason });
        }
      }}><PrivilegedAccessForm form={form} /></Modal>
    </Space>
  );
}

function PrivilegedAccessForm({ form }: { form: FormInstance<PrivilegedAccessFormValues> }) {
  return (
    <Form form={form} layout="vertical" initialValues={{ status: "requested" }}>
      <Form.Item label="Requested By" name="requestedBy" rules={[{ required: true, message: "Requester is required." }]}><Input /></Form.Item>
      <Form.Item label="Approved By" name="approvedBy"><Input /></Form.Item>
      <Form.Item label="Used By" name="usedBy"><Input /></Form.Item>
      <Form.Item label="Requested At" name="requestedAtDay" rules={[{ required: true, message: "Request time is required." }]}><DatePicker showTime style={{ width: "100%" }} /></Form.Item>
      <Form.Item label="Approved At" name="approvedAtDay"><DatePicker showTime style={{ width: "100%" }} /></Form.Item>
      <Form.Item label="Used At" name="usedAtDay"><DatePicker showTime style={{ width: "100%" }} /></Form.Item>
      <Form.Item label="Reviewed At" name="reviewedAtDay"><DatePicker showTime style={{ width: "100%" }} /></Form.Item>
      <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}><Select options={statusOptions} /></Form.Item>
      <Form.Item label="Reason" name="reason" rules={[{ required: true, message: "Reason is required." }]}><Input.TextArea rows={4} /></Form.Item>
    </Form>
  );
}
