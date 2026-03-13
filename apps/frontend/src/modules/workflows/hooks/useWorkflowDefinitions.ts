import { useQuery } from "@tanstack/react-query";
import { listWorkflowDefinitions } from "../api/workflowsApi";

export function useWorkflowDefinitions() {
  return useQuery({
    queryKey: ["workflows", "definitions"],
    queryFn: ({ signal }) => listWorkflowDefinitions(signal),
    staleTime: 30_000,
  });
}
