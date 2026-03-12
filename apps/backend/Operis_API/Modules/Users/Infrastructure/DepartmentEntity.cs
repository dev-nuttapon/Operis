namespace Operis_API.Modules.Users.Infrastructure;

public sealed class DepartmentEntity
{
    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
