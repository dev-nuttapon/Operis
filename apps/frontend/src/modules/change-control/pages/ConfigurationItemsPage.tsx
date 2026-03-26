import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { DeploymentUnitOutlined, PlusOutlined } from "@ant-design/icons";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useProjectOptions } from "../../users";
import { useApproveConfigurationItem, useConfigurationItems, useCreateConfigurationItem } from "../hooks/useChangeControl";
import type { ConfigurationItem, ConfigurationItemFormInput } from "../types/changeControl";

const { Title, Paragraph } = Typography;

export function ConfigurationItemsPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.changeControl.readConfiguration);
  const canManage = permissionState.hasPermission(permissions.changeControl.manageConfiguration);
  const [messageApi, contextHolder] = message.useMessage();
  const [form] = Form.useForm<ConfigurationItemFormInput>();
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [filters, setFilters] = useState({ search: "", projectId: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 10 });
  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 20 });
  const query = useConfigurationItems(filters, canRead);
  const createMutation = useCreateConfigurationItem();
  const approveMutation = useApproveConfigurationItem();

  const columns = useMemo<ColumnsType<ConfigurationItem>>(
    () => [
      { title: "Code", dataIndex: "code", key: "code" },
      { title: "Name", dataIndex: "name", key: "name" },
      { title: "Project", dataIndex: "projectName", key: "projectName" },
      { title: "Type", dataIndex: "itemType", key: "itemType", render: (value) => <Tag>{value}</Tag> },
      { title: "Owner Module", dataIndex: "ownerModule", key: "ownerModule" },
      { title: "Status", dataIndex: "status", key: "status", render: (value) => <Tag color={value === "baseline" ? "blue" : value === "approved" ? "green" : "default"}>{value}</Tag> },
      { title: "Baseline Ref", dataIndex: "baselineRef", key: "baselineRef", render: (value) => value ?? "-" },
      { title: "Updated", dataIndex: "updatedAt", key: "updatedAt", render: (value) => new Date(value).toLocaleString() },
      { title: "Actions", key: "actions", render: (_, item) => <Button size="small" disabled={!canManage || item.status !== "draft"} onClick={() => void approveMutation.mutateAsync(item.id)}>Approve</Button> },
    ],
    [approveMutation, canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Configuration item access is not available for this account." />;
  }

  const handleCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      void messageApi.success("Configuration item created.");
      setIsCreateOpen(false);
      form.resetFields();
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create configuration item");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #065f46, #0f172a)", color: "#fff" }}>
            <DeploymentUnitOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Configuration Items</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>Controlled configuration inventory with owner module and governed baseline reference.</Paragraph>
          </div>
        </Space>
      </Card>
      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search code, name, or module" style={{ width: 260 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
            <Select allowClear showSearch placeholder="Project" style={{ width: 220 }} options={projectOptions.options} onSearch={projectOptions.onSearch} onPopupScroll={(event) => { const target = event.target as HTMLDivElement; if (target.scrollTop + target.clientHeight >= target.scrollHeight - 24) { projectOptions.onLoadMore(); } }} onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["draft", "approved", "baseline", "superseded"].map((value) => ({ label: value, value }))} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setIsCreateOpen(true)}>New CI</Button>
        </Flex>
        <Table rowKey="id" loading={query.isLoading} dataSource={query.data?.items ?? []} columns={columns} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal open={isCreateOpen} title="Create configuration item" onOk={() => void handleCreate()} onCancel={() => setIsCreateOpen(false)} confirmLoading={createMutation.isPending}>
        <Form form={form} layout="vertical">
          <Form.Item label="Project" name="projectId" rules={[{ required: true }]}><Select showSearch options={projectOptions.options} onSearch={projectOptions.onSearch} onPopupScroll={(event) => { const target = event.target as HTMLDivElement; if (target.scrollTop + target.clientHeight >= target.scrollHeight - 24) { projectOptions.onLoadMore(); } }} /></Form.Item>
          <Form.Item label="Code" name="code" rules={[{ required: true }]}><Input placeholder="CI-001" /></Form.Item>
          <Form.Item label="Name" name="name" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item label="Type" name="itemType" rules={[{ required: true }]}><Input placeholder="application" /></Form.Item>
          <Form.Item label="Owner Module" name="ownerModule" rules={[{ required: true }]}><Input placeholder="documents" /></Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
