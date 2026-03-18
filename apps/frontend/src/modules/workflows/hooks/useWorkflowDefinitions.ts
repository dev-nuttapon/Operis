import { useQuery } from "@tanstack/react-query";
import { listWorkflowDefinitions } from "../api/workflowsApi";
import type { WorkflowDefinitionListInput } from "../types/workflows";

export function useWorkflowDefinitions(input: WorkflowDefinitionListInput) {
  return useQuery({
    queryKey: ["workflows", "definitions", input],
    queryFn: ({ signal }) => listWorkflowDefinitions(input, signal),
    staleTime: 30_000,
  });
}
