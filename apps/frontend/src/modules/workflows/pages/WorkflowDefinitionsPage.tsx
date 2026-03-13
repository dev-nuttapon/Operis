import { Alert, Card, Divider, Typography } from "antd";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { WorkflowDefinitionCreateForm } from "../components/WorkflowDefinitionCreateForm";
import { WorkflowDefinitionList } from "../components/WorkflowDefinitionList";
import { useCreateWorkflowDefinition } from "../hooks/useCreateWorkflowDefinition";
import { useUpdateWorkflowDefinition } from "../hooks/useUpdateWorkflowDefinition";
import { useWorkflowDefinitionActions } from "../hooks/useWorkflowDefinitionActions";
import { useWorkflowDefinitionEditor } from "../hooks/useWorkflowDefinitionEditor";
import { useWorkflowDefinitions } from "../hooks/useWorkflowDefinitions";

const { Paragraph, Title } = Typography;

export function WorkflowDefinitionsPage() {
  const definitionsQuery = useWorkflowDefinitions();
  const createDefinitionMutation = useCreateWorkflowDefinition();
  const updateDefinitionMutation = useUpdateWorkflowDefinition();
  const { activateMutation, archiveMutation } = useWorkflowDefinitionActions();
  const { editingWorkflowDefinitionId, startEditing, stopEditing } = useWorkflowDefinitionEditor();
  const createErrorPresentation = createDefinitionMutation.isError
    ? getApiErrorPresentation(createDefinitionMutation.error, "Unable to create workflow definition")
    : null;
  const actionErrorPresentation = updateDefinitionMutation.isError
    ? getApiErrorPresentation(updateDefinitionMutation.error, "Unable to update workflow definition")
    : activateMutation.isError
    ? getApiErrorPresentation(activateMutation.error, "Unable to activate workflow definition")
    : archiveMutation.isError
      ? getApiErrorPresentation(archiveMutation.error, "Unable to archive workflow definition")
      : null;

  return (
    <Card variant="borderless" style={{ borderRadius: 16 }}>
      <Title level={2} style={{ marginTop: 0 }}>
        Workflow Definitions
      </Title>
      <Paragraph type="secondary">
        Workflow management will live in this module as the feature surface grows.
      </Paragraph>

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

      <WorkflowDefinitionCreateForm
        isSubmitting={createDefinitionMutation.isPending}
        onSubmit={(values) => createDefinitionMutation.mutate(values)}
      />

      <Divider />

      <WorkflowDefinitionList
        definitions={definitionsQuery.data ?? []}
        isLoading={definitionsQuery.isLoading}
        isMutating={updateDefinitionMutation.isPending || activateMutation.isPending || archiveMutation.isPending}
        editingWorkflowDefinitionId={editingWorkflowDefinitionId}
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
    </Card>
  );
}
