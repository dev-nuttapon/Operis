namespace Operis_API.Modules.Governance.Infrastructure;

public sealed class ProjectPlanEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ScopeSummary { get; set; } = string.Empty;
    public string LifecycleModel { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly TargetEndDate { get; set; }
    public string OwnerUserId { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
    public string MilestonesJson { get; set; } = "[]";
    public string RolesJson { get; set; } = "[]";
    public string RiskApproach { get; set; } = string.Empty;
    public string QualityApproach { get; set; } = string.Empty;
    public string? ApprovalReason { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}
