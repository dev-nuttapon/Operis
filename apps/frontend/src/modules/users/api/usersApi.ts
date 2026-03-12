import { apiRequest } from "../../../shared/lib/apiClient";
import type {
  ApproveRegistrationInput,
  CreateMasterDataInput,
  CreateInvitationInput,
  CreateUserInput,
  Invitation,
  InvitationStatus,
  MasterDataItem,
  RegistrationRequest,
  RegistrationRequestStatus,
  RejectRegistrationInput,
  UpdateMasterDataInput,
  UpdateCurrentUserPreferencesInput,
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

export function createUser(input: CreateUserInput) {
  return apiRequest<User>("/api/v1/users", {
    method: "POST",
    body: input,
  });
}

export function listDepartments(signal?: AbortSignal) {
  return apiRequest<MasterDataItem[]>("/api/v1/users/departments", { signal });
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
    body: { name: input.name },
  });
}

export function deleteDepartment(id: string) {
  return apiRequest<void>(`/api/v1/users/departments/${id}`, {
    method: "DELETE",
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
    body: { name: input.name },
  });
}

export function deleteJobTitle(id: string) {
  return apiRequest<void>(`/api/v1/users/job-titles/${id}`, {
    method: "DELETE",
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
