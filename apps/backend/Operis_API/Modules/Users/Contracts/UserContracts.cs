using Operis_API.Modules.Users.Domain;

namespace Operis_API.Modules.Users.Contracts;

public sealed record CreateRegistrationRequest(string Email, string FirstName, string LastName, Guid? DivisionId, Guid? DepartmentId, Guid? JobTitleId);
public sealed record ReviewRegistrationRequest(string ReviewedBy);
public sealed record RejectRegistrationRequest(string ReviewedBy, string Reason);
public sealed record CompleteRegistrationPasswordSetupRequest(string Password, string ConfirmPassword);
public sealed record CreateInvitationRequest(string Email, string InvitedBy, DateTimeOffset? ExpiresAt, Guid? DivisionId, Guid? DepartmentId, Guid? JobTitleId);
public sealed record UpdateInvitationRequest(string Email, DateTimeOffset? ExpiresAt, Guid? DivisionId, Guid? DepartmentId, Guid? JobTitleId);
public sealed record AcceptInvitationRequest(string FirstName, string LastName, string Password, string ConfirmPassword);
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
public sealed record CreateUserRequest(string Email, string FirstName, string LastName, string Password, string ConfirmPassword, string CreatedBy, Guid? DivisionId, Guid? DepartmentId, Guid? JobTitleId, IReadOnlyList<Guid>? RoleIds, string? RoleChangeReason = null);
public sealed record UpdateUserRequest(string Email, string FirstName, string LastName, Guid? DivisionId, Guid? DepartmentId, Guid? JobTitleId, IReadOnlyList<Guid>? RoleIds, string? RoleChangeReason = null);
public sealed record UpsertUserOrgAssignmentRequest(Guid? DivisionId, Guid? DepartmentId, Guid? PositionId);
public sealed record UpdateUserPreferencesRequest(string? PreferredLanguage, string? PreferredTheme);
public sealed record CreateMasterDataRequest(string Name, int DisplayOrder);
public sealed record UpdateMasterDataRequest(string Name, int DisplayOrder);
public sealed record CreateDepartmentRequest(string Name, int DisplayOrder, Guid? DivisionId);
public sealed record UpdateDepartmentRequest(string Name, int DisplayOrder, Guid? DivisionId);
public sealed record CreateJobTitleRequest(string Name, int DisplayOrder, Guid? DepartmentId);
public sealed record UpdateJobTitleRequest(string Name, int DisplayOrder, Guid? DepartmentId);
public sealed record CreateMasterDataCatalogRequest(string Domain, string Code, string Name, int DisplayOrder, string Reason);
public sealed record UpdateMasterDataCatalogRequest(string Domain, string Code, string Name, string Status, int DisplayOrder, string Reason);
public sealed record CreateProjectRequest(
    string Code,
    string Name,
    string ProjectType,
    string? OwnerUserId,
    string? SponsorUserId,
    string? Methodology,
    string? Phase,
    string Status,
    string? StatusReason,
    Guid? WorkflowDefinitionId,
    Guid? DocumentTemplateId,
    DateTimeOffset? PlannedStartAt,
    DateTimeOffset? PlannedEndAt,
    DateTimeOffset? StartAt,
    DateTimeOffset? EndAt);
public sealed record UpdateProjectRequest(
    string Code,
    string Name,
    string ProjectType,
    string? OwnerUserId,
    string? SponsorUserId,
    string? Methodology,
    string? Phase,
    string Status,
    string? StatusReason,
    Guid? WorkflowDefinitionId,
    Guid? DocumentTemplateId,
    DateTimeOffset? PlannedStartAt,
    DateTimeOffset? PlannedEndAt,
    DateTimeOffset? StartAt,
    DateTimeOffset? EndAt);
public sealed record CreateProjectRoleRequest(
    Guid? ProjectId,
    string Name,
    string? Code,
    string? Status,
    string? Description,
    string? Responsibilities,
    string? AuthorityScope,
    int DisplayOrder);
public sealed record CreateProjectTypeTemplateRequest(
    string ProjectType,
    bool RequireSponsor,
    bool RequirePlannedPeriod,
    bool RequireActiveTeam,
    bool RequirePrimaryAssignment,
    bool RequireReportingRoot,
    bool RequireDocumentCreator,
    bool RequireReviewer,
    bool RequireApprover,
    bool RequireReleaseRole);
public sealed record UpdateProjectTypeTemplateRequest(
    string ProjectType,
    bool RequireSponsor,
    bool RequirePlannedPeriod,
    bool RequireActiveTeam,
    bool RequirePrimaryAssignment,
    bool RequireReportingRoot,
    bool RequireDocumentCreator,
    bool RequireReviewer,
    bool RequireApprover,
    bool RequireReleaseRole);
public sealed record CreateProjectTypeRoleRequirementRequest(
    Guid ProjectTypeTemplateId,
    string RoleName,
    string? RoleCode,
    string? Description,
    int DisplayOrder);
public sealed record UpdateProjectTypeRoleRequirementRequest(
    Guid ProjectTypeTemplateId,
    string RoleName,
    string? RoleCode,
    string? Description,
    int DisplayOrder);
public sealed record UpdateProjectRoleRequest(
    Guid? ProjectId,
    string Name,
    string? Code,
    string? Status,
    string? Description,
    string? Responsibilities,
    string? AuthorityScope,
    int DisplayOrder);
public sealed record CreateProjectAssignmentRequest(string UserId, Guid ProjectId, Guid ProjectRoleId, string? ReportsToUserId, bool IsPrimary, DateTimeOffset? StartAt, DateTimeOffset? EndAt);
public sealed record UpdateProjectAssignmentRequest(string UserId, Guid ProjectId, Guid ProjectRoleId, string? ReportsToUserId, bool IsPrimary, DateTimeOffset? StartAt, DateTimeOffset? EndAt, string Reason);
public sealed record CreatePhaseApprovalRequest(Guid ProjectId, string PhaseCode, string EntryCriteriaSummary, IReadOnlyList<string> RequiredEvidenceRefs);
public sealed record DecisionPhaseApprovalRequest(string DecisionReason);
public sealed record BaselinePhaseApprovalRequest(string? DecisionReason);
public sealed record SoftDeleteRequest(string Reason);

public sealed record UserResponse(
    string Id,
    UserStatus Status,
    DateTimeOffset CreatedAt,
    string CreatedBy,
    Guid? DivisionId,
    string? DivisionName,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? JobTitleId,
    string? JobTitleName,
    IReadOnlyList<string> Roles,
    string? PreferredLanguage,
    string? PreferredTheme,
    string? DeletedReason,
    string? DeletedBy,
    DateTimeOffset? DeletedAt,
    KeycloakUserSummary? Keycloak);

public sealed record KeycloakUserSummary(
    string Id,
    string Email,
    string Username,
    string? FirstName,
    string? LastName,
    bool Enabled,
    bool EmailVerified);

public sealed record RegistrationRequestResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    Guid? DivisionId,
    string? DivisionName,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? JobTitleId,
    string? JobTitleName,
    RegistrationRequestStatus Status,
    DateTimeOffset RequestedAt,
    DateTimeOffset? ReviewedAt,
    string? ReviewedBy,
    string? RejectionReason,
    string? PasswordSetupLink,
    DateTimeOffset? PasswordSetupExpiresAt,
    DateTimeOffset? PasswordSetupCompletedAt);

public sealed record RegistrationPasswordSetupDetailResponse(
    string Email,
    string FirstName,
    string LastName,
    string? DivisionName,
    string? DepartmentName,
    string? JobTitleName,
    bool IsExpired,
    bool IsCompleted,
    DateTimeOffset? ExpiresAt);

public sealed record InvitationResponse(
    Guid Id,
    string Email,
    string InvitationToken,
    string InvitedBy,
    Guid? DivisionId,
    string? DivisionName,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? JobTitleId,
    string? JobTitleName,
    InvitationStatus Status,
    DateTimeOffset InvitedAt,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? AcceptedAt,
    DateTimeOffset? RejectedAt,
    string InvitationLink);

public sealed record InvitationDetailResponse(
    Guid Id,
    string Email,
    Guid? DivisionId,
    string? DivisionName,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? JobTitleId,
    string? JobTitleName,
    InvitationStatus Status,
    DateTimeOffset InvitedAt,
    DateTimeOffset? ExpiresAt);

public sealed record MasterDataResponse(
    Guid Id,
    string Name,
    int DisplayOrder,
    Guid? DivisionId,
    string? DivisionName,
    Guid? DepartmentId,
    string? DepartmentName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? DeletedReason,
    string? DeletedBy,
    DateTimeOffset? DeletedAt);

public sealed record MasterDataCatalogResponse(
    Guid Id,
    string Domain,
    string Code,
    string Name,
    string Status,
    int DisplayOrder,
    string? LastChangedBy,
    DateTimeOffset? LastChangedAt,
    IReadOnlyList<MasterDataChangeResponse> Changes);

public sealed record MasterDataChangeResponse(
    Guid Id,
    string ChangeType,
    string ChangedBy,
    DateTimeOffset ChangedAt,
    string Reason);

public sealed record AppRoleResponse(Guid Id, string Name, string KeycloakRoleName, string? Description, int DisplayOrder);
public sealed record ProjectListItem(
    Guid Id,
    string Code,
    string Name,
    string ProjectType,
    string? OwnerUserId,
    string? OwnerDisplayName,
    string? SponsorDisplayName,
    string? Phase,
    string Status,
    DateTimeOffset? PlannedStartAt,
    DateTimeOffset? StartAt,
    DateTimeOffset? EndAt,
    DateTimeOffset CreatedAt);
public sealed record ProjectResponse(
    Guid Id,
    string Code,
    string Name,
    string ProjectType,
    string? OwnerUserId,
    string? OwnerDisplayName,
    string? SponsorUserId,
    string? SponsorDisplayName,
    string? Methodology,
    string? Phase,
    string Status,
    string? StatusReason,
    Guid? WorkflowDefinitionId,
    Guid? DocumentTemplateId,
    DateTimeOffset? PlannedStartAt,
    DateTimeOffset? PlannedEndAt,
    DateTimeOffset? StartAt,
    DateTimeOffset? EndAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? DeletedReason,
    string? DeletedBy,
    DateTimeOffset? DeletedAt);
public sealed record ProjectRoleResponse(
    Guid Id,
    Guid? ProjectId,
    string? ProjectName,
    string Name,
    string? Code,
    string Status,
    string? Description,
    string? Responsibilities,
    string? AuthorityScope,
    int AssignedCount,
    int DisplayOrder,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? DeletedReason,
    string? DeletedBy,
    DateTimeOffset? DeletedAt);
public sealed record ProjectAssignmentResponse(
    Guid Id,
    string UserId,
    string? UserEmail,
    string? UserDisplayName,
    Guid ProjectId,
    string ProjectName,
    Guid ProjectRoleId,
    string ProjectRoleName,
    string? ReportsToUserId,
    string? ReportsToDisplayName,
    bool IsPrimary,
    string Status,
    string? ChangeReason,
    Guid? ReplacedByAssignmentId,
    DateTimeOffset StartAt,
    DateTimeOffset? EndAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record ProjectTypeTemplateResponse(
    Guid Id,
    string ProjectType,
    bool RequireSponsor,
    bool RequirePlannedPeriod,
    bool RequireActiveTeam,
    bool RequirePrimaryAssignment,
    bool RequireReportingRoot,
    bool RequireDocumentCreator,
    bool RequireReviewer,
    bool RequireApprover,
    bool RequireReleaseRole,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? DeletedReason,
    string? DeletedBy,
    DateTimeOffset? DeletedAt);

public sealed record ProjectTypeRoleRequirementResponse(
    Guid Id,
    Guid ProjectTypeTemplateId,
    string ProjectType,
    string RoleName,
    string? RoleCode,
    string? Description,
    int DisplayOrder,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? DeletedReason,
    string? DeletedBy,
    DateTimeOffset? DeletedAt);

public sealed record ProjectOrgChartNodeResponse(
    Guid AssignmentId,
    string UserId,
    string? UserEmail,
    string? UserDisplayName,
    Guid ProjectRoleId,
    string ProjectRoleName,
    bool IsPrimary,
    string Status,
    string? ReportsToUserId,
    DateTimeOffset StartAt,
    DateTimeOffset? EndAt,
    IReadOnlyList<ProjectOrgChartNodeResponse> Children);

public sealed record ProjectTeamRegisterRowResponse(
    Guid AssignmentId,
    string UserId,
    string? UserEmail,
    string? UserDisplayName,
    string ProjectRoleName,
    string? ReportsToDisplayName,
    bool IsPrimary,
    string Status,
    DateTimeOffset StartAt,
    DateTimeOffset? EndAt);

public sealed record ProjectRoleResponsibilityRowResponse(
    Guid ProjectRoleId,
    string ProjectRoleName,
    string? Code,
    string? Description,
    string? Responsibilities,
    string? AuthorityScope,
    int MemberCount);

public sealed record ProjectAssignmentHistoryRowResponse(
    Guid AssignmentId,
    string UserId,
    string? UserEmail,
    string? UserDisplayName,
    string ProjectRoleName,
    string Status,
    string? ChangeReason,
    string? ReportsToDisplayName,
    bool IsPrimary,
    DateTimeOffset StartAt,
    DateTimeOffset? EndAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record ProjectEvidenceResponse(
    Guid ProjectId,
    string ProjectName,
    IReadOnlyList<ProjectTeamRegisterRowResponse> TeamRegister,
    IReadOnlyList<ProjectRoleResponsibilityRowResponse> RoleResponsibilities,
    IReadOnlyList<ProjectAssignmentHistoryRowResponse> AssignmentHistory);

public sealed record ProjectEvidenceExportResult(
    string FileName,
    string ContentType,
    byte[] Content);

public sealed record ProjectComplianceCheckResponse(
    string Code,
    string Title,
    string Description,
    string Severity,
    string Status,
    string? Detail);

public sealed record ProjectComplianceResponse(
    Guid ProjectId,
    string ProjectName,
    string ProjectType,
    string Status,
    int PassedChecks,
    int WarningChecks,
    int FailedChecks,
    IReadOnlyList<ProjectComplianceCheckResponse> Checks);

public sealed record PhaseApprovalRequestResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string PhaseCode,
    string EntryCriteriaSummary,
    IReadOnlyList<string> RequiredEvidenceRefs,
    string Status,
    string? SubmittedBy,
    string? SubmittedByDisplayName,
    DateTimeOffset? SubmittedAt,
    string? Decision,
    string? DecisionReason,
    string? DecidedBy,
    string? DecidedByDisplayName,
    DateTimeOffset? DecidedAt,
    string? BaselineBy,
    string? BaselineByDisplayName,
    DateTimeOffset? BaselinedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
