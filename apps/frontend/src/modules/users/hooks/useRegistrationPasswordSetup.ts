import { useMutation, useQuery } from "@tanstack/react-query";
import {
  completeRegistrationPasswordSetup,
  getRegistrationPasswordSetup,
} from "../api/usersApi";
import type { CompleteRegistrationPasswordSetupInput } from "../types/users";

export function useRegistrationPasswordSetup(token?: string) {
  const setupQuery = useQuery({
    queryKey: ["public", "registration-password-setup", token],
    queryFn: () => getRegistrationPasswordSetup(token ?? ""),
    enabled: Boolean(token),
  });

  const completeMutation = useMutation({
    mutationFn: (input: CompleteRegistrationPasswordSetupInput) =>
      completeRegistrationPasswordSetup(token ?? "", input),
  });

  return {
    completeMutation,
    setupQuery,
  };
}
