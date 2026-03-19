import { useMemo, useState } from "react";
import { Alert, Button, Card, Dropdown, Modal, Space, Table, Tag, Typography, Skeleton } from "antd";
import type { ColumnsType } from "antd/es/table";
import { ArrowLeftOutlined, DeleteOutlined, DownloadOutlined, MoreOutlined, UploadOutlined, CheckCircleOutlined, StopOutlined, FileTextOutlined, BranchesOutlined } from "@ant-design/icons";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import i18n from "../../../shared/i18n/config";
import { useI18nLanguage } from "../../../shared/i18n/hooks/useI18nLanguage";
import { downloadDocument } from "../api/documentsApi";
import { useDeleteDocumentVersion, useDocumentVersions, usePublishDocumentVersion, useUnpublishDocumentVersion } from "../hooks/useDocuments";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import type { DocumentVersionListItem } from "../api/documentsApi";
import { saveBlobAsFile } from "../utils/download";

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

  const [paging, setPaging] = useState({ page: 1, pageSize: 10 });
  const versionsQuery = useDocumentVersions(documentId ?? null, paging, canReadDocuments);
  const deleteVersionMutation = useDeleteDocumentVersion();
  const publishVersionMutation = usePublishDocumentVersion();
  const unpublishVersionMutation = useUnpublishDocumentVersion();
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
      render: (_, item) => {
        const menuItems = [
          {
            key: "download",
            label: tr("documents.download_action"),
            icon: <DownloadOutlined />,
            onClick: () => {
              void downloadDocument(item.documentId)
                .then(({ blob, fileName }) => saveBlobAsFile(blob, fileName ?? item.fileName ?? "document"))
                .catch(() => null);
            },
          },
          {
            key: "publish",
            label: item.isPublished ? tr("documents.version_actions.unpublish.label") : tr("documents.version_actions.publish.label"),
            icon: item.isPublished ? <StopOutlined /> : <CheckCircleOutlined />,
            disabled: !canManageVersions,
            onClick: () => {
              if (!documentId) {
                return;
              }
              if (item.isPublished) {
                Modal.confirm({
                  title: tr("documents.version_actions.unpublish.confirm"),
                  okText: tr("documents.version_actions.unpublish.confirm_ok"),
                  cancelText: tr("documents.version_actions.unpublish.confirm_cancel"),
                  okButtonProps: { danger: true },
                  onOk: () => unpublishVersionMutation.mutateAsync({ documentId }),
                });
                return;
              }

              Modal.confirm({
                title: tr("documents.version_actions.publish.confirm"),
                okText: tr("documents.version_actions.publish.confirm_ok"),
                cancelText: tr("documents.version_actions.publish.confirm_cancel"),
                onOk: () => publishVersionMutation.mutateAsync({ documentId, versionId: item.id }),
              });
            },
          },
          {
            key: "delete",
            label: tr("documents.version_actions.delete.label"),
            icon: <DeleteOutlined />,
            danger: true,
            disabled: !canManageVersions,
            onClick: () => {
              if (!documentId) {
                return;
              }
              Modal.confirm({
                title: tr("documents.version_actions.delete.confirm"),
                okText: tr("documents.version_actions.delete.confirm_ok"),
                cancelText: tr("documents.version_actions.delete.confirm_cancel"),
                okButtonProps: { danger: true },
                onOk: () => deleteVersionMutation.mutateAsync({ documentId, versionId: item.id }),
              });
            },
          },
        ];

        return (
          <Dropdown menu={{ items: menuItems }} trigger={["click"]}>
            <Button
              size="small"
              icon={<MoreOutlined />}
              loading={deleteVersionMutation.isPending || publishVersionMutation.isPending || unpublishVersionMutation.isPending}
            />
          </Dropdown>
        );
      },
    },
  ];


  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Space style={{ width: "100%", justifyContent: "flex-start" }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate("/app/documents")}>
          {tr("documents.versions_page.back_action")}
        </Button>
      </Space>

      <Card variant="borderless">
        <Space style={{ width: "100%", justifyContent: "space-between" }} align="start">
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
              <BranchesOutlined />
            </div>
            <div>
              <Title level={3} style={{ margin: 0 }}>
                {tr("documents.versions_page.title")}
              </Title>
              <Text type="secondary">{documentLabel}</Text>
              <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
                {tr("documents.versions_page.description")}
              </Paragraph>
            </div>
          </Space>
        </Space>
      </Card>

      <Card variant="borderless">
        <Space style={{ width: "100%", justifyContent: "space-between", marginBottom: 16 }}>
          <Title level={4} style={{ margin: 0 }}>
            {tr("documents.versions_page.list_title")}
          </Title>
          <Space>
            {canManageVersions ? (
              <Button icon={<UploadOutlined />} onClick={() => navigate(`/app/documents/${documentId}/versions/new`, { state: { documentName: documentLabel } })}>
                {tr("documents.versions_page.actions.add_version")}
              </Button>
            ) : null}
            <Button
              icon={<FileTextOutlined />}
              onClick={() =>
                navigate(`/app/documents/${documentId}/history`, {
                  state: { documentName: documentLabel, from: `/app/documents/${documentId}/versions` },
                })
              }
            >
              {tr("documents.history_page.open_action")}
            </Button>
          </Space>
        </Space>
        {!canReadDocuments ? (
          <Alert type="info" showIcon message={tr("documents.read_only_title")} description={tr("documents.read_only_description")} style={{ marginBottom: 24 }} />
        ) : null}

        {versionsQuery.isLoading && (versionsQuery.data?.items?.length ?? 0) === 0 ? (
          <Skeleton active paragraph={{ rows: 6 }} />
        ) : (
          <Table<DocumentVersionListItem>
            rowKey="id"
            loading={versionsQuery.isLoading}
            columns={columns}
            dataSource={versionsQuery.data?.items ?? []}
            pagination={{
              current: versionsQuery.data?.page ?? paging.page,
              pageSize: versionsQuery.data?.pageSize ?? paging.pageSize,
              total: versionsQuery.data?.total ?? 0,
              showSizeChanger: true,
              pageSizeOptions: [10, 25, 50, 100],
              onChange: (page, pageSize) => setPaging({ page, pageSize }),
            }}
            scroll={{ x: "max-content" }}
            locale={{ emptyText: versionsQuery.isError ? tr("documents.load_failed") : tr("documents.empty") }}
          />
        )}
      </Card>

    </Space>
  );
}
