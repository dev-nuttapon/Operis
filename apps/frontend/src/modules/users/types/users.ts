export type UserStatus = "Active" | "Rejected";
export type RegistrationRequestStatus = "Pending" | "Approved" | "Rejected";
export type InvitationStatus = "Pending" | "Accepted" | "Rejected" | "Expired";

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
  preferredLanguage: string | null;
  preferredTheme: string | null;
  deletedBy: string | null;
  deletedAt: string | null;
  keycloak: KeycloakUserSummary | null;
}

export interface RegistrationRequest {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  status: RegistrationRequestStatus;
  requestedAt: string;
  reviewedAt: string | null;
  reviewedBy: string | null;
  rejectionReason: string | null;
}

export interface Invitation {
  id: string;
  email: string;
  invitedBy: string;
  status: InvitationStatus;
  invitedAt: string;
  expiresAt: string | null;
  acceptedAt: string | null;
  rejectedAt: string | null;
}

export interface CreateInvitationInput {
  email: string;
  invitedBy: string;
  expiresInDays?: number;
}

export interface CreateUserInput {
  email: string;
  firstName: string;
  lastName: string;
  createdBy: string;
  departmentId?: string;
  jobTitleId?: string;
}

export interface MasterDataItem {
  id: string;
  name: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateMasterDataInput {
  name: string;
}

export interface UpdateMasterDataInput {
  id: string;
  name: string;
}

export interface ApproveRegistrationInput {
  reviewedBy: string;
}

export interface RejectRegistrationInput {
  reviewedBy: string;
  reason: string;
}

export interface UpdateCurrentUserPreferencesInput {
  preferredLanguage: string | null;
  preferredTheme: "light" | "dark" | "system" | null;
}
