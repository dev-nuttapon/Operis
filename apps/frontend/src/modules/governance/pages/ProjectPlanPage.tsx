import { useState } from "react";
import dayjs from "dayjs";
import { Alert, Button, Card, DatePicker, Form, Input, Modal, Select, Space, Table, Tag, Typography } from "antd";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useAuth } from "../../auth";
import { useProjectList } from "../../users/public";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import { useCreateProjectPlan, useProjectPlanActions, useProjectPlans, useUpdateProjectPlan } from "../hooks/useGovernance";
import type { ProjectPlanFormInput, ProjectPlanListItem } from "../types/governance";

const { Title, Text } = Typography;

type PlanFormValues = Omit<ProjectPlanFormInput, "startDate" | "targetEndDate" | "milestones" | "roles"> & {
  startDate: dayjs.Dayjs;
  targetEndDate: dayjs.Dayjs;
  milestonesText: string;
  rolesText: string;
};

export function ProjectPlanPage() {
  const { user } = useAuth();
  const permissionState = usePermissions();
  const canManage = permissionState.hasPermission(permissions.governance.projectPlanManage);
  const canApprove = permissionState.hasPermission(permissions.governance.projectPlanApprove);
  const [modalOpen, setModalOpen] = useState(false);
  const [approvalReason, setApprovalReason] = useState("");
  const [selected, setSelected] = useState<ProjectPlanListItem | null>(null);
  const [form] = Form.useForm<PlanFormValues>();
  const plansQuery = useProjectPlans({ page: 1, pageSize: 20 });
  const projectsQuery = useProjectList({ page: 1, pageSize: 100 });
  const createMutation = useCreateProjectPlan();
  const updateMutation = useUpdateProjectPlan();
  const actions = useProjectPlanActions();
  const error = plansQuery.error ?? createMutation.error ?? updateMutation.error;

  const openCreate = () => {
    setSelected(null);
    form.resetFields();
    form.setFieldsValue({ ownerUserId: String(user?.email ?? user?.name ?? "") } as Partial<PlanFormValues>);
    setModalOpen(true);
  };

  const openEdit = (item: ProjectPlanListItem) => {
    setSelected(item);
    form.setFieldsValue({
      projectId: item.projectId,
      name: item.name,
      lifecycleModel: item.lifecycleModel,
      ownerUserId: item.ownerUserId,
      startDate: dayjs(item.startDate),
      targetEndDate: dayjs(item.targetEndDate),
    } as Partial<PlanFormValues>);
    setModalOpen(true);
  };

  const submit = async () => {
    const values = await form.validateFields();
    const input: ProjectPlanFormInput = {
      projectId: values.projectId,
      name: values.name,
      scopeSummary: values.scopeSummary,
      lifecycleModel: values.lifecycleModel,
      startDate: values.startDate.format("YYYY-MM-DD"),
      targetEndDate: values.targetEndDate.format("YYYY-MM-DD"),
      ownerUserId: values.ownerUserId,
      milestones: values.milestonesText.split("\n").map((item) => item.trim()).filter(Boolean),
      roles: values.rolesText.split("\n").map((item) => item.trim()).filter(Boolean),
      riskApproach: values.riskApproach,
      qualityApproach: values.qualityApproach,
    };
    if (selected) {
      await updateMutation.mutateAsync({ id: selected.id, input });
    } else {
      await createMutation.mutateAsync(input);
    }
    setModalOpen(false);
  };

  return (
    <Space direction="vertical" size={16} style={{ width: "100%" }}>
      <Card>
        <Title level={3} style={{ margin: 0 }}>Project Plan</Title>
        <Text type="secondary">Governance baseline path for Draft → Review → Approved → Baseline → Superseded.</Text>
      </Card>
      {error ? <Alert type="error" showIcon message={getApiErrorPresentation(error).title} description={getApiErrorPresentation(error).description} /> : null}
      <Card extra={canManage ? <Button type="primary" onClick={openCreate}>New Project Plan</Button> : null}>
        <Table
          rowKey="id"
          loading={plansQuery.isLoading}
          dataSource={plansQuery.data?.items ?? []}
          pagination={false}
          columns={[
            { title: "Project", dataIndex: "projectName", key: "projectName" },
            { title: "Plan", dataIndex: "name", key: "name" },
            { title: "Lifecycle", dataIndex: "lifecycleModel", key: "lifecycleModel" },
            { title: "Owner", dataIndex: "ownerUserId", key: "ownerUserId" },
            { title: "Status", key: "status", render: (_, item) => <Tag>{item.status}</Tag> },
            {
              title: "Actions",
              key: "actions",
              render: (_, item) => (
                <Space wrap>
                  {canManage ? <Button size="small" onClick={() => openEdit(item)}>Edit</Button> : null}
                  {canManage && item.status === "draft" ? <Button size="small" onClick={() => void actions.submitReview.mutateAsync(item.id)}>Submit Review</Button> : null}
                  {canApprove && item.status === "review" ? <Button size="small" onClick={() => void actions.approve.mutateAsync({ id: item.id, reason: approvalReason || "Project plan approved" })}>Approve</Button> : null}
                  {canApprove && item.status === "approved" ? <Button size="small" onClick={() => void actions.baseline.mutateAsync(item.id)}>Baseline</Button> : null}
                  {canApprove && item.status === "baseline" ? <Button size="small" danger onClick={() => void actions.supersede.mutateAsync({ id: item.id, reason: approvalReason || "Superseded by newer plan" })}>Supersede</Button> : null}
                </Space>
              ),
            },
          ]}
        />
        {canApprove ? <Input style={{ marginTop: 12 }} value={approvalReason} onChange={(event) => setApprovalReason(event.target.value)} placeholder="Approval or supersede rationale" /> : null}
      </Card>
      <Modal title={selected ? "Edit Project Plan" : "New Project Plan"} open={modalOpen} onCancel={() => setModalOpen(false)} onOk={() => void submit()} confirmLoading={createMutation.isPending || updateMutation.isPending} width={720}>
        <Form layout="vertical" form={form}>
          <Form.Item name="projectId" label="Project" rules={[{ required: true }]}>
            <Select options={(projectsQuery.data?.items ?? []).map((project) => ({ value: project.id, label: `${project.code} · ${project.name}` }))} showSearch optionFilterProp="label" />
          </Form.Item>
          <Form.Item name="name" label="Plan Name" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="scopeSummary" label="Scope Summary" rules={[{ required: true }]}><Input.TextArea rows={3} /></Form.Item>
          <Form.Item name="lifecycleModel" label="Lifecycle Model" rules={[{ required: true }]}><Input /></Form.Item>
          <Space style={{ width: "100%" }}>
            <Form.Item name="startDate" label="Start Date" rules={[{ required: true }]}><DatePicker /></Form.Item>
            <Form.Item name="targetEndDate" label="Target End Date" rules={[{ required: true }]}><DatePicker /></Form.Item>
          </Space>
          <Form.Item name="ownerUserId" label="Owner User Id" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="milestonesText" label="Milestones"><Input.TextArea rows={3} placeholder="One milestone per line" /></Form.Item>
          <Form.Item name="rolesText" label="Roles"><Input.TextArea rows={3} placeholder="One role per line" /></Form.Item>
          <Form.Item name="riskApproach" label="Risk Approach" rules={[{ required: true }]}><Input.TextArea rows={2} /></Form.Item>
          <Form.Item name="qualityApproach" label="Quality Approach" rules={[{ required: true }]}><Input.TextArea rows={2} /></Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
