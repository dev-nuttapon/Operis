namespace Operis_API.Modules.Governance.Infrastructure;

public sealed class TailoringRecordEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; set; }
    public Guid? TailoringCriteriaId { get; set; }
    public Guid? TailoringReviewCycleId { get; set; }
    public string RequesterUserId { get; set; } = string.Empty;
    public string RequestedChange { get; set; } = string.Empty;
    public string StandardReference { get; set; } = string.Empty;
    public string DeviationReason { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string ImpactSummary { get; set; } = string.Empty;
    public DateTimeOffset? ReviewDueAt { get; set; }
    public string Status { get; set; } = "draft";
    public string? ApproverUserId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public Guid? ImpactedProcessAssetId { get; set; }
    public string? ApprovalRationale { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}
