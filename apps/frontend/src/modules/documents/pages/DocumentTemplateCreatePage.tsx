import { useMemo, useState } from "react";
import { App, Button, Card, Form, Input, Select, Space, Typography, Alert, Table, Flex, Grid, Tooltip } from "antd";
import { ArrowLeftOutlined, DeleteOutlined, FileTextOutlined, SaveOutlined, ExclamationCircleOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useCreateDocumentTemplate, useDocumentOptions } from "../hooks/useDocumentTemplates";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";

type TemplateFormValues = {
  name: string;
};

type SelectedDocumentRow = {
  id: string;
  documentName: string;
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

export function DocumentTemplateCreatePage() {
  const { t } = useTranslation();
  const { notification, modal } = App.useApp();
  const navigate = useNavigate();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const permissionState = usePermissions();
  const canReadDocuments = permissionState.hasPermission(permissions.documents.read);
  const canCreateTemplates = permissionState.hasPermission(permissions.documents.upload);
  const [form] = Form.useForm<TemplateFormValues>();
  const [submitting, setSubmitting] = useState(false);
  const documentOptions = useDocumentOptions(canReadDocuments);
  const [selectedDocumentId, setSelectedDocumentId] = useState<string | null>(null);
  const [selectedRows, setSelectedRows] = useState<SelectedDocumentRow[]>([]);
  const [documentSelectionError, setDocumentSelectionError] = useState<string | null>(null);
  const createTemplateMutation = useCreateDocumentTemplate();

  const optionMetaById = useMemo(() => {
    return new Map(documentOptions.options.map((option) => [option.value, option.meta] as const));
  }, [documentOptions.options]);

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
      return [
        ...current,
        {
          id: item.id,
          documentName: item.documentName,
          fileName: item.fileName,
          contentType: item.contentType,
          sizeBytes: item.sizeBytes,
          publishedVersionCode: item.publishedVersionCode,
          publishedRevision: item.publishedRevision,
          latestPublishedVersionCode: item.publishedVersionCode,
          latestPublishedRevision: item.publishedRevision,
        },
      ];
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
      onOk: () => setSelectedRows((current) => current.filter((row) => row.id !== id)),
    });
  };

  const handleCreate = async () => {
    const values = await form.validateFields();
    if (selectedRows.length === 0) {
      setDocumentSelectionError(t("documents.templates.validation.documents_required"));
      return;
    }
    setSubmitting(true);
    createTemplateMutation.mutate(
      {
        name: values.name,
        documentIds: selectedRows.map((row) => row.id),
      },
      {
      onSuccess: () => {
        notification.success({ message: t("documents.templates.messages.created") });
        navigate("/app/document-templates");
      },
      onError: (error) => {
        const presentation = getApiErrorPresentation(error, t("documents.templates.messages.create_failed"));
        notification.error({ message: presentation.title, description: presentation.description });
      },
        onSettled: () => setSubmitting(false),
      },
    );
  };

  return (
      <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Space style={{ width: "100%", justifyContent: "flex-start" }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate("/app/document-templates")} block={isMobile}>
          {t("documents.templates.create_page_back")}
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
            <FileTextOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("documents.templates.create_page_title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("documents.templates.create_page_description")}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {!canReadDocuments ? (
          <Alert type="warning" showIcon message={t("errors.title_forbidden")} />
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
                size={isMobile ? "small" : "middle"}
                columns={[
                  { title: t("documents.columns.document_name"), dataIndex: "documentName" },
                  { title: t("documents.columns.file_name"), dataIndex: "fileName", render: (value) => renderFileName(value) },
                  { title: t("documents.columns.content_type"), dataIndex: "contentType", render: (value) => toFileExtension(value) },
                  { title: t("documents.columns.size"), dataIndex: "sizeBytes", render: (value) => toSizeLabel(value) },
                  {
                    title: t("documents.columns.published_version"),
                    dataIndex: "publishedVersionCode",
                    render: (value) => value ?? "-",
                  },
                  {
                    title: t("documents.columns.latest_published_version"),
                    dataIndex: "latestPublishedVersionCode",
                    render: (value) => value ?? "-",
                  },
                  {
                    title: t("admin_users.columns.actions"),
                    key: "actions",
                    render: (_, record) => (
                      <Button
                        type="text"
                        danger
                        icon={<DeleteOutlined />}
                        onClick={() => handleRemoveDocument(record.id)}
                      >
                        {t("common.actions.delete")}
                      </Button>
                    ),
                  },
                ]}
                locale={{ emptyText: t("documents.templates.empty_selection") }}
              />
            </Form>
            <Flex gap={12} wrap={!isMobile} vertical={isMobile} align={isMobile ? "stretch" : "center"} justify="flex-start" style={{ marginTop: 16 }}>
              <Button
                type="primary"
                icon={<SaveOutlined />}
                disabled={!canCreateTemplates}
                loading={submitting}
                onClick={() => void handleCreate()}
                block={isMobile}
              >
                {t("documents.templates.create_page_submit")}
              </Button>
              <Button onClick={() => navigate("/app/document-templates")} block={isMobile}>
                {t("common.actions.cancel")}
              </Button>
            </Flex>
          </>
        )}
      </Card>
    </Space>
  );
}
