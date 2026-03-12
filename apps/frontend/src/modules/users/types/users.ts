export type UserStatus = "Active" | "Rejected" | "Deleted";
export type RegistrationRequestStatus = "Pending" | "Approved" | "Rejected";
export type InvitationStatus = "Pending" | "Accepted" | "Rejected" | "Expired" | "Cancelled";

export interface KeycloakUserSummary {
  id: string;
  email: string;
  username: string;
  firstName: string | null;
  lastName: string | null;
  enabled: boolean;
  emailVerified: boolean;
}

export interface User {
  id: string;
  status: UserStatus;
  createdAt: string;
  createdBy: string;
  departmentId: string | null;
  departmentName: string | null;
  jobTitleId: string | null;
  jobTitleName: string | null;
  roles: string[];
  preferredLanguage: string | null;
  preferredTheme: string | null;
  deletedReason: string | null;
  deletedBy: string | null;
  deletedAt: string | null;
  keycloak: KeycloakUserSummary | null;
}

export interface RegistrationRequest {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  departmentId: string | null;
  departmentName: string | null;
  jobTitleId: string | null;
  jobTitleName: string | null;
  status: RegistrationRequestStatus;
  requestedAt: string;
  reviewedAt: string | null;
  reviewedBy: string | null;
  rejectionReason: string | null;
  passwordSetupLink: string | null;
  passwordSetupExpiresAt: string | null;
  passwordSetupCompletedAt: string | null;
}

export interface RegistrationPasswordSetupDetail {
  email: string;
  firstName: string;
  lastName: string;
  departmentName: string | null;
  jobTitleName: string | null;
  isExpired: boolean;
  isCompleted: boolean;
  expiresAt: string | null;
}

export interface Invitation {
  id: string;
  email: string;
  invitationToken: string;
  invitedBy: string;
  departmentId: string | null;
  departmentName: string | null;
  jobTitleId: string | null;
  jobTitleName: string | null;
  status: InvitationStatus;
  invitedAt: string;
  expiresAt: string | null;
  acceptedAt: string | null;
  rejectedAt: string | null;
  invitationLink: string;
}

export interface CreateInvitationInput {
  email: string;
  invitedBy: string;
  expiresAt?: string;
  departmentId?: string;
  jobTitleId?: string;
}

export interface UpdateInvitationInput {
  id: string;
  email: string;
  expiresAt?: string;
  departmentId?: string;
  jobTitleId?: string;
}

export interface InvitationDetail {
  id: string;
  email: string;
  departmentId: string | null;
  departmentName: string | null;
  jobTitleId: string | null;
  jobTitleName: string | null;
  status: InvitationStatus;
  invitedAt: string;
  expiresAt: string | null;
}

export interface AcceptInvitationInput {
  firstName: string;
  lastName: string;
  password: string;
  confirmPassword: string;
}

export interface CreateUserInput {
  email: string;
  firstName: string;
  lastName: string;
  password: string;
  confirmPassword: string;
  createdBy: string;
  departmentId?: string;
  jobTitleId?: string;
  roleIds?: string[];
}

export interface UpdateUserInput {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  departmentId?: string;
  jobTitleId?: string;
  roleIds?: string[];
}

export interface MasterDataItem {
  id: string;
  name: string;
  displayOrder: number;
  createdAt: string;
  updatedAt: string | null;
  deletedReason: string | null;
  deletedBy: string | null;
  deletedAt: string | null;
}

export interface CreateMasterDataInput {
  name: string;
  displayOrder: number;
}

export interface UpdateMasterDataInput {
  id: string;
  name: string;
  displayOrder: number;
}

export interface SoftDeleteInput {
  reason: string;
}

export interface AppRoleItem {
  id: string;
  name: string;
  keycloakRoleName: string;
  description: string | null;
  displayOrder: number;
}

export interface ApproveRegistrationInput {
  reviewedBy: string;
}

export interface RejectRegistrationInput {
  reviewedBy: string;
  reason: string;
}

export interface CreateRegistrationRequestInput {
  email: string;
  firstName: string;
  lastName: string;
  departmentId?: string;
  jobTitleId?: string;
}

export interface UpdateCurrentUserPreferencesInput {
  preferredLanguage: string | null;
  preferredTheme: "light" | "dark" | "system" | null;
}

export interface CompleteRegistrationPasswordSetupInput {
  password: string;
  confirmPassword: string;
}
