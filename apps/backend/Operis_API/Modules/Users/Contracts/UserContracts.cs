using Operis_API.Modules.Users.Domain;

namespace Operis_API.Modules.Users.Contracts;

public sealed record CreateRegistrationRequest(string Email, string FirstName, string LastName);
public sealed record ReviewRegistrationRequest(string ReviewedBy);
public sealed record RejectRegistrationRequest(string ReviewedBy, string Reason);
public sealed record CreateInvitationRequest(string Email, string InvitedBy, int? ExpiresInDays);
public sealed record CreateUserRequest(string Email, string FirstName, string LastName, string CreatedBy);

public sealed record UserResponse(
    Guid Id,
    string? KeycloakUserId,
    string Email,
    string FirstName,
    string LastName,
    UserStatus Status,
    DateTimeOffset CreatedAt,
    string CreatedBy,
    DateTimeOffset? ApprovedAt,
    KeycloakUserSummary? Keycloak);

public sealed record KeycloakUserSummary(
    string Id,
    string Email,
    string Username,
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
    string InvitedBy,
    InvitationStatus Status,
    DateTimeOffset InvitedAt,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? AcceptedAt,
    DateTimeOffset? RejectedAt);
