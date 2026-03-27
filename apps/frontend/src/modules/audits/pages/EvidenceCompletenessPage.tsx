import { useMemo, useState } from "react";
import { Alert, Button, Card, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { CheckCircleOutlined, PlayCircleOutlined, PlusOutlined } from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useProjectList } from "../../users/public";
import {
  useCreateEvidenceRule,
  useEvaluateEvidenceRules,
  useEvidenceRuleResults,
  useEvidenceRules,
  useUpdateEvidenceRule,
} from "../hooks/useAuditLogs";
import type { EvidenceRuleListItem, EvidenceRuleResultListItem } from "../types/audits";

const { Title, Paragraph, Text } = Typography;

const processAreaOptions = [
  { value: "process-assets-planning", label: "Process Assets & Planning" },
  { value: "requirements-traceability", label: "Requirements & Traceability" },
  { value: "document-governance", label: "Document Governance" },
  { value: "change-configuration", label: "Change & Configuration" },
  { value: "verification-release", label: "Verification & Release" },
  { value: "audit-capa", label: "Audit & CAPA" },
  { value: "security-resilience", label: "Security & Resilience" },
] as const;

const artifactTypeOptions = [
  { value: "project_plan_baseline", label: "Project Plan Baseline" },
  { value: "tailoring_approval", label: "Tailoring Approval" },
  { value: "requirement_baseline", label: "Requirement Baseline" },
  { value: "requirement_test_traceability", label: "Requirement/Test Traceability" },
  { value: "approved_document", label: "Approved Document" },
  { value: "approved_change_request", label: "Approved Change Request" },
  { value: "baseline_registry_link", label: "Baseline Registry Link" },
  { value: "approved_uat_signoff", label: "Approved UAT Signoff" },
  { value: "resolved_audit_finding", label: "Resolved Audit Finding" },
  { value: "security_review_completion", label: "Security Review Completion" },
] as const;

interface RuleFormValues {
  ruleCode: string;
  title: string;
  processArea: string;
  artifactType: string;
  projectId?: string;
  status: string;
  expressionType: string;
  reason?: string;
}

export function EvidenceCompletenessPage() {
  const navigate = useNavigate();
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.audits.evidenceRead, permissions.audits.evidenceManage);
  const canManage = permissionState.hasPermission(permissions.audits.evidenceManage);
  const [messageApi, contextHolder] = message.useMessage();
  const [form] = Form.useForm<RuleFormValues>();
  const [editingRule, setEditingRule] = useState<EvidenceRuleListItem | null>(null);
  const [ruleModalOpen, setRuleModalOpen] = useState(false);
  const [selectedProjectId, setSelectedProjectId] = useState<string | undefined>(undefined);
  const [selectedProcessArea, setSelectedProcessArea] = useState<string | undefined>(undefined);

  const projectsQuery = useProjectList({ page: 1, pageSize: 100 });
  const rulesQuery = useEvidenceRules({ page: 1, pageSize: 50, projectId: selectedProjectId, processArea: selectedProcessArea }, canRead);
  const resultsQuery = useEvidenceRuleResults({ page: 1, pageSize: 25, projectId: selectedProjectId, processArea: selectedProcessArea }, canRead);
  const createRuleMutation = useCreateEvidenceRule();
  const updateRuleMutation = useUpdateEvidenceRule();
  const evaluateMutation = useEvaluateEvidenceRules();

  const ruleColumns = useMemo<ColumnsType<EvidenceRuleListItem>>(
    () => [
      { title: "Rule", key: "rule", render: (_, item) => <Space direction="vertical" size={0}><Text strong>{item.ruleCode}</Text><Text type="secondary">{item.title}</Text></Space> },
      { title: "Process Area", dataIndex: "processArea" },
      { title: "Artifact", dataIndex: "artifactType" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "active" ? "green" : value === "retired" ? "default" : "gold"}>{value}</Tag> },
      { title: "Updated", dataIndex: "updatedAt", render: (value: string) => new Date(value).toLocaleString() },
      canManage
        ? {
            title: "Action",
            key: "action",
            render: (_, item) => <Button type="link" onClick={() => openEdit(item)}>Edit</Button>,
          }
        : {},
    ],
    [canManage],
  );

  const resultColumns = useMemo<ColumnsType<EvidenceRuleResultListItem>>(
    () => [
      { title: "Scope", key: "scope", render: (_, item) => item.projectCode ? `${item.projectCode} · ${item.scopeType}` : item.scopeRef },
      { title: "Process Area", dataIndex: "processArea", render: (value: string | null) => value ?? "all" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "completed" ? "green" : value === "superseded" ? "default" : "blue"}>{value}</Tag> },
      { title: "Rules", dataIndex: "evaluatedRuleCount" },
      { title: "Missing", dataIndex: "missingItemCount" },
      { title: "Completed", dataIndex: "completedAt", render: (value: string) => new Date(value).toLocaleString() },
      {
        title: "Open",
        key: "open",
        render: (_, item) => <Button type="link" onClick={() => navigate(`/app/audits/evidence-completeness/${item.id}`)}>Detail</Button>,
      },
    ],
    [navigate],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Evidence completeness is not available for this account." />;
  }

  const openCreate = () => {
    setEditingRule(null);
    form.setFieldsValue({
      ruleCode: "",
      title: "",
      processArea: processAreaOptions[0].value,
      artifactType: artifactTypeOptions[0].value,
      status: "draft",
      expressionType: "required",
      projectId: undefined,
      reason: "",
    });
    setRuleModalOpen(true);
  };

  const openEdit = (item: EvidenceRuleListItem) => {
    setEditingRule(item);
    form.setFieldsValue({
      ruleCode: item.ruleCode,
      title: item.title,
      processArea: item.processArea,
      artifactType: item.artifactType,
      projectId: item.projectId ?? undefined,
      status: item.status,
      expressionType: item.expressionType,
      reason: "",
    });
    setRuleModalOpen(true);
  };

  const submitRule = async () => {
    const values = await form.validateFields();

    try {
      if (editingRule) {
        await updateRuleMutation.mutateAsync({ id: editingRule.id, input: values });
        void messageApi.success("Evidence rule updated.");
      } else {
        await createRuleMutation.mutateAsync(values);
        void messageApi.success("Evidence rule created.");
      }
      setRuleModalOpen(false);
      form.resetFields();
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to save evidence rule");
      void messageApi.error(presentation.description);
    }
  };

  const runEvaluation = async () => {
    try {
      const detail = await evaluateMutation.mutateAsync({
        projectId: selectedProjectId,
        processArea: selectedProcessArea,
      });
      void messageApi.success("Evidence evaluation completed.");
      navigate(`/app/audits/evidence-completeness/${detail.id}`);
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to evaluate evidence completeness");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #7c2d12, #0f766e)", color: "#fff" }}>
            <CheckCircleOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Evidence Completeness</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Define required evidence rules, evaluate current project coverage, and drill into missing governed artifacts.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Space wrap>
          <Select
            allowClear
            placeholder="Project"
            style={{ width: 240 }}
            options={(projectsQuery.data?.items ?? []).map((project) => ({ value: project.id, label: `${project.code} · ${project.name}` }))}
            value={selectedProjectId}
            onChange={(value) => setSelectedProjectId(value)}
          />
          <Select
            allowClear
            placeholder="Process area"
            style={{ width: 240 }}
            options={processAreaOptions.map((option) => ({ value: option.value, label: option.label }))}
            value={selectedProcessArea}
            onChange={(value) => setSelectedProcessArea(value)}
          />
          <Button icon={<PlayCircleOutlined />} type="primary" loading={evaluateMutation.isPending} disabled={!canManage} onClick={() => void runEvaluation()}>
            Run evaluation
          </Button>
          <Button icon={<PlusOutlined />} disabled={!canManage} onClick={openCreate}>
            Add rule
          </Button>
        </Space>
      </Card>

      <Card variant="borderless" title="Evidence Rules">
        <Table rowKey="id" loading={rulesQuery.isLoading} columns={ruleColumns} dataSource={rulesQuery.data?.items ?? []} pagination={false} />
      </Card>

      <Card variant="borderless" title="Evaluation Results">
        <Table rowKey="id" loading={resultsQuery.isLoading} columns={resultColumns} dataSource={resultsQuery.data?.items ?? []} pagination={false} />
      </Card>

      <Modal
        title={editingRule ? "Edit Evidence Rule" : "Create Evidence Rule"}
        open={ruleModalOpen}
        onCancel={() => setRuleModalOpen(false)}
        onOk={() => void submitRule()}
        confirmLoading={createRuleMutation.isPending || updateRuleMutation.isPending}
        destroyOnHidden
      >
        <Form form={form} layout="vertical">
          <Form.Item name="ruleCode" label="Rule Code" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="title" label="Title" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="processArea" label="Process Area" rules={[{ required: true }]}>
            <Select options={processAreaOptions.map((option) => ({ value: option.value, label: option.label }))} />
          </Form.Item>
          <Form.Item name="artifactType" label="Artifact Target" rules={[{ required: true }]}>
            <Select options={artifactTypeOptions.map((option) => ({ value: option.value, label: option.label }))} />
          </Form.Item>
          <Form.Item name="projectId" label="Project Scope">
            <Select allowClear options={(projectsQuery.data?.items ?? []).map((project) => ({ value: project.id, label: `${project.code} · ${project.name}` }))} />
          </Form.Item>
          <Form.Item name="status" label="Status" rules={[{ required: true }]}>
            <Select options={[{ value: "draft", label: "draft" }, { value: "active", label: "active" }, { value: "retired", label: "retired" }]} />
          </Form.Item>
          <Form.Item name="expressionType" label="Expression" rules={[{ required: true }]}>
            <Select options={[{ value: "required", label: "required" }]} />
          </Form.Item>
          <Form.Item name="reason" label="Reason">
            <Input.TextArea rows={3} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
