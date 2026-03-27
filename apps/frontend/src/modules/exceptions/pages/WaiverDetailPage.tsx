import { useMemo, useState } from "react";
import { Alert, Button, Card, Descriptions, Form, Input, Modal, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useParams } from "react-router-dom";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useTransitionWaiver, useWaiver } from "../hooks/useExceptions";
import type { CompensatingControlItem, WaiverReviewItem } from "../types/exceptions";

const { Title, Paragraph } = Typography;

export function WaiverDetailPage() {
  const { waiverId } = useParams<{ waiverId: string }>();
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.exceptions.read, permissions.exceptions.manage, permissions.exceptions.approve);
  const canManage = permissionState.hasPermission(permissions.exceptions.manage);
  const canApprove = permissionState.hasPermission(permissions.exceptions.approve);
  const [messageApi, contextHolder] = message.useMessage();
  const [transitionOpen, setTransitionOpen] = useState(false);
  const [form] = Form.useForm();
  const query = useWaiver(waiverId, canRead);
  const transitionMutation = useTransitionWaiver();

  const controlColumns = useMemo<ColumnsType<CompensatingControlItem>>(
    () => [
      { title: "Control Code", dataIndex: "controlCode" },
      { title: "Description", dataIndex: "description" },
      { title: "Owner", dataIndex: "ownerUserId" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
    ],
    [],
  );

  const reviewColumns = useMemo<ColumnsType<WaiverReviewItem>>(
    () => [
      { title: "Type", dataIndex: "reviewType" },
      { title: "Outcome", dataIndex: "outcomeStatus", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Reviewer", dataIndex: "reviewerUserId" },
      { title: "Reviewed At", dataIndex: "reviewedAt" },
      { title: "Next Review", dataIndex: "nextReviewAt", render: (value?: string | null) => value ?? "-" },
    ],
    [],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Waiver detail is not available for this account." />;
  }

  if (query.isLoading || !query.data) {
    return <Card loading variant="borderless" />;
  }

  const item = query.data;
  const allowedTargets = [
    ...(item.status === "draft" && canManage ? ["submitted"] : []),
    ...(item.status === "submitted" && canApprove ? ["approved", "rejected"] : []),
    ...(item.status === "approved" && canManage ? ["expired", "closed"] : []),
    ...(item.status === "rejected" && canManage ? ["closed"] : []),
    ...(item.status === "expired" && canManage ? ["closed"] : []),
  ];

  const submitTransition = async () => {
    const values = await form.validateFields();
    try {
      await transitionMutation.mutateAsync({
        id: item.id,
        input: {
          targetStatus: values.targetStatus,
          reason: values.reason ?? null,
        },
      });
      setTransitionOpen(false);
      form.resetFields();
      void messageApi.success("Waiver transitioned.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to transition waiver");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" style={{ width: "100%", justifyContent: "space-between" }}>
          <div>
            <Title level={3} style={{ margin: 0 }}>{item.waiverCode}</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Review scope, compensating controls, and workflow history for this temporary process waiver.
            </Paragraph>
          </div>
          <Space>
            <Tag color={item.isExpired ? "red" : "blue"}>{item.status}</Tag>
            <Button disabled={allowedTargets.length === 0} onClick={() => setTransitionOpen(true)}>Transition</Button>
          </Space>
        </Space>
      </Card>

      <Card variant="borderless" title="Waiver Detail">
        <Descriptions bordered size="small" column={2}>
          <Descriptions.Item label="Project">{item.projectName ?? "-"}</Descriptions.Item>
          <Descriptions.Item label="Process Area">{item.processArea}</Descriptions.Item>
          <Descriptions.Item label="Requester">{item.requestedByUserId}</Descriptions.Item>
          <Descriptions.Item label="Effective Window">{item.effectiveFrom} to {item.expiresAt}</Descriptions.Item>
          <Descriptions.Item label="Scope" span={2}>{item.scopeSummary}</Descriptions.Item>
          <Descriptions.Item label="Justification" span={2}>{item.justification}</Descriptions.Item>
          <Descriptions.Item label="Decision Reason" span={2}>{item.decisionReason ?? "-"}</Descriptions.Item>
          <Descriptions.Item label="Closure Reason" span={2}>{item.closureReason ?? "-"}</Descriptions.Item>
        </Descriptions>
      </Card>

      <Card variant="borderless" title="Compensating Controls">
        <Table rowKey="id" pagination={false} columns={controlColumns} dataSource={item.compensatingControls} />
      </Card>

      <Card variant="borderless" title="Review History">
        <Table rowKey="id" pagination={false} columns={reviewColumns} dataSource={item.reviews} />
      </Card>

      <Modal title="Transition waiver" open={transitionOpen} onOk={() => void submitTransition()} onCancel={() => { setTransitionOpen(false); form.resetFields(); }} confirmLoading={transitionMutation.isPending} destroyOnHidden>
        <Form form={form} layout="vertical">
          <Form.Item label="Target Status" name="targetStatus" rules={[{ required: true, message: "Target status is required." }]}>
            <Input list="waiver-status-options" />
          </Form.Item>
          <datalist id="waiver-status-options">
            {allowedTargets.map((value) => <option key={value} value={value} />)}
          </datalist>
          <Form.Item label="Reason" name="reason">
            <Input.TextArea rows={4} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
