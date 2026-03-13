import { apiRequest, publicApiRequest } from "../../../shared/lib/apiClient";
import type { PaginatedResult, PaginationInput } from "../../../shared/types/pagination";
import type {
  AcceptInvitationInput,
  ApproveRegistrationInput,
  CompleteRegistrationPasswordSetupInput,
  CreateDepartmentInput,
  CreateJobTitleInput,
  CreateMasterDataInput,
  CreateInvitationInput,
  CreateUserInput,
  RegistrationPasswordSetupDetail,
  UpdateUserInput,
  InvitationDetail,
  Invitation,
  AppRoleItem,
  CreateRegistrationRequestInput,
  ListInvitationsInput,
  ListRegistrationRequestsInput,
  ListUsersInput,
  MasterDataItem,
  RegistrationRequest,
  RejectRegistrationInput,
  SoftDeleteInput,
  UpdateMasterDataInput,
  UpdateCurrentUserPreferencesInput,
  UpdateDepartmentInput,
  UpdateJobTitleInput,
  UpdateInvitationInput,
  UpsertUserOrgAssignmentInput,
  User,
} from "../types/users";

type ListQueryInput = PaginationInput & {
  search?: string;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
  from?: string;
  to?: string;
  divisionId?: string;
  departmentId?: string;
};

function toListQuery(input?: ListQueryInput) {
  const params = new URLSearchParams();
  if (input?.page) params.set("page", String(input.page));
  if (input?.pageSize) params.set("pageSize", String(input.pageSize));
  if (input?.search) params.set("search", input.search);
  if (input?.sortBy) params.set("sortBy", input.sortBy);
  if (input?.sortOrder) params.set("sortOrder", input.sortOrder);
  if (input?.from) params.set("from", input.from);
  if (input?.to) params.set("to", input.to);
  if (input?.divisionId) params.set("divisionId", input.divisionId);
  if (input?.departmentId) params.set("departmentId", input.departmentId);
  const query = params.toString();
  return query ? `?${query}` : "";
}

export function listUsers(input?: ListUsersInput, signal?: AbortSignal) {
  const params = new URLSearchParams();
  if (input?.status) params.set("status", input.status);
  if (input?.page) params.set("page", String(input.page));
  if (input?.pageSize) params.set("pageSize", String(input.pageSize));
  if (input?.search) params.set("search", input.search);
  if (input?.sortBy) params.set("sortBy", input.sortBy);
  if (input?.sortOrder) params.set("sortOrder", input.sortOrder);
  if (input?.from) params.set("from", input.from);
  if (input?.to) params.set("to", input.to);
  const query = params.toString();
  return apiRequest<PaginatedResult<User>>(`/api/v1/users${query ? `?${query}` : ""}`, { signal });
}

export function createRegistrationRequest(input: CreateRegistrationRequestInput) {
  return publicApiRequest<RegistrationRequest>("/api/v1/users/register", {
    method: "POST",
    body: input,
  });
}

export function listRegistrationRequests(input?: ListRegistrationRequestsInput, signal?: AbortSignal) {
  const params = new URLSearchParams();
  if (input?.status) params.set("status", input.status);
  if (input?.page) params.set("page", String(input.page));
  if (input?.pageSize) params.set("pageSize", String(input.pageSize));
  if (input?.search) params.set("search", input.search);
  if (input?.sortBy) params.set("sortBy", input.sortBy);
  if (input?.sortOrder) params.set("sortOrder", input.sortOrder);
  if (input?.from) params.set("from", input.from);
  if (input?.to) params.set("to", input.to);
  const query = params.toString();
  return apiRequest<PaginatedResult<RegistrationRequest>>(`/api/v1/users/registration-requests${query ? `?${query}` : ""}`, { signal });
}

export function getRegistrationPasswordSetup(token: string) {
  return publicApiRequest<RegistrationPasswordSetupDetail>(`/api/v1/users/registration-requests/${encodeURIComponent(token)}/setup-password`);
}

export function completeRegistrationPasswordSetup(token: string, input: CompleteRegistrationPasswordSetupInput) {
  return publicApiRequest<void>(`/api/v1/users/registration-requests/${encodeURIComponent(token)}/setup-password`, {
    method: "POST",
    body: input,
  });
}

export function listInvitations(input?: ListInvitationsInput, signal?: AbortSignal) {
  const params = new URLSearchParams();
  if (input?.status) params.set("status", input.status);
  if (input?.page) params.set("page", String(input.page));
  if (input?.pageSize) params.set("pageSize", String(input.pageSize));
  if (input?.search) params.set("search", input.search);
  if (input?.sortBy) params.set("sortBy", input.sortBy);
  if (input?.sortOrder) params.set("sortOrder", input.sortOrder);
  if (input?.from) params.set("from", input.from);
  if (input?.to) params.set("to", input.to);
  const query = params.toString();
  return apiRequest<PaginatedResult<Invitation>>(`/api/v1/users/invitations${query ? `?${query}` : ""}`, { signal });
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
      divisionId: input.divisionId,
      expiresAt: input.expiresAt,
      departmentId: input.departmentId,
      jobTitleId: input.jobTitleId,
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
      divisionId: input.divisionId,
      departmentId: input.departmentId,
      jobTitleId: input.jobTitleId,
      roleIds: input.roleIds,
    },
  });
}

export function upsertUserOrgAssignment(input: UpsertUserOrgAssignmentInput) {
  return apiRequest<void>(`/api/v1/users/${input.userId}/org-assignment`, {
    method: "PUT",
    body: {
      divisionId: input.divisionId,
      departmentId: input.departmentId,
      positionId: input.positionId,
    },
  });
}

export function deleteUser(id: string, input: SoftDeleteInput) {
  return apiRequest<void>(`/api/v1/users/${id}`, {
    method: "DELETE",
    body: input,
  });
}

export function listDepartments(input?: ListQueryInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<MasterDataItem>>(`/api/v1/users/departments${toListQuery(input)}`, { signal });
}

export function listDivisions(input?: ListQueryInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<MasterDataItem>>(`/api/v1/users/divisions${toListQuery(input)}`, { signal });
}

export function listPublicDepartments(signal?: AbortSignal) {
  return publicApiRequest<PaginatedResult<MasterDataItem>>("/api/v1/users/departments?page=1&pageSize=100", { signal });
}

export function listPublicDepartmentsByDivision(divisionId: string, signal?: AbortSignal) {
  return publicApiRequest<PaginatedResult<MasterDataItem>>(`/api/v1/users/departments?page=1&pageSize=100&divisionId=${encodeURIComponent(divisionId)}`, { signal });
}

export function listPublicDivisions(signal?: AbortSignal) {
  return publicApiRequest<PaginatedResult<MasterDataItem>>("/api/v1/users/divisions?page=1&pageSize=100", { signal });
}

export function listRoles(signal?: AbortSignal) {
  return apiRequest<AppRoleItem[]>("/api/v1/users/roles", { signal });
}

export function createDepartment(input: CreateDepartmentInput) {
  return apiRequest<MasterDataItem>("/api/v1/users/departments", {
    method: "POST",
    body: input,
  });
}

export function createDivision(input: CreateMasterDataInput) {
  return apiRequest<MasterDataItem>("/api/v1/users/divisions", {
    method: "POST",
    body: input,
  });
}

export function updateDepartment(input: UpdateDepartmentInput) {
  return apiRequest<MasterDataItem>(`/api/v1/users/departments/${input.id}`, {
    method: "PUT",
    body: { name: input.name, displayOrder: input.displayOrder, divisionId: input.divisionId },
  });
}

export function updateDivision(input: UpdateMasterDataInput) {
  return apiRequest<MasterDataItem>(`/api/v1/users/divisions/${input.id}`, {
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

export function deleteDivision(id: string, input: SoftDeleteInput) {
  return apiRequest<void>(`/api/v1/users/divisions/${id}`, {
    method: "DELETE",
    body: input,
  });
}

export function listJobTitles(input?: ListQueryInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<MasterDataItem>>(`/api/v1/users/job-titles${toListQuery(input)}`, { signal });
}

export function listPublicJobTitlesByDepartment(departmentId: string, signal?: AbortSignal) {
  return publicApiRequest<PaginatedResult<MasterDataItem>>(`/api/v1/users/job-titles?page=1&pageSize=100&departmentId=${encodeURIComponent(departmentId)}`, { signal });
}

export function listProjectRoles(input?: ListQueryInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<MasterDataItem>>(`/api/v1/users/project-roles${toListQuery(input)}`, { signal });
}

export function listPublicJobTitles(signal?: AbortSignal) {
  return publicApiRequest<PaginatedResult<MasterDataItem>>("/api/v1/users/job-titles?page=1&pageSize=100", { signal });
}

export function createJobTitle(input: CreateJobTitleInput) {
  return apiRequest<MasterDataItem>("/api/v1/users/job-titles", {
    method: "POST",
    body: input,
  });
}

export function createProjectRole(input: CreateMasterDataInput) {
  return apiRequest<MasterDataItem>("/api/v1/users/project-roles", {
    method: "POST",
    body: input,
  });
}

export function updateJobTitle(input: UpdateJobTitleInput) {
  return apiRequest<MasterDataItem>(`/api/v1/users/job-titles/${input.id}`, {
    method: "PUT",
    body: { name: input.name, displayOrder: input.displayOrder, departmentId: input.departmentId },
  });
}

export function updateProjectRole(input: UpdateMasterDataInput) {
  return apiRequest<MasterDataItem>(`/api/v1/users/project-roles/${input.id}`, {
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

export function deleteProjectRole(id: string, input: SoftDeleteInput) {
  return apiRequest<void>(`/api/v1/users/project-roles/${id}`, {
    method: "DELETE",
    body: input,
  });
}

export function approveRegistration(requestId: string, input: ApproveRegistrationInput) {
  return apiRequest<RegistrationRequest>(`/api/v1/users/registration-requests/${requestId}/approve`, {
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
