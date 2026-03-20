import { useMutation, useQueryClient } from "@tanstack/react-query";
import { applyWorkflowStepAction, createWorkflowInstance } from "../api/workflowsApi";
import type { CreateWorkflowInstanceInput, WorkflowStepActionInput } from "../types/workflows";

export function useWorkflowInstanceActions() {
  const queryClient = useQueryClient();

  const createInstanceMutation = useMutation({
    mutationFn: (input: CreateWorkflowInstanceInput) => createWorkflowInstance(input),
    onSuccess: async (data) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["workflows", "instance", data.instance.id] }),
        queryClient.invalidateQueries({ queryKey: ["workflows", "instance-by-document", data.instance.documentId] }),
      ]);
    },
  });

  const applyStepActionMutation = useMutation({
    mutationFn: (input: WorkflowStepActionInput) => applyWorkflowStepAction(input),
    onSuccess: async (data) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["workflows", "instance", data.instance.id] }),
        queryClient.invalidateQueries({ queryKey: ["workflows", "instance-by-document", data.instance.documentId] }),
      ]);
    },
  });

  return {
    createInstanceMutation,
    applyStepActionMutation,
  };
}
