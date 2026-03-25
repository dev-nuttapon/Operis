import { useMutation, useQueryClient } from "@tanstack/react-query";
import { refreshKeycloakUser } from "../api/usersApi";

export function useRefreshKeycloakUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (userId: string) => refreshKeycloakUser(userId),
    onSuccess: async (_data, userId) => {
      await queryClient.invalidateQueries({ queryKey: ["admin", "users"] });
      await queryClient.invalidateQueries({ queryKey: ["admin", "users", userId] });
    },
  });
}
