using Operis_API.Modules.Users.Domain;

namespace Operis_API.Modules.Users.Infrastructure;

public sealed class UserEntity
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DateTimeOffset CreatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public DateTimeOffset? ApprovedAt { get; set; }
}
