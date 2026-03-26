import { useParams } from "react-router-dom";
import { Alert, Button, Card, Checkbox, Descriptions, Flex, Form, Input, List, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useMeeting, useMeetingActions, useUpdateMeeting } from "../hooks/useMeetings";
import type { DecisionListItem, MeetingAttendeeItem, MeetingMinutesInput, MeetingUpdateInput } from "../types/meetings";

const { Title, Paragraph, Text } = Typography;

export function MeetingDetailPage() {
  const { meetingId } = useParams<{ meetingId: string }>();
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.meetings.read);
  const canManage = permissionState.hasPermission(permissions.meetings.manage);
  const canApprove = permissionState.hasPermission(permissions.meetings.approve);
  const canReadRestricted = permissionState.hasPermission(permissions.meetings.readRestricted);
  const [messageApi, contextHolder] = message.useMessage();
  const [meetingForm] = Form.useForm<MeetingUpdateInput>();
  const [minutesForm] = Form.useForm<MeetingMinutesInput>();
  const meetingQuery = useMeeting(meetingId ?? null, canRead);
  const updateMeetingMutation = useUpdateMeeting();
  const actions = useMeetingActions();

  if (!canRead) {
    return <Alert type="warning" showIcon message="Meeting access is not available for this account." />;
  }

  if (!meetingQuery.data) {
    return <Alert type="info" showIcon message={meetingQuery.isLoading ? "Loading meeting..." : "Meeting not found or restricted."} />;
  }

  const meeting = meetingQuery.data;
  const attendeeColumns: ColumnsType<MeetingAttendeeItem> = [
    { title: "User", dataIndex: "userId", key: "userId" },
    { title: "Attendance", dataIndex: "attendanceStatus", key: "attendanceStatus" },
  ];
  const decisionColumns: ColumnsType<DecisionListItem> = [
    { title: "Code", dataIndex: "code", key: "code" },
    { title: "Title", dataIndex: "title", key: "title" },
    { title: "Type", dataIndex: "decisionType", key: "decisionType" },
    { title: "Status", dataIndex: "status", key: "status", render: (value) => <Tag>{value}</Tag> },
  ];

  const handleSaveMeeting = async () => {
    const values = await meetingForm.validateFields();
    try {
      await updateMeetingMutation.mutateAsync({ id: meeting.id, input: values });
      void messageApi.success("Meeting updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update meeting");
      void messageApi.error(presentation.description);
    }
  };

  const handleSaveMinutes = async () => {
    const values = await minutesForm.validateFields();
    try {
      await actions.updateMinutes.mutateAsync({ id: meeting.id, input: values });
      void messageApi.success("Meeting minutes updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update meeting minutes");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Flex justify="space-between" gap={16} wrap="wrap">
          <div>
            <Title level={3} style={{ margin: 0 }}>{meeting.title}</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {meeting.meetingType} · <Tag>{meeting.status}</Tag>
            </Paragraph>
          </div>
          <Flex gap={8}>
            <Button disabled={!canApprove || meeting.status !== "draft"} onClick={() => void actions.approve.mutateAsync({ id: meeting.id, input: { reason: "Approved from detail" } })}>Approve meeting</Button>
          </Flex>
        </Flex>
      </Card>

      <Card variant="borderless">
        <Descriptions bordered size="small" column={2}>
          <Descriptions.Item label="Project">{meeting.projectName}</Descriptions.Item>
          <Descriptions.Item label="Facilitator">{meeting.facilitatorUserId}</Descriptions.Item>
          <Descriptions.Item label="Meeting At">{meeting.meetingAt}</Descriptions.Item>
          <Descriptions.Item label="Restricted">{meeting.isRestricted ? "yes" : "no"}</Descriptions.Item>
        </Descriptions>
      </Card>

      <Card variant="borderless" title="Meeting detail">
        <Form form={meetingForm} layout="vertical" initialValues={{ meetingType: meeting.meetingType, title: meeting.title, meetingAt: meeting.meetingAt, facilitatorUserId: meeting.facilitatorUserId, attendeeUserIds: meeting.attendees.map((item) => item.userId), agenda: meeting.agenda ?? undefined, discussionSummary: meeting.discussionSummary ?? undefined, isRestricted: meeting.isRestricted, classification: meeting.classification ?? undefined }}>
          <Form.Item label="Meeting Type" name="meetingType" rules={[{ required: true, message: "Meeting type is required." }]}><Select disabled={!canManage} options={["kickoff", "review", "management_review", "change_board"].map((value) => ({ label: value, value }))} /></Form.Item>
          <Form.Item label="Title" name="title" rules={[{ required: true, message: "Title is required." }]}><Input disabled={!canManage} /></Form.Item>
          <Form.Item label="Meeting At" name="meetingAt" rules={[{ required: true, message: "Meeting time is required." }]}><Input disabled={!canManage} /></Form.Item>
          <Form.Item label="Facilitator" name="facilitatorUserId" rules={[{ required: true, message: "Facilitator is required." }]}><Input disabled={!canManage} /></Form.Item>
          <Form.Item label="Attendees" name="attendeeUserIds"><Select mode="tags" disabled={!canManage} tokenSeparators={[","]} /></Form.Item>
          <Form.Item label="Agenda" name="agenda"><Input.TextArea rows={3} disabled={!canManage} /></Form.Item>
          <Form.Item label="Discussion Summary" name="discussionSummary"><Input.TextArea rows={3} disabled={!canManage} /></Form.Item>
          {canReadRestricted ? (
            <>
              <Form.Item name="isRestricted" valuePropName="checked"><Checkbox disabled={!canManage}>Restricted meeting</Checkbox></Form.Item>
              <Form.Item shouldUpdate noStyle>
                {() => meetingForm.getFieldValue("isRestricted") ? <Form.Item label="Classification" name="classification" rules={[{ required: true, message: "Classification is required." }]}><Input disabled={!canManage} /></Form.Item> : null}
              </Form.Item>
            </>
          ) : null}
          <Button type="primary" disabled={!canManage} loading={updateMeetingMutation.isPending} onClick={() => void handleSaveMeeting()}>Save meeting</Button>
        </Form>
      </Card>

      <Card variant="borderless" title="Minutes">
        <Form form={minutesForm} layout="vertical" initialValues={{ summary: meeting.minutes.summary ?? undefined, decisionsSummary: meeting.minutes.decisionsSummary ?? undefined, actionsSummary: meeting.minutes.actionsSummary ?? undefined, status: meeting.minutes.status, attendeeUserIds: meeting.attendees.map((item) => item.userId) }}>
          <Form.Item label="Summary" name="summary"><Input.TextArea rows={3} disabled={!canManage} /></Form.Item>
          <Form.Item label="Decisions Summary" name="decisionsSummary"><Input.TextArea rows={3} disabled={!canManage} /></Form.Item>
          <Form.Item label="Actions Summary" name="actionsSummary"><Input.TextArea rows={3} disabled={!canManage} /></Form.Item>
          <Form.Item label="Minutes Status" name="status"><Select disabled={!canManage} options={["draft", "reviewed", "approved", "archived"].map((value) => ({ label: value, value }))} /></Form.Item>
          <Button type="primary" disabled={!canManage} loading={actions.updateMinutes.isPending} onClick={() => void handleSaveMinutes()}>Save minutes</Button>
        </Form>
      </Card>

      <Card variant="borderless" title="Attendees">
        <Table rowKey="id" columns={attendeeColumns} dataSource={meeting.attendees} pagination={false} />
      </Card>

      <Card variant="borderless" title="Linked decisions">
        <Table rowKey="id" columns={decisionColumns} dataSource={meeting.decisions} pagination={false} />
      </Card>

      <Card variant="borderless" title="History">
        <List dataSource={meeting.history} renderItem={(item) => <List.Item><Space direction="vertical" size={0}><Text strong>{item.eventType}</Text><Text type="secondary">{item.actorUserId ?? "system"} · {item.occurredAt}</Text>{item.reason ? <Text>{item.reason}</Text> : null}</Space></List.Item>} />
      </Card>
    </Space>
  );
}
