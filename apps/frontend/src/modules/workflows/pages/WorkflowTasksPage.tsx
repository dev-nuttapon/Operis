import { Card, Table, Typography, Space, Tag, Grid, Flex, Button, Descriptions, Divider, App } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useWorkflowTasks } from "../hooks/useWorkflowTasks";
import type { WorkflowTaskItem } from "../types/workflows";
import { downloadDocument } from "../../documents";
import { useWorkflowInstanceActions } from "../hooks/useWorkflowInstanceActions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";

export function WorkflowTasksPage() {
  const { t } = useTranslation();
  const { notification } = App.useApp();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const [paging, setPaging] = useState({ page: 1, pageSize: 10 });
  const tasksQuery = useWorkflowTasks(paging);
  const workflowActions = useWorkflowInstanceActions();
  const [selectedTaskId, setSelectedTaskId] = useState<string | null>(null);

  const columns = useMemo<ColumnsType<WorkflowTaskItem>>(
    () => [
      { title: t("workflow_tasks.columns.project"), dataIndex: "projectName", width: 200, ellipsis: true },
      { title: t("workflow_tasks.columns.document"), dataIndex: "documentName", width: 220, ellipsis: true },
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
    ],
    [t],
  );

  const tasks = tasksQuery.data?.items ?? [];
  const selectedTask = tasks.find((item) => item.workflowInstanceStepId === selectedTaskId) ?? null;

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Typography.Title level={3} style={{ margin: 0 }}>
          {t("workflow_tasks.page_title")}
        </Typography.Title>
        <Typography.Paragraph type="secondary" style={{ marginTop: 4 }}>
          {t("workflow_tasks.page_description")}
        </Typography.Paragraph>
      </Card>

      <Flex gap={16} vertical={isMobile} align="stretch">
        <Card variant="borderless" style={{ flex: 2, minWidth: 0 }}>
          <Table
          rowKey="workflowInstanceStepId"
            dataSource={tasks}
            loading={tasksQuery.isLoading}
            columns={columns}
            locale={{ emptyText: t("workflow_tasks.empty") }}
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

        <Card variant="borderless" style={{ flex: 1, minWidth: 280 }}>
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
      </Flex>
    </Space>
  );
}
