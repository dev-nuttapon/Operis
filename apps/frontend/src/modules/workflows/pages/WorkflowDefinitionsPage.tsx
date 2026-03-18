import { Alert, Card, Divider, Typography } from "antd";
import { useTranslation } from "react-i18next";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { WorkflowDefinitionCreateForm } from "../components/WorkflowDefinitionCreateForm";
import { WorkflowDefinitionFilters } from "../components/WorkflowDefinitionFilters";
import { WorkflowDefinitionList } from "../components/WorkflowDefinitionList";
import { useCreateWorkflowDefinition } from "../hooks/useCreateWorkflowDefinition";
import { useUpdateWorkflowDefinition } from "../hooks/useUpdateWorkflowDefinition";
import { useWorkflowDefinitionActions } from "../hooks/useWorkflowDefinitionActions";
import { useWorkflowDefinitionEditor } from "../hooks/useWorkflowDefinitionEditor";
import { useWorkflowDefinitionsScreen } from "../hooks/useWorkflowDefinitionsScreen";

const { Paragraph, Title } = Typography;

export function WorkflowDefinitionsPage() {
  const { t } = useTranslation();
  const permissionState = usePermissions();
  const canReadWorkflows = permissionState.hasPermission(permissions.workflows.read);
  const canManageDefinitions = permissionState.hasPermission(permissions.workflows.manageDefinitions);
  const {
    definitionsQuery,
    filteredDefinitions,
    setStatusFilter,
    statusFilter,
    statusSummary,
    paging,
    setPaging,
  } = useWorkflowDefinitionsScreen();
  const createDefinitionMutation = useCreateWorkflowDefinition();
  const updateDefinitionMutation = useUpdateWorkflowDefinition();
  const { activateMutation, archiveMutation } = useWorkflowDefinitionActions();
  const { editingWorkflowDefinitionId, startEditing, stopEditing } = useWorkflowDefinitionEditor();
  const createErrorPresentation = createDefinitionMutation.isError
    ? getApiErrorPresentation(createDefinitionMutation.error, t("workflow_definitions.notifications.create_failed_title"))
    : null;
  const actionErrorPresentation = updateDefinitionMutation.isError
    ? getApiErrorPresentation(updateDefinitionMutation.error, t("workflow_definitions.notifications.update_failed_title"))
    : activateMutation.isError
    ? getApiErrorPresentation(activateMutation.error, t("workflow_definitions.notifications.activate_failed_title"))
    : archiveMutation.isError
      ? getApiErrorPresentation(archiveMutation.error, t("workflow_definitions.notifications.archive_failed_title"))
      : null;

  return (
    <Card variant="borderless" style={{ borderRadius: 16 }}>
      <Title level={2} style={{ marginTop: 0 }}>
        {t("workflow_definitions.page_title")}
      </Title>
      <Paragraph type="secondary">
        {t("workflow_definitions.page_description")}
      </Paragraph>

      {!canReadWorkflows ? (
        <Alert
          type="warning"
          showIcon
          style={{ marginBottom: 16 }}
          message={t("errors.title_forbidden")}
        />
      ) : null}

      {createErrorPresentation ? (
        <Alert
          type="error"
          showIcon
          style={{ marginBottom: 16 }}
          message={createErrorPresentation.title}
          description={createErrorPresentation.description}
        />
      ) : null}

      {actionErrorPresentation ? (
        <Alert
          type="error"
          showIcon
          style={{ marginBottom: 16 }}
          message={actionErrorPresentation.title}
          description={actionErrorPresentation.description}
        />
      ) : null}

      {canReadWorkflows ? (
        <WorkflowDefinitionCreateForm
          canManage={canManageDefinitions}
          isSubmitting={createDefinitionMutation.isPending}
          onSubmit={(values) => createDefinitionMutation.mutate(values)}
        />
      ) : null}

      <Divider />

      {canReadWorkflows ? (
        <WorkflowDefinitionFilters
          selectedFilter={statusFilter}
          statusSummary={statusSummary}
          onSelectFilter={setStatusFilter}
        />
      ) : null}

      <Divider />

      {canReadWorkflows ? (
        <WorkflowDefinitionList
          canManage={canManageDefinitions}
          definitions={filteredDefinitions}
          isLoading={definitionsQuery.isLoading}
          isMutating={updateDefinitionMutation.isPending || activateMutation.isPending || archiveMutation.isPending}
          editingWorkflowDefinitionId={editingWorkflowDefinitionId}
          pagination={{
            page: definitionsQuery.data?.page ?? paging.page,
            pageSize: definitionsQuery.data?.pageSize ?? paging.pageSize,
            total: definitionsQuery.data?.total ?? 0,
          }}
          onPageChange={(page, pageSize) => {
            setPaging((current) => ({
              ...current,
              page: pageSize === current.pageSize ? page : 1,
              pageSize,
            }));
          }}
          onStartEdit={startEditing}
          onCancelEdit={stopEditing}
          onUpdate={(workflowDefinitionId, name) => {
            updateDefinitionMutation.mutate(
              { workflowDefinitionId, name },
              { onSuccess: () => stopEditing() },
            );
          }}
          onActivate={(workflowDefinitionId) => activateMutation.mutate({ workflowDefinitionId })}
          onArchive={(workflowDefinitionId) => archiveMutation.mutate({ workflowDefinitionId })}
        />
      ) : null}
    </Card>
  );
}
