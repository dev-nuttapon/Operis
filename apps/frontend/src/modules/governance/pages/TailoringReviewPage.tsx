import { useState } from "react";
import dayjs from "dayjs";
import { Alert, Button, Card, DatePicker, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useProjectList } from "../../users/public";
import { useCreateTailoringReviewCycle, useTailoringReviewCycleActions, useTailoringReviewCycles, useUpdateTailoringReviewCycle } from "../hooks/useGovernance";
import type { TailoringReviewCycle, TailoringReviewCycleFormInput, TailoringReviewCycleUpdateInput } from "../types/governance";

const { Title, Paragraph } = Typography;

export function TailoringReviewPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.governance.tailoringRead, permissions.governance.tailoringManage, permissions.governance.tailoringApprove);
  const canManage = permissionState.hasPermission(permissions.governance.tailoringManage);
  const canApprove = permissionState.hasPermission(permissions.governance.tailoringApprove);
  const [messageApi, contextHolder] = message.useMessage();
  const [selected, setSelected] = useState<TailoringReviewCycle | null>(null);
  const [open, setOpen] = useState(false);
  const [decisionReason, setDecisionReason] = useState("");
  const [form] = Form.useForm<(TailoringReviewCycleFormInput & TailoringReviewCycleUpdateInput) & { reviewDueAtValue?: dayjs.Dayjs }>();
  const reviewsQuery = useTailoringReviewCycles({ page: 1, pageSize: 100 }, canRead);
  const projectsQuery = useProjectList({ page: 1, pageSize: 100 });
  const createMutation = useCreateTailoringReviewCycle();
  const updateMutation = useUpdateTailoringReviewCycle();
  const actions = useTailoringReviewCycleActions();

  if (!canRead) {
    return <Alert type="warning" showIcon message="Tailoring reviews are not available for this account." />;
  }

  const submit = async () => {
    const values = await form.validateFields();
    try {
      if (selected) {
        await updateMutation.mutateAsync({
          id: selected.id,
          input: {
            title: values.title,
            ownerUserId: values.ownerUserId,
            reviewDueAt: values.reviewDueAtValue?.toISOString() ?? values.reviewDueAt ?? null,
            decisionReason: values.decisionReason ?? null,
          },
        });
        void messageApi.success("Tailoring review updated.");
      } else {
        await createMutation.mutateAsync({
          projectId: values.projectId,
          reviewCode: values.reviewCode,
          title: values.title,
          ownerUserId: values.ownerUserId,
          reviewDueAt: values.reviewDueAtValue?.toISOString() ?? values.reviewDueAt ?? null,
          decisionReason: values.decisionReason ?? null,
        });
        void messageApi.success("Tailoring review created.");
      }
      setOpen(false);
      setSelected(null);
      form.resetFields();
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to save tailoring review");
      void messageApi.error(presentation.description);
    }
  };

  const transition = async (review: TailoringReviewCycle, targetStatus: string) => {
    try {
      await actions.transition.mutateAsync({ id: review.id, input: { targetStatus, reason: decisionReason || `Move review to ${targetStatus}.` } });
      void messageApi.success(`Review moved to ${targetStatus}.`);
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to transition tailoring review");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={16} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Title level={3} style={{ margin: 0 }}>Tailoring Reviews</Title>
        <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
          Track project tailoring review cycles, approval decisions, overdue reviews, and open deviations.
        </Paragraph>
      </Card>
      <Card variant="borderless" extra={canManage ? <Button type="primary" onClick={() => { setSelected(null); form.resetFields(); setOpen(true); }}>New review</Button> : null}>
        <Table
          rowKey="id"
          loading={reviewsQuery.isLoading}
          dataSource={reviewsQuery.data?.items ?? []}
          pagination={false}
          columns={[
            { title: "Review", render: (_, item: TailoringReviewCycle) => <Space direction="vertical" size={0}><strong>{item.reviewCode}</strong><span>{item.title}</span><span>{item.projectName}</span></Space> },
            { title: "Owner", dataIndex: "ownerUserId" },
            { title: "Due", dataIndex: "reviewDueAt", render: (value: string) => dayjs(value).format("DD MMM YYYY") },
            { title: "Deviations", render: (_, item: TailoringReviewCycle) => `${item.openDeviationCount}/${item.deviationCount}` },
            { title: "Status", render: (_, item: TailoringReviewCycle) => <Space><Tag>{item.status}</Tag>{item.isExpired ? <Tag color="red">expired</Tag> : null}</Space> },
            {
              title: "Actions",
              render: (_, item: TailoringReviewCycle) => (
                <Space wrap>
                  {canManage && item.status === "draft" ? <Button size="small" onClick={() => {
                    setSelected(item);
                    form.setFieldsValue({
                      projectId: item.projectId,
                      reviewCode: item.reviewCode,
                      title: item.title,
                      ownerUserId: item.ownerUserId,
                      reviewDueAtValue: dayjs(item.reviewDueAt),
                      decisionReason: item.decisionReason ?? undefined,
                    });
                    setOpen(true);
                  }}>Edit</Button> : null}
                  {canManage && item.status === "draft" ? <Button size="small" onClick={() => void transition(item, "submitted")}>Submit</Button> : null}
                  {canApprove && item.status === "submitted" ? <Button size="small" onClick={() => void transition(item, "approved")}>Approve</Button> : null}
                  {canApprove && item.status === "submitted" ? <Button size="small" danger onClick={() => void transition(item, "rejected")}>Reject</Button> : null}
                  {(canApprove || canManage) && (item.status === "draft" || item.status === "submitted") ? <Button size="small" ghost onClick={() => void transition(item, "expired")}>Expire</Button> : null}
                </Space>
              ),
            },
          ]}
        />
        {(canApprove || canManage) ? <Input style={{ marginTop: 12 }} placeholder="Decision reason" value={decisionReason} onChange={(event) => setDecisionReason(event.target.value)} /> : null}
      </Card>
      <Modal title={selected ? "Edit Tailoring Review" : "New Tailoring Review"} open={open} onCancel={() => setOpen(false)} onOk={() => void submit()} confirmLoading={createMutation.isPending || updateMutation.isPending} destroyOnHidden>
        <Form form={form} layout="vertical">
          {!selected ? <Form.Item name="projectId" label="Project" rules={[{ required: true }]}><Select options={(projectsQuery.data?.items ?? []).map((project) => ({ value: project.id, label: `${project.code} · ${project.name}` }))} showSearch optionFilterProp="label" /></Form.Item> : null}
          {!selected ? <Form.Item name="reviewCode" label="Review Code" rules={[{ required: true }]}><Input /></Form.Item> : null}
          <Form.Item name="title" label="Title" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="ownerUserId" label="Owner User Id" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="reviewDueAtValue" label="Review Due Date" rules={[{ required: true }]}><DatePicker style={{ width: "100%" }} /></Form.Item>
          <Form.Item name="decisionReason" label="Decision Reason"><Input.TextArea rows={3} /></Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
