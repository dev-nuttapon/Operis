import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { BranchesOutlined, PlusOutlined } from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useProjectOptions } from "../../users";
import { useBaselineRegistry, useChangeRequestActions, useChangeRequests, useCreateChangeRequest } from "../hooks/useChangeControl";
import type { ChangeRequestFormInput, ChangeRequestListItem } from "../types/changeControl";

const { Title, Paragraph } = Typography;

export function ChangeRequestRegisterPage() {
  const navigate = useNavigate();
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.changeControl.read);
  const canManage = permissionState.hasPermission(permissions.changeControl.manage);
  const canApprove = permissionState.hasPermission(permissions.changeControl.approve);
  const [messageApi, contextHolder] = message.useMessage();
  const [form] = Form.useForm<ChangeRequestFormInput>();
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [filters, setFilters] = useState({ search: "", projectId: undefined as string | undefined, status: undefined as string | undefined, priority: undefined as string | undefined, page: 1, pageSize: 10 });
  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 20 });
  const baselinesQuery = useBaselineRegistry({ projectId: filters.projectId, page: 1, pageSize: 100 }, canRead);
  const changeRequestsQuery = useChangeRequests(filters, canRead);
  const createMutation = useCreateChangeRequest();
  const actions = useChangeRequestActions();

  const columns = useMemo<ColumnsType<ChangeRequestListItem>>(
    () => [
      { title: "Code", dataIndex: "code", key: "code" },
      { title: "Title", dataIndex: "title", key: "title" },
      { title: "Project", dataIndex: "projectName", key: "projectName" },
      { title: "Priority", dataIndex: "priority", key: "priority", render: (value) => <Tag>{value}</Tag> },
      { title: "Requester", dataIndex: "requestedBy", key: "requestedBy" },
      { title: "Status", dataIndex: "status", key: "status", render: (value) => <Tag color={value === "approved" ? "green" : value === "rejected" ? "red" : "default"}>{value}</Tag> },
      { title: "Target Baseline", dataIndex: "targetBaselineName", key: "targetBaselineName", render: (value) => value ?? "-" },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => (
          <Flex gap={8} wrap>
            <Button size="small" onClick={() => navigate(`/app/change-control/change-requests/${item.id}`)}>View</Button>
            <Button size="small" disabled={!canManage || item.status !== "draft"} onClick={() => void actions.submit.mutateAsync(item.id)}>Submit</Button>
            <Button size="small" disabled={!canApprove || item.status !== "submitted"} onClick={() => void actions.approve.mutateAsync({ id: item.id, reason: "Approved from register" })}>Approve</Button>
          </Flex>
        ),
      },
    ],
    [actions.approve, actions.submit, canApprove, canManage, navigate],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Change request access is not available for this account." />;
  }

  const handleCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      void messageApi.success("Change request created.");
      setIsCreateOpen(false);
      form.resetFields();
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create change request");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #991b1b, #111827)", color: "#fff" }}>
            <BranchesOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Change Request Register</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>Governed change requests with impact analysis, baseline targeting, and approval state.</Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search code or title" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
            <Select allowClear showSearch placeholder="Project" style={{ width: 220 }} options={projectOptions.options} value={filters.projectId} onSearch={projectOptions.onSearch} onPopupScroll={(event) => { const target = event.target as HTMLDivElement; if (target.scrollTop + target.clientHeight >= target.scrollHeight - 24) { projectOptions.onLoadMore(); } }} onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["draft", "submitted", "approved", "rejected", "implemented", "closed"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Select allowClear placeholder="Priority" style={{ width: 160 }} options={["low", "medium", "high", "critical"].map((value) => ({ label: value, value }))} value={filters.priority} onChange={(value) => setFilters((current) => ({ ...current, priority: value, page: 1 }))} />
          </Flex>
          <Flex gap={8}>
            <Button onClick={() => navigate("/app/change-control/configuration-items")}>Configuration Items</Button>
            <Button onClick={() => navigate("/app/change-control/baseline-registry")}>Baseline Registry</Button>
            <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setIsCreateOpen(true)}>New CR</Button>
          </Flex>
        </Flex>
        <Table<ChangeRequestListItem>
          rowKey="id"
          loading={changeRequestsQuery.isLoading}
          dataSource={changeRequestsQuery.data?.items ?? []}
          columns={columns}
          pagination={{ current: changeRequestsQuery.data?.page ?? filters.page, pageSize: changeRequestsQuery.data?.pageSize ?? filters.pageSize, total: changeRequestsQuery.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }}
        />
      </Card>

      <Modal open={isCreateOpen} title="Create change request" onOk={() => void handleCreate()} onCancel={() => setIsCreateOpen(false)} confirmLoading={createMutation.isPending} width={760}>
        <Form form={form} layout="vertical" initialValues={{ priority: "medium", impact: {} }}>
          <Flex gap={16} wrap="wrap">
            <Form.Item label="Project" name="projectId" rules={[{ required: true }]} style={{ minWidth: 240, flex: "1 1 240px" }}>
              <Select showSearch options={projectOptions.options} onSearch={projectOptions.onSearch} onPopupScroll={(event) => { const target = event.target as HTMLDivElement; if (target.scrollTop + target.clientHeight >= target.scrollHeight - 24) { projectOptions.onLoadMore(); } }} />
            </Form.Item>
            <Form.Item label="Target Baseline" name="targetBaselineId" style={{ minWidth: 240, flex: "1 1 240px" }}>
              <Select allowClear options={(baselinesQuery.data?.items ?? []).map((item) => ({ label: `${item.baselineName} · ${item.baselineType}`, value: item.id }))} />
            </Form.Item>
          </Flex>
          <Flex gap={16} wrap="wrap">
            <Form.Item label="Code" name="code" rules={[{ required: true }]} style={{ minWidth: 180, flex: "1 1 180px" }}><Input placeholder="CR-001" /></Form.Item>
            <Form.Item label="Priority" name="priority" rules={[{ required: true }]} style={{ minWidth: 180, flex: "1 1 180px" }}><Select options={["low", "medium", "high", "critical"].map((value) => ({ label: value, value }))} /></Form.Item>
            <Form.Item label="Requested By" name="requestedBy" rules={[{ required: true }]} style={{ minWidth: 220, flex: "1 1 220px" }}><Input placeholder="pm@example.com" /></Form.Item>
          </Flex>
          <Form.Item label="Title" name="title" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item label="Reason" name="reason" rules={[{ required: true }]}><Input.TextArea rows={3} /></Form.Item>
          <Typography.Title level={5}>Impact Analysis</Typography.Title>
          <Form.Item label="Scope Impact" name={["impact", "scopeImpact"]} rules={[{ required: true }]}><Input.TextArea rows={2} /></Form.Item>
          <Form.Item label="Schedule Impact" name={["impact", "scheduleImpact"]} rules={[{ required: true }]}><Input.TextArea rows={2} /></Form.Item>
          <Form.Item label="Quality Impact" name={["impact", "qualityImpact"]} rules={[{ required: true }]}><Input.TextArea rows={2} /></Form.Item>
          <Form.Item label="Security Impact" name={["impact", "securityImpact"]} rules={[{ required: true }]}><Input.TextArea rows={2} /></Form.Item>
          <Form.Item label="Performance Impact" name={["impact", "performanceImpact"]} rules={[{ required: true }]}><Input.TextArea rows={2} /></Form.Item>
          <Form.Item label="Risk Impact" name={["impact", "riskImpact"]} rules={[{ required: true }]}><Input.TextArea rows={2} /></Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
