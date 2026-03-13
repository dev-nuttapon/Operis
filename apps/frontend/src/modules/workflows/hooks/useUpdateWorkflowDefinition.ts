import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateWorkflowDefinition } from "../api/workflowsApi";
import type { UpdateWorkflowDefinitionInput } from "../types/workflows";

export function useUpdateWorkflowDefinition() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: UpdateWorkflowDefinitionInput) => updateWorkflowDefinition(input),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["workflows", "definitions"] });
    },
  });
}
