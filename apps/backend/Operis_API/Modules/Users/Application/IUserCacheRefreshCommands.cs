namespace Operis_API.Modules.Users.Application;

public interface IUserCacheRefreshCommands
{
    Task<int> RefreshDepartmentsAsync(CancellationToken cancellationToken);
    Task<int> RefreshDivisionsAsync(CancellationToken cancellationToken);
    Task<int> RefreshJobTitlesAsync(CancellationToken cancellationToken);
    Task<int> RefreshProjectRolesAsync(CancellationToken cancellationToken);
}
