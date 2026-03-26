import { useEffect, useState } from "react";
import { Alert, Button, Card, Checkbox, Flex, Form, Input, List, Select, Space, Table, Tag, Typography, message } from "antd";
import { useProjectOptions } from "../../users";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useCreateRequirementBaseline, useRequirementBaselines, useRequirements } from "../hooks/useRequirements";

const { Title, Paragraph, Text } = Typography;

export function RequirementBaselinesPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.requirements.read);
  const canBaseline = permissionState.hasPermission(permissions.requirements.baseline);
  const [messageApi, contextHolder] = message.useMessage();
  const [form] = Form.useForm();
  const [selectedProjectId, setSelectedProjectId] = useState<string | undefined>(undefined);
  const baselinesQuery = useRequirementBaselines(selectedProjectId, undefined, 1, 20, canRead);
  const approvedRequirementsQuery = useRequirements({ projectId: selectedProjectId, status: "approved", page: 1, pageSize: 100 }, canRead);
  const createBaselineMutation = useCreateRequirementBaseline();
  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 20 });

  useEffect(() => {
    form.setFieldValue("projectId", selectedProjectId);
  }, [form, selectedProjectId]);

  if (!canRead) {
    return <Alert type="warning" showIcon message="Requirement baseline access is not available for this account." />;
  }

  const handleCreate = async () => {
    const values = await form.validateFields();
    try {
      await createBaselineMutation.mutateAsync(values);
      void messageApi.success("Requirement baseline created.");
      form.resetFields();
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create baseline");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Title level={3} style={{ margin: 0 }}>Requirement Baselines</Title>
        <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
          Lock approved requirements into governed project baselines once mandatory traceability is complete.
        </Paragraph>
      </Card>

      <Card variant="borderless" title="Create Baseline">
        <Form form={form} layout="vertical">
          <Flex gap={16} wrap="wrap" align="start">
            <Form.Item label="Project" name="projectId" rules={[{ required: true }]} style={{ minWidth: 240, flex: "1 1 240px" }}>
              <Select
                showSearch
                options={projectOptions.options}
                onSearch={projectOptions.onSearch}
                onChange={(value) => setSelectedProjectId(value)}
                onPopupScroll={(event) => {
                  const target = event.target as HTMLDivElement;
                  if (target.scrollTop + target.clientHeight >= target.scrollHeight - 24) {
                    projectOptions.onLoadMore();
                  }
                }}
              />
            </Form.Item>
            <Form.Item label="Baseline Name" name="baselineName" rules={[{ required: true }]} style={{ minWidth: 240, flex: "1 1 240px" }}>
              <Input placeholder="Release Baseline 1" />
            </Form.Item>
          </Flex>
          <Form.Item label="Reason" name="reason" rules={[{ required: true }]}>
            <Input.TextArea rows={3} />
          </Form.Item>
          <Form.Item label="Approved Requirements" name="requirementIds" rules={[{ required: true, message: "Select at least one requirement." }]}>
            <Checkbox.Group style={{ width: "100%" }}>
              <List
                bordered
                dataSource={approvedRequirementsQuery.data?.items ?? []}
                locale={{ emptyText: selectedProjectId ? "No approved requirements available." : "Select a project first." }}
                renderItem={(item) => (
                  <List.Item>
                    <Checkbox value={item.id}>
                      <Space>
                        <Text strong>{item.code}</Text>
                        <Text>{item.title}</Text>
                        <Tag color={item.missingLinkCount === 0 ? "green" : "orange"}>
                          {item.missingLinkCount === 0 ? "Traceable" : `${item.missingLinkCount} gap(s)`}
                        </Tag>
                      </Space>
                    </Checkbox>
                  </List.Item>
                )}
              />
            </Checkbox.Group>
          </Form.Item>
          <Button type="primary" disabled={!canBaseline} loading={createBaselineMutation.isPending} onClick={() => void handleCreate()}>
            Create Baseline
          </Button>
        </Form>
      </Card>

      <Card variant="borderless" title="Baseline Register">
        <Table
          rowKey="id"
          loading={baselinesQuery.isLoading}
          dataSource={baselinesQuery.data?.items ?? []}
          pagination={false}
          columns={[
            { title: "Baseline", dataIndex: "baselineName", key: "baselineName" },
            { title: "Project", dataIndex: "projectName", key: "projectName" },
            { title: "Requirements", key: "requirementIds", render: (_, item) => item.requirementIds.length },
            { title: "Status", dataIndex: "status", key: "status", render: (value: string) => <Tag color="blue">{value}</Tag> },
            { title: "Approved By", dataIndex: "approvedBy", key: "approvedBy" },
            { title: "Approved At", dataIndex: "approvedAt", key: "approvedAt", render: (value: string) => new Date(value).toLocaleString() },
          ]}
        />
      </Card>
    </Space>
  );
}
