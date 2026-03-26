import { useMemo, useState } from "react";
import { Alert, Button, Card, Descriptions, Empty, Flex, Form, Input, List, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useNavigate, useParams } from "react-router-dom";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useCreateTraceabilityLink, useDeleteTraceabilityLink, useRequirement, useRequirementActions, useUpdateRequirement } from "../hooks/useRequirements";
import type { RequirementUpdateInput, TraceabilityLinkItem } from "../types/requirements";

const { Title, Paragraph } = Typography;

export function RequirementDetailPage() {
  const { requirementId = null } = useParams();
  const navigate = useNavigate();
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.requirements.read);
  const canManage = permissionState.hasPermission(permissions.requirements.manage);
  const canApprove = permissionState.hasPermission(permissions.requirements.approve);
  const canBaseline = permissionState.hasPermission(permissions.requirements.baseline);
  const canManageTraceability = permissionState.hasPermission(permissions.requirements.manageTraceability);
  const requirementQuery = useRequirement(requirementId, canRead);
  const updateRequirementMutation = useUpdateRequirement();
  const createTraceabilityLinkMutation = useCreateTraceabilityLink();
  const deleteTraceabilityLinkMutation = useDeleteTraceabilityLink();
  const actions = useRequirementActions();
  const [messageApi, contextHolder] = message.useMessage();
  const [editForm] = Form.useForm<RequirementUpdateInput>();
  const [linkForm] = Form.useForm();
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [isLinkOpen, setIsLinkOpen] = useState(false);

  const detail = requirementQuery.data;

  const linkColumns = useMemo<ColumnsType<TraceabilityLinkItem>>(
    () => [
      { title: "Target Type", dataIndex: "targetType", key: "targetType", render: (value) => <Tag>{value}</Tag> },
      { title: "Target Id", dataIndex: "targetId", key: "targetId" },
      { title: "Rule", dataIndex: "linkRule", key: "linkRule" },
      { title: "Status", dataIndex: "status", key: "status" },
      { title: "Created By", dataIndex: "createdBy", key: "createdBy" },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => (
          <Button
            size="small"
            danger
            disabled={!canManageTraceability}
            onClick={() =>
              void deleteTraceabilityLinkMutation.mutateAsync(item.id).then(() => {
                void messageApi.success("Traceability link removed.");
              }).catch((error) => {
                const presentation = getApiErrorPresentation(error, "Unable to delete traceability link");
                void messageApi.error(presentation.description);
              })
            }
          >
            Remove
          </Button>
        ),
      },
    ],
    [canManageTraceability, deleteTraceabilityLinkMutation, messageApi],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Requirement access is not available for this account." />;
  }

  if (!detail && !requirementQuery.isLoading) {
    return <Empty description="Requirement not found." />;
  }

  const latestVersion = detail?.versions[0];

  const openEdit = () => {
    if (!detail || !latestVersion) {
      return;
    }

    editForm.setFieldsValue({
      title: detail.title,
      description: detail.description,
      priority: detail.priority as RequirementUpdateInput["priority"],
      ownerUserId: detail.ownerUserId,
      businessReason: latestVersion.businessReason,
      acceptanceCriteria: latestVersion.acceptanceCriteria,
      securityImpact: latestVersion.securityImpact,
      performanceImpact: latestVersion.performanceImpact,
    });
    setIsEditOpen(true);
  };

  const handleEdit = async () => {
    if (!requirementId) {
      return;
    }

    const values = await editForm.validateFields();
    try {
      await updateRequirementMutation.mutateAsync({ id: requirementId, input: values });
      void messageApi.success("Requirement updated.");
      setIsEditOpen(false);
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update requirement");
      void messageApi.error(presentation.description);
    }
  };

  const handleCreateLink = async () => {
    if (!requirementId) {
      return;
    }

    const values = await linkForm.validateFields();
    try {
      await createTraceabilityLinkMutation.mutateAsync({
        sourceType: "requirement",
        sourceId: requirementId,
        targetType: values.targetType,
        targetId: values.targetId,
        linkRule: values.linkRule,
      });
      void messageApi.success("Traceability link added.");
      setIsLinkOpen(false);
      linkForm.resetFields();
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create traceability link");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless" loading={requirementQuery.isLoading}>
        <Flex justify="space-between" gap={16} wrap="wrap">
          <div>
            <Title level={3} style={{ margin: 0 }}>
              {detail?.code}
            </Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {detail?.projectName}
            </Paragraph>
          </div>
          <Flex gap={8} wrap>
            <Button onClick={() => navigate("/app/requirements")}>Back</Button>
            <Button disabled={!canManage} onClick={openEdit}>
              Edit
            </Button>
            <Button disabled={!canManage || detail?.status !== "draft"} onClick={() => requirementId && void actions.submit.mutateAsync(requirementId)}>
              Submit
            </Button>
            <Button
              disabled={!canApprove || detail?.status !== "review"}
              onClick={() =>
                Modal.confirm({
                  title: "Approve requirement",
                  content: "Approve this requirement with a standard decision reason.",
                  onOk: async () => {
                    if (requirementId) {
                      await actions.approve.mutateAsync({ requirementId, reason: "Approved from detail screen" });
                    }
                  },
                })
              }
            >
              Approve
            </Button>
            <Button disabled={!canBaseline || detail?.status !== "approved"} onClick={() => requirementId && void actions.baseline.mutateAsync(requirementId)}>
              Baseline
            </Button>
            <Button
              danger
              disabled={!canBaseline}
              onClick={() =>
                Modal.confirm({
                  title: "Supersede requirement",
                  content: "Mark this requirement as superseded.",
                  onOk: async () => {
                    if (requirementId) {
                      await actions.supersede.mutateAsync({ requirementId, reason: "Superseded from detail screen" });
                    }
                  },
                })
              }
            >
              Supersede
            </Button>
          </Flex>
        </Flex>
      </Card>

      <Card variant="borderless" title="Requirement Detail">
        <Descriptions column={2} bordered size="small">
          <Descriptions.Item label="Title" span={2}>{detail?.title}</Descriptions.Item>
          <Descriptions.Item label="Description" span={2}>{detail?.description}</Descriptions.Item>
          <Descriptions.Item label="Priority">{detail?.priority}</Descriptions.Item>
          <Descriptions.Item label="Owner">{detail?.ownerUserId}</Descriptions.Item>
          <Descriptions.Item label="Status">{detail?.status}</Descriptions.Item>
          <Descriptions.Item label="Current Version">{latestVersion ? `v${latestVersion.versionNumber}` : "-"}</Descriptions.Item>
          <Descriptions.Item label="Business Reason" span={2}>{latestVersion?.businessReason ?? "-"}</Descriptions.Item>
          <Descriptions.Item label="Acceptance Criteria" span={2}>{latestVersion?.acceptanceCriteria ?? "-"}</Descriptions.Item>
          <Descriptions.Item label="Security Impact" span={2}>{latestVersion?.securityImpact ?? "-"}</Descriptions.Item>
          <Descriptions.Item label="Performance Impact" span={2}>{latestVersion?.performanceImpact ?? "-"}</Descriptions.Item>
        </Descriptions>
      </Card>

      <Card
        variant="borderless"
        title="Traceability Links"
        extra={<Button disabled={!canManageTraceability} onClick={() => setIsLinkOpen(true)}>Add Link</Button>}
      >
        <Table<TraceabilityLinkItem> rowKey="id" columns={linkColumns} dataSource={detail?.traceabilityLinks ?? []} pagination={false} />
      </Card>

      <Card variant="borderless" title="Version History">
        <List
          dataSource={detail?.versions ?? []}
          renderItem={(item) => (
            <List.Item>
              <List.Item.Meta
                title={`v${item.versionNumber} · ${item.status}`}
                description={`Created ${new Date(item.createdAt).toLocaleString()} by governed change.`}
              />
            </List.Item>
          )}
        />
      </Card>

      <Card variant="borderless" title="Change History">
        <List
          dataSource={detail?.history ?? []}
          renderItem={(item) => (
            <List.Item>
              <List.Item.Meta
                title={item.summary ?? item.eventType}
                description={`${item.actorUserId ?? "system"} · ${new Date(item.occurredAt).toLocaleString()}${item.reason ? ` · ${item.reason}` : ""}`}
              />
            </List.Item>
          )}
        />
      </Card>

      <Modal open={isEditOpen} title="Edit requirement" onOk={() => void handleEdit()} onCancel={() => setIsEditOpen(false)} confirmLoading={updateRequirementMutation.isPending}>
        <Form form={editForm} layout="vertical">
          <Form.Item label="Title" name="title" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item label="Description" name="description" rules={[{ required: true }]}><Input.TextArea rows={3} /></Form.Item>
          <Form.Item label="Priority" name="priority" rules={[{ required: true }]}><Select options={["low", "medium", "high", "critical"].map((value) => ({ label: value, value }))} /></Form.Item>
          <Form.Item label="Owner User Id" name="ownerUserId" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item label="Business Reason" name="businessReason" rules={[{ required: true }]}><Input.TextArea rows={2} /></Form.Item>
          <Form.Item label="Acceptance Criteria" name="acceptanceCriteria" rules={[{ required: true }]}><Input.TextArea rows={3} /></Form.Item>
          <Form.Item label="Security Impact" name="securityImpact"><Input.TextArea rows={2} /></Form.Item>
          <Form.Item label="Performance Impact" name="performanceImpact"><Input.TextArea rows={2} /></Form.Item>
        </Form>
      </Modal>

      <Modal open={isLinkOpen} title="Add traceability link" onOk={() => void handleCreateLink()} onCancel={() => setIsLinkOpen(false)} confirmLoading={createTraceabilityLinkMutation.isPending}>
        <Form form={linkForm} layout="vertical" initialValues={{ linkRule: "implements" }}>
          <Form.Item label="Target Type" name="targetType" rules={[{ required: true }]}><Select options={["document", "test", "change_request", "release"].map((value) => ({ label: value, value }))} /></Form.Item>
          <Form.Item label="Target Id" name="targetId" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item label="Link Rule" name="linkRule" rules={[{ required: true }]}><Input /></Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
