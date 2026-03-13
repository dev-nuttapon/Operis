using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Infrastructure;

namespace Operis_API.Tests.Support;

internal sealed class TestReferenceDataCache : IReferenceDataCache
{
    public int InvalidateDivisionsCalls { get; private set; }
    public int InvalidateDepartmentsCalls { get; private set; }
    public int InvalidateJobTitlesCalls { get; private set; }
    public int InvalidateProjectRolesCalls { get; private set; }
    public int InvalidateAppRolesCalls { get; private set; }

    public IReadOnlyList<CachedDivisionItem> Divisions { get; init; } = [];
    public IReadOnlyList<CachedDepartmentItem> Departments { get; init; } = [];
    public IReadOnlyList<CachedJobTitleItem> JobTitles { get; init; } = [];
    public IReadOnlyList<CachedProjectRoleItem> ProjectRoles { get; init; } = [];
    public IReadOnlyList<CachedAppRoleItem> AppRoles { get; init; } = [];

    public Task<IReadOnlyList<CachedDivisionItem>> GetDivisionsAsync(OperisDbContext dbContext, CancellationToken cancellationToken) =>
        Task.FromResult(Divisions);

    public Task<IReadOnlyList<CachedDepartmentItem>> GetDepartmentsAsync(OperisDbContext dbContext, CancellationToken cancellationToken) =>
        Task.FromResult(Departments);

    public Task<IReadOnlyList<CachedJobTitleItem>> GetJobTitlesAsync(OperisDbContext dbContext, CancellationToken cancellationToken) =>
        Task.FromResult(JobTitles);

    public Task<IReadOnlyList<CachedProjectRoleItem>> GetProjectRolesAsync(OperisDbContext dbContext, CancellationToken cancellationToken) =>
        Task.FromResult(ProjectRoles);

    public Task<IReadOnlyList<CachedAppRoleItem>> GetAppRolesAsync(OperisDbContext dbContext, CancellationToken cancellationToken) =>
        Task.FromResult(AppRoles);

    public Task InvalidateDivisionsAsync(CancellationToken cancellationToken)
    {
        InvalidateDivisionsCalls++;
        return Task.CompletedTask;
    }

    public Task InvalidateDepartmentsAsync(CancellationToken cancellationToken)
    {
        InvalidateDepartmentsCalls++;
        return Task.CompletedTask;
    }

    public Task InvalidateJobTitlesAsync(CancellationToken cancellationToken)
    {
        InvalidateJobTitlesCalls++;
        return Task.CompletedTask;
    }

    public Task InvalidateProjectRolesAsync(CancellationToken cancellationToken)
    {
        InvalidateProjectRolesCalls++;
        return Task.CompletedTask;
    }

    public Task InvalidateAppRolesAsync(CancellationToken cancellationToken)
    {
        InvalidateAppRolesCalls++;
        return Task.CompletedTask;
    }
}
