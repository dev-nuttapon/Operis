using Operis_API.Modules.Users.Domain;

namespace Operis_API.Modules.Users.Infrastructure;

public sealed class UserEntity
{
    public string Id { get; init; } = string.Empty;
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DateTimeOffset CreatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public Guid? JobTitleId { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? PreferredTheme { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
