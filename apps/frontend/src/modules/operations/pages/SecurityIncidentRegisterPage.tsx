import { useMemo, useState } from "react";
import { Alert, Button, Card, DatePicker, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { FormInstance } from "antd";
import type { ColumnsType } from "antd/es/table";
import { PlusOutlined, SafetyCertificateOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateSecurityIncident, useSecurityIncidents, useUpdateSecurityIncident } from "../hooks/useOperations";
import type { CreateSecurityIncidentInput, SecurityIncident, UpdateSecurityIncidentInput } from "../types/operations";

const { Title, Paragraph } = Typography;
const severityOptions = ["low", "medium", "high", "critical"].map((value) => ({ label: value, value }));
const statusOptions = ["reported", "assessed", "contained", "resolved", "closed"].map((value) => ({ label: value, value }));

type IncidentFormValues = Omit<CreateSecurityIncidentInput, "reportedAt"> & { reportedAtDay?: dayjs.Dayjs };

export function SecurityIncidentRegisterPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.operations.read, permissions.operations.manage);
  const canManage = permissionState.hasPermission(permissions.operations.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", severity: undefined as string | undefined, status: undefined as string | undefined, ownerUserId: undefined as string | undefined, projectId: undefined as string | undefined, page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<SecurityIncident | null>(null);
  const [form] = Form.useForm<IncidentFormValues>();
  const query = useSecurityIncidents({ ...filters, sortBy: "reportedAt", sortOrder: "desc" }, canRead);
  const createMutation = useCreateSecurityIncident();
  const updateMutation = useUpdateSecurityIncident();

  const columns = useMemo<ColumnsType<SecurityIncident>>(
    () => [
      { title: "Code", dataIndex: "code" },
      { title: "Title", dataIndex: "title" },
      { title: "Severity", dataIndex: "severity", render: (value: string) => <Tag color={value === "critical" ? "red" : value === "high" ? "volcano" : value === "medium" ? "gold" : "default"}>{value}</Tag> },
      { title: "Owner", dataIndex: "ownerUserId" },
      { title: "Project", render: (_, item) => item.projectName ?? item.projectId ?? "-" },
      { title: "Reported", dataIndex: "reportedAt", render: (value: string) => dayjs(value).format("YYYY-MM-DD HH:mm") },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button> },
    ],
    [canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Security incidents are not available for this account." />;
  }

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync({
        projectId: values.projectId || null,
        code: values.code,
        title: values.title,
        severity: values.severity,
        reportedAt: (values.reportedAtDay ?? dayjs()).toISOString(),
        ownerUserId: values.ownerUserId,
        status: values.status,
        resolutionSummary: values.resolutionSummary ?? null,
      });
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Security incident created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create security incident");
      void messageApi.error(presentation.description);
    }
  };

  const submitUpdate = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({
        id: editing.id,
        input: {
          projectId: values.projectId || null,
          code: values.code,
          title: values.title,
          severity: values.severity,
          reportedAt: (values.reportedAtDay ?? dayjs(editing.reportedAt)).toISOString(),
          ownerUserId: values.ownerUserId,
          status: values.status,
          resolutionSummary: values.resolutionSummary ?? null,
        } as UpdateSecurityIncidentInput,
      });
      form.resetFields();
      setEditing(null);
      void messageApi.success("Security incident updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update security incident");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #991b1b, #b45309)", color: "#fff" }}>
            <SafetyCertificateOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Security Incident Register</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>Track incident response through assessment, containment, resolution, and formal closure.</Paragraph>
          </div>
        </Space>
      </Card>
      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search code, title, owner, or project" style={{ width: 260 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
            <Select allowClear placeholder="Severity" style={{ width: 160 }} options={severityOptions} value={filters.severity} onChange={(value) => setFilters((current) => ({ ...current, severity: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 170 }} options={statusOptions} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Input allowClear placeholder="Owner User" style={{ width: 200 }} value={filters.ownerUserId} onChange={(event) => setFilters((current) => ({ ...current, ownerUserId: event.target.value || undefined, page: 1 }))} />
            <Input allowClear placeholder="Project Id" style={{ width: 220 }} value={filters.projectId} onChange={(event) => setFilters((current) => ({ ...current, projectId: event.target.value || undefined, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New incident</Button>
        </Flex>
        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>
      <Modal title="Create security incident" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <IncidentForm form={form} />
      </Modal>
      <Modal title="Edit security incident" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => {
        if (open && editing) {
          form.setFieldsValue({ projectId: editing.projectId ?? undefined, code: editing.code, title: editing.title, severity: editing.severity, reportedAtDay: dayjs(editing.reportedAt), ownerUserId: editing.ownerUserId, status: editing.status, resolutionSummary: editing.resolutionSummary ?? undefined });
        }
      }}>
        <IncidentForm form={form} />
      </Modal>
    </Space>
  );
}

function IncidentForm({ form }: { form: FormInstance<IncidentFormValues> }) {
  return (
    <Form form={form} layout="vertical" initialValues={{ severity: "medium", status: "reported" }}>
      <Form.Item label="Project Id" name="projectId"><Input placeholder="Optional project reference" /></Form.Item>
      <Form.Item label="Incident Code" name="code" rules={[{ required: true, message: "Incident code is required." }]}><Input /></Form.Item>
      <Form.Item label="Title" name="title" rules={[{ required: true, message: "Title is required." }]}><Input /></Form.Item>
      <Form.Item label="Severity" name="severity" rules={[{ required: true, message: "Severity is required." }]}><Select options={severityOptions} /></Form.Item>
      <Form.Item label="Reported At" name="reportedAtDay" rules={[{ required: true, message: "Reported time is required." }]}><DatePicker showTime style={{ width: "100%" }} /></Form.Item>
      <Form.Item label="Owner User" name="ownerUserId" rules={[{ required: true, message: "Owner is required." }]}><Input /></Form.Item>
      <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}><Select options={statusOptions} /></Form.Item>
      <Form.Item label="Resolution Summary" name="resolutionSummary"><Input.TextArea rows={4} placeholder="Required when closing the incident" /></Form.Item>
    </Form>
  );
}
