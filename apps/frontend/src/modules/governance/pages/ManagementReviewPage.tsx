import { useMemo, useState } from "react";
import { Alert, Button, Card, DatePicker, Flex, Form, Input, Modal, Select, Space, Switch, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { FormInstance } from "antd";
import { CalendarOutlined, PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { Link } from "react-router-dom";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useProjectList } from "../../users/public";
import { useCreateManagementReview, useManagementReviews, useUpdateManagementReview } from "../hooks/useGovernance";
import type { ManagementReviewFormInput, ManagementReviewListItem } from "../types/governance";

const { Title, Paragraph, Text } = Typography;

const reviewStatuses = ["draft", "scheduled", "in_review", "closed", "archived"];
const itemTypeOptions = ["agenda", "decision", "risk", "issue"];
const itemStatusOptions = ["open", "noted", "closed"];
const actionStatusOptions = ["open", "in_progress", "closed"];

export function ManagementReviewPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(
    permissions.governance.managementReviewRead,
    permissions.governance.managementReviewManage,
    permissions.governance.managementReviewApprove,
  );
  const canManage = permissionState.hasPermission(permissions.governance.managementReviewManage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({
    search: "",
    status: undefined as string | undefined,
    projectId: undefined as string | undefined,
    facilitatorUserId: undefined as string | undefined,
    scheduledFrom: undefined as string | undefined,
    scheduledTo: undefined as string | undefined,
    page: 1,
    pageSize: 25,
  });
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<ManagementReviewListItem | null>(null);
  const [form] = Form.useForm<ManagementReviewFormInput>();
  const query = useManagementReviews(filters, canRead);
  const projectsQuery = useProjectList({ page: 1, pageSize: 100 });
  const createMutation = useCreateManagementReview();
  const updateMutation = useUpdateManagementReview();

  const columns = useMemo<ColumnsType<ManagementReviewListItem>>(
    () => [
      {
        title: "Review",
        key: "review",
        render: (_, item) => (
          <Space direction="vertical" size={0}>
            <Link to={`/app/governance/management-reviews/${item.id}`}>{item.reviewCode}</Link>
            <Text type="secondary">{item.title}</Text>
          </Space>
        ),
      },
      { title: "Project", dataIndex: "projectName", render: (value, item) => value ?? item.projectId ?? "-" },
      { title: "Period", dataIndex: "reviewPeriod" },
      { title: "Scheduled", dataIndex: "scheduledAt", render: (value: string) => dayjs(value).format("DD MMM YYYY HH:mm") },
      { title: "Facilitator", dataIndex: "facilitatorUserId" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "closed" ? "green" : value === "in_review" ? "blue" : value === "scheduled" ? "gold" : "default"}>{value}</Tag> },
      { title: "Open Actions", dataIndex: "openActionCount" },
      { title: "Actions", key: "actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button> },
    ],
    [canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Management review data is not available for this account." />;
  }

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      setCreateOpen(false);
      form.resetFields();
      void messageApi.success("Management review created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create management review");
      void messageApi.error(presentation.description);
    }
  };

  const submitUpdate = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({ id: editing.id, input: values });
      setEditing(null);
      form.resetFields();
      void messageApi.success("Management review updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update management review");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #1d4ed8, #0f766e)", color: "#fff" }}>
            <CalendarOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Management Reviews</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Govern recurring management review cadence, capture minutes and decisions, and block close until mandatory follow-up actions are complete.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search
              allowClear
              placeholder="Search code, title, or period"
              style={{ width: 240 }}
              value={filters.search}
              onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))}
              onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))}
            />
            <Select
              allowClear
              placeholder="Project"
              style={{ width: 220 }}
              options={(projectsQuery.data?.items ?? []).map((project) => ({ value: project.id, label: `${project.code} · ${project.name}` }))}
              value={filters.projectId}
              onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))}
              showSearch
              optionFilterProp="label"
            />
            <Select
              allowClear
              placeholder="Status"
              style={{ width: 180 }}
              options={reviewStatuses.map((value) => ({ value, label: value }))}
              value={filters.status}
              onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))}
            />
            <Input
              allowClear
              placeholder="Facilitator user id"
              style={{ width: 220 }}
              value={filters.facilitatorUserId}
              onChange={(event) => setFilters((current) => ({ ...current, facilitatorUserId: event.target.value || undefined, page: 1 }))}
            />
            <DatePicker
              placeholder="Scheduled from"
              value={filters.scheduledFrom ? dayjs(filters.scheduledFrom) : null}
              onChange={(value) => setFilters((current) => ({ ...current, scheduledFrom: value ? value.format("YYYY-MM-DD") : undefined, page: 1 }))}
            />
            <DatePicker
              placeholder="Scheduled to"
              value={filters.scheduledTo ? dayjs(filters.scheduledTo) : null}
              onChange={(value) => setFilters((current) => ({ ...current, scheduledTo: value ? value.format("YYYY-MM-DD") : undefined, page: 1 }))}
            />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New management review</Button>
        </Flex>

        <Table
          rowKey="id"
          loading={query.isLoading}
          columns={columns}
          dataSource={query.data?.items ?? []}
          pagination={{
            current: query.data?.page ?? filters.page,
            pageSize: query.data?.pageSize ?? filters.pageSize,
            total: query.data?.total ?? 0,
            showSizeChanger: true,
            onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })),
          }}
        />
      </Card>

      <Modal title="Create management review" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} width={920} destroyOnHidden>
        <ManagementReviewForm form={form} projectOptions={(projectsQuery.data?.items ?? []).map((project) => ({ value: project.id, label: `${project.code} · ${project.name}` }))} />
      </Modal>

      <Modal
        title="Edit management review"
        open={Boolean(editing)}
        onOk={() => void submitUpdate()}
        onCancel={() => { setEditing(null); form.resetFields(); }}
        confirmLoading={updateMutation.isPending}
        width={920}
        destroyOnHidden
        afterOpenChange={(open) => {
          if (open && editing) {
            form.setFieldsValue({
              projectId: editing.projectId ?? undefined,
              reviewCode: editing.reviewCode,
              title: editing.title,
              reviewPeriod: editing.reviewPeriod,
              scheduledAt: dayjs(editing.scheduledAt) as unknown as string,
              facilitatorUserId: editing.facilitatorUserId,
            });
          }
        }}
      >
        <ManagementReviewForm form={form} projectOptions={(projectsQuery.data?.items ?? []).map((project) => ({ value: project.id, label: `${project.code} · ${project.name}` }))} />
      </Modal>
    </Space>
  );
}

function ManagementReviewForm({ form, projectOptions }: { form: FormInstance<ManagementReviewFormInput>; projectOptions: Array<{ value: string; label: string }> }) {
  return (
    <Form form={form} layout="vertical" initialValues={{ items: [{ itemType: "agenda", status: "open" }], actions: [{ status: "open", isMandatory: true }] }}>
      <Form.Item label="Project" name="projectId">
        <Select allowClear options={projectOptions} showSearch optionFilterProp="label" />
      </Form.Item>
      <Form.Item label="Review Code" name="reviewCode" rules={[{ required: true, message: "Review code is required." }]}>
        <Input placeholder="MR-2026-Q1" />
      </Form.Item>
      <Form.Item label="Title" name="title" rules={[{ required: true, message: "Title is required." }]}>
        <Input />
      </Form.Item>
      <Form.Item label="Review Period" name="reviewPeriod" rules={[{ required: true, message: "Review period is required." }]}>
        <Input placeholder="2026 Q1" />
      </Form.Item>
      <Form.Item label="Scheduled At" name="scheduledAt" rules={[{ required: true, message: "Scheduled time is required." }]}>
        <DatePicker showTime style={{ width: "100%" }} />
      </Form.Item>
      <Form.Item label="Facilitator User Id" name="facilitatorUserId" rules={[{ required: true, message: "Facilitator is required." }]}>
        <Input />
      </Form.Item>
      <Form.Item label="Agenda Summary" name="agendaSummary">
        <Input.TextArea rows={3} />
      </Form.Item>
      <Form.Item label="Minutes Summary" name="minutesSummary">
        <Input.TextArea rows={3} />
      </Form.Item>
      <Form.Item label="Decision Summary" name="decisionSummary">
        <Input.TextArea rows={3} />
      </Form.Item>
      <Form.Item label="Escalation Entity Type" name="escalationEntityType">
        <Select allowClear options={["capa", "escalation", "risk"].map((value) => ({ value, label: value }))} />
      </Form.Item>
      <Form.Item label="Escalation Entity Id" name="escalationEntityId">
        <Input />
      </Form.Item>

      <Form.List name="items">
        {(fields, { add, remove }) => (
          <Card size="small" title="Agenda & Decision Items" extra={<Button size="small" onClick={() => add({ itemType: "agenda", status: "open" })}>Add Item</Button>}>
            <Space direction="vertical" size={12} style={{ width: "100%" }}>
              {fields.map((field) => (
                <Card key={field.key} size="small">
                  <Form.Item label="Type" name={[field.name, "itemType"]} rules={[{ required: true }]}>
                    <Select options={itemTypeOptions.map((value) => ({ value, label: value }))} />
                  </Form.Item>
                  <Form.Item label="Title" name={[field.name, "title"]} rules={[{ required: true }]}>
                    <Input />
                  </Form.Item>
                  <Form.Item label="Summary" name={[field.name, "summary"]}>
                    <Input.TextArea rows={2} />
                  </Form.Item>
                  <Form.Item label="Decision" name={[field.name, "decision"]}>
                    <Input.TextArea rows={2} />
                  </Form.Item>
                  <Form.Item label="Owner User Id" name={[field.name, "ownerUserId"]}>
                    <Input />
                  </Form.Item>
                  <Form.Item label="Due At" name={[field.name, "dueAt"]}>
                    <DatePicker showTime style={{ width: "100%" }} />
                  </Form.Item>
                  <Form.Item label="Status" name={[field.name, "status"]}>
                    <Select options={itemStatusOptions.map((value) => ({ value, label: value }))} />
                  </Form.Item>
                  <Button danger size="small" onClick={() => remove(field.name)}>Remove Item</Button>
                </Card>
              ))}
            </Space>
          </Card>
        )}
      </Form.List>

      <Form.List name="actions">
        {(fields, { add, remove }) => (
          <Card size="small" title="Follow-up Actions" extra={<Button size="small" onClick={() => add({ status: "open", isMandatory: true })}>Add Action</Button>}>
            <Space direction="vertical" size={12} style={{ width: "100%" }}>
              {fields.map((field) => (
                <Card key={field.key} size="small">
                  <Form.Item label="Title" name={[field.name, "title"]} rules={[{ required: true }]}>
                    <Input />
                  </Form.Item>
                  <Form.Item label="Description" name={[field.name, "description"]}>
                    <Input.TextArea rows={2} />
                  </Form.Item>
                  <Form.Item label="Owner User Id" name={[field.name, "ownerUserId"]} rules={[{ required: true }]}>
                    <Input />
                  </Form.Item>
                  <Form.Item label="Due At" name={[field.name, "dueAt"]}>
                    <DatePicker showTime style={{ width: "100%" }} />
                  </Form.Item>
                  <Form.Item label="Status" name={[field.name, "status"]}>
                    <Select options={actionStatusOptions.map((value) => ({ value, label: value }))} />
                  </Form.Item>
                  <Form.Item label="Mandatory" name={[field.name, "isMandatory"]} valuePropName="checked">
                    <Switch checkedChildren="Yes" unCheckedChildren="No" />
                  </Form.Item>
                  <Form.Item label="Linked Entity Type" name={[field.name, "linkedEntityType"]}>
                    <Select allowClear options={["capa", "escalation", "risk"].map((value) => ({ value, label: value }))} />
                  </Form.Item>
                  <Form.Item label="Linked Entity Id" name={[field.name, "linkedEntityId"]}>
                    <Input />
                  </Form.Item>
                  <Button danger size="small" onClick={() => remove(field.name)}>Remove Action</Button>
                </Card>
              ))}
            </Space>
          </Card>
        )}
      </Form.List>
    </Form>
  );
}
