import { useMutation, useQuery } from "@tanstack/react-query";
import {
  createRegistrationRequest,
  listPublicDepartments,
  listPublicJobTitles,
} from "../api/usersApi";
import type { CreateRegistrationRequestInput } from "../types/users";

export function usePublicRegistration() {
  const registerMutation = useMutation({
    mutationFn: (input: CreateRegistrationRequestInput) => createRegistrationRequest(input),
  });

  const departmentsQuery = useQuery({
    queryKey: ["public", "departments"],
    queryFn: ({ signal }) => listPublicDepartments(signal),
  });

  const jobTitlesQuery = useQuery({
    queryKey: ["public", "job-titles"],
    queryFn: ({ signal }) => listPublicJobTitles(signal),
  });

  return {
    departmentsQuery,
    jobTitlesQuery,
    registerMutation,
  };
}
