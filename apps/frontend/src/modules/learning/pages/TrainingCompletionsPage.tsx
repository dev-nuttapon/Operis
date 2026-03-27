import { useState } from "react";
import dayjs from "dayjs";
import { Alert, Button, Card, DatePicker, Flex, Form, Input, Modal, Select, Space, Switch, Table, Tag, Typography, message } from "antd";
import { CheckCircleOutlined, TeamOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useProjectList } from "../../users/public";
import { useCompetencyReviews, useCreateCompetencyReview, useProjectRoleOptions, useRecordTrainingCompletion, useTrainingCompletions, useTrainingCourses, useUpdateCompetencyReview, useUpdateTrainingCompletion } from "../hooks/useLearning";
import type { CreateCompetencyReviewInput, RecordTrainingCompletionInput, TrainingCompletionItem, UpdateCompetencyReviewInput, UpdateTrainingCompletionInput } from "../types/learning";

const { Title, Paragraph, Text } = Typography;

export function TrainingCompletionsPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.learning.read, permissions.learning.manage, permissions.learning.approve);
  const canManage = permissionState.hasPermission(permissions.learning.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ projectId: undefined as string | undefined, projectRoleId: undefined as string | undefined, courseId: undefined as string | undefined, userId: undefined as string | undefined, status: undefined as string | undefined, onlyOverdue: false, search: "", page: 1, pageSize: 25 });
  const [completionOpen, setCompletionOpen] = useState(false);
  const [competencyOpen, setCompetencyOpen] = useState(false);
  const [editingCompletion, setEditingCompletion] = useState<TrainingCompletionItem | null>(null);
  const [editingReviewId, setEditingReviewId] = useState<string | null>(null);
  const [completionForm] = Form.useForm<RecordTrainingCompletionInput>();
  const [competencyForm] = Form.useForm<CreateCompetencyReviewInput & { status?: string; completedAt?: dayjs.Dayjs }>();
  const projectsQuery = useProjectList({ page: 1, pageSize: 100 });
  const roleOptionsQuery = useProjectRoleOptions(filters.projectId, canRead);
  const coursesQuery = useTrainingCourses({ page: 1, pageSize: 200 }, canRead);
  const completionsQuery = useTrainingCompletions(filters, canRead);
  const competencyQuery = useCompetencyReviews({ projectId: filters.projectId, userId: filters.userId, page: 1, pageSize: 10 }, canRead);
  const recordCompletionMutation = useRecordTrainingCompletion();
  const updateCompletionMutation = useUpdateTrainingCompletion();
  const createCompetencyMutation = useCreateCompetencyReview();
  const updateCompetencyMutation = useUpdateCompetencyReview();

  if (!canRead) {
    return <Alert type="warning" showIcon message="Training completions are not available for this account." />;
  }

  const completionColumns = [
    {
      title: "User",
      dataIndex: "userId",
      key: "userId",
      render: (value: string, item: TrainingCompletionItem) => (
        <Space direction="vertical" size={0}>
          <Text strong>{value}</Text>
          <Text type="secondary">{item.projectName}</Text>
        </Space>
      ),
    },
    { title: "Role", dataIndex: "projectRoleName", key: "projectRoleName" },
    {
      title: "Course",
      key: "course",
      render: (_: unknown, item: TrainingCompletionItem) => (
        <Space direction="vertical" size={0}>
          <Text>{item.courseTitle}</Text>
          <Text type="secondary">{item.courseCode ?? item.status}</Text>
        </Space>
      ),
    },
    { title: "Due", dataIndex: "dueAt", key: "dueAt", render: (value: string | null) => (value ? dayjs(value).format("DD MMM YYYY") : "-") },
    { title: "Completed", dataIndex: "completionDate", key: "completionDate", render: (value: string | null) => (value ? dayjs(value).format("DD MMM YYYY") : "-") },
    {
      title: "Status",
      key: "status",
      render: (_: unknown, item: TrainingCompletionItem) => (
        <Space>
          <Tag color={item.isExpired ? "red" : item.isOverdue ? "gold" : item.status === "completed" ? "green" : "default"}>{item.status}</Tag>
          {item.isOverdue ? <Tag color="orange">overdue</Tag> : null}
        </Space>
      ),
    },
    {
      title: "Actions",
      key: "actions",
      render: (_: unknown, item: TrainingCompletionItem) => <Button size="small" disabled={!canManage} onClick={() => openCompletion(item)}>Update</Button>,
    },
  ];

  const openCompletion = (item?: TrainingCompletionItem) => {
    setEditingCompletion(item ?? null);
    completionForm.setFieldsValue({
      courseId: item?.courseId,
      projectRoleId: item?.projectRoleId,
      projectId: item?.projectId,
      userId: item?.userId,
      status: item?.status === "expired" ? "completed" : item?.status ?? "assigned",
      dueAt: item?.dueAt ?? undefined,
      completionDate: item?.completionDate ?? undefined,
      evidenceRef: item?.evidenceRef ?? undefined,
      notes: item?.notes ?? undefined,
    });
    setCompletionOpen(true);
  };

  const handleCompletionSave = async () => {
    const values = await completionForm.validateFields();
    try {
      if (editingCompletion && editingCompletion.id !== "00000000-0000-0000-0000-000000000000" && editingCompletion.id !== "") {
        await updateCompletionMutation.mutateAsync({
          id: editingCompletion.id,
          input: {
            status: values.status,
            dueAt: values.dueAt ?? null,
            completionDate: values.completionDate ?? null,
            evidenceRef: values.evidenceRef ?? null,
            notes: values.notes ?? null,
          } as UpdateTrainingCompletionInput,
        });
      } else {
        await recordCompletionMutation.mutateAsync(values);
      }
      setCompletionOpen(false);
      completionForm.resetFields();
      void messageApi.success("Training completion saved.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to save training completion");
      void messageApi.error(presentation.description);
    }
  };

  const handleCompetencySave = async () => {
    const values = await competencyForm.validateFields();
    try {
      if (editingReviewId) {
        await updateCompetencyMutation.mutateAsync({
          id: editingReviewId,
          input: {
            reviewPeriod: values.reviewPeriod,
            reviewerUserId: values.reviewerUserId,
            status: values.status ?? "planned",
            plannedAt: values.plannedAt,
            completedAt: values.completedAt ? values.completedAt.toISOString() : null,
            summary: values.summary ?? null,
          } as UpdateCompetencyReviewInput,
        });
      } else {
        await createCompetencyMutation.mutateAsync({
          userId: values.userId,
          projectId: values.projectId ?? null,
          reviewPeriod: values.reviewPeriod,
          reviewerUserId: values.reviewerUserId,
          plannedAt: values.plannedAt,
          summary: values.summary ?? null,
        });
      }
      setCompetencyOpen(false);
      setEditingReviewId(null);
      competencyForm.resetFields();
      void messageApi.success("Competency review saved.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to save competency review");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #14532d, #0f172a)", color: "#fff" }}>
            <CheckCircleOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Training Completions</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Track assigned, completed, overdue, and expired training by user and project, with competency reviews alongside delivery evidence.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Select allowClear placeholder="Project" style={{ width: 220 }} options={(projectsQuery.data?.items ?? []).map((project) => ({ value: project.id, label: `${project.code} · ${project.name}` }))} value={filters.projectId} onChange={(value) => setFilters((current) => ({ ...current, projectId: value, projectRoleId: undefined, page: 1 }))} showSearch optionFilterProp="label" />
            <Select allowClear placeholder="Role" style={{ width: 220 }} options={(roleOptionsQuery.data ?? []).map((role) => ({ value: role.id, label: role.projectName ? `${role.projectName} · ${role.name}` : role.name }))} value={filters.projectRoleId} onChange={(value) => setFilters((current) => ({ ...current, projectRoleId: value, page: 1 }))} showSearch optionFilterProp="label" />
            <Select allowClear placeholder="Course" style={{ width: 220 }} options={(coursesQuery.data?.items ?? []).map((course) => ({ value: course.id, label: `${course.courseCode ?? "COURSE"} · ${course.title}` }))} value={filters.courseId} onChange={(value) => setFilters((current) => ({ ...current, courseId: value, page: 1 }))} showSearch optionFilterProp="label" />
            <Input allowClear placeholder="User ID" style={{ width: 200 }} value={filters.userId} onChange={(event) => setFilters((current) => ({ ...current, userId: event.target.value || undefined, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 160 }} options={["assigned", "completed", "expired"].map((value) => ({ value, label: value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Input.Search allowClear placeholder="Search user, course, or role" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
            <Space>
              <Switch checked={filters.onlyOverdue} onChange={(checked) => setFilters((current) => ({ ...current, onlyOverdue: checked, page: 1 }))} />
              <Text>Only overdue</Text>
            </Space>
          </Flex>
          <Flex gap={8} wrap>
            <Button disabled={!canManage} onClick={() => openCompletion()}>Record completion</Button>
            <Button type="primary" icon={<TeamOutlined />} disabled={!canManage} onClick={() => { setEditingReviewId(null); competencyForm.resetFields(); setCompetencyOpen(true); }}>New competency review</Button>
          </Flex>
        </Flex>

        <Table
          rowKey={(item) => `${item.projectId}-${item.projectRoleId}-${item.courseId}-${item.userId}`}
          loading={completionsQuery.isLoading}
          columns={completionColumns}
          dataSource={completionsQuery.data?.items ?? []}
          pagination={{
            current: completionsQuery.data?.page ?? filters.page,
            pageSize: completionsQuery.data?.pageSize ?? filters.pageSize,
            total: completionsQuery.data?.total ?? 0,
            showSizeChanger: true,
            onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })),
          }}
        />
      </Card>

      <Card variant="borderless" title="Competency Reviews">
        <Table
          rowKey="id"
          loading={competencyQuery.isLoading}
          dataSource={competencyQuery.data?.items ?? []}
          pagination={false}
          columns={[
            { title: "User", dataIndex: "userId" },
            { title: "Project", dataIndex: "projectName", render: (value: string | null) => value ?? "-" },
            { title: "Period", dataIndex: "reviewPeriod" },
            { title: "Reviewer", dataIndex: "reviewerUserId" },
            { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "completed" ? "green" : value === "in_progress" ? "blue" : "gold"}>{value}</Tag> },
            { title: "Planned", dataIndex: "plannedAt", render: (value: string) => dayjs(value).format("DD MMM YYYY") },
            { title: "Actions", key: "actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => { setEditingReviewId(item.id); competencyForm.setFieldsValue({ userId: item.userId, projectId: item.projectId ?? undefined, reviewPeriod: item.reviewPeriod, reviewerUserId: item.reviewerUserId, status: item.status, plannedAt: item.plannedAt, completedAt: item.completedAt ? dayjs(item.completedAt) : undefined, summary: item.summary ?? undefined }); setCompetencyOpen(true); }}>Edit</Button> },
          ]}
        />
      </Card>

      <Modal title={editingCompletion ? "Update training completion" : "Record training completion"} open={completionOpen} onOk={() => void handleCompletionSave()} onCancel={() => { setCompletionOpen(false); setEditingCompletion(null); completionForm.resetFields(); }} confirmLoading={recordCompletionMutation.isPending || updateCompletionMutation.isPending} destroyOnHidden>
        <Form form={completionForm} layout="vertical" initialValues={{ status: "assigned" }}>
          <Form.Item label="Project" name="projectId" rules={[{ required: true, message: "Project is required." }]}>
            <Select showSearch optionFilterProp="label" options={(projectsQuery.data?.items ?? []).map((project) => ({ value: project.id, label: `${project.code} · ${project.name}` }))} />
          </Form.Item>
          <Form.Item label="Project Role" name="projectRoleId" rules={[{ required: true, message: "Role is required." }]}>
            <Select showSearch optionFilterProp="label" options={(roleOptionsQuery.data ?? []).map((role) => ({ value: role.id, label: role.projectName ? `${role.projectName} · ${role.name}` : role.name }))} />
          </Form.Item>
          <Form.Item label="Course" name="courseId" rules={[{ required: true, message: "Course is required." }]}>
            <Select showSearch optionFilterProp="label" options={(coursesQuery.data?.items ?? []).map((course) => ({ value: course.id, label: `${course.courseCode ?? "COURSE"} · ${course.title}` }))} />
          </Form.Item>
          <Form.Item label="User ID" name="userId" rules={[{ required: true, message: "User is required." }]}>
            <Input />
          </Form.Item>
          <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}>
            <Select options={["assigned", "completed"].map((value) => ({ value, label: value }))} />
          </Form.Item>
          <Form.Item label="Due At" name="dueAt">
            <Input placeholder="2026-04-30T00:00:00Z" />
          </Form.Item>
          <Form.Item label="Completion Date" name="completionDate">
            <Input placeholder="2026-03-27T12:00:00Z" />
          </Form.Item>
          <Form.Item label="Evidence Reference" name="evidenceRef">
            <Input />
          </Form.Item>
          <Form.Item label="Notes" name="notes">
            <Input.TextArea rows={3} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal title={editingReviewId ? "Edit competency review" : "Create competency review"} open={competencyOpen} onOk={() => void handleCompetencySave()} onCancel={() => { setCompetencyOpen(false); setEditingReviewId(null); competencyForm.resetFields(); }} confirmLoading={createCompetencyMutation.isPending || updateCompetencyMutation.isPending} destroyOnHidden>
        <Form form={competencyForm} layout="vertical" initialValues={{ status: "planned" }}>
          <Form.Item label="User ID" name="userId" rules={[{ required: true, message: "User is required." }]}>
            <Input />
          </Form.Item>
          <Form.Item label="Project" name="projectId">
            <Select allowClear showSearch optionFilterProp="label" options={(projectsQuery.data?.items ?? []).map((project) => ({ value: project.id, label: `${project.code} · ${project.name}` }))} />
          </Form.Item>
          <Form.Item label="Review Period" name="reviewPeriod" rules={[{ required: true, message: "Review period is required." }]}>
            <Input placeholder="2026-Q2" />
          </Form.Item>
          <Form.Item label="Reviewer User ID" name="reviewerUserId" rules={[{ required: true, message: "Reviewer is required." }]}>
            <Input />
          </Form.Item>
          <Form.Item label="Status" name="status">
            <Select options={["planned", "in_progress", "completed", "archived"].map((value) => ({ value, label: value }))} />
          </Form.Item>
          <Form.Item label="Planned At" name="plannedAt" rules={[{ required: true, message: "Planned date is required." }]}>
            <Input placeholder="2026-04-15T09:00:00Z" />
          </Form.Item>
          <Form.Item label="Completed At" name="completedAt">
            <DatePicker showTime style={{ width: "100%" }} />
          </Form.Item>
          <Form.Item label="Summary" name="summary">
            <Input.TextArea rows={3} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
