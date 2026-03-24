using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;

namespace Operis_API.Modules.Workflows;

public sealed record WorkflowProjectStatusSummary(int InProgress, int Completed, int Total);

public interface IWorkflowProjectStatusQueries
{
    Task<IReadOnlyDictionary<Guid, WorkflowProjectStatusSummary>> GetProjectStatusSummaryAsync(
        IEnumerable<Guid> projectIds,
        CancellationToken cancellationToken);

    Task<WorkflowProjectStatusSummary?> GetProjectStatusSummaryAsync(
        Guid projectId,
        CancellationToken cancellationToken);
}

public sealed class WorkflowProjectStatusQueries(OperisDbContext dbContext) : IWorkflowProjectStatusQueries
{
    public async Task<IReadOnlyDictionary<Guid, WorkflowProjectStatusSummary>> GetProjectStatusSummaryAsync(
        IEnumerable<Guid> projectIds,
        CancellationToken cancellationToken)
    {
        var ids = projectIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        if (ids.Length == 0)
        {
            return new Dictionary<Guid, WorkflowProjectStatusSummary>();
        }

        var summaries = await dbContext.WorkflowInstances
            .AsNoTracking()
            .Where(x => ids.Contains(x.ProjectId))
            .GroupBy(x => x.ProjectId)
            .Select(group => new
            {
                ProjectId = group.Key,
                InProgress = group.Count(x => x.Status == "in_progress"),
                Completed = group.Count(x => x.Status == "completed"),
                Total = group.Count()
            })
            .ToListAsync(cancellationToken);

        return summaries.ToDictionary(
            item => item.ProjectId,
            item => new WorkflowProjectStatusSummary(item.InProgress, item.Completed, item.Total));
    }

    public async Task<WorkflowProjectStatusSummary?> GetProjectStatusSummaryAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        if (projectId == Guid.Empty)
        {
            return null;
        }

        return await dbContext.WorkflowInstances
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId)
            .GroupBy(x => x.ProjectId)
            .Select(group => new WorkflowProjectStatusSummary(
                group.Count(x => x.Status == "in_progress"),
                group.Count(x => x.Status == "completed"),
                group.Count()))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
