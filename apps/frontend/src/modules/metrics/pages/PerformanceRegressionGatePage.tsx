import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { LineChartOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useEvaluatePerformanceGate, useOverridePerformanceGate, usePerformanceGates } from "../hooks/useMetrics";
import type { EvaluatePerformanceGateInput, PerformanceGateItem } from "../types/metrics";

const { Title, Paragraph } = Typography;
const resultOptions = ["pending", "passed", "failed", "overridden"].map((value) => ({ label: value, value }));

export function PerformanceRegressionGatePage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.metrics.read);
  const canManage = permissionState.hasPermission(permissions.metrics.manage);
  const canOverride = permissionState.hasPermission(permissions.metrics.overrideQualityGates);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", scopeRef: undefined as string | undefined, result: undefined as string | undefined, page: 1, pageSize: 25 });
  const [evaluateOpen, setEvaluateOpen] = useState(false);
  const [overrideTarget, setOverrideTarget] = useState<PerformanceGateItem | null>(null);
  const [evaluateForm] = Form.useForm<EvaluatePerformanceGateInput>();
  const [overrideForm] = Form.useForm<{ reason: string }>();
  const query = usePerformanceGates(filters, canRead);
  const evaluateMutation = useEvaluatePerformanceGate();
  const overrideMutation = useOverridePerformanceGate();

  const columns = useMemo<ColumnsType<PerformanceGateItem>>(
    () => [
      { title: "Scope", dataIndex: "scopeRef" },
      { title: "Result", dataIndex: "result", render: (value: string) => <Tag color={value === "failed" ? "red" : value === "passed" ? "green" : value === "overridden" ? "gold" : "default"}>{value}</Tag> },
      { title: "Evaluated At", dataIndex: "evaluatedAt", render: (value: string) => new Date(value).toLocaleString() },
      { title: "Reason", dataIndex: "reason", render: (value: string | null | undefined) => value ?? "-" },
      { title: "Evidence", dataIndex: "evidenceRef", render: (value: string | null | undefined) => value ?? "-" },
      { title: "Override", dataIndex: "overrideReason", render: (value: string | null | undefined) => value ?? "-" },
      { title: "Actions", render: (_, item) => <Button size="small" disabled={!canOverride || item.result !== "failed"} onClick={() => setOverrideTarget(item)}>Override</Button> },
    ],
    [canOverride],
  );

  if (!canRead) return <Alert type="warning" showIcon message="Performance gates are not available for this account." />;

  const handleEvaluate = async () => {
    const values = await evaluateForm.validateFields();
    try {
      await evaluateMutation.mutateAsync(values);
      evaluateForm.resetFields();
      setEvaluateOpen(false);
      void messageApi.success("Performance gate evaluated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to evaluate performance gate");
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
      void messageApi.success("Performance gate overridden.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to override performance gate");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless"><Space align="start" size={16}><div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #7c3aed, #1d4ed8)", color: "#fff" }}><LineChartOutlined /></div><div><Title level={3} style={{ margin: 0 }}>Performance Regression Gate</Title><Paragraph type="secondary" style={{ margin: "4px 0 0" }}>Evaluate scoped regression outcomes and require explicit override reasons for failed gates.</Paragraph></div></Space></Card>
      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search scope or reason" style={{ width: 260 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
            <Input allowClear placeholder="Scope Ref" style={{ width: 200 }} value={filters.scopeRef} onChange={(event) => setFilters((current) => ({ ...current, scopeRef: event.target.value || undefined, page: 1 }))} />
            <Select allowClear placeholder="Result" style={{ width: 180 }} options={resultOptions} value={filters.result} onChange={(value) => setFilters((current) => ({ ...current, result: value, page: 1 }))} />
          </Flex>
          <Button type="primary" disabled={!canManage} onClick={() => setEvaluateOpen(true)}>Evaluate gate</Button>
        </Flex>
        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>
      <Modal title="Evaluate performance gate" open={evaluateOpen} onOk={() => void handleEvaluate()} onCancel={() => setEvaluateOpen(false)} confirmLoading={evaluateMutation.isPending} destroyOnHidden>
        <Form form={evaluateForm} layout="vertical">
          <Form.Item label="Scope Ref" name="scopeRef" rules={[{ required: true, message: "Scope reference is required." }]}><Input /></Form.Item>
          <Form.Item label="Result" name="result" rules={[{ required: true, message: "Result is required." }]}><Select options={["pending", "passed", "failed"].map((value) => ({ label: value, value }))} /></Form.Item>
          <Form.Item label="Reason" name="reason"><Input.TextArea rows={3} /></Form.Item>
          <Form.Item label="Evidence Ref" name="evidenceRef"><Input placeholder="grafana://dashboard/latency" /></Form.Item>
        </Form>
      </Modal>
      <Modal title="Override performance gate" open={Boolean(overrideTarget)} onOk={() => void handleOverride()} onCancel={() => setOverrideTarget(null)} confirmLoading={overrideMutation.isPending} destroyOnHidden>
        <Form form={overrideForm} layout="vertical">
          <Form.Item label="Reason" name="reason" rules={[{ required: true, message: "Override reason is required." }]}><Input.TextArea rows={4} /></Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
