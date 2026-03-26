namespace Operis_API.Modules.Governance.Infrastructure;

public sealed class ProcessAssetVersionEntity
{
    public Guid Id { get; init; }
    public Guid ProcessAssetId { get; set; }
    public int VersionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? ContentRef { get; set; }
    public string Status { get; set; } = "draft";
    public string? ChangeSummary { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}
