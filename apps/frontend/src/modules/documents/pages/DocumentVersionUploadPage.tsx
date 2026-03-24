import { useMemo, useRef, useState } from "react";
import { Alert, App, Button, Card, Form, Input, Space, Typography, Flex, Grid } from "antd";
import { ArrowLeftOutlined, UploadOutlined } from "@ant-design/icons";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import i18n from "../../../shared/i18n/config";
import { useI18nLanguage } from "../../../shared/i18n/hooks/useI18nLanguage";
import { useCreateDocumentVersion } from "../hooks/useDocuments";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import { ApiError, getApiErrorPresentation } from "../../../shared/lib/apiClient";

const { Title, Paragraph, Text } = Typography;
const allowedDocumentExtensions = [".pdf", ".doc", ".docx", ".xls", ".xlsx"] as const;

type LocationState = {
  documentName?: string;
  from?: string;
  mode?: "working";
};

export function DocumentVersionUploadPage() {
  const { notification } = App.useApp();
  const language = useI18nLanguage();
  const navigate = useNavigate();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const { documentId } = useParams<{ documentId: string }>();
  const location = useLocation();
  const locationState = location.state as LocationState | null;
  const backTarget = locationState?.from ?? "/app/documents";
  const permissionState = usePermissions();
  const canManageVersions = permissionState.hasPermission(permissions.documents.manageVersions);
  const createVersionMutation = useCreateDocumentVersion();
  const [form] = Form.useForm();
  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const tr = (key: string) => i18n.t(key, { lng: language });

  const documentLabel = useMemo(() => locationState?.documentName ?? documentId ?? "-", [locationState?.documentName, documentId]);
  const isWorkingMode = locationState?.mode === "working";

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
    if (!documentId) {
      return;
    }

    try {
      const values = await form.validateFields();
      if (!selectedFile) {
        notification.error({
          message: tr("documents.version_page.fields.file_required"),
          description: tr("documents.version_page.fields.file_required"),
        });
        return;
      }

      await createVersionMutation.mutateAsync({
        documentId,
        versionCode: values.versionCode,
        file: selectedFile,
      });

      notification.success({
        message: tr("documents.upload.success_title"),
        description: tr("documents.upload.success_description"),
      });
      navigate(backTarget);
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
      <Space style={{ width: "100%", justifyContent: "flex-start" }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate(backTarget)} block={isMobile}>
          {tr("documents.version_page.back_action")}
        </Button>
      </Space>

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
              {isWorkingMode ? tr("documents.version_page.working_title") : tr("documents.version_page.title")}
            </Title>
            <Text type="secondary">{documentLabel}</Text>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {isWorkingMode ? tr("documents.version_page.working_description") : tr("documents.version_page.description")}
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {!canManageVersions ? (
          <Alert type="info" showIcon message={tr("documents.read_only_title")} description={tr("documents.read_only_description")} style={{ marginBottom: 24 }} />
        ) : null}

        <Form form={form} layout="vertical" disabled={!canManageVersions}>
          <Form.Item label={tr("documents.version_page.fields.document_name")}>
            <Input value={documentLabel} disabled />
          </Form.Item>

          <Form.Item
            name="versionCode"
            label={tr("documents.version_page.fields.version_code")}
            rules={[{ required: true, message: tr("documents.version_page.fields.version_code_required") }]}
          >
            <Input placeholder={tr("documents.version_page.fields.version_code_placeholder")} />
          </Form.Item>

          <Form.Item label={tr("documents.version_page.fields.file")}>
            <Space direction="vertical" size={8} style={{ width: "100%" }}>
              <input
                ref={fileInputRef}
                type="file"
                accept={allowedDocumentExtensions.join(",")}
                style={{ display: "none" }}
                onChange={handleFileSelected}
              />
              <Button icon={<UploadOutlined />} onClick={handlePickFile} disabled={!canManageVersions} block={isMobile}>
                {tr("documents.version_page.actions.pick_file")}
              </Button>
              <Text type="secondary">
                {selectedFile ? selectedFile.name : tr("documents.version_page.fields.file_required")}
              </Text>
              <Text type="secondary">{tr("documents.upload.allowed_file_types")}</Text>
            </Space>
          </Form.Item>

          <Flex gap={12} wrap={!isMobile} vertical={isMobile} align={isMobile ? "stretch" : "center"}>
            <Button
              type="primary"
              onClick={handleSubmit}
              loading={createVersionMutation.isPending}
              disabled={!canManageVersions}
              block={isMobile}
            >
              {tr("documents.version_page.actions.submit")}
            </Button>
            <Button onClick={() => navigate(backTarget)} block={isMobile}>
              {tr("documents.version_page.actions.cancel")}
            </Button>
          </Flex>
        </Form>
      </Card>
    </Space>
  );
}
