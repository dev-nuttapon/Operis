import { useMutation } from "@tanstack/react-query";
import { refreshKeycloakUsersCache } from "../api/usersApi";

export function useRefreshKeycloakUsersCache() {
  return useMutation({
    mutationFn: () => refreshKeycloakUsersCache(),
  });
}
