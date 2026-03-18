using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Users.Application;

public interface IProjectTemplateQueries
{
    Task<PagedResult<ProjectTypeTemplateResponse>> ListProjectTypeTemplatesAsync(ProjectListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<ProjectTypeRoleRequirementResponse>> ListProjectTypeRoleRequirementsAsync(Guid templateId, string? search, string? sortBy, string? sortOrder, int page, int pageSize, CancellationToken cancellationToken);
    Task<ProjectTypeTemplateResponse?> GetProjectTypeTemplateByProjectTypeAsync(string projectType, CancellationToken cancellationToken);
}

public sealed class ProjectTemplateQueries(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter) : IProjectTemplateQueries
{
    public async Task<PagedResult<ProjectTypeTemplateResponse>> ListProjectTypeTemplatesAsync(ProjectListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        IQueryable<ProjectTypeTemplateEntity> source = dbContext.ProjectTypeTemplates.AsNoTracking().Where(x => x.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x => EF.Functions.ILike(x.ProjectType, search));
        }

        source = ApplySorting(source, query.SortBy, query.SortOrder);
        var total = await source.CountAsync(cancellationToken);
        var items = await source.Skip(skip).Take(pageSize).Select(ToTemplateResponseProjection()).ToListAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "list", EntityType: "project_type_template", StatusCode: StatusCodes.Status200OK, Metadata: new { total, page, pageSize, query.Search, query.SortBy, query.SortOrder }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PagedResult<ProjectTypeTemplateResponse>(items, total, page, pageSize);
    }

    public async Task<PagedResult<ProjectTypeRoleRequirementResponse>> ListProjectTypeRoleRequirementsAsync(Guid templateId, string? search, string? sortBy, string? sortOrder, int page, int pageSize, CancellationToken cancellationToken)
    {
        var (normalizedPage, normalizedPageSize, skip) = NormalizePaging(page, pageSize);
        IQueryable<ProjectTypeRoleRequirementEntity> source = dbContext.ProjectTypeRoleRequirements.AsNoTracking().Where(x => x.ProjectTypeTemplateId == templateId && x.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = $"%{search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.RoleName, normalizedSearch)
                || (x.RoleCode != null && EF.Functions.ILike(x.RoleCode, normalizedSearch)));
        }

        source = ApplyRoleSorting(source, sortBy, sortOrder);
        var total = await source.CountAsync(cancellationToken);
        var projectType = await dbContext.ProjectTypeTemplates.Where(x => x.Id == templateId).Select(x => x.ProjectType).FirstOrDefaultAsync(cancellationToken);
        var items = await source
            .Skip(skip)
            .Take(normalizedPageSize)
            .Select(x => new ProjectTypeRoleRequirementResponse(
                x.Id,
                x.ProjectTypeTemplateId,
                projectType ?? string.Empty,
                x.RoleName,
                x.RoleCode,
                x.Description,
                x.DisplayOrder,
                x.CreatedAt,
                x.UpdatedAt,
                x.DeletedReason,
                x.DeletedBy,
                x.DeletedAt))
            .ToListAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "list", EntityType: "project_type_role_requirement", EntityId: templateId.ToString(), StatusCode: StatusCodes.Status200OK, Metadata: new { total, page = normalizedPage, pageSize = normalizedPageSize, search, sortBy, sortOrder }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PagedResult<ProjectTypeRoleRequirementResponse>(items, total, normalizedPage, normalizedPageSize);
    }

    public Task<ProjectTypeTemplateResponse?> GetProjectTypeTemplateByProjectTypeAsync(string projectType, CancellationToken cancellationToken)
    {
        var normalized = projectType.Trim();
        return dbContext.ProjectTypeTemplates
            .AsNoTracking()
            .Where(x => x.ProjectType == normalized && x.DeletedAt == null)
            .Select(ToTemplateResponseProjection())
            .FirstOrDefaultAsync(cancellationToken)!;
    }

    private static System.Linq.Expressions.Expression<Func<ProjectTypeTemplateEntity, ProjectTypeTemplateResponse>> ToTemplateResponseProjection() =>
        x => new ProjectTypeTemplateResponse(
            x.Id,
            x.ProjectType,
            x.RequireSponsor,
            x.RequirePlannedPeriod,
            x.RequireActiveTeam,
            x.RequirePrimaryAssignment,
            x.RequireReportingRoot,
            x.RequireDocumentCreator,
            x.RequireReviewer,
            x.RequireApprover,
            x.RequireReleaseRole,
            x.CreatedAt,
            x.UpdatedAt,
            x.DeletedReason,
            x.DeletedBy,
            x.DeletedAt);

    private static IQueryable<ProjectTypeTemplateEntity> ApplySorting(IQueryable<ProjectTypeTemplateEntity> source, string? sortBy, string? sortOrder)
    {
        var descending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "projecttype" => descending ? source.OrderByDescending(x => x.ProjectType) : source.OrderBy(x => x.ProjectType),
            _ => descending ? source.OrderByDescending(x => x.CreatedAt) : source.OrderBy(x => x.CreatedAt)
        };
    }

    private static IQueryable<ProjectTypeRoleRequirementEntity> ApplyRoleSorting(IQueryable<ProjectTypeRoleRequirementEntity> source, string? sortBy, string? sortOrder)
    {
        var descending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "rolename" => descending ? source.OrderByDescending(x => x.RoleName) : source.OrderBy(x => x.RoleName),
            "rolecode" => descending ? source.OrderByDescending(x => x.RoleCode) : source.OrderBy(x => x.RoleCode),
            _ => descending ? source.OrderByDescending(x => x.DisplayOrder) : source.OrderBy(x => x.DisplayOrder)
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
