import { useQuery } from "@tanstack/react-query";
import { listWorkflowDefinitions } from "../api/workflowsApi";
import type { WorkflowDefinitionListInput } from "../types/workflows";

export function useWorkflowDefinitions(input: WorkflowDefinitionListInput, enabled = true) {
  return useQuery({
    queryKey: ["workflows", "definitions", input],
    enabled,
    queryFn: ({ signal }) => listWorkflowDefinitions(input, signal),
    staleTime: 30_000,
  });
}
