import { useMemo, useState } from "react";
import { Alert, Button, Card, DatePicker, Descriptions, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import dayjs from "dayjs";
import { ExportOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateEvidenceExport, useEvidenceExport, useEvidenceExports } from "../hooks/useAuditLogs";
import type { CreateEvidenceExportInput, EvidenceExportItem } from "../types/audits";

const { RangePicker } = DatePicker;
const { Title, Paragraph } = Typography;

interface ExportFormValues {
  scopeType: string;
  scopeRef: string;
  range: [dayjs.Dayjs, dayjs.Dayjs];
  includedArtifactTypes?: string[];
}

export function EvidenceExportsPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.auditLogs.read, permissions.auditLogs.export);
  const canExport = permissionState.hasPermission(permissions.auditLogs.export);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ scopeType: undefined as string | undefined, status: undefined as string | undefined, requestedBy: undefined as string | undefined, page: 1, pageSize: 10 });
  const [createOpen, setCreateOpen] = useState(false);
  const [selectedExportId, setSelectedExportId] = useState<string | null>(null);
  const [form] = Form.useForm<ExportFormValues>();
  const exportsQuery = useEvidenceExports(filters, canRead);
  const detailQuery = useEvidenceExport(selectedExportId, canRead && Boolean(selectedExportId));
  const createMutation = useCreateEvidenceExport();

  const handleCreate = async () => {
    const values = await form.validateFields();
    const input: CreateEvidenceExportInput = {
      scopeType: values.scopeType,
      scopeRef: values.scopeRef,
      from: values.range[0].startOf("day").toISOString(),
      to: values.range[1].endOf("day").toISOString(),
      includedArtifactTypes: values.includedArtifactTypes,
    };

    try {
      await createMutation.mutateAsync(input);
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Evidence export requested.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to request evidence export");
      void messageApi.error(presentation.description);
    }
  };

  const columns = useMemo<ColumnsType<EvidenceExportItem>>(
    () => [
      { title: "Requested By", dataIndex: "requestedBy", key: "requestedBy" },
      { title: "Scope", key: "scope", render: (_, item) => `${item.scopeType}: ${item.scopeRef}` },
      { title: "Requested At", dataIndex: "requestedAt", key: "requestedAt", render: (value: string) => dayjs(value).format("YYYY-MM-DD HH:mm") },
      { title: "Status", dataIndex: "status", key: "status", render: (value: string) => <Tag color={value === "generated" ? "green" : value === "requested" ? "blue" : "default"}>{value}</Tag> },
      { title: "Output", dataIndex: "outputRef", key: "outputRef", render: (value: string | null) => value ?? "-" },
      { title: "Actions", key: "actions", render: (_, item) => <Button size="small" onClick={() => setSelectedExportId(item.id)}>View</Button> },
    ],
    [],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Evidence export access is not available for this account." />;
  }

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0f766e, #1d4ed8)", color: "#fff" }}>
            <ExportOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Evidence Export</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Generate governed evidence packages by scope, date range, and artifact type for internal or external audits.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Select allowClear placeholder="Scope Type" style={{ width: 180 }} options={["project", "module", "audit_plan"].map((value) => ({ label: value, value }))} value={filters.scopeType} onChange={(value) => setFilters((current) => ({ ...current, scopeType: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["requested", "generated", "downloaded", "expired"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Input allowClear placeholder="Requested by" style={{ width: 220 }} value={filters.requestedBy} onChange={(event) => setFilters((current) => ({ ...current, requestedBy: event.target.value || undefined, page: 1 }))} />
          </Flex>
          <Button type="primary" disabled={!canExport} onClick={() => setCreateOpen(true)}>Request export</Button>
        </Flex>

        <Table
          rowKey="id"
          loading={exportsQuery.isLoading}
          columns={columns}
          dataSource={exportsQuery.data?.items ?? []}
          pagination={{
            current: exportsQuery.data?.page ?? filters.page,
            pageSize: exportsQuery.data?.pageSize ?? filters.pageSize,
            total: exportsQuery.data?.total ?? 0,
            showSizeChanger: true,
            onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })),
          }}
        />
      </Card>

      <Modal title="Request evidence export" open={createOpen} onOk={() => void handleCreate()} onCancel={() => setCreateOpen(false)} confirmLoading={createMutation.isPending} destroyOnHidden>
        <Form form={form} layout="vertical">
          <Form.Item label="Scope Type" name="scopeType" rules={[{ required: true, message: "Scope type is required." }]}>
            <Select options={["project", "module", "audit_plan"].map((value) => ({ label: value, value }))} />
          </Form.Item>
          <Form.Item label="Scope Reference" name="scopeRef" rules={[{ required: true, message: "Scope reference is required." }]}><Input placeholder="Project ID or module code" /></Form.Item>
          <Form.Item label="Date Range" name="range" rules={[{ required: true, message: "Date range is required." }]}><RangePicker style={{ width: "100%" }} /></Form.Item>
          <Form.Item label="Artifact Types" name="includedArtifactTypes">
            <Select mode="multiple" options={["audit_logs", "findings", "documents", "requirements", "verification", "meetings"].map((value) => ({ label: value, value }))} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal title="Evidence export detail" open={Boolean(selectedExportId)} onCancel={() => setSelectedExportId(null)} footer={<Button onClick={() => setSelectedExportId(null)}>Close</Button>} width={860} destroyOnHidden>
        {detailQuery.data ? (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <Descriptions bordered size="small" column={2}>
              <Descriptions.Item label="Requested By">{detailQuery.data.requestedBy}</Descriptions.Item>
              <Descriptions.Item label="Status"><Tag>{detailQuery.data.status}</Tag></Descriptions.Item>
              <Descriptions.Item label="Scope">{detailQuery.data.scopeType}: {detailQuery.data.scopeRef}</Descriptions.Item>
              <Descriptions.Item label="Requested At">{dayjs(detailQuery.data.requestedAt).format("YYYY-MM-DD HH:mm")}</Descriptions.Item>
              <Descriptions.Item label="Date Range" span={2}>
                {detailQuery.data.from ? `${dayjs(detailQuery.data.from).format("YYYY-MM-DD HH:mm")} to ${dayjs(detailQuery.data.to).format("YYYY-MM-DD HH:mm")}` : "-"}
              </Descriptions.Item>
              <Descriptions.Item label="Output" span={2}>{detailQuery.data.outputRef ?? "Pending generation"}</Descriptions.Item>
              <Descriptions.Item label="Artifacts" span={2}>{detailQuery.data.includedArtifactTypes.join(", ") || "-"}</Descriptions.Item>
              <Descriptions.Item label="Failure Reason" span={2}>{detailQuery.data.failureReason ?? "-"}</Descriptions.Item>
            </Descriptions>
          </Space>
        ) : null}
      </Modal>
    </Space>
  );
}
