import { App, Button, Card, Form, Input, Select, Space, Typography } from "antd";
import { ArrowLeftOutlined, PlusOutlined } from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import { useCreateDocument, useDocumentTypes } from "../hooks/useDocuments";
import { useProjectOptions } from "../../users";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";

const { Title, Paragraph } = Typography;

const classificationOptions = ["public", "internal", "confidential", "restricted"];
const retentionOptions = ["short_term", "standard", "regulated", "permanent"];

type FormValues = {
  documentTypeId: string;
  projectId: string;
  phaseCode: string;
  ownerUserId: string;
  classification: string;
  retentionClass: string;
  title: string;
  tags?: string[];
};

export function DocumentUploadPage() {
  const { notification } = App.useApp();
  const navigate = useNavigate();
  const permissionState = usePermissions();
  const canUpload = permissionState.hasPermission(permissions.documents.upload);
  const createDocumentMutation = useCreateDocument();
  const documentTypesQuery = useDocumentTypes({ page: 1, pageSize: 100, status: "active" }, canUpload);
  const projectOptions = useProjectOptions({ enabled: canUpload, assignedOnly: false, pageSize: 20 });
  const [form] = Form.useForm<FormValues>();

  const handleSubmit = async () => {
    const values = await form.validateFields();
    await createDocumentMutation.mutateAsync({
      documentTypeId: values.documentTypeId,
      projectId: values.projectId,
      phaseCode: values.phaseCode,
      ownerUserId: values.ownerUserId,
      classification: values.classification,
      retentionClass: values.retentionClass,
      title: values.title,
      tags: values.tags,
    });

    notification.success({
      message: "Document created",
      description: "The draft document shell is ready for version upload and review.",
    });
    navigate("/app/documents");
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Button icon={<ArrowLeftOutlined />} onClick={() => navigate("/app/documents")} style={{ width: "fit-content" }}>
        Back to register
      </Button>

      <Card variant="borderless">
        <Space align="start" size={16}>
          <div
            style={{
              width: 48,
              height: 48,
              borderRadius: 14,
              display: "grid",
              placeItems: "center",
              background: "linear-gradient(135deg, #2563eb, #0f172a)",
              color: "#fff",
            }}
          >
            <PlusOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>
              New governed document
            </Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Register metadata first, then upload controlled versions into the approval workflow.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Form form={form} layout="vertical" disabled={!canUpload}>
          <Form.Item name="title" label="Title" rules={[{ required: true, message: "Title is required." }]}>
            <Input placeholder="Software development plan" />
          </Form.Item>

          <Form.Item name="documentTypeId" label="Document type" rules={[{ required: true, message: "Document type is required." }]}>
            <Select
              showSearch
              placeholder="Select a governed document type"
              options={(documentTypesQuery.data?.items ?? []).map((item) => ({ label: `${item.code} · ${item.name}`, value: item.id }))}
            />
          </Form.Item>

          <Form.Item name="projectId" label="Project" rules={[{ required: true, message: "Project is required." }]}>
            <Select
              showSearch
              placeholder="Select project"
              options={projectOptions.options}
              onSearch={projectOptions.onSearch}
              onPopupScroll={(event) => {
                const target = event.target as HTMLDivElement;
                if (target.scrollTop + target.clientHeight >= target.scrollHeight - 24) {
                  projectOptions.onLoadMore();
                }
              }}
            />
          </Form.Item>

          <Form.Item name="phaseCode" label="Phase code" rules={[{ required: true, message: "Phase code is required." }]}>
            <Input placeholder="REQ, DES, DEV, TEST" />
          </Form.Item>

          <Form.Item name="ownerUserId" label="Owner user id" rules={[{ required: true, message: "Owner user id is required." }]}>
            <Input placeholder="user-123" />
          </Form.Item>

          <Form.Item name="classification" label="Classification" initialValue="internal" rules={[{ required: true }]}>
            <Select options={classificationOptions.map((item) => ({ label: item, value: item }))} />
          </Form.Item>

          <Form.Item name="retentionClass" label="Retention class" initialValue="standard" rules={[{ required: true }]}>
            <Select options={retentionOptions.map((item) => ({ label: item, value: item }))} />
          </Form.Item>

          <Form.Item name="tags" label="Tags">
            <Select mode="tags" tokenSeparators={[","]} placeholder="governance, phase-2, cmmi" />
          </Form.Item>

          <Space>
            <Button type="primary" onClick={handleSubmit} loading={createDocumentMutation.isPending} disabled={!canUpload}>
              Create document
            </Button>
            <Button onClick={() => navigate("/app/documents")}>Cancel</Button>
          </Space>
        </Form>
      </Card>
    </Space>
  );
}
