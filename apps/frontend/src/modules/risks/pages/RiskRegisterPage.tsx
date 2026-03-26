import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, InputNumber, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { AlertOutlined, PlusOutlined } from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useProjectOptions } from "../../users";
import { useCreateRisk, useRiskActions, useRisks } from "../hooks/useRisks";
import type { RiskFormInput, RiskListItem } from "../types/risks";

const { Title, Paragraph, Text } = Typography;

const statusColors: Record<string, string> = {
  draft: "default",
  assessed: "gold",
  mitigated: "blue",
  closed: "green",
};

export function RiskRegisterPage() {
  const navigate = useNavigate();
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.risks.read);
  const canManage = permissionState.hasPermission(permissions.risks.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({
    search: "",
    projectId: undefined as string | undefined,
    status: undefined as string | undefined,
    riskLevel: undefined as number | undefined,
    page: 1,
    pageSize: 10,
  });
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [form] = Form.useForm<RiskFormInput>();

  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 20 });
  const risksQuery = useRisks(filters, canRead);
  const createMutation = useCreateRisk();
  const actions = useRiskActions();

  const columns = useMemo<ColumnsType<RiskListItem>>(
    () => [
      {
        title: "Risk",
        key: "risk",
        render: (_, item) => (
          <Space direction="vertical" size={0}>
            <Text strong>{item.code}</Text>
            <Text>{item.title}</Text>
          </Space>
        ),
      },
      { title: "Project", dataIndex: "projectName", key: "projectName" },
      { title: "Owner", dataIndex: "ownerUserId", key: "ownerUserId" },
      {
        title: "Level",
        key: "level",
        render: (_, item) => <Tag>{item.probability * item.impact}</Tag>,
      },
      {
        title: "Status",
        dataIndex: "status",
        key: "status",
        render: (value) => <Tag color={statusColors[value] ?? "default"}>{value}</Tag>,
      },
      {
        title: "Next Review",
        dataIndex: "nextReviewAt",
        key: "nextReviewAt",
        render: (value) => value ? new Date(value).toLocaleString() : <Text type="secondary">not set</Text>,
      },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => (
          <Flex gap={8} wrap>
            <Button size="small" onClick={() => navigate(`/app/risks/${item.id}`)}>
              View
            </Button>
            <Button
              size="small"
              disabled={!canManage || item.status !== "draft"}
              onClick={() =>
                void actions.assess.mutateAsync({ id: item.id, input: {} }).then(() => {
                  void messageApi.success("Risk assessed.");
                }).catch((error) => {
                  const presentation = getApiErrorPresentation(error, "Unable to assess risk");
                  void messageApi.error(presentation.description);
                })
              }
            >
              Assess
            </Button>
            <Button
              size="small"
              disabled={!canManage || item.status !== "assessed"}
              onClick={() =>
                Modal.confirm({
                  title: `Mitigate ${item.code}`,
                  content: "Move this risk to mitigated.",
                  onOk: async () => {
                    await actions.mitigate.mutateAsync({ id: item.id, input: {} });
                    void messageApi.success("Risk mitigated.");
                  },
                })
              }
            >
              Mitigate
            </Button>
            <Button
              size="small"
              disabled={!canManage || item.status !== "mitigated"}
              onClick={() =>
                void actions.close.mutateAsync({ id: item.id, input: {} }).then(() => {
                  void messageApi.success("Risk closed.");
                }).catch((error) => {
                  const presentation = getApiErrorPresentation(error, "Unable to close risk");
                  void messageApi.error(presentation.description);
                })
              }
            >
              Close
            </Button>
          </Flex>
        ),
      },
    ],
    [actions.assess, actions.close, actions.mitigate, canManage, messageApi, navigate],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Risk access is not available for this account." />;
  }

  const handleCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      setIsCreateOpen(false);
      form.resetFields();
      void messageApi.success("Risk created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create risk");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #b91c1c, #1f2937)", color: "#fff" }}>
            <AlertOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Risk Register</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Track assessed risks, mitigation coverage, and review cadence before downstream delivery phases.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search
              allowClear
              placeholder="Search code, title, description"
              style={{ width: 260 }}
              value={filters.search}
              onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))}
              onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))}
            />
            <Select
              allowClear
              showSearch
              placeholder="Project"
              style={{ width: 220 }}
              options={projectOptions.options}
              value={filters.projectId}
              onSearch={projectOptions.onSearch}
              onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))}
            />
            <Select
              allowClear
              placeholder="Status"
              style={{ width: 160 }}
              options={["draft", "assessed", "mitigated", "closed"].map((value) => ({ label: value, value }))}
              value={filters.status}
              onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))}
            />
            <InputNumber
              min={1}
              max={25}
              placeholder="Risk level"
              value={filters.riskLevel}
              onChange={(value) => setFilters((current) => ({ ...current, riskLevel: value ?? undefined, page: 1 }))}
            />
          </Flex>
          <Flex gap={8}>
            <Button onClick={() => navigate("/app/issues")}>Issue Log</Button>
            <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setIsCreateOpen(true)}>
              New risk
            </Button>
          </Flex>
        </Flex>

        <Table<RiskListItem>
          rowKey="id"
          loading={risksQuery.isLoading}
          columns={columns}
          dataSource={risksQuery.data?.items ?? []}
          pagination={{
            current: risksQuery.data?.page ?? filters.page,
            pageSize: risksQuery.data?.pageSize ?? filters.pageSize,
            total: risksQuery.data?.total ?? 0,
            showSizeChanger: true,
            onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })),
          }}
        />
      </Card>

      <Modal
        title="Create risk"
        open={isCreateOpen}
        onOk={() => void handleCreate()}
        onCancel={() => setIsCreateOpen(false)}
        okText="Create"
        confirmLoading={createMutation.isPending}
        destroyOnHidden
      >
        <Form form={form} layout="vertical" initialValues={{ probability: 3, impact: 3 }}>
          <Form.Item label="Project" name="projectId" rules={[{ required: true, message: "Project is required." }]}>
            <Select showSearch options={projectOptions.options} onSearch={projectOptions.onSearch} />
          </Form.Item>
          <Form.Item label="Code" name="code" rules={[{ required: true, message: "Code is required." }]}>
            <Input />
          </Form.Item>
          <Form.Item label="Title" name="title" rules={[{ required: true, message: "Title is required." }]}>
            <Input />
          </Form.Item>
          <Form.Item label="Description" name="description" rules={[{ required: true, message: "Description is required." }]}>
            <Input.TextArea rows={4} />
          </Form.Item>
          <Flex gap={12}>
            <Form.Item label="Probability" name="probability" rules={[{ required: true, message: "Probability is required." }]} style={{ flex: 1 }}>
              <InputNumber min={1} max={5} style={{ width: "100%" }} />
            </Form.Item>
            <Form.Item label="Impact" name="impact" rules={[{ required: true, message: "Impact is required." }]} style={{ flex: 1 }}>
              <InputNumber min={1} max={5} style={{ width: "100%" }} />
            </Form.Item>
          </Flex>
          <Form.Item label="Owner" name="ownerUserId" rules={[{ required: true, message: "Owner is required." }]}>
            <Input placeholder="owner@example.com" />
          </Form.Item>
          <Form.Item label="Mitigation Plan" name="mitigationPlan">
            <Input.TextArea rows={3} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
