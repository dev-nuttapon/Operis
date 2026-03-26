namespace Operis_API.Modules.Governance.Infrastructure;

public sealed class ProcessAssetEntity
{
    public Guid Id { get; init; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
    public string OwnerUserId { get; set; } = string.Empty;
    public DateTimeOffset? EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public Guid? CurrentVersionId { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}
