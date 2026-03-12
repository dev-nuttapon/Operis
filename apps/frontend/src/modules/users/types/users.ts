export type UserStatus = "Active" | "Rejected";
export type RegistrationRequestStatus = "Pending" | "Approved" | "Rejected";
export type InvitationStatus = "Pending" | "Accepted" | "Rejected" | "Expired";

export interface KeycloakUserSummary {
  id: string;
  email: string;
  username: string;
  enabled: boolean;
  emailVerified: boolean;
}

export interface User {
  id: string;
  keycloakUserId: string | null;
  email: string;
  firstName: string;
  lastName: string;
  status: UserStatus;
  createdAt: string;
  createdBy: string;
  approvedAt: string | null;
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
}

export interface ApproveRegistrationInput {
  reviewedBy: string;
}

export interface RejectRegistrationInput {
  reviewedBy: string;
  reason: string;
}
