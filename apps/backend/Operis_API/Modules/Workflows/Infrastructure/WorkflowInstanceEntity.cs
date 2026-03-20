namespace Operis_API.Modules.Workflows.Infrastructure;

public sealed class WorkflowInstanceEntity
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid DocumentId { get; set; }
    public Guid WorkflowDefinitionId { get; set; }
    public string Status { get; set; } = "in_progress";
    public int CurrentStepOrder { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class WorkflowInstanceStepEntity
{
    public Guid Id { get; set; }
    public Guid WorkflowInstanceId { get; set; }
    public Guid WorkflowStepId { get; set; }
    public string StepType { get; set; } = "submit";
    public int DisplayOrder { get; set; }
    public bool IsRequired { get; set; }
    public string Status { get; set; } = "pending";
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class WorkflowInstanceActionEntity
{
    public Guid Id { get; set; }
    public Guid WorkflowInstanceStepId { get; set; }
    public string Action { get; set; } = "submit";
    public string? ActorUserId { get; set; }
    public string? ActorEmail { get; set; }
    public string? ActorDisplayName { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
