import { useMutation } from "@tanstack/react-query";
import { createRegistrationRequest } from "../api/usersApi";
import type { CreateRegistrationRequestInput } from "../types/users";

export function usePublicRegistration() {
  const registerMutation = useMutation({
    mutationFn: (input: CreateRegistrationRequestInput) => createRegistrationRequest(input),
  });

  return {
    registerMutation,
  };
}
