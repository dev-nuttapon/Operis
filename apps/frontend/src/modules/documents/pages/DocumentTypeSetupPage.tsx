import { useState } from "react";
import { App, Button, Card, Drawer, Form, Input, Select, Space, Switch, Table, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { SettingOutlined } from "@ant-design/icons";
import type { DocumentTypeListItem } from "../api/documentsApi";
import { useCreateDocumentType, useDocumentTypes, useUpdateDocumentType } from "../hooks/useDocuments";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";

const { Title, Paragraph } = Typography;

type FormValues = {
  code: string;
  name: string;
  moduleOwner: string;
  classificationDefault: string;
  retentionClassDefault: string;
  approvalRequired: boolean;
  status: string;
};

export function DocumentTypeSetupPage() {
  const { notification } = App.useApp();
  const permissionState = usePermissions();
  const canManage = permissionState.hasPermission(permissions.documents.deactivate);
  const documentTypesQuery = useDocumentTypes({ page: 1, pageSize: 100 });
  const createMutation = useCreateDocumentType();
  const updateMutation = useUpdateDocumentType();
  const [form] = Form.useForm<FormValues>();
  const [editing, setEditing] = useState<DocumentTypeListItem | null>(null);
  const [open, setOpen] = useState(false);

  const columns: ColumnsType<DocumentTypeListItem> = [
    { title: "Code", dataIndex: "code", key: "code" },
    { title: "Name", dataIndex: "name", key: "name" },
    { title: "Module owner", dataIndex: "moduleOwner", key: "moduleOwner" },
    { title: "Classification", dataIndex: "classificationDefault", key: "classificationDefault" },
    { title: "Retention", dataIndex: "retentionClassDefault", key: "retentionClassDefault" },
    { title: "Approval required", dataIndex: "approvalRequired", key: "approvalRequired", render: (value) => (value ? "Yes" : "No") },
    { title: "Status", dataIndex: "status", key: "status", render: (value) => <Tag color={value === "active" ? "green" : "default"}>{value}</Tag> },
    {
      title: "Actions",
      key: "actions",
      render: (_, item) => (
        <Button
          size="small"
          disabled={!canManage}
          onClick={() => {
            setEditing(item);
            setOpen(true);
            form.setFieldsValue({
              code: item.code,
              name: item.name,
              moduleOwner: item.moduleOwner,
              classificationDefault: item.classificationDefault,
              retentionClassDefault: item.retentionClassDefault,
              approvalRequired: item.approvalRequired,
              status: item.status,
            });
          }}
        >
          Edit
        </Button>
      ),
    },
  ];

  const submit = async () => {
    const values = await form.validateFields();
    if (editing) {
      await updateMutation.mutateAsync({ documentTypeId: editing.id, payload: values });
      notification.success({ message: "Document type updated" });
    } else {
      await createMutation.mutateAsync(values);
      notification.success({ message: "Document type created" });
    }
    setOpen(false);
    setEditing(null);
    form.resetFields();
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div
            style={{
              width: 48,
              height: 48,
              borderRadius: 14,
              display: "grid",
              placeItems: "center",
              background: "linear-gradient(135deg, #6d28d9, #111827)",
              color: "#fff",
            }}
          >
            <SettingOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Document Type Setup</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Manage governed document categories, default controls, and active/deprecated state.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card
        variant="borderless"
        extra={<Button type="primary" disabled={!canManage} onClick={() => { setEditing(null); form.resetFields(); form.setFieldValue("status", "active"); setOpen(true); }}>New type</Button>}
      >
        <Table<DocumentTypeListItem> rowKey="id" columns={columns} dataSource={documentTypesQuery.data?.items ?? []} loading={documentTypesQuery.isLoading} pagination={false} />
      </Card>

      <Drawer
        title={editing ? "Edit document type" : "Create document type"}
        open={open}
        onClose={() => {
          setOpen(false);
          setEditing(null);
        }}
        width={420}
      >
        <Form form={form} layout="vertical" initialValues={{ approvalRequired: true, status: "active", classificationDefault: "internal", retentionClassDefault: "standard" }}>
          <Form.Item name="code" label="Code" rules={[{ required: true, message: "Code is required." }]}>
            <Input />
          </Form.Item>
          <Form.Item name="name" label="Name" rules={[{ required: true, message: "Name is required." }]}>
            <Input />
          </Form.Item>
          <Form.Item name="moduleOwner" label="Module owner" rules={[{ required: true, message: "Module owner is required." }]}>
            <Input />
          </Form.Item>
          <Form.Item name="classificationDefault" label="Default classification" rules={[{ required: true }]}>
            <Select options={["public", "internal", "confidential", "restricted"].map((item) => ({ label: item, value: item }))} />
          </Form.Item>
          <Form.Item name="retentionClassDefault" label="Default retention" rules={[{ required: true }]}>
            <Select options={["short_term", "standard", "regulated", "permanent"].map((item) => ({ label: item, value: item }))} />
          </Form.Item>
          <Form.Item name="status" label="Status" rules={[{ required: true }]}>
            <Select options={[{ label: "active", value: "active" }, { label: "deprecated", value: "deprecated" }]} />
          </Form.Item>
          <Form.Item name="approvalRequired" label="Approval required" valuePropName="checked">
            <Switch />
          </Form.Item>
          <Space>
            <Button type="primary" onClick={submit} loading={createMutation.isPending || updateMutation.isPending}>Save</Button>
            <Button onClick={() => setOpen(false)}>Cancel</Button>
          </Space>
        </Form>
      </Drawer>
    </Space>
  );
}
