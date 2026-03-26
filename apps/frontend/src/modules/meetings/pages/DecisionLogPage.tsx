import { useMemo, useState } from "react";
import { Alert, Button, Card, Checkbox, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { AuditOutlined, PlusOutlined } from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useProjectOptions } from "../../users";
import { useCreateDecision, useDecisionActions, useDecisions } from "../hooks/useMeetings";
import type { DecisionFormInput, DecisionListItem } from "../types/meetings";

const { Title, Paragraph, Text } = Typography;

export function DecisionLogPage() {
  const navigate = useNavigate();
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.meetings.read);
  const canManage = permissionState.hasPermission(permissions.meetings.manage);
  const canApprove = permissionState.hasPermission(permissions.meetings.approve);
  const canReadRestricted = permissionState.hasPermission(permissions.meetings.readRestricted);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", projectId: undefined as string | undefined, decisionType: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 10 });
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [form] = Form.useForm<DecisionFormInput>();

  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 20 });
  const decisionsQuery = useDecisions(filters, canRead);
  const createMutation = useCreateDecision();
  const actions = useDecisionActions();

  const columns = useMemo<ColumnsType<DecisionListItem>>(
    () => [
      { title: "Code", dataIndex: "code", key: "code" },
      { title: "Title", dataIndex: "title", key: "title" },
      { title: "Type", dataIndex: "decisionType", key: "decisionType" },
      { title: "Approved By", dataIndex: "approvedBy", key: "approvedBy", render: (value) => value ?? <Text type="secondary">pending</Text> },
      { title: "Status", dataIndex: "status", key: "status", render: (value) => <Tag>{value}</Tag> },
      { title: "Meeting", dataIndex: "meetingTitle", key: "meetingTitle", render: (value) => value ?? <Text type="secondary">standalone</Text> },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => (
          <Flex gap={8} wrap>
            <Button size="small" onClick={() => navigate(`/app/decisions/${item.id}`)}>View</Button>
            <Button size="small" disabled={!canApprove || item.status !== "proposed"} onClick={() => void actions.approve.mutateAsync({ id: item.id, input: { reason: "Approved from log" } })}>Approve</Button>
            <Button size="small" disabled={!canManage || item.status !== "approved"} onClick={() => void actions.apply.mutateAsync({ id: item.id, input: { reason: "Applied from log" } })}>Apply</Button>
          </Flex>
        ),
      },
    ],
    [actions.apply, actions.approve, canApprove, canManage, navigate],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Decision access is not available for this account." />;
  }

  const handleCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync({
        ...values,
        meetingId: values.meetingId ? values.meetingId : undefined,
        classification: values.isRestricted ? values.classification ?? undefined : undefined,
      });
      form.resetFields();
      setIsCreateOpen(false);
      void messageApi.success("Decision created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create decision");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #1d4ed8, #0f172a)", color: "#fff" }}>
            <AuditOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Decision Log</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Capture approved rationale and applied decisions linked to meetings, requirements, and change items.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search code or title" style={{ width: 260 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
            <Select allowClear showSearch placeholder="Project" style={{ width: 220 }} options={projectOptions.options} value={filters.projectId} onSearch={projectOptions.onSearch} onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))} />
            <Select allowClear placeholder="Decision Type" style={{ width: 180 }} options={["approval", "architecture", "governance", "change"].map((value) => ({ label: value, value }))} value={filters.decisionType} onChange={(value) => setFilters((current) => ({ ...current, decisionType: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 160 }} options={["proposed", "approved", "applied", "archived"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Flex gap={8}>
            <Button onClick={() => navigate("/app/meetings")}>MOM Register</Button>
            <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setIsCreateOpen(true)}>New decision</Button>
          </Flex>
        </Flex>

        <Table rowKey="id" loading={decisionsQuery.isLoading} columns={columns} dataSource={decisionsQuery.data?.items ?? []} pagination={{ current: decisionsQuery.data?.page ?? filters.page, pageSize: decisionsQuery.data?.pageSize ?? filters.pageSize, total: decisionsQuery.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create decision" open={isCreateOpen} onOk={() => void handleCreate()} onCancel={() => setIsCreateOpen(false)} okText="Create" confirmLoading={createMutation.isPending} destroyOnHidden>
        <Form form={form} layout="vertical" initialValues={{ decisionType: "approval", isRestricted: false }}>
          <Form.Item label="Project" name="projectId" rules={[{ required: true, message: "Project is required." }]}><Select showSearch options={projectOptions.options} onSearch={projectOptions.onSearch} /></Form.Item>
          <Form.Item label="Meeting Id" name="meetingId"><Input placeholder="optional meeting GUID" /></Form.Item>
          <Form.Item label="Code" name="code" rules={[{ required: true, message: "Code is required." }]}><Input /></Form.Item>
          <Form.Item label="Title" name="title" rules={[{ required: true, message: "Title is required." }]}><Input /></Form.Item>
          <Form.Item label="Decision Type" name="decisionType" rules={[{ required: true, message: "Decision type is required." }]}><Select options={["approval", "architecture", "governance", "change"].map((value) => ({ label: value, value }))} /></Form.Item>
          <Form.Item label="Rationale" name="rationale" rules={[{ required: true, message: "Rationale is required." }]}><Input.TextArea rows={4} /></Form.Item>
          <Form.Item label="Alternatives Considered" name="alternativesConsidered"><Input.TextArea rows={3} /></Form.Item>
          <Form.Item label="Impacted Artifacts" name="impactedArtifacts"><Select mode="tags" tokenSeparators={[","]} placeholder="requirement:REQ-1, baseline:BL-1" /></Form.Item>
          {canReadRestricted ? (
            <>
              <Form.Item name="isRestricted" valuePropName="checked"><Checkbox>Restricted decision</Checkbox></Form.Item>
              <Form.Item shouldUpdate noStyle>
                {() => form.getFieldValue("isRestricted") ? <Form.Item label="Classification" name="classification" rules={[{ required: true, message: "Classification is required." }]}><Input placeholder="confidential" /></Form.Item> : null}
              </Form.Item>
            </>
          ) : null}
        </Form>
      </Modal>
    </Space>
  );
}
