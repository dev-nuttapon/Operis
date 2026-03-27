import { useMemo, useState } from "react";
import { Alert, Button, Card, DatePicker, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { ReloadOutlined, PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useBackupEvidence, useCreateRestoreVerification, useRestoreVerifications } from "../hooks/useOperations";
import type { CreateRestoreVerificationInput, RestoreVerification } from "../types/operations";

const { Title, Paragraph } = Typography;
type RestoreVerificationFormValues = Omit<CreateRestoreVerificationInput, "executedAt"> & { executedAt: dayjs.Dayjs };

export function RestoreVerificationPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.operations.read);
  const canManage = permissionState.hasPermission(permissions.operations.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", backupEvidenceId: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [form] = Form.useForm<RestoreVerificationFormValues>();
  const query = useRestoreVerifications({ ...filters, sortBy: "executedAt", sortOrder: "desc" }, canRead);
  const backupQuery = useBackupEvidence({ page: 1, pageSize: 100, sortBy: "executedAt", sortOrder: "desc" }, canManage);
  const createMutation = useCreateRestoreVerification();

  const columns = useMemo<ColumnsType<RestoreVerification>>(
    () => [
      { title: "Backup Scope", dataIndex: "backupScope" },
      { title: "Executed At", dataIndex: "executedAt", render: (value) => new Date(value).toLocaleString() },
      { title: "Operator", dataIndex: "executedBy" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Result", dataIndex: "resultSummary" },
    ],
    [],
  );

  if (!canRead) return <Alert type="warning" showIcon message="Restore verification data is not available for this account." />;

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync({ ...values, executedAt: values.executedAt.toISOString() });
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Restore verification recorded.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to record restore verification");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #1d4ed8, #0f172a)", color: "#fff" }}>
            <ReloadOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Restore Verification</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Verify restore evidence against recorded backup runs and keep recovery proof reviewable for audit.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search scope or result" style={{ width: 260 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
            <Select allowClear placeholder="Backup run" style={{ width: 240 }} options={(backupQuery.data?.items ?? []).map((item) => ({ label: `${item.backupScope} · ${new Date(item.executedAt).toLocaleDateString()}`, value: item.id }))} value={filters.backupEvidenceId} onChange={(value) => setFilters((current) => ({ ...current, backupEvidenceId: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["planned", "executed", "verified", "closed"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New restore verification</Button>
        </Flex>

        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create restore verification" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <Form form={form} layout="vertical" initialValues={{ status: "executed" }}>
          <Form.Item label="Backup Evidence" name="backupEvidenceId" rules={[{ required: true, message: "Backup evidence is required." }]}>
            <Select options={(backupQuery.data?.items ?? []).map((item) => ({ label: `${item.backupScope} · ${new Date(item.executedAt).toLocaleString()}`, value: item.id }))} />
          </Form.Item>
          <Form.Item label="Executed At" name="executedAt" rules={[{ required: true, message: "Execution time is required." }]}>
            <DatePicker showTime style={{ width: "100%" }} />
          </Form.Item>
          <Form.Item label="Operator" name="executedBy" rules={[{ required: true, message: "Operator is required." }]}>
            <Input placeholder="ops@example.com" />
          </Form.Item>
          <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}>
            <Select options={["planned", "executed", "verified", "closed"].map((value) => ({ label: value, value }))} />
          </Form.Item>
          <Form.Item label="Result Summary" name="resultSummary" rules={[{ required: true, message: "Result summary is required." }]}>
            <Input.TextArea rows={4} placeholder="Restore completed to recovery target and verification checks passed." />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
