import { useState } from "react";
import { Alert, Button, Card, Form, Input, Modal, Space, Switch, Table, Tag, Typography } from "antd";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useAuth } from "../../auth";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import { useCreateQaChecklist, useQaChecklistActions, useQaChecklists, useUpdateQaChecklist } from "../hooks/useGovernance";
import type { QaChecklistFormInput, QaChecklistItem } from "../types/governance";

const { Title, Text } = Typography;

export function QaReviewChecklistPage() {
  const { user } = useAuth();
  const permissionState = usePermissions();
  const canManage = permissionState.hasPermission(permissions.governance.qaChecklistManage);
  const [modalOpen, setModalOpen] = useState(false);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [items, setItems] = useState<QaChecklistItem[]>([{ itemText: "", mandatory: true, applicablePhase: "phase-1", evidenceRule: "" }]);
  const [form] = Form.useForm<QaChecklistFormInput>();
  const listQuery = useQaChecklists({ page: 1, pageSize: 20 });
  const createMutation = useCreateQaChecklist();
  const updateMutation = useUpdateQaChecklist();
  const actions = useQaChecklistActions();
  const error = listQuery.error ?? createMutation.error ?? updateMutation.error;

  const openCreate = () => {
    setSelectedId(null);
    setItems([{ itemText: "", mandatory: true, applicablePhase: "phase-1", evidenceRule: "" }]);
    form.resetFields();
    form.setFieldsValue({ ownerUserId: String(user?.email ?? user?.name ?? "") });
    setModalOpen(true);
  };

  const openEdit = (record: QaChecklistFormInput & { id: string }) => {
    setSelectedId(record.id);
    form.setFieldsValue(record);
    setItems(record.items);
    setModalOpen(true);
  };

  const submit = async () => {
    const values = await form.validateFields();
    const input = { ...values, items };
    if (selectedId) {
      await updateMutation.mutateAsync({ id: selectedId, input });
    } else {
      await createMutation.mutateAsync(input);
    }
    setModalOpen(false);
  };

  return (
    <Space direction="vertical" size={16} style={{ width: "100%" }}>
      <Card>
        <Title level={3} style={{ margin: 0 }}>QA Review Checklist</Title>
        <Text type="secondary">PPQA checklist setup with Draft → Approved → Active → Deprecated flow.</Text>
      </Card>
      {error ? <Alert type="error" showIcon message={getApiErrorPresentation(error).title} description={getApiErrorPresentation(error).description} /> : null}
      <Card extra={canManage ? <Button type="primary" onClick={openCreate}>New Checklist</Button> : null}>
        <Table
          rowKey="id"
          loading={listQuery.isLoading}
          dataSource={listQuery.data?.items ?? []}
          pagination={false}
          columns={[
            { title: "Code", dataIndex: "code", key: "code" },
            { title: "Name", dataIndex: "name", key: "name" },
            { title: "Scope", dataIndex: "scope", key: "scope" },
            { title: "Owner", dataIndex: "ownerUserId", key: "ownerUserId" },
            { title: "Status", key: "status", render: (_, item) => <Tag>{item.status}</Tag> },
            {
              title: "Actions",
              key: "actions",
              render: (_, item) => canManage ? (
                <Space wrap>
                  <Button size="small" onClick={() => openEdit({ ...item, items })}>Edit</Button>
                  {item.status === "draft" ? <Button size="small" onClick={() => void actions.approve.mutateAsync(item.id)}>Approve</Button> : null}
                  {item.status === "approved" ? <Button size="small" onClick={() => void actions.activate.mutateAsync(item.id)}>Activate</Button> : null}
                  {item.status === "active" ? <Button size="small" danger onClick={() => void actions.deprecate.mutateAsync(item.id)}>Deprecate</Button> : null}
                </Space>
              ) : null,
            },
          ]}
        />
      </Card>
      <Modal title={selectedId ? "Edit QA Checklist" : "New QA Checklist"} open={modalOpen} onCancel={() => setModalOpen(false)} onOk={() => void submit()} confirmLoading={createMutation.isPending || updateMutation.isPending}>
        <Form layout="vertical" form={form}>
          <Form.Item name="code" label="Code" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="name" label="Name" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="scope" label="Scope" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="ownerUserId" label="Owner User Id" rules={[{ required: true }]}><Input /></Form.Item>
        </Form>
        <Space direction="vertical" style={{ width: "100%" }}>
          {items.map((item, index) => (
            <Card key={index} size="small">
              <Space direction="vertical" style={{ width: "100%" }}>
                <Input value={item.itemText} placeholder="Checklist item" onChange={(event) => setItems((current) => current.map((entry, itemIndex) => itemIndex === index ? { ...entry, itemText: event.target.value } : entry))} />
                <Input value={item.applicablePhase} placeholder="Applicable phase" onChange={(event) => setItems((current) => current.map((entry, itemIndex) => itemIndex === index ? { ...entry, applicablePhase: event.target.value } : entry))} />
                <Input value={item.evidenceRule} placeholder="Evidence rule" onChange={(event) => setItems((current) => current.map((entry, itemIndex) => itemIndex === index ? { ...entry, evidenceRule: event.target.value } : entry))} />
                <Space>
                  <Switch checked={item.mandatory} onChange={(checked) => setItems((current) => current.map((entry, itemIndex) => itemIndex === index ? { ...entry, mandatory: checked } : entry))} />
                  <Button danger size="small" onClick={() => setItems((current) => current.filter((_, itemIndex) => itemIndex !== index))}>Remove</Button>
                </Space>
              </Space>
            </Card>
          ))}
          <Button onClick={() => setItems((current) => [...current, { itemText: "", mandatory: false, applicablePhase: "phase-1", evidenceRule: "" }])}>Add Item</Button>
        </Space>
      </Modal>
    </Space>
  );
}
