import { useMemo, useState } from "react";
import { Alert, Button, Card, Drawer, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { PlayCircleOutlined, PlusOutlined, SettingOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import {
  useAutomationJob,
  useAutomationJobs,
  useCreateAutomationJob,
  useExecuteAutomationJob,
  useTransitionAutomationJob,
  useUpdateAutomationJob,
} from "../hooks/useOperations";
import type { AutomationJob, CreateAutomationJobInput, ExecuteAutomationJobInput, UpdateAutomationJobInput } from "../types/operations";

const { Title, Paragraph, Text } = Typography;

const jobTypeOptions = ["backup", "retention", "export", "secret_rotation", "alert"].map((value) => ({ value, label: value }));
const jobStatusOptions = ["draft", "active", "paused", "retired"].map((value) => ({ value, label: value }));
const runStatusOptions = ["queued", "running", "succeeded", "failed"].map((value) => ({ value, label: value }));

export function OperationsAutomationPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.operations.automationRead, permissions.operations.automationManage, permissions.operations.automationExecute);
  const canManage = permissionState.hasPermission(permissions.operations.automationManage);
  const canExecute = permissionState.hasPermission(permissions.operations.automationExecute);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ jobType: undefined as string | undefined, status: undefined as string | undefined, scopeRef: undefined as string | undefined, search: "", page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<AutomationJob | null>(null);
  const [selectedJobId, setSelectedJobId] = useState<string | null>(null);
  const [transitionTarget, setTransitionTarget] = useState<"active" | "paused" | "retired" | null>(null);
  const [executeOpen, setExecuteOpen] = useState(false);
  const [form] = Form.useForm<CreateAutomationJobInput>();
  const [transitionForm] = Form.useForm<{ reason: string }>();
  const [executeForm] = Form.useForm<ExecuteAutomationJobInput>();

  const jobsQuery = useAutomationJobs(filters, canRead);
  const selectedJobQuery = useAutomationJob(selectedJobId ?? undefined, canRead && Boolean(selectedJobId));
  const createMutation = useCreateAutomationJob();
  const updateMutation = useUpdateAutomationJob();
  const transitionMutation = useTransitionAutomationJob();
  const executeMutation = useExecuteAutomationJob();

  const columns = useMemo<ColumnsType<AutomationJob>>(
    () => [
      { title: "Job", render: (_, item) => <Space direction="vertical" size={0}><Text strong>{item.jobName}</Text><Text type="secondary">{item.jobType}</Text></Space> },
      { title: "Scope", dataIndex: "scopeRef" },
      { title: "Schedule", dataIndex: "scheduleRef" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "active" ? "green" : value === "paused" ? "gold" : value === "retired" ? "default" : "blue"}>{value}</Tag> },
      { title: "Latest Run", render: (_, item) => item.latestRunStatus ? <Tag color={item.latestRunStatus === "failed" ? "red" : item.latestRunStatus === "succeeded" ? "green" : "blue"}>{item.latestRunStatus}</Tag> : "-" },
      { title: "Failure", dataIndex: "failureSummary", render: (value) => value ?? "-" },
      {
        title: "Actions",
        render: (_, item) => (
          <Space size={8}>
            <Button size="small" onClick={() => setSelectedJobId(item.id)}>Open</Button>
            <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button>
            <Button size="small" type="primary" icon={<PlayCircleOutlined />} disabled={!canExecute || item.status !== "active"} onClick={() => { setSelectedJobId(item.id); setExecuteOpen(true); }}>Execute</Button>
          </Space>
        ),
      },
    ],
    [canExecute, canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Operational automation is not available for this account." />;
  }

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Automation job created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create automation job");
      void messageApi.error(presentation.description);
    }
  };

  const submitUpdate = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({ id: editing.id, input: values as UpdateAutomationJobInput });
      form.resetFields();
      setEditing(null);
      void messageApi.success("Automation job updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update automation job");
      void messageApi.error(presentation.description);
    }
  };

  const submitTransition = async () => {
    if (!selectedJobId || !transitionTarget) return;
    const values = await transitionForm.validateFields();
    try {
      await transitionMutation.mutateAsync({ id: selectedJobId, input: { targetStatus: transitionTarget, reason: values.reason } });
      transitionForm.resetFields();
      setTransitionTarget(null);
      void messageApi.success(`Automation job moved to ${transitionTarget}.`);
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to transition automation job");
      void messageApi.error(presentation.description);
    }
  };

  const submitExecute = async () => {
    if (!selectedJobId) return;
    const values = await executeForm.validateFields();
    try {
      await executeMutation.mutateAsync({ id: selectedJobId, input: values });
      executeForm.resetFields();
      setExecuteOpen(false);
      void messageApi.success("Automation job run recorded.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to execute automation job");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #1d4ed8, #0f766e)", color: "#fff" }}>
            <SettingOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Operational Automation</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Govern recurring backup, retention, export, secret rotation, and alert jobs as auditable operational controls.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless" title="Automation Jobs" extra={<Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New job</Button>}>
        <Flex gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Select allowClear placeholder="Job Type" style={{ width: 180 }} options={jobTypeOptions} value={filters.jobType} onChange={(value) => setFilters((current) => ({ ...current, jobType: value, page: 1 }))} />
          <Select allowClear placeholder="Status" style={{ width: 180 }} options={jobStatusOptions} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          <Input allowClear placeholder="Scope Ref" style={{ width: 220 }} value={filters.scopeRef} onChange={(event) => setFilters((current) => ({ ...current, scopeRef: event.target.value || undefined, page: 1 }))} />
          <Input.Search allowClear placeholder="Search job or schedule" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
        </Flex>

        <Table rowKey="id" loading={jobsQuery.isLoading} columns={columns} dataSource={jobsQuery.data?.items ?? []} pagination={{ current: jobsQuery.data?.page ?? filters.page, pageSize: jobsQuery.data?.pageSize ?? filters.pageSize, total: jobsQuery.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create automation job" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <AutomationJobForm form={form} />
      </Modal>

      <Modal title="Edit automation job" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => { if (open && editing) form.setFieldsValue(editing); }}>
        <AutomationJobForm form={form} />
      </Modal>

      <Drawer title="Automation Job Detail" open={Boolean(selectedJobId)} width={720} onClose={() => setSelectedJobId(null)} destroyOnHidden>
        {selectedJobQuery.data ? (
          <Space direction="vertical" size={12} style={{ width: "100%" }}>
            <DetailCard label="Job Name" value={selectedJobQuery.data.jobName} />
            <DetailCard label="Job Type" value={selectedJobQuery.data.jobType} />
            <DetailCard label="Scope" value={selectedJobQuery.data.scopeRef} />
            <DetailCard label="Schedule" value={selectedJobQuery.data.scheduleRef} />
            <DetailCard label="Status" value={selectedJobQuery.data.status} />
            <DetailCard label="Latest Run Status" value={selectedJobQuery.data.latestRunStatus ?? "-"} />
            <DetailCard label="Failure Summary" value={selectedJobQuery.data.failureSummary ?? "-"} />
            <Flex gap={8} wrap="wrap">
              <Button disabled={!canManage || selectedJobQuery.data.status !== "draft"} onClick={() => setTransitionTarget("active")}>Activate</Button>
              <Button disabled={!canManage || selectedJobQuery.data.status !== "active"} onClick={() => setTransitionTarget("paused")}>Pause</Button>
              <Button disabled={!canManage || (selectedJobQuery.data.status !== "active" && selectedJobQuery.data.status !== "paused")} onClick={() => setTransitionTarget("retired")}>Retire</Button>
            </Flex>
          </Space>
        ) : null}
      </Drawer>

      <Modal title={`Transition automation job to ${transitionTarget ?? ""}`} open={Boolean(transitionTarget)} onOk={() => void submitTransition()} onCancel={() => { setTransitionTarget(null); transitionForm.resetFields(); }} confirmLoading={transitionMutation.isPending} destroyOnHidden>
        <Form form={transitionForm} layout="vertical">
          <Form.Item label="Reason" name="reason" rules={[{ required: true, message: "Reason is required." }]}>
            <Input.TextArea rows={4} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal title="Record automation run" open={executeOpen} onOk={() => void submitExecute()} onCancel={() => { setExecuteOpen(false); executeForm.resetFields(); }} confirmLoading={executeMutation.isPending} destroyOnHidden width={760}>
        <Form form={executeForm} layout="vertical" initialValues={{ initialStatus: "queued", evidenceRefs: [{ entityType: "", entityId: "", route: "", evidenceRef: "" }] }}>
          <Form.Item label="Run Status" name="initialStatus" rules={[{ required: true, message: "Run status is required." }]}>
            <Select options={runStatusOptions} />
          </Form.Item>
          <Form.Item label="Trigger Reason" name="triggerReason">
            <Input.TextArea rows={3} />
          </Form.Item>
          <Form.Item noStyle shouldUpdate={(previous, current) => previous.initialStatus !== current.initialStatus}>
            {({ getFieldValue }) => getFieldValue("initialStatus") === "failed" ? (
              <>
                <Form.Item label="Error Summary" name="errorSummary" rules={[{ required: true, message: "Error summary is required for failed runs." }]}>
                  <Input.TextArea rows={3} />
                </Form.Item>
                <Form.Item label="Remediation Path" name="remediationPath">
                  <Input />
                </Form.Item>
              </>
            ) : null}
          </Form.Item>
          <Form.List name="evidenceRefs">
            {(fields, { add, remove }) => (
              <Space direction="vertical" style={{ width: "100%" }}>
                {fields.map((field, index) => (
                  <Card key={field.key} size="small" title={`Evidence ${index + 1}`} extra={fields.length > 1 ? <Button size="small" onClick={() => remove(field.name)}>Remove</Button> : null}>
                    <Form.Item label="Entity Type" name={[field.name, "entityType"]} rules={[{ required: true, message: "Entity type is required." }]}>
                      <Input />
                    </Form.Item>
                    <Form.Item label="Entity Id" name={[field.name, "entityId"]} rules={[{ required: true, message: "Entity id is required." }]}>
                      <Input />
                    </Form.Item>
                    <Form.Item label="Route" name={[field.name, "route"]} rules={[{ required: true, message: "Route is required." }]}>
                      <Input />
                    </Form.Item>
                    <Form.Item label="Evidence Ref" name={[field.name, "evidenceRef"]} rules={[{ required: true, message: "Evidence ref is required." }]}>
                      <Input />
                    </Form.Item>
                  </Card>
                ))}
                <Button onClick={() => add({ entityType: "", entityId: "", route: "", evidenceRef: "" })}>Add evidence</Button>
              </Space>
            )}
          </Form.List>
        </Form>
      </Modal>
    </Space>
  );
}

function AutomationJobForm({ form }: { form: ReturnType<typeof Form.useForm<CreateAutomationJobInput>>[0] }) {
  return (
    <Form form={form} layout="vertical">
      <Form.Item label="Job Name" name="jobName" rules={[{ required: true, message: "Job name is required." }]}>
        <Input />
      </Form.Item>
      <Form.Item label="Job Type" name="jobType" rules={[{ required: true, message: "Job type is required." }]}>
        <Select options={jobTypeOptions} />
      </Form.Item>
      <Form.Item label="Scope Ref" name="scopeRef" rules={[{ required: true, message: "Scope ref is required." }]}>
        <Input />
      </Form.Item>
      <Form.Item label="Schedule Ref" name="scheduleRef" rules={[{ required: true, message: "Schedule ref is required." }]}>
        <Input />
      </Form.Item>
      <Form.Item label="Status" name="status" initialValue="draft" rules={[{ required: true, message: "Status is required." }]}>
        <Select options={jobStatusOptions} />
      </Form.Item>
    </Form>
  );
}

function DetailCard({ label, value }: { label: string; value: string }) {
  return (
    <Card size="small">
      <Space direction="vertical" size={2}>
        <Text type="secondary">{label}</Text>
        <Text>{value}</Text>
      </Space>
    </Card>
  );
}
