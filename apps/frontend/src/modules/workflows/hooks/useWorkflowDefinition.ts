import { useQuery } from "@tanstack/react-query";
import { getWorkflowDefinition } from "../api/workflowsApi";

export function useWorkflowDefinition(workflowDefinitionId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["workflow-definition", workflowDefinitionId],
    enabled: enabled && Boolean(workflowDefinitionId),
    queryFn: ({ signal }) =>
      workflowDefinitionId ? getWorkflowDefinition(workflowDefinitionId, signal) : Promise.resolve(null),
    staleTime: 30_000,
  });
}
