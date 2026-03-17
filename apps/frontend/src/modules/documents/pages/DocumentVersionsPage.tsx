import { useMemo } from "react";
import { Alert, Button, Card, Divider, Popconfirm, Space, Table, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { ArrowLeftOutlined, DeleteOutlined, DownloadOutlined, UploadOutlined } from "@ant-design/icons";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import i18n from "../../../shared/i18n/config";
import { useI18nLanguage } from "../../../shared/i18n/hooks/useI18nLanguage";
import { useDeleteDocumentVersion, useDocumentVersions } from "../hooks/useDocuments";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import type { DocumentVersionListItem } from "../api/documentsApi";

const { Title, Paragraph, Text } = Typography;

type LocationState = {
  documentName?: string;
};

export function DocumentVersionsPage() {
  const language = useI18nLanguage();
  const navigate = useNavigate();
  const { documentId } = useParams<{ documentId: string }>();
  const location = useLocation();
  const locationState = location.state as LocationState | null;
  const permissionState = usePermissions();
  const canReadDocuments = permissionState.hasPermission(permissions.documents.read);
  const canManageVersions = permissionState.hasPermission(permissions.documents.manageVersions);
  const tr = (key: string) => i18n.t(key, { lng: language });

  const versionsQuery = useDocumentVersions(documentId ?? null, canReadDocuments);
  const deleteVersionMutation = useDeleteDocumentVersion();
  const documentLabel = useMemo(() => locationState?.documentName ?? documentId ?? "-", [locationState?.documentName, documentId]);

  const columns: ColumnsType<DocumentVersionListItem> = [
    {
      title: tr("documents.columns.version_code"),
      dataIndex: "versionCode",
      key: "versionCode",
      align: "center",
    },
    {
      title: tr("documents.columns.revision"),
      dataIndex: "revision",
      key: "revision",
      align: "center",
      render: (value: number) => `r${value}`,
    },
    {
      title: tr("documents.columns.status"),
      dataIndex: "isPublished",
      key: "status",
      align: "center",
      render: (value: boolean) =>
        value
          ? <Tag color="green">{tr("documents.version_status.published")}</Tag>
          : <Tag>{tr("documents.version_status.draft")}</Tag>,
    },
    {
      title: tr("documents.columns.file_name"),
      dataIndex: "fileName",
      key: "fileName",
      ellipsis: true,
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
        <Space>
          <Button
            size="small"
            icon={<DownloadOutlined />}
            onClick={() => {
              window.open(`/api/v1/documents/${item.documentId}/download`, "_blank", "noopener,noreferrer");
            }}
          >
            {tr("documents.download_action")}
          </Button>
          {canManageVersions ? (
            <Popconfirm
              title={tr("documents.version_actions.delete.confirm")}
              okText={tr("documents.version_actions.delete.confirm_ok")}
              cancelText={tr("documents.version_actions.delete.confirm_cancel")}
              onConfirm={() => {
                if (!documentId) {
                  return;
                }
                void deleteVersionMutation.mutateAsync({ documentId, versionId: item.id });
              }}
            >
              <Button size="small" danger icon={<DeleteOutlined />} loading={deleteVersionMutation.isPending}>
                {tr("documents.version_actions.delete.label")}
              </Button>
            </Popconfirm>
          ) : null}
        </Space>
      ),
    },
  ];

  return (
    <Card bordered={false} style={{ borderRadius: 16 }}>
      <Space style={{ width: "100%", justifyContent: "space-between", marginBottom: 16 }}>
        <div>
          <Title level={2} style={{ margin: 0 }}>{tr("documents.versions_page.title")}</Title>
          <Text type="secondary">{documentLabel}</Text>
        </div>
        <Space>
          {canManageVersions ? (
            <Button icon={<UploadOutlined />} onClick={() => navigate(`/app/documents/${documentId}/versions/new`, { state: { documentName: documentLabel } })}>
              {tr("documents.versions_page.actions.add_version")}
            </Button>
          ) : null}
          <Button icon={<ArrowLeftOutlined />} onClick={() => navigate("/app/documents")}>
            {tr("documents.versions_page.back_action")}
          </Button>
        </Space>
      </Space>

      <Paragraph type="secondary">{tr("documents.versions_page.description")}</Paragraph>

      <Divider />

      {!canReadDocuments ? (
        <Alert type="info" showIcon message={tr("documents.read_only_title")} description={tr("documents.read_only_description")} style={{ marginBottom: 24 }} />
      ) : null}

      <Table<DocumentVersionListItem>
        rowKey="id"
        loading={versionsQuery.isLoading}
        columns={columns}
        dataSource={versionsQuery.data ?? []}
        pagination={false}
        scroll={{ x: "max-content" }}
        locale={{ emptyText: versionsQuery.isError ? tr("documents.load_failed") : tr("documents.empty") }}
      />
    </Card>
  );
}
