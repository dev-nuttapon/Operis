import { useParams } from "react-router-dom";
import { Alert, Button, Card, Checkbox, Descriptions, Flex, Form, Input, List, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useIssue, useIssueActions, useUpdateIssue } from "../hooks/useRisks";
import type { IssueAction, IssueActionInput, IssueUpdateInput } from "../types/risks";

const { Title, Paragraph, Text } = Typography;

export function IssueDetailPage() {
  const { issueId } = useParams<{ issueId: string }>();
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.risks.read);
  const canManage = permissionState.hasPermission(permissions.risks.manage);
  const canReadSensitive = permissionState.hasPermission(permissions.risks.readSensitive);
  const [messageApi, contextHolder] = message.useMessage();
  const [issueForm] = Form.useForm<IssueUpdateInput>();
  const [actionForm] = Form.useForm<IssueActionInput>();
  const issueQuery = useIssue(issueId ?? null, canRead);
  const updateIssueMutation = useUpdateIssue();
  const actions = useIssueActions();

  if (!canRead) {
    return <Alert type="warning" showIcon message="Issue access is not available for this account." />;
  }

  if (!issueQuery.data) {
    return <Alert type="info" showIcon message={issueQuery.isLoading ? "Loading issue..." : "Issue not found or restricted."} />;
  }

  const issue = issueQuery.data;

  const actionColumns: ColumnsType<IssueAction> = [
    {
      title: "Action",
      dataIndex: "actionDescription",
      key: "actionDescription",
    },
    {
      title: "Assigned",
      dataIndex: "assignedTo",
      key: "assignedTo",
    },
    {
      title: "Due",
      dataIndex: "dueDate",
      key: "dueDate",
      render: (value) => value ?? <Text type="secondary">not set</Text>,
    },
    {
      title: "Status",
      dataIndex: "status",
      key: "status",
      render: (value) => <Tag>{value}</Tag>,
    },
    {
      title: "Update",
      key: "update",
      render: (_, item) => (
        <Select
          disabled={!canManage}
          value={item.status}
          style={{ width: 140 }}
          options={["open", "in_progress", "completed", "verified"].map((value) => ({ label: value, value }))}
          onChange={(value) =>
            void actions.updateAction.mutateAsync({
              issueId: issue.id,
              actionId: item.id,
              input: {
                actionDescription: item.actionDescription,
                assignedTo: item.assignedTo,
                dueDate: item.dueDate ?? undefined,
                status: value,
                verificationNote: item.verificationNote ?? undefined,
              },
            }).then(() => {
              void messageApi.success("Action updated.");
            }).catch((error) => {
              const presentation = getApiErrorPresentation(error, "Unable to update action");
              void messageApi.error(presentation.description);
            })
          }
        />
      ),
    },
  ];

  const handleSaveIssue = async () => {
    const values = await issueForm.validateFields();
    try {
      await updateIssueMutation.mutateAsync({ id: issue.id, input: values });
      void messageApi.success("Issue updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update issue");
      void messageApi.error(presentation.description);
    }
  };

  const handleAddAction = async () => {
    const values = await actionForm.validateFields();
    try {
      await actions.createAction.mutateAsync({ issueId: issue.id, input: values });
      actionForm.resetFields();
      void messageApi.success("Action added.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to add action");
      void messageApi.error(presentation.description);
    }
  };

  const handleLifecycleAction = async (action: "resolve" | "close") => {
    try {
      const input = { resolutionSummary: issueForm.getFieldValue("resolutionSummary") };
      if (action === "resolve") {
        await actions.resolve.mutateAsync({ id: issue.id, input });
      } else {
        await actions.close.mutateAsync({ id: issue.id, input });
      }

      void messageApi.success(`Issue ${action}d.`);
    } catch (error) {
      const presentation = getApiErrorPresentation(error, `Unable to ${action} issue`);
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Title level={3} style={{ margin: 0 }}>{issue.code} · {issue.title}</Title>
        <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
          Manage action closure, dependencies, and final resolution evidence.
        </Paragraph>
      </Card>

      <Card variant="borderless">
        <Descriptions column={2} bordered size="small">
          <Descriptions.Item label="Project">{issue.projectName}</Descriptions.Item>
          <Descriptions.Item label="Owner">{issue.ownerUserId}</Descriptions.Item>
          <Descriptions.Item label="Severity"><Tag>{issue.severity}</Tag></Descriptions.Item>
          <Descriptions.Item label="Status"><Tag>{issue.status}</Tag></Descriptions.Item>
          <Descriptions.Item label="Sensitive">{issue.isSensitive ? "restricted" : "no"}</Descriptions.Item>
          <Descriptions.Item label="Due date">{issue.dueDate ?? "not set"}</Descriptions.Item>
        </Descriptions>
      </Card>

      <Card variant="borderless" title="Update issue">
        <Form
          form={issueForm}
          layout="vertical"
          initialValues={{
            title: issue.title,
            description: issue.description ?? "",
            ownerUserId: issue.ownerUserId,
            dueDate: issue.dueDate ?? undefined,
            severity: issue.severity,
            rootIssue: issue.rootIssue ?? undefined,
            dependencies: issue.dependencies ?? undefined,
            resolutionSummary: issue.resolutionSummary ?? undefined,
            isSensitive: issue.isSensitive,
            sensitiveContext: issue.sensitiveContext ?? undefined,
            status: issue.status,
          }}
        >
          <Form.Item label="Title" name="title" rules={[{ required: true, message: "Title is required." }]}>
            <Input disabled={!canManage} />
          </Form.Item>
          <Form.Item label="Description" name="description" rules={[{ required: true, message: "Description is required." }]}>
            <Input.TextArea rows={4} disabled={!canManage} />
          </Form.Item>
          <Form.Item label="Owner" name="ownerUserId" rules={[{ required: true, message: "Owner is required." }]}>
            <Input disabled={!canManage} />
          </Form.Item>
          <Form.Item label="Severity" name="severity" rules={[{ required: true, message: "Severity is required." }]}>
            <Select disabled={!canManage} options={["low", "medium", "high", "critical"].map((value) => ({ label: value, value }))} />
          </Form.Item>
          <Form.Item label="Root issue" name="rootIssue">
            <Input.TextArea rows={2} disabled={!canManage} />
          </Form.Item>
          <Form.Item label="Dependencies" name="dependencies">
            <Input.TextArea rows={2} disabled={!canManage} />
          </Form.Item>
          <Form.Item label="Resolution summary" name="resolutionSummary">
            <Input.TextArea rows={3} disabled={!canManage} />
          </Form.Item>
          {canReadSensitive ? (
            <>
              <Form.Item name="isSensitive" valuePropName="checked">
                <Checkbox disabled={!canManage}>Sensitive issue</Checkbox>
              </Form.Item>
              <Form.Item shouldUpdate noStyle>
                {() => issueForm.getFieldValue("isSensitive") ? (
                  <Form.Item label="Sensitive context" name="sensitiveContext" rules={[{ required: true, message: "Sensitive context is required." }]}>
                    <Input disabled={!canManage} />
                  </Form.Item>
                ) : null}
              </Form.Item>
            </>
          ) : null}
          <Flex gap={8} wrap>
            <Button type="primary" disabled={!canManage} loading={updateIssueMutation.isPending} onClick={() => void handleSaveIssue()}>
              Save changes
            </Button>
            <Button disabled={!canManage} onClick={() => void handleLifecycleAction("resolve")}>
              Resolve issue
            </Button>
            <Button disabled={!canManage || issue.status !== "resolved"} onClick={() => void handleLifecycleAction("close")}>
              Close issue
            </Button>
          </Flex>
        </Form>
      </Card>

      <Card variant="borderless" title="Actions">
        <Table rowKey="id" columns={actionColumns} dataSource={issue.actions} pagination={false} />
        <Form form={actionForm} layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item label="Action description" name="actionDescription" rules={[{ required: true, message: "Action description is required." }]}>
            <Input.TextArea rows={2} disabled={!canManage} />
          </Form.Item>
          <Flex gap={12}>
            <Form.Item label="Assigned to" name="assignedTo" rules={[{ required: true, message: "Assignee is required." }]} style={{ flex: 1 }}>
              <Input disabled={!canManage} />
            </Form.Item>
            <Form.Item label="Due date" name="dueDate" style={{ flex: 1 }}>
              <Input disabled={!canManage} placeholder="YYYY-MM-DD" />
            </Form.Item>
          </Flex>
          <Button type="dashed" disabled={!canManage} onClick={() => void handleAddAction()}>
            Add action
          </Button>
        </Form>
      </Card>

      <Card variant="borderless" title="History">
        <List
          dataSource={issue.history}
          renderItem={(item) => (
            <List.Item>
              <Space direction="vertical" size={0}>
                <Text strong>{item.eventType}</Text>
                <Text type="secondary">{item.actorUserId ?? "system"} · {new Date(item.occurredAt).toLocaleString()}</Text>
                {item.reason ? <Text>{item.reason}</Text> : null}
              </Space>
            </List.Item>
          )}
        />
      </Card>
    </Space>
  );
}
