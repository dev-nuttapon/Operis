import { useEffect, useMemo, useState } from "react";
import { App, Button, Card, Form, Input, Space, Typography, Alert, Table, Transfer } from "antd";
import type { ColumnsType } from "antd/es/table";
import { ArrowLeftOutlined, EditOutlined, SaveOutlined, DeleteOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router-dom";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useDocumentOptions, useDocumentTemplate, useUpdateDocumentTemplate } from "../hooks/useDocumentTemplates";
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

export function DocumentTemplateEditPage() {
  const { t } = useTranslation();
  const { notification } = App.useApp();
  const navigate = useNavigate();
  const { templateId } = useParams<{ templateId: string }>();
  const permissionState = usePermissions();
  const canReadDocuments = permissionState.hasPermission(permissions.documents.read);
  const canUpdateTemplates = permissionState.hasPermission(permissions.documents.upload);
  const [form] = Form.useForm<TemplateFormValues>();
  const [submitting, setSubmitting] = useState(false);
  const [targetKeys, setTargetKeys] = useState<string[]>([]);

  const documentOptions = useDocumentOptions(canReadDocuments);
  const templateQuery = useDocumentTemplate(templateId ?? null, Boolean(templateId));
  const updateTemplateMutation = useUpdateDocumentTemplate();

  useEffect(() => {
    if (!templateQuery.data || !templateId) return;
    form.setFieldsValue({
      name: templateQuery.data.name,
      documentIds: templateQuery.data.documentIds,
    });
    setTargetKeys(templateQuery.data.documentIds.map((id) => id.toString()));

    const optionsById = new Map(
      documentOptions.options.map((option) => [option.value, option.meta] as const),
    );
    const resolved = templateQuery.data.documentIds
      .map((id: string) => optionsById.get(id))
      .filter((item): item is DocumentListItemView => Boolean(item));
    if (resolved.length === 0) return;
  }, [documentOptions.options, form, templateId, templateQuery.data]);

  const transferData = useMemo(
    () =>
      documentOptions.options.map((option) => ({
        key: option.value,
        title: option.label,
        description: option.meta.documentName,
        meta: option.meta,
      })),
    [documentOptions.options],
  );

  const selectedRows = useMemo<SelectedDocumentRow[]>(() => {
    if (targetKeys.length === 0) return [];
    const byId = new Map(transferData.map((item) => [item.key, item.meta] as const));
    return targetKeys.map((id) => {
      const item = byId.get(id);
      if (!item) {
        return { id, documentName: id };
      }
      return {
        id: item.id,
        documentName: item.documentName,
        fileName: item.fileName,
        contentType: item.contentType,
        sizeBytes: item.sizeBytes,
        publishedVersionCode: item.publishedVersionCode,
        publishedRevision: item.publishedRevision,
      };
    });
  }, [targetKeys, transferData]);

  const handleUpdate = async () => {
    const values = await form.validateFields();
    if (!templateId) {
      return;
    }
    setSubmitting(true);
    updateTemplateMutation.mutate(
      { templateId, payload: values },
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

  const columns = useMemo<ColumnsType<SelectedDocumentRow>>(
    () => [
      { title: t("documents.columns.document_name"), dataIndex: "documentName" },
      { title: t("documents.columns.file_name"), dataIndex: "fileName", render: (value) => value ?? "-" },
      { title: t("documents.columns.content_type"), dataIndex: "contentType", render: (value) => value ?? "-" },
      { title: t("documents.columns.size"), dataIndex: "sizeBytes", render: (value) => toSizeLabel(value) },
      { title: t("documents.columns.published_version"), dataIndex: "publishedVersionCode", render: (value) => value ?? "-" },
      { title: t("documents.columns.published_revision"), dataIndex: "publishedRevision", render: (value) => (value ? `r${value}` : "-") },
      {
        title: "",
        key: "remove",
        align: "center",
        render: (_, record) => (
          <Button
            size="small"
            icon={<DeleteOutlined />}
            onClick={() => {
              const nextKeys = targetKeys.filter((key) => key !== record.id);
              setTargetKeys(nextKeys);
              form.setFieldsValue({ documentIds: nextKeys });
            }}
          >
            {t("documents.templates.actions.remove_document")}
          </Button>
        ),
      },
    ],
    [form, t, targetKeys],
  );

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Space style={{ width: "100%", justifyContent: "flex-start" }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate("/app/document-templates")}>
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
                name="documentIds"
                rules={[{ required: true, message: t("documents.templates.validation.documents_required") }]}
                hidden
              >
                <Input />
              </Form.Item>
              <Form.Item
                label={t("documents.templates.fields.documents")}
              >
                <Transfer
                  dataSource={transferData}
                  titles={[t("documents.templates.actions.available_documents"), t("documents.templates.actions.selected_documents")]}
                  targetKeys={targetKeys}
                  onChange={(nextTargetKeys) => {
                    setTargetKeys(nextTargetKeys);
                    form.setFieldsValue({ documentIds: nextTargetKeys });
                  }}
                  render={(item) => item.title}
                  listStyle={{ width: 260, height: 320 }}
                  showSearch
                  onSearch={(direction, value) => {
                    if (direction === "left") {
                      documentOptions.onSearch(value);
                    }
                  }}
                  footer={(props) =>
                    props.direction === "left" && documentOptions.hasMore ? (
                      <Button type="link" onClick={() => documentOptions.onLoadMore?.()}>
                        {t("documents.templates.actions.load_more")}
                      </Button>
                    ) : null
                  }
                />
              </Form.Item>
              <Table<SelectedDocumentRow>
                rowKey="id"
                pagination={false}
                dataSource={selectedRows}
                columns={columns}
                locale={{ emptyText: t("documents.templates.empty_selection") }}
              />
            </Form>
            <Space style={{ width: "100%", justifyContent: "flex-end" }}>
              <Button
                type="primary"
                icon={<SaveOutlined />}
                disabled={!canUpdateTemplates}
                loading={submitting || updateTemplateMutation.isPending}
                onClick={() => void handleUpdate()}
              >
                {t("documents.templates.edit_page_submit")}
              </Button>
            </Space>
          </>
        )}
      </Card>
    </Space>
  );
}
