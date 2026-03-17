import { useRef, useState } from "react";
import { App, Button, Card, Divider, Form, Input, Space, Typography, Alert } from "antd";
import { UploadOutlined, ArrowLeftOutlined } from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import i18n from "../../../shared/i18n/config";
import { useI18nLanguage } from "../../../shared/i18n/hooks/useI18nLanguage";
import { useUploadDocument } from "../hooks/useDocuments";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import { ApiError, getApiErrorPresentation } from "../../../shared/lib/apiClient";

const { Title, Paragraph, Text } = Typography;
const allowedDocumentExtensions = [".pdf", ".doc", ".docx", ".xls", ".xlsx"] as const;

export function DocumentUploadPage() {
  const { notification } = App.useApp();
  const language = useI18nLanguage();
  const navigate = useNavigate();
  const permissionState = usePermissions();
  const canUploadDocuments = permissionState.hasPermission(permissions.documents.upload);
  const uploadDocumentMutation = useUploadDocument();
  const [form] = Form.useForm();
  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const tr = (key: string) => i18n.t(key, { lng: language });

  const handlePickFile = () => {
    fileInputRef.current?.click();
  };

  const handleFileSelected = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0] ?? null;
    if (!file) {
      return;
    }

    const normalizedFileName = file.name.trim().toLowerCase();
    const isAllowedFileType = allowedDocumentExtensions.some((extension) => normalizedFileName.endsWith(extension));
    if (!isAllowedFileType) {
      notification.error({
        message: tr("documents.upload.invalid_type_title"),
        description: tr("documents.upload.invalid_type_description"),
      });
      event.target.value = "";
      return;
    }

    setSelectedFile(file);
  };

  const handleSubmit = async () => {
    if (!canUploadDocuments) {
      return;
    }

    try {
      const values = await form.validateFields();
      if (!selectedFile) {
        notification.error({
          message: tr("documents.upload.file_required_title"),
          description: tr("documents.upload.file_required_description"),
        });
        return;
      }

      await uploadDocumentMutation.mutateAsync({ file: selectedFile, documentName: values.documentName });
      notification.success({
        message: tr("documents.upload.success_title"),
        description: tr("documents.upload.success_description"),
      });
      navigate("/app/documents");
    } catch (error) {
      if (error && typeof error === "object" && "errorFields" in error) {
        return;
      }

      const presentation =
        error instanceof ApiError
          ? getApiErrorPresentation(error, tr("documents.upload.failed_title"))
          : getApiErrorPresentation(error, tr("documents.upload.failed_title"));

      notification.error({
        message: presentation.title,
        description: presentation.description,
      });
    }
  };

  return (
    <Card bordered={false} style={{ borderRadius: 16 }}>
      <Space style={{ width: "100%", justifyContent: "space-between", marginBottom: 16 }}>
        <Title level={2} style={{ margin: 0 }}>{tr("documents.upload_page.title")}</Title>
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate("/app/documents")}>
          {tr("documents.upload_page.back_action")}
        </Button>
      </Space>

      <Paragraph type="secondary">{tr("documents.upload_page.description")}</Paragraph>

      <Divider />

      {!canUploadDocuments ? (
        <Alert type="info" showIcon message={tr("documents.read_only_title")} description={tr("documents.read_only_description")} style={{ marginBottom: 24 }} />
      ) : null}

      <Form form={form} layout="vertical" disabled={!canUploadDocuments}>
        <Form.Item
          name="documentName"
          label={tr("documents.upload_page.fields.document_name")}
          rules={[{ required: true, message: tr("documents.upload_page.fields.document_name_required") }]}
        >
          <Input placeholder={tr("documents.upload_page.fields.document_name_placeholder")} />
        </Form.Item>

        <Form.Item label={tr("documents.upload_page.fields.file")}>
          <Space direction="vertical" size={8} style={{ width: "100%" }}>
            <input
              ref={fileInputRef}
              type="file"
              accept={allowedDocumentExtensions.join(",")}
              style={{ display: "none" }}
              onChange={handleFileSelected}
            />
            <Button icon={<UploadOutlined />} onClick={handlePickFile} disabled={!canUploadDocuments}>
              {tr("documents.upload_page.actions.pick_file")}
            </Button>
            <Text type={selectedFile ? "secondary" : "secondary"}>
              {selectedFile ? selectedFile.name : tr("documents.upload_page.fields.file_placeholder")}
            </Text>
            <Text type="secondary">{tr("documents.upload.allowed_file_types")}</Text>
          </Space>
        </Form.Item>

        <Space>
          <Button type="primary" onClick={handleSubmit} loading={uploadDocumentMutation.isPending} disabled={!canUploadDocuments}>
            {tr("documents.upload_page.actions.submit")}
          </Button>
          <Button onClick={() => navigate("/app/documents")}>{tr("documents.upload_page.actions.cancel")}</Button>
        </Space>
      </Form>
    </Card>
  );
}
