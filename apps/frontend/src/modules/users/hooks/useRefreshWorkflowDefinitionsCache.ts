import { useMutation } from "@tanstack/react-query";
import { refreshWorkflowDefinitionsCache } from "../api/usersApi";

export function useRefreshWorkflowDefinitionsCache() {
  return useMutation({
    mutationFn: () => refreshWorkflowDefinitionsCache(),
  });
}
