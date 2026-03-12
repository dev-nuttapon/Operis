namespace Operis_API.Modules.Users.Infrastructure;

public sealed class AppRoleEntity
{
    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string KeycloakRoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? DeletedReason { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
