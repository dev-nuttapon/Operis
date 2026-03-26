import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { CheckCircleOutlined, PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { useNavigate } from "react-router-dom";
import { useProjectOptions } from "../../users";
import { useRequirements } from "../../requirements";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateTestPlan, useTestPlanActions, useTestPlans } from "../hooks/useVerification";
import type { TestPlanFormInput, TestPlanListItem } from "../types/verification";

const { Title, Paragraph, Text } = Typography;

export function TestPlanPage() {
  const navigate = useNavigate();
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.verification.read);
  const canManage = permissionState.hasPermission(permissions.verification.manage);
  const canApprove = permissionState.hasPermission(permissions.verification.approve);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", projectId: undefined as string | undefined, status: undefined as string | undefined, coverageStatus: undefined as string | undefined, page: 1, pageSize: 10 });
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [form] = Form.useForm<TestPlanFormInput>();

  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 20 });
  const requirementsQuery = useRequirements({ page: 1, pageSize: 100 }, canManage);
  const plansQuery = useTestPlans(filters, canRead);
  const createMutation = useCreateTestPlan();
  const actions = useTestPlanActions();

  const columns = useMemo<ColumnsType<TestPlanListItem>>(
    () => [
      {
        title: "Plan",
        key: "plan",
        render: (_, item) => (
          <Space direction="vertical" size={0}>
            <Text strong>{item.title}</Text>
            <Text type="secondary">{item.code}</Text>
          </Space>
        ),
      },
      { title: "Project", dataIndex: "projectName", key: "projectName" },
      { title: "Owner", dataIndex: "ownerUserId", key: "ownerUserId" },
      { title: "Status", dataIndex: "status", key: "status", render: (value) => <Tag>{value}</Tag> },
      {
        title: "Coverage",
        key: "coverage",
        render: (_, item) => (
          <Space direction="vertical" size={0}>
            <Tag color={item.coverageStatus === "complete" ? "green" : item.coverageStatus === "partial" ? "gold" : "red"}>{item.coverageStatus}</Tag>
            <Text type="secondary">{item.coveredRequirementCount}/{item.linkedRequirementCount} reqs covered</Text>
          </Space>
        ),
      },
      { title: "Updated", dataIndex: "updatedAt", key: "updatedAt", render: (value) => dayjs(value).format("YYYY-MM-DD HH:mm") },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => (
          <Flex gap={8} wrap>
            <Button size="small" onClick={() => navigate(`/app/test-cases?testPlanId=${item.id}`)}>Cases</Button>
            <Button size="small" disabled={!canManage || item.status !== "draft"} onClick={() => void runAction(() => actions.submit.mutateAsync(item.id), "Test plan submitted.")}>Submit</Button>
            <Button size="small" disabled={!canApprove || item.status !== "review"} onClick={() => void runAction(() => actions.approve.mutateAsync({ id: item.id, input: { reason: "Approved from register" } }), "Test plan approved.")}>Approve</Button>
            <Button size="small" disabled={!canApprove || item.status !== "approved"} onClick={() => void runAction(() => actions.baseline.mutateAsync({ id: item.id, input: { reason: "Baselined from register" } }), "Test plan baselined.")}>Baseline</Button>
          </Flex>
        ),
      },
    ],
    [actions.approve, actions.baseline, actions.submit, canApprove, canManage, navigate],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Verification access is not available for this account." />;
  }

  const runAction = async (action: () => Promise<unknown>, successMessage: string) => {
    try {
      await action();
      void messageApi.success(successMessage);
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update test plan");
      void messageApi.error(presentation.description);
    }
  };

  const handleCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      form.resetFields();
      setIsCreateOpen(false);
      void messageApi.success("Test plan created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create test plan");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #14532d, #1d4ed8)", color: "#fff" }}>
            <CheckCircleOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Test Plan</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Govern verification scope, linked requirement coverage, entry criteria, and baseline readiness.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search plan" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
            <Select allowClear showSearch placeholder="Project" style={{ width: 220 }} options={projectOptions.options} value={filters.projectId} onSearch={projectOptions.onSearch} onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 160 }} options={["draft", "review", "approved", "baseline"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Select allowClear placeholder="Coverage" style={{ width: 160 }} options={["missing", "partial", "complete"].map((value) => ({ label: value, value }))} value={filters.coverageStatus} onChange={(value) => setFilters((current) => ({ ...current, coverageStatus: value, page: 1 }))} />
          </Flex>
          <Flex gap={8}>
            <Button onClick={() => navigate("/app/test-cases")}>Test Case & Execution</Button>
            <Button onClick={() => navigate("/app/uat-signoffs")}>UAT Sign-off</Button>
            <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setIsCreateOpen(true)}>New plan</Button>
          </Flex>
        </Flex>

        <Table rowKey="id" loading={plansQuery.isLoading} columns={columns} dataSource={plansQuery.data?.items ?? []} pagination={{ current: plansQuery.data?.page ?? filters.page, pageSize: plansQuery.data?.pageSize ?? filters.pageSize, total: plansQuery.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create test plan" open={isCreateOpen} onOk={() => void handleCreate()} onCancel={() => setIsCreateOpen(false)} okText="Create" confirmLoading={createMutation.isPending} destroyOnHidden>
        <Form form={form} layout="vertical">
          <Form.Item label="Project" name="projectId" rules={[{ required: true, message: "Project is required." }]}><Select showSearch options={projectOptions.options} onSearch={projectOptions.onSearch} /></Form.Item>
          <Form.Item label="Code" name="code" rules={[{ required: true, message: "Code is required." }]}><Input placeholder="TP-001" /></Form.Item>
          <Form.Item label="Title" name="title" rules={[{ required: true, message: "Title is required." }]}><Input /></Form.Item>
          <Form.Item label="Owner" name="ownerUserId" rules={[{ required: true, message: "Owner is required." }]}><Input placeholder="qa@example.com" /></Form.Item>
          <Form.Item label="Scope" name="scopeSummary" rules={[{ required: true, message: "Scope is required." }]}><Input.TextArea rows={3} /></Form.Item>
          <Form.Item label="Entry Criteria" name="entryCriteria"><Input.TextArea rows={2} /></Form.Item>
          <Form.Item label="Exit Criteria" name="exitCriteria"><Input.TextArea rows={2} /></Form.Item>
          <Form.Item label="Linked Requirements" name="linkedRequirementIds">
            <Select mode="multiple" options={(requirementsQuery.data?.items ?? []).map((item) => ({ label: `${item.code} • ${item.title}`, value: item.id }))} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
