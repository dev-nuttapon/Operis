import { useMemo, useState } from "react";
import { Alert, Button, Card, DatePicker, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { DatabaseOutlined, PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useBackupEvidence, useCreateBackupEvidence } from "../hooks/useOperations";
import type { BackupEvidence, CreateBackupEvidenceInput } from "../types/operations";

const { Title, Paragraph } = Typography;
type BackupEvidenceFormValues = Omit<CreateBackupEvidenceInput, "executedAt"> & { executedAt: dayjs.Dayjs };

export function BackupEvidencePage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.operations.read);
  const canManage = permissionState.hasPermission(permissions.operations.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", backupScope: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [form] = Form.useForm<BackupEvidenceFormValues>();
  const query = useBackupEvidence({ ...filters, sortBy: "executedAt", sortOrder: "desc" }, canRead);
  const createMutation = useCreateBackupEvidence();

  const columns = useMemo<ColumnsType<BackupEvidence>>(
    () => [
      { title: "Scope", dataIndex: "backupScope" },
      { title: "Executed At", dataIndex: "executedAt", render: (value) => new Date(value).toLocaleString() },
      { title: "Operator", dataIndex: "executedBy" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Evidence", dataIndex: "evidenceRef", render: (value) => value ?? "-" },
    ],
    [],
  );

  if (!canRead) return <Alert type="warning" showIcon message="Backup evidence is not available for this account." />;

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync({ ...values, executedAt: values.executedAt.toISOString() });
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Backup evidence recorded.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to record backup evidence");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0f766e, #1d4ed8)", color: "#fff" }}>
            <DatabaseOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Backup Evidence</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Record governed PostgreSQL or MinIO backup execution evidence for audit and recovery review.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search scope or evidence" style={{ width: 260 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
            <Input allowClear placeholder="Backup scope" style={{ width: 200 }} value={filters.backupScope} onChange={(event) => setFilters((current) => ({ ...current, backupScope: event.target.value || undefined, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["planned", "completed", "verified", "archived"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New backup evidence</Button>
        </Flex>

        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create backup evidence" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <Form form={form} layout="vertical" initialValues={{ status: "completed" }}>
          <Form.Item label="Backup Scope" name="backupScope" rules={[{ required: true, message: "Backup scope is required." }]}>
            <Input placeholder="postgresql:operis" />
          </Form.Item>
          <Form.Item label="Executed At" name="executedAt" rules={[{ required: true, message: "Execution time is required." }]}>
            <DatePicker showTime style={{ width: "100%" }} />
          </Form.Item>
          <Form.Item label="Operator" name="executedBy" rules={[{ required: true, message: "Operator is required." }]}>
            <Input placeholder="ops@example.com" />
          </Form.Item>
          <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}>
            <Select options={["planned", "completed", "verified", "archived"].map((value) => ({ label: value, value }))} />
          </Form.Item>
          <Form.Item label="Evidence Ref" name="evidenceRef">
            <Input placeholder="minio://dr/backup-proof-2026-03-27.json" />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
