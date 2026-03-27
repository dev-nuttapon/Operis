import { useMemo, useState } from "react";
import { Alert, Button, Card, DatePicker, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { FormInstance } from "antd";
import type { ColumnsType } from "antd/es/table";
import { SafetyOutlined, CheckOutlined, PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import {
  useAccessRecertifications,
  useAddAccessRecertificationDecision,
  useCompleteAccessRecertification,
  useCreateAccessRecertification,
  useUpdateAccessRecertification,
} from "../hooks/useOperations";
import type {
  AccessRecertification,
  AddAccessRecertificationDecisionInput,
  CreateAccessRecertificationInput,
  UpdateAccessRecertificationInput,
} from "../types/operations";

const { Title, Paragraph, Text } = Typography;

const scopeTypeOptions = ["role", "group", "application", "system_area"].map((value) => ({ label: value, value }));
const statusOptions = ["planned", "in_review", "approved", "completed"].map((value) => ({ label: value, value }));
const decisionOptions = ["kept", "revoked", "adjusted"].map((value) => ({ label: value, value }));

type ScheduleFormValues = CreateAccessRecertificationInput & {
  status?: string;
  plannedAtDay?: dayjs.Dayjs;
  subjectUserIdsText?: string;
};

export function AccessRecertificationsPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(
    permissions.operations.read,
    permissions.operations.manage,
    permissions.operations.approve,
  );
  const canManage = permissionState.hasPermission(permissions.operations.manage);
  const canApprove = permissionState.hasPermission(permissions.operations.approve);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({
    search: "",
    scopeType: undefined as string | undefined,
    reviewOwnerUserId: undefined as string | undefined,
    status: undefined as string | undefined,
    plannedBefore: undefined as string | undefined,
    page: 1,
    pageSize: 25,
  });
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<AccessRecertification | null>(null);
  const [decisionTarget, setDecisionTarget] = useState<AccessRecertification | null>(null);
  const [form] = Form.useForm<ScheduleFormValues>();
  const [decisionForm] = Form.useForm<AddAccessRecertificationDecisionInput>();
  const query = useAccessRecertifications({ ...filters, sortBy: "plannedAt", sortOrder: "asc" }, canRead);
  const createMutation = useCreateAccessRecertification();
  const updateMutation = useUpdateAccessRecertification();
  const decisionMutation = useAddAccessRecertificationDecision();
  const completeMutation = useCompleteAccessRecertification();

  const columns = useMemo<ColumnsType<AccessRecertification>>(
    () => [
      { title: "Scope", key: "scope", render: (_, item) => `${item.scopeType}: ${item.scopeRef}` },
      { title: "Planned Date", dataIndex: "plannedAt", render: (value: string) => dayjs(value).format("YYYY-MM-DD") },
      { title: "Review Owner", dataIndex: "reviewOwnerUserId" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "completed" ? "green" : value === "approved" ? "blue" : value === "in_review" ? "gold" : "default"}>{value}</Tag> },
      { title: "Completed", dataIndex: "completedCount" },
      { title: "Pending", dataIndex: "pendingCount" },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => (
          <Flex gap={8} wrap>
            <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button>
            <Button size="small" disabled={!canManage || item.status === "completed"} onClick={() => setDecisionTarget(item)}>Decision</Button>
            <Button size="small" icon={<CheckOutlined />} disabled={!canApprove || item.status !== "approved"} onClick={() => void handleComplete(item.id)}>Complete</Button>
          </Flex>
        ),
      },
    ],
    [canApprove, canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Access recertification data is not available for this account." />;
  }

  const parseSubjects = (value?: string | null) =>
    value
      ?.split(/[\n,]+/)
      .map((item) => item.trim())
      .filter(Boolean) ?? [];

  const handleComplete = async (id: string) => {
    try {
      await completeMutation.mutateAsync(id);
      void messageApi.success("Access recertification completed.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to complete access recertification");
      void messageApi.error(presentation.description);
    }
  };

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync({
        scopeType: values.scopeType,
        scopeRef: values.scopeRef,
        plannedAt: (values.plannedAtDay ?? dayjs()).toISOString(),
        reviewOwnerUserId: values.reviewOwnerUserId,
        subjectUserIds: parseSubjects(values.subjectUserIdsText),
        exceptionNotes: values.exceptionNotes ?? null,
      });
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Access recertification created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create access recertification");
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
          scopeType: values.scopeType,
          scopeRef: values.scopeRef,
          plannedAt: (values.plannedAtDay ?? dayjs(editing.plannedAt)).toISOString(),
          reviewOwnerUserId: values.reviewOwnerUserId,
          status: values.status ?? editing.status,
          subjectUserIds: parseSubjects(values.subjectUserIdsText),
          exceptionNotes: values.exceptionNotes ?? null,
        } as UpdateAccessRecertificationInput,
      });
      form.resetFields();
      setEditing(null);
      void messageApi.success("Access recertification updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update access recertification");
      void messageApi.error(presentation.description);
    }
  };

  const submitDecision = async () => {
    if (!decisionTarget) return;
    const values = await decisionForm.validateFields();
    try {
      await decisionMutation.mutateAsync({ id: decisionTarget.id, input: values });
      decisionForm.resetFields();
      setDecisionTarget(null);
      void messageApi.success("Decision recorded.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to record decision");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0f766e, #1d4ed8)", color: "#fff" }}>
            <SafetyOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Access Recertification</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Schedule scoped recertification reviews, record subject decisions, and complete only when every subject has a traceable outcome.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search scope or owner" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
            <Select allowClear placeholder="Scope type" style={{ width: 180 }} options={scopeTypeOptions} value={filters.scopeType} onChange={(value) => setFilters((current) => ({ ...current, scopeType: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={statusOptions} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Input allowClear placeholder="Review owner" style={{ width: 220 }} value={filters.reviewOwnerUserId} onChange={(event) => setFilters((current) => ({ ...current, reviewOwnerUserId: event.target.value || undefined, page: 1 }))} />
            <DatePicker
              allowClear
              placeholder="Planned before"
              style={{ width: 180 }}
              value={filters.plannedBefore ? dayjs(filters.plannedBefore) : null}
              onChange={(value) =>
                setFilters((current) => ({
                  ...current,
                  plannedBefore: value ? value.endOf("day").toISOString() : undefined,
                  page: 1,
                }))
              }
            />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New recertification</Button>
        </Flex>

        <Table
          rowKey="id"
          loading={query.isLoading}
          columns={columns}
          dataSource={query.data?.items ?? []}
          expandable={{
            expandedRowRender: (item) => (
              <Space direction="vertical" size={8} style={{ width: "100%" }}>
                <Text strong>Subjects</Text>
                <Text>{item.subjectUserIds.length > 0 ? item.subjectUserIds.join(", ") : "-"}</Text>
                <Text strong>Decisions</Text>
                {item.decisions.length > 0 ? item.decisions.map((decision) => (
                  <Text key={decision.id}>{`${decision.subjectUserId}: ${decision.decision} (${decision.reason})`}</Text>
                )) : <Text type="secondary">No decisions recorded.</Text>}
                <Text strong>Exceptions</Text>
                <Text>{item.exceptionNotes || "-"}</Text>
              </Space>
            ),
          }}
          pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, defaultPageSize: 25, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }}
        />
      </Card>

      <Modal title="Create access recertification" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <RecertificationForm form={form} />
      </Modal>

      <Modal title="Edit access recertification" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => {
        if (open && editing) {
          form.setFieldsValue({
            scopeType: editing.scopeType,
            scopeRef: editing.scopeRef,
            plannedAtDay: dayjs(editing.plannedAt),
            reviewOwnerUserId: editing.reviewOwnerUserId,
            status: editing.status,
            subjectUserIdsText: editing.subjectUserIds.join(", "),
            exceptionNotes: editing.exceptionNotes ?? undefined,
          });
        }
      }}>
        <RecertificationForm form={form} includeStatus />
      </Modal>

      <Modal title="Record subject decision" open={Boolean(decisionTarget)} onOk={() => void submitDecision()} onCancel={() => { setDecisionTarget(null); decisionForm.resetFields(); }} confirmLoading={decisionMutation.isPending} destroyOnHidden>
        <Form form={decisionForm} layout="vertical">
          <Form.Item label="Subject User" name="subjectUserId" rules={[{ required: true, message: "Subject user is required." }]}>
            <Select showSearch options={(decisionTarget?.subjectUserIds ?? []).map((value) => ({ label: value, value }))} placeholder="Choose a scoped subject" />
          </Form.Item>
          <Form.Item label="Decision" name="decision" rules={[{ required: true, message: "Decision is required." }]}>
            <Select options={decisionOptions} />
          </Form.Item>
          <Form.Item label="Rationale" name="reason" rules={[{ required: true, message: "Rationale is required." }]}>
            <Input.TextArea rows={4} placeholder="State why access is kept, revoked, or adjusted." />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}

function RecertificationForm({ form, includeStatus = false }: { form: FormInstance<ScheduleFormValues>; includeStatus?: boolean }) {
  return (
    <Form form={form} layout="vertical">
      <Form.Item label="Scope Type" name="scopeType" rules={[{ required: true, message: "Scope type is required." }]}>
        <Select options={scopeTypeOptions} />
      </Form.Item>
      <Form.Item label="Scope Reference" name="scopeRef" rules={[{ required: true, message: "Scope reference is required." }]}>
        <Input placeholder="finance-approver" />
      </Form.Item>
      <Form.Item label="Planned Date" name="plannedAtDay" rules={[{ required: true, message: "Planned date is required." }]}>
        <DatePicker style={{ width: "100%" }} />
      </Form.Item>
      <Form.Item label="Review Owner" name="reviewOwnerUserId" rules={[{ required: true, message: "Review owner is required." }]}>
        <Input placeholder="review-owner@example.com" />
      </Form.Item>
      {includeStatus ? (
        <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}>
          <Select options={statusOptions} />
        </Form.Item>
      ) : null}
      <Form.Item label="Subjects" name="subjectUserIdsText" extra="Comma or line separated subject user ids.">
        <Input.TextArea rows={4} placeholder="user-1, user-2" />
      </Form.Item>
      <Form.Item label="Exceptions" name="exceptionNotes">
        <Input.TextArea rows={3} />
      </Form.Item>
    </Form>
  );
}
