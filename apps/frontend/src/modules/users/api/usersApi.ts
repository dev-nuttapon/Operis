import { apiRequest } from "../../../shared/lib/apiClient";
import type {
  ApproveRegistrationInput,
  CreateInvitationInput,
  CreateUserInput,
  Invitation,
  InvitationStatus,
  RegistrationRequest,
  RegistrationRequestStatus,
  RejectRegistrationInput,
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
