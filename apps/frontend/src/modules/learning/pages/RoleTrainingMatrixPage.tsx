import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, InputNumber, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { AuditOutlined, PlusOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useProjectList } from "../../users/public";
import { useCreateRoleTrainingRequirement, useProjectRoleOptions, useRoleTrainingRequirements, useTrainingCourses, useUpdateRoleTrainingRequirement } from "../hooks/useLearning";
import type { CreateRoleTrainingRequirementInput, RoleTrainingRequirementItem, UpdateRoleTrainingRequirementInput } from "../types/learning";

const { Title, Paragraph, Text } = Typography;

export function RoleTrainingMatrixPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.learning.read, permissions.learning.manage, permissions.learning.approve);
  const canManage = permissionState.hasPermission(permissions.learning.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ projectId: undefined as string | undefined, projectRoleId: undefined as string | undefined, courseId: undefined as string | undefined, status: undefined as string | undefined, search: "", page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<RoleTrainingRequirementItem | null>(null);
  const [form] = Form.useForm<CreateRoleTrainingRequirementInput & { status?: string }>();
  const projectsQuery = useProjectList({ page: 1, pageSize: 100 });
  const roleOptionsQuery = useProjectRoleOptions(filters.projectId, canRead);
  const coursesQuery = useTrainingCourses({ page: 1, pageSize: 200 }, canRead);
  const query = useRoleTrainingRequirements(filters, canRead);
  const createMutation = useCreateRoleTrainingRequirement();
  const updateMutation = useUpdateRoleTrainingRequirement();

  const columns = useMemo<ColumnsType<RoleTrainingRequirementItem>>(
    () => [
      {
        title: "Role",
        key: "role",
        render: (_, item) => (
          <Space direction="vertical" size={0}>
            <Text strong>{item.projectRoleName}</Text>
            <Text type="secondary">{item.projectName ?? "Global role"}</Text>
          </Space>
        ),
      },
      {
        title: "Course",
        key: "course",
        render: (_, item) => (
          <Space direction="vertical" size={0}>
            <Text>{item.courseTitle}</Text>
            <Text type="secondary">{item.courseCode ?? item.courseStatus}</Text>
          </Space>
        ),
      },
      { title: "Window", render: (_, item) => `${item.requiredWithinDays}d / renew ${item.renewalIntervalMonths}m` },
      { title: "Assigned", dataIndex: "assignedUserCount" },
      { title: "Overdue", dataIndex: "overdueUserCount" },
      { title: "Expired", dataIndex: "expiredUserCount" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "active" ? "green" : "default"}>{value}</Tag> },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button>,
      },
    ],
    [canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Role training matrix is not available for this account." />;
  }

  const handleCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      setCreateOpen(false);
      form.resetFields();
      void messageApi.success("Role training requirement created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create role training requirement");
      void messageApi.error(presentation.description);
    }
  };

  const handleUpdate = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({
        id: editing.id,
        input: {
          courseId: values.courseId,
          projectRoleId: values.projectRoleId,
          requiredWithinDays: values.requiredWithinDays,
          renewalIntervalMonths: values.renewalIntervalMonths,
          notes: values.notes,
          status: values.status ?? editing.status,
        } as UpdateRoleTrainingRequirementInput,
      });
      setEditing(null);
      form.resetFields();
      void messageApi.success("Role training requirement updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update role training requirement");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #7c2d12, #1d4ed8)", color: "#fff" }}>
            <AuditOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Role Training Matrix</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Map role ownership from project structures to mandatory training requirements and track coverage gaps by role.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Select
              allowClear
              placeholder="Project"
              style={{ width: 240 }}
              options={(projectsQuery.data?.items ?? []).map((project) => ({ value: project.id, label: `${project.code} · ${project.name}` }))}
              value={filters.projectId}
              onChange={(value) => setFilters((current) => ({ ...current, projectId: value, projectRoleId: undefined, page: 1 }))}
              showSearch
              optionFilterProp="label"
            />
            <Select
              allowClear
              placeholder="Role"
              style={{ width: 240 }}
              options={(roleOptionsQuery.data ?? []).map((role) => ({ value: role.id, label: role.projectName ? `${role.projectName} · ${role.name}` : role.name }))}
              value={filters.projectRoleId}
              onChange={(value) => setFilters((current) => ({ ...current, projectRoleId: value, page: 1 }))}
              showSearch
              optionFilterProp="label"
            />
            <Select
              allowClear
              placeholder="Course"
              style={{ width: 240 }}
              options={(coursesQuery.data?.items ?? []).map((course) => ({ value: course.id, label: `${course.courseCode ?? "COURSE"} · ${course.title}` }))}
              value={filters.courseId}
              onChange={(value) => setFilters((current) => ({ ...current, courseId: value, page: 1 }))}
              showSearch
              optionFilterProp="label"
            />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["active", "archived"].map((value) => ({ value, label: value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Input.Search allowClear placeholder="Search role or course" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New requirement</Button>
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

      <Modal title="Create role training requirement" open={createOpen} onOk={() => void handleCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <RoleTrainingRequirementForm form={form} courseOptions={coursesQuery.data?.items ?? []} roleOptions={roleOptionsQuery.data ?? []} />
      </Modal>

      <Modal
        title="Edit role training requirement"
        open={Boolean(editing)}
        onOk={() => void handleUpdate()}
        onCancel={() => { setEditing(null); form.resetFields(); }}
        confirmLoading={updateMutation.isPending}
        destroyOnHidden
        afterOpenChange={(open) => {
          if (open && editing) {
            form.setFieldsValue({
              courseId: editing.courseId,
              projectRoleId: editing.projectRoleId,
              requiredWithinDays: editing.requiredWithinDays,
              renewalIntervalMonths: editing.renewalIntervalMonths,
              notes: editing.notes ?? undefined,
              status: editing.status,
            });
          }
        }}
      >
        <RoleTrainingRequirementForm form={form} courseOptions={coursesQuery.data?.items ?? []} roleOptions={roleOptionsQuery.data ?? []} />
      </Modal>
    </Space>
  );
}

function RoleTrainingRequirementForm({
  form,
  courseOptions,
  roleOptions,
}: {
  form: ReturnType<typeof Form.useForm<CreateRoleTrainingRequirementInput & { status?: string }>>[0];
  courseOptions: Array<{ id: string; courseCode: string | null; title: string }>;
  roleOptions: Array<{ id: string; projectName: string | null; name: string }>;
}) {
  return (
    <Form form={form} layout="vertical" initialValues={{ requiredWithinDays: 30, renewalIntervalMonths: 12, status: "active" }}>
      <Form.Item label="Course" name="courseId" rules={[{ required: true, message: "Course is required." }]}>
        <Select showSearch optionFilterProp="label" options={courseOptions.map((course) => ({ value: course.id, label: `${course.courseCode ?? "COURSE"} · ${course.title}` }))} />
      </Form.Item>
      <Form.Item label="Project Role" name="projectRoleId" rules={[{ required: true, message: "Role is required." }]}>
        <Select showSearch optionFilterProp="label" options={roleOptions.map((role) => ({ value: role.id, label: role.projectName ? `${role.projectName} · ${role.name}` : role.name }))} />
      </Form.Item>
      <Form.Item label="Required Within (days)" name="requiredWithinDays" rules={[{ required: true, message: "Required window is required." }]}>
        <InputNumber min={0} style={{ width: "100%" }} />
      </Form.Item>
      <Form.Item label="Renewal Interval (months)" name="renewalIntervalMonths" rules={[{ required: true, message: "Renewal interval is required." }]}>
        <InputNumber min={0} style={{ width: "100%" }} />
      </Form.Item>
      <Form.Item label="Status" name="status">
        <Select options={["active", "archived"].map((value) => ({ value, label: value }))} />
      </Form.Item>
      <Form.Item label="Notes" name="notes">
        <Input.TextArea rows={3} />
      </Form.Item>
    </Form>
  );
}
