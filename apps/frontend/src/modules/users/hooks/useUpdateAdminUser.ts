import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateUser } from "../api/usersApi";
import type { UpdateUserInput } from "../types/users";

export function useUpdateAdminUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: UpdateUserInput) => updateUser(input),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ["admin", "users"] });
      queryClient.invalidateQueries({ queryKey: ["admin", "users", data.id] });
    },
  });
}

