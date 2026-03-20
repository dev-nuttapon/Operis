namespace Operis_API.Modules.Workflows.Infrastructure;

public sealed class WorkflowDefinitionEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
    public Guid? DocumentTemplateId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
