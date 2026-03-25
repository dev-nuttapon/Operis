import { useMutation, useQueryClient } from "@tanstack/react-query";
import { refreshCurrentKeycloakUser } from "../api/usersApi";

export function useRefreshCurrentKeycloakUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => refreshCurrentKeycloakUser(),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["current-user-profile"] });
    },
  });
}
