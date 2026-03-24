import { useEffect, useMemo, useState } from "react";
import { App, Button, Card, Form, Input, Space, Typography, Alert, Table, Flex, Grid, Tooltip, Select, Modal, Tag } from "antd";
import type { ColumnsType } from "antd/es/table";
import { ArrowLeftOutlined, EditOutlined, SaveOutlined, DeleteOutlined, ExclamationCircleOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router-dom";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useDocumentOptions, useDocumentTemplate, useRefreshDocumentTemplateItemVersion, useUpdateDocumentTemplate } from "../hooks/useDocumentTemplates";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import type { DocumentListItemView } from "../types/documents";
import { useDocumentVersions } from "../hooks/useDocuments";
import type { DocumentTemplateItemDetail } from "../types/documentTemplates";

type TemplateFormValues = {
  name: string;
  documentIds: string[];
};

type SelectedDocumentRow = {
  id: string;
  documentName: string;
  documentVersionId?: string | null;
  fileName?: string | null;
  contentType?: string | null;
  sizeBytes?: number | null;
  publishedVersionCode?: string | null;
  publishedRevision?: number | null;
  latestPublishedVersionCode?: string | null;
  latestPublishedRevision?: number | null;
};

const toSizeLabel = (bytes?: number | null) => {
  if (!bytes || bytes <= 0) return "-";
  if (bytes < 1024) return `${bytes} B`;
  const kb = bytes / 1024;
  if (kb < 1024) return `${kb.toFixed(1)} KB`;
  const mb = kb / 1024;
  return `${mb.toFixed(1)} MB`;
};

const toFileExtension = (value?: string | null) => {
  if (!value) return "-";
  const lower = value.toLowerCase();
  if (lower.includes("pdf")) return "pdf";
  if (lower.includes("word")) return "docx";
  if (lower.includes("excel") || lower.includes("spreadsheet")) return "xls";
  if (lower.includes("powerpoint") || lower.includes("presentation")) return "pptx";
  if (lower.includes("text")) return "txt";
  return value.split("/").pop() ?? value;
};

const renderFileName = (value?: string | null) => {
  const display = value ?? "-";
  const short = display.length > 100 ? `${display.slice(0, 100)}...` : display;
  return (
    <Tooltip title={display} placement="topLeft">
      <span
        style={{
          display: "inline-block",
          maxWidth: 340,
          overflow: "hidden",
          textOverflow: "ellipsis",
          whiteSpace: "nowrap",
          verticalAlign: "bottom",
        }}
      >
        {short}
      </span>
    </Tooltip>
  );
};

export function DocumentTemplateEditPage() {
  const { t } = useTranslation();
  const { notification, modal } = App.useApp();
  const navigate = useNavigate();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const { templateId } = useParams<{ templateId: string }>();
  const permissionState = usePermissions();
  const canReadDocuments = permissionState.hasPermission(permissions.documents.read);
  const canUpdateTemplates = permissionState.hasPermission(permissions.documents.upload);
  const canManageVersions = permissionState.hasPermission(permissions.documents.manageVersions);
  const canRefreshVersions = canUpdateTemplates || canManageVersions;
  const [form] = Form.useForm<TemplateFormValues>();
  const [submitting, setSubmitting] = useState(false);
  const [selectedDocumentId, setSelectedDocumentId] = useState<string | null>(null);
  const [selectedRows, setSelectedRows] = useState<SelectedDocumentRow[]>([]);
  const [documentSelectionError, setDocumentSelectionError] = useState<string | null>(null);
  const [versionModalOpen, setVersionModalOpen] = useState(false);
  const [versionDocumentId, setVersionDocumentId] = useState<string | null>(null);
  const [selectedVersionId, setSelectedVersionId] = useState<string | null>(null);

  const documentOptions = useDocumentOptions(canReadDocuments);
  const templateQuery = useDocumentTemplate(templateId ?? null, Boolean(templateId));
  const updateTemplateMutation = useUpdateDocumentTemplate();
  const refreshItemMutation = useRefreshDocumentTemplateItemVersion();
  const versionsQuery = useDocumentVersions(
    versionDocumentId,
    { page: 1, pageSize: 200 },
    versionModalOpen && Boolean(versionDocumentId) && canReadDocuments,
  );
  const optionMetaById = useMemo(
    () => new Map(documentOptions.options.map((option) => [option.value, option.meta] as const)),
    [documentOptions.options],
  );

  useEffect(() => {
    if (!templateQuery.data || !templateId) return;
    form.setFieldsValue({
      name: templateQuery.data.name,
    });
    const items = (templateQuery.data.items ?? []) as DocumentTemplateItemDetail[];
    if (items.length === 0) {
      setSelectedRows([]);
      return;
    }
    setSelectedRows(
      items.map((item) => {
        const meta = optionMetaById.get(item.documentId);
        return {
          id: item.documentId,
          documentName: item.documentName ?? meta?.documentName ?? item.documentId,
          documentVersionId: item.documentVersionId ?? null,
          fileName: meta?.fileName ?? null,
          contentType: meta?.contentType ?? null,
          sizeBytes: meta?.sizeBytes ?? null,
          publishedVersionCode: item.versionCode ?? null,
          publishedRevision: item.revision ?? null,
          latestPublishedVersionCode: meta?.publishedVersionCode ?? null,
          latestPublishedRevision: meta?.publishedRevision ?? null,
        };
      }),
    );
  }, [form, optionMetaById, templateId, templateQuery.data]);

  const availableOptions = useMemo(() => {
    const selectedIds = new Set(selectedRows.map((row) => row.id));
    return documentOptions.options.filter((option) => !selectedIds.has(option.value));
  }, [documentOptions.options, selectedRows]);

  const handleAddDocument = () => {
    if (!selectedDocumentId) {
      return;
    }
    const item = optionMetaById.get(selectedDocumentId);
    if (!item) {
      return;
    }
    setSelectedRows((current) => {
      if (current.some((row) => row.id === item.id)) {
        return current;
      }
      const next = [
        ...current,
        {
          id: item.id,
          documentName: item.documentName,
          documentVersionId: null,
          fileName: item.fileName,
          contentType: item.contentType,
          sizeBytes: item.sizeBytes,
          publishedVersionCode: item.publishedVersionCode,
          publishedRevision: item.publishedRevision,
          latestPublishedVersionCode: item.publishedVersionCode,
          latestPublishedRevision: item.publishedRevision,
        },
      ];
      return next;
    });
    setSelectedDocumentId(null);
    setDocumentSelectionError(null);
  };

  const handleRemoveDocument = (id: string) => {
    modal.confirm({
      title: t("common.actions.confirm_delete"),
      content: t("documents.templates.messages.confirm_remove_document"),
      icon: <ExclamationCircleOutlined />,
      okText: t("common.actions.confirm_delete"),
      cancelText: t("common.actions.cancel"),
      okButtonProps: { danger: true },
      onOk: () =>
        setSelectedRows((current) => {
          const next = current.filter((row) => row.id !== id);
          return next;
        }),
    });
  };

  const handleUpdate = async () => {
    const values = await form.validateFields();
    if (!templateId) {
      return;
    }
    if (selectedRows.length === 0) {
      setDocumentSelectionError(t("documents.templates.validation.documents_required"));
      return;
    }
    setSubmitting(true);
    updateTemplateMutation.mutate(
      { templateId, payload: { ...values, documentIds: selectedRows.map((row) => row.id) } },
      {
        onSuccess: () => {
          notification.success({ message: t("documents.templates.messages.updated") });
          navigate("/app/document-templates");
        },
        onError: (error) => {
          const presentation = getApiErrorPresentation(error, t("documents.templates.messages.update_failed"));
          notification.error({ message: presentation.title, description: presentation.description });
        },
        onSettled: () => setSubmitting(false),
      },
    );
  };

  const handleOpenVersionPicker = (record: SelectedDocumentRow) => {
    setVersionDocumentId(record.id);
    setSelectedVersionId(record.documentVersionId ?? null);
    setVersionModalOpen(true);
  };

  const handleConfirmVersion = async () => {
    if (!templateId || !versionDocumentId || !selectedVersionId) {
      return;
    }
    try {
      await refreshItemMutation.mutateAsync({
        templateId,
        documentId: versionDocumentId,
        documentVersionId: selectedVersionId,
      });
      notification.success({ message: t("documents.templates.messages.updated") });
      setVersionModalOpen(false);
    } catch (error) {
      const presentation = getApiErrorPresentation(error, t("documents.templates.messages.update_failed"));
      notification.error({ message: presentation.title, description: presentation.description });
    }
  };

  const columns = useMemo<ColumnsType<SelectedDocumentRow>>(
    () => [
      { title: t("documents.columns.document_name"), dataIndex: "documentName" },
      { title: t("documents.columns.file_name"), dataIndex: "fileName", render: (value) => renderFileName(value) },
      { title: t("documents.columns.content_type"), dataIndex: "contentType", render: (value) => toFileExtension(value) },
      { title: t("documents.columns.size"), dataIndex: "sizeBytes", render: (value) => toSizeLabel(value) },
      { title: t("documents.columns.published_version"), dataIndex: "publishedVersionCode", render: (value) => value ?? "-" },
      {
        title: t("documents.columns.latest_published_version"),
        dataIndex: "latestPublishedVersionCode",
        render: (value) => value ?? "-",
      },
      {
        title: "",
        key: "remove",
        align: "center",
        render: (_, record) => (
          <Space size={4}>
            <Button
              type="text"
              size="small"
              disabled={!canRefreshVersions || refreshItemMutation.isPending}
              onClick={() => handleOpenVersionPicker(record)}
            >
              {t("documents.templates.actions.refresh_item")}
            </Button>
            <Button
              type="text"
              danger
              size="small"
              icon={<DeleteOutlined />}
              onClick={() => handleRemoveDocument(record.id)}
            >
              {t("common.actions.delete")}
            </Button>
          </Space>
        ),
      },
    ],
    [canUpdateTemplates, canRefreshVersions, handleRemoveDocument, refreshItemMutation.isPending, t],
  );

  const versionOptions = useMemo(
    () =>
      (versionsQuery.data?.items ?? []).map((version) => ({
        value: version.id,
        label: `${version.versionCode ?? "-"} (Rev ${version.revision ?? "-"})`,
        isPublished: version.isPublished,
      })),
    [versionsQuery.data?.items],
  );

  return (
      <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Space style={{ width: "100%", justifyContent: "flex-start" }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate("/app/document-templates")} block={isMobile}>
          {t("documents.templates.edit_page_back")}
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
            <EditOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("documents.templates.edit_page_title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("documents.templates.edit_page_description")}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {!canReadDocuments ? (
          <Alert type="warning" showIcon message={t("errors.title_forbidden")} />
        ) : templateQuery.isError ? (
          <Alert
            type="error"
            showIcon
            message={t("documents.templates.messages.load_failed")}
            description={getApiErrorPresentation(templateQuery.error, t("documents.templates.messages.load_failed")).description}
          />
        ) : templateQuery.isLoading && !templateQuery.data ? (
          <Alert type="info" showIcon message={t("documents.templates.messages.loading")} />
        ) : (
          <>
            <Form form={form} layout="vertical">
              <Form.Item
                name="name"
                label={t("documents.templates.fields.name")}
                rules={[{ required: true, message: t("documents.templates.validation.name_required") }]}
              >
                <Input placeholder={t("documents.templates.placeholders.name")} />
              </Form.Item>
              <Form.Item
                label={t("documents.templates.fields.documents")}
                required
                validateStatus={documentSelectionError ? "error" : undefined}
                help={documentSelectionError ?? undefined}
              >
                <Flex vertical gap={8} align="flex-start" style={{ width: "100%" }}>
                  <Select
                    allowClear
                    showSearch
                    filterOption={false}
                    value={selectedDocumentId}
                    options={availableOptions}
                    optionRender={(option) => (
                      <span style={{ display: "block", whiteSpace: "normal" }}>{option.label}</span>
                    )}
                    placeholder={t("documents.templates.placeholders.documents")}
                    onSearch={documentOptions.onSearch}
                    onChange={(value) => setSelectedDocumentId(value ?? null)}
                    loading={documentOptions.loading}
                    style={{ width: "100%" }}
                    dropdownRender={(menu) => (
                      <>
                        {menu}
                        {documentOptions.hasMore ? (
                          <div style={{ padding: 8 }}>
                            <button
                              type="button"
                              onMouseDown={(event) => event.preventDefault()}
                              onClick={() => documentOptions.onLoadMore?.()}
                              style={{
                                width: "100%",
                                border: "none",
                                background: "transparent",
                                color: "#1677ff",
                                cursor: "pointer",
                                padding: 4,
                              }}
                            >
                              {t("documents.templates.actions.load_more")}
                            </button>
                          </div>
                        ) : null}
                      </>
                    )}
                  />
                  <div style={{ width: isMobile ? "100%" : "auto" }}>
                    <Button onClick={handleAddDocument} disabled={!selectedDocumentId} type="primary" block={isMobile}>
                      {t("common.actions.add")}
                    </Button>
                  </div>
                </Flex>
              </Form.Item>
              <Table<SelectedDocumentRow>
                rowKey="id"
                pagination={false}
                dataSource={selectedRows}
                scroll={{ x: "max-content" }}
                columns={columns}
                locale={{ emptyText: t("documents.templates.empty_selection") }}
              />
            </Form>
            <Flex gap={12} wrap={!isMobile} vertical={isMobile} align={isMobile ? "stretch" : "center"} justify="flex-start">
              <Button
                type="primary"
                icon={<SaveOutlined />}
                disabled={!canUpdateTemplates}
                loading={submitting || updateTemplateMutation.isPending}
                onClick={() => void handleUpdate()}
                block={isMobile}
              >
                {t("documents.templates.edit_page_submit")}
              </Button>
            </Flex>
            <Modal
              open={versionModalOpen}
              title={t("documents.templates.actions.refresh_item_title")}
              okText={t("documents.templates.actions.refresh_item_confirm")}
              cancelText={t("common.actions.cancel")}
              onCancel={() => setVersionModalOpen(false)}
              onOk={() => void handleConfirmVersion()}
              okButtonProps={{
                disabled: !selectedVersionId || refreshItemMutation.isPending,
                loading: refreshItemMutation.isPending,
              }}
            >
              <Space direction="vertical" size={12} style={{ width: "100%" }}>
                <Typography.Text type="secondary">
                  {t("documents.templates.actions.refresh_item_description")}
                </Typography.Text>
                <Form layout="vertical">
                  <Form.Item label={t("documents.templates.fields.version")}>
                    <Select
                      showSearch
                      optionFilterProp="label"
                      placeholder={t("documents.templates.placeholders.version")}
                      value={selectedVersionId ?? undefined}
                      options={versionOptions}
                      loading={versionsQuery.isLoading}
                      onChange={(value) => setSelectedVersionId(value)}
                      optionRender={(option) => (
                        <Space size={8}>
                          <span>{option.label}</span>
                          {option.data.isPublished ? (
                            <Tag color="green">{t("documents.version_status.published")}</Tag>
                          ) : null}
                        </Space>
                      )}
                    />
                  </Form.Item>
                </Form>
              </Space>
            </Modal>
          </>
        )}
      </Card>
    </Space>
  );
}
