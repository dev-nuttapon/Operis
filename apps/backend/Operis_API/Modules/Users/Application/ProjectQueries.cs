using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Users.Application;

public sealed record ProjectListQuery(
    string? Search,
    string? SortBy,
    string? SortOrder,
    int Page = 1,
    int PageSize = 10);

public sealed record ProjectAssignmentListQuery(
    Guid ProjectId,
    string? Search,
    string? SortBy,
    string? SortOrder,
    int Page = 1,
    int PageSize = 10);

public interface IProjectQueries
{
    Task<PagedResult<ProjectResponse>> ListProjectsAsync(ProjectListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<ProjectRoleResponse>> ListProjectRolesAsync(ReferenceDataQuery query, CancellationToken cancellationToken);
    Task<PagedResult<ProjectAssignmentResponse>> ListProjectAssignmentsAsync(ProjectAssignmentListQuery query, CancellationToken cancellationToken);
}

public sealed class ProjectQueries(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter) : IProjectQueries
{
    public async Task<PagedResult<ProjectResponse>> ListProjectsAsync(ProjectListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        IQueryable<ProjectEntity> source = dbContext.Projects.AsNoTracking().Where(x => x.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();
            source = source.Where(x => x.Name.ToLower().Contains(search) || x.Code.ToLower().Contains(search));
        }

        source = ApplyProjectSorting(source, query.SortBy, query.SortOrder);
        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new ProjectResponse(
                x.Id,
                x.Code,
                x.Name,
                x.Status,
                x.StartAt,
                x.EndAt,
                x.CreatedAt,
                x.UpdatedAt,
                x.DeletedReason,
                x.DeletedBy,
                x.DeletedAt))
            .ToListAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "list",
            EntityType: "project",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { total, page, pageSize, query.Search, query.SortBy, query.SortOrder }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PagedResult<ProjectResponse>(items, total, page, pageSize);
    }

    public async Task<PagedResult<ProjectRoleResponse>> ListProjectRolesAsync(ReferenceDataQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        IQueryable<ProjectRoleEntity> source = dbContext.ProjectRoles.AsNoTracking().Where(x => x.DeletedAt == null);

        if (query.DivisionId.HasValue)
        {
            source = source.Where(x => x.ProjectId == query.DivisionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();
            source = source.Where(x => x.Name.ToLower().Contains(search));
        }

        source = ApplyProjectRoleSorting(source, query.SortBy, query.SortOrder);
        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new ProjectRoleResponse(
                x.Id,
                x.ProjectId,
                x.ProjectId.HasValue
                    ? dbContext.Projects.Where(project => project.Id == x.ProjectId.Value).Select(project => project.Name).FirstOrDefault()
                    : null,
                x.Name,
                x.DisplayOrder,
                x.CreatedAt,
                x.UpdatedAt,
                x.DeletedReason,
                x.DeletedBy,
                x.DeletedAt))
            .ToListAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "list",
            EntityType: "project_role",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { total, page, pageSize, projectId = query.DivisionId, query.Search, query.SortBy, query.SortOrder }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PagedResult<ProjectRoleResponse>(items, total, page, pageSize);
    }

    public async Task<PagedResult<ProjectAssignmentResponse>> ListProjectAssignmentsAsync(ProjectAssignmentListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        IQueryable<UserProjectAssignmentEntity> source = dbContext.UserProjectAssignments
            .AsNoTracking()
            .Where(x => x.ProjectId == query.ProjectId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();
            source = source.Where(x =>
                x.UserId.ToLower().Contains(search) ||
                dbContext.Users.Where(user => user.Id == x.UserId).Any(user => user.Id.ToLower().Contains(search)) ||
                dbContext.ProjectRoles.Where(role => role.Id == x.ProjectRoleId).Any(role => role.Name.ToLower().Contains(search)));
        }

        source = ApplyProjectAssignmentSorting(source, query.SortBy, query.SortOrder);
        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new ProjectAssignmentResponse(
                x.Id,
                x.UserId,
                dbContext.Users.Where(user => user.Id == x.UserId).Select(user => user.Id).FirstOrDefault(),
                dbContext.Users.Where(user => user.Id == x.UserId)
                    .Select(user => user.Id)
                    .FirstOrDefault(),
                x.ProjectId,
                dbContext.Projects.Where(project => project.Id == x.ProjectId).Select(project => project.Name).FirstOrDefault() ?? string.Empty,
                x.ProjectRoleId,
                dbContext.ProjectRoles.Where(role => role.Id == x.ProjectRoleId).Select(role => role.Name).FirstOrDefault() ?? string.Empty,
                x.ReportsToUserId,
                x.ReportsToUserId == null
                    ? null
                    : dbContext.Users.Where(user => user.Id == x.ReportsToUserId).Select(user => user.Id).FirstOrDefault(),
                x.IsPrimary,
                x.StartAt,
                x.EndAt,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "list",
            EntityType: "project_assignment",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { total, page, pageSize, query.ProjectId, query.Search, query.SortBy, query.SortOrder }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PagedResult<ProjectAssignmentResponse>(items, total, page, pageSize);
    }

    private static IQueryable<ProjectEntity> ApplyProjectSorting(IQueryable<ProjectEntity> source, string? sortBy, string? sortOrder)
    {
        var descending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "code" => descending ? source.OrderByDescending(x => x.Code) : source.OrderBy(x => x.Code),
            "status" => descending ? source.OrderByDescending(x => x.Status) : source.OrderBy(x => x.Status),
            "startat" => descending ? source.OrderByDescending(x => x.StartAt) : source.OrderBy(x => x.StartAt),
            _ => descending ? source.OrderByDescending(x => x.CreatedAt) : source.OrderBy(x => x.CreatedAt)
        };
    }

    private static IQueryable<ProjectRoleEntity> ApplyProjectRoleSorting(IQueryable<ProjectRoleEntity> source, string? sortBy, string? sortOrder)
    {
        var descending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "name" => descending ? source.OrderByDescending(x => x.Name) : source.OrderBy(x => x.Name),
            _ => descending ? source.OrderByDescending(x => x.DisplayOrder) : source.OrderBy(x => x.DisplayOrder)
        };
    }

    private static IQueryable<UserProjectAssignmentEntity> ApplyProjectAssignmentSorting(IQueryable<UserProjectAssignmentEntity> source, string? sortBy, string? sortOrder)
    {
        var descending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "startat" => descending ? source.OrderByDescending(x => x.StartAt) : source.OrderBy(x => x.StartAt),
            _ => descending ? source.OrderByDescending(x => x.CreatedAt) : source.OrderBy(x => x.CreatedAt)
        };
    }

    private static (int Page, int PageSize, int Skip) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);
        var skip = (normalizedPage - 1) * normalizedPageSize;
        return (normalizedPage, normalizedPageSize, skip);
    }
}
