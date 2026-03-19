import { useState } from "react";
import { App, Button, Card, Form, Input, Select, Space, Typography, Alert, Table, Flex, Grid } from "antd";
import { ArrowLeftOutlined, FileTextOutlined, SaveOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useCreateDocumentTemplate, useDocumentOptions } from "../hooks/useDocumentTemplates";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import type { DocumentListItemView } from "../types/documents";

type TemplateFormValues = {
  name: string;
  documentIds: string[];
};

type SelectedDocumentRow = {
  id: string;
  documentName: string;
  fileName?: string | null;
  contentType?: string | null;
  sizeBytes?: number | null;
  publishedVersionCode?: string | null;
  publishedRevision?: number | null;
};

const toSizeLabel = (bytes?: number | null) => {
  if (!bytes || bytes <= 0) return "-";
  if (bytes < 1024) return `${bytes} B`;
  const kb = bytes / 1024;
  if (kb < 1024) return `${kb.toFixed(1)} KB`;
  const mb = kb / 1024;
  return `${mb.toFixed(1)} MB`;
};

export function DocumentTemplateCreatePage() {
  const { t } = useTranslation();
  const { notification } = App.useApp();
  const navigate = useNavigate();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const permissionState = usePermissions();
  const canReadDocuments = permissionState.hasPermission(permissions.documents.read);
  const canCreateTemplates = permissionState.hasPermission(permissions.documents.upload);
  const [form] = Form.useForm<TemplateFormValues>();
  const [submitting, setSubmitting] = useState(false);
  const documentOptions = useDocumentOptions(canReadDocuments);
  const [selectedRows, setSelectedRows] = useState<SelectedDocumentRow[]>([]);
  const createTemplateMutation = useCreateDocumentTemplate();

  const handleCreate = async () => {
    const values = await form.validateFields();
    setSubmitting(true);
    createTemplateMutation.mutate(values, {
      onSuccess: () => {
        notification.success({ message: t("documents.templates.messages.created") });
        navigate("/app/document-templates");
      },
      onError: (error) => {
        const presentation = getApiErrorPresentation(error, t("documents.templates.messages.create_failed"));
        notification.error({ message: presentation.title, description: presentation.description });
      },
      onSettled: () => setSubmitting(false),
    });
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
                name="documentIds"
                label={t("documents.templates.fields.documents")}
                rules={[{ required: true, message: t("documents.templates.validation.documents_required") }]}
              >
                <Select
                  mode="multiple"
                  allowClear
                  showSearch
                  filterOption={false}
                  options={documentOptions.options}
                  placeholder={t("documents.templates.placeholders.documents")}
                  onSearch={documentOptions.onSearch}
                  loading={documentOptions.loading}
                  onChange={(values: string[]) => {
                    const optionsById = new Map(
                      documentOptions.options.map((option) => [option.value, option.meta] as const),
                    );
                    const selected = values
                      .map((id: string) => optionsById.get(id))
                      .filter((item): item is DocumentListItemView => Boolean(item));
                    setSelectedRows(
                      selected.map((item) => ({
                        id: item.id,
                        documentName: item.documentName,
                        fileName: item.fileName,
                        contentType: item.contentType,
                        sizeBytes: item.sizeBytes,
                        publishedVersionCode: item.publishedVersionCode,
                        publishedRevision: item.publishedRevision,
                      })),
                    );
                  }}
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
              </Form.Item>
              <Table<SelectedDocumentRow>
                rowKey="id"
                pagination={false}
                dataSource={selectedRows}
                scroll={{ x: "max-content" }}
                columns={[
                  { title: t("documents.columns.document_name"), dataIndex: "documentName" },
                  { title: t("documents.columns.file_name"), dataIndex: "fileName", render: (value) => value ?? "-" },
                  { title: t("documents.columns.content_type"), dataIndex: "contentType", render: (value) => value ?? "-" },
                  { title: t("documents.columns.size"), dataIndex: "sizeBytes", render: (value) => toSizeLabel(value) },
                  {
                    title: t("documents.columns.published_version"),
                    dataIndex: "publishedVersionCode",
                    render: (value) => value ?? "-",
                  },
                  {
                    title: t("documents.columns.published_revision"),
                    dataIndex: "publishedRevision",
                    render: (value) => (value ? `r${value}` : "-"),
                  },
                ]}
                locale={{ emptyText: t("documents.templates.empty_selection") }}
              />
            </Form>
            <Flex justify="flex-start">
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
            </Flex>
          </>
        )}
      </Card>
    </Space>
  );
}
