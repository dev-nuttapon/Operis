using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Infrastructure;

namespace Operis_API.Modules.Users.Application;

public sealed class UserCacheRefreshCommands(OperisDbContext dbContext, IReferenceDataCache referenceDataCache) : IUserCacheRefreshCommands
{
    public Task<int> RefreshDepartmentsAsync(CancellationToken cancellationToken) =>
        referenceDataCache.RefreshDepartmentsAsync(dbContext, cancellationToken);

    public Task<int> RefreshDivisionsAsync(CancellationToken cancellationToken) =>
        referenceDataCache.RefreshDivisionsAsync(dbContext, cancellationToken);

    public Task<int> RefreshJobTitlesAsync(CancellationToken cancellationToken) =>
        referenceDataCache.RefreshJobTitlesAsync(dbContext, cancellationToken);

    public Task<int> RefreshProjectRolesAsync(CancellationToken cancellationToken) =>
        referenceDataCache.RefreshProjectRolesAsync(dbContext, cancellationToken);
}
