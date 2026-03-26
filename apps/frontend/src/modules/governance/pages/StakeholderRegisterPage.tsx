import { useState } from "react";
import { Alert, Button, Card, Form, Input, Modal, Select, Space, Table, Tag, Typography } from "antd";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useProjectList } from "../../users/public";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import { useArchiveStakeholder, useCreateStakeholder, useStakeholders, useUpdateStakeholder } from "../hooks/useGovernance";
import type { Stakeholder, StakeholderFormInput } from "../types/governance";

const { Title, Text } = Typography;

export function StakeholderRegisterPage() {
  const permissionState = usePermissions();
  const canManage = permissionState.hasPermission(permissions.governance.stakeholderManage);
  const [modalOpen, setModalOpen] = useState(false);
  const [selected, setSelected] = useState<Stakeholder | null>(null);
  const [form] = Form.useForm<StakeholderFormInput>();
  const stakeholdersQuery = useStakeholders({ page: 1, pageSize: 30 });
  const projectsQuery = useProjectList({ page: 1, pageSize: 100 });
  const createMutation = useCreateStakeholder();
  const updateMutation = useUpdateStakeholder();
  const archiveMutation = useArchiveStakeholder();
  const error = stakeholdersQuery.error ?? createMutation.error ?? updateMutation.error ?? archiveMutation.error;

  const openCreate = () => {
    setSelected(null);
    form.resetFields();
    setModalOpen(true);
  };

  const openEdit = (item: Stakeholder) => {
    setSelected(item);
    form.setFieldsValue(item);
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
        <Title level={3} style={{ margin: 0 }}>Stakeholder Register</Title>
        <Text type="secondary">Project-level stakeholder ownership and communication coverage.</Text>
      </Card>
      {error ? <Alert type="error" showIcon message={getApiErrorPresentation(error).title} description={getApiErrorPresentation(error).description} /> : null}
      <Card extra={canManage ? <Button type="primary" onClick={openCreate}>New Stakeholder</Button> : null}>
        <Table
          rowKey="id"
          loading={stakeholdersQuery.isLoading}
          dataSource={stakeholdersQuery.data?.items ?? []}
          pagination={false}
          columns={[
            { title: "Project", dataIndex: "projectName", key: "projectName" },
            { title: "Name", dataIndex: "name", key: "name" },
            { title: "Role", dataIndex: "roleName", key: "roleName" },
            { title: "Influence", dataIndex: "influenceLevel", key: "influenceLevel" },
            { title: "Contact", dataIndex: "contactChannel", key: "contactChannel" },
            { title: "Status", key: "status", render: (_, item) => <Tag>{item.status}</Tag> },
            {
              title: "Actions",
              key: "actions",
              render: (_, item) => canManage ? (
                <Space wrap>
                  <Button size="small" onClick={() => openEdit(item)}>Edit</Button>
                  {item.status !== "archived" ? <Button size="small" danger onClick={() => void archiveMutation.mutateAsync(item.id)}>Archive</Button> : null}
                </Space>
              ) : null,
            },
          ]}
        />
      </Card>
      <Modal title={selected ? "Edit Stakeholder" : "New Stakeholder"} open={modalOpen} onCancel={() => setModalOpen(false)} onOk={() => void submit()} confirmLoading={createMutation.isPending || updateMutation.isPending}>
        <Form layout="vertical" form={form}>
          <Form.Item name="projectId" label="Project" rules={[{ required: true }]}>
            <Select options={(projectsQuery.data?.items ?? []).map((project) => ({ value: project.id, label: `${project.code} · ${project.name}` }))} showSearch optionFilterProp="label" />
          </Form.Item>
          <Form.Item name="name" label="Name" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="roleName" label="Role" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="influenceLevel" label="Influence Level" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="contactChannel" label="Contact Channel" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="status" label="Status"><Select options={[{ value: "active", label: "Active" }, { value: "archived", label: "Archived" }]} /></Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
