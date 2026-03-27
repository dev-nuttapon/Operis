import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateDeploymentChecklist, useDeploymentChecklists, useReleases, useUpdateDeploymentChecklist } from "../hooks/useReleases";
import type { DeploymentChecklistItem } from "../types/releases";

const { Title, Paragraph } = Typography;

const statusOptions = ["draft", "reviewed", "approved", "executed"].map((value) => ({ label: value, value }));

export function DeploymentChecklistPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.releases.read);
  const canManage = permissionState.hasPermission(permissions.releases.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ releaseId: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 10 });
  const [createOpen, setCreateOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<DeploymentChecklistItem | null>(null);
  const [createForm] = Form.useForm<DeploymentChecklistItem>();
  const [editForm] = Form.useForm<DeploymentChecklistItem>();

  const releaseOptionsQuery = useReleases({ page: 1, pageSize: 100 }, canRead);
  const checklistQuery = useDeploymentChecklists(filters, canRead);
  const createMutation = useCreateDeploymentChecklist();
  const updateMutation = useUpdateDeploymentChecklist();

  const columns = useMemo<ColumnsType<DeploymentChecklistItem>>(
    () => [
      { title: "Release", dataIndex: "releaseCode", key: "releaseCode" },
      { title: "Checklist Item", dataIndex: "checklistItem", key: "checklistItem" },
      { title: "Owner", dataIndex: "ownerUserId", key: "ownerUserId" },
      { title: "Status", dataIndex: "status", key: "status", render: (value: string) => <Tag color={value === "executed" ? "green" : value === "approved" ? "blue" : "default"}>{value}</Tag> },
      { title: "Completed At", dataIndex: "completedAt", key: "completedAt", render: (value?: string | null) => (value ? new Date(value).toLocaleString() : "-") },
      { title: "Evidence", dataIndex: "evidenceRef", key: "evidenceRef", render: (value?: string | null) => value ?? "-" },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditTarget(item)}>Edit</Button>,
      },
    ],
    [canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Deployment checklist access is not available for this account." />;
  }

  const releaseOptions = (releaseOptionsQuery.data?.items ?? []).map((item) => ({ label: `${item.releaseCode} · ${item.title}`, value: item.id }));

  const handleCreate = async () => {
    const values = await createForm.validateFields();
    try {
      await createMutation.mutateAsync({
        releaseId: values.releaseId,
        checklistItem: values.checklistItem,
        ownerUserId: values.ownerUserId,
        status: values.status,
        evidenceRef: values.evidenceRef ?? null,
      });
      createForm.resetFields();
      setCreateOpen(false);
      void messageApi.success("Checklist item created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create checklist item");
      void messageApi.error(presentation.description);
    }
  };

  const handleUpdate = async () => {
    if (!editTarget) return;
    const values = await editForm.validateFields();
    try {
      await updateMutation.mutateAsync({
        id: editTarget.id,
        input: {
          checklistItem: values.checklistItem,
          ownerUserId: values.ownerUserId,
          status: values.status,
          completedAt: values.status === "executed" ? new Date().toISOString() : null,
          evidenceRef: values.evidenceRef ?? null,
        },
      });
      editForm.resetFields();
      setEditTarget(null);
      void messageApi.success("Checklist item updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update checklist item");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Title level={3} style={{ margin: 0 }}>Deployment Checklist</Title>
        <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
          Track required deployment steps, owners, and execution evidence before release cutover.
        </Paragraph>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Select allowClear showSearch placeholder="Release" style={{ width: 280 }} options={releaseOptions} value={filters.releaseId} onChange={(value) => setFilters((current) => ({ ...current, releaseId: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={statusOptions} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" disabled={!canManage} onClick={() => setCreateOpen(true)}>Add checklist item</Button>
        </Flex>

        <Table rowKey="id" loading={checklistQuery.isLoading} columns={columns} dataSource={checklistQuery.data?.items ?? []} pagination={{ current: checklistQuery.data?.page ?? filters.page, pageSize: checklistQuery.data?.pageSize ?? filters.pageSize, total: checklistQuery.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create checklist item" open={createOpen} onOk={() => void handleCreate()} onCancel={() => setCreateOpen(false)} confirmLoading={createMutation.isPending} destroyOnHidden>
        <Form form={createForm} layout="vertical">
          <Form.Item label="Release" name="releaseId" rules={[{ required: true, message: "Release is required." }]}>
            <Select showSearch options={releaseOptions} />
          </Form.Item>
          <Form.Item label="Checklist Item" name="checklistItem" rules={[{ required: true, message: "Checklist item is required." }]}>
            <Input />
          </Form.Item>
          <Form.Item label="Owner User Id" name="ownerUserId" rules={[{ required: true, message: "Owner is required." }]}>
            <Input placeholder="ops@example.com" />
          </Form.Item>
          <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}>
            <Select options={statusOptions} />
          </Form.Item>
          <Form.Item label="Evidence Ref" name="evidenceRef">
            <Input placeholder="runbook://release-14/backup-proof" />
          </Form.Item>
        </Form>
      </Modal>

      <Modal title="Edit checklist item" open={Boolean(editTarget)} onOk={() => void handleUpdate()} onCancel={() => setEditTarget(null)} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => {
        if (open && editTarget) {
          editForm.setFieldsValue(editTarget);
        }
      }}>
        <Form form={editForm} layout="vertical">
          <Form.Item label="Checklist Item" name="checklistItem" rules={[{ required: true, message: "Checklist item is required." }]}>
            <Input />
          </Form.Item>
          <Form.Item label="Owner User Id" name="ownerUserId" rules={[{ required: true, message: "Owner is required." }]}>
            <Input />
          </Form.Item>
          <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}>
            <Select options={statusOptions} />
          </Form.Item>
          <Form.Item label="Evidence Ref" name="evidenceRef">
            <Input />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
