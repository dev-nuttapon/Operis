namespace Operis_API.Modules.Workflows;

public interface IWorkflowCacheCommands
{
    Task<int> RefreshDefinitionsAsync(CancellationToken cancellationToken);
}
