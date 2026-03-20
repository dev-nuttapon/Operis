using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Workflows;

public interface IWorkflowTaskQueries
{
    Task<PagedResult<WorkflowTaskListItem>> ListTasksAsync(
        WorkflowTaskListQuery query,
        string? currentUserId,
        CancellationToken cancellationToken);
}
