namespace Operis_API.Modules.Users.Infrastructure;

public sealed class ProjectTypeRoleRequirementEntity
{
    public Guid Id { get; init; }
    public Guid ProjectTypeTemplateId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? RoleCode { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? DeletedReason { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
