using Operis_API.Modules.Users.Domain;

namespace Operis_API.Modules.Users.Contracts;

public sealed record CreateRegistrationRequest(string Email, string FirstName, string LastName, Guid? DivisionId, Guid? DepartmentId, Guid? JobTitleId);
public sealed record ReviewRegistrationRequest(string ReviewedBy);
public sealed record RejectRegistrationRequest(string ReviewedBy, string Reason);
public sealed record CompleteRegistrationPasswordSetupRequest(string Password, string ConfirmPassword);
public sealed record CreateInvitationRequest(string Email, string InvitedBy, DateTimeOffset? ExpiresAt, Guid? DivisionId, Guid? DepartmentId, Guid? JobTitleId);
public sealed record UpdateInvitationRequest(string Email, DateTimeOffset? ExpiresAt, Guid? DivisionId, Guid? DepartmentId, Guid? JobTitleId);
public sealed record AcceptInvitationRequest(string FirstName, string LastName, string Password, string ConfirmPassword);
public sealed record CreateUserRequest(string Email, string FirstName, string LastName, string Password, string ConfirmPassword, string CreatedBy, Guid? DivisionId, Guid? DepartmentId, Guid? JobTitleId, IReadOnlyList<Guid>? RoleIds);
public sealed record UpdateUserRequest(string Email, string FirstName, string LastName, Guid? DivisionId, Guid? DepartmentId, Guid? JobTitleId, IReadOnlyList<Guid>? RoleIds);
public sealed record UpsertUserOrgAssignmentRequest(Guid? DivisionId, Guid? DepartmentId, Guid? PositionId);
public sealed record UpdateUserPreferencesRequest(string? PreferredLanguage, string? PreferredTheme);
public sealed record CreateMasterDataRequest(string Name, int DisplayOrder);
public sealed record UpdateMasterDataRequest(string Name, int DisplayOrder);
public sealed record CreateDepartmentRequest(string Name, int DisplayOrder, Guid? DivisionId);
public sealed record UpdateDepartmentRequest(string Name, int DisplayOrder, Guid? DivisionId);
public sealed record CreateJobTitleRequest(string Name, int DisplayOrder, Guid? DepartmentId);
public sealed record UpdateJobTitleRequest(string Name, int DisplayOrder, Guid? DepartmentId);
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
    DateTimeOffset? PlannedStartAt,
    DateTimeOffset? PlannedEndAt,
    DateTimeOffset? StartAt,
    DateTimeOffset? EndAt);
public sealed record CreateProjectRoleRequest(
    Guid ProjectId,
    string Name,
    string? Code,
    string? Description,
    string? Responsibilities,
    string? AuthorityScope,
    bool IsReviewRole,
    bool IsApprovalRole,
    int DisplayOrder);
public sealed record UpdateProjectRoleRequest(
    Guid ProjectId,
    string Name,
    string? Code,
    string? Description,
    string? Responsibilities,
    string? AuthorityScope,
    bool IsReviewRole,
    bool IsApprovalRole,
    int DisplayOrder);
public sealed record CreateProjectAssignmentRequest(string UserId, Guid ProjectId, Guid ProjectRoleId, string? ReportsToUserId, bool IsPrimary, DateTimeOffset? StartAt, DateTimeOffset? EndAt);
public sealed record UpdateProjectAssignmentRequest(string UserId, Guid ProjectId, Guid ProjectRoleId, string? ReportsToUserId, bool IsPrimary, DateTimeOffset? StartAt, DateTimeOffset? EndAt, string Reason);
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

public sealed record AppRoleResponse(Guid Id, string Name, string KeycloakRoleName, string? Description, int DisplayOrder);
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
    string? Description,
    string? Responsibilities,
    string? AuthorityScope,
    bool IsReviewRole,
    bool IsApprovalRole,
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
