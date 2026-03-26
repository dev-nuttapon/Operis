import { useState } from "react";
import { Alert, Button, Card, Checkbox, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useProjectOptions } from "../../users";
import { useBaselineRegistry, useBaselineRegistryActions, useChangeRequests, useCreateBaselineRegistry } from "../hooks/useChangeControl";

const { Title, Paragraph } = Typography;

export function BaselineRegistryPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.changeControl.manageBaselines, permissions.changeControl.approveBaselines, permissions.changeControl.readConfiguration);
  const canManage = permissionState.hasPermission(permissions.changeControl.manageBaselines);
  const canApprove = permissionState.hasPermission(permissions.changeControl.approveBaselines);
  const canEmergencyOverride = permissionState.hasPermission(permissions.changeControl.emergencyOverride);
  const [messageApi, contextHolder] = message.useMessage();
  const [form] = Form.useForm();
  const [overrideForm] = Form.useForm();
  const [selectedProjectId, setSelectedProjectId] = useState<string | undefined>(undefined);
  const [overrideModal, setOverrideModal] = useState<{ open: boolean; id?: string }>({ open: false });
  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 20 });
  const baselineQuery = useBaselineRegistry({ projectId: selectedProjectId, page: 1, pageSize: 20 }, canRead);
  const approvedChangeRequestsQuery = useChangeRequests({ projectId: selectedProjectId, status: "approved", page: 1, pageSize: 100 }, canRead);
  const createMutation = useCreateBaselineRegistry();
  const actions = useBaselineRegistryActions();

  if (!canRead) {
    return <Alert type="warning" showIcon message="Baseline registry access is not available for this account." />;
  }

  const handleCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(values);
      void messageApi.success("Baseline registry record created.");
      form.resetFields();
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create baseline registry record");
      void messageApi.error(presentation.description);
    }
  };

  const openOverrideModal = (id: string) => {
    setOverrideModal({ open: true, id });
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Title level={3} style={{ margin: 0 }}>Baseline Registry</Title>
        <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>Governed baseline records that require approved change requests before approval and lock.</Paragraph>
      </Card>

      <Card variant="borderless" title="Create Baseline Registry Record">
        <Form form={form} layout="vertical">
          <Flex gap={16} wrap="wrap">
            <Form.Item label="Project" name="projectId" rules={[{ required: true }]} style={{ minWidth: 220, flex: "1 1 220px" }}>
              <Select showSearch options={projectOptions.options} onSearch={projectOptions.onSearch} onPopupScroll={(event) => { const target = event.target as HTMLDivElement; if (target.scrollTop + target.clientHeight >= target.scrollHeight - 24) { projectOptions.onLoadMore(); } }} onChange={(value) => setSelectedProjectId(value)} />
            </Form.Item>
            <Form.Item label="Baseline Name" name="baselineName" rules={[{ required: true }]} style={{ minWidth: 220, flex: "1 1 220px" }}><Input /></Form.Item>
            <Form.Item label="Baseline Type" name="baselineType" rules={[{ required: true }]} style={{ minWidth: 180, flex: "1 1 180px" }}><Select options={["requirements", "configuration", "documents", "project-plan"].map((value) => ({ label: value, value }))} /></Form.Item>
          </Flex>
          <Flex gap={16} wrap="wrap">
            <Form.Item label="Source Entity Type" name="sourceEntityType" rules={[{ required: true }]} style={{ minWidth: 220, flex: "1 1 220px" }}>
              <Select options={[{ label: "configuration_item", value: "configuration_item" }, { label: "requirement_baseline", value: "requirement_baseline" }, { label: "project_plan", value: "project_plan" }, { label: "document", value: "document" }]} />
            </Form.Item>
            <Form.Item label="Source Entity Id" name="sourceEntityId" rules={[{ required: true }]} style={{ minWidth: 220, flex: "1 1 220px" }}>
              <Input placeholder="Paste eligible source entity id" />
            </Form.Item>
            <Form.Item label="Approved Change Request" name="changeRequestId" rules={[{ required: true }]} style={{ minWidth: 220, flex: "1 1 220px" }}>
              <Select options={(approvedChangeRequestsQuery.data?.items ?? []).map((item) => ({ label: `${item.code} · ${item.title}`, value: item.id }))} />
            </Form.Item>
          </Flex>
          <Button type="primary" disabled={!canManage} loading={createMutation.isPending} onClick={() => void handleCreate()}>Create Baseline Record</Button>
        </Form>
      </Card>

      <Card variant="borderless" title="Baseline Register">
        <Table
          rowKey="id"
          loading={baselineQuery.isLoading}
          dataSource={baselineQuery.data?.items ?? []}
          pagination={false}
          columns={[
            { title: "Baseline", dataIndex: "baselineName", key: "baselineName" },
            { title: "Project", dataIndex: "projectName", key: "projectName" },
            { title: "Type", dataIndex: "baselineType", key: "baselineType", render: (value: string) => <Tag>{value}</Tag> },
            { title: "Source", key: "source", render: (_, item) => `${item.sourceEntityType}:${item.sourceEntityId}` },
            { title: "Status", dataIndex: "status", key: "status", render: (value: string) => <Tag color={value === "locked" ? "blue" : value === "superseded" ? "purple" : "default"}>{value}</Tag> },
            { title: "Approved By", dataIndex: "approvedBy", key: "approvedBy", render: (value?: string | null) => value ?? "-" },
            {
              title: "Actions",
              key: "actions",
              render: (_, item) => (
                <Flex gap={8} wrap>
                  <Button size="small" disabled={!canApprove || item.status !== "proposed"} onClick={() => void actions.approve.mutateAsync(item.id)}>Approve + Lock</Button>
                  <Button size="small" danger disabled={!canApprove || item.status !== "locked"} onClick={() => openOverrideModal(item.id)}>Supersede</Button>
                </Flex>
              ),
            },
          ]}
        />
      </Card>

      <Modal
        open={overrideModal.open}
        title="Supersede baseline"
        onCancel={() => setOverrideModal({ open: false })}
        onOk={() => {
          const formValues = overrideForm.getFieldsValue(["supersededByBaselineId", "emergencyOverride", "overrideReason"]);
          if (overrideModal.id) {
            void actions.supersede.mutateAsync({
              id: overrideModal.id,
              input: {
                supersededByBaselineId: formValues.supersededByBaselineId,
                emergencyOverride: Boolean(formValues.emergencyOverride),
                reason: formValues.overrideReason,
              },
            }).then(() => {
              void messageApi.success("Baseline superseded.");
              setOverrideModal({ open: false });
              overrideForm.resetFields();
            }).catch((error) => {
              const presentation = getApiErrorPresentation(error, "Unable to supersede baseline");
              void messageApi.error(presentation.description);
            });
          }
        }}
      >
        <Form form={overrideForm} layout="vertical">
          <Form.Item label="Superseded By Baseline" name="supersededByBaselineId">
            <Select allowClear options={(baselineQuery.data?.items ?? []).filter((item) => item.status === "locked").map((item) => ({ label: item.baselineName, value: item.id }))} />
          </Form.Item>
          <Form.Item name="emergencyOverride" valuePropName="checked">
            <Checkbox disabled={!canEmergencyOverride}>Emergency override</Checkbox>
          </Form.Item>
          <Form.Item label="Reason" name="overrideReason">
            <Input.TextArea rows={3} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
