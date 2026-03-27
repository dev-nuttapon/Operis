import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { FormInstance } from "antd";
import type { ColumnsType } from "antd/es/table";
import { FolderOpenOutlined, PlusOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useClassificationPolicies, useCreateClassificationPolicy, useUpdateClassificationPolicy } from "../hooks/useOperations";
import type { ClassificationPolicy, CreateClassificationPolicyInput, UpdateClassificationPolicyInput } from "../types/operations";

const { Title, Paragraph } = Typography;
const classificationLevelOptions = ["public", "internal", "confidential", "restricted"].map((value) => ({ label: value, value }));
const statusOptions = ["draft", "approved", "active", "archived"].map((value) => ({ label: value, value }));

export function ClassificationPolicyPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.operations.read, permissions.operations.manage);
  const canManage = permissionState.hasPermission(permissions.operations.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", classificationLevel: undefined as string | undefined, status: undefined as string | undefined, scope: undefined as string | undefined, page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<ClassificationPolicy | null>(null);
  const [form] = Form.useForm<CreateClassificationPolicyInput>();
  const query = useClassificationPolicies({ ...filters, sortBy: "policyCode", sortOrder: "asc" }, canRead);
  const createMutation = useCreateClassificationPolicy();
  const updateMutation = useUpdateClassificationPolicy();

  const columns = useMemo<ColumnsType<ClassificationPolicy>>(
    () => [
      { title: "Policy Code", dataIndex: "policyCode" },
      { title: "Classification", dataIndex: "classificationLevel", render: (value: string) => <Tag color={value === "restricted" ? "red" : value === "confidential" ? "volcano" : value === "internal" ? "gold" : "default"}>{value}</Tag> },
      { title: "Scope", dataIndex: "scope" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Handling Rule", dataIndex: "handlingRule", ellipsis: true, render: (value: string | null) => value ?? "-" },
      { title: "Actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button> },
    ],
    [canManage],
  );

  if (!canRead) return <Alert type="warning" showIcon message="Classification policies are not available for this account." />;

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync({ ...values, handlingRule: values.handlingRule ?? null });
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Classification policy created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create classification policy");
      void messageApi.error(presentation.description);
    }
  };

  const submitUpdate = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({ id: editing.id, input: { ...values, handlingRule: values.handlingRule ?? null } as UpdateClassificationPolicyInput });
      form.resetFields();
      setEditing(null);
      void messageApi.success("Classification policy updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update classification policy");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless"><Space align="start" size={16}><div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #1e3a8a, #312e81)", color: "#fff" }}><FolderOpenOutlined /></div><div><Title level={3} style={{ margin: 0 }}>Data Classification Policy</Title><Paragraph type="secondary" style={{ margin: "4px 0 0" }}>Maintain governed classification levels, scopes, and handling rules for operational data control.</Paragraph></div></Space></Card>
      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search code, scope, or handling rule" style={{ width: 300 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
            <Select allowClear placeholder="Classification" style={{ width: 180 }} options={classificationLevelOptions} value={filters.classificationLevel} onChange={(value) => setFilters((current) => ({ ...current, classificationLevel: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={statusOptions} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Input allowClear placeholder="Scope" style={{ width: 220 }} value={filters.scope} onChange={(event) => setFilters((current) => ({ ...current, scope: event.target.value || undefined, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New policy</Button>
        </Flex>
        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>
      <Modal title="Create classification policy" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden><ClassificationPolicyForm form={form} /></Modal>
      <Modal title="Edit classification policy" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => {
        if (open && editing) {
          form.setFieldsValue({ policyCode: editing.policyCode, classificationLevel: editing.classificationLevel, scope: editing.scope, status: editing.status, handlingRule: editing.handlingRule ?? undefined });
        }
      }}><ClassificationPolicyForm form={form} /></Modal>
    </Space>
  );
}

function ClassificationPolicyForm({ form }: { form: FormInstance<CreateClassificationPolicyInput> }) {
  return (
    <Form form={form} layout="vertical" initialValues={{ classificationLevel: "internal", status: "draft" }}>
      <Form.Item label="Policy Code" name="policyCode" rules={[{ required: true, message: "Policy code is required." }]}><Input /></Form.Item>
      <Form.Item label="Classification Level" name="classificationLevel" rules={[{ required: true, message: "Classification level is required." }]}><Select options={classificationLevelOptions} /></Form.Item>
      <Form.Item label="Scope" name="scope" rules={[{ required: true, message: "Scope is required." }]}><Input /></Form.Item>
      <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}><Select options={statusOptions} /></Form.Item>
      <Form.Item label="Handling Rule" name="handlingRule"><Input.TextArea rows={4} /></Form.Item>
    </Form>
  );
}
