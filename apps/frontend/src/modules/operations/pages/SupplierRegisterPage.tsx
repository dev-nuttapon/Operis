import { useMemo, useState } from "react";
import { Alert, Button, Card, DatePicker, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { FormInstance } from "antd";
import type { ColumnsType } from "antd/es/table";
import { ShopOutlined, PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateSupplier, useSuppliers, useUpdateSupplier } from "../hooks/useOperations";
import type { CreateSupplierInput, Supplier, UpdateSupplierInput } from "../types/operations";

const { Title, Paragraph } = Typography;

type SupplierFormValues = Omit<CreateSupplierInput, "reviewDueAt"> & { reviewDueAt?: dayjs.Dayjs };

export function SupplierRegisterPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.operations.read);
  const canManage = permissionState.hasPermission(permissions.operations.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", supplierType: undefined as string | undefined, ownerUserId: undefined as string | undefined, criticality: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 25 });
  const [editing, setEditing] = useState<Supplier | null>(null);
  const [createOpen, setCreateOpen] = useState(false);
  const [form] = Form.useForm<SupplierFormValues>();
  const query = useSuppliers({ ...filters, sortBy: "reviewDueAt", sortOrder: "asc" }, canRead);
  const createMutation = useCreateSupplier();
  const updateMutation = useUpdateSupplier();

  const columns = useMemo<ColumnsType<Supplier>>(
    () => [
      { title: "Name", dataIndex: "name" },
      { title: "Type", dataIndex: "supplierType" },
      { title: "Owner", dataIndex: "ownerUserId" },
      { title: "Criticality", dataIndex: "criticality", render: (value: string) => <Tag color={value === "critical" ? "red" : value === "high" ? "orange" : "blue"}>{value}</Tag> },
      { title: "Review Due", dataIndex: "reviewDueAt", render: (value) => value ? new Date(value).toLocaleDateString() : "-" },
      { title: "Active Agreements", dataIndex: "activeAgreementCount" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Actions", key: "actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button> },
    ],
    [canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Supplier data is not available for this account." />;
  }

  const mapValues = (values: SupplierFormValues): CreateSupplierInput => ({
    ...values,
    reviewDueAt: values.reviewDueAt ? values.reviewDueAt.toISOString() : null,
  });

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(mapValues(values));
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Supplier created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create supplier");
      void messageApi.error(presentation.description);
    }
  };

  const submitUpdate = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({ id: editing.id, input: mapValues(values) as UpdateSupplierInput });
      form.resetFields();
      setEditing(null);
      void messageApi.success("Supplier updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update supplier");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0f766e, #0f172a)", color: "#fff" }}>
            <ShopOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Supplier Register</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Manage supplier ownership, review cadence, and criticality for governed external dependencies.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search supplier or owner" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
            <Select allowClear placeholder="Supplier type" style={{ width: 180 }} options={["vendor", "partner", "service_provider", "contractor"].map((value) => ({ label: value, value }))} value={filters.supplierType} onChange={(value) => setFilters((current) => ({ ...current, supplierType: value, page: 1 }))} />
            <Input allowClear placeholder="Owner user" style={{ width: 180 }} value={filters.ownerUserId} onChange={(event) => setFilters((current) => ({ ...current, ownerUserId: event.target.value || undefined, page: 1 }))} />
            <Select allowClear placeholder="Criticality" style={{ width: 180 }} options={["low", "medium", "high", "critical"].map((value) => ({ label: value, value }))} value={filters.criticality} onChange={(value) => setFilters((current) => ({ ...current, criticality: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["Active", "Review Due", "Updated", "Archived"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New supplier</Button>
        </Flex>

        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, defaultPageSize: 25, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create supplier" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <SupplierForm form={form} />
      </Modal>

      <Modal title="Edit supplier" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => { if (open && editing) { form.setFieldsValue({ ...editing, reviewDueAt: editing.reviewDueAt ? dayjs(editing.reviewDueAt) : undefined }); } }}>
        <SupplierForm form={form} />
      </Modal>
    </Space>
  );
}

function SupplierForm({ form }: { form: FormInstance<SupplierFormValues> }) {
  return (
    <Form form={form} layout="vertical" initialValues={{ criticality: "medium", status: "Active", supplierType: "vendor" }}>
      <Form.Item label="Name" name="name" rules={[{ required: true, message: "Name is required." }]}>
        <Input placeholder="Acme Services" />
      </Form.Item>
      <Form.Item label="Supplier Type" name="supplierType" rules={[{ required: true, message: "Type is required." }]}>
        <Select options={["vendor", "partner", "service_provider", "contractor"].map((value) => ({ label: value, value }))} />
      </Form.Item>
      <Form.Item label="Owner User" name="ownerUserId" rules={[{ required: true, message: "Owner is required." }]}>
        <Input placeholder="vendor-manager@example.com" />
      </Form.Item>
      <Form.Item label="Criticality" name="criticality" rules={[{ required: true, message: "Criticality is required." }]}>
        <Select options={["low", "medium", "high", "critical"].map((value) => ({ label: value, value }))} />
      </Form.Item>
      <Form.Item label="Review Due" name="reviewDueAt">
        <DatePicker style={{ width: "100%" }} />
      </Form.Item>
      <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}>
        <Select options={["Active", "Review Due", "Updated", "Archived"].map((value) => ({ label: value, value }))} />
      </Form.Item>
    </Form>
  );
}
