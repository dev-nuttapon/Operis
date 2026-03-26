namespace Operis_API.Modules.Users.Infrastructure;

public sealed class PhaseApprovalRequestEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; set; }
    public string PhaseCode { get; set; } = string.Empty;
    public string EntryCriteriaSummary { get; set; } = string.Empty;
    public string RequiredEvidenceRefsJson { get; set; } = "[]";
    public string Status { get; set; } = "Draft";
    public string? SubmittedBy { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public string? Decision { get; set; }
    public string? DecisionReason { get; set; }
    public string? DecidedBy { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }
    public string? BaselineBy { get; set; }
    public DateTimeOffset? BaselinedAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
