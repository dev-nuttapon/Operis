import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { FileSearchOutlined, PlusOutlined } from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useProjectOptions } from "../../users";
import { useCreateRequirement, useRequirementActions, useRequirements } from "../hooks/useRequirements";
import type { RequirementFormInput, RequirementListItem } from "../types/requirements";

const { Title, Paragraph, Text } = Typography;

const statusColors: Record<string, string> = {
  draft: "default",
  review: "gold",
  approved: "green",
  baselined: "blue",
  superseded: "purple",
};

const priorityOptions = ["low", "medium", "high", "critical"].map((value) => ({ label: value, value }));

export function RequirementRegisterPage() {
  const navigate = useNavigate();
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.requirements.read);
  const canManage = permissionState.hasPermission(permissions.requirements.manage);
  const canApprove = permissionState.hasPermission(permissions.requirements.approve);
  const canBaseline = permissionState.hasPermission(permissions.requirements.baseline);
  const [messageApi, contextHolder] = message.useMessage();
  const [form] = Form.useForm<RequirementFormInput>();
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [filters, setFilters] = useState({
    search: "",
    projectId: undefined as string | undefined,
    priority: undefined as string | undefined,
    status: undefined as string | undefined,
    baselineStatus: undefined as string | undefined,
    missingDownstreamLinks: undefined as boolean | undefined,
    page: 1,
    pageSize: 10,
  });

  const requirementsQuery = useRequirements(filters, canRead);
  const createRequirementMutation = useCreateRequirement();
  const actions = useRequirementActions();
  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 20 });

  const columns = useMemo<ColumnsType<RequirementListItem>>(
    () => [
      {
        title: "Requirement",
        key: "requirement",
        render: (_, item) => (
          <Space direction="vertical" size={0}>
            <Text strong>{item.code}</Text>
            <Text>{item.title}</Text>
          </Space>
        ),
      },
      { title: "Project", dataIndex: "projectName", key: "projectName" },
      { title: "Owner", dataIndex: "ownerUserId", key: "ownerUserId" },
      { title: "Priority", dataIndex: "priority", key: "priority", render: (value) => <Tag>{value}</Tag> },
      { title: "Missing Links", dataIndex: "missingLinkCount", key: "missingLinkCount" },
      {
        title: "Baseline",
        dataIndex: "baselineStatus",
        key: "baselineStatus",
        render: (value) => (value ? <Tag color="blue">{value}</Tag> : <Text type="secondary">not baselined</Text>),
      },
      {
        title: "Status",
        dataIndex: "status",
        key: "status",
        render: (value) => <Tag color={statusColors[value] ?? "default"}>{value}</Tag>,
      },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => (
          <Flex gap={8} wrap>
            <Button size="small" onClick={() => navigate(`/app/requirements/${item.id}`)}>
              View
            </Button>
            <Button
              size="small"
              disabled={!canManage || item.status !== "draft"}
              onClick={() =>
                void actions.submit.mutateAsync(item.id).then(() => {
                  void messageApi.success("Requirement submitted for review.");
                }).catch((error) => {
                  const presentation = getApiErrorPresentation(error, "Unable to submit requirement");
                  void messageApi.error(presentation.description);
                })
              }
            >
              Submit
            </Button>
            <Button
              size="small"
              disabled={!canApprove || item.status !== "review"}
              onClick={() =>
                Modal.confirm({
                  title: `Approve ${item.code}`,
                  content: "Approve this requirement and move it to the approved state.",
                  onOk: async () => {
                    await actions.approve.mutateAsync({ requirementId: item.id, reason: "Approved from register" });
                    void messageApi.success("Requirement approved.");
                  },
                })
              }
            >
              Approve
            </Button>
            <Button
              size="small"
              disabled={!canBaseline || item.status !== "approved"}
              onClick={() =>
                void actions.baseline.mutateAsync(item.id).then(() => {
                  void messageApi.success("Requirement baselined.");
                }).catch((error) => {
                  const presentation = getApiErrorPresentation(error, "Unable to baseline requirement");
                  void messageApi.error(presentation.description);
                })
              }
            >
              Baseline
            </Button>
          </Flex>
        ),
      },
    ],
    [actions.approve, actions.baseline, actions.submit, canApprove, canBaseline, canManage, messageApi, navigate],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Requirement access is not available for this account." />;
  }

  const handleCreate = async () => {
    const values = await form.validateFields();
    try {
      await createRequirementMutation.mutateAsync(values);
      void messageApi.success("Requirement created.");
      setIsCreateOpen(false);
      form.resetFields();
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create requirement");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div
            style={{
              width: 48,
              height: 48,
              borderRadius: 14,
              display: "grid",
              placeItems: "center",
              background: "linear-gradient(135deg, #1d4ed8, #0f172a)",
              color: "#fff",
            }}
          >
            <FileSearchOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>
              Requirement Register
            </Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Track requirements, downstream traceability, and baseline readiness before gate approvals.
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
              onPopupScroll={(event) => {
                const target = event.target as HTMLDivElement;
                if (target.scrollTop + target.clientHeight >= target.scrollHeight - 24) {
                  projectOptions.onLoadMore();
                }
              }}
              onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))}
            />
            <Select allowClear placeholder="Priority" style={{ width: 160 }} options={priorityOptions} value={filters.priority} onChange={(value) => setFilters((current) => ({ ...current, priority: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 160 }} options={["draft", "review", "approved", "baselined", "superseded"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Select allowClear placeholder="Baseline" style={{ width: 160 }} options={[{ label: "locked", value: "locked" }, { label: "not baselined", value: "none" }]} value={filters.baselineStatus} onChange={(value) => setFilters((current) => ({ ...current, baselineStatus: value, page: 1 }))} />
            <Select allowClear placeholder="Traceability" style={{ width: 180 }} options={[{ label: "Missing links", value: "missing" }, { label: "Complete links", value: "complete" }]} onChange={(value) => setFilters((current) => ({ ...current, missingDownstreamLinks: value === undefined ? undefined : value === "missing", page: 1 }))} />
          </Flex>
          <Flex gap={8}>
            <Button onClick={() => navigate("/app/requirements/baselines")}>Baselines</Button>
            <Button onClick={() => navigate("/app/requirements/traceability")}>Traceability Matrix</Button>
            <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setIsCreateOpen(true)}>
              New requirement
            </Button>
          </Flex>
        </Flex>

        <Table<RequirementListItem>
          rowKey="id"
          loading={requirementsQuery.isLoading}
          columns={columns}
          dataSource={requirementsQuery.data?.items ?? []}
          pagination={{
            current: requirementsQuery.data?.page ?? filters.page,
            pageSize: requirementsQuery.data?.pageSize ?? filters.pageSize,
            total: requirementsQuery.data?.total ?? 0,
            showSizeChanger: true,
            onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })),
          }}
        />
      </Card>

      <Modal
        title="Create requirement"
        open={isCreateOpen}
        onOk={() => void handleCreate()}
        onCancel={() => setIsCreateOpen(false)}
        okText="Create"
        confirmLoading={createRequirementMutation.isPending}
        destroyOnHidden
      >
        <Form form={form} layout="vertical" initialValues={{ priority: "medium" }}>
          <Form.Item label="Project" name="projectId" rules={[{ required: true, message: "Project is required." }]}>
            <Select
              showSearch
              options={projectOptions.options}
              onSearch={projectOptions.onSearch}
              onPopupScroll={(event) => {
                const target = event.target as HTMLDivElement;
                if (target.scrollTop + target.clientHeight >= target.scrollHeight - 24) {
                  projectOptions.onLoadMore();
                }
              }}
            />
          </Form.Item>
          <Form.Item label="Code" name="code" rules={[{ required: true }]}>
            <Input placeholder="REQ-001" />
          </Form.Item>
          <Form.Item label="Title" name="title" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item label="Description" name="description" rules={[{ required: true }]}>
            <Input.TextArea rows={3} />
          </Form.Item>
          <Form.Item label="Priority" name="priority" rules={[{ required: true }]}>
            <Select options={priorityOptions} />
          </Form.Item>
          <Form.Item label="Owner User Id" name="ownerUserId" rules={[{ required: true }]}>
            <Input placeholder="ba@example.com" />
          </Form.Item>
          <Form.Item label="Business Reason" name="businessReason" rules={[{ required: true }]}>
            <Input.TextArea rows={2} />
          </Form.Item>
          <Form.Item label="Acceptance Criteria" name="acceptanceCriteria" rules={[{ required: true }]}>
            <Input.TextArea rows={3} />
          </Form.Item>
          <Form.Item label="Security Impact" name="securityImpact">
            <Input.TextArea rows={2} />
          </Form.Item>
          <Form.Item label="Performance Impact" name="performanceImpact">
            <Input.TextArea rows={2} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
