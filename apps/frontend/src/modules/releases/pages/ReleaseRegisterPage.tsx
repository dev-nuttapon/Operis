import { useMemo, useState } from "react";
import dayjs, { type Dayjs } from "dayjs";
import { Alert, Button, Card, DatePicker, Descriptions, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useProjectOptions } from "../../users";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateRelease, useRelease, useReleaseActions, useReleases, useUpdateRelease } from "../hooks/useReleases";
import type { ReleaseListItem } from "../types/releases";

const { Title, Paragraph, Text } = Typography;

interface ReleaseFormValues {
  projectId: string;
  releaseCode: string;
  title: string;
  plannedAt?: Dayjs | null;
}

export function ReleaseRegisterPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.releases.read);
  const canManage = permissionState.hasPermission(permissions.releases.manage);
  const canApprove = permissionState.hasPermission(permissions.releases.approve);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ projectId: undefined as string | undefined, status: undefined as string | undefined, search: undefined as string | undefined, page: 1, pageSize: 10 });
  const [selectedReleaseId, setSelectedReleaseId] = useState<string | null>(null);
  const [createOpen, setCreateOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<ReleaseListItem | null>(null);
  const [approveTarget, setApproveTarget] = useState<ReleaseListItem | null>(null);
  const [releaseTarget, setReleaseTarget] = useState<ReleaseListItem | null>(null);
  const [createForm] = Form.useForm<ReleaseFormValues>();
  const [editForm] = Form.useForm<Omit<ReleaseFormValues, "projectId" | "releaseCode">>();
  const [approveForm] = Form.useForm<{ reason?: string }>();
  const [releaseForm] = Form.useForm<{ overrideReason?: string }>();

  const releasesQuery = useReleases(filters, canRead);
  const releaseDetailQuery = useRelease(selectedReleaseId, canRead);
  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 50 });
  const createMutation = useCreateRelease();
  const updateMutation = useUpdateRelease();
  const releaseActions = useReleaseActions();

  const columns = useMemo<ColumnsType<ReleaseListItem>>(
    () => [
      { title: "Release Code", dataIndex: "releaseCode", key: "releaseCode" },
      { title: "Project", dataIndex: "projectName", key: "projectName" },
      { title: "Title", dataIndex: "title", key: "title" },
      { title: "Planned", dataIndex: "plannedAt", key: "plannedAt", render: (value?: string | null) => (value ? new Date(value).toLocaleString() : "-") },
      { title: "Released", dataIndex: "releasedAt", key: "releasedAt", render: (value?: string | null) => (value ? new Date(value).toLocaleString() : "-") },
      { title: "Checklist", key: "checklist", render: (_, item) => `${item.checklistCompleted}/${item.checklistTotal}` },
      { title: "Gate", dataIndex: "latestQualityGateResult", key: "latestQualityGateResult", render: (value?: string | null) => value ? <Tag color={value === "passed" ? "green" : value === "overridden" ? "gold" : "red"}>{value}</Tag> : "-" },
      { title: "Status", dataIndex: "status", key: "status", render: (value: string) => <Tag color={value === "released" ? "green" : value === "approved" ? "blue" : "default"}>{value}</Tag> },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => (
          <Space wrap>
            <Button size="small" onClick={() => setSelectedReleaseId(item.id)}>View</Button>
            <Button size="small" disabled={!canManage || item.status === "released" || item.status === "archived"} onClick={() => {
              setEditTarget(item);
              editForm.setFieldsValue({ title: item.title, plannedAt: item.plannedAt ? dayjs(item.plannedAt) : null });
            }}>
              Edit
            </Button>
            <Button size="small" disabled={!canApprove || item.status !== "draft"} onClick={() => setApproveTarget(item)}>Approve</Button>
            <Button size="small" type="primary" disabled={!canApprove || item.status !== "approved"} onClick={() => setReleaseTarget(item)}>Release</Button>
          </Space>
        ),
      },
    ],
    [canApprove, canManage, editForm],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Release management is not available for this account." />;
  }

  const handleCreate = async () => {
    const values = await createForm.validateFields();
    try {
      await createMutation.mutateAsync({
        projectId: values.projectId,
        releaseCode: values.releaseCode,
        title: values.title,
        plannedAt: values.plannedAt?.toISOString() ?? null,
      });
      createForm.resetFields();
      setCreateOpen(false);
      void messageApi.success("Release created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create release");
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
          title: values.title,
          plannedAt: values.plannedAt?.toISOString() ?? null,
        },
      });
      setEditTarget(null);
      editForm.resetFields();
      void messageApi.success("Release updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update release");
      void messageApi.error(presentation.description);
    }
  };

  const handleApprove = async () => {
    if (!approveTarget) return;
    const values = await approveForm.validateFields();
    try {
      await releaseActions.approve.mutateAsync({ id: approveTarget.id, input: { reason: values.reason ?? null } });
      setApproveTarget(null);
      approveForm.resetFields();
      void messageApi.success("Release approved.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to approve release");
      void messageApi.error(presentation.description);
    }
  };

  const handleRelease = async () => {
    if (!releaseTarget) return;
    const values = await releaseForm.validateFields();
    try {
      await releaseActions.execute.mutateAsync({ id: releaseTarget.id, input: { overrideReason: values.overrideReason ?? null } });
      setReleaseTarget(null);
      releaseForm.resetFields();
      void messageApi.success("Release executed.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to execute release");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Title level={3} style={{ margin: 0 }}>Release Register</Title>
        <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
          Govern release approvals, deployment readiness, and release execution with checklist and quality gate evidence.
        </Paragraph>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Select allowClear showSearch placeholder="Project" style={{ width: 240 }} options={projectOptions.options} value={filters.projectId} onSearch={projectOptions.onSearch} onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["draft", "approved", "released", "archived"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Input allowClear placeholder="Search release code or title" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value || undefined, page: 1 }))} />
          </Flex>
          <Button type="primary" disabled={!canManage} onClick={() => setCreateOpen(true)}>Create release</Button>
        </Flex>

        <Table rowKey="id" loading={releasesQuery.isLoading} columns={columns} dataSource={releasesQuery.data?.items ?? []} pagination={{ current: releasesQuery.data?.page ?? filters.page, pageSize: releasesQuery.data?.pageSize ?? filters.pageSize, total: releasesQuery.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      {releaseDetailQuery.data ? (
        <Card variant="borderless">
          <Flex justify="space-between" align="center" wrap="wrap" gap={12}>
            <Title level={4} style={{ margin: 0 }}>Release Detail</Title>
            <Button size="small" onClick={() => setSelectedReleaseId(null)}>Close</Button>
          </Flex>
          <Descriptions size="small" column={2} style={{ marginTop: 16 }}>
            <Descriptions.Item label="Release">{releaseDetailQuery.data.releaseCode}</Descriptions.Item>
            <Descriptions.Item label="Project">{releaseDetailQuery.data.projectName}</Descriptions.Item>
            <Descriptions.Item label="Status">{releaseDetailQuery.data.status}</Descriptions.Item>
            <Descriptions.Item label="Quality Gate">{releaseDetailQuery.data.qualityGateResult ?? "-"}</Descriptions.Item>
            <Descriptions.Item label="Approved By">{releaseDetailQuery.data.approvedByUserId ?? "-"}</Descriptions.Item>
            <Descriptions.Item label="Override Reason">{releaseDetailQuery.data.qualityGateOverrideReason ?? "-"}</Descriptions.Item>
          </Descriptions>
          <Paragraph type="secondary" style={{ marginTop: 16 }}>
            Checklist items: {releaseDetailQuery.data.checklistItems.length} · Release notes: {releaseDetailQuery.data.notes.length}
          </Paragraph>
          <Space direction="vertical" size={6}>
            {releaseDetailQuery.data.checklistItems.slice(0, 5).map((item) => (
              <Text key={item.id}>{item.checklistItem} · {item.status}</Text>
            ))}
          </Space>
        </Card>
      ) : null}

      <Modal title="Create release" open={createOpen} onOk={() => void handleCreate()} onCancel={() => setCreateOpen(false)} confirmLoading={createMutation.isPending} destroyOnHidden>
        <Form form={createForm} layout="vertical">
          <Form.Item label="Project" name="projectId" rules={[{ required: true, message: "Project is required." }]}>
            <Select showSearch options={projectOptions.options} onSearch={projectOptions.onSearch} />
          </Form.Item>
          <Form.Item label="Release Code" name="releaseCode" rules={[{ required: true, message: "Release code is required." }]}>
            <Input placeholder="REL-2026.03.27" />
          </Form.Item>
          <Form.Item label="Title" name="title" rules={[{ required: true, message: "Title is required." }]}>
            <Input placeholder="Q1 maintenance release" />
          </Form.Item>
          <Form.Item label="Planned Date" name="plannedAt">
            <DatePicker showTime style={{ width: "100%" }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal title="Edit release" open={Boolean(editTarget)} onOk={() => void handleUpdate()} onCancel={() => setEditTarget(null)} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => {
        if (open && editTarget) {
          editForm.setFieldsValue({ title: editTarget.title, plannedAt: editTarget.plannedAt ? dayjs(editTarget.plannedAt) : null });
        }
      }}>
        <Form form={editForm} layout="vertical">
          <Form.Item label="Title" name="title" rules={[{ required: true, message: "Title is required." }]}>
            <Input />
          </Form.Item>
          <Form.Item label="Planned Date" name="plannedAt">
            <DatePicker showTime style={{ width: "100%" }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal title="Approve release" open={Boolean(approveTarget)} onOk={() => void handleApprove()} onCancel={() => setApproveTarget(null)} confirmLoading={releaseActions.approve.isPending} destroyOnHidden>
        <Form form={approveForm} layout="vertical">
          <Form.Item label="Approval reason" name="reason">
            <Input.TextArea rows={4} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal title="Execute release" open={Boolean(releaseTarget)} onOk={() => void handleRelease()} onCancel={() => setReleaseTarget(null)} confirmLoading={releaseActions.execute.isPending} destroyOnHidden>
        <Form form={releaseForm} layout="vertical">
          <Form.Item label="Override reason" name="overrideReason" extra="Leave blank when the release checklist is complete and the latest release_readiness gate passed.">
            <Input.TextArea rows={4} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
