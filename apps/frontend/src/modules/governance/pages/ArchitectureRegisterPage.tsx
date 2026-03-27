import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { FormInstance } from "antd";
import type { ColumnsType } from "antd/es/table";
import { ApartmentOutlined, PlusOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useProjectList } from "../../users/public";
import { useArchitectureRecords, useCreateArchitectureRecord, useUpdateArchitectureRecord } from "../hooks/useGovernance";
import type { ArchitectureRecord, ArchitectureRecordFormInput } from "../types/governance";

const { Title, Paragraph, Text } = Typography;

const architectureStatuses = ["draft", "reviewed", "approved", "active", "superseded"];
const architectureTypes = ["application", "solution", "security", "data", "integration"];

export function ArchitectureRegisterPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.governance.architectureRead, permissions.governance.architectureManage);
  const canManage = permissionState.hasPermission(permissions.governance.architectureManage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", status: undefined as string | undefined, projectId: undefined as string | undefined, architectureType: undefined as string | undefined, ownerUserId: undefined as string | undefined, page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<ArchitectureRecord | null>(null);
  const [form] = Form.useForm<ArchitectureRecordFormInput>();
  const query = useArchitectureRecords(filters, canRead);
  const projectsQuery = useProjectList({ page: 1, pageSize: 100 });
  const createMutation = useCreateArchitectureRecord();
  const updateMutation = useUpdateArchitectureRecord();

  const columns = useMemo<ColumnsType<ArchitectureRecord>>(
    () => [
      { title: "Title", dataIndex: "title" },
      { title: "Type", dataIndex: "architectureType" },
      { title: "Owner", dataIndex: "ownerUserId" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Current Version", dataIndex: "currentVersionId", render: (value) => value ?? "-" },
      { title: "Actions", key: "actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button> },
    ],
    [canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Architecture governance data is not available for this account." />;
  }

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      setCreateOpen(false);
      form.resetFields();
      void messageApi.success("Architecture record created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create architecture record");
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
      void messageApi.success("Architecture record updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update architecture record");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #1d4ed8, #0f766e)", color: "#fff" }}>
            <ApartmentOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Architecture Register</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Track governed architecture decisions, review readiness, and current version lineage before delivery gates move downstream.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search title or version" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
            <Select allowClear placeholder="Project" style={{ width: 220 }} options={(projectsQuery.data?.items ?? []).map((project) => ({ value: project.id, label: `${project.code} · ${project.name}` }))} value={filters.projectId} onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))} showSearch optionFilterProp="label" />
            <Select allowClear placeholder="Type" style={{ width: 180 }} options={architectureTypes.map((value) => ({ value, label: value }))} value={filters.architectureType} onChange={(value) => setFilters((current) => ({ ...current, architectureType: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={architectureStatuses.map((value) => ({ value, label: value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Input allowClear placeholder="Owner user id" style={{ width: 220 }} value={filters.ownerUserId} onChange={(event) => setFilters((current) => ({ ...current, ownerUserId: event.target.value || undefined, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New architecture record</Button>
        </Flex>

        <Table
          rowKey="id"
          loading={query.isLoading}
          columns={columns}
          dataSource={query.data?.items ?? []}
          expandable={{
            expandedRowRender: (item) => (
              <Space direction="vertical" size={8} style={{ width: "100%" }}>
                <Text strong>Project</Text>
                <Text>{item.projectName ?? item.projectId}</Text>
                <Text strong>Summary</Text>
                <Text>{item.summary || "-"}</Text>
                <Text strong>Security Impact</Text>
                <Text>{item.securityImpact || "-"}</Text>
                <Text strong>Evidence</Text>
                <Text>{item.evidenceRef || "-"}</Text>
              </Space>
            ),
          }}
          pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }}
        />
      </Card>

      <Modal title="Create architecture record" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <ArchitectureRecordForm form={form} projectOptions={(projectsQuery.data?.items ?? []).map((project) => ({ value: project.id, label: `${project.code} · ${project.name}` }))} />
      </Modal>

      <Modal title="Edit architecture record" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => { if (open && editing) form.setFieldsValue(editing); }}>
        <ArchitectureRecordForm form={form} projectOptions={(projectsQuery.data?.items ?? []).map((project) => ({ value: project.id, label: `${project.code} · ${project.name}` }))} />
      </Modal>
    </Space>
  );
}

function ArchitectureRecordForm({ form, projectOptions }: { form: FormInstance<ArchitectureRecordFormInput>; projectOptions: Array<{ value: string; label: string }> }) {
  return (
    <Form form={form} layout="vertical" initialValues={{ status: "draft", architectureType: "application" }}>
      <Form.Item label="Project" name="projectId" rules={[{ required: true, message: "Project is required." }]}>
        <Select options={projectOptions} showSearch optionFilterProp="label" />
      </Form.Item>
      <Form.Item label="Title" name="title" rules={[{ required: true, message: "Title is required." }]}>
        <Input />
      </Form.Item>
      <Form.Item label="Architecture Type" name="architectureType" rules={[{ required: true, message: "Architecture type is required." }]}>
        <Select options={architectureTypes.map((value) => ({ value, label: value }))} />
      </Form.Item>
      <Form.Item label="Owner User Id" name="ownerUserId" rules={[{ required: true, message: "Owner is required." }]}>
        <Input />
      </Form.Item>
      <Form.Item label="Current Version" name="currentVersionId">
        <Input placeholder="ARCH-V2" />
      </Form.Item>
      <Form.Item label="Summary" name="summary">
        <Input.TextArea rows={3} />
      </Form.Item>
      <Form.Item label="Security Impact" name="securityImpact">
        <Input.TextArea rows={3} />
      </Form.Item>
      <Form.Item label="Evidence Ref" name="evidenceRef">
        <Input placeholder="minio://architecture/reviews/..." />
      </Form.Item>
      <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}>
        <Select options={architectureStatuses.map((value) => ({ value, label: value }))} />
      </Form.Item>
    </Form>
  );
}
