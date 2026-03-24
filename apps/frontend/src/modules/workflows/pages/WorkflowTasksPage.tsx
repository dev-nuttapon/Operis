import { Card, Table, Typography, Space, Tag, Grid, Button, Descriptions, Divider, App, Select } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { FolderOpenOutlined } from "@ant-design/icons";
import { useWorkflowTasks } from "../hooks/useWorkflowTasks";
import type { WorkflowTaskItem } from "../types/workflows";
import { downloadDocument, useDocumentOptions } from "../../documents";
import { useWorkflowInstanceActions } from "../hooks/useWorkflowInstanceActions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import { useProjectOptions } from "../../users";

export function WorkflowTasksPage() {
  const { t } = useTranslation();
  const { notification } = App.useApp();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const permissionState = usePermissions();
  const canManageProjects = permissionState.hasPermission(permissions.projects.manage);
  const canReadProjects = permissionState.hasPermission(permissions.projects.read);
  const canReadDocuments = permissionState.hasPermission(permissions.documents.read);
  const [paging, setPaging] = useState({ page: 1, pageSize: 10 });
  const workflowActions = useWorkflowInstanceActions();
  const [selectedTaskId, setSelectedTaskId] = useState<string | null>(null);
  const [selectedProjectId, setSelectedProjectId] = useState<string | null>(null);
  const [selectedDocumentId, setSelectedDocumentId] = useState<string | null>(null);
  const [startError, setStartError] = useState<string | null>(null);
  const projectOptionsState = useProjectOptions({ enabled: canReadProjects, assignedOnly: !canManageProjects });
  const documentOptions = useDocumentOptions(canReadDocuments);
  const tasksQuery = useWorkflowTasks(
    { ...paging, projectId: selectedProjectId ?? undefined },
    Boolean(selectedProjectId),
  );

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
  const selectedProjectLabel =
    selectedProjectId && projectOptionsState.itemsById?.get(selectedProjectId)
      ? `${projectOptionsState.itemsById.get(selectedProjectId)!.code} - ${
          projectOptionsState.itemsById.get(selectedProjectId)!.name
        }`
      : null;

  const handleStartWorkflow = async () => {
    if (!selectedProjectId || !selectedDocumentId) {
      setStartError(t("workflow_tasks.start.validation_required"));
      return;
    }
    setStartError(null);
    try {
      await workflowActions.createInstanceMutation.mutateAsync({
        projectId: selectedProjectId,
        documentId: selectedDocumentId,
      });
      notification.success({ message: t("workflow_tasks.start.started") });
      setSelectedProjectId(null);
      setSelectedDocumentId(null);
      await tasksQuery.refetch();
    } catch (error) {
      const presentation = getApiErrorPresentation(error, t("workflow_tasks.start.failed"));
      notification.error({ message: presentation.title, description: presentation.description });
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
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
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Typography.Title level={4} style={{ marginTop: 0 }}>
          {t("workflow_tasks.select_project.title")}
        </Typography.Title>
        <Typography.Paragraph type="secondary" style={{ marginTop: 0 }}>
          {t("workflow_tasks.select_project.description")}
        </Typography.Paragraph>
        <Select
          showSearch
          allowClear
          placeholder={t("workflow_tasks.select_project.placeholder")}
          value={selectedProjectId}
          options={projectOptionsState.options}
          onSearch={projectOptionsState.onSearch}
          onChange={(value) => {
            setSelectedProjectId(value ?? null);
            setSelectedDocumentId(null);
            setSelectedTaskId(null);
            setStartError(null);
            setPaging((current) => ({ ...current, page: 1 }));
          }}
          loading={projectOptionsState.loading}
          filterOption={false}
          optionRender={(option) => (
            <span style={{ display: "block", whiteSpace: "normal" }}>{option.label}</span>
          )}
          dropdownRender={(menu) => (
            <>
              {menu}
              {projectOptionsState.hasMore ? (
                <div style={{ padding: 8 }}>
                  <button
                    type="button"
                    onMouseDown={(event) => event.preventDefault()}
                    onClick={() => projectOptionsState.onLoadMore?.()}
                    style={{
                      width: "100%",
                      border: "none",
                      background: "transparent",
                      color: "#1677ff",
                      cursor: "pointer",
                      padding: 4,
                    }}
                  >
                    {t("workflow_tasks.start.load_more_projects")}
                  </button>
                </div>
              ) : null}
            </>
          )}
        />
      </Card>

      {!selectedProjectId ? (
        <Card variant="borderless">
          <Typography.Text type="secondary">{t("workflow_tasks.select_project.empty")}</Typography.Text>
        </Card>
      ) : null}

      {selectedProjectId ? (
        <Space direction="vertical" size={16} style={{ width: "100%" }}>
        <Card variant="borderless">
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

        <Card variant="borderless">
          <Typography.Title level={4} style={{ marginTop: 0 }}>
            {t("workflow_tasks.workspace.title")}
          </Typography.Title>
          <Typography.Paragraph type="secondary" style={{ marginTop: 0 }}>
            {t("workflow_tasks.workspace.description")}
          </Typography.Paragraph>

          {canManageProjects ? (
            <>
              <Typography.Text strong>{t("workflow_tasks.start.title")}</Typography.Text>
              {selectedProjectLabel ? (
                <Typography.Paragraph style={{ marginTop: 4 }}>
                  {t("workflow_tasks.start.project_label")}: {selectedProjectLabel}
                </Typography.Paragraph>
              ) : null}
              <Space direction="vertical" size={8} style={{ width: "100%", marginTop: 8 }}>
                <Select
                  showSearch
                  allowClear
                  placeholder={
                    selectedProjectId
                      ? t("workflow_tasks.start.document_placeholder")
                      : t("workflow_tasks.start.document_disabled_placeholder")
                  }
                  value={selectedDocumentId}
                  options={selectedProjectId ? documentOptions.options : []}
                  onSearch={(value) => {
                    if (selectedProjectId) {
                      documentOptions.onSearch(value);
                    }
                  }}
                  onChange={(value) => setSelectedDocumentId(value ?? null)}
                  loading={selectedProjectId ? documentOptions.loading : false}
                  disabled={!selectedProjectId}
                  filterOption={false}
                  optionRender={(option) => (
                    <span style={{ display: "block", whiteSpace: "normal" }}>{option.label}</span>
                  )}
                  dropdownRender={(menu) => (
                    <>
                      {menu}
                      {selectedProjectId && documentOptions.hasMore ? (
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
                            {t("workflow_tasks.start.load_more_documents")}
                          </button>
                        </div>
                      ) : null}
                    </>
                  )}
                />
                {startError ? <Typography.Text type="danger">{startError}</Typography.Text> : null}
                <Button
                  type="primary"
                  onClick={handleStartWorkflow}
                  disabled={!selectedProjectId || !selectedDocumentId}
                  loading={workflowActions.createInstanceMutation.isPending}
                >
                  {t("workflow_tasks.start.action")}
                </Button>
              </Space>
              <Divider style={{ margin: "16px 0" }} />
            </>
          ) : null}

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
      </Space>
      ) : null}
    </Space>
  );
}
