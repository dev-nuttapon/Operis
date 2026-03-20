using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Workflows.Infrastructure;

namespace Operis_API.Modules.Workflows;

public sealed class WorkflowInstanceQueries(OperisDbContext dbContext) : IWorkflowInstanceQueries
{
    public Task<WorkflowInstanceDetailContract?> GetInstanceAsync(Guid workflowInstanceId, CancellationToken cancellationToken) =>
        LoadInstanceAsync(dbContext.WorkflowInstances.AsNoTracking().Where(x => x.Id == workflowInstanceId), cancellationToken);

    public Task<WorkflowInstanceDetailContract?> GetInstanceByDocumentAsync(Guid documentId, CancellationToken cancellationToken) =>
        LoadInstanceAsync(dbContext.WorkflowInstances.AsNoTracking().Where(x => x.DocumentId == documentId), cancellationToken);

    private async Task<WorkflowInstanceDetailContract?> LoadInstanceAsync(
        IQueryable<WorkflowInstanceEntity> source,
        CancellationToken cancellationToken)
    {
        var instance = await source.FirstOrDefaultAsync(cancellationToken);
        if (instance is null)
        {
            return null;
        }

        var steps = await dbContext.WorkflowInstanceSteps
            .AsNoTracking()
            .Where(x => x.WorkflowInstanceId == instance.Id)
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new WorkflowInstanceStepContract(
                x.Id,
                x.WorkflowStepId,
                x.StepType,
                x.DisplayOrder,
                x.IsRequired,
                x.Status,
                x.StartedAt,
                x.CompletedAt,
                dbContext.WorkflowStepRoles
                    .Where(role => role.WorkflowStepId == x.WorkflowStepId)
                    .Select(role => role.ProjectRoleId)
                    .ToList()))
            .ToListAsync(cancellationToken);

        var actions = await dbContext.WorkflowInstanceActions
            .AsNoTracking()
            .Where(x => dbContext.WorkflowInstanceSteps
                .Where(step => step.WorkflowInstanceId == instance.Id)
                .Select(step => step.Id)
                .Contains(x.WorkflowInstanceStepId))
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new WorkflowInstanceActionContract(
                x.Id,
                x.WorkflowInstanceStepId,
                x.Action,
                x.ActorUserId,
                x.ActorEmail,
                x.ActorDisplayName,
                x.Comment,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new WorkflowInstanceDetailContract(
            new WorkflowInstanceContract(
                instance.Id,
                instance.ProjectId,
                instance.DocumentId,
                instance.WorkflowDefinitionId,
                instance.Status,
                instance.CurrentStepOrder,
                instance.StartedAt,
                instance.CompletedAt,
                instance.CreatedAt),
            steps,
            actions);
    }
}
