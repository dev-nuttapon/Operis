import { useMemo, useState } from "react";
import { Alert, Button, Card, DatePicker, Flex, Form, Input, InputNumber, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { AuditOutlined, PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useConfigurationAudits, useCreateConfigurationAudit } from "../hooks/useOperations";
import type { ConfigurationAudit, CreateConfigurationAuditInput } from "../types/operations";

const { Title, Paragraph } = Typography;
type ConfigurationAuditFormValues = Omit<CreateConfigurationAuditInput, "plannedAt"> & { plannedAt: dayjs.Dayjs };

export function ConfigurationAuditsPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.operations.read);
  const canManage = permissionState.hasPermission(permissions.operations.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", scopeRef: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [form] = Form.useForm<ConfigurationAuditFormValues>();
  const query = useConfigurationAudits({ ...filters, sortBy: "plannedAt", sortOrder: "desc" }, canRead);
  const createMutation = useCreateConfigurationAudit();

  const columns = useMemo<ColumnsType<ConfigurationAudit>>(
    () => [
      { title: "Scope", dataIndex: "scopeRef" },
      { title: "Plan Date", dataIndex: "plannedAt", render: (value) => new Date(value).toLocaleDateString() },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Finding Count", dataIndex: "findingCount" },
    ],
    [],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Configuration audit data is not available for this account." />;
  }

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync({ ...values, plannedAt: values.plannedAt.toISOString() });
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Configuration audit created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create configuration audit");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #7c3aed, #0f172a)", color: "#fff" }}>
            <AuditOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Configuration Audit Log</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Record planned configuration audits, track open findings, and keep scope-based audit status visible to compliance teams.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search scope" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["Planned", "In Review", "Findings Issued", "Closed"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New configuration audit</Button>
        </Flex>

        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, defaultPageSize: 25, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create configuration audit" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <Form form={form} layout="vertical" initialValues={{ status: "Planned", findingCount: 0 }}>
          <Form.Item label="Scope Reference" name="scopeRef" rules={[{ required: true, message: "Scope reference is required." }]}>
            <Input placeholder="config-item-baseline-2026-03" />
          </Form.Item>
          <Form.Item label="Plan Date" name="plannedAt" rules={[{ required: true, message: "Plan date is required." }]}>
            <DatePicker style={{ width: "100%" }} />
          </Form.Item>
          <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}>
            <Select options={["Planned", "In Review", "Findings Issued", "Closed"].map((value) => ({ label: value, value }))} />
          </Form.Item>
          <Form.Item label="Finding Count" name="findingCount" rules={[{ required: true, message: "Finding count is required." }]}>
            <InputNumber min={0} style={{ width: "100%" }} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
