import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { FormInstance } from "antd";
import type { ColumnsType } from "antd/es/table";
import { BookOutlined, ExportOutlined, PlusOutlined } from "@ant-design/icons";
import { useProjectOptions } from "../../users";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateLessonLearned, useLessonsLearned, usePublishLessonLearned, useUpdateLessonLearned } from "../hooks/useKnowledge";
import type { CreateLessonLearnedInput, LessonLearnedItem, UpdateLessonLearnedInput } from "../types/knowledge";

const { Title, Paragraph, Text } = Typography;

const lessonTypeOptions = [
  { label: "process", value: "process" },
  { label: "quality", value: "quality" },
  { label: "delivery", value: "delivery" },
  { label: "risk", value: "risk" },
  { label: "general", value: "general" },
];

const statusOptions = [
  { label: "draft", value: "draft" },
  { label: "reviewed", value: "reviewed" },
  { label: "published", value: "published" },
  { label: "archived", value: "archived" },
];

type LessonFormValues = CreateLessonLearnedInput & {
  status?: string;
  linkedEvidenceText?: string;
};

type PublishFormValues = {
  summary?: string;
  context?: string;
  sourceRef?: string;
  linkedEvidenceText?: string;
};

export function LessonsLearnedPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.knowledge.read, permissions.knowledge.manage);
  const canManage = permissionState.hasPermission(permissions.knowledge.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({
    projectId: undefined as string | undefined,
    lessonType: undefined as string | undefined,
    status: undefined as string | undefined,
    ownerUserId: undefined as string | undefined,
    search: "",
    page: 1,
    pageSize: 25,
  });
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<LessonLearnedItem | null>(null);
  const [publishing, setPublishing] = useState<LessonLearnedItem | null>(null);
  const [form] = Form.useForm<LessonFormValues>();
  const [publishForm] = Form.useForm<PublishFormValues>();
  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 50 });
  const query = useLessonsLearned(filters, canRead);
  const createMutation = useCreateLessonLearned();
  const updateMutation = useUpdateLessonLearned();
  const publishMutation = usePublishLessonLearned();

  const columns = useMemo<ColumnsType<LessonLearnedItem>>(
    () => [
      { title: "Title", dataIndex: "title" },
      { title: "Type", dataIndex: "lessonType", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Project", dataIndex: "projectName" },
      { title: "Owner", dataIndex: "ownerUserId" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "published" ? "green" : value === "reviewed" ? "gold" : "default"}>{value}</Tag> },
      {
        title: "Published At",
        dataIndex: "publishedAt",
        render: (value: string | null) => (value ? new Date(value).toLocaleString() : <Text type="secondary">Not published</Text>),
      },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => (
          <Space>
            <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>
              Edit
            </Button>
            <Button size="small" icon={<ExportOutlined />} disabled={!canManage || item.status !== "reviewed"} onClick={() => setPublishing(item)}>
              Publish
            </Button>
          </Space>
        ),
      },
    ],
    [canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Lessons learned are not available for this account." />;
  }

  const parseEvidence = (value?: string | null) =>
    value
      ?.split(/[\n,]+/)
      .map((item) => item.trim())
      .filter(Boolean) ?? [];

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync({
        projectId: values.projectId,
        title: values.title,
        summary: values.summary,
        lessonType: values.lessonType,
        ownerUserId: values.ownerUserId,
        sourceRef: values.sourceRef ?? null,
        context: values.context ?? null,
        whatHappened: values.whatHappened ?? null,
        whatToRepeat: values.whatToRepeat ?? null,
        whatToAvoid: values.whatToAvoid ?? null,
        linkedEvidence: parseEvidence(values.linkedEvidenceText),
      });
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Lesson learned created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create lesson learned");
      void messageApi.error(presentation.description);
    }
  };

  const submitUpdate = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({
        id: editing.id,
        input: {
          projectId: values.projectId,
          title: values.title,
          summary: values.summary,
          lessonType: values.lessonType,
          ownerUserId: values.ownerUserId,
          status: values.status ?? editing.status,
          sourceRef: values.sourceRef ?? null,
          context: values.context ?? null,
          whatHappened: values.whatHappened ?? null,
          whatToRepeat: values.whatToRepeat ?? null,
          whatToAvoid: values.whatToAvoid ?? null,
          linkedEvidence: parseEvidence(values.linkedEvidenceText),
        } as UpdateLessonLearnedInput,
      });
      form.resetFields();
      setEditing(null);
      void messageApi.success("Lesson learned updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update lesson learned");
      void messageApi.error(presentation.description);
    }
  };

  const submitPublish = async () => {
    if (!publishing) return;
    const values = await publishForm.validateFields();
    try {
      await publishMutation.mutateAsync({
        id: publishing.id,
        input: {
          summary: values.summary ?? publishing.summary,
          context: values.context ?? publishing.context,
          sourceRef: values.sourceRef ?? publishing.sourceRef,
          linkedEvidence: parseEvidence(values.linkedEvidenceText),
        },
      });
      publishForm.resetFields();
      setPublishing(null);
      void messageApi.success("Lesson learned published.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to publish lesson learned");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #14532d, #1e3a8a)", color: "#fff" }}>
            <BookOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Lessons Learned</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Capture project evidence, what to repeat, and what to avoid before publishing reusable knowledge.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Select allowClear showSearch placeholder="Project" style={{ width: 240 }} options={projectOptions.options} value={filters.projectId} onSearch={projectOptions.onSearch} onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))} />
            <Select allowClear placeholder="Type" style={{ width: 180 }} options={lessonTypeOptions} value={filters.lessonType} onChange={(value) => setFilters((current) => ({ ...current, lessonType: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={statusOptions} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Input allowClear placeholder="Owner" style={{ width: 180 }} value={filters.ownerUserId} onChange={(event) => setFilters((current) => ({ ...current, ownerUserId: event.target.value || undefined, page: 1 }))} />
            <Input.Search allowClear placeholder="Search title or project" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>
            New lesson
          </Button>
        </Flex>
        <Table
          rowKey="id"
          loading={query.isLoading}
          columns={columns}
          dataSource={query.data?.items ?? []}
          expandable={{
            expandedRowRender: (item) => (
              <Space direction="vertical" size={8} style={{ width: "100%" }}>
                <Text strong>Context</Text>
                <Text>{item.context || "-"}</Text>
                <Text strong>What happened</Text>
                <Text>{item.whatHappened || "-"}</Text>
                <Text strong>What to repeat</Text>
                <Text>{item.whatToRepeat || "-"}</Text>
                <Text strong>What to avoid</Text>
                <Text>{item.whatToAvoid || "-"}</Text>
                <Text strong>Linked evidence</Text>
                <Text>{item.linkedEvidence.length > 0 ? item.linkedEvidence.join(", ") : item.sourceRef || "-"}</Text>
              </Space>
            ),
          }}
          pagination={{
            current: query.data?.page ?? filters.page,
            pageSize: query.data?.pageSize ?? filters.pageSize,
            total: query.data?.total ?? 0,
            showSizeChanger: true,
            defaultPageSize: 25,
            onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })),
          }}
        />
      </Card>

      <Modal
        title="Create lesson learned"
        open={createOpen}
        onOk={() => void submitCreate()}
        onCancel={() => {
          setCreateOpen(false);
          form.resetFields();
        }}
        confirmLoading={createMutation.isPending}
        destroyOnHidden
      >
        <LessonForm form={form} projectOptions={projectOptions.options} />
      </Modal>

      <Modal
        title="Edit lesson learned"
        open={Boolean(editing)}
        onOk={() => void submitUpdate()}
        onCancel={() => {
          setEditing(null);
          form.resetFields();
        }}
        confirmLoading={updateMutation.isPending}
        destroyOnHidden
        afterOpenChange={(open) => {
          if (open && editing) {
            form.setFieldsValue({
              projectId: editing.projectId,
              title: editing.title,
              summary: editing.summary,
              lessonType: editing.lessonType,
              ownerUserId: editing.ownerUserId,
              status: editing.status,
              sourceRef: editing.sourceRef ?? undefined,
              context: editing.context ?? undefined,
              whatHappened: editing.whatHappened ?? undefined,
              whatToRepeat: editing.whatToRepeat ?? undefined,
              whatToAvoid: editing.whatToAvoid ?? undefined,
              linkedEvidenceText: editing.linkedEvidence.join(", "),
            });
          }
        }}
      >
        <LessonForm form={form} projectOptions={projectOptions.options} includeStatus />
      </Modal>

      <Modal
        title="Publish lesson learned"
        open={Boolean(publishing)}
        onOk={() => void submitPublish()}
        onCancel={() => {
          setPublishing(null);
          publishForm.resetFields();
        }}
        confirmLoading={publishMutation.isPending}
        destroyOnHidden
        afterOpenChange={(open) => {
          if (open && publishing) {
            publishForm.setFieldsValue({
              summary: publishing.summary,
              context: publishing.context ?? undefined,
              sourceRef: publishing.sourceRef ?? undefined,
              linkedEvidenceText: publishing.linkedEvidence.join(", "),
            });
          }
        }}
      >
        <Form form={publishForm} layout="vertical">
          <Form.Item label="Summary" name="summary">
            <Input.TextArea rows={3} />
          </Form.Item>
          <Form.Item label="Context" name="context">
            <Input.TextArea rows={3} />
          </Form.Item>
          <Form.Item label="Source Reference" name="sourceRef">
            <Input />
          </Form.Item>
          <Form.Item label="Linked Evidence" name="linkedEvidenceText" extra="Comma or line separated evidence references.">
            <Input.TextArea rows={3} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}

function LessonForm({
  form,
  projectOptions,
  includeStatus = false,
}: {
  form: FormInstance<LessonFormValues>;
  projectOptions: Array<{ label: string; value: string }>;
  includeStatus?: boolean;
}) {
  return (
    <Form form={form} layout="vertical">
      <Form.Item label="Project" name="projectId" rules={[{ required: true, message: "Project is required." }]}>
        <Select showSearch options={projectOptions} />
      </Form.Item>
      <Form.Item label="Title" name="title" rules={[{ required: true, message: "Title is required." }]}>
        <Input />
      </Form.Item>
      <Form.Item label="Summary" name="summary" rules={[{ required: true, message: "Summary is required." }]}>
        <Input.TextArea rows={3} />
      </Form.Item>
      <Form.Item label="Lesson Type" name="lessonType" rules={[{ required: true, message: "Lesson type is required." }]}>
        <Select options={lessonTypeOptions} />
      </Form.Item>
      <Form.Item label="Owner" name="ownerUserId" rules={[{ required: true, message: "Owner is required." }]}>
        <Input placeholder="pm@example.com" />
      </Form.Item>
      {includeStatus ? (
        <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}>
          <Select options={statusOptions} />
        </Form.Item>
      ) : null}
      <Form.Item label="Context" name="context">
        <Input.TextArea rows={3} />
      </Form.Item>
      <Form.Item label="What Happened" name="whatHappened">
        <Input.TextArea rows={3} />
      </Form.Item>
      <Form.Item label="What To Repeat" name="whatToRepeat">
        <Input.TextArea rows={3} />
      </Form.Item>
      <Form.Item label="What To Avoid" name="whatToAvoid">
        <Input.TextArea rows={3} />
      </Form.Item>
      <Form.Item label="Source Reference" name="sourceRef">
        <Input />
      </Form.Item>
      <Form.Item label="Linked Evidence" name="linkedEvidenceText" extra="Comma or line separated evidence references.">
        <Input.TextArea rows={3} />
      </Form.Item>
    </Form>
  );
}
