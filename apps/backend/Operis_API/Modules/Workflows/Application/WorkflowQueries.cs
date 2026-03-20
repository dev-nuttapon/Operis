using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Workflows.Infrastructure;
using Operis_API.Shared.Auditing;

namespace Operis_API.Modules.Workflows;

public sealed class WorkflowQueries(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter) : IWorkflowQueries
{
    public async Task<WorkflowDefinitionListResponse> ListDefinitionsAsync(
        WorkflowDefinitionListQuery query,
        CancellationToken cancellationToken)
    {
        var source = dbContext.WorkflowDefinitions.AsNoTracking();
        var statusSummary = await BuildStatusSummaryAsync(source, cancellationToken);
        var normalizedStatus = NormalizeStatus(query.Status);
        if (!string.IsNullOrWhiteSpace(normalizedStatus))
        {
            source = source.Where(x => x.Status == normalizedStatus);
        }

        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var total = await source.CountAsync(cancellationToken);

        var definitions = await source
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new WorkflowDefinitionContract(
                x.Id,
                x.Code,
                x.Name,
                x.Status))
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "workflows",
            Action: "list",
            EntityType: "workflow_definition",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { count = definitions.Count, total, page, pageSize, status = normalizedStatus }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new WorkflowDefinitionListResponse(definitions, total, page, pageSize, statusSummary);
    }

    public async Task<WorkflowDefinitionDetailContract?> GetDefinitionAsync(Guid workflowDefinitionId, CancellationToken cancellationToken)
    {
        var definition = await dbContext.WorkflowDefinitions
            .AsNoTracking()
            .Where(x => x.Id == workflowDefinitionId)
            .Select(x => new WorkflowDefinitionContract(x.Id, x.Code, x.Name, x.Status))
            .SingleOrDefaultAsync(cancellationToken);

        if (definition is null)
        {
            return null;
        }

        var steps = await dbContext.WorkflowSteps
            .AsNoTracking()
            .Where(x => x.WorkflowDefinitionId == workflowDefinitionId)
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.StepType,
                x.DisplayOrder,
                x.IsRequired
            })
            .ToListAsync(cancellationToken);

        var stepIds = steps.Select(x => x.Id).ToList();
        var stepRoles = stepIds.Count == 0
            ? new Dictionary<Guid, IReadOnlyList<Guid>>()
            : await dbContext.WorkflowStepRoles
                .AsNoTracking()
                .Where(x => stepIds.Contains(x.WorkflowStepId))
                .GroupBy(x => x.WorkflowStepId)
                .Select(group => new
                {
                    StepId = group.Key,
                    RoleIds = (IReadOnlyList<Guid>)group.Select(x => x.ProjectRoleId).ToList()
                })
                .ToDictionaryAsync(x => x.StepId, x => x.RoleIds, cancellationToken);

        var stepContracts = steps
            .Select(step => new WorkflowStepContract(
                step.Id,
                step.Name,
                step.StepType,
                step.DisplayOrder,
                step.IsRequired,
                stepRoles.TryGetValue(step.Id, out var roleIds) ? roleIds : []))
            .ToList();

        auditLogWriter.Append(new AuditLogEntry(
            Module: "workflows",
            Action: "get",
            EntityType: "workflow_definition",
            EntityId: workflowDefinitionId.ToString(),
            StatusCode: StatusCodes.Status200OK));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new WorkflowDefinitionDetailContract(
            definition.Id,
            definition.Code,
            definition.Name,
            definition.Status,
            stepContracts);
    }

    private static (int Page, int PageSize, int Skip) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = Math.Clamp(pageSize, 5, 100);
        return (normalizedPage, normalizedPageSize, (normalizedPage - 1) * normalizedPageSize);
    }

    private static string? NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        return status.Trim().ToLowerInvariant();
    }

    private static async Task<WorkflowDefinitionStatusSummary> BuildStatusSummaryAsync(
        IQueryable<WorkflowDefinitionEntity> source,
        CancellationToken cancellationToken)
    {
        var counts = await source
            .GroupBy(x => x.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var draft = counts.FirstOrDefault(x => x.Status == "draft")?.Count ?? 0;
        var active = counts.FirstOrDefault(x => x.Status == "active")?.Count ?? 0;
        var archived = counts.FirstOrDefault(x => x.Status == "archived")?.Count ?? 0;
        var total = draft + active + archived;

        return new WorkflowDefinitionStatusSummary(total, draft, active, archived);
    }
}
