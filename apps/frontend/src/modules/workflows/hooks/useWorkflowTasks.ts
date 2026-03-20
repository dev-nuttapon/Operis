import { useQuery } from "@tanstack/react-query";
import { listWorkflowTasks } from "../api/workflowsApi";
import type { WorkflowTaskListInput, WorkflowTaskListResponse } from "../types/workflows";

export function useWorkflowTasks(input: WorkflowTaskListInput) {
  return useQuery<WorkflowTaskListResponse>({
    queryKey: ["workflows", "tasks", input],
    queryFn: () => listWorkflowTasks(input),
    staleTime: 30_000,
  });
}
