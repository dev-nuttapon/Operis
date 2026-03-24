import { App, Button, Card, Form, Grid, Input, Space, Typography, Flex } from "antd";
import { ArrowLeftOutlined, UploadOutlined } from "@ant-design/icons";
import { useMemo, useRef, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useWorkflowTasks } from "../hooks/useWorkflowTasks";
import { useCreateDocumentVersion } from "../../documents";
import { ApiError, getApiErrorPresentation } from "../../../shared/lib/apiClient";

const { Title, Paragraph, Text } = Typography;
const allowedDocumentExtensions = [".pdf", ".doc", ".docx", ".xls", ".xlsx"] as const;

export function WorkflowTaskUploadPage() {
  const { t } = useTranslation();
  const { notification } = App.useApp();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const navigate = useNavigate();
  const { projectId, workflowInstanceStepId } = useParams<{
    projectId: string;
    workflowInstanceStepId: string;
  }>();

  const tasksQuery = useWorkflowTasks(
    { page: 1, pageSize: 200, projectId: projectId ?? undefined },
    Boolean(projectId),
  );

  const task = useMemo(
    () => tasksQuery.data?.items.find((item) => item.workflowInstanceStepId === workflowInstanceStepId) ?? null,
    [tasksQuery.data?.items, workflowInstanceStepId],
  );

  const createVersionMutation = useCreateDocumentVersion();
  const [form] = Form.useForm();
  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);

  const canUpload = Boolean(task?.canAct);
  const documentLabel = task?.documentName ?? "-";
  const backTarget = projectId ? `/app/workspace/${projectId}/tasks/${workflowInstanceStepId}` : "/app/workspace";

  const handlePickFile = () => {
    fileInputRef.current?.click();
  };

  const handleFileSelected = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0] ?? null;
    if (!file) {
      return;
    }

    const normalizedFileName = file.name.trim().toLowerCase();
    const isAllowedFileType = allowedDocumentExtensions.some((extension) => normalizedFileName.endsWith(extension));
    if (!isAllowedFileType) {
      notification.error({
        message: t("documents.upload.invalid_type_title"),
        description: t("documents.upload.invalid_type_description"),
      });
      event.target.value = "";
      return;
    }

    setSelectedFile(file);
  };

  const handleSubmit = async () => {
    if (!task?.documentId) {
      return;
    }

    try {
      const values = await form.validateFields();
      if (!selectedFile) {
        notification.error({
          message: t("documents.version_page.fields.file_required"),
          description: t("documents.version_page.fields.file_required"),
        });
        return;
      }

      await createVersionMutation.mutateAsync({
        documentId: task.documentId,
        versionCode: values.versionCode,
        file: selectedFile,
      });

      notification.success({
        message: t("documents.upload.success_title"),
        description: t("documents.upload.success_description"),
      });
      navigate(backTarget);
    } catch (error) {
      if (error && typeof error === "object" && "errorFields" in error) {
        return;
      }

      const presentation =
        error instanceof ApiError
          ? getApiErrorPresentation(error, t("documents.upload.failed_title"))
          : getApiErrorPresentation(error, t("documents.upload.failed_title"));

      notification.error({
        message: presentation.title,
        description: presentation.description,
      });
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Space style={{ width: "100%", justifyContent: "flex-start" }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate(backTarget)} block={isMobile}>
          {t("common.actions.back")}
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
            <UploadOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>
              {t("workflow_tasks.upload.title")}
            </Title>
            <Text type="secondary">{documentLabel}</Text>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("workflow_tasks.upload.description")}
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {tasksQuery.isLoading ? (
          <Text type="secondary">{t("workflow_tasks.workspace.loading")}</Text>
        ) : !task ? (
          <Text type="secondary">{t("workflow_tasks.workspace.not_found")}</Text>
        ) : (
          <Form form={form} layout="vertical" disabled={!canUpload}>
            <Form.Item label={t("documents.version_page.fields.document_name")}>
              <Input value={documentLabel} disabled />
            </Form.Item>

            <Form.Item
              name="versionCode"
              label={t("documents.version_page.fields.version_code")}
              rules={[{ required: true, message: t("documents.version_page.fields.version_code_required") }]}
            >
              <Input placeholder={t("documents.version_page.fields.version_code_placeholder")} />
            </Form.Item>

            <Form.Item label={t("documents.version_page.fields.file")}>
              <Space direction="vertical" size={8} style={{ width: "100%" }}>
                <input
                  ref={fileInputRef}
                  type="file"
                  accept={allowedDocumentExtensions.join(",")}
                  style={{ display: "none" }}
                  onChange={handleFileSelected}
                />
                <Button icon={<UploadOutlined />} onClick={handlePickFile} disabled={!canUpload} block={isMobile}>
                  {t("documents.version_page.actions.pick_file")}
                </Button>
                <Text type="secondary">
                  {selectedFile ? selectedFile.name : t("documents.version_page.fields.file_required")}
                </Text>
                <Text type="secondary">{t("documents.upload.allowed_file_types")}</Text>
              </Space>
            </Form.Item>

            <Flex gap={12} wrap={!isMobile} vertical={isMobile} align={isMobile ? "stretch" : "center"}>
              <Button
                type="primary"
                onClick={handleSubmit}
                loading={createVersionMutation.isPending}
                disabled={!canUpload}
                block={isMobile}
              >
                {t("documents.version_page.actions.submit")}
              </Button>
              <Button onClick={() => navigate(backTarget)} block={isMobile}>
                {t("documents.version_page.actions.cancel")}
              </Button>
            </Flex>
          </Form>
        )}
      </Card>
    </Space>
  );
}
