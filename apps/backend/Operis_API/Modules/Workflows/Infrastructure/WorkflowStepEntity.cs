namespace Operis_API.Modules.Workflows.Infrastructure;

public sealed class WorkflowStepEntity
{
    public Guid Id { get; set; }
    public Guid WorkflowDefinitionId { get; set; }
    public Guid? DocumentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StepType { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsRequired { get; set; }
    public int MinApprovals { get; set; } = 1;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
