import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateReleaseNote, usePublishReleaseNote, useReleaseNotes, useReleases } from "../hooks/useReleases";
import type { ReleaseNoteItem } from "../types/releases";

const { Title, Paragraph } = Typography;

export function ReleaseNotesPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.releases.read);
  const canManage = permissionState.hasPermission(permissions.releases.manage);
  const canApprove = permissionState.hasPermission(permissions.releases.approve);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ releaseId: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 10 });
  const [createOpen, setCreateOpen] = useState(false);
  const [createForm] = Form.useForm<{ releaseId: string; summary: string; includedChanges: string; knownIssues?: string }>();

  const notesQuery = useReleaseNotes(filters, canRead);
  const releaseOptionsQuery = useReleases({ page: 1, pageSize: 100 }, canRead);
  const createMutation = useCreateReleaseNote();
  const publishMutation = usePublishReleaseNote();

  const columns = useMemo<ColumnsType<ReleaseNoteItem>>(
    () => [
      { title: "Release", dataIndex: "releaseCode", key: "releaseCode" },
      { title: "Summary", dataIndex: "summary", key: "summary" },
      { title: "Included Changes", dataIndex: "includedChanges", key: "includedChanges", ellipsis: true },
      { title: "Known Issues", dataIndex: "knownIssues", key: "knownIssues", render: (value?: string | null) => value ?? "-" },
      { title: "Status", dataIndex: "status", key: "status", render: (value: string) => <Tag color={value === "published" ? "green" : "blue"}>{value}</Tag> },
      { title: "Published At", dataIndex: "publishedAt", key: "publishedAt", render: (value?: string | null) => (value ? new Date(value).toLocaleString() : "-") },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => (
          <Button size="small" disabled={!canApprove || item.status === "published"} onClick={() => void handlePublish(item.id)}>
            Publish
          </Button>
        ),
      },
    ],
    [canApprove],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Release notes are not available for this account." />;
  }

  const releaseOptions = (releaseOptionsQuery.data?.items ?? []).map((item) => ({ label: `${item.releaseCode} · ${item.title}`, value: item.id }));

  const handleCreate = async () => {
    const values = await createForm.validateFields();
    try {
      await createMutation.mutateAsync(values);
      createForm.resetFields();
      setCreateOpen(false);
      void messageApi.success("Release note created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create release note");
      void messageApi.error(presentation.description);
    }
  };

  async function handlePublish(id: string) {
    try {
      await publishMutation.mutateAsync(id);
      void messageApi.success("Release note published.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to publish release note");
      void messageApi.error(presentation.description);
    }
  }

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Title level={3} style={{ margin: 0 }}>Release Notes</Title>
        <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
          Publish governed release communication only after the underlying release has reached an approved state.
        </Paragraph>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Select allowClear showSearch placeholder="Release" style={{ width: 280 }} options={releaseOptions} value={filters.releaseId} onChange={(value) => setFilters((current) => ({ ...current, releaseId: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["draft", "published"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" disabled={!canManage} onClick={() => setCreateOpen(true)}>Create release note</Button>
        </Flex>

        <Table rowKey="id" loading={notesQuery.isLoading} columns={columns} dataSource={notesQuery.data?.items ?? []} pagination={{ current: notesQuery.data?.page ?? filters.page, pageSize: notesQuery.data?.pageSize ?? filters.pageSize, total: notesQuery.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create release note" open={createOpen} onOk={() => void handleCreate()} onCancel={() => setCreateOpen(false)} confirmLoading={createMutation.isPending} destroyOnHidden>
        <Form form={createForm} layout="vertical">
          <Form.Item label="Release" name="releaseId" rules={[{ required: true, message: "Release is required." }]}>
            <Select showSearch options={releaseOptions} />
          </Form.Item>
          <Form.Item label="Summary" name="summary" rules={[{ required: true, message: "Summary is required." }]}>
            <Input.TextArea rows={3} />
          </Form.Item>
          <Form.Item label="Included Changes" name="includedChanges" rules={[{ required: true, message: "Included changes are required." }]}>
            <Input.TextArea rows={4} />
          </Form.Item>
          <Form.Item label="Known Issues" name="knownIssues">
            <Input.TextArea rows={3} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
