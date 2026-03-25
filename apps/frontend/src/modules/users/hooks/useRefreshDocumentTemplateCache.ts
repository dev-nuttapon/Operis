import { useMutation } from "@tanstack/react-query";
import { refreshDocumentTemplateCache } from "../api/usersApi";

export function useRefreshDocumentTemplateCache() {
  return useMutation({
    mutationFn: () => refreshDocumentTemplateCache(),
  });
}
