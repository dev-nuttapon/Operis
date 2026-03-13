using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Shared.Auditing;

namespace Operis_API.Modules.Workflows;

public sealed class WorkflowQueries(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter) : IWorkflowQueries
{
    public async Task<IReadOnlyList<WorkflowDefinitionContract>> ListDefinitionsAsync(CancellationToken cancellationToken)
    {
        var definitions = await dbContext.WorkflowDefinitions
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new WorkflowDefinitionContract(
                x.Id,
                x.Code,
                x.Name,
                x.Status))
            .ToListAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "workflows",
            Action: "list",
            EntityType: "workflow_definition",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { count = definitions.Count }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return definitions;
    }
}
