import { useMutation } from "@tanstack/react-query";
import { refreshDivisionsCache } from "../api/usersApi";

export function useRefreshDivisionsCache() {
  return useMutation({
    mutationFn: () => refreshDivisionsCache(),
  });
}
