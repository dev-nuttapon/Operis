import { apiRequest, publicApiRequest } from "../../../shared/lib/apiClient";
import type {
  AcceptInvitationInput,
  ApproveRegistrationInput,
  CreateMasterDataInput,
  CreateInvitationInput,
  CreateUserInput,
  UpdateUserInput,
  InvitationDetail,
  Invitation,
  InvitationStatus,
  AppRoleItem,
  MasterDataItem,
  RegistrationRequest,
  RegistrationRequestStatus,
  RejectRegistrationInput,
  SoftDeleteInput,
  UpdateMasterDataInput,
  UpdateCurrentUserPreferencesInput,
  UpdateInvitationInput,
  User,
} from "../types/users";

export function listUsers(signal?: AbortSignal) {
  return apiRequest<User[]>("/api/v1/users", { signal });
}

export function listRegistrationRequests(status?: RegistrationRequestStatus, signal?: AbortSignal) {
  const query = status ? `?status=${encodeURIComponent(status)}` : "";
  return apiRequest<RegistrationRequest[]>(`/api/v1/users/registration-requests${query}`, { signal });
}

export function listInvitations(status?: InvitationStatus, signal?: AbortSignal) {
  const query = status ? `?status=${encodeURIComponent(status)}` : "";
  return apiRequest<Invitation[]>(`/api/v1/users/invitations${query}`, { signal });
}

export function createInvitation(input: CreateInvitationInput) {
  return apiRequest<Invitation>("/api/v1/users/invitations", {
    method: "POST",
    body: input,
  });
}

export function updateInvitation(input: UpdateInvitationInput) {
  return apiRequest<Invitation>(`/api/v1/users/invitations/${input.id}`, {
    method: "PUT",
    body: {
      email: input.email,
      expiresAt: input.expiresAt,
    },
  });
}

export function cancelInvitation(invitationId: string) {
  return apiRequest<Invitation>(`/api/v1/users/invitations/${invitationId}/cancel`, {
    method: "POST",
  });
}

export function getInvitationByToken(token: string) {
  return publicApiRequest<InvitationDetail>(`/api/v1/users/invitations/${encodeURIComponent(token)}`);
}

export function acceptInvitation(token: string, input: AcceptInvitationInput) {
  return publicApiRequest<Invitation>(`/api/v1/users/invitations/${encodeURIComponent(token)}/accept`, {
    method: "POST",
    body: input,
  });
}

export function createUser(input: CreateUserInput) {
  return apiRequest<User>("/api/v1/users", {
    method: "POST",
    body: input,
  });
}

export function updateUser(input: UpdateUserInput) {
  return apiRequest<User>(`/api/v1/users/${input.id}`, {
    method: "PUT",
    body: {
      email: input.email,
      firstName: input.firstName,
      lastName: input.lastName,
      departmentId: input.departmentId,
      jobTitleId: input.jobTitleId,
      roleIds: input.roleIds,
    },
  });
}

export function deleteUser(id: string, input: SoftDeleteInput) {
  return apiRequest<void>(`/api/v1/users/${id}`, {
    method: "DELETE",
    body: input,
  });
}

export function listDepartments(signal?: AbortSignal) {
  return apiRequest<MasterDataItem[]>("/api/v1/users/departments", { signal });
}

export function listRoles(signal?: AbortSignal) {
  return apiRequest<AppRoleItem[]>("/api/v1/users/roles", { signal });
}

export function createDepartment(input: CreateMasterDataInput) {
  return apiRequest<MasterDataItem>("/api/v1/users/departments", {
    method: "POST",
    body: input,
  });
}

export function updateDepartment(input: UpdateMasterDataInput) {
  return apiRequest<MasterDataItem>(`/api/v1/users/departments/${input.id}`, {
    method: "PUT",
    body: { name: input.name, displayOrder: input.displayOrder },
  });
}

export function deleteDepartment(id: string, input: SoftDeleteInput) {
  return apiRequest<void>(`/api/v1/users/departments/${id}`, {
    method: "DELETE",
    body: input,
  });
}

export function listJobTitles(signal?: AbortSignal) {
  return apiRequest<MasterDataItem[]>("/api/v1/users/job-titles", { signal });
}

export function createJobTitle(input: CreateMasterDataInput) {
  return apiRequest<MasterDataItem>("/api/v1/users/job-titles", {
    method: "POST",
    body: input,
  });
}

export function updateJobTitle(input: UpdateMasterDataInput) {
  return apiRequest<MasterDataItem>(`/api/v1/users/job-titles/${input.id}`, {
    method: "PUT",
    body: { name: input.name, displayOrder: input.displayOrder },
  });
}

export function deleteJobTitle(id: string, input: SoftDeleteInput) {
  return apiRequest<void>(`/api/v1/users/job-titles/${id}`, {
    method: "DELETE",
    body: input,
  });
}

export function approveRegistration(requestId: string, input: ApproveRegistrationInput) {
  return apiRequest<User>(`/api/v1/users/registration-requests/${requestId}/approve`, {
    method: "POST",
    body: input,
  });
}

export function rejectRegistration(requestId: string, input: RejectRegistrationInput) {
  return apiRequest<RegistrationRequest>(`/api/v1/users/registration-requests/${requestId}/reject`, {
    method: "POST",
    body: input,
  });
}

export function updateCurrentUserPreferences(input: UpdateCurrentUserPreferencesInput) {
  return apiRequest<void>("/api/v1/users/me/preferences", {
    method: "PUT",
    body: {
      preferredLanguage: input.preferredLanguage,
      preferredTheme: input.preferredTheme,
    },
  });
}
