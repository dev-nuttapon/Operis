import { useState } from "react";
import { Typography, Card, Button, Space, Divider, Table, Tag, Alert, Dropdown, Modal, Form, Input } from "antd";
import { UploadOutlined, BranchesOutlined, CheckCircleOutlined, DeleteOutlined, RollbackOutlined, MoreOutlined } from "@ant-design/icons";
import type { ColumnsType } from "antd/es/table";
import { useNavigate } from "react-router-dom";
import i18n from "../../../shared/i18n/config";
import { useI18nLanguage } from "../../../shared/i18n/hooks/useI18nLanguage";
import { useDocumentDashboard } from "../hooks/useDocumentDashboard";
import { useDeleteDocument, useUpdateDocument } from "../hooks/useDocuments";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import type { DocumentListItemView } from "../types/documents";

const { Title, Paragraph } = Typography;
export function DocumentDashboardPage() {
  const language = useI18nLanguage();
  const navigate = useNavigate();
  const permissionState = usePermissions();
  const canReadDocuments = permissionState.hasPermission(permissions.documents.read);
  const { documentsQuery } = useDocumentDashboard(canReadDocuments);
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
      render: (value: string) => {
        const label = value?.trim() ? value : tr("documents.columns.no_file");
        return <span title={label}>{label}</span>;
      },
    },
    {
      title: tr("documents.columns.version_code"),
      dataIndex: "versionCode",
      key: "versionCode",
      align: "center",
      render: (value: string | null) => value ?? "-",
    },
    {
      title: tr("documents.columns.revision"),
      dataIndex: "revision",
      key: "revision",
      align: "center",
      render: (value: number | null) => (value ? `r${value}` : "-"),
    },
    {
      title: tr("documents.columns.content_type"),
      dataIndex: "contentType",
      key: "contentType",
      ellipsis: true,
      render: (value: string, record) => {
        const hasFile = Boolean(record.fileName?.trim()) && record.sizeBytes > 0;
        return hasFile ? <Tag>{value}</Tag> : "-";
      },
    },
    {
      title: tr("documents.columns.size"),
      dataIndex: "sizeBytes",
      key: "sizeBytes",
      align: "right",
      render: (value: number) => (value > 0 ? `${Math.ceil(value / 1024)} KB` : "-"),
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
        const hasFile = Boolean(item.fileName?.trim()) && item.sizeBytes > 0;
        const menuItems = [
          {
            key: "edit",
            label: tr("documents.actions.edit.label"),
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
            disabled: !hasFile,
            onClick: () => {
              if (!hasFile) {
                return;
              }
              window.open(`/api/v1/documents/${item.id}/download`, "_blank", "noopener,noreferrer");
            },
          },
          {
            key: "upload",
            label: tr("documents.actions.upload_file.label"),
            disabled: !canManageVersions,
            onClick: () => navigate(`/app/documents/${item.id}/versions/new`, { state: { documentName: item.documentName } }),
          },
          {
            key: "publish",
            label: tr("documents.actions.publish_list.label"),
            disabled: !canPublishDocuments,
            onClick: () => navigate(`/app/documents/${item.id}/versions`, { state: { documentName: item.documentName } }),
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
        dataSource={canReadDocuments ? (documentsQuery.data ?? []) : []}
        pagination={false}
        scroll={{ x: "max-content" }}
        locale={{ emptyText: !canReadDocuments ? tr("documents.read_only_title") : documentsQuery.isError ? tr("documents.load_failed") : tr("documents.empty") }}
        style={{ marginBottom: 24 }}
      />

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
    </Card>
  );
}
