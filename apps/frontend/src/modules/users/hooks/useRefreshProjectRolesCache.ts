import { useMutation } from "@tanstack/react-query";
import { refreshProjectRolesCache } from "../api/usersApi";

export function useRefreshProjectRolesCache() {
  return useMutation({
    mutationFn: () => refreshProjectRolesCache(),
  });
}
