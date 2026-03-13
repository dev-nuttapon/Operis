import { useMutation, useQueryClient } from "@tanstack/react-query";
import { activateWorkflowDefinition, archiveWorkflowDefinition } from "../api/workflowsApi";
import type { WorkflowDefinitionActionInput } from "../types/workflows";

export function useWorkflowDefinitionActions() {
  const queryClient = useQueryClient();

  async function invalidateDefinitions() {
    await queryClient.invalidateQueries({ queryKey: ["workflows", "definitions"] });
  }

  const activateMutation = useMutation({
    mutationFn: ({ workflowDefinitionId }: WorkflowDefinitionActionInput) => activateWorkflowDefinition(workflowDefinitionId),
    onSuccess: invalidateDefinitions,
  });

  const archiveMutation = useMutation({
    mutationFn: ({ workflowDefinitionId }: WorkflowDefinitionActionInput) => archiveWorkflowDefinition(workflowDefinitionId),
    onSuccess: invalidateDefinitions,
  });

  return {
    activateMutation,
    archiveMutation,
  };
}
