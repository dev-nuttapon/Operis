import { useState } from "react";
import dayjs from "dayjs";
import { AuditOutlined, PlusOutlined, SendOutlined } from "@ant-design/icons";
import { Alert, Button, Card, DatePicker, Flex, Form, Input, Modal, Select, Space, Switch, Table, Tag, Typography, message } from "antd";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import {
  useCreatePolicy,
  useCreatePolicyCampaign,
  usePolicies,
  usePolicyCampaigns,
  useTransitionPolicy,
  useTransitionPolicyCampaign,
  useUpdatePolicy,
  useUpdatePolicyCampaign,
} from "../hooks/useGovernance";
import type {
  PolicyCampaignFormInput,
  PolicyCampaignItem,
  PolicyFormInput,
  PolicyListItem,
} from "../types/governance";

const { Title, Paragraph, Text } = Typography;

export function PoliciesPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.governance.policyRead, permissions.governance.policyManage, permissions.governance.policyApprove);
  const canManage = permissionState.hasPermission(permissions.governance.policyManage);
  const canApprove = permissionState.hasPermission(permissions.governance.policyApprove);
  const [messageApi, contextHolder] = message.useMessage();
  const [policyFilters, setPolicyFilters] = useState({ search: "", status: undefined as string | undefined, page: 1, pageSize: 25 });
  const [campaignFilters, setCampaignFilters] = useState({ policyId: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 25 });
  const [policyOpen, setPolicyOpen] = useState(false);
  const [campaignOpen, setCampaignOpen] = useState(false);
  const [editingPolicy, setEditingPolicy] = useState<PolicyListItem | null>(null);
  const [editingCampaign, setEditingCampaign] = useState<PolicyCampaignItem | null>(null);
  const [policyForm] = Form.useForm<PolicyFormInput & { effectiveDateValue?: dayjs.Dayjs }>();
  const [campaignForm] = Form.useForm<PolicyCampaignFormInput & { dueAtValue?: dayjs.Dayjs }>();
  const policiesQuery = usePolicies(policyFilters, canRead);
  const campaignsQuery = usePolicyCampaigns(campaignFilters, canRead);
  const createPolicyMutation = useCreatePolicy();
  const updatePolicyMutation = useUpdatePolicy();
  const transitionPolicyMutation = useTransitionPolicy();
  const createCampaignMutation = useCreatePolicyCampaign();
  const updateCampaignMutation = useUpdatePolicyCampaign();
  const transitionCampaignMutation = useTransitionPolicyCampaign();

  if (!canRead) {
    return <Alert type="warning" showIcon message="Policy access is not available for this account." />;
  }

  const submitPolicy = async () => {
    const values = await policyForm.validateFields();
    const input: PolicyFormInput = {
      policyCode: values.policyCode,
      title: values.title,
      summary: values.summary ?? null,
      effectiveDate: values.effectiveDateValue?.toISOString() ?? values.effectiveDate,
      requiresAttestation: values.requiresAttestation,
    };

    try {
      if (editingPolicy) {
        await updatePolicyMutation.mutateAsync({ id: editingPolicy.id, input });
        void messageApi.success("Policy updated.");
      } else {
        await createPolicyMutation.mutateAsync(input);
        void messageApi.success("Policy created.");
      }
      setPolicyOpen(false);
      setEditingPolicy(null);
      policyForm.resetFields();
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to save policy");
      void messageApi.error(presentation.description);
    }
  };

  const submitCampaign = async () => {
    const values = await campaignForm.validateFields();
    const input: PolicyCampaignFormInput = {
      policyId: values.policyId,
      campaignCode: values.campaignCode,
      title: values.title,
      targetScopeType: values.targetScopeType,
      targetScopeRef: values.targetScopeRef,
      dueAt: values.dueAtValue?.toISOString() ?? values.dueAt,
    };

    try {
      if (editingCampaign) {
        await updateCampaignMutation.mutateAsync({ id: editingCampaign.id, input });
        void messageApi.success("Policy campaign updated.");
      } else {
        await createCampaignMutation.mutateAsync(input);
        void messageApi.success("Policy campaign created.");
      }
      setCampaignOpen(false);
      setEditingCampaign(null);
      campaignForm.resetFields();
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to save policy campaign");
      void messageApi.error(presentation.description);
    }
  };

  const changePolicyStatus = async (item: PolicyListItem, targetStatus: string) => {
    try {
      await transitionPolicyMutation.mutateAsync({ id: item.id, input: { targetStatus, reason: `Move policy to ${targetStatus}.` } });
      void messageApi.success(`Policy moved to ${targetStatus}.`);
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to transition policy");
      void messageApi.error(presentation.description);
    }
  };

  const changeCampaignStatus = async (item: PolicyCampaignItem, targetStatus: string) => {
    try {
      await transitionCampaignMutation.mutateAsync({ id: item.id, input: { targetStatus, reason: `Move campaign to ${targetStatus}.` } });
      void messageApi.success(`Policy campaign moved to ${targetStatus}.`);
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to transition campaign");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #1d4ed8, #1e293b)", color: "#fff" }}>
            <AuditOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Policies</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Govern policy publication, acknowledgement campaigns, and publication workflow from one register.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless" title="Policy Register" extra={<Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => { setEditingPolicy(null); policyForm.resetFields(); setPolicyOpen(true); }}>New policy</Button>}>
        <Flex gap={12} wrap style={{ marginBottom: 16 }}>
          <Input.Search allowClear placeholder="Search code or title" style={{ width: 260 }} value={policyFilters.search} onChange={(event) => setPolicyFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setPolicyFilters((current) => ({ ...current, search: value, page: 1 }))} />
          <Select allowClear placeholder="Status" style={{ width: 180 }} options={["draft", "approved", "published", "retired"].map((value) => ({ value, label: value }))} value={policyFilters.status} onChange={(value) => setPolicyFilters((current) => ({ ...current, status: value, page: 1 }))} />
        </Flex>
        <Table
          rowKey="id"
          loading={policiesQuery.isLoading}
          dataSource={policiesQuery.data?.items ?? []}
          pagination={{
            current: policiesQuery.data?.page ?? policyFilters.page,
            pageSize: policiesQuery.data?.pageSize ?? policyFilters.pageSize,
            total: policiesQuery.data?.total ?? 0,
            onChange: (page, pageSize) => setPolicyFilters((current) => ({ ...current, page, pageSize })),
          }}
          columns={[
            {
              title: "Policy",
              key: "policy",
              render: (_, item: PolicyListItem) => (
                <Space direction="vertical" size={0}>
                  <Text strong>{item.policyCode}</Text>
                  <Text>{item.title}</Text>
                </Space>
              ),
            },
            { title: "Effective", dataIndex: "effectiveDate", render: (value: string) => dayjs(value).format("DD MMM YYYY") },
            { title: "Attestation", dataIndex: "requiresAttestation", render: (value: boolean) => <Tag color={value ? "gold" : "default"}>{value ? "required" : "optional"}</Tag> },
            { title: "Campaigns", dataIndex: "campaignCount" },
            { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "published" ? "green" : value === "approved" ? "blue" : value === "retired" ? "default" : "gold"}>{value}</Tag> },
            {
              title: "Actions",
              key: "actions",
              render: (_, item: PolicyListItem) => (
                <Flex gap={8} wrap>
                  <Button size="small" disabled={!canManage} onClick={() => {
                    setEditingPolicy(item);
                    policyForm.setFieldsValue({ ...item, effectiveDateValue: dayjs(item.effectiveDate) });
                    setPolicyOpen(true);
                  }}>
                    Edit
                  </Button>
                  {item.status === "draft" ? <Button size="small" disabled={!canApprove} onClick={() => void changePolicyStatus(item, "approved")}>Approve</Button> : null}
                  {item.status === "approved" ? <Button size="small" type="primary" ghost disabled={!canApprove} onClick={() => void changePolicyStatus(item, "published")}>Publish</Button> : null}
                  {item.status === "published" ? <Button size="small" danger ghost disabled={!canApprove} onClick={() => void changePolicyStatus(item, "retired")}>Retire</Button> : null}
                </Flex>
              ),
            },
          ]}
        />
      </Card>

      <Card variant="borderless" title="Acknowledgement Campaigns" extra={<Button icon={<SendOutlined />} disabled={!canManage} onClick={() => { setEditingCampaign(null); campaignForm.resetFields(); setCampaignOpen(true); }}>New campaign</Button>}>
        <Flex gap={12} wrap style={{ marginBottom: 16 }}>
          <Select allowClear placeholder="Policy" style={{ width: 260 }} options={(policiesQuery.data?.items ?? []).map((item) => ({ value: item.id, label: `${item.policyCode} · ${item.title}` }))} value={campaignFilters.policyId} onChange={(value) => setCampaignFilters((current) => ({ ...current, policyId: value, page: 1 }))} showSearch optionFilterProp="label" />
          <Select allowClear placeholder="Status" style={{ width: 180 }} options={["draft", "launched", "closed"].map((value) => ({ value, label: value }))} value={campaignFilters.status} onChange={(value) => setCampaignFilters((current) => ({ ...current, status: value, page: 1 }))} />
        </Flex>
        <Table
          rowKey="id"
          loading={campaignsQuery.isLoading}
          dataSource={campaignsQuery.data?.items ?? []}
          pagination={{
            current: campaignsQuery.data?.page ?? campaignFilters.page,
            pageSize: campaignsQuery.data?.pageSize ?? campaignFilters.pageSize,
            total: campaignsQuery.data?.total ?? 0,
            onChange: (page, pageSize) => setCampaignFilters((current) => ({ ...current, page, pageSize })),
          }}
          columns={[
            {
              title: "Campaign",
              key: "campaign",
              render: (_, item: PolicyCampaignItem) => (
                <Space direction="vertical" size={0}>
                  <Text strong>{item.campaignCode}</Text>
                  <Text>{item.title}</Text>
                  <Text type="secondary">{item.policyTitle}</Text>
                </Space>
              ),
            },
            { title: "Scope", render: (_, item: PolicyCampaignItem) => `${item.targetScopeType} · ${item.targetScopeRef}` },
            { title: "Due", dataIndex: "dueAt", render: (value: string) => dayjs(value).format("DD MMM YYYY") },
            { title: "Targeted", dataIndex: "targetUserCount" },
            { title: "Acknowledged", dataIndex: "acknowledgedCount" },
            { title: "Overdue", dataIndex: "overdueCount" },
            { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "launched" ? "green" : value === "closed" ? "default" : "gold"}>{value}</Tag> },
            {
              title: "Actions",
              key: "actions",
              render: (_, item: PolicyCampaignItem) => (
                <Flex gap={8} wrap>
                  <Button size="small" disabled={!canManage} onClick={() => {
                    setEditingCampaign(item);
                    campaignForm.setFieldsValue({ ...item, dueAtValue: dayjs(item.dueAt) });
                    setCampaignOpen(true);
                  }}>
                    Edit
                  </Button>
                  {item.status === "draft" ? <Button size="small" type="primary" ghost disabled={!canApprove} onClick={() => void changeCampaignStatus(item, "launched")}>Launch</Button> : null}
                  {item.status === "launched" ? <Button size="small" danger ghost disabled={!canApprove} onClick={() => void changeCampaignStatus(item, "closed")}>Close</Button> : null}
                </Flex>
              ),
            },
          ]}
        />
      </Card>

      <Modal title={editingPolicy ? "Edit policy" : "Create policy"} open={policyOpen} onOk={() => void submitPolicy()} onCancel={() => { setPolicyOpen(false); setEditingPolicy(null); policyForm.resetFields(); }} confirmLoading={createPolicyMutation.isPending || updatePolicyMutation.isPending} destroyOnHidden>
        <Form form={policyForm} layout="vertical" initialValues={{ requiresAttestation: true }}>
          <Form.Item label="Policy Code" name="policyCode" rules={[{ required: true, message: "Policy code is required." }]}><Input /></Form.Item>
          <Form.Item label="Title" name="title" rules={[{ required: true, message: "Policy title is required." }]}><Input /></Form.Item>
          <Form.Item label="Summary" name="summary"><Input.TextArea rows={4} /></Form.Item>
          <Form.Item label="Effective Date" name="effectiveDateValue" rules={[{ required: true, message: "Effective date is required." }]}><DatePicker style={{ width: "100%" }} /></Form.Item>
          <Form.Item label="Requires Attestation" name="requiresAttestation" valuePropName="checked"><Switch /></Form.Item>
        </Form>
      </Modal>

      <Modal title={editingCampaign ? "Edit policy campaign" : "Create policy campaign"} open={campaignOpen} onOk={() => void submitCampaign()} onCancel={() => { setCampaignOpen(false); setEditingCampaign(null); campaignForm.resetFields(); }} confirmLoading={createCampaignMutation.isPending || updateCampaignMutation.isPending} destroyOnHidden>
        <Form form={campaignForm} layout="vertical" initialValues={{ targetScopeType: "all_users" }}>
          <Form.Item label="Policy" name="policyId" rules={[{ required: true, message: "Policy is required." }]}>
            <Select showSearch optionFilterProp="label" options={(policiesQuery.data?.items ?? []).map((item) => ({ value: item.id, label: `${item.policyCode} · ${item.title}` }))} />
          </Form.Item>
          <Form.Item label="Campaign Code" name="campaignCode" rules={[{ required: true, message: "Campaign code is required." }]}><Input /></Form.Item>
          <Form.Item label="Title" name="title" rules={[{ required: true, message: "Campaign title is required." }]}><Input /></Form.Item>
          <Form.Item label="Target Scope Type" name="targetScopeType" rules={[{ required: true, message: "Scope type is required." }]}>
            <Select options={[{ value: "all_users", label: "all_users" }, { value: "project", label: "project" }, { value: "department", label: "department" }]} />
          </Form.Item>
          <Form.Item label="Target Scope Ref" name="targetScopeRef" rules={[{ required: true, message: "Scope reference is required." }]}><Input placeholder="project id, department id, or all" /></Form.Item>
          <Form.Item label="Due At" name="dueAtValue" rules={[{ required: true, message: "Due date is required." }]}><DatePicker showTime style={{ width: "100%" }} /></Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
