import { useMutation, useQuery } from "@tanstack/react-query";
import {
  createRegistrationRequest,
  listPublicDivisions,
} from "../api/usersApi";
import type { CreateRegistrationRequestInput } from "../types/users";

export function usePublicRegistration() {
  const registerMutation = useMutation({
    mutationFn: (input: CreateRegistrationRequestInput) => createRegistrationRequest(input),
  });

  const divisionsQuery = useQuery({
    queryKey: ["public", "divisions"],
    queryFn: ({ signal }) => listPublicDivisions(signal),
    staleTime: 5 * 60_000,
  });

  return {
    divisionsQuery,
    registerMutation,
  };
}
