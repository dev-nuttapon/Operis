export type UserStatus = "Active" | "Rejected" | "Deleted";
export type RegistrationRequestStatus = "Pending" | "Approved" | "Rejected";
export type InvitationStatus = "Pending" | "Accepted" | "Rejected" | "Expired" | "Cancelled";

export interface ListUsersInput {
  status?: UserStatus;
  divisionId?: string;
  departmentId?: string;
  jobTitleId?: string;
  search?: string;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}

export interface ListRegistrationRequestsInput {
  status?: RegistrationRequestStatus;
  search?: string;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}

export interface ListInvitationsInput {
  status?: InvitationStatus;
  search?: string;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}

export interface ListProjectsInput {
  search?: string;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
  assignedOnly?: boolean;
  page?: number;
  pageSize?: number;
}

export interface ListProjectAssignmentsInput {
  projectId: string;
  search?: string;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
  page?: number;
  pageSize?: number;
}

export interface ReferenceDataListInput {
  search?: string;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
  page?: number;
  pageSize?: number;
  divisionId?: string;
  departmentId?: string;
}

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
  email: string;
  status: UserStatus;
  createdAt: string;
  createdBy: string;
  divisionId: string | null;
  divisionName: string | null;
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
  divisionId: string | null;
  divisionName: string | null;
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
  divisionName: string | null;
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
  divisionId: string | null;
  divisionName: string | null;
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
  divisionId?: string;
  departmentId?: string;
  jobTitleId?: string;
}

export interface UpdateInvitationInput {
  id: string;
  email: string;
  expiresAt?: string;
  divisionId?: string;
  departmentId?: string;
  jobTitleId?: string;
}

export interface InvitationDetail {
  id: string;
  email: string;
  divisionId: string | null;
  divisionName: string | null;
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
  divisionId?: string;
  departmentId?: string;
  jobTitleId?: string;
  roleIds?: string[];
  roleChangeReason?: string;
}

export interface UpdateUserInput {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  divisionId?: string;
  departmentId?: string;
  jobTitleId?: string;
  roleIds?: string[];
  roleChangeReason?: string;
}

export interface UpsertUserOrgAssignmentInput {
  userId: string;
  divisionId?: string;
  departmentId?: string;
  positionId?: string;
}

export interface MasterDataItem {
  id: string;
  name: string;
  displayOrder: number;
  divisionId: string | null;
  divisionName: string | null;
  departmentId: string | null;
  departmentName: string | null;
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

export interface CreateDepartmentInput extends CreateMasterDataInput {
  divisionId?: string;
}

export interface UpdateMasterDataInput {
  id: string;
  name: string;
  displayOrder: number;
}

export interface UpdateDepartmentInput extends UpdateMasterDataInput {
  divisionId?: string;
}

export interface CreateJobTitleInput extends CreateMasterDataInput {
  departmentId?: string;
}

export interface UpdateJobTitleInput extends UpdateMasterDataInput {
  departmentId?: string;
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

export interface PermissionCatalogItem {
  key: string;
  label: string;
}

export interface PermissionRoleGrant {
  roleId: string;
  roleName: string;
  roleKeycloakName: string;
  grantedPermissions: string[];
}

export interface PermissionMatrix {
  state: "draft" | "applied";
  appliedAt: string | null;
  appliedBy: string | null;
  appliedReason: string | null;
  permissions: PermissionCatalogItem[];
  roles: PermissionRoleGrant[];
}

export interface ApplyPermissionMatrixInput {
  reason: string;
  roles: Array<{
    roleId: string;
    permissionKeys: string[];
  }>;
}

export interface SystemSettings {
  sessionIdleTimeoutMinutes: number;
  sessionWarningMinutes: number;
  redisSessionTtlMinutes: number;
  redisUserCacheTtlMinutes: number;
  permissionMatrixCacheTtlMinutes: number;
  keycloakRoleMappingRequired: boolean;
  updatedAt: string | null;
  updatedBy: string | null;
  reason: string | null;
}

export interface UpdateSystemSettingsInput {
  reason: string;
  sessionIdleTimeoutMinutes: number;
  sessionWarningMinutes: number;
  redisSessionTtlMinutes: number;
  redisUserCacheTtlMinutes: number;
  permissionMatrixCacheTtlMinutes: number;
  keycloakRoleMappingRequired: boolean;
}

export interface ProjectListItem {
  id: string;
  code: string;
  name: string;
  projectType: string;
  ownerUserId: string | null;
  ownerDisplayName: string | null;
  sponsorDisplayName: string | null;
  phase: string | null;
  status: string;
  plannedStartAt: string | null;
  startAt: string | null;
  endAt: string | null;
  createdAt: string;
}

export interface Project {
  id: string;
  code: string;
  name: string;
  projectType: string;
  ownerUserId: string | null;
  ownerDisplayName: string | null;
  sponsorUserId: string | null;
  sponsorDisplayName: string | null;
  methodology: string | null;
  phase: string | null;
  status: string;
  statusReason: string | null;
  workflowDefinitionId: string | null;
  documentTemplateId: string | null;
  plannedStartAt: string | null;
  plannedEndAt: string | null;
  startAt: string | null;
  endAt: string | null;
  createdAt: string;
  updatedAt: string | null;
  deletedReason: string | null;
  deletedBy: string | null;
  deletedAt: string | null;
}

export interface ProjectHistoryItem {
  id: string;
  projectId: string;
  eventType: string;
  summary: string | null;
  reason: string | null;
  actorUserId: string | null;
  actorEmail: string | null;
  actorDisplayName: string | null;
  beforeJson: string | null;
  afterJson: string | null;
  metadataJson: string | null;
  occurredAt: string;
}

export interface ProjectHistoryListInput {
  page?: number;
  pageSize?: number;
  search?: string;
}

export interface ProjectRole {
  id: string;
  projectId: string | null;
  projectName: string | null;
  name: string;
  code: string | null;
  description: string | null;
  responsibilities: string | null;
  authorityScope: string | null;
  displayOrder: number;
  createdAt: string;
  updatedAt: string | null;
  deletedReason: string | null;
  deletedBy: string | null;
  deletedAt: string | null;
}

export interface ProjectAssignment {
  id: string;
  userId: string;
  userEmail: string | null;
  userDisplayName: string | null;
  projectId: string;
  projectName: string;
  projectRoleId: string;
  projectRoleName: string;
  reportsToUserId: string | null;
  reportsToDisplayName: string | null;
  isPrimary: boolean;
  status: string;
  changeReason: string | null;
  replacedByAssignmentId: string | null;
  startAt: string;
  endAt: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface ProjectOrgChartNode {
  assignmentId: string;
  userId: string;
  userEmail: string | null;
  userDisplayName: string | null;
  projectRoleId: string;
  projectRoleName: string;
  isPrimary: boolean;
  status: string;
  reportsToUserId: string | null;
  startAt: string;
  endAt: string | null;
  children: ProjectOrgChartNode[];
}

export interface ProjectTeamRegisterRow {
  assignmentId: string;
  userId: string;
  userEmail: string | null;
  userDisplayName: string | null;
  projectRoleName: string;
  reportsToDisplayName: string | null;
  isPrimary: boolean;
  status: string;
  startAt: string;
  endAt: string | null;
}

export interface ProjectRoleResponsibilityRow {
  projectRoleId: string;
  projectRoleName: string;
  code: string | null;
  description: string | null;
  responsibilities: string | null;
  authorityScope: string | null;
  memberCount: number;
}

export interface ProjectAssignmentHistoryRow {
  assignmentId: string;
  userId: string;
  userEmail: string | null;
  userDisplayName: string | null;
  projectRoleName: string;
  status: string;
  changeReason: string | null;
  reportsToDisplayName: string | null;
  isPrimary: boolean;
  startAt: string;
  endAt: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface ProjectTypeTemplate {
  id: string;
  projectType: string;
  requireSponsor: boolean;
  requirePlannedPeriod: boolean;
  requireActiveTeam: boolean;
  requirePrimaryAssignment: boolean;
  requireReportingRoot: boolean;
  requireDocumentCreator: boolean;
  requireReviewer: boolean;
  requireApprover: boolean;
  requireReleaseRole: boolean;
  createdAt: string;
  updatedAt: string | null;
  deletedReason: string | null;
  deletedBy: string | null;
  deletedAt: string | null;
}

export interface CreateProjectInput {
  code: string;
  name: string;
  projectType: string;
  ownerUserId?: string;
  sponsorUserId?: string;
  methodology?: string;
  phase?: string;
  status: string;
  statusReason?: string;
  workflowDefinitionId?: string;
  documentTemplateId?: string;
  plannedStartAt?: string;
  plannedEndAt?: string;
  startAt?: string;
  endAt?: string;
}

export interface UpdateProjectInput extends CreateProjectInput {
  id: string;
}

export interface CreateProjectRoleInput {
  projectId?: string;
  name: string;
  code?: string;
  description?: string;
  responsibilities?: string;
  authorityScope?: string;
  displayOrder: number;
}

export interface UpdateProjectRoleInput extends CreateProjectRoleInput {
  id: string;
}

export interface CreateProjectAssignmentInput {
  userId: string;
  projectId: string;
  projectRoleId: string;
  reportsToUserId?: string;
  isPrimary: boolean;
  startAt?: string;
  endAt?: string;
}

export interface UpdateProjectAssignmentInput extends CreateProjectAssignmentInput {
  id: string;
  reason: string;
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
  divisionId?: string;
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

export interface ChangePasswordInput {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}
