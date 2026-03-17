import { Typography, Card, Button, Space, Divider, Table, Tag, Alert } from "antd";
import { UploadOutlined, DownloadOutlined, CheckCircleOutlined, DeleteOutlined, RollbackOutlined, BranchesOutlined } from "@ant-design/icons";
import type { ColumnsType } from "antd/es/table";
import { useNavigate } from "react-router-dom";
import i18n from "../../../shared/i18n/config";
import { useI18nLanguage } from "../../../shared/i18n/hooks/useI18nLanguage";
import { useDocumentDashboard } from "../hooks/useDocumentDashboard";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import type { DocumentListItemView } from "../types/documents";

const { Title, Paragraph } = Typography;
export function DocumentDashboardPage() {
  const language = useI18nLanguage();
  const navigate = useNavigate();
  const permissionState = usePermissions();
  const canReadDocuments = permissionState.hasPermission(permissions.documents.read);
  const { documentsQuery, latestDocuments } = useDocumentDashboard(canReadDocuments);
  const tr = (key: string) => i18n.t(key, { lng: language });
  const canUploadDocuments = permissionState.hasPermission(permissions.documents.upload);
  const canManageVersions = permissionState.hasPermission(permissions.documents.manageVersions);
  const canPublishDocuments = permissionState.hasPermission(permissions.documents.publish);
  const canDeleteDrafts = permissionState.hasPermission(permissions.documents.deleteDraft);
  const canDeactivateDocuments = permissionState.hasPermission(permissions.documents.deactivate);
  const canOperateDocuments = canUploadDocuments || canManageVersions || canPublishDocuments || canDeleteDrafts || canDeactivateDocuments;
  const latestDocumentColumns: ColumnsType<DocumentListItemView> = [
    {
      title: tr("documents.columns.document_name"),
      dataIndex: "documentName",
      key: "documentName",
      ellipsis: true,
      render: (value: string) => <span title={value}>{value}</span>,
    },
    {
      title: tr("documents.columns.file_name"),
      dataIndex: "fileName",
      key: "fileName",
      ellipsis: true,
      render: (value: string) => <span title={value}>{value}</span>,
    },
    {
      title: tr("documents.columns.content_type"),
      dataIndex: "contentType",
      key: "contentType",
      ellipsis: true,
      render: (value: string) => <Tag>{value}</Tag>,
    },
    {
      title: tr("documents.columns.size"),
      dataIndex: "sizeBytes",
      key: "sizeBytes",
      align: "right",
      render: (value: number) => `${Math.ceil(value / 1024)} KB`,
    },
    {
      title: tr("documents.columns.uploaded_at"),
      dataIndex: "uploadedAt",
      key: "uploadedAt",
      render: (value: string) => new Date(value).toLocaleDateString(language.startsWith("th") ? "th-TH" : "en-US"),
    },
    {
      title: tr("documents.columns.actions"),
      key: "actions",
      align: "center",
      render: (_, item) => (
        <Button
          size="small"
          icon={<DownloadOutlined />}
          onClick={() => {
            window.open(`/api/v1/documents/${item.id}/download`, "_blank", "noopener,noreferrer");
          }}
        >
          {tr("documents.download_action")}
        </Button>
      ),
    },
  ];

  return (
    <Card bordered={false} style={{ borderRadius: 16 }}>
      <Space style={{ width: "100%", justifyContent: "space-between", marginBottom: 16 }}>
        <Title level={2} style={{ margin: 0 }}>{tr("documents.page_title")}</Title>
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
            <Button type="primary" icon={<UploadOutlined />} onClick={() => navigate("/app/documents/upload")}>
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
      <Table<DocumentListItemView>
        rowKey="id"
        loading={documentsQuery.isLoading}
        columns={latestDocumentColumns}
        dataSource={canReadDocuments ? latestDocuments : []}
        pagination={false}
        scroll={{ x: "max-content" }}
        locale={{ emptyText: !canReadDocuments ? tr("documents.read_only_title") : documentsQuery.isError ? tr("documents.load_failed") : tr("documents.empty") }}
        style={{ marginBottom: 24 }}
      />
    </Card>
  );
}
