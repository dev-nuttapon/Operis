using Operis_API.Modules.Users.Domain;

namespace Operis_API.Modules.Users.Contracts;

public sealed record CreateRegistrationRequest(string Email, string FirstName, string LastName);
public sealed record ReviewRegistrationRequest(string ReviewedBy);
public sealed record RejectRegistrationRequest(string ReviewedBy, string Reason);
public sealed record CreateInvitationRequest(string Email, string InvitedBy, DateTimeOffset? ExpiresAt, Guid? DepartmentId, Guid? JobTitleId);
public sealed record UpdateInvitationRequest(string Email, DateTimeOffset? ExpiresAt, Guid? DepartmentId, Guid? JobTitleId);
public sealed record AcceptInvitationRequest(string FirstName, string LastName, string Password, string ConfirmPassword);
public sealed record CreateUserRequest(string Email, string FirstName, string LastName, string Password, string ConfirmPassword, string CreatedBy, Guid? DepartmentId, Guid? JobTitleId, IReadOnlyList<Guid>? RoleIds);
public sealed record UpdateUserRequest(string Email, string FirstName, string LastName, Guid? DepartmentId, Guid? JobTitleId, IReadOnlyList<Guid>? RoleIds);
public sealed record UpdateUserPreferencesRequest(string? PreferredLanguage, string? PreferredTheme);
public sealed record CreateMasterDataRequest(string Name, int DisplayOrder);
public sealed record UpdateMasterDataRequest(string Name, int DisplayOrder);
public sealed record SoftDeleteRequest(string Reason);

public sealed record UserResponse(
    string Id,
    UserStatus Status,
    DateTimeOffset CreatedAt,
    string CreatedBy,
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
    RegistrationRequestStatus Status,
    DateTimeOffset RequestedAt,
    DateTimeOffset? ReviewedAt,
    string? ReviewedBy,
    string? RejectionReason);

public sealed record InvitationResponse(
    Guid Id,
    string Email,
    string InvitationToken,
    string InvitedBy,
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
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? DeletedReason,
    string? DeletedBy,
    DateTimeOffset? DeletedAt);

public sealed record AppRoleResponse(Guid Id, string Name, string KeycloakRoleName, string? Description, int DisplayOrder);
