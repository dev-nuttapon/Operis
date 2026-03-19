import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { CreateUserInput } from "../types/users";
import { createUser } from "../api/usersApi";

const usersQueryKey = ["admin", "users"];

export function useCreateAdminUser() {
  const queryClient = useQueryClient();
  const mutation = useMutation({
    mutationFn: (payload: CreateUserInput) => createUser(payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: usersQueryKey });
    },
  });

  return mutation;
}

