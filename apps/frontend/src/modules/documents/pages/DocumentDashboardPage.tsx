import { useRef } from "react";
import { Typography, Card, Button, Space, Divider, List, Tag, App, Alert } from "antd";
import { LogoutOutlined, UploadOutlined, DownloadOutlined, CheckCircleOutlined, DeleteOutlined, RollbackOutlined, BranchesOutlined } from "@ant-design/icons";
import { useAuth } from "../../../modules/auth";
import i18n from "../../../shared/i18n/config";
import { useI18nLanguage } from "../../../shared/i18n/hooks/useI18nLanguage";
import { useDocumentDashboard } from "../hooks/useDocumentDashboard";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import { ApiError, getApiErrorPresentation } from "../../../shared/lib/apiClient";

const { Title, Paragraph } = Typography;
const allowedDocumentExtensions = [".pdf", ".doc", ".docx", ".xls", ".xlsx"] as const;

export function DocumentDashboardPage() {
  const { notification } = App.useApp();
  const { logout, isAuthenticated } = useAuth();
  const language = useI18nLanguage();
  const permissionState = usePermissions();
  const canReadDocuments = permissionState.hasPermission(permissions.documents.read);
  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const { documentsQuery, uploadDocumentMutation, latestDocuments } = useDocumentDashboard(canReadDocuments);
  const tr = (key: string) => i18n.t(key, { lng: language });
  const canUploadDocuments = permissionState.hasPermission(permissions.documents.upload);
  const canManageVersions = permissionState.hasPermission(permissions.documents.manageVersions);
  const canPublishDocuments = permissionState.hasPermission(permissions.documents.publish);
  const canDeleteDrafts = permissionState.hasPermission(permissions.documents.deleteDraft);
  const canDeactivateDocuments = permissionState.hasPermission(permissions.documents.deactivate);
  const canOperateDocuments = canUploadDocuments || canManageVersions || canPublishDocuments || canDeleteDrafts || canDeactivateDocuments;
  const acceptedFileTypes = allowedDocumentExtensions.join(",");

  const handleUploadClick = () => {
    fileInputRef.current?.click();
  };

  const handleFileSelected = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
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

    try {
      await uploadDocumentMutation.mutateAsync({ file });
      notification.success({
        message: tr("documents.upload.success_title"),
        description: i18n.t("documents.upload.success_description", { lng: language, fileName: file.name }),
      });
    } catch (error) {
      const presentation =
        error instanceof ApiError
          ? getApiErrorPresentation(error, tr("documents.upload.failed_title"))
          : getApiErrorPresentation(error, tr("documents.upload.failed_title"));

      notification.error({
        message: presentation.title,
        description: presentation.description,
      });
    } finally {
      event.target.value = "";
    }
  };

  return (
    <Card bordered={false} style={{ borderRadius: 16 }}>
      <Space style={{ width: "100%", justifyContent: "space-between", marginBottom: 16 }}>
        <Title level={2} style={{ margin: 0 }}>{tr("documents.page_title")}</Title>
        <div style={{ display: "none" }}>
          {canUploadDocuments && (
            <input ref={fileInputRef} type="file" accept={acceptedFileTypes} style={{ display: "none" }} onChange={(event) => void handleFileSelected(event)} />
          )}
        </div>
      </Space>
      <Paragraph>
        {tr("documents.welcome")}
      </Paragraph>

      <Divider />

      <Title level={4} style={{ marginTop: 0 }}>
        {tr("documents.operations_title")}
      </Title>
      <Paragraph type="secondary">
        {tr("documents.operations_description")}
      </Paragraph>
      <Paragraph type="secondary" style={{ marginTop: -8 }}>
        {tr("documents.upload.allowed_file_types")}
      </Paragraph>
      {!canOperateDocuments ? (
        <Alert type="info" showIcon message={tr("documents.read_only_title")} description={tr("documents.read_only_description")} style={{ marginBottom: 24 }} />
      ) : null}
      <Space wrap size={16} style={{ marginBottom: 24 }}>
        {canUploadDocuments ? (
          <Card size="small" title={tr("documents.actions.upload.title")} style={{ width: 280 }}>
            <Paragraph type="secondary">{tr("documents.actions.upload.description")}</Paragraph>
            <Button type="primary" icon={<UploadOutlined />} onClick={handleUploadClick} loading={uploadDocumentMutation.isPending}>
              {tr("documents.upload.action")}
            </Button>
          </Card>
        ) : null}
        {canManageVersions ? (
          <Card size="small" title={tr("documents.actions.version.title")} style={{ width: 280 }}>
            <Paragraph type="secondary">{tr("documents.actions.version.description")}</Paragraph>
            <Button icon={<BranchesOutlined />} disabled>
              {tr("documents.actions.version.button")}
            </Button>
          </Card>
        ) : null}
        {canPublishDocuments ? (
          <Card size="small" title={tr("documents.actions.publish.title")} style={{ width: 280 }}>
            <Paragraph type="secondary">{tr("documents.actions.publish.description")}</Paragraph>
            <Button icon={<CheckCircleOutlined />} disabled>
              {tr("documents.actions.publish.button")}
            </Button>
          </Card>
        ) : null}
        {canDeleteDrafts ? (
          <Card size="small" title={tr("documents.actions.delete_draft.title")} style={{ width: 280 }}>
            <Paragraph type="secondary">{tr("documents.actions.delete_draft.description")}</Paragraph>
            <Button danger icon={<DeleteOutlined />} disabled>
              {tr("documents.actions.delete_draft.button")}
            </Button>
          </Card>
        ) : null}
        {canDeactivateDocuments ? (
          <Card size="small" title={tr("documents.actions.deactivate.title")} style={{ width: 280 }}>
            <Paragraph type="secondary">{tr("documents.actions.deactivate.description")}</Paragraph>
            <Button icon={<RollbackOutlined />} disabled>
              {tr("documents.actions.deactivate.button")}
            </Button>
          </Card>
        ) : null}
      </Space>

      <Title level={4} style={{ marginTop: 0 }}>
        {tr("documents.latest_documents")}
      </Title>
      <List
        loading={documentsQuery.isLoading}
        dataSource={canReadDocuments ? latestDocuments : []}
        locale={{ emptyText: !canReadDocuments ? tr("documents.read_only_title") : documentsQuery.isError ? tr("documents.load_failed") : tr("documents.empty") }}
        renderItem={(item) => (
          <List.Item>
            <Space style={{ width: "100%", justifyContent: "space-between" }}>
              <span>{item.fileName}</span>
              <Space>
                <Tag>{item.contentType}</Tag>
                <Tag>{Math.ceil(item.sizeBytes / 1024)} KB</Tag>
                <Tag>{new Date(item.uploadedAt).toLocaleDateString(language.startsWith("th") ? "th-TH" : "en-US")}</Tag>
                <Button
                  size="small"
                  icon={<DownloadOutlined />}
                  onClick={() => {
                    window.open(`/api/v1/documents/${item.id}/download`, "_blank", "noopener,noreferrer");
                  }}
                >
                  {tr("documents.download_action")}
                </Button>
              </Space>
            </Space>
          </List.Item>
        )}
        style={{ marginBottom: 24 }}
      />
    </Card>
  );
}
