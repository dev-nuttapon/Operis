namespace Operis_API.Modules.Workflows.Infrastructure;

public sealed class WorkflowStepRouteEntity
{
    public Guid Id { get; set; }
    public Guid WorkflowStepId { get; set; }
    public string Action { get; set; } = string.Empty;
    public Guid? NextStepId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
