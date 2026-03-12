import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  approveRegistration,
  cancelInvitation,
  createDepartment,
  createInvitation,
  createJobTitle,
  createUser,
  deleteDepartment,
  deleteJobTitle,
  deleteUser,
  listDepartments,
  listInvitations,
  listJobTitles,
  listRoles,
  listRegistrationRequests,
  listUsers,
  rejectRegistration,
  updateInvitation,
  updateUser,
  updateDepartment,
  updateJobTitle,
} from "../api/usersApi";
import type {
  ApproveRegistrationInput,
  CreateMasterDataInput,
  CreateInvitationInput,
  CreateUserInput,
  RejectRegistrationInput,
  UpdateInvitationInput,
  UpdateUserInput,
  UpdateMasterDataInput,
} from "../types/users";

const usersQueryKey = ["admin", "users"];
const requestsQueryKey = ["admin", "registration-requests"];
const invitationsQueryKey = ["admin", "invitations"];
const departmentsQueryKey = ["admin", "departments"];
const jobTitlesQueryKey = ["admin", "job-titles"];
const rolesQueryKey = ["admin", "roles"];

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

  const departmentsQuery = useQuery({
    queryKey: departmentsQueryKey,
    queryFn: ({ signal }) => listDepartments(signal),
  });

  const jobTitlesQuery = useQuery({
    queryKey: jobTitlesQueryKey,
    queryFn: ({ signal }) => listJobTitles(signal),
  });

  const rolesQuery = useQuery({
    queryKey: rolesQueryKey,
    queryFn: ({ signal }) => listRoles(signal),
  });

  const invalidateAll = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: usersQueryKey }),
      queryClient.invalidateQueries({ queryKey: requestsQueryKey }),
      queryClient.invalidateQueries({ queryKey: invitationsQueryKey }),
      queryClient.invalidateQueries({ queryKey: departmentsQueryKey }),
      queryClient.invalidateQueries({ queryKey: jobTitlesQueryKey }),
      queryClient.invalidateQueries({ queryKey: rolesQueryKey }),
    ]);
  };

  const createInvitationMutation = useMutation({
    mutationFn: (input: CreateInvitationInput) => createInvitation(input),
    onSuccess: invalidateAll,
  });

  const updateInvitationMutation = useMutation({
    mutationFn: (input: UpdateInvitationInput) => updateInvitation(input),
    onSuccess: invalidateAll,
  });

  const cancelInvitationMutation = useMutation({
    mutationFn: (invitationId: string) => cancelInvitation(invitationId),
    onSuccess: invalidateAll,
  });

  const createUserMutation = useMutation({
    mutationFn: (input: CreateUserInput) => createUser(input),
    onSuccess: invalidateAll,
  });

  const updateUserMutation = useMutation({
    mutationFn: (input: UpdateUserInput) => updateUser(input),
    onSuccess: invalidateAll,
  });

  const deleteUserMutation = useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) => deleteUser(id, { reason }),
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

  const createDepartmentMutation = useMutation({
    mutationFn: (input: CreateMasterDataInput) => createDepartment(input),
    onSuccess: invalidateAll,
  });

  const updateDepartmentMutation = useMutation({
    mutationFn: (input: UpdateMasterDataInput) => updateDepartment(input),
    onSuccess: invalidateAll,
  });

  const deleteDepartmentMutation = useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) => deleteDepartment(id, { reason }),
    onSuccess: invalidateAll,
  });

  const createJobTitleMutation = useMutation({
    mutationFn: (input: CreateMasterDataInput) => createJobTitle(input),
    onSuccess: invalidateAll,
  });

  const updateJobTitleMutation = useMutation({
    mutationFn: (input: UpdateMasterDataInput) => updateJobTitle(input),
    onSuccess: invalidateAll,
  });

  const deleteJobTitleMutation = useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) => deleteJobTitle(id, { reason }),
    onSuccess: invalidateAll,
  });

  return {
    usersQuery,
    registrationRequestsQuery,
    invitationsQuery,
    departmentsQuery,
    jobTitlesQuery,
    rolesQuery,
    createInvitationMutation,
    updateInvitationMutation,
    cancelInvitationMutation,
    createUserMutation,
    updateUserMutation,
    deleteUserMutation,
    approveRegistrationMutation,
    rejectRegistrationMutation,
    createDepartmentMutation,
    updateDepartmentMutation,
    deleteDepartmentMutation,
    createJobTitleMutation,
    updateJobTitleMutation,
    deleteJobTitleMutation,
  };
}
