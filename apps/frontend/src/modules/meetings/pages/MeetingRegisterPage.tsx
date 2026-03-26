import { useMemo, useState } from "react";
import { Alert, Button, Card, Checkbox, DatePicker, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { CalendarOutlined, PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { useNavigate } from "react-router-dom";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useProjectOptions } from "../../users";
import { useCreateMeeting, useMeetingActions, useMeetings } from "../hooks/useMeetings";
import type { MeetingFormInput, MeetingListItem } from "../types/meetings";

const { Title, Paragraph, Text } = Typography;

export function MeetingRegisterPage() {
  const navigate = useNavigate();
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.meetings.read);
  const canManage = permissionState.hasPermission(permissions.meetings.manage);
  const canApprove = permissionState.hasPermission(permissions.meetings.approve);
  const canReadRestricted = permissionState.hasPermission(permissions.meetings.readRestricted);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", projectId: undefined as string | undefined, meetingType: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 10 });
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [form] = Form.useForm<MeetingFormInput>();

  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 20 });
  const meetingsQuery = useMeetings(filters, canRead);
  const createMutation = useCreateMeeting();
  const actions = useMeetingActions();

  const columns = useMemo<ColumnsType<MeetingListItem>>(
    () => [
      {
        title: "Meeting",
        key: "meeting",
        render: (_, item) => (
          <Space direction="vertical" size={0}>
            <Text strong>{item.title}</Text>
            <Text type="secondary">{item.meetingType}</Text>
          </Space>
        ),
      },
      { title: "Project", dataIndex: "projectName", key: "projectName" },
      { title: "Facilitator", dataIndex: "facilitatorUserId", key: "facilitatorUserId" },
      { title: "When", dataIndex: "meetingAt", key: "meetingAt", render: (value) => dayjs(value).format("YYYY-MM-DD HH:mm") },
      { title: "Status", dataIndex: "status", key: "status", render: (value) => <Tag>{value}</Tag> },
      { title: "Restriction", dataIndex: "isRestricted", key: "isRestricted", render: (value) => value ? <Tag color="red">restricted</Tag> : <Text type="secondary">normal</Text> },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => (
          <Flex gap={8} wrap>
            <Button size="small" onClick={() => navigate(`/app/meetings/${item.id}`)}>View</Button>
            <Button
              size="small"
              disabled={!canApprove || item.status !== "draft"}
              onClick={() =>
                void actions.approve.mutateAsync({ id: item.id, input: { reason: "Approved from register" } }).then(() => {
                  void messageApi.success("Meeting approved.");
                }).catch((error) => {
                  const presentation = getApiErrorPresentation(error, "Unable to approve meeting");
                  void messageApi.error(presentation.description);
                })
              }
            >
              Approve
            </Button>
          </Flex>
        ),
      },
    ],
    [actions.approve, canApprove, messageApi, navigate],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Meeting access is not available for this account." />;
  }

  const handleCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync({
        ...values,
        meetingAt: dayjs.isDayjs(values.meetingAt) ? values.meetingAt.toISOString() : values.meetingAt,
        classification: values.isRestricted ? values.classification ?? undefined : undefined,
      });
      form.resetFields();
      setIsCreateOpen(false);
      void messageApi.success("Meeting created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create meeting");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0f766e, #0f172a)", color: "#fff" }}>
            <CalendarOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>MOM Register</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Track meeting records, minutes approval readiness, and linked decision outputs.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search title or type" style={{ width: 260 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
            <Select allowClear showSearch placeholder="Project" style={{ width: 220 }} options={projectOptions.options} value={filters.projectId} onSearch={projectOptions.onSearch} onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))} />
            <Select allowClear placeholder="Type" style={{ width: 180 }} options={["kickoff", "review", "management_review", "change_board"].map((value) => ({ label: value, value }))} value={filters.meetingType} onChange={(value) => setFilters((current) => ({ ...current, meetingType: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 160 }} options={["draft", "approved", "archived"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Flex gap={8}>
            <Button onClick={() => navigate("/app/decisions")}>Decision Log</Button>
            <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setIsCreateOpen(true)}>New meeting</Button>
          </Flex>
        </Flex>

        <Table rowKey="id" loading={meetingsQuery.isLoading} columns={columns} dataSource={meetingsQuery.data?.items ?? []} pagination={{ current: meetingsQuery.data?.page ?? filters.page, pageSize: meetingsQuery.data?.pageSize ?? filters.pageSize, total: meetingsQuery.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create meeting" open={isCreateOpen} onOk={() => void handleCreate()} onCancel={() => setIsCreateOpen(false)} okText="Create" confirmLoading={createMutation.isPending} destroyOnHidden>
        <Form form={form} layout="vertical" initialValues={{ meetingType: "review", isRestricted: false }}>
          <Form.Item label="Project" name="projectId" rules={[{ required: true, message: "Project is required." }]}><Select showSearch options={projectOptions.options} onSearch={projectOptions.onSearch} /></Form.Item>
          <Form.Item label="Meeting Type" name="meetingType" rules={[{ required: true, message: "Meeting type is required." }]}><Select options={["kickoff", "review", "management_review", "change_board"].map((value) => ({ label: value, value }))} /></Form.Item>
          <Form.Item label="Title" name="title" rules={[{ required: true, message: "Title is required." }]}><Input /></Form.Item>
          <Form.Item label="Meeting At" name="meetingAt" rules={[{ required: true, message: "Meeting time is required." }]}><DatePicker showTime style={{ width: "100%" }} /></Form.Item>
          <Form.Item label="Facilitator" name="facilitatorUserId" rules={[{ required: true, message: "Facilitator is required." }]}><Input placeholder="facilitator@example.com" /></Form.Item>
          <Form.Item label="Attendees" name="attendeeUserIds"><Select mode="tags" tokenSeparators={[","]} placeholder="user ids" /></Form.Item>
          <Form.Item label="Agenda" name="agenda"><Input.TextArea rows={3} /></Form.Item>
          <Form.Item label="Discussion Summary" name="discussionSummary"><Input.TextArea rows={3} /></Form.Item>
          {canReadRestricted ? (
            <>
              <Form.Item name="isRestricted" valuePropName="checked"><Checkbox>Restricted meeting</Checkbox></Form.Item>
              <Form.Item shouldUpdate noStyle>
                {() => form.getFieldValue("isRestricted") ? <Form.Item label="Classification" name="classification" rules={[{ required: true, message: "Classification is required." }]}><Input placeholder="confidential" /></Form.Item> : null}
              </Form.Item>
            </>
          ) : null}
        </Form>
      </Modal>
    </Space>
  );
}
