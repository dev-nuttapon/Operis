import { App, Button, Card, Descriptions, Divider, Grid, Space, Tag, Typography, Select } from "antd";
import { ArrowLeftOutlined, FolderOpenOutlined } from "@ant-design/icons";
import { useMemo, useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useWorkflowTasks } from "../hooks/useWorkflowTasks";
import { useWorkflowInstanceActions } from "../hooks/useWorkflowInstanceActions";
import { downloadDocument } from "../../documents";
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

  const [selectedAction, setSelectedAction] = useState<string | null>(null);

  useEffect(() => {
    if (task?.stepType) {
      setSelectedAction(task.stepType);
    }
  }, [task?.stepType]);

  const actionOptions = useMemo(
    () => [
      { value: "submit", label: t("workflow_definitions.steps.types.submit") },
      { value: "peer_review", label: t("workflow_definitions.steps.types.peer_review") },
      { value: "review", label: t("workflow_definitions.steps.types.review") },
      { value: "approve", label: t("workflow_definitions.steps.types.approve") },
    ],
    [t],
  );

  const selectedProjectLabel = task ? `${task.projectName}` : null;

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
            <Space direction="vertical" size={8} style={{ width: "100%" }}>
              <Button onClick={() => navigate(`/app/documents/${task.documentId}/versions/new`)} block={isMobile}>
                {t("workflow_tasks.workspace.upload")}
              </Button>
              <Button type="primary" onClick={() => void downloadDocument(task.documentId)} block={isMobile}>
                {t("workflow_tasks.workspace.download")}
              </Button>
              <Select
                value={selectedAction ?? undefined}
                options={actionOptions}
                onChange={(value) => setSelectedAction(value)}
                placeholder={t("workflow_tasks.workspace.action_placeholder")}
                style={{ width: "100%" }}
                disabled={!task.canAct}
              />
              <Button
                disabled={!task.canAct || !selectedAction || workflowActions.applyStepActionMutation.isPending}
                onClick={async () => {
                  try {
                    await workflowActions.applyStepActionMutation.mutateAsync({
                      workflowInstanceId: task.workflowInstanceId,
                      workflowInstanceStepId: task.workflowInstanceStepId,
                      action: selectedAction,
                    });
                    notification.success({ message: t("workflow_tasks.workspace.submitted") });
                    await tasksQuery.refetch();
                  } catch (error) {
                    const presentation = getApiErrorPresentation(error, t("workflow_tasks.workspace.submit_failed"));
                    notification.error({ message: presentation.title, description: presentation.description });
                  }
                }}
                block={isMobile}
              >
                {t("workflow_tasks.workspace.submit")}
              </Button>
            </Space>
          </>
        )}
      </Card>
    </Space>
  );
}
