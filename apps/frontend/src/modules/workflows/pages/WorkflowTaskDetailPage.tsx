import { App, Button, Card, Descriptions, Divider, Grid, Space, Tag, Typography, Steps, Table } from "antd";
import type { ColumnsType } from "antd/es/table";
import { ArrowLeftOutlined, FolderOpenOutlined } from "@ant-design/icons";
import { useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useWorkflowTasks } from "../hooks/useWorkflowTasks";
import { useWorkflowInstance } from "../hooks/useWorkflowInstance";
import { useWorkflowInstanceActions } from "../hooks/useWorkflowInstanceActions";
import { downloadDocument, useDocumentVersions, type DocumentVersionListItem } from "../../documents";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";

export function WorkflowTaskDetailPage() {
  const { t } = useTranslation();
  const { notification } = App.useApp();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const navigate = useNavigate();
  const { projectId, workflowInstanceStepId } = useParams<{
    projectId: string;
    workflowInstanceStepId: string;
  }>();

  const workflowActions = useWorkflowInstanceActions();
  const tasksQuery = useWorkflowTasks(
    { page: 1, pageSize: 100, projectId: projectId ?? undefined },
    Boolean(projectId),
  );

  const task = useMemo(
    () => tasksQuery.data?.items.find((item) => item.workflowInstanceStepId === workflowInstanceStepId) ?? null,
    [tasksQuery.data?.items, workflowInstanceStepId],
  );
  const instanceQuery = useWorkflowInstance(task?.workflowInstanceId ?? null, Boolean(task?.workflowInstanceId));
  const [versionPaging, setVersionPaging] = useState({ page: 1, pageSize: 10 });
  const versionsQuery = useDocumentVersions(
    task?.documentId ?? null,
    versionPaging,
    Boolean(task?.documentId),
  );
  const [selectedSendId, setSelectedSendId] = useState<string | null>(null);

  const statusItems = useMemo(() => {
    if (!instanceQuery.data?.steps) {
      return [];
    }

    return [...instanceQuery.data.steps]
      .sort((a, b) => a.displayOrder - b.displayOrder)
      .map((step) => {
        const status = step.status.toLowerCase();
        const stepStatus =
          status === "completed" ? "finish" : status === "in_progress" ? "process" : "wait";
        return {
          title: `${t(`workflow_definitions.steps.types.${step.stepType}`)} (${step.status})`,
          description: step.isRequired ? t("workflow_tasks.status_timeline.required") : t("workflow_tasks.status_timeline.optional"),
          status: stepStatus as "finish" | "process" | "wait",
        };
      });
  }, [instanceQuery.data?.steps, t]);

  const selectedProjectLabel = task ? `${task.projectName}` : null;
  const canSend = Boolean(task?.canAct);

  const versionColumns: ColumnsType<DocumentVersionListItem> = useMemo(
    () => [
      {
        title: t("workflow_tasks.workspace.files.version_code"),
        dataIndex: "versionCode",
        key: "versionCode",
        align: "center",
      },
      {
        title: t("workflow_tasks.workspace.files.revision"),
        dataIndex: "revision",
        key: "revision",
        align: "center",
        render: (value: number) => `r${value}`,
      },
      {
        title: t("workflow_tasks.workspace.files.file_name"),
        dataIndex: "fileName",
        key: "fileName",
        ellipsis: true,
      },
      {
        title: t("workflow_tasks.workspace.files.uploaded_at"),
        dataIndex: "uploadedAt",
        key: "uploadedAt",
        render: (value: string) => new Date(value).toLocaleDateString(),
      },
      {
        title: t("workflow_tasks.workspace.files.status"),
        dataIndex: "isPublished",
        key: "status",
        align: "center",
        render: (value: boolean) =>
          value ? <Tag color="green">{t("workflow_tasks.workspace.files.published")}</Tag> : <Tag>{t("workflow_tasks.workspace.files.draft")}</Tag>,
      },
      {
        title: t("workflow_tasks.workspace.files.actions"),
        key: "actions",
        align: "center",
        render: (_, item) => (
          <Button
            type="primary"
            size="small"
            disabled={!canSend || workflowActions.applyStepActionMutation.isPending || (selectedSendId !== null && selectedSendId !== item.id)}
            onClick={async () => {
              if (!task) {
                return;
              }

              try {
                setSelectedSendId(item.id);
                await workflowActions.applyStepActionMutation.mutateAsync({
                  workflowInstanceId: task.workflowInstanceId,
                  workflowInstanceStepId: task.workflowInstanceStepId,
                  action: task.stepType,
                });
                notification.success({ message: t("workflow_tasks.workspace.submitted") });
                await Promise.all([tasksQuery.refetch(), instanceQuery.refetch(), versionsQuery.refetch()]);
              } catch (error) {
                const presentation = getApiErrorPresentation(error, t("workflow_tasks.workspace.submit_failed"));
                notification.error({ message: presentation.title, description: presentation.description });
              } finally {
                setSelectedSendId(null);
              }
            }}
          >
            {t("workflow_tasks.workspace.send_document")}
          </Button>
        ),
      },
    ],
    [
      canSend,
      instanceQuery,
      notification,
      selectedSendId,
      t,
      task,
      tasksQuery,
      versionsQuery,
      workflowActions.applyStepActionMutation,
    ],
  );

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Space style={{ width: "100%", justifyContent: "flex-start" }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate(`/app/workspace/${projectId}`)} block={isMobile}>
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
            <FolderOpenOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("workflow_tasks.workspace.title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ marginTop: 4 }}>
              {t("workflow_tasks.workspace.description")}
            </Typography.Paragraph>
            {selectedProjectLabel ? (
              <Typography.Text type="secondary" style={{ display: "block", marginTop: 4 }}>
                {t("workflow_tasks.project_label")}: {selectedProjectLabel}
              </Typography.Text>
            ) : null}
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {tasksQuery.isLoading ? (
          <Typography.Text type="secondary">{t("workflow_tasks.workspace.loading")}</Typography.Text>
        ) : !task ? (
          <Typography.Text type="secondary">{t("workflow_tasks.workspace.not_found")}</Typography.Text>
        ) : (
          <>
            <Descriptions
              column={1}
              size="small"
              items={[
                { label: t("workflow_tasks.columns.document"), children: task.documentName },
                { label: t("workflow_tasks.columns.step"), children: task.stepName },
                {
                  label: t("workflow_tasks.columns.type"),
                  children: <Tag>{t(`workflow_definitions.steps.types.${task.stepType}`)}</Tag>,
                },
                { label: t("workflow_tasks.columns.role"), children: task.roleName },
                { label: t("workflow_tasks.columns.status"), children: <Tag>{task.status}</Tag> },
              ]}
            />
            <Divider style={{ margin: "12px 0" }} />
            <Typography.Title level={5} style={{ marginTop: 0 }}>
              {t("workflow_tasks.status_timeline.title")}
            </Typography.Title>
            {instanceQuery.isLoading ? (
              <Typography.Text type="secondary">{t("workflow_tasks.status_timeline.loading")}</Typography.Text>
            ) : statusItems.length === 0 ? (
              <Typography.Text type="secondary">{t("workflow_tasks.status_timeline.empty")}</Typography.Text>
            ) : (
              <Steps size="small" direction="vertical" items={statusItems} />
            )}
            <Divider style={{ margin: "12px 0" }} />
            <Space direction="vertical" size={8} style={{ width: "100%" }}>
              <Button
                onClick={() =>
                  navigate(`/app/workspace/${projectId}/tasks/${workflowInstanceStepId}/upload`)
                }
                block={isMobile}
              >
                {t("workflow_tasks.workspace.upload")}
              </Button>
              <Button type="primary" onClick={() => void downloadDocument(task.documentId)} block={isMobile}>
                {t("workflow_tasks.workspace.download")}
              </Button>
            </Space>
            <Divider style={{ margin: "16px 0" }} />
            <Typography.Title level={5} style={{ marginTop: 0 }}>
              {t("workflow_tasks.workspace.uploaded_documents")}
            </Typography.Title>
            <Table<DocumentVersionListItem>
              rowKey="id"
              loading={versionsQuery.isLoading}
              columns={versionColumns}
              dataSource={versionsQuery.data?.items ?? []}
              pagination={{
                current: versionsQuery.data?.page ?? versionPaging.page,
                pageSize: versionsQuery.data?.pageSize ?? versionPaging.pageSize,
                total: versionsQuery.data?.total ?? 0,
                showSizeChanger: true,
                pageSizeOptions: [10, 25, 50, 100],
                onChange: (page, pageSize) => setVersionPaging({ page, pageSize }),
              }}
              scroll={{ x: "max-content" }}
              locale={{
                emptyText: versionsQuery.isError ? t("workflow_tasks.workspace.files.load_failed") : t("workflow_tasks.workspace.files.empty"),
              }}
            />
          </>
        )}
      </Card>
    </Space>
  );
}
