using Operis_API.Modules.Users.Domain;

namespace Operis_API.Modules.Users.Infrastructure;

public sealed class UserInvitationEntity
{
    public Guid Id { get; init; }
    public string Email { get; set; } = string.Empty;
    public string InvitationToken { get; init; } = string.Empty;
    public string InvitedBy { get; init; } = string.Empty;
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTimeOffset InvitedAt { get; init; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }
}
