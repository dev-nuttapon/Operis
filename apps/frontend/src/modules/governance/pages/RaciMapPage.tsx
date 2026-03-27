import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { FormInstance } from "antd";
import { ApartmentOutlined, PlusOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateRaciMap, useRaciMaps, useUpdateRaciMap } from "../hooks/useGovernance";
import type { RaciMap, RaciMapFormInput } from "../types/governance";

const { Title, Paragraph } = Typography;

export function RaciMapPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.governance.raciRead);
  const canManage = permissionState.hasPermission(permissions.governance.raciManage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", processCode: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 25 });
  const [editing, setEditing] = useState<RaciMap | null>(null);
  const [createOpen, setCreateOpen] = useState(false);
  const [form] = Form.useForm<RaciMapFormInput>();
  const query = useRaciMaps(filters, canRead);
  const createMutation = useCreateRaciMap();
  const updateMutation = useUpdateRaciMap();

  const columns = useMemo<ColumnsType<RaciMap>>(
    () => [
      { title: "Process", dataIndex: "processCode" },
      { title: "Role", dataIndex: "roleName" },
      { title: "R/A/C/I", dataIndex: "responsibilityType", render: (value: string) => <Tag color="blue">{value}</Tag> },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "active" ? "green" : value === "approved" ? "gold" : "default"}>{value}</Tag> },
      { title: "Actions", key: "actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button> },
    ],
    [canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="RACI map access is not available for this account." />;
  }

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      setCreateOpen(false);
      form.resetFields();
      void messageApi.success("RACI entry created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create RACI entry");
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
      void messageApi.success("RACI entry updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update RACI entry");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0f766e, #155e75)", color: "#fff" }}>
            <ApartmentOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>RACI Map</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Define who is responsible, accountable, consulted, and informed for governed processes.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search process or role" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["draft", "approved", "active", "archived"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New RACI entry</Button>
        </Flex>

        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create RACI entry" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <RaciMapForm form={form} />
      </Modal>

      <Modal title="Edit RACI entry" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => { if (open && editing) form.setFieldsValue(editing); }}>
        <RaciMapForm form={form} />
      </Modal>
    </Space>
  );
}

function RaciMapForm({ form }: { form: FormInstance<RaciMapFormInput> }) {
  return (
    <Form form={form} layout="vertical" initialValues={{ status: "draft", responsibilityType: "R" }}>
      <Form.Item label="Process Code" name="processCode" rules={[{ required: true, message: "Process code is required." }]}>
        <Input placeholder="REQ-CHANGE-CONTROL" />
      </Form.Item>
      <Form.Item label="Role Name" name="roleName" rules={[{ required: true, message: "Role name is required." }]}>
        <Input placeholder="Project Manager" />
      </Form.Item>
      <Form.Item label="Responsibility Type" name="responsibilityType" rules={[{ required: true, message: "R/A/C/I is required." }]}>
        <Select options={["R", "A", "C", "I"].map((value) => ({ label: value, value }))} />
      </Form.Item>
      <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}>
        <Select options={["draft", "approved", "active", "archived"].map((value) => ({ label: value, value }))} />
      </Form.Item>
      <Form.Item label="Reason" name="reason">
        <Input.TextArea rows={3} placeholder="Optional note for approvals or archival." />
      </Form.Item>
    </Form>
  );
}
