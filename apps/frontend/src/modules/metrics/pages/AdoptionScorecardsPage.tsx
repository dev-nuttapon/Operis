import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, InputNumber, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { FormInstance } from "antd";
import type { ColumnsType } from "antd/es/table";
import { CheckSquareOutlined, PlusOutlined, SyncOutlined } from "@ant-design/icons";
import { useProjectOptions } from "../../users";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import {
  useAdoptionRules,
  useAdoptionScorecards,
  useCreateAdoptionRule,
  useEvaluateAdoptionRules,
  useUpdateAdoptionRule,
} from "../hooks/useMetrics";
import type {
  AdoptionRuleItem,
  AdoptionScorecardItem,
  CreateAdoptionRuleInput,
  EvaluateAdoptionRulesInput,
  UpdateAdoptionRuleInput,
} from "../types/metrics";

const { Title, Paragraph, Text } = Typography;

const processAreaOptions = [
  "project_governance",
  "requirements_traceability",
  "verification",
  "change_control",
  "operations_review",
].map((value) => ({ label: value, value }));

const scopeTypeOptions = ["project", "portfolio", "platform"].map((value) => ({ label: value, value }));
const ruleStatusOptions = ["draft", "active", "archived"].map((value) => ({ label: value, value }));

export function AdoptionScorecardsPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.metrics.adoptionRead);
  const canManage = permissionState.hasPermission(permissions.metrics.adoptionManage);
  const [messageApi, contextHolder] = message.useMessage();
  const [ruleFilters, setRuleFilters] = useState({ processArea: undefined as string | undefined, scopeType: undefined as string | undefined, status: undefined as string | undefined, search: "", page: 1, pageSize: 25 });
  const [scoreFilters, setScoreFilters] = useState({ projectId: undefined as string | undefined, processArea: undefined as string | undefined, scopeType: undefined as string | undefined, scoreState: undefined as string | undefined, search: "", page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<AdoptionRuleItem | null>(null);
  const [ruleForm] = Form.useForm<CreateAdoptionRuleInput & { status?: string }>();
  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 50 });
  const rulesQuery = useAdoptionRules(ruleFilters, canRead);
  const scorecardsQuery = useAdoptionScorecards(scoreFilters, canRead);
  const createMutation = useCreateAdoptionRule();
  const updateMutation = useUpdateAdoptionRule();
  const evaluateMutation = useEvaluateAdoptionRules();

  const ruleColumns = useMemo<ColumnsType<AdoptionRuleItem>>(
    () => [
      { title: "Rule Code", dataIndex: "ruleCode" },
      { title: "Process Area", dataIndex: "processArea" },
      { title: "Scope", dataIndex: "scopeType" },
      { title: "Threshold", dataIndex: "thresholdPercentage", render: (value: number) => `${value}%` },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button> },
    ],
    [canManage],
  );

  const scorecardColumns = useMemo<ColumnsType<AdoptionScorecardItem>>(
    () => [
      { title: "Project", dataIndex: "projectName" },
      { title: "Rule", dataIndex: "ruleCode" },
      { title: "Process Area", dataIndex: "processArea" },
      { title: "Score", render: (_, item) => <Text strong>{item.scorePercentage}%</Text> },
      { title: "Coverage", render: (_, item) => `${item.evidenceCount}/${item.expectedCount}` },
      { title: "Threshold", render: (_, item) => `${item.thresholdPercentage}%` },
      { title: "State", dataIndex: "scoreState", render: (value: string) => <Tag color={value === "meets_threshold" ? "green" : "red"}>{value}</Tag> },
      { title: "Anomalies", render: (_, item) => item.anomalies.length > 0 ? <Tag color="orange">{item.anomalies.length}</Tag> : <Tag>0</Tag> },
    ],
    [],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Adoption scoring is not available for this account." />;
  }

  const submitCreate = async () => {
    const values = await ruleForm.validateFields();
    try {
      await createMutation.mutateAsync(values);
      ruleForm.resetFields();
      setCreateOpen(false);
      void messageApi.success("Adoption rule created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create adoption rule");
      void messageApi.error(presentation.description);
    }
  };

  const submitUpdate = async () => {
    if (!editing) return;
    const values = await ruleForm.validateFields();
    try {
      await updateMutation.mutateAsync({
        id: editing.id,
        input: {
          processArea: values.processArea,
          scopeType: values.scopeType,
          thresholdPercentage: values.thresholdPercentage,
          status: values.status ?? editing.status,
        } as UpdateAdoptionRuleInput,
      });
      ruleForm.resetFields();
      setEditing(null);
      void messageApi.success("Adoption rule updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update adoption rule");
      void messageApi.error(presentation.description);
    }
  };

  const runEvaluation = async () => {
    try {
      await evaluateMutation.mutateAsync({
        projectId: scoreFilters.projectId ?? null,
        processArea: scoreFilters.processArea ?? null,
        scopeType: scoreFilters.scopeType ?? null,
      } as EvaluateAdoptionRulesInput);
      void messageApi.success("Adoption scorecards refreshed.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to evaluate adoption rules");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #1d4ed8, #0f766e)", color: "#fff" }}>
            <CheckSquareOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Adoption Scorecards</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Measure project-level process adoption, refresh governed scorecards, and surface threshold anomalies from actual workflow evidence.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless" title="Adoption Rules" extra={<Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New rule</Button>}>
        <Flex gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Input.Search allowClear placeholder="Search rule or process area" style={{ width: 240 }} value={ruleFilters.search} onChange={(event) => setRuleFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
          <Select allowClear placeholder="Process Area" style={{ width: 220 }} options={processAreaOptions} value={ruleFilters.processArea} onChange={(value) => setRuleFilters((current) => ({ ...current, processArea: value, page: 1 }))} />
          <Select allowClear placeholder="Scope Type" style={{ width: 180 }} options={scopeTypeOptions} value={ruleFilters.scopeType} onChange={(value) => setRuleFilters((current) => ({ ...current, scopeType: value, page: 1 }))} />
          <Select allowClear placeholder="Status" style={{ width: 160 }} options={ruleStatusOptions} value={ruleFilters.status} onChange={(value) => setRuleFilters((current) => ({ ...current, status: value, page: 1 }))} />
        </Flex>
        <Table rowKey="id" loading={rulesQuery.isLoading} columns={ruleColumns} dataSource={rulesQuery.data?.items ?? []} pagination={{ current: rulesQuery.data?.page ?? ruleFilters.page, pageSize: rulesQuery.data?.pageSize ?? ruleFilters.pageSize, total: rulesQuery.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setRuleFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Card
        variant="borderless"
        title="Project Adoption Scorecards"
        extra={<Button icon={<SyncOutlined spin={evaluateMutation.isPending} />} disabled={!canManage} loading={evaluateMutation.isPending} onClick={() => void runEvaluation()}>Refresh Scorecards</Button>}
      >
        <Flex gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Select allowClear showSearch placeholder="Project" style={{ width: 240 }} options={projectOptions.options} value={scoreFilters.projectId} onSearch={projectOptions.onSearch} onChange={(value) => setScoreFilters((current) => ({ ...current, projectId: value, page: 1 }))} />
          <Select allowClear placeholder="Process Area" style={{ width: 220 }} options={processAreaOptions} value={scoreFilters.processArea} onChange={(value) => setScoreFilters((current) => ({ ...current, processArea: value, page: 1 }))} />
          <Select allowClear placeholder="Scope Type" style={{ width: 180 }} options={scopeTypeOptions} value={scoreFilters.scopeType} onChange={(value) => setScoreFilters((current) => ({ ...current, scopeType: value, page: 1 }))} />
          <Select allowClear placeholder="Score State" style={{ width: 180 }} options={["meets_threshold", "below_threshold"].map((value) => ({ label: value, value }))} value={scoreFilters.scoreState} onChange={(value) => setScoreFilters((current) => ({ ...current, scoreState: value, page: 1 }))} />
          <Input.Search allowClear placeholder="Search project or rule" style={{ width: 220 }} value={scoreFilters.search} onChange={(event) => setScoreFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
        </Flex>
        <Table
          rowKey="id"
          loading={scorecardsQuery.isLoading}
          columns={scorecardColumns}
          expandable={{
            expandedRowRender: (item) => item.anomalies.length === 0 ? <Text type="secondary">No open anomalies for this scorecard.</Text> : (
              <Space direction="vertical" size={8} style={{ width: "100%" }}>
                {item.anomalies.map((anomaly) => (
                  <Card key={anomaly.id} size="small">
                    <Space direction="vertical" size={4}>
                      <Text strong>{anomaly.summary}</Text>
                      <Text type="secondary">Severity: {anomaly.severity} · Detected: {new Date(anomaly.detectedAt).toLocaleString()}</Text>
                    </Space>
                  </Card>
                ))}
              </Space>
            ),
            rowExpandable: (item) => item.anomalies.length > 0,
          }}
          dataSource={scorecardsQuery.data?.items ?? []}
          pagination={{ current: scorecardsQuery.data?.page ?? scoreFilters.page, pageSize: scorecardsQuery.data?.pageSize ?? scoreFilters.pageSize, total: scorecardsQuery.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setScoreFilters((current) => ({ ...current, page, pageSize })) }}
        />
      </Card>

      <Modal title="Create adoption rule" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); ruleForm.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <AdoptionRuleForm form={ruleForm} />
      </Modal>

      <Modal
        title="Edit adoption rule"
        open={Boolean(editing)}
        onOk={() => void submitUpdate()}
        onCancel={() => { setEditing(null); ruleForm.resetFields(); }}
        confirmLoading={updateMutation.isPending}
        destroyOnHidden
        afterOpenChange={(open) => {
          if (open && editing) {
            ruleForm.setFieldsValue({
              ruleCode: editing.ruleCode,
              processArea: editing.processArea,
              scopeType: editing.scopeType,
              thresholdPercentage: editing.thresholdPercentage,
              status: editing.status,
            });
          }
        }}
      >
        <AdoptionRuleForm form={ruleForm} includeStatus disableRuleCode />
      </Modal>
    </Space>
  );
}

function AdoptionRuleForm({
  form,
  includeStatus = false,
  disableRuleCode = false,
}: {
  form: FormInstance<CreateAdoptionRuleInput & { status?: string }>;
  includeStatus?: boolean;
  disableRuleCode?: boolean;
}) {
  return (
    <Form form={form} layout="vertical">
      <Form.Item label="Rule Code" name="ruleCode" rules={[{ required: true, message: "Rule code is required." }]}>
        <Input disabled={disableRuleCode} />
      </Form.Item>
      <Form.Item label="Process Area" name="processArea" rules={[{ required: true, message: "Process area is required." }]}>
        <Select options={processAreaOptions} />
      </Form.Item>
      <Form.Item label="Scope Type" name="scopeType" rules={[{ required: true, message: "Scope type is required." }]}>
        <Select options={scopeTypeOptions} />
      </Form.Item>
      <Form.Item label="Threshold Percentage" name="thresholdPercentage" rules={[{ required: true, message: "Threshold percentage is required." }]}>
        <InputNumber min={1} max={100} style={{ width: "100%" }} />
      </Form.Item>
      {includeStatus ? (
        <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}>
          <Select options={ruleStatusOptions} />
        </Form.Item>
      ) : null}
    </Form>
  );
}
