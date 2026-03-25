import { useMutation } from "@tanstack/react-query";
import { refreshDepartmentsCache } from "../api/usersApi";

export function useRefreshDepartmentsCache() {
  return useMutation({
    mutationFn: () => refreshDepartmentsCache(),
  });
}
