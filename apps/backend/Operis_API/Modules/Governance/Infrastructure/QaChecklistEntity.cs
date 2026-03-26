namespace Operis_API.Modules.Governance.Infrastructure;

public sealed class QaChecklistEntity
{
    public Guid Id { get; init; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
    public string OwnerUserId { get; set; } = string.Empty;
    public string ItemsJson { get; set; } = "[]";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}
