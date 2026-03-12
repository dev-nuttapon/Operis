using Operis_API.Modules.Users.Domain;

namespace Operis_API.Modules.Users.Infrastructure;

public sealed class UserRegistrationRequestEntity
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public Guid? JobTitleId { get; set; }
    public string? ProvisionedUserId { get; set; }
    public string? PasswordSetupToken { get; set; }
    public DateTimeOffset? PasswordSetupExpiresAt { get; set; }
    public DateTimeOffset? PasswordSetupCompletedAt { get; set; }
    public RegistrationRequestStatus Status { get; set; } = RegistrationRequestStatus.Pending;
    public DateTimeOffset RequestedAt { get; init; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public string? RejectionReason { get; set; }
}
