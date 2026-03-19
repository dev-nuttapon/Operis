import { App, Button, Card, Form, Input, Space, Typography, Alert, Flex, Grid } from "antd";
import { UploadOutlined } from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import i18n from "../../../shared/i18n/config";
import { useI18nLanguage } from "../../../shared/i18n/hooks/useI18nLanguage";
import { useCreateDocument } from "../hooks/useDocuments";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import { ApiError, getApiErrorPresentation } from "../../../shared/lib/apiClient";

const { Title, Paragraph } = Typography;

export function DocumentUploadPage() {
  const { notification } = App.useApp();
  const language = useI18nLanguage();
  const navigate = useNavigate();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const permissionState = usePermissions();
  const canUploadDocuments = permissionState.hasPermission(permissions.documents.upload);
  const createDocumentMutation = useCreateDocument();
  const [form] = Form.useForm();
  const tr = (key: string) => i18n.t(key, { lng: language });

  const handleSubmit = async () => {
    if (!canUploadDocuments) {
      return;
    }

    try {
      const values = await form.validateFields();
      await createDocumentMutation.mutateAsync({ documentName: values.documentName });
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
              background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)",
              color: "#fff",
            }}
          >
            <UploadOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>
              {tr("documents.upload_page.title")}
            </Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {tr("documents.upload_page.description")}
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
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

          <Flex gap={12} wrap={!isMobile} vertical={isMobile} align={isMobile ? "stretch" : "center"}>
            <Button
              type="primary"
              onClick={handleSubmit}
              loading={createDocumentMutation.isPending}
              disabled={!canUploadDocuments}
              block={isMobile}
            >
              {tr("documents.upload_page.actions.submit")}
            </Button>
            <Button onClick={() => navigate("/app/documents")} block={isMobile}>
              {tr("documents.upload_page.actions.cancel")}
            </Button>
          </Flex>
        </Form>
      </Card>
    </Space>
  );
}
