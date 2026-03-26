import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, InputNumber, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useProjectOptions } from "../../users";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useEvaluateQualityGate, useMetricDefinitions, useOverrideQualityGate, useQualityGates } from "../hooks/useMetrics";
import type { EvaluateQualityGateInput, QualityGateResultItem } from "../types/metrics";

const { Title, Paragraph, Text } = Typography;

interface EvaluateQualityGateFormValues {
  projectId: string;
  gateType: string;
  reason?: string;
  metricDefinitionId: string;
  measuredValue: number;
  sourceRef: string;
}

export function QualityGatesPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.metrics.read);
  const canManage = permissionState.hasPermission(permissions.metrics.manage);
  const canOverride = permissionState.hasPermission(permissions.metrics.overrideQualityGates);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ projectId: undefined as string | undefined, gateType: undefined as string | undefined, result: undefined as string | undefined, page: 1, pageSize: 10 });
  const [evaluateOpen, setEvaluateOpen] = useState(false);
  const [overrideTarget, setOverrideTarget] = useState<QualityGateResultItem | null>(null);
  const [evaluateForm] = Form.useForm<EvaluateQualityGateFormValues>();
  const [overrideForm] = Form.useForm<{ reason: string }>();
  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 20 });
  const definitionsQuery = useMetricDefinitions({ page: 1, pageSize: 100, status: "active" }, canRead);
  const gatesQuery = useQualityGates(filters, canRead);
  const evaluateMutation = useEvaluateQualityGate();
  const overrideMutation = useOverrideQualityGate();

  const columns = useMemo<ColumnsType<QualityGateResultItem>>(
    () => [
      { title: "Project", dataIndex: "projectName", key: "projectName" },
      { title: "Gate Type", dataIndex: "gateType", key: "gateType" },
      { title: "Result", dataIndex: "result", key: "result", render: (value: string) => <Tag color={value === "failed" ? "red" : value === "passed" ? "green" : "gold"}>{value}</Tag> },
      { title: "Evaluated At", dataIndex: "evaluatedAt", key: "evaluatedAt", render: (value: string) => new Date(value).toLocaleString() },
      { title: "Reason", dataIndex: "reason", key: "reason", render: (value: string | null | undefined) => value ?? "-" },
      { title: "Override", dataIndex: "overrideReason", key: "overrideReason", render: (value: string | null | undefined) => value ?? "-" },
      {
        title: "Metrics",
        key: "metrics",
        render: (_, item) => (
          <Space direction="vertical" size={0}>
            {item.metrics.map((metric) => (
              <Text key={metric.id} type={metric.status === "threshold_breached" ? "danger" : undefined}>
                {metric.metricCode}: {metric.measuredValue}
              </Text>
            ))}
          </Space>
        ),
      },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => (
          <Button size="small" disabled={!canOverride || item.result !== "failed"} onClick={() => setOverrideTarget(item)}>
            Override
          </Button>
        ),
      },
    ],
    [canOverride],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Quality gate access is not available for this account." />;
  }

  const handleEvaluate = async () => {
    const values = await evaluateForm.validateFields();
    const input: EvaluateQualityGateInput = {
      projectId: values.projectId,
      gateType: values.gateType,
      reason: values.reason,
      metricInputs: [
        {
          metricDefinitionId: values.metricDefinitionId,
          measuredValue: values.measuredValue,
          sourceRef: values.sourceRef,
        },
      ],
    };

    try {
      await evaluateMutation.mutateAsync(input);
      evaluateForm.resetFields();
      setEvaluateOpen(false);
      void messageApi.success("Quality gate evaluated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to evaluate quality gate");
      void messageApi.error(presentation.description);
    }
  };

  const handleOverride = async () => {
    if (!overrideTarget) return;
    const values = await overrideForm.validateFields();
    try {
      await overrideMutation.mutateAsync({ id: overrideTarget.id, input: values });
      overrideForm.resetFields();
      setOverrideTarget(null);
      void messageApi.success("Quality gate overridden.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to override quality gate");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Title level={3} style={{ margin: 0 }}>Quality Gate Status</Title>
        <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
          Evaluate blocking gates from objective metric data and require governed override reasons for any exception path.
        </Paragraph>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Select allowClear showSearch placeholder="Project" style={{ width: 240 }} options={projectOptions.options} value={filters.projectId} onSearch={projectOptions.onSearch} onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))} />
            <Input allowClear placeholder="Gate Type" style={{ width: 180 }} value={filters.gateType} onChange={(event) => setFilters((current) => ({ ...current, gateType: event.target.value || undefined, page: 1 }))} />
            <Select allowClear placeholder="Result" style={{ width: 180 }} options={["pending", "passed", "failed", "overridden"].map((value) => ({ label: value, value }))} value={filters.result} onChange={(value) => setFilters((current) => ({ ...current, result: value, page: 1 }))} />
          </Flex>
          <Button type="primary" disabled={!canManage} onClick={() => setEvaluateOpen(true)}>Evaluate gate</Button>
        </Flex>

        <Table rowKey="id" loading={gatesQuery.isLoading} columns={columns} dataSource={gatesQuery.data?.items ?? []} pagination={{ current: gatesQuery.data?.page ?? filters.page, pageSize: gatesQuery.data?.pageSize ?? filters.pageSize, total: gatesQuery.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Evaluate quality gate" open={evaluateOpen} onOk={() => void handleEvaluate()} onCancel={() => setEvaluateOpen(false)} confirmLoading={evaluateMutation.isPending} destroyOnHidden>
        <Form form={evaluateForm} layout="vertical">
          <Form.Item label="Project" name="projectId" rules={[{ required: true, message: "Project is required." }]}>
            <Select showSearch options={projectOptions.options} onSearch={projectOptions.onSearch} />
          </Form.Item>
          <Form.Item label="Gate Type" name="gateType" rules={[{ required: true, message: "Gate type is required." }]}><Input placeholder="release_readiness" /></Form.Item>
          <Form.Item label="Metric" name="metricDefinitionId" rules={[{ required: true, message: "Metric is required." }]}>
            <Select options={(definitionsQuery.data?.items ?? []).map((item) => ({ label: `${item.code} • ${item.name}`, value: item.id }))} />
          </Form.Item>
          <Form.Item label="Measured Value" name="measuredValue" rules={[{ required: true, message: "Measured value is required." }]}><InputNumber style={{ width: "100%" }} /></Form.Item>
          <Form.Item label="Source Ref" name="sourceRef" rules={[{ required: true, message: "Source reference is required." }]}><Input placeholder="prometheus:query-id" /></Form.Item>
          <Form.Item label="Reason" name="reason"><Input.TextArea rows={3} /></Form.Item>
        </Form>
      </Modal>

      <Modal title="Override quality gate" open={Boolean(overrideTarget)} onOk={() => void handleOverride()} onCancel={() => setOverrideTarget(null)} confirmLoading={overrideMutation.isPending} destroyOnHidden>
        <Form form={overrideForm} layout="vertical">
          <Form.Item label="Reason" name="reason" rules={[{ required: true, message: "Override reason is required." }]}>
            <Input.TextArea rows={4} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
