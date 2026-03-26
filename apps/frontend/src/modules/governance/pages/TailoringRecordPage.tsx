import { useState } from "react";
import { Alert, Button, Card, Form, Input, Modal, Select, Space, Table, Tag, Typography } from "antd";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useAuth } from "../../auth";
import { useProjectList } from "../../users/public";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import { useProcessAssets, useCreateTailoringRecord, useTailoringActions, useTailoringRecords, useUpdateTailoringRecord } from "../hooks/useGovernance";
import type { TailoringRecordFormInput, TailoringRecordListItem } from "../types/governance";

const { Title, Text } = Typography;

export function TailoringRecordPage() {
  const { user } = useAuth();
  const permissionState = usePermissions();
  const canManage = permissionState.hasPermission(permissions.governance.tailoringManage);
  const canApprove = permissionState.hasPermission(permissions.governance.tailoringApprove);
  const [modalOpen, setModalOpen] = useState(false);
  const [decisionReason, setDecisionReason] = useState("");
  const [selected, setSelected] = useState<TailoringRecordListItem | null>(null);
  const [form] = Form.useForm<TailoringRecordFormInput>();
  const recordsQuery = useTailoringRecords({ page: 1, pageSize: 30 });
  const projectsQuery = useProjectList({ page: 1, pageSize: 100 });
  const assetsQuery = useProcessAssets({ page: 1, pageSize: 100 });
  const createMutation = useCreateTailoringRecord();
  const updateMutation = useUpdateTailoringRecord();
  const actions = useTailoringActions();
  const error = recordsQuery.error ?? createMutation.error ?? updateMutation.error;

  const openCreate = () => {
    setSelected(null);
    form.resetFields();
    form.setFieldsValue({ requesterUserId: String(user?.email ?? user?.name ?? "") });
    setModalOpen(true);
  };

  const openEdit = (item: TailoringRecordListItem) => {
    setSelected(item);
    form.setFieldsValue({
      projectId: item.projectId,
      requesterUserId: item.requesterUserId,
      requestedChange: item.requestedChange,
    } as Partial<TailoringRecordFormInput>);
    setModalOpen(true);
  };

  const submit = async () => {
    const values = await form.validateFields();
    if (selected) {
      await updateMutation.mutateAsync({ id: selected.id, input: values });
    } else {
      await createMutation.mutateAsync(values);
    }
    setModalOpen(false);
  };

  return (
    <Space direction="vertical" size={16} style={{ width: "100%" }}>
      <Card>
        <Title level={3} style={{ margin: 0 }}>Tailoring Record</Title>
        <Text type="secondary">Deviation control with evidence-backed approval and application audit trail.</Text>
      </Card>
      {error ? <Alert type="error" showIcon message={getApiErrorPresentation(error).title} description={getApiErrorPresentation(error).description} /> : null}
      <Card extra={canManage ? <Button type="primary" onClick={openCreate}>New Tailoring Record</Button> : null}>
        <Table
          rowKey="id"
          loading={recordsQuery.isLoading}
          dataSource={recordsQuery.data?.items ?? []}
          pagination={false}
          columns={[
            { title: "Project", dataIndex: "projectName", key: "projectName" },
            { title: "Requested Change", dataIndex: "requestedChange", key: "requestedChange" },
            { title: "Requester", dataIndex: "requesterUserId", key: "requesterUserId" },
            { title: "Approver", dataIndex: "approverUserId", key: "approverUserId" },
            { title: "Status", key: "status", render: (_, item) => <Tag>{item.status}</Tag> },
            {
              title: "Actions",
              key: "actions",
              render: (_, item) => (
                <Space wrap>
                  {canManage && item.status === "draft" ? <Button size="small" onClick={() => openEdit(item)}>Edit</Button> : null}
                  {canManage && item.status === "draft" ? <Button size="small" onClick={() => void actions.submit.mutateAsync(item.id)}>Submit</Button> : null}
                  {canApprove && item.status === "submitted" ? <Button size="small" onClick={() => void actions.approve.mutateAsync({ id: item.id, decision: "approved", reason: decisionReason || "Approved tailoring request" })}>Approve</Button> : null}
                  {canApprove && item.status === "submitted" ? <Button size="small" danger onClick={() => void actions.approve.mutateAsync({ id: item.id, decision: "rejected", reason: decisionReason || "Rejected tailoring request" })}>Reject</Button> : null}
                  {canManage && item.status === "approved" ? <Button size="small" onClick={() => void actions.apply.mutateAsync(item.id)}>Apply</Button> : null}
                  {canManage && (item.status === "applied" || item.status === "rejected") ? <Button size="small" onClick={() => void actions.archive.mutateAsync(item.id)}>Archive</Button> : null}
                </Space>
              ),
            },
          ]}
        />
        {(canApprove || canManage) ? <Input style={{ marginTop: 12 }} value={decisionReason} onChange={(event) => setDecisionReason(event.target.value)} placeholder="Approval rationale" /> : null}
      </Card>
      <Modal title={selected ? "Edit Tailoring Record" : "New Tailoring Record"} open={modalOpen} onCancel={() => setModalOpen(false)} onOk={() => void submit()} confirmLoading={createMutation.isPending || updateMutation.isPending}>
        <Form layout="vertical" form={form}>
          <Form.Item name="projectId" label="Project" rules={[{ required: true }]}>
            <Select options={(projectsQuery.data?.items ?? []).map((project) => ({ value: project.id, label: `${project.code} · ${project.name}` }))} showSearch optionFilterProp="label" />
          </Form.Item>
          <Form.Item name="requesterUserId" label="Requester User Id" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="requestedChange" label="Requested Deviation" rules={[{ required: true }]}><Input.TextArea rows={3} /></Form.Item>
          <Form.Item name="reason" label="Justification" rules={[{ required: true }]}><Input.TextArea rows={2} /></Form.Item>
          <Form.Item name="impactSummary" label="Impact Summary" rules={[{ required: true }]}><Input.TextArea rows={2} /></Form.Item>
          <Form.Item name="impactedProcessAssetId" label="Impacted Process Asset">
            <Select allowClear options={(assetsQuery.data?.items ?? []).map((asset) => ({ value: asset.id, label: `${asset.code} · ${asset.name}` }))} showSearch optionFilterProp="label" />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
