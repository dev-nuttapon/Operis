import { useMemo, useState } from "react";
import { Alert, Button, Card, Checkbox, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { ExperimentOutlined, PlusOutlined, UploadOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { useSearchParams } from "react-router-dom";
import { useRequirements } from "../../requirements";
import { useProjectOptions } from "../../users";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateTestCase, useCreateTestExecution, useExportTestExecutions, useTestCase, useTestCases, useTestPlans } from "../hooks/useVerification";
import type { TestCaseFormInput, TestCaseListItem, TestExecutionFormInput } from "../types/verification";

const { Title, Paragraph, Text } = Typography;

export function TestCaseExecutionPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.verification.read);
  const canManage = permissionState.hasPermission(permissions.verification.manage);
  const canExport = permissionState.hasPermission(permissions.verification.export);
  const canReadSensitive = permissionState.hasPermission(permissions.verification.readSensitiveEvidence);
  const [messageApi, contextHolder] = message.useMessage();
  const [searchParams] = useSearchParams();
  const initialPlanId = searchParams.get("testPlanId") ?? undefined;
  const [filters, setFilters] = useState({ search: "", testPlanId: initialPlanId, requirementId: undefined as string | undefined, status: undefined as string | undefined, latestResult: undefined as string | undefined, page: 1, pageSize: 10 });
  const [selectedCaseId, setSelectedCaseId] = useState<string | null>(null);
  const [isCreateCaseOpen, setIsCreateCaseOpen] = useState(false);
  const [isExecutionOpen, setIsExecutionOpen] = useState(false);
  const [caseForm] = Form.useForm<TestCaseFormInput>();
  const [executionForm] = Form.useForm<TestExecutionFormInput>();

  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 20 });
  const requirementsQuery = useRequirements({ projectId: undefined, page: 1, pageSize: 100 }, canRead);
  const testCasesQuery = useTestCases(filters, canRead);
  const testPlansQuery = useTestPlans({ page: 1, pageSize: 100 }, canRead);
  const selectedCaseQuery = useTestCase(selectedCaseId, canRead);
  const createCaseMutation = useCreateTestCase();
  const createExecutionMutation = useCreateTestExecution();
  const exportMutation = useExportTestExecutions();

  const columns = useMemo<ColumnsType<TestCaseListItem>>(
    () => [
      {
        title: "Case",
        key: "case",
        render: (_, item) => (
          <Space direction="vertical" size={0}>
            <Text strong>{item.title}</Text>
            <Text type="secondary">{item.code}</Text>
          </Space>
        ),
      },
      { title: "Plan", dataIndex: "testPlanCode", key: "testPlanCode" },
      { title: "Project", dataIndex: "projectName", key: "projectName" },
      { title: "Requirement", dataIndex: "requirementCode", key: "requirementCode", render: (value) => value ?? <Text type="secondary">unlinked</Text> },
      { title: "Status", dataIndex: "status", key: "status", render: (value) => <Tag>{value}</Tag> },
      { title: "Latest", dataIndex: "latestResult", key: "latestResult", render: (value) => value ? <Tag color={value === "passed" ? "green" : value === "failed" ? "red" : "gold"}>{value}</Tag> : <Text type="secondary">not run</Text> },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => (
          <Flex gap={8} wrap>
            <Button size="small" onClick={() => setSelectedCaseId(item.id)}>Detail</Button>
            <Button size="small" disabled={!canManage} onClick={() => { setSelectedCaseId(item.id); setIsExecutionOpen(true); }}>Execute</Button>
          </Flex>
        ),
      },
    ],
    [canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Test case access is not available for this account." />;
  }

  const handleCreateCase = async () => {
    const values = await caseForm.validateFields();
    try {
      await createCaseMutation.mutateAsync({ ...values, steps: values.steps ?? [] });
      caseForm.resetFields();
      setIsCreateCaseOpen(false);
      void messageApi.success("Test case created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create test case");
      void messageApi.error(presentation.description);
    }
  };

  const handleExecution = async () => {
    const values = await executionForm.validateFields();
    if (!selectedCaseId) {
      return;
    }

    try {
      await createExecutionMutation.mutateAsync({ ...values, testCaseId: selectedCaseId, evidenceClassification: values.isSensitiveEvidence ? values.evidenceClassification ?? undefined : undefined });
      executionForm.resetFields();
      setIsExecutionOpen(false);
      void messageApi.success("Execution recorded.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to record execution");
      void messageApi.error(presentation.description);
    }
  };

  const handleExport = async () => {
    try {
      const result = await exportMutation.mutateAsync({ testCaseId: selectedCaseId ?? undefined });
      if (result.status === "queued") {
        void messageApi.info(result.message ?? "Execution export queued.");
        return;
      }

      void messageApi.success(`Export completed with ${result.count} execution record(s).`);
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to export execution history");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #7c2d12, #0f172a)", color: "#fff" }}>
            <ExperimentOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Test Case & Execution</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Link requirements to executable cases, record evidence, and review history with paging and export controls.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search cases" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
            <Select allowClear showSearch placeholder="Project Context" style={{ width: 220 }} options={projectOptions.options} value={undefined} onSearch={projectOptions.onSearch} disabled />
            <Select allowClear placeholder="Requirement" style={{ width: 220 }} options={(requirementsQuery.data?.items ?? []).map((item) => ({ label: item.code, value: item.id }))} value={filters.requirementId} onChange={(value) => setFilters((current) => ({ ...current, requirementId: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 160 }} options={["draft", "ready", "active", "retired"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Select allowClear placeholder="Latest Result" style={{ width: 160 }} options={["passed", "failed", "retest"].map((value) => ({ label: value, value }))} value={filters.latestResult} onChange={(value) => setFilters((current) => ({ ...current, latestResult: value, page: 1 }))} />
          </Flex>
          <Flex gap={8}>
            <Button icon={<UploadOutlined />} disabled={!canExport} onClick={() => void handleExport()}>Export History</Button>
            <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setIsCreateCaseOpen(true)}>New case</Button>
          </Flex>
        </Flex>

        <Table rowKey="id" loading={testCasesQuery.isLoading} columns={columns} dataSource={testCasesQuery.data?.items ?? []} onRow={(record) => ({ onClick: () => setSelectedCaseId(record.id) })} pagination={{ current: testCasesQuery.data?.page ?? filters.page, pageSize: testCasesQuery.data?.pageSize ?? filters.pageSize, total: testCasesQuery.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Card title="Execution Detail" variant="borderless">
        {selectedCaseQuery.data ? (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <Flex justify="space-between" wrap="wrap" gap={12}>
              <div>
                <Text strong>{selectedCaseQuery.data.code} • {selectedCaseQuery.data.title}</Text>
                <br />
                <Text type="secondary">Requirement: {selectedCaseQuery.data.requirementCode ?? "unlinked"} · Latest result: {selectedCaseQuery.data.latestResult ?? "not run"}</Text>
              </div>
              <Button disabled={!canManage} onClick={() => setIsExecutionOpen(true)}>Record execution</Button>
            </Flex>
            <Paragraph type="secondary" style={{ margin: 0 }}>Expected result: {selectedCaseQuery.data.expectedResult}</Paragraph>
            <Table rowKey="id" size="small" pagination={false} dataSource={selectedCaseQuery.data.executions} columns={[
              { title: "Executed At", dataIndex: "executedAt", render: (value: string) => dayjs(value).format("YYYY-MM-DD HH:mm") },
              { title: "By", dataIndex: "executedBy" },
              { title: "Result", dataIndex: "result", render: (value: string) => <Tag color={value === "passed" ? "green" : value === "failed" ? "red" : "gold"}>{value}</Tag> },
              { title: "Evidence", dataIndex: "evidenceRef", render: (value: string | null) => value ?? <Text type="secondary">none</Text> },
              { title: "Classification", dataIndex: "evidenceClassification", render: (value: string | null) => value ?? <Text type="secondary">normal</Text> },
            ]} />
          </Space>
        ) : (
          <Alert type="info" showIcon message="Select a test case to review execution history." />
        )}
      </Card>

      <Modal title="Create test case" open={isCreateCaseOpen} onOk={() => void handleCreateCase()} onCancel={() => setIsCreateCaseOpen(false)} okText="Create" confirmLoading={createCaseMutation.isPending} destroyOnHidden>
        <Form form={caseForm} layout="vertical" initialValues={{ status: "draft", steps: [] }}>
          <Form.Item label="Test Plan" name="testPlanId" rules={[{ required: true, message: "Test plan is required." }]}><Select options={(testPlansQuery.data?.items ?? []).map((item) => ({ label: `${item.code} • ${item.title}`, value: item.id }))} /></Form.Item>
          <Form.Item label="Code" name="code" rules={[{ required: true, message: "Code is required." }]}><Input placeholder="TC-001" /></Form.Item>
          <Form.Item label="Title" name="title" rules={[{ required: true, message: "Title is required." }]}><Input /></Form.Item>
          <Form.Item label="Preconditions" name="preconditions"><Input.TextArea rows={2} /></Form.Item>
          <Form.Item label="Steps" name="steps"><Select mode="tags" tokenSeparators={[","]} placeholder="Add one step per tag" /></Form.Item>
          <Form.Item label="Expected Result" name="expectedResult" rules={[{ required: true, message: "Expected result is required." }]}><Input.TextArea rows={2} /></Form.Item>
          <Form.Item label="Requirement" name="requirementId"><Select allowClear options={(requirementsQuery.data?.items ?? []).map((item) => ({ label: `${item.code} • ${item.title}`, value: item.id }))} /></Form.Item>
          <Form.Item label="Status" name="status"><Select options={["draft", "ready", "active", "retired"].map((value) => ({ label: value, value }))} /></Form.Item>
        </Form>
      </Modal>

      <Modal title="Record execution" open={isExecutionOpen} onOk={() => void handleExecution()} onCancel={() => setIsExecutionOpen(false)} okText="Record" confirmLoading={createExecutionMutation.isPending} destroyOnHidden>
        <Form form={executionForm} layout="vertical" initialValues={{ result: "passed", isSensitiveEvidence: false }}>
          <Form.Item label="Result" name="result" rules={[{ required: true, message: "Result is required." }]}><Select options={["passed", "failed", "retest"].map((value) => ({ label: value, value }))} /></Form.Item>
          <Form.Item label="Evidence Reference" name="evidenceRef"><Input placeholder="DOC-123 or minio/object" /></Form.Item>
          <Form.Item label="Notes" name="notes"><Input.TextArea rows={3} /></Form.Item>
          {canReadSensitive ? (
            <>
              <Form.Item name="isSensitiveEvidence" valuePropName="checked"><Checkbox>Sensitive evidence</Checkbox></Form.Item>
              <Form.Item shouldUpdate noStyle>
                {() => executionForm.getFieldValue("isSensitiveEvidence") ? <Form.Item label="Classification" name="evidenceClassification" rules={[{ required: true, message: "Classification is required." }]}><Input placeholder="confidential" /></Form.Item> : null}
              </Form.Item>
            </>
          ) : null}
        </Form>
      </Modal>
    </Space>
  );
}
