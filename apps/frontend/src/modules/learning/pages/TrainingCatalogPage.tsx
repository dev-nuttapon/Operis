import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Form, Input, InputNumber, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { BookOutlined, PlusOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateTrainingCourse, useTrainingCourses, useTransitionTrainingCourse, useUpdateTrainingCourse } from "../hooks/useLearning";
import type { CreateTrainingCourseInput, TrainingCourseItem } from "../types/learning";

const { Title, Paragraph, Text } = Typography;

const courseStatusOptions = ["draft", "active", "retired"];

export function TrainingCatalogPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.learning.read, permissions.learning.manage, permissions.learning.approve);
  const canManage = permissionState.hasPermission(permissions.learning.manage);
  const canApprove = permissionState.hasPermission(permissions.learning.approve);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", status: undefined as string | undefined, page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<TrainingCourseItem | null>(null);
  const [form] = Form.useForm<CreateTrainingCourseInput>();
  const query = useTrainingCourses(filters, canRead);
  const createMutation = useCreateTrainingCourse();
  const updateMutation = useUpdateTrainingCourse();
  const transitionMutation = useTransitionTrainingCourse();

  const columns = useMemo<ColumnsType<TrainingCourseItem>>(
    () => [
      {
        title: "Course",
        key: "course",
        render: (_, item) => (
          <Space direction="vertical" size={0}>
            <Text strong>{item.courseCode ?? "COURSE"}</Text>
            <Text>{item.title}</Text>
            <Text type="secondary">{item.provider ?? "Internal"}</Text>
          </Space>
        ),
      },
      { title: "Mode", dataIndex: "deliveryMode", render: (value: string | null) => value ?? "-" },
      { title: "Audience", dataIndex: "audienceScope", render: (value: string | null) => value ?? "-" },
      { title: "Validity", dataIndex: "validityMonths", render: (value: number) => `${value} months` },
      { title: "Requirements", dataIndex: "requirementCount" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "active" ? "green" : value === "retired" ? "default" : "gold"}>{value}</Tag> },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => (
          <Flex gap={8} wrap>
            <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button>
            {item.status === "draft" ? (
              <Button
                size="small"
                type="primary"
                ghost
                disabled={!canApprove}
                loading={transitionMutation.isPending}
                onClick={() => void handleTransition(item.id, "active")}
              >
                Activate
              </Button>
            ) : null}
            {item.status === "active" ? (
              <Button
                size="small"
                danger
                ghost
                disabled={!canApprove}
                loading={transitionMutation.isPending}
                onClick={() => void handleTransition(item.id, "retired")}
              >
                Retire
              </Button>
            ) : null}
          </Flex>
        ),
      },
    ],
    [canApprove, canManage, transitionMutation.isPending],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Training catalog is not available for this account." />;
  }

  const handleTransition = async (courseId: string, targetStatus: string) => {
    try {
      await transitionMutation.mutateAsync({ id: courseId, input: { targetStatus } });
      void messageApi.success(`Course ${targetStatus}.`);
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to transition training course");
      void messageApi.error(presentation.description);
    }
  };

  const handleCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Training course created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create training course");
      void messageApi.error(presentation.description);
    }
  };

  const handleUpdate = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({ id: editing.id, input: values });
      setEditing(null);
      form.resetFields();
      void messageApi.success("Training course updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update training course");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #1d4ed8, #14532d)", color: "#fff" }}>
            <BookOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Training Catalog</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Govern required training courses, audience scope, validity windows, and approval-controlled activation.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search title, code, or provider" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={courseStatusOptions.map((value) => ({ value, label: value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New course</Button>
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

      <Modal title="Create training course" open={createOpen} onOk={() => void handleCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <TrainingCourseForm form={form} />
      </Modal>

      <Modal
        title="Edit training course"
        open={Boolean(editing)}
        onOk={() => void handleUpdate()}
        onCancel={() => { setEditing(null); form.resetFields(); }}
        confirmLoading={updateMutation.isPending}
        destroyOnHidden
        afterOpenChange={(open) => {
          if (open && editing) {
            form.setFieldsValue({
              courseCode: editing.courseCode ?? undefined,
              title: editing.title,
              description: editing.description ?? undefined,
              provider: editing.provider ?? undefined,
              deliveryMode: editing.deliveryMode ?? undefined,
              audienceScope: editing.audienceScope ?? undefined,
              validityMonths: editing.validityMonths,
            });
          }
        }}
      >
        <TrainingCourseForm form={form} />
      </Modal>
    </Space>
  );
}

function TrainingCourseForm({ form }: { form: ReturnType<typeof Form.useForm<CreateTrainingCourseInput>>[0] }) {
  return (
    <Form form={form} layout="vertical" initialValues={{ validityMonths: 12 }}>
      <Form.Item label="Course Code" name="courseCode">
        <Input placeholder="CMMI_FOUNDATION" />
      </Form.Item>
      <Form.Item label="Title" name="title" rules={[{ required: true, message: "Title is required." }]}>
        <Input />
      </Form.Item>
      <Form.Item label="Provider" name="provider">
        <Input placeholder="Internal Academy" />
      </Form.Item>
      <Form.Item label="Delivery Mode" name="deliveryMode">
        <Select allowClear options={["self_paced", "instructor_led", "workshop"].map((value) => ({ value, label: value }))} />
      </Form.Item>
      <Form.Item label="Audience Scope" name="audienceScope">
        <Input placeholder="PM, QA, ComplianceAdmin" />
      </Form.Item>
      <Form.Item label="Validity (months)" name="validityMonths" rules={[{ required: true, message: "Validity is required." }]}>
        <InputNumber min={0} style={{ width: "100%" }} />
      </Form.Item>
      <Form.Item label="Description" name="description">
        <Input.TextArea rows={4} />
      </Form.Item>
    </Form>
  );
}
