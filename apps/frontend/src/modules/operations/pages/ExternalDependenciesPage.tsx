import { useMemo, useState } from "react";
import { Alert, Button, Card, DatePicker, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { FormInstance } from "antd";
import type { ColumnsType } from "antd/es/table";
import { LinkOutlined, PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateExternalDependency, useExternalDependencies, useUpdateExternalDependency } from "../hooks/useOperations";
import type { CreateExternalDependencyInput, ExternalDependency, UpdateExternalDependencyInput } from "../types/operations";

const { Title, Paragraph } = Typography;

type ExternalDependencyFormValues = Omit<CreateExternalDependencyInput, "reviewDueAt"> & { reviewDueAt?: dayjs.Dayjs };

export function ExternalDependenciesPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.operations.read);
  const canManage = permissionState.hasPermission(permissions.operations.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", dependencyType: undefined as string | undefined, criticality: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 25 });
  const [editing, setEditing] = useState<ExternalDependency | null>(null);
  const [createOpen, setCreateOpen] = useState(false);
  const [form] = Form.useForm<ExternalDependencyFormValues>();
  const query = useExternalDependencies({ ...filters, sortBy: "reviewDueAt", sortOrder: "asc" }, canRead);
  const createMutation = useCreateExternalDependency();
  const updateMutation = useUpdateExternalDependency();

  const columns = useMemo<ColumnsType<ExternalDependency>>(
    () => [
      { title: "Name", dataIndex: "name" },
      { title: "Type", dataIndex: "dependencyType" },
      { title: "Owner", dataIndex: "ownerUserId" },
      { title: "Criticality", dataIndex: "criticality", render: (value: string) => <Tag color={value === "critical" ? "red" : value === "high" ? "orange" : "blue"}>{value}</Tag> },
      { title: "Review Due", dataIndex: "reviewDueAt", render: (value) => value ? new Date(value).toLocaleDateString() : "-" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Actions", key: "actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button> },
    ],
    [canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="External dependency data is not available for this account." />;
  }

  const mapValues = (values: ExternalDependencyFormValues): CreateExternalDependencyInput => ({
    ...values,
    reviewDueAt: values.reviewDueAt ? values.reviewDueAt.toISOString() : null,
  });

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(mapValues(values));
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("External dependency created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create external dependency");
      void messageApi.error(presentation.description);
    }
  };

  const submitUpdate = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({ id: editing.id, input: mapValues(values) as UpdateExternalDependencyInput });
      form.resetFields();
      setEditing(null);
      void messageApi.success("External dependency updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update external dependency");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #1d4ed8, #0f172a)", color: "#fff" }}>
            <LinkOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>External Dependency Register</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Maintain owned third-party dependencies, criticality, due reviews, and update status for operational governance.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search dependency or owner" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
            <Select allowClear placeholder="Type" style={{ width: 180 }} options={["identity_provider", "cloud_service", "integration", "vendor"].map((value) => ({ label: value, value }))} value={filters.dependencyType} onChange={(value) => setFilters((current) => ({ ...current, dependencyType: value, page: 1 }))} />
            <Select allowClear placeholder="Criticality" style={{ width: 180 }} options={["low", "medium", "high", "critical"].map((value) => ({ label: value, value }))} value={filters.criticality} onChange={(value) => setFilters((current) => ({ ...current, criticality: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["Active", "Review Due", "Updated", "Archived"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New dependency</Button>
        </Flex>

        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, defaultPageSize: 25, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create external dependency" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <ExternalDependencyForm form={form} />
      </Modal>

      <Modal title="Edit external dependency" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => { if (open && editing) { form.setFieldsValue({ ...editing, reviewDueAt: editing.reviewDueAt ? dayjs(editing.reviewDueAt) : undefined }); } }}>
        <ExternalDependencyForm form={form} />
      </Modal>
    </Space>
  );
}

function ExternalDependencyForm({ form }: { form: FormInstance<ExternalDependencyFormValues> }) {
  return (
    <Form form={form} layout="vertical" initialValues={{ criticality: "medium", status: "Active" }}>
      <Form.Item label="Name" name="name" rules={[{ required: true, message: "Name is required." }]}>
        <Input placeholder="Keycloak" />
      </Form.Item>
      <Form.Item label="Dependency Type" name="dependencyType" rules={[{ required: true, message: "Type is required." }]}>
        <Select options={["identity_provider", "cloud_service", "integration", "vendor"].map((value) => ({ label: value, value }))} />
      </Form.Item>
      <Form.Item label="Owner User" name="ownerUserId" rules={[{ required: true, message: "Owner is required." }]}>
        <Input placeholder="platform-owner@example.com" />
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
