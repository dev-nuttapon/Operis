using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Domain;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Users.Application;

public sealed class UserQueries(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IKeycloakAdminClient keycloakAdminClient,
    IReferenceDataCache referenceDataCache) : IUserQueries
{
    private const int IdentityConcurrency = 4;

    public async Task<PagedResult<UserResponse>> ListUsersAsync(UserListQuery query, CancellationToken cancellationToken)
    {
        var (normalizedPage, normalizedPageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var baseQuery = dbContext.Users.Where(x => x.DeletedAt == null);

        if (query.Status.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Status == query.Status.Value);
        }

        if (query.JobTitleId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.JobTitleId == query.JobTitleId.Value);
        }

        if (query.DepartmentId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.DepartmentId == query.DepartmentId.Value);
        }

        if (!query.DepartmentId.HasValue && query.DivisionId.HasValue)
        {
            var departmentIds = await dbContext.Departments
                .Where(x => x.DeletedAt == null && x.DivisionId == query.DivisionId.Value)
                .Select(x => x.Id)
                .ToArrayAsync(cancellationToken);
            baseQuery = baseQuery.Where(x => x.DepartmentId.HasValue && departmentIds.Contains(x.DepartmentId.Value));
        }

        if (query.From.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.CreatedAt >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.CreatedAt <= query.To.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchPattern = $"%{query.Search.Trim()}%";
            var matchedIds = Array.Empty<string>();
            if (query.IncludeIdentity)
            {
                var keycloakMatches = await keycloakAdminClient.SearchUsersAsync(query.Search.Trim(), 0, 200, cancellationToken);
                matchedIds = keycloakMatches.Select(x => x.Id).Distinct(StringComparer.Ordinal).ToArray();
            }
            baseQuery = baseQuery.Where(x =>
                EF.Functions.ILike(x.Id, searchPattern)
                || EF.Functions.ILike(x.CreatedBy, searchPattern)
                || (x.DeletedBy != null && EF.Functions.ILike(x.DeletedBy, searchPattern))
                || (matchedIds.Length > 0 && matchedIds.Contains(x.Id)));
        }

        baseQuery = ApplyUserSorting(baseQuery, query.SortBy, query.SortOrder);
        var total = await baseQuery.CountAsync(cancellationToken);
        var users = await baseQuery
            .Skip(skip)
            .Take(normalizedPageSize)
            .ToListAsync(cancellationToken);

        var divisions = (await referenceDataCache.GetDivisionsAsync(dbContext, cancellationToken))
            .ToDictionary(x => x.Id, x => x.Name);
        var departments = (await referenceDataCache.GetDepartmentsAsync(dbContext, cancellationToken))
            .ToDictionary(x => x.Id, x => x);
        var jobTitles = (await referenceDataCache.GetJobTitlesAsync(dbContext, cancellationToken))
            .ToDictionary(x => x.Id, x => x.Name);
        var appRoles = await referenceDataCache.GetAppRolesAsync(dbContext, cancellationToken);

        IReadOnlyList<UserResponse> responses;
        if (!query.IncludeIdentity)
        {
            responses = users.Select(x => ToResponse(x, null, [], divisions, departments, jobTitles)).ToList();
        }
        else
        {
            responses = await BuildIdentityResponsesAsync(users, appRoles, divisions, departments, jobTitles, cancellationToken);
        }

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "list",
            EntityType: "user",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new
            {
                count = responses.Count,
                total,
                includeIdentity = query.IncludeIdentity,
                query.Status,
                query.DivisionId,
                query.DepartmentId,
                query.JobTitleId,
                query.From,
                query.To,
                page = normalizedPage,
                pageSize = normalizedPageSize,
                query.Search,
                query.SortBy,
                query.SortOrder
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PagedResult<UserResponse>(responses, total, normalizedPage, normalizedPageSize);
    }

    public async Task<UserResponse?> GetUserAsync(string userId, bool includeIdentity, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var entity = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId && x.DeletedAt == null, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var divisions = (await referenceDataCache.GetDivisionsAsync(dbContext, cancellationToken))
            .ToDictionary(x => x.Id, x => x.Name);
        var departments = (await referenceDataCache.GetDepartmentsAsync(dbContext, cancellationToken))
            .ToDictionary(x => x.Id, x => x);
        var jobTitles = (await referenceDataCache.GetJobTitlesAsync(dbContext, cancellationToken))
            .ToDictionary(x => x.Id, x => x.Name);
        var appRoles = await referenceDataCache.GetAppRolesAsync(dbContext, cancellationToken);
        var primaryAssignment = await dbContext.UserOrgAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == entity.Id && x.IsPrimary, cancellationToken);
        var effectiveDepartmentId = entity.DepartmentId ?? primaryAssignment?.DepartmentId;
        var effectiveJobTitleId = entity.JobTitleId ?? primaryAssignment?.PositionId;

        UserResponse response;
        if (!includeIdentity)
        {
            response = ToResponse(entity, null, [], divisions, departments, jobTitles, effectiveDepartmentId, effectiveJobTitleId);
        }
        else
        {
            var appRolesByKeycloakName = appRoles
                .GroupBy(x => x.KeycloakRoleName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.OrdinalIgnoreCase);

            var profileTask = ResolveKeycloakProfileAsync(entity, cancellationToken);
            var rolesTask = keycloakAdminClient.GetUserRealmRolesAsync(entity.Id, cancellationToken);
            await Task.WhenAll(profileTask, rolesTask);

            var mappedRoles = rolesTask.Result
                .SelectMany(role => appRolesByKeycloakName.TryGetValue(role.Name, out var matchedRoles) ? matchedRoles : [])
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .Select(x => x.Name)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            response = ToResponse(entity, profileTask.Result, mappedRoles, divisions, departments, jobTitles, effectiveDepartmentId, effectiveJobTitleId);
        }

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "get",
            EntityType: "user",
            EntityId: userId,
            StatusCode: StatusCodes.Status200OK,
            Metadata: new
            {
                includeIdentity
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return response;
    }

    private async Task<IReadOnlyList<UserResponse>> BuildIdentityResponsesAsync(
        IReadOnlyList<UserEntity> users,
        IReadOnlyList<CachedAppRoleItem> appRoles,
        IReadOnlyDictionary<Guid, string> divisions,
        IReadOnlyDictionary<Guid, CachedDepartmentItem> departments,
        IReadOnlyDictionary<Guid, string> jobTitles,
        CancellationToken cancellationToken)
    {
        var appRolesByKeycloakName = appRoles
            .GroupBy(x => x.KeycloakRoleName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.OrdinalIgnoreCase);

        var semaphore = new SemaphoreSlim(IdentityConcurrency);
        try
        {
            var tasks = users.Select(async user =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var profileTask = ResolveKeycloakProfileAsync(user, cancellationToken);
                    var rolesTask = keycloakAdminClient.GetUserRealmRolesAsync(user.Id, cancellationToken);
                    await Task.WhenAll(profileTask, rolesTask);

                    var mappedRoles = rolesTask.Result
                        .SelectMany(role => appRolesByKeycloakName.TryGetValue(role.Name, out var matchedRoles) ? matchedRoles : [])
                        .OrderBy(x => x.DisplayOrder)
                        .ThenBy(x => x.Name)
                        .Select(x => x.Name)
                        .Distinct(StringComparer.Ordinal)
                        .ToArray();

                    return ToResponse(user, profileTask.Result, mappedRoles, divisions, departments, jobTitles);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            return await Task.WhenAll(tasks);
        }
        finally
        {
            semaphore.Dispose();
        }
    }

    private async Task<KeycloakUserProfile?> ResolveKeycloakProfileAsync(UserEntity user, CancellationToken cancellationToken)
    {
        return string.IsNullOrWhiteSpace(user.Id)
            ? null
            : await keycloakAdminClient.GetUserByIdAsync(user.Id, cancellationToken);
    }

    private static IQueryable<UserEntity> ApplyUserSorting(IQueryable<UserEntity> query, string? sortBy, string? sortOrder)
    {
        var desc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return sortBy?.ToLowerInvariant() switch
        {
            "status" => desc ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.CreatedAt) : query.OrderBy(x => x.Status).ThenByDescending(x => x.CreatedAt),
            "createdby" => desc ? query.OrderByDescending(x => x.CreatedBy).ThenByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedBy).ThenByDescending(x => x.CreatedAt),
            _ => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt)
        };
    }

    private static UserResponse ToResponse(
        UserEntity entity,
        KeycloakUserProfile? keycloakProfile,
        IReadOnlyList<string> roles,
        IReadOnlyDictionary<Guid, string>? divisions,
        IReadOnlyDictionary<Guid, CachedDepartmentItem>? departments,
        IReadOnlyDictionary<Guid, string>? jobTitles,
        Guid? departmentIdOverride = null,
        Guid? jobTitleIdOverride = null)
    {
        Guid? divisionId = null;
        string? divisionName = null;
        string? departmentName = null;
        var departmentId = departmentIdOverride ?? entity.DepartmentId;
        var jobTitleId = jobTitleIdOverride ?? entity.JobTitleId;

        if (departmentId.HasValue && departments is not null && departments.TryGetValue(departmentId.Value, out var department))
        {
            departmentName = department.Name;
            divisionId = department.DivisionId;

            if (divisionId.HasValue && divisions is not null && divisions.TryGetValue(divisionId.Value, out var resolvedDivisionName))
            {
                divisionName = resolvedDivisionName;
            }
        }

        return new UserResponse(
            entity.Id,
            entity.Status,
            entity.CreatedAt,
            entity.CreatedBy,
            divisionId,
            divisionName,
            departmentId,
            departmentName,
            jobTitleId,
            jobTitleId.HasValue && jobTitles is not null && jobTitles.TryGetValue(jobTitleId.Value, out var jobTitleName) ? jobTitleName : null,
            roles,
            entity.PreferredLanguage,
            entity.PreferredTheme,
            entity.DeletedReason,
            entity.DeletedBy,
            entity.DeletedAt,
            keycloakProfile is null
                ? null
                : new KeycloakUserSummary(
                    keycloakProfile.Id,
                    keycloakProfile.Email,
                    keycloakProfile.Username,
                    keycloakProfile.FirstName,
                    keycloakProfile.LastName,
                    keycloakProfile.Enabled,
                    keycloakProfile.EmailVerified));
    }

    private static (int Page, int PageSize, int Skip) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = Math.Clamp(pageSize, 5, 100);
        var skip = (normalizedPage - 1) * normalizedPageSize;
        return (normalizedPage, normalizedPageSize, skip);
    }
}
