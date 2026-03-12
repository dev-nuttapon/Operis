import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  approveRegistration,
  createDepartment,
  createInvitation,
  createJobTitle,
  createUser,
  deleteDepartment,
  deleteJobTitle,
  listDepartments,
  listInvitations,
  listJobTitles,
  listRegistrationRequests,
  listUsers,
  rejectRegistration,
  updateDepartment,
  updateJobTitle,
} from "../api/usersApi";
import type {
  ApproveRegistrationInput,
  CreateMasterDataInput,
  CreateInvitationInput,
  CreateUserInput,
  RejectRegistrationInput,
  UpdateMasterDataInput,
} from "../types/users";

const usersQueryKey = ["admin", "users"];
const requestsQueryKey = ["admin", "registration-requests"];
const invitationsQueryKey = ["admin", "invitations"];
const departmentsQueryKey = ["admin", "departments"];
const jobTitlesQueryKey = ["admin", "job-titles"];

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

  const invalidateAll = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: usersQueryKey }),
      queryClient.invalidateQueries({ queryKey: requestsQueryKey }),
      queryClient.invalidateQueries({ queryKey: invitationsQueryKey }),
      queryClient.invalidateQueries({ queryKey: departmentsQueryKey }),
      queryClient.invalidateQueries({ queryKey: jobTitlesQueryKey }),
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

  const createDepartmentMutation = useMutation({
    mutationFn: (input: CreateMasterDataInput) => createDepartment(input),
    onSuccess: invalidateAll,
  });

  const updateDepartmentMutation = useMutation({
    mutationFn: (input: UpdateMasterDataInput) => updateDepartment(input),
    onSuccess: invalidateAll,
  });

  const deleteDepartmentMutation = useMutation({
    mutationFn: (id: string) => deleteDepartment(id),
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
    mutationFn: (id: string) => deleteJobTitle(id),
    onSuccess: invalidateAll,
  });

  return {
    usersQuery,
    registrationRequestsQuery,
    invitationsQuery,
    departmentsQuery,
    jobTitlesQuery,
    createInvitationMutation,
    createUserMutation,
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
