namespace Operis_API.Modules.Governance.Infrastructure;

public sealed class TailoringRecordEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; set; }
    public string RequesterUserId { get; set; } = string.Empty;
    public string RequestedChange { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string ImpactSummary { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
    public string? ApproverUserId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public Guid? ImpactedProcessAssetId { get; set; }
    public string? ApprovalRationale { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}
