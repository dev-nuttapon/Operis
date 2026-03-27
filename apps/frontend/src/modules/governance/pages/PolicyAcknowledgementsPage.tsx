import { useState } from "react";
import dayjs from "dayjs";
import { CheckCircleOutlined } from "@ant-design/icons";
import { Alert, Button, Card, Flex, Form, Input, Modal, Select, Space, Switch, Table, Tag, Typography, message } from "antd";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreatePolicyAcknowledgement, usePolicies, usePolicyAcknowledgements, usePolicyCampaigns } from "../hooks/useGovernance";

const { Title, Paragraph, Text } = Typography;

export function PolicyAcknowledgementsPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.governance.policyRead, permissions.governance.policyManage, permissions.governance.policyApprove);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ policyId: undefined as string | undefined, campaignId: undefined as string | undefined, userId: undefined as string | undefined, status: undefined as string | undefined, onlyOverdue: false, page: 1, pageSize: 25 });
  const [selectedCampaignId, setSelectedCampaignId] = useState<string | null>(null);
  const [ackForm] = Form.useForm<{ attestationText?: string }>();
  const policiesQuery = usePolicies({ page: 1, pageSize: 100 }, canRead);
  const campaignsQuery = usePolicyCampaigns({ page: 1, pageSize: 100, policyId: filters.policyId }, canRead);
  const acknowledgementsQuery = usePolicyAcknowledgements(filters, canRead);
  const acknowledgeMutation = useCreatePolicyAcknowledgement();

  if (!canRead) {
    return <Alert type="warning" showIcon message="Policy acknowledgements are not available for this account." />;
  }

  const submitAcknowledgement = async () => {
    const values = await ackForm.validateFields();
    if (!selectedCampaignId) {
      return;
    }

    try {
      await acknowledgeMutation.mutateAsync({ policyCampaignId: selectedCampaignId, attestationText: values.attestationText ?? null });
      void messageApi.success("Policy acknowledgement recorded.");
      setSelectedCampaignId(null);
      ackForm.resetFields();
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to acknowledge policy");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #166534, #0f172a)", color: "#fff" }}>
            <CheckCircleOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Policy Acknowledgements</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Review pending and overdue acknowledgements, then complete attestation against active campaigns.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex gap={12} wrap style={{ marginBottom: 16 }}>
          <Select allowClear placeholder="Policy" style={{ width: 260 }} options={(policiesQuery.data?.items ?? []).map((item) => ({ value: item.id, label: `${item.policyCode} · ${item.title}` }))} value={filters.policyId} onChange={(value) => setFilters((current) => ({ ...current, policyId: value, campaignId: undefined, page: 1 }))} showSearch optionFilterProp="label" />
          <Select allowClear placeholder="Campaign" style={{ width: 260 }} options={(campaignsQuery.data?.items ?? []).map((item) => ({ value: item.id, label: `${item.campaignCode} · ${item.title}` }))} value={filters.campaignId} onChange={(value) => setFilters((current) => ({ ...current, campaignId: value, page: 1 }))} showSearch optionFilterProp="label" />
          <Input allowClear placeholder="User ID" style={{ width: 220 }} value={filters.userId} onChange={(event) => setFilters((current) => ({ ...current, userId: event.target.value || undefined, page: 1 }))} />
          <Select allowClear placeholder="Status" style={{ width: 160 }} options={["pending", "acknowledged"].map((value) => ({ value, label: value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          <Space>
            <Switch checked={filters.onlyOverdue} onChange={(checked) => setFilters((current) => ({ ...current, onlyOverdue: checked, page: 1 }))} />
            <Text>Only overdue</Text>
          </Space>
        </Flex>

        <Table
          rowKey="id"
          loading={acknowledgementsQuery.isLoading}
          dataSource={acknowledgementsQuery.data?.items ?? []}
          pagination={{
            current: acknowledgementsQuery.data?.page ?? filters.page,
            pageSize: acknowledgementsQuery.data?.pageSize ?? filters.pageSize,
            total: acknowledgementsQuery.data?.total ?? 0,
            onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })),
          }}
          columns={[
            {
              title: "Policy",
              key: "policy",
              render: (_, item) => (
                <Space direction="vertical" size={0}>
                  <Text strong>{item.policyTitle}</Text>
                  <Text type="secondary">{item.campaignTitle}</Text>
                </Space>
              ),
            },
            { title: "User", dataIndex: "userId" },
            { title: "Due", dataIndex: "dueAt", render: (value: string) => dayjs(value).format("DD MMM YYYY HH:mm") },
            { title: "Attestation", dataIndex: "requiresAttestation", render: (value: boolean) => <Tag color={value ? "gold" : "default"}>{value ? "required" : "optional"}</Tag> },
            {
              title: "Status",
              key: "status",
              render: (_, item) => (
                <Space>
                  <Tag color={item.status === "acknowledged" ? "green" : "default"}>{item.status}</Tag>
                  {item.isOverdue ? <Tag color="red">overdue</Tag> : null}
                </Space>
              ),
            },
            { title: "Acknowledged", dataIndex: "acknowledgedAt", render: (value: string | null) => (value ? dayjs(value).format("DD MMM YYYY HH:mm") : "-") },
            {
              title: "Actions",
              key: "actions",
              render: (_, item) => (
                <Button size="small" type="primary" disabled={item.status === "acknowledged"} onClick={() => { setSelectedCampaignId(item.policyCampaignId); ackForm.setFieldsValue({ attestationText: item.attestationText ?? undefined }); }}>
                  Acknowledge
                </Button>
              ),
            },
          ]}
        />
      </Card>

      <Modal title="Acknowledge policy" open={Boolean(selectedCampaignId)} onOk={() => void submitAcknowledgement()} onCancel={() => { setSelectedCampaignId(null); ackForm.resetFields(); }} confirmLoading={acknowledgeMutation.isPending} destroyOnHidden>
        <Form form={ackForm} layout="vertical">
          <Form.Item label="Attestation Text" name="attestationText">
            <Input.TextArea rows={4} placeholder="I have reviewed and understood this policy." />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
