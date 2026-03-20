namespace Operis_API.Modules.Users.Infrastructure;

public sealed class ProjectEntity
{
    public Guid Id { get; init; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ProjectType { get; set; } = "Internal";
    public string? OwnerUserId { get; set; }
    public string? SponsorUserId { get; set; }
    public string? Methodology { get; set; }
    public string? Phase { get; set; }
    public string Status { get; set; } = "active";
    public string? StatusReason { get; set; }
    public Guid? WorkflowDefinitionId { get; set; }
    public Guid? DocumentTemplateId { get; set; }
    public DateTimeOffset? PlannedStartAt { get; set; }
    public DateTimeOffset? PlannedEndAt { get; set; }
    public DateTimeOffset? StartAt { get; set; }
    public DateTimeOffset? EndAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? DeletedReason { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
