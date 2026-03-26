import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { SafetyCertificateOutlined, PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { useProjectOptions } from "../../users";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateUatSignoff, useUatActions, useUatSignoff, useUatSignoffs } from "../hooks/useVerification";
import type { UatSignoffFormInput, UatSignoffListItem } from "../types/verification";

const { Title, Paragraph, Text } = Typography;

export function UatSignoffPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.verification.read, permissions.verification.submitUat, permissions.verification.approve);
  const canSubmit = permissionState.hasPermission(permissions.verification.submitUat);
  const canApprove = permissionState.hasPermission(permissions.verification.approve);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ projectId: undefined as string | undefined, status: undefined as string | undefined, submittedBy: undefined as string | undefined, page: 1, pageSize: 10 });
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [form] = Form.useForm<UatSignoffFormInput>();

  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 20 });
  const query = useUatSignoffs(filters, canRead);
  const detailQuery = useUatSignoff(selectedId, canRead);
  const createMutation = useCreateUatSignoff();
  const actions = useUatActions();

  const columns = useMemo<ColumnsType<UatSignoffListItem>>(
    () => [
      { title: "Project", dataIndex: "projectName", key: "projectName" },
      { title: "Release", dataIndex: "releaseId", key: "releaseId", render: (value) => value ?? <Text type="secondary">pending</Text> },
      { title: "Status", dataIndex: "status", key: "status", render: (value) => <Tag>{value}</Tag> },
      { title: "Submitted By", dataIndex: "submittedBy", key: "submittedBy", render: (value) => value ?? <Text type="secondary">draft</Text> },
      { title: "Evidence", dataIndex: "evidenceCount", key: "evidenceCount" },
      { title: "Updated", dataIndex: "updatedAt", key: "updatedAt", render: (value) => dayjs(value).format("YYYY-MM-DD HH:mm") },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => (
          <Flex gap={8} wrap>
            <Button size="small" onClick={() => setSelectedId(item.id)}>Detail</Button>
            <Button size="small" disabled={!canSubmit || item.status !== "draft"} onClick={() => void runAction(() => actions.submit.mutateAsync(item.id), "UAT submitted.")}>Submit</Button>
            <Button size="small" disabled={!canApprove || item.status !== "submitted"} onClick={() => void runAction(() => actions.approve.mutateAsync({ id: item.id, input: { reason: "Approved from register" } }), "UAT approved.")}>Approve</Button>
            <Button size="small" disabled={!canApprove || item.status !== "submitted"} onClick={() => void runAction(() => actions.reject.mutateAsync({ id: item.id, input: { reason: "Rejected from register" } }), "UAT rejected.")}>Reject</Button>
          </Flex>
        ),
      },
    ],
    [actions.approve, actions.reject, actions.submit, canApprove, canSubmit],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="UAT access is not available for this account." />;
  }

  const runAction = async (action: () => Promise<unknown>, successMessage: string) => {
    try {
      await action();
      void messageApi.success(successMessage);
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update UAT sign-off");
      void messageApi.error(presentation.description);
    }
  };

  const handleCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      form.resetFields();
      setIsCreateOpen(false);
      void messageApi.success("UAT sign-off draft created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create UAT sign-off");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #1e40af, #047857)", color: "#fff" }}>
            <SafetyCertificateOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>UAT Sign-off</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Capture release-scoped user acceptance decisions with evidence, approver traceability, and stable workflow controls.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Select allowClear showSearch placeholder="Project" style={{ width: 220 }} options={projectOptions.options} value={filters.projectId} onSearch={projectOptions.onSearch} onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["draft", "submitted", "approved", "rejected"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canSubmit} onClick={() => setIsCreateOpen(true)}>New UAT</Button>
        </Flex>

        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} onRow={(record) => ({ onClick: () => setSelectedId(record.id) })} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Card title="Selected UAT" variant="borderless">
        {detailQuery.data ? (
          <Space direction="vertical" size={12} style={{ width: "100%" }}>
            <Text strong>{detailQuery.data.projectName} · {detailQuery.data.releaseId ?? "pending release ref"}</Text>
            <Paragraph style={{ margin: 0 }}>{detailQuery.data.scopeSummary}</Paragraph>
            <Text type="secondary">Status: {detailQuery.data.status} · Evidence refs: {detailQuery.data.evidenceRefs.join(", ") || "none"}</Text>
            <Table rowKey="id" size="small" pagination={false} dataSource={detailQuery.data.history} columns={[
              { title: "Occurred", dataIndex: "occurredAt", render: (value: string) => dayjs(value).format("YYYY-MM-DD HH:mm") },
              { title: "Event", dataIndex: "eventType" },
              { title: "Reason", dataIndex: "reason", render: (value: string | null) => value ?? <Text type="secondary">-</Text> },
            ]} />
          </Space>
        ) : (
          <Alert type="info" showIcon message="Select a UAT sign-off to review detail and history." />
        )}
      </Card>

      <Modal title="Create UAT sign-off" open={isCreateOpen} onOk={() => void handleCreate()} onCancel={() => setIsCreateOpen(false)} okText="Create" confirmLoading={createMutation.isPending} destroyOnHidden>
        <Form form={form} layout="vertical">
          <Form.Item label="Project" name="projectId" rules={[{ required: true, message: "Project is required." }]}><Select showSearch options={projectOptions.options} onSearch={projectOptions.onSearch} /></Form.Item>
          <Form.Item label="Release Reference" name="releaseId"><Input placeholder="REL-2026.03.26" /></Form.Item>
          <Form.Item label="Scope" name="scopeSummary" rules={[{ required: true, message: "Scope is required." }]}><Input.TextArea rows={3} /></Form.Item>
          <Form.Item label="Evidence References" name="evidenceRefs"><Select mode="tags" tokenSeparators={[","]} placeholder="DOC-123, export-001" /></Form.Item>
          <Form.Item label="Decision Reason" name="decisionReason"><Input.TextArea rows={2} /></Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
