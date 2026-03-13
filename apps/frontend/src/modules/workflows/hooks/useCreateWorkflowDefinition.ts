import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createWorkflowDefinition } from "../api/workflowsApi";
import type { CreateWorkflowDefinitionInput } from "../types/workflows";

export function useCreateWorkflowDefinition() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: CreateWorkflowDefinitionInput) => createWorkflowDefinition(input),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["workflows", "definitions"] });
    },
  });
}
