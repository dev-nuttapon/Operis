import { useMemo, useState } from "react";
import { Alert, Button, Card, DatePicker, Flex, Form, Input, InputNumber, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { ExperimentOutlined, PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateDrDrill, useDrDrills, useUpdateDrDrill } from "../hooks/useOperations";
import type { CreateDrDrillInput, DrDrill, UpdateDrDrillInput } from "../types/operations";

const { Title, Paragraph } = Typography;
type DrDrillFormValues = Omit<CreateDrDrillInput, "plannedAt" | "executedAt"> & { plannedAt: dayjs.Dayjs; executedAt?: dayjs.Dayjs | null };

export function DrDrillLogPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.operations.read);
  const canManage = permissionState.hasPermission(permissions.operations.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", scopeRef: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<DrDrill | null>(null);
  const [form] = Form.useForm<DrDrillFormValues>();
  const query = useDrDrills({ ...filters, sortBy: "plannedAt", sortOrder: "desc" }, canRead);
  const createMutation = useCreateDrDrill();
  const updateMutation = useUpdateDrDrill();

  const columns = useMemo<ColumnsType<DrDrill>>(
    () => [
      { title: "Scope", dataIndex: "scopeRef" },
      { title: "Planned", dataIndex: "plannedAt", render: (value) => new Date(value).toLocaleString() },
      { title: "Executed", dataIndex: "executedAt", render: (value) => value ? new Date(value).toLocaleString() : "-" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Finding Count", dataIndex: "findingCount" },
      { title: "Actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button> },
    ],
    [canManage],
  );

  if (!canRead) return <Alert type="warning" showIcon message="DR drill data is not available for this account." />;

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync({ ...values, plannedAt: values.plannedAt.toISOString(), executedAt: values.executedAt?.toISOString() ?? null });
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("DR drill recorded.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create DR drill");
      void messageApi.error(presentation.description);
    }
  };

  const submitUpdate = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({ id: editing.id, input: { ...values, plannedAt: values.plannedAt.toISOString(), executedAt: values.executedAt?.toISOString() ?? null } as UpdateDrDrillInput });
      form.resetFields();
      setEditing(null);
      void messageApi.success("DR drill updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update DR drill");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #7c3aed, #1d4ed8)", color: "#fff" }}>
            <ExperimentOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>DR Drill Log</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Record disaster recovery drill execution, findings, and closure outcomes for governed continuity evidence.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search scope or summary" style={{ width: 260 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
            <Input allowClear placeholder="Scope Ref" style={{ width: 200 }} value={filters.scopeRef} onChange={(event) => setFilters((current) => ({ ...current, scopeRef: event.target.value || undefined, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["planned", "executed", "findings_issued", "closed"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New DR drill</Button>
        </Flex>

        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create DR drill" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <DrDrillForm form={form} />
      </Modal>

      <Modal title="Edit DR drill" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => { if (open && editing) { form.setFieldsValue({ scopeRef: editing.scopeRef, plannedAt: dayjs(editing.plannedAt), executedAt: editing.executedAt ? dayjs(editing.executedAt) : null, status: editing.status, findingCount: editing.findingCount, summary: editing.summary ?? undefined }); } }}>
        <DrDrillForm form={form} />
      </Modal>
    </Space>
  );
}

function DrDrillForm({ form }: { form: ReturnType<typeof Form.useForm<DrDrillFormValues>>[0] }) {
  return (
    <Form form={form} layout="vertical" initialValues={{ status: "planned", findingCount: 0 }}>
      <Form.Item label="Scope Ref" name="scopeRef" rules={[{ required: true, message: "Scope reference is required." }]}>
        <Input placeholder="operis-platform" />
      </Form.Item>
      <Form.Item label="Planned At" name="plannedAt" rules={[{ required: true, message: "Planned time is required." }]}>
        <DatePicker showTime style={{ width: "100%" }} />
      </Form.Item>
      <Form.Item label="Executed At" name="executedAt">
        <DatePicker showTime style={{ width: "100%" }} />
      </Form.Item>
      <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}>
        <Select options={["planned", "executed", "findings_issued", "closed"].map((value) => ({ label: value, value }))} />
      </Form.Item>
      <Form.Item label="Finding Count" name="findingCount" rules={[{ required: true, message: "Finding count is required." }]}>
        <InputNumber min={0} style={{ width: "100%" }} />
      </Form.Item>
      <Form.Item label="Summary" name="summary">
        <Input.TextArea rows={4} placeholder="Recovery target met, failover check completed, follow-up findings logged." />
      </Form.Item>
    </Form>
  );
}
