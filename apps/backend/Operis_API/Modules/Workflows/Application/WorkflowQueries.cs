using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Workflows.Infrastructure;
using Operis_API.Shared.Auditing;

namespace Operis_API.Modules.Workflows;

public sealed class WorkflowQueries(
    OperisDbContext dbContext,
    IWorkflowDefinitionCache definitionCache,
    IAuditLogWriter auditLogWriter) : IWorkflowQueries
{
    public async Task<WorkflowDefinitionListResponse> ListDefinitionsAsync(
        WorkflowDefinitionListQuery query,
        CancellationToken cancellationToken)
    {
        var normalizedStatus = NormalizeStatus(query.Status);
        var definitions = await definitionCache.GetDefinitionsAsync(dbContext, cancellationToken);
        var statusSummary = BuildStatusSummary(definitions);

        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var filtered = string.IsNullOrWhiteSpace(normalizedStatus)
            ? definitions
            : definitions.Where(x => string.Equals(x.Status, normalizedStatus, StringComparison.OrdinalIgnoreCase)).ToList();
        var total = filtered.Count;
        var pagedDefinitions = filtered
            .Skip(skip)
            .Take(pageSize)
            .ToList();

        auditLogWriter.Append(new AuditLogEntry(
            Module: "workflows",
            Action: "list",
            EntityType: "workflow_definition",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { count = pagedDefinitions.Count, total, page, pageSize, status = normalizedStatus }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new WorkflowDefinitionListResponse(pagedDefinitions, total, page, pageSize, statusSummary);
    }

    public async Task<WorkflowDefinitionDetailContract?> GetDefinitionAsync(Guid workflowDefinitionId, CancellationToken cancellationToken)
    {
        var definition = await dbContext.WorkflowDefinitions
            .AsNoTracking()
            .Where(x => x.Id == workflowDefinitionId)
            .Select(x => new WorkflowDefinitionContract(x.Id, x.Code, x.Name, x.Status, x.DocumentTemplateId))
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
                x.DocumentId,
                x.Name,
                x.StepType,
                x.DisplayOrder,
                x.IsRequired,
                x.MinApprovals
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

        var stepRoutes = new Dictionary<Guid, IReadOnlyList<WorkflowStepRouteContract>>();
        if (stepIds.Count > 0)
        {
            var routeEntities = await dbContext.WorkflowStepRoutes
                .AsNoTracking()
                .Where(x => stepIds.Contains(x.WorkflowStepId))
                .ToListAsync(cancellationToken);

            var orderById = steps.ToDictionary(step => step.Id, step => step.DisplayOrder);
            stepRoutes = routeEntities
                .GroupBy(route => route.WorkflowStepId)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<WorkflowStepRouteContract>)group
                        .Select(route => new WorkflowStepRouteContract(
                            route.Action,
                            route.NextStepId,
                            route.NextStepId.HasValue && orderById.TryGetValue(route.NextStepId.Value, out var order) ? order : null))
                        .ToList());
        }

        var stepContracts = steps
            .Select(step => new WorkflowStepContract(
                step.Id,
                step.Name,
                step.StepType,
                step.DisplayOrder,
                step.IsRequired,
                step.DocumentId,
                step.MinApprovals,
                stepRoles.TryGetValue(step.Id, out var roleIds) ? roleIds : [],
                stepRoutes.TryGetValue(step.Id, out var routes) ? routes : []))
            .ToList();

        var hasInstances = await dbContext.WorkflowInstances
            .AsNoTracking()
            .AnyAsync(x => x.WorkflowDefinitionId == workflowDefinitionId, cancellationToken);

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
            definition.DocumentTemplateId,
            hasInstances,
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

    private static WorkflowDefinitionStatusSummary BuildStatusSummary(
        IReadOnlyList<WorkflowDefinitionContract> definitions)
    {
        var all = definitions.Count;
        var draft = definitions.Count(x => string.Equals(x.Status, "Draft", StringComparison.OrdinalIgnoreCase));
        var active = definitions.Count(x => string.Equals(x.Status, "Active", StringComparison.OrdinalIgnoreCase));
        var archived = definitions.Count(x => string.Equals(x.Status, "Archived", StringComparison.OrdinalIgnoreCase));
        return new WorkflowDefinitionStatusSummary(all, draft, active, archived);
    }

}
