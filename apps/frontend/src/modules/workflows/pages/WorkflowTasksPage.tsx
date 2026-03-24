import { Card, Table, Typography, Space, Tag, Grid, Button, Descriptions, Divider, App, Steps } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { ArrowLeftOutlined, FolderOpenOutlined } from "@ant-design/icons";
import { useWorkflowTasks } from "../hooks/useWorkflowTasks";
import type { WorkflowTaskItem } from "../types/workflows";
import { downloadDocument } from "../../documents";
import { useWorkflowInstanceActions } from "../hooks/useWorkflowInstanceActions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import { useProjectDetail } from "../../users";
import { useNavigate, useParams } from "react-router-dom";

export function WorkflowTasksPage() {
  const { t } = useTranslation();
  const { notification } = App.useApp();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const navigate = useNavigate();
  const { projectId } = useParams<{ projectId: string }>();
  const permissionState = usePermissions();
  const canReadProjects = permissionState.hasPermission(permissions.projects.read);
  const [paging, setPaging] = useState({ page: 1, pageSize: 10 });
  const workflowActions = useWorkflowInstanceActions();
  const [selectedTaskId, setSelectedTaskId] = useState<string | null>(null);
  const projectDetailQuery = useProjectDetail(projectId);
  const tasksQuery = useWorkflowTasks(
    { ...paging, projectId: projectId ?? undefined },
    Boolean(projectId),
  );

  const columns = useMemo<ColumnsType<WorkflowTaskItem>>(() => {
    const base: ColumnsType<WorkflowTaskItem> = [
      { title: t("workflow_tasks.columns.document"), dataIndex: "documentName", width: 240, ellipsis: true },
      { title: t("workflow_tasks.columns.step"), dataIndex: "stepName", width: 200, ellipsis: true },
      {
        title: t("workflow_tasks.columns.type"),
        dataIndex: "stepType",
        width: 160,
        render: (value) => <Tag>{t(`workflow_definitions.steps.types.${value}`)}</Tag>,
      },
      { title: t("workflow_tasks.columns.role"), dataIndex: "roleName", width: 180, ellipsis: true },
      {
        title: t("workflow_tasks.columns.status"),
        dataIndex: "status",
        width: 140,
        render: (value) => <Tag>{value}</Tag>,
      },
      { title: t("workflow_tasks.columns.due"), dataIndex: "dueAt", width: 160 },
    ];

    if (!projectId) {
      base.unshift({ title: t("workflow_tasks.columns.project"), dataIndex: "projectName", width: 200, ellipsis: true });
    }

    return base;
  }, [projectId, t]);

  const tasks = tasksQuery.data?.items ?? [];
  const selectedTask = tasks.find((item) => item.workflowInstanceStepId === selectedTaskId) ?? null;
  const selectedProjectLabel = projectDetailQuery.data
    ? `${projectDetailQuery.data.code} - ${projectDetailQuery.data.name}`
    : null;

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Space style={{ width: "100%", justifyContent: "flex-start" }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate("/app/workspace")} block={isMobile}>
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
              {t("workflow_tasks.page_title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ marginTop: 4 }}>
              {t("workflow_tasks.page_description")}
            </Typography.Paragraph>
            {selectedProjectLabel ? (
              <Typography.Text type="secondary" style={{ display: "block", marginTop: 4 }}>
                {t("workflow_tasks.project_label")}: {selectedProjectLabel}
              </Typography.Text>
            ) : null}
          </div>
        </Space>
      </Card>

      {projectId ? (
        <Space direction="vertical" size={16} style={{ width: "100%" }}>
        <Card variant="borderless">
          <Table
          rowKey="workflowInstanceStepId"
            dataSource={tasks}
            loading={tasksQuery.isLoading}
            columns={columns}
            locale={{
              emptyText: (
                <Space direction="vertical" size={8} style={{ width: "100%" }}>
                  <Typography.Text>{t("workflow_tasks.empty_project")}</Typography.Text>
                  <Typography.Text type="secondary">
                    {t("workflow_tasks.empty_project_hint")}
                  </Typography.Text>
                  <Steps
                    size="small"
                    direction="vertical"
                    items={[
                      { title: t("workflow_tasks.empty_steps.select_workflow") },
                      { title: t("workflow_tasks.empty_steps.start_document") },
                      { title: t("workflow_tasks.empty_steps.wait_task") },
                    ]}
                  />
                  <Space wrap>
                    <Button onClick={() => navigate(`/app/projects/${projectId}/edit`)}>
                      {t("workflow_tasks.empty_steps.go_project")}
                    </Button>
                    <Button onClick={() => navigate("/app/documents")}>
                      {t("workflow_tasks.empty_steps.go_documents")}
                    </Button>
                  </Space>
                </Space>
              ),
            }}
            pagination={{
              current: tasksQuery.data?.page ?? paging.page,
              pageSize: tasksQuery.data?.pageSize ?? paging.pageSize,
              total: tasksQuery.data?.total ?? 0,
              showSizeChanger: true,
              pageSizeOptions: [10, 25, 50, 100],
              onChange: (page, pageSize) =>
                setPaging((current) => ({
                  page: pageSize === current.pageSize ? page : 1,
                  pageSize,
                })),
            }}
            scroll={{ x: "max-content" }}
            size={isMobile ? "small" : "middle"}
            onRow={(record) => ({
              onClick: () => setSelectedTaskId(record.workflowInstanceStepId),
            })}
            rowClassName={(record) => (record.workflowInstanceStepId === selectedTaskId ? "table-row-selected" : "")}
          />
        </Card>

        <Card variant="borderless">
          <Typography.Title level={4} style={{ marginTop: 0 }}>
            {t("workflow_tasks.workspace.title")}
          </Typography.Title>
          <Typography.Paragraph type="secondary" style={{ marginTop: 0 }}>
            {t("workflow_tasks.workspace.description")}
          </Typography.Paragraph>

          {selectedTask ? (
            <>
              <Descriptions
                column={1}
                size="small"
                items={[
                  { label: t("workflow_tasks.columns.project"), children: selectedTask.projectName },
                  { label: t("workflow_tasks.columns.document"), children: selectedTask.documentName },
                  { label: t("workflow_tasks.columns.step"), children: selectedTask.stepName },
                  { label: t("workflow_tasks.columns.type"), children: t(`workflow_definitions.steps.types.${selectedTask.stepType}`) },
                  { label: t("workflow_tasks.columns.role"), children: selectedTask.roleName },
                ]}
              />
              <Divider style={{ margin: "12px 0" }} />
              <Space direction="vertical" size={8} style={{ width: "100%" }}>
                <Button
                  type="primary"
                  onClick={() => void downloadDocument(selectedTask.documentId)}
                  block={isMobile}
                >
                  {t("workflow_tasks.workspace.download")}
                </Button>
                <Button
                  disabled={!selectedTask.canAct || workflowActions.applyStepActionMutation.isPending}
                  onClick={async () => {
                    try {
                      await workflowActions.applyStepActionMutation.mutateAsync({
                        workflowInstanceId: selectedTask.workflowInstanceId,
                        workflowInstanceStepId: selectedTask.workflowInstanceStepId,
                        action: selectedTask.stepType,
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
          ) : (
            <Typography.Text type="secondary">
              {t("workflow_tasks.workspace.empty")}
            </Typography.Text>
          )}
        </Card>
      </Space>
      ) : (
        <Card variant="borderless">
          <Typography.Text type="secondary">
            {canReadProjects ? t("workflow_tasks.project_missing") : t("errors.title_forbidden")}
          </Typography.Text>
        </Card>
      )}
    </Space>
  );
}
