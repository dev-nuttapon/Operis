using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Operis_API.Infrastructure.Persistence;

namespace Operis_API.Modules.Users.Infrastructure;

public sealed class ReferenceDataCache(IDistributedCache cache) : IReferenceDataCache
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    };

    private const string DivisionsKey = "reference-data:divisions";
    private const string DepartmentsKey = "reference-data:departments";
    private const string JobTitlesKey = "reference-data:job-titles";
    private const string ProjectRolesKey = "reference-data:project-roles";
    private const string AppRolesKey = "reference-data:app-roles";

    public Task<IReadOnlyList<CachedDivisionItem>> GetDivisionsAsync(OperisDbContext dbContext, CancellationToken cancellationToken) =>
        GetOrCreateAsync(
            DivisionsKey,
            async () =>
            {
                var items = await dbContext.Divisions
                    .AsNoTracking()
                    .Where(x => x.DeletedAt == null)
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.Name)
                    .Select(x => new CachedDivisionItem(x.Id, x.Name, x.DisplayOrder, x.CreatedAt, x.UpdatedAt, x.DeletedReason, x.DeletedBy, x.DeletedAt))
                    .ToListAsync(cancellationToken);
                return (IReadOnlyList<CachedDivisionItem>)items;
            },
            cancellationToken);

    public Task<IReadOnlyList<CachedDepartmentItem>> GetDepartmentsAsync(OperisDbContext dbContext, CancellationToken cancellationToken) =>
        GetOrCreateAsync(
            DepartmentsKey,
            async () =>
            {
                var items = await dbContext.Departments
                    .AsNoTracking()
                    .Where(x => x.DeletedAt == null)
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.Name)
                    .Select(x => new CachedDepartmentItem(
                        x.Id,
                        x.Name,
                        x.DisplayOrder,
                        x.DivisionId,
                        x.DivisionId.HasValue
                            ? dbContext.Divisions
                                .Where(division => division.Id == x.DivisionId.Value)
                                .Select(division => division.Name)
                                .FirstOrDefault()
                            : null,
                        x.CreatedAt,
                        x.UpdatedAt,
                        x.DeletedReason,
                        x.DeletedBy,
                        x.DeletedAt))
                    .ToListAsync(cancellationToken);
                return (IReadOnlyList<CachedDepartmentItem>)items;
            },
            cancellationToken);

    public Task<IReadOnlyList<CachedJobTitleItem>> GetJobTitlesAsync(OperisDbContext dbContext, CancellationToken cancellationToken) =>
        GetOrCreateAsync(
            JobTitlesKey,
            async () =>
            {
                var items = await dbContext.JobTitles
                    .AsNoTracking()
                    .Where(x => x.DeletedAt == null)
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.Name)
                    .Select(x => new CachedJobTitleItem(
                        x.Id,
                        x.Name,
                        x.DisplayOrder,
                        x.DepartmentId.HasValue
                            ? dbContext.Departments
                                .Where(department => department.Id == x.DepartmentId.Value)
                                .Select(department => department.DivisionId)
                                .FirstOrDefault()
                            : null,
                        x.DepartmentId.HasValue
                            ? dbContext.Departments
                                .Where(department => department.Id == x.DepartmentId.Value && department.DivisionId.HasValue)
                                .Select(department => dbContext.Divisions
                                    .Where(division => division.Id == department.DivisionId!.Value)
                                    .Select(division => division.Name)
                                    .FirstOrDefault())
                                .FirstOrDefault()
                            : null,
                        x.DepartmentId,
                        x.DepartmentId.HasValue
                            ? dbContext.Departments
                                .Where(department => department.Id == x.DepartmentId.Value)
                                .Select(department => department.Name)
                                .FirstOrDefault()
                            : null,
                        x.CreatedAt,
                        x.UpdatedAt,
                        x.DeletedReason,
                        x.DeletedBy,
                        x.DeletedAt))
                    .ToListAsync(cancellationToken);
                return (IReadOnlyList<CachedJobTitleItem>)items;
            },
            cancellationToken);

    public Task<IReadOnlyList<CachedProjectRoleItem>> GetProjectRolesAsync(OperisDbContext dbContext, CancellationToken cancellationToken) =>
        GetOrCreateAsync(
            ProjectRolesKey,
            async () =>
            {
                var items = await dbContext.ProjectRoles
                    .AsNoTracking()
                    .Where(x => x.DeletedAt == null)
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.Name)
                    .Select(x => new CachedProjectRoleItem(x.Id, x.Name, x.DisplayOrder, x.CreatedAt, x.UpdatedAt, x.DeletedReason, x.DeletedBy, x.DeletedAt))
                    .ToListAsync(cancellationToken);
                return (IReadOnlyList<CachedProjectRoleItem>)items;
            },
            cancellationToken);

    public Task<IReadOnlyList<CachedAppRoleItem>> GetAppRolesAsync(OperisDbContext dbContext, CancellationToken cancellationToken) =>
        GetOrCreateAsync(
            AppRolesKey,
            async () =>
            {
                var items = await dbContext.AppRoles
                    .AsNoTracking()
                    .Where(x => x.DeletedAt == null)
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.Name)
                    .Select(x => new CachedAppRoleItem(x.Id, x.Name, x.KeycloakRoleName, x.Description, x.DisplayOrder))
                    .ToListAsync(cancellationToken);
                return (IReadOnlyList<CachedAppRoleItem>)items;
            },
            cancellationToken);

    public Task InvalidateDepartmentsAsync(CancellationToken cancellationToken) =>
        cache.RemoveAsync(DepartmentsKey, cancellationToken);

    public Task InvalidateDivisionsAsync(CancellationToken cancellationToken) =>
        cache.RemoveAsync(DivisionsKey, cancellationToken);

    public Task InvalidateJobTitlesAsync(CancellationToken cancellationToken) =>
        cache.RemoveAsync(JobTitlesKey, cancellationToken);

    public Task InvalidateProjectRolesAsync(CancellationToken cancellationToken) =>
        cache.RemoveAsync(ProjectRolesKey, cancellationToken);

    public Task InvalidateAppRolesAsync(CancellationToken cancellationToken) =>
        cache.RemoveAsync(AppRolesKey, cancellationToken);

    private async Task<IReadOnlyList<TItem>> GetOrCreateAsync<TItem>(
        string cacheKey,
        Func<Task<IReadOnlyList<TItem>>> factory,
        CancellationToken cancellationToken)
    {
        var cached = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrWhiteSpace(cached))
        {
            var restored = JsonSerializer.Deserialize<List<TItem>>(cached);
            if (restored is not null)
            {
                return restored;
            }
        }

        var items = await factory();
        await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(items), CacheOptions, cancellationToken);
        return items;
    }
}
