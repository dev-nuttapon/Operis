import { useState } from "react";
import { Typography, Card, Button, Space, Table, Alert, Dropdown, Modal, Form, Input, Tag, Skeleton } from "antd";
import { BranchesOutlined, DeleteOutlined, DownloadOutlined, UploadOutlined, MoreOutlined, FileTextOutlined } from "@ant-design/icons";
import type { ColumnsType } from "antd/es/table";
import { useNavigate } from "react-router-dom";
import i18n from "../../../shared/i18n/config";
import { useI18nLanguage } from "../../../shared/i18n/hooks/useI18nLanguage";
import { useDocumentDashboard } from "../hooks/useDocumentDashboard";
import { useDeleteDocument, useUpdateDocument } from "../hooks/useDocuments";
import { downloadDocument } from "../api/documentsApi";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import type { DocumentListItemView } from "../types/documents";
import { saveBlobAsFile } from "../utils/download";

const { Title, Paragraph } = Typography;
export function DocumentDashboardPage() {
  const language = useI18nLanguage();
  const navigate = useNavigate();
  const permissionState = usePermissions();
  const canReadDocuments = permissionState.hasPermission(permissions.documents.read);
  const [paging, setPaging] = useState({ page: 1, pageSize: 10 });
  const { documentsQuery } = useDocumentDashboard(canReadDocuments, paging.page, paging.pageSize);
  const tr = (key: string) => i18n.t(key, { lng: language });
  const [editForm] = Form.useForm();
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [deleteForm] = Form.useForm();
  const [editingDocument, setEditingDocument] = useState<DocumentListItemView | null>(null);
  const updateDocumentMutation = useUpdateDocument();
  const deleteDocumentMutation = useDeleteDocument();
  const canUploadDocuments = permissionState.hasPermission(permissions.documents.upload);
  const canManageVersions = permissionState.hasPermission(permissions.documents.manageVersions);
  const canPublishDocuments = permissionState.hasPermission(permissions.documents.publish);
  const canDeleteDrafts = permissionState.hasPermission(permissions.documents.deleteDraft);
  const canOperateDocuments = canUploadDocuments || canManageVersions || canPublishDocuments || canDeleteDrafts;
  const resolveDocumentStatus = (item: DocumentListItemView) => {
    const revisionCount = item.revision ?? 0;
    if (revisionCount === 0) {
      return { key: "empty", label: tr("documents.document_status.empty"), color: "default" as const };
    }
    if (item.publishedVersionCode) {
      return { key: "published", label: tr("documents.document_status.published"), color: "green" as const };
    }
    return { key: "unpublished", label: tr("documents.document_status.unpublished"), color: "gold" as const };
  };
  const latestDocumentColumns: ColumnsType<DocumentListItemView> = [
    {
      title: tr("documents.columns.document_name"),
      dataIndex: "documentName",
      key: "documentName",
      ellipsis: true,
      render: (value: string) => <span title={value}>{value}</span>,
    },
    {
      title: tr("documents.columns.created_at"),
      dataIndex: "uploadedAt",
      key: "createdAt",
      render: (value: string) => new Date(value).toLocaleDateString(language.startsWith("th") ? "th-TH" : "en-US"),
    },
    {
      title: tr("documents.columns.document_count"),
      dataIndex: "revision",
      key: "documentCount",
      align: "center",
      render: (value: number | null) => value ?? 0,
    },
    {
      title: tr("documents.columns.published_version"),
      dataIndex: "publishedVersionCode",
      key: "publishedVersion",
      align: "center",
      render: (value: string | null) => value ?? "-",
    },
    {
      title: tr("documents.columns.published_revision"),
      dataIndex: "publishedRevision",
      key: "publishedRevision",
      align: "center",
      render: (value: number | null) => (value ? `r${value}` : "-"),
    },
    {
      title: tr("documents.columns.status"),
      key: "status",
      align: "center",
      render: (_, item) => {
        const status = resolveDocumentStatus(item);
        return <Tag color={status.color}>{status.label}</Tag>;
      },
    },
    {
      title: tr("documents.columns.actions"),
      key: "actions",
      align: "center",
      render: (_, item) => {
        const hasFile = Boolean(item.fileName?.trim()) && item.sizeBytes > 0;
        const hasPublished = Boolean(item.publishedVersionCode);
        const menuItems = [
          {
            key: "edit",
            label: tr("documents.actions.edit.label"),
            icon: <UploadOutlined />,
            disabled: !canUploadDocuments,
            onClick: () => {
              setEditingDocument(item);
              editForm.setFieldsValue({ documentName: item.documentName });
              setEditModalOpen(true);
            },
          },
          {
            key: "delete",
            label: tr("documents.actions.delete.label"),
            icon: <DeleteOutlined />,
            disabled: !canDeleteDrafts,
            onClick: () => {
              setEditingDocument(item);
              deleteForm.resetFields();
              setDeleteModalOpen(true);
            },
          },
          {
            key: "download",
            label: tr("documents.actions.download.label"),
            icon: <DownloadOutlined />,
            disabled: !hasFile || !hasPublished,
            onClick: () => {
              if (!hasFile || !hasPublished) {
                return;
              }
              void downloadDocument(item.id)
                .then(({ blob, fileName }) => saveBlobAsFile(blob, fileName ?? item.fileName ?? "document"))
                .catch(() => null);
            },
          },
          {
            key: "upload",
            label: tr("documents.actions.upload_file.label"),
            icon: <UploadOutlined />,
            disabled: !canManageVersions,
            onClick: () => navigate(`/app/documents/${item.id}/versions/new`, { state: { documentName: item.documentName } }),
          },
          {
            key: "publish",
            label: tr("documents.actions.publish_list.label"),
            icon: <BranchesOutlined />,
            disabled: !canPublishDocuments,
            onClick: () => navigate(`/app/documents/${item.id}/versions`, { state: { documentName: item.documentName } }),
          },
          {
            key: "history",
            label: tr("documents.actions.history.label"),
            icon: <FileTextOutlined />,
            disabled: !canReadDocuments,
            onClick: () => navigate(`/app/documents/${item.id}/history`, { state: { documentName: item.documentName, from: "/app/documents" } }),
          },
        ];
        return (
          <Dropdown menu={{ items: menuItems }} trigger={["click"]}>
            <Button size="small" icon={<MoreOutlined />} />
          </Dropdown>
        );
      },
    },
  ];

  const handleEditSubmit = async () => {
    const values = await editForm.validateFields();
    if (!editingDocument) {
      return;
    }
    await updateDocumentMutation.mutateAsync({ documentId: editingDocument.id, documentName: values.documentName });
    setEditModalOpen(false);
    setEditingDocument(null);
  };

  const handleDeleteSubmit = async () => {
    const values = await deleteForm.validateFields();
    if (!editingDocument) {
      return;
    }
    await deleteDocumentMutation.mutateAsync({ documentId: editingDocument.id, reason: values.reason });
    setDeleteModalOpen(false);
    setEditingDocument(null);
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
            <FileTextOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>
              {tr("documents.page_title")}
            </Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {tr("documents.welcome")}
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {!canOperateDocuments ? (
          <Alert type="info" showIcon message={tr("documents.read_only_title")} description={tr("documents.read_only_description")} style={{ marginBottom: 24 }} />
        ) : (
          <Space style={{ width: "100%", justifyContent: "flex-end", marginBottom: 16 }}>
            <Button
              type="primary"
              icon={<UploadOutlined />}
              disabled={!canUploadDocuments}
              onClick={() => navigate("/app/documents/upload")}
            >
              {tr("documents.upload.action")}
            </Button>
          </Space>
        )}

        <Title level={4} style={{ marginTop: 0 }}>
          {tr("documents.list_title")}
        </Title>
        {documentsQuery.isLoading && (documentsQuery.data?.items?.length ?? 0) === 0 ? (
          <Skeleton active paragraph={{ rows: 6 }} />
        ) : (
          <Table<DocumentListItemView>
            rowKey="id"
            loading={documentsQuery.isLoading}
            columns={latestDocumentColumns}
            dataSource={canReadDocuments ? (documentsQuery.data?.items ?? []) : []}
            pagination={{
              current: documentsQuery.data?.page ?? paging.page,
              pageSize: documentsQuery.data?.pageSize ?? paging.pageSize,
              total: documentsQuery.data?.total ?? 0,
              showSizeChanger: true,
              pageSizeOptions: [10, 25, 50, 100],
              onChange: (page, pageSize) => setPaging({ page, pageSize }),
            }}
            scroll={{ x: "max-content" }}
            locale={{ emptyText: !canReadDocuments ? tr("documents.read_only_title") : documentsQuery.isError ? tr("documents.load_failed") : tr("documents.empty") }}
            style={{ marginBottom: 24 }}
          />
        )}
      </Card>

      <Modal
        open={editModalOpen}
        title={tr("documents.actions.edit.title")}
        onCancel={() => {
          setEditModalOpen(false);
          setEditingDocument(null);
        }}
        onOk={() => void handleEditSubmit()}
        okText={tr("documents.actions.edit.submit")}
        cancelText={tr("documents.actions.edit.cancel")}
        confirmLoading={updateDocumentMutation.isPending}
      >
        <Form form={editForm} layout="vertical">
          <Form.Item
            name="documentName"
            label={tr("documents.actions.edit.field")}
            rules={[{ required: true, message: tr("documents.actions.edit.required") }]}
          >
            <Input />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        open={deleteModalOpen}
        title={tr("documents.actions.delete.confirm_title")}
        onCancel={() => {
          setDeleteModalOpen(false);
          setEditingDocument(null);
        }}
        onOk={() => void handleDeleteSubmit()}
        okText={tr("documents.actions.delete.confirm_ok")}
        cancelText={tr("documents.actions.delete.confirm_cancel")}
        okButtonProps={{ danger: true }}
        confirmLoading={deleteDocumentMutation.isPending}
      >
        <Form form={deleteForm} layout="vertical">
          <Form.Item
            name="reason"
            label={tr("documents.actions.delete.reason_label")}
            rules={[{ required: true, message: tr("documents.actions.delete.reason_required") }]}
          >
            <Input.TextArea rows={3} />
          </Form.Item>
          <Alert type="warning" message={tr("documents.actions.delete.confirm_description")} showIcon />
        </Form>
      </Modal>
    </Space>
  );
}
