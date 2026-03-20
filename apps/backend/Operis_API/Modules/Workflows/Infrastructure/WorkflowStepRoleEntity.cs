namespace Operis_API.Modules.Workflows.Infrastructure;

public sealed class WorkflowStepRoleEntity
{
    public Guid Id { get; set; }
    public Guid WorkflowStepId { get; set; }
    public Guid ProjectRoleId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
