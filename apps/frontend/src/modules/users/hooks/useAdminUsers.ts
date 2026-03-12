import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  approveRegistration,
  createInvitation,
  createUser,
  listInvitations,
  listRegistrationRequests,
  listUsers,
  rejectRegistration,
} from "../api/usersApi";
import type {
  ApproveRegistrationInput,
  CreateInvitationInput,
  CreateUserInput,
  RejectRegistrationInput,
} from "../types/users";

const usersQueryKey = ["admin", "users"];
const requestsQueryKey = ["admin", "registration-requests"];
const invitationsQueryKey = ["admin", "invitations"];

export function useAdminUsers() {
  const queryClient = useQueryClient();

  const usersQuery = useQuery({
    queryKey: usersQueryKey,
    queryFn: ({ signal }) => listUsers(signal),
  });

  const registrationRequestsQuery = useQuery({
    queryKey: requestsQueryKey,
    queryFn: ({ signal }) => listRegistrationRequests(undefined, signal),
  });

  const invitationsQuery = useQuery({
    queryKey: invitationsQueryKey,
    queryFn: ({ signal }) => listInvitations(undefined, signal),
  });

  const invalidateAll = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: usersQueryKey }),
      queryClient.invalidateQueries({ queryKey: requestsQueryKey }),
      queryClient.invalidateQueries({ queryKey: invitationsQueryKey }),
    ]);
  };

  const createInvitationMutation = useMutation({
    mutationFn: (input: CreateInvitationInput) => createInvitation(input),
    onSuccess: invalidateAll,
  });

  const createUserMutation = useMutation({
    mutationFn: (input: CreateUserInput) => createUser(input),
    onSuccess: invalidateAll,
  });

  const approveRegistrationMutation = useMutation({
    mutationFn: ({ requestId, input }: { requestId: string; input: ApproveRegistrationInput }) =>
      approveRegistration(requestId, input),
    onSuccess: invalidateAll,
  });

  const rejectRegistrationMutation = useMutation({
    mutationFn: ({ requestId, input }: { requestId: string; input: RejectRegistrationInput }) =>
      rejectRegistration(requestId, input),
    onSuccess: invalidateAll,
  });

  return {
    usersQuery,
    registrationRequestsQuery,
    invitationsQuery,
    createInvitationMutation,
    createUserMutation,
    approveRegistrationMutation,
    rejectRegistrationMutation,
  };
}
