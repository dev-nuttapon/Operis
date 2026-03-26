import { useMemo, useState } from "react";
import { Alert, Button, Card, DatePicker, Descriptions, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import dayjs from "dayjs";
import { FileSearchOutlined, PlusOutlined } from "@ant-design/icons";
import { useProjectOptions } from "../../users";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import {
  useAuditPlan,
  useAuditPlans,
  useCloseAuditFinding,
  useCreateAuditFinding,
  useCreateAuditPlan,
  useUpdateAuditPlan,
} from "../hooks/useAuditLogs";
import type { AuditPlanListItem, CreateAuditFindingInput, CreateAuditPlanInput, UpdateAuditPlanInput } from "../types/audits";

const { Title, Paragraph, Text } = Typography;

interface AuditPlanFormValues {
  projectId: string;
  title: string;
  scope: string;
  criteria: string;
  plannedAt: dayjs.Dayjs;
  ownerUserId: string;
}

interface FindingFormValues {
  code: string;
  title: string;
  description: string;
  severity: string;
  ownerUserId: string;
  dueDate?: dayjs.Dayjs;
}

export function AuditPlansPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.auditLogs.read);
  const canManage = permissionState.hasPermission(permissions.auditLogs.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ projectId: undefined as string | undefined, status: undefined as string | undefined, ownerUserId: undefined as string | undefined, page: 1, pageSize: 10 });
  const [createOpen, setCreateOpen] = useState(false);
  const [detailPlanId, setDetailPlanId] = useState<string | null>(null);
  const [findingOpen, setFindingOpen] = useState(false);
  const [selectedPlanId, setSelectedPlanId] = useState<string | null>(null);
  const [planForm] = Form.useForm<AuditPlanFormValues>();
  const [findingForm] = Form.useForm<FindingFormValues>();
  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 20 });
  const plansQuery = useAuditPlans(filters, canRead);
  const detailQuery = useAuditPlan(detailPlanId, canRead && Boolean(detailPlanId));
  const createMutation = useCreateAuditPlan();
  const updateMutation = useUpdateAuditPlan();
  const createFindingMutation = useCreateAuditFinding();
  const closeFindingMutation = useCloseAuditFinding();

  const handleMutationError = (error: unknown, fallback: string) => {
    const presentation = getApiErrorPresentation(error, fallback);
    void messageApi.error(presentation.description);
  };

  const handleCreatePlan = async () => {
    const values = await planForm.validateFields();
    const input: CreateAuditPlanInput = {
      projectId: values.projectId,
      title: values.title,
      scope: values.scope,
      criteria: values.criteria,
      plannedAt: values.plannedAt.toISOString(),
      ownerUserId: values.ownerUserId,
    };

    try {
      await createMutation.mutateAsync(input);
      planForm.resetFields();
      setCreateOpen(false);
      void messageApi.success("Audit plan created.");
    } catch (error) {
      handleMutationError(error, "Unable to create audit plan");
    }
  };

  const handleUpdatePlanStatus = async (item: AuditPlanListItem, status: string) => {
    try {
      const input: UpdateAuditPlanInput = {
        title: detailQuery.data?.id === item.id ? detailQuery.data.title : item.title,
        scope: detailQuery.data?.id === item.id ? detailQuery.data.scope : item.scope,
        criteria: detailQuery.data?.id === item.id ? detailQuery.data.criteria : item.scope,
        plannedAt: item.plannedAt,
        status,
        ownerUserId: item.ownerUserId,
      };
      await updateMutation.mutateAsync({ id: item.id, input });
      void messageApi.success(`Audit plan moved to ${status}.`);
    } catch (error) {
      handleMutationError(error, "Unable to update audit plan");
    }
  };

  const handleCreateFinding = async () => {
    if (!selectedPlanId) {
      return;
    }

    const values = await findingForm.validateFields();
    const input: CreateAuditFindingInput = {
      auditPlanId: selectedPlanId,
      code: values.code,
      title: values.title,
      description: values.description,
      severity: values.severity,
      ownerUserId: values.ownerUserId,
      dueDate: values.dueDate?.format("YYYY-MM-DD"),
    };

    try {
      await createFindingMutation.mutateAsync(input);
      findingForm.resetFields();
      setFindingOpen(false);
      void messageApi.success("Audit finding created.");
    } catch (error) {
      handleMutationError(error, "Unable to create audit finding");
    }
  };

  const columns = useMemo<ColumnsType<AuditPlanListItem>>(
    () => [
      {
        title: "Plan",
        key: "title",
        render: (_, item) => (
          <Space direction="vertical" size={0}>
            <Text strong>{item.title}</Text>
            <Text type="secondary">{item.projectName}</Text>
          </Space>
        ),
      },
      {
        title: "Scope",
        dataIndex: "scope",
        key: "scope",
        render: (value: string) => <Text ellipsis={{ tooltip: value }}>{value}</Text>,
      },
      {
        title: "Status",
        dataIndex: "status",
        key: "status",
        render: (value: string) => <Tag color={value === "closed" ? "green" : value === "findings_issued" ? "volcano" : "blue"}>{value}</Tag>,
      },
      {
        title: "Planned At",
        dataIndex: "plannedAt",
        key: "plannedAt",
        render: (value: string) => dayjs(value).format("YYYY-MM-DD HH:mm"),
      },
      { title: "Owner", dataIndex: "ownerUserId", key: "ownerUserId" },
      { title: "Open Findings", dataIndex: "openFindingCount", key: "openFindingCount" },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => (
          <Flex gap={8} wrap>
            <Button size="small" onClick={() => setDetailPlanId(item.id)}>View</Button>
            <Button size="small" disabled={!canManage || item.status !== "planned"} onClick={() => void handleUpdatePlanStatus(item, "in_review")}>Start review</Button>
            <Button size="small" disabled={!canManage || item.status !== "findings_issued"} onClick={() => void handleUpdatePlanStatus(item, "closed")}>Close</Button>
          </Flex>
        ),
      },
    ],
    [canManage, detailQuery.data],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Audit plan access is not available for this account." />;
  }

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #7c2d12, #b91c1c)", color: "#fff" }}>
            <FileSearchOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Process Audit Plan & Findings</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Manage audit plans, findings, remediation ownership, and closeout evidence in one governed register.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Select allowClear showSearch placeholder="Project" style={{ width: 240 }} options={projectOptions.options} value={filters.projectId} onSearch={projectOptions.onSearch} onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["planned", "in_review", "findings_issued", "closed"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Input allowClear placeholder="Owner" style={{ width: 220 }} value={filters.ownerUserId} onChange={(event) => setFilters((current) => ({ ...current, ownerUserId: event.target.value || undefined, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New audit plan</Button>
        </Flex>

        <Table
          rowKey="id"
          loading={plansQuery.isLoading}
          columns={columns}
          dataSource={plansQuery.data?.items ?? []}
          pagination={{
            current: plansQuery.data?.page ?? filters.page,
            pageSize: plansQuery.data?.pageSize ?? filters.pageSize,
            total: plansQuery.data?.total ?? 0,
            showSizeChanger: true,
            onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })),
          }}
        />
      </Card>

      <Modal title="Create audit plan" open={createOpen} onOk={() => void handleCreatePlan()} onCancel={() => setCreateOpen(false)} confirmLoading={createMutation.isPending} destroyOnHidden>
        <Form form={planForm} layout="vertical">
          <Form.Item label="Project" name="projectId" rules={[{ required: true, message: "Project is required." }]}>
            <Select showSearch options={projectOptions.options} onSearch={projectOptions.onSearch} />
          </Form.Item>
          <Form.Item label="Title" name="title" rules={[{ required: true, message: "Title is required." }]}><Input /></Form.Item>
          <Form.Item label="Scope" name="scope" rules={[{ required: true, message: "Scope is required." }]}><Input.TextArea rows={3} /></Form.Item>
          <Form.Item label="Criteria" name="criteria" rules={[{ required: true, message: "Criteria is required." }]}><Input.TextArea rows={3} /></Form.Item>
          <Form.Item label="Planned At" name="plannedAt" rules={[{ required: true, message: "Planned date is required." }]}><DatePicker showTime style={{ width: "100%" }} /></Form.Item>
          <Form.Item label="Owner" name="ownerUserId" rules={[{ required: true, message: "Owner is required." }]}><Input placeholder="auditor@example.com" /></Form.Item>
        </Form>
      </Modal>

      <Modal
        title="Audit plan detail"
        open={Boolean(detailPlanId)}
        onCancel={() => setDetailPlanId(null)}
        footer={[
          <Button key="add-finding" disabled={!canManage || !detailQuery.data} onClick={() => { setSelectedPlanId(detailQuery.data?.id ?? null); setFindingOpen(true); }}>Add finding</Button>,
          <Button key="close" onClick={() => setDetailPlanId(null)}>Close</Button>,
        ]}
        width={900}
        destroyOnHidden
      >
        {detailQuery.data ? (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <Descriptions bordered size="small" column={2}>
              <Descriptions.Item label="Project">{detailQuery.data.projectName}</Descriptions.Item>
              <Descriptions.Item label="Owner">{detailQuery.data.ownerUserId}</Descriptions.Item>
              <Descriptions.Item label="Status"><Tag>{detailQuery.data.status}</Tag></Descriptions.Item>
              <Descriptions.Item label="Planned At">{dayjs(detailQuery.data.plannedAt).format("YYYY-MM-DD HH:mm")}</Descriptions.Item>
              <Descriptions.Item label="Scope" span={2}>{detailQuery.data.scope}</Descriptions.Item>
              <Descriptions.Item label="Criteria" span={2}>{detailQuery.data.criteria}</Descriptions.Item>
            </Descriptions>

            <Card size="small" title="Findings">
              <Table
                rowKey="id"
                size="small"
                pagination={false}
                dataSource={detailQuery.data.findings}
                columns={[
                  { title: "Code", dataIndex: "code", key: "code" },
                  { title: "Title", dataIndex: "title", key: "title" },
                  { title: "Severity", dataIndex: "severity", key: "severity", render: (value: string) => <Tag color={value === "high" ? "red" : value === "medium" ? "gold" : "blue"}>{value}</Tag> },
                  { title: "Status", dataIndex: "status", key: "status", render: (value: string) => <Tag>{value}</Tag> },
                  { title: "Owner", dataIndex: "ownerUserId", key: "ownerUserId" },
                  {
                    title: "Action",
                    key: "action",
                    render: (_, item) => (
                      <Button
                        size="small"
                        disabled={!canManage || item.status === "closed"}
                        onClick={() => void closeFindingMutation.mutateAsync({ id: item.id, input: { resolutionSummary: "Closed from audit plan detail" } }).then(() => {
                          void messageApi.success("Audit finding closed.");
                        }).catch((error: unknown) => handleMutationError(error, "Unable to close audit finding"))}
                      >
                        Close
                      </Button>
                    ),
                  },
                ]}
              />
            </Card>

            <Card size="small" title="History">
              <Space direction="vertical" size={8} style={{ width: "100%" }}>
                {detailQuery.data.history.map((item) => (
                  <Card key={item.id} size="small">
                    <Flex justify="space-between" gap={8} wrap="wrap">
                      <Space direction="vertical" size={0}>
                        <Text strong>{item.summary ?? item.eventType}</Text>
                        <Text type="secondary">{item.actorEmail ?? item.actorUserId ?? "system"}</Text>
                      </Space>
                      <Text type="secondary">{dayjs(item.occurredAt).format("YYYY-MM-DD HH:mm")}</Text>
                    </Flex>
                  </Card>
                ))}
              </Space>
            </Card>
          </Space>
        ) : null}
      </Modal>

      <Modal title="Create audit finding" open={findingOpen} onOk={() => void handleCreateFinding()} onCancel={() => setFindingOpen(false)} confirmLoading={createFindingMutation.isPending} destroyOnHidden>
        <Form form={findingForm} layout="vertical">
          <Form.Item label="Code" name="code" rules={[{ required: true, message: "Code is required." }]}><Input placeholder="F-001" /></Form.Item>
          <Form.Item label="Title" name="title" rules={[{ required: true, message: "Title is required." }]}><Input /></Form.Item>
          <Form.Item label="Description" name="description" rules={[{ required: true, message: "Description is required." }]}><Input.TextArea rows={3} /></Form.Item>
          <Form.Item label="Severity" name="severity" rules={[{ required: true, message: "Severity is required." }]}><Select options={["low", "medium", "high"].map((value) => ({ label: value, value }))} /></Form.Item>
          <Form.Item label="Owner" name="ownerUserId" rules={[{ required: true, message: "Owner is required." }]}><Input placeholder="owner@example.com" /></Form.Item>
          <Form.Item label="Due Date" name="dueDate"><DatePicker style={{ width: "100%" }} /></Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
