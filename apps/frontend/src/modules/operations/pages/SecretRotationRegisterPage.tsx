import { useMemo, useState } from "react";
import { Alert, Button, Card, DatePicker, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { FormInstance } from "antd";
import type { ColumnsType } from "antd/es/table";
import { KeyOutlined, PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateSecretRotation, useSecretRotations, useUpdateSecretRotation } from "../hooks/useOperations";
import type { CreateSecretRotationInput, SecretRotation, UpdateSecretRotationInput } from "../types/operations";

const { Title, Paragraph } = Typography;
const statusOptions = ["planned", "rotated", "verified", "archived"].map((value) => ({ label: value, value }));
const touchpointOptions = ["keycloak", "redis", "minio", "custom"].map((value) => ({ label: value, value }));
type SecretRotationFormValues = Omit<CreateSecretRotationInput, "plannedAt" | "rotatedAt" | "verifiedAt"> & { plannedAtDay?: dayjs.Dayjs; rotatedAtDay?: dayjs.Dayjs; verifiedAtDay?: dayjs.Dayjs };

export function SecretRotationRegisterPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.operations.read, permissions.operations.manage);
  const canManage = permissionState.hasPermission(permissions.operations.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", touchpoint: undefined as string | undefined, status: undefined as string | undefined, verifiedBy: undefined as string | undefined, page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<SecretRotation | null>(null);
  const [form] = Form.useForm<SecretRotationFormValues>();
  const query = useSecretRotations({ ...filters, sortBy: "plannedAt", sortOrder: "desc" }, canRead);
  const createMutation = useCreateSecretRotation();
  const updateMutation = useUpdateSecretRotation();

  const columns = useMemo<ColumnsType<SecretRotation>>(
    () => [
      { title: "Touchpoint", dataIndex: "touchpoint", render: (value: string) => <Tag color={value === "keycloak" ? "blue" : value === "redis" ? "gold" : value === "minio" ? "cyan" : "default"}>{value}</Tag> },
      { title: "Secret Scope", dataIndex: "secretScope" },
      { title: "Evidence", dataIndex: "evidenceRef", render: (value: string | null) => value ?? "-" },
      { title: "Planned", dataIndex: "plannedAt", render: (value: string) => dayjs(value).format("YYYY-MM-DD HH:mm") },
      { title: "Rotated", dataIndex: "rotatedAt", render: (value: string | null) => value ? dayjs(value).format("YYYY-MM-DD HH:mm") : "-" },
      { title: "Verified By", dataIndex: "verifiedBy", render: (value: string | null) => value ?? "-" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button> },
    ],
    [canManage],
  );

  if (!canRead) return <Alert type="warning" showIcon message="Secret rotation data is not available for this account." />;

  const toInput = (values: SecretRotationFormValues, fallbackPlannedAt?: string): CreateSecretRotationInput => ({
    touchpoint: values.touchpoint,
    secretScope: values.secretScope,
    evidenceRef: values.evidenceRef ?? null,
    plannedAt: (values.plannedAtDay ?? dayjs(fallbackPlannedAt)).toISOString(),
    rotatedAt: values.rotatedAtDay ? values.rotatedAtDay.toISOString() : null,
    verifiedBy: values.verifiedBy ?? null,
    verifiedAt: values.verifiedAtDay ? values.verifiedAtDay.toISOString() : null,
    status: values.status,
  });

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(toInput(values, dayjs().toISOString()));
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Secret rotation created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create secret rotation");
      void messageApi.error(presentation.description);
    }
  };

  const submitUpdate = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({ id: editing.id, input: toInput(values, editing.plannedAt) as UpdateSecretRotationInput });
      form.resetFields();
      setEditing(null);
      void messageApi.success("Secret rotation updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update secret rotation");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless"><Space align="start" size={16}><div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #1d4ed8, #0f766e)", color: "#fff" }}><KeyOutlined /></div><div><Title level={3} style={{ margin: 0 }}>Secret Rotation Register</Title><Paragraph type="secondary" style={{ margin: "4px 0 0" }}>Plan, rotate, verify, and archive secret rotations with Keycloak, Redis, MinIO, or custom touchpoint evidence.</Paragraph></div></Space></Card>
      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search secret scope or verifier" style={{ width: 280 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
            <Select allowClear placeholder="Touchpoint" style={{ width: 180 }} options={touchpointOptions} value={filters.touchpoint} onChange={(value) => setFilters((current) => ({ ...current, touchpoint: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={statusOptions} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Input allowClear placeholder="Verified By" style={{ width: 220 }} value={filters.verifiedBy} onChange={(event) => setFilters((current) => ({ ...current, verifiedBy: event.target.value || undefined, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New rotation</Button>
        </Flex>
        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>
      <Modal title="Create secret rotation" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden><SecretRotationForm form={form} /></Modal>
      <Modal title="Edit secret rotation" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => {
        if (open && editing) {
          form.setFieldsValue({ touchpoint: editing.touchpoint, secretScope: editing.secretScope, evidenceRef: editing.evidenceRef ?? undefined, plannedAtDay: dayjs(editing.plannedAt), rotatedAtDay: editing.rotatedAt ? dayjs(editing.rotatedAt) : undefined, verifiedBy: editing.verifiedBy ?? undefined, verifiedAtDay: editing.verifiedAt ? dayjs(editing.verifiedAt) : undefined, status: editing.status });
        }
      }}><SecretRotationForm form={form} /></Modal>
    </Space>
  );
}

function SecretRotationForm({ form }: { form: FormInstance<SecretRotationFormValues> }) {
  return (
    <Form form={form} layout="vertical" initialValues={{ touchpoint: "keycloak", status: "planned" }}>
      <Form.Item label="Touchpoint" name="touchpoint" rules={[{ required: true, message: "Touchpoint is required." }]}><Select options={touchpointOptions} /></Form.Item>
      <Form.Item label="Secret Scope" name="secretScope" rules={[{ required: true, message: "Secret scope is required." }]}><Input /></Form.Item>
      <Form.Item label="Evidence Reference" name="evidenceRef"><Input placeholder="ops://rotation/keycloak-admin-client-2026-03" /></Form.Item>
      <Form.Item label="Planned At" name="plannedAtDay" rules={[{ required: true, message: "Planned time is required." }]}><DatePicker showTime style={{ width: "100%" }} /></Form.Item>
      <Form.Item label="Rotated At" name="rotatedAtDay"><DatePicker showTime style={{ width: "100%" }} /></Form.Item>
      <Form.Item label="Verified By" name="verifiedBy"><Input /></Form.Item>
      <Form.Item label="Verified At" name="verifiedAtDay"><DatePicker showTime style={{ width: "100%" }} /></Form.Item>
      <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}><Select options={statusOptions} /></Form.Item>
    </Form>
  );
}
