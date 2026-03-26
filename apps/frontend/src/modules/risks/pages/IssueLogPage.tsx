import { useMemo, useState } from "react";
import { Alert, Button, Card, Checkbox, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { BugOutlined, PlusOutlined, SafetyCertificateOutlined } from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useProjectOptions } from "../../users";
import { useCreateIssue, useIssueActions, useIssues } from "../hooks/useRisks";
import type { IssueFormInput, IssueListItem } from "../types/risks";

const { Title, Paragraph, Text } = Typography;

const severityColors: Record<string, string> = {
  low: "default",
  medium: "gold",
  high: "orange",
  critical: "red",
};

export function IssueLogPage() {
  const navigate = useNavigate();
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.risks.read);
  const canManage = permissionState.hasPermission(permissions.risks.manage);
  const canReadSensitive = permissionState.hasPermission(permissions.risks.readSensitive);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({
    search: "",
    projectId: undefined as string | undefined,
    status: undefined as string | undefined,
    severity: undefined as string | undefined,
    page: 1,
    pageSize: 10,
  });
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [form] = Form.useForm<IssueFormInput>();

  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 20 });
  const issuesQuery = useIssues(filters, canRead);
  const createMutation = useCreateIssue();
  const actions = useIssueActions();

  const columns = useMemo<ColumnsType<IssueListItem>>(
    () => [
      {
        title: "Issue",
        key: "issue",
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
        title: "Severity",
        dataIndex: "severity",
        key: "severity",
        render: (value) => <Tag color={severityColors[value] ?? "default"}>{value}</Tag>,
      },
      {
        title: "Open Actions",
        dataIndex: "openActionCount",
        key: "openActionCount",
      },
      {
        title: "Status",
        dataIndex: "status",
        key: "status",
        render: (value) => <Tag>{value}</Tag>,
      },
      {
        title: "Sensitive",
        dataIndex: "isSensitive",
        key: "isSensitive",
        render: (value) => value ? <Tag color="red">restricted</Tag> : <Text type="secondary">no</Text>,
      },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => (
          <Flex gap={8} wrap>
            <Button size="small" onClick={() => navigate(`/app/issues/${item.id}`)}>
              View
            </Button>
            <Button
              size="small"
              disabled={!canManage || item.openActionCount > 0 || item.status === "closed"}
              onClick={() =>
                void actions.resolve.mutateAsync({ id: item.id, input: {} }).then(() => {
                  void messageApi.success("Issue resolved.");
                }).catch((error) => {
                  const presentation = getApiErrorPresentation(error, "Unable to resolve issue");
                  void messageApi.error(presentation.description);
                })
              }
            >
              Resolve
            </Button>
          </Flex>
        ),
      },
    ],
    [actions.resolve, canManage, messageApi, navigate],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Issue access is not available for this account." />;
  }

  const handleCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      setIsCreateOpen(false);
      form.resetFields();
      void messageApi.success("Issue created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create issue");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #9a3412, #111827)", color: "#fff" }}>
            <BugOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Issue / Action Log</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Follow defects, blockers, and remediation actions with ownership and close-loop resolution controls.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search
              allowClear
              placeholder="Search code or title"
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
              placeholder="Severity"
              style={{ width: 160 }}
              options={["low", "medium", "high", "critical"].map((value) => ({ label: value, value }))}
              value={filters.severity}
              onChange={(value) => setFilters((current) => ({ ...current, severity: value, page: 1 }))}
            />
            <Select
              allowClear
              placeholder="Status"
              style={{ width: 160 }}
              options={["open", "in_progress", "resolved", "closed"].map((value) => ({ label: value, value }))}
              value={filters.status}
              onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))}
            />
          </Flex>
          <Flex gap={8}>
            <Button onClick={() => navigate("/app/risks")}>Risk Register</Button>
            <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setIsCreateOpen(true)}>
              New issue
            </Button>
          </Flex>
        </Flex>

        <Table<IssueListItem>
          rowKey="id"
          loading={issuesQuery.isLoading}
          columns={columns}
          dataSource={issuesQuery.data?.items ?? []}
          pagination={{
            current: issuesQuery.data?.page ?? filters.page,
            pageSize: issuesQuery.data?.pageSize ?? filters.pageSize,
            total: issuesQuery.data?.total ?? 0,
            showSizeChanger: true,
            onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })),
          }}
        />
      </Card>

      <Modal
        title="Create issue"
        open={isCreateOpen}
        onOk={() => void handleCreate()}
        onCancel={() => setIsCreateOpen(false)}
        okText="Create"
        confirmLoading={createMutation.isPending}
        destroyOnHidden
      >
        <Form form={form} layout="vertical" initialValues={{ severity: "medium", isSensitive: false }}>
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
          <Form.Item label="Owner" name="ownerUserId" rules={[{ required: true, message: "Owner is required." }]}>
            <Input placeholder="owner@example.com" />
          </Form.Item>
          <Form.Item label="Severity" name="severity" rules={[{ required: true, message: "Severity is required." }]}>
            <Select options={["low", "medium", "high", "critical"].map((value) => ({ label: value, value }))} />
          </Form.Item>
          {canReadSensitive ? (
            <>
              <Form.Item name="isSensitive" valuePropName="checked">
                <Checkbox><SafetyCertificateOutlined /> Mark as sensitive</Checkbox>
              </Form.Item>
              <Form.Item shouldUpdate noStyle>
                {() => form.getFieldValue("isSensitive") ? (
                  <Form.Item label="Sensitive context" name="sensitiveContext" rules={[{ required: true, message: "Sensitive context is required." }]}>
                    <Input placeholder="incident_linked or access_finding" />
                  </Form.Item>
                ) : null}
              </Form.Item>
            </>
          ) : null}
        </Form>
      </Modal>
    </Space>
  );
}
