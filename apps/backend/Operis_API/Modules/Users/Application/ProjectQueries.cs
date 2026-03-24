using Microsoft.EntityFrameworkCore;
using System.Text;
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
    string? AssignedUserId = null,
    int Page = 1,
    int PageSize = 10);

public sealed record ProjectAssignmentListQuery(
    Guid ProjectId,
    string? Search,
    string? SortBy,
    string? SortOrder,
    int Page = 1,
    int PageSize = 10);

public sealed record ProjectEvidenceListQuery(
    Guid ProjectId,
    int Page = 1,
    int PageSize = 10);

sealed record ProjectOrgChartRow(
    Guid Id,
    string UserId,
    string? UserEmail,
    string? UserDisplayName,
    Guid ProjectRoleId,
    string ProjectRoleName,
    bool IsPrimary,
    string Status,
    string? ReportsToUserId,
    DateTimeOffset StartAt,
    DateTimeOffset? EndAt);

sealed record ProjectAssignmentRow(
    Guid Id,
    string UserId,
    string? UserEmail,
    string? UserDisplayName,
    Guid ProjectId,
    string ProjectName,
    Guid ProjectRoleId,
    string ProjectRoleName,
    string? ReportsToUserId,
    string? ReportsToDisplayName,
    bool IsPrimary,
    string Status,
    string? ChangeReason,
    Guid? ReplacedByAssignmentId,
    DateTimeOffset StartAt,
    DateTimeOffset? EndAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public interface IProjectQueries
{
    Task<PagedResult<ProjectListItem>> ListProjectsAsync(ProjectListQuery query, CancellationToken cancellationToken);
    Task<ProjectResponse?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken);
    Task<PagedResult<ProjectRoleResponse>> ListProjectRolesAsync(ReferenceDataQuery query, CancellationToken cancellationToken);
    Task<ProjectRoleResponse?> GetProjectRoleAsync(Guid projectRoleId, CancellationToken cancellationToken);
    Task<PagedResult<ProjectAssignmentResponse>> ListProjectAssignmentsAsync(ProjectAssignmentListQuery query, CancellationToken cancellationToken);
    Task<ProjectAssignmentResponse?> GetProjectAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken);
    Task<bool> HasProjectAccessAsync(Guid projectId, string userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProjectOrgChartNodeResponse>> GetProjectOrgChartAsync(Guid projectId, CancellationToken cancellationToken);
    Task<ProjectEvidenceResponse?> GetProjectEvidenceAsync(Guid projectId, CancellationToken cancellationToken);
    Task<PagedResult<ProjectTeamRegisterRowResponse>?> ListProjectTeamRegisterAsync(ProjectEvidenceListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<ProjectRoleResponsibilityRowResponse>?> ListProjectRoleResponsibilitiesAsync(ProjectEvidenceListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<ProjectAssignmentHistoryRowResponse>?> ListProjectAssignmentHistoryAsync(ProjectEvidenceListQuery query, CancellationToken cancellationToken);
    Task<ProjectEvidenceExportResult?> GetProjectEvidenceExportAsync(Guid projectId, CancellationToken cancellationToken);
    Task<ProjectComplianceResponse?> GetProjectComplianceAsync(Guid projectId, CancellationToken cancellationToken);
}

public sealed class ProjectQueries(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IKeycloakAdminClient keycloakAdminClient) : IProjectQueries
{
    public async Task<PagedResult<ProjectListItem>> ListProjectsAsync(ProjectListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        IQueryable<ProjectEntity> source = dbContext.Projects.AsNoTracking().Where(x => x.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(query.AssignedUserId))
        {
            source = source.Where(project =>
                dbContext.UserProjectAssignments.Any(assignment =>
                    assignment.ProjectId == project.Id &&
                    assignment.UserId == query.AssignedUserId &&
                    assignment.Status == "Active"));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x => EF.Functions.ILike(x.Name, search) || EF.Functions.ILike(x.Code, search));
        }

        source = ApplyProjectSorting(source, query.SortBy, query.SortOrder);
        var total = await source.CountAsync(cancellationToken);
        var items = await (
                from project in source.Skip(skip).Take(pageSize)
                join owner in dbContext.Users.AsNoTracking() on project.OwnerUserId equals owner.Id into ownerJoin
                from owner in ownerJoin.DefaultIfEmpty()
                join sponsor in dbContext.Users.AsNoTracking() on project.SponsorUserId equals sponsor.Id into sponsorJoin
                from sponsor in sponsorJoin.DefaultIfEmpty()
                select new ProjectListItem(
                    project.Id,
                    project.Code,
                    project.Name,
                    project.ProjectType,
                    project.OwnerUserId,
                    owner != null && owner.DeletedAt == null ? owner.Id : null,
                    sponsor != null && sponsor.DeletedAt == null ? sponsor.Id : null,
                    project.Phase,
                    project.Status,
                    project.PlannedStartAt,
                    project.StartAt,
                    project.EndAt,
                    project.CreatedAt))
            .ToListAsync(cancellationToken);

        var ownerDisplayNames = await ResolveUserDisplayNamesAsync(
            items.Select(x => x.OwnerUserId),
            cancellationToken);
        var resolvedItems = items
            .Select(item => item with
            {
                OwnerDisplayName = item.OwnerUserId is not null && ownerDisplayNames.TryGetValue(item.OwnerUserId, out var name)
                    ? name
                    : item.OwnerDisplayName
            })
            .ToList();

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "list",
            EntityType: "project",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { total, page, pageSize, query.Search, query.SortBy, query.SortOrder }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PagedResult<ProjectListItem>(resolvedItems, total, page, pageSize);
    }

    public async Task<ProjectResponse?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var project = await (
            from entity in dbContext.Projects.AsNoTracking()
            where entity.Id == projectId && entity.DeletedAt == null
            join owner in dbContext.Users.AsNoTracking() on entity.OwnerUserId equals owner.Id into ownerJoin
            from owner in ownerJoin.DefaultIfEmpty()
            join sponsor in dbContext.Users.AsNoTracking() on entity.SponsorUserId equals sponsor.Id into sponsorJoin
            from sponsor in sponsorJoin.DefaultIfEmpty()
            select new
            {
                Entity = entity,
                OwnerDisplayName = owner != null && owner.DeletedAt == null ? owner.Id : null,
                SponsorDisplayName = sponsor != null && sponsor.DeletedAt == null ? sponsor.Id : null
            }).SingleOrDefaultAsync(cancellationToken);

        if (project is null)
        {
            return null;
        }

        var ownerDisplayName = project.Entity.OwnerUserId is not null
            ? await ResolveUserDisplayNameAsync(project.Entity.OwnerUserId, cancellationToken)
            : null;
        var sponsorDisplayName = project.Entity.SponsorUserId is not null
            ? await ResolveUserDisplayNameAsync(project.Entity.SponsorUserId, cancellationToken)
            : null;

        return new ProjectResponse(
            project.Entity.Id,
            project.Entity.Code,
            project.Entity.Name,
            project.Entity.ProjectType,
            project.Entity.OwnerUserId,
            ownerDisplayName ?? project.OwnerDisplayName,
            project.Entity.SponsorUserId,
            sponsorDisplayName ?? project.SponsorDisplayName,
            project.Entity.Methodology,
            project.Entity.Phase,
            project.Entity.Status,
            project.Entity.StatusReason,
            project.Entity.WorkflowDefinitionId,
            project.Entity.DocumentTemplateId,
            project.Entity.PlannedStartAt,
            project.Entity.PlannedEndAt,
            project.Entity.StartAt,
            project.Entity.EndAt,
            project.Entity.CreatedAt,
            project.Entity.UpdatedAt,
            project.Entity.DeletedReason,
            project.Entity.DeletedBy,
            project.Entity.DeletedAt);
    }

    private async Task<IReadOnlyDictionary<string, string>> ResolveUserDisplayNamesAsync(
        IEnumerable<string?> userIds,
        CancellationToken cancellationToken)
    {
        var distinctIds = userIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (distinctIds.Length == 0)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        var results = new Dictionary<string, string>(StringComparer.Ordinal);
        using var semaphore = new SemaphoreSlim(4);
        var tasks = distinctIds.Select(async userId =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var profile = await ResolveProfileAsync(userId, cancellationToken);
                var displayName = ResolveDisplayName(profile, userId);
                if (!string.IsNullOrWhiteSpace(displayName))
                {
                    results[userId] = displayName!;
                }
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        return results;
    }

    private async Task<string?> ResolveUserDisplayNameAsync(string userId, CancellationToken cancellationToken)
    {
        var profile = await ResolveProfileAsync(userId, cancellationToken);
        return ResolveDisplayName(profile, userId);
    }

    private async Task<KeycloakUserProfile?> ResolveProfileAsync(string userId, CancellationToken cancellationToken)
    {
        var profile = await keycloakAdminClient.GetUserByIdAsync(userId, cancellationToken);
        if (profile is not null)
        {
            return profile;
        }

        var matches = await keycloakAdminClient.SearchUsersAsync(userId, 0, 5, cancellationToken);
        if (matches.Count == 0)
        {
            return null;
        }

        return matches.FirstOrDefault(item => string.Equals(item.Id, userId, StringComparison.Ordinal))
            ?? matches.FirstOrDefault(item => string.Equals(item.Username, userId, StringComparison.OrdinalIgnoreCase))
            ?? matches.FirstOrDefault(item => string.Equals(item.Email, userId, StringComparison.OrdinalIgnoreCase))
            ?? matches.First();
    }

    private static string? ResolveDisplayName(KeycloakUserProfile? profile, string fallbackId)
    {
        if (profile is null)
        {
            return fallbackId;
        }

        var displayName = string.Join(' ', new[] { profile.FirstName, profile.LastName }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim()));

        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return displayName;
        }

        if (!string.IsNullOrWhiteSpace(profile.Email))
        {
            return profile.Email;
        }

        if (!string.IsNullOrWhiteSpace(profile.Username))
        {
            return profile.Username;
        }

        return fallbackId;
    }

    public Task<bool> HasProjectAccessAsync(Guid projectId, string userId, CancellationToken cancellationToken) =>
        dbContext.UserProjectAssignments
            .AsNoTracking()
            .AnyAsync(
                assignment => assignment.ProjectId == projectId &&
                              assignment.UserId == userId &&
                              assignment.Status == "Active",
                cancellationToken);

    public async Task<PagedResult<ProjectRoleResponse>> ListProjectRolesAsync(ReferenceDataQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        IQueryable<ProjectRoleEntity> source = dbContext.ProjectRoles.AsNoTracking().Where(x => x.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x => EF.Functions.ILike(x.Name, search));
        }

        source = ApplyProjectRoleSorting(source, query.SortBy, query.SortOrder);
        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .Select(role => new ProjectRoleResponse(
                role.Id,
                null,
                null,
                role.Name,
                role.Code,
                role.Description,
                role.Responsibilities,
                role.AuthorityScope,
                role.DisplayOrder,
                role.CreatedAt,
                role.UpdatedAt,
                role.DeletedReason,
                role.DeletedBy,
                role.DeletedAt))
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "list",
            EntityType: "project_role",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { total, page, pageSize, query.Search, query.SortBy, query.SortOrder }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PagedResult<ProjectRoleResponse>(items, total, page, pageSize);
    }

    public async Task<ProjectRoleResponse?> GetProjectRoleAsync(Guid projectRoleId, CancellationToken cancellationToken)
    {
        var result = await dbContext.ProjectRoles
            .AsNoTracking()
            .Where(role => role.Id == projectRoleId && role.DeletedAt == null)
            .Select(role => new ProjectRoleResponse(
                role.Id,
                null,
                null,
                role.Name,
                role.Code,
                role.Description,
                role.Responsibilities,
                role.AuthorityScope,
                role.DisplayOrder,
                role.CreatedAt,
                role.UpdatedAt,
                role.DeletedReason,
                role.DeletedBy,
                role.DeletedAt))
            .SingleOrDefaultAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "get",
            EntityType: "project_role",
            EntityId: projectRoleId.ToString(),
            StatusCode: result is null ? StatusCodes.Status404NotFound : StatusCodes.Status200OK));
        await dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<PagedResult<ProjectAssignmentResponse>> ListProjectAssignmentsAsync(ProjectAssignmentListQuery query, CancellationToken cancellationToken)
    {
        var projectName = await dbContext.Projects.AsNoTracking()
            .Where(project => project.Id == query.ProjectId && project.DeletedAt == null)
            .Select(project => project.Name)
            .SingleOrDefaultAsync(cancellationToken) ?? string.Empty;

        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var source = from assignment in dbContext.UserProjectAssignments.AsNoTracking()
                     where assignment.ProjectId == query.ProjectId && assignment.Status == "Active"
                     join user in dbContext.Users.AsNoTracking() on assignment.UserId equals user.Id into userJoin
                     from user in userJoin.DefaultIfEmpty()
                     join role in dbContext.ProjectRoles.AsNoTracking() on assignment.ProjectRoleId equals role.Id into roleJoin
                     from role in roleJoin.DefaultIfEmpty()
                     join reportsTo in dbContext.Users.AsNoTracking() on assignment.ReportsToUserId equals reportsTo.Id into reportsJoin
                     from reportsTo in reportsJoin.DefaultIfEmpty()
                     select new
                     {
                         Assignment = assignment,
                         User = user,
                         Role = role,
                         ReportsTo = reportsTo
                     };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchPattern = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.Assignment.UserId, searchPattern)
                || (x.User != null && EF.Functions.ILike(x.User.Id, searchPattern))
                || (x.Role != null && EF.Functions.ILike(x.Role.Name, searchPattern)));
        }

        var total = await source.CountAsync(cancellationToken);
        var descending = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        var ordered = (query.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "startat" => descending
                ? source.OrderByDescending(x => x.Assignment.StartAt)
                : source.OrderBy(x => x.Assignment.StartAt),
            _ => descending
                ? source.OrderByDescending(x => x.Assignment.CreatedAt)
                : source.OrderBy(x => x.Assignment.CreatedAt)
        };

        var items = await ordered
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new ProjectAssignmentResponse(
                x.Assignment.Id,
                x.Assignment.UserId,
                x.User != null ? x.User.Id : null,
                x.User != null ? x.User.Id : null,
                x.Assignment.ProjectId,
                projectName,
                x.Assignment.ProjectRoleId,
                x.Role != null ? x.Role.Name : string.Empty,
                x.Assignment.ReportsToUserId,
                x.ReportsTo != null ? x.ReportsTo.Id : null,
                x.Assignment.IsPrimary,
                x.Assignment.Status,
                x.Assignment.ChangeReason,
                x.Assignment.ReplacedByAssignmentId,
                x.Assignment.StartAt,
                x.Assignment.EndAt,
                x.Assignment.CreatedAt,
                x.Assignment.UpdatedAt))
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

    public async Task<ProjectAssignmentResponse?> GetProjectAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken)
    {
        var result = await (
                from assignment in dbContext.UserProjectAssignments.AsNoTracking()
                where assignment.Id == assignmentId && assignment.Status != "Removed"
                join user in dbContext.Users.AsNoTracking() on assignment.UserId equals user.Id into userJoin
                from user in userJoin.DefaultIfEmpty()
                join project in dbContext.Projects.AsNoTracking() on assignment.ProjectId equals project.Id into projectJoin
                from project in projectJoin.DefaultIfEmpty()
                join role in dbContext.ProjectRoles.AsNoTracking() on assignment.ProjectRoleId equals role.Id into roleJoin
                from role in roleJoin.DefaultIfEmpty()
                join reportsTo in dbContext.Users.AsNoTracking() on assignment.ReportsToUserId equals reportsTo.Id into reportsJoin
                from reportsTo in reportsJoin.DefaultIfEmpty()
                select new ProjectAssignmentResponse(
                    assignment.Id,
                    assignment.UserId,
                    user != null ? user.Id : null,
                    user != null ? user.Id : null,
                    assignment.ProjectId,
                    project != null && project.DeletedAt == null ? project.Name : string.Empty,
                    assignment.ProjectRoleId,
                    role != null && role.DeletedAt == null ? role.Name : string.Empty,
                    assignment.ReportsToUserId,
                    reportsTo != null ? reportsTo.Id : null,
                    assignment.IsPrimary,
                    assignment.Status,
                    assignment.ChangeReason,
                    assignment.ReplacedByAssignmentId,
                    assignment.StartAt,
                    assignment.EndAt,
                    assignment.CreatedAt,
                    assignment.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "get",
            EntityType: "project_assignment",
            EntityId: assignmentId.ToString(),
            StatusCode: result is null ? StatusCodes.Status404NotFound : StatusCodes.Status200OK));
        await dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<IReadOnlyList<ProjectOrgChartNodeResponse>> GetProjectOrgChartAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var projectExists = await dbContext.Projects
            .AsNoTracking()
            .AnyAsync(x => x.Id == projectId && x.DeletedAt == null, cancellationToken);
        if (!projectExists)
        {
            return [];
        }

        var assignments = await (
            from assignment in dbContext.UserProjectAssignments.AsNoTracking()
            where assignment.ProjectId == projectId && assignment.Status == "Active"
            join user in dbContext.Users.AsNoTracking() on assignment.UserId equals user.Id into userJoin
            from user in userJoin.DefaultIfEmpty()
            join role in dbContext.ProjectRoles.AsNoTracking() on assignment.ProjectRoleId equals role.Id into roleJoin
            from role in roleJoin.DefaultIfEmpty()
            orderby assignment.StartAt, assignment.CreatedAt
            select new ProjectOrgChartRow(
                assignment.Id,
                assignment.UserId,
                user != null ? user.Id : null,
                user != null ? user.Id : null,
                assignment.ProjectRoleId,
                role != null ? role.Name : string.Empty,
                assignment.IsPrimary,
                assignment.Status,
                assignment.ReportsToUserId,
                assignment.StartAt,
                assignment.EndAt))
            .ToListAsync(cancellationToken);

        var childrenLookup = assignments
            .Where(x => !string.IsNullOrWhiteSpace(x.ReportsToUserId))
            .GroupBy(x => x.ReportsToUserId!)
            .ToDictionary(group => group.Key, group => group.ToList());

        ProjectOrgChartNodeResponse BuildNode(ProjectOrgChartRow assignment)
        {
            var children = childrenLookup.TryGetValue(assignment.UserId, out var childAssignments)
                ? childAssignments.Select(BuildNode).ToList()
                : [];

            return new ProjectOrgChartNodeResponse(
                assignment.Id,
                assignment.UserId,
                assignment.UserEmail,
                assignment.UserDisplayName,
                assignment.ProjectRoleId,
                assignment.ProjectRoleName,
                assignment.IsPrimary,
                assignment.Status,
                assignment.ReportsToUserId,
                assignment.StartAt,
                assignment.EndAt,
                children);
        }

        var roots = assignments
            .Where(x => string.IsNullOrWhiteSpace(x.ReportsToUserId) || assignments.All(parent => parent.UserId != x.ReportsToUserId))
            .Select(BuildNode)
            .ToList();

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "view_org_chart",
            EntityType: "project_assignment",
            EntityId: projectId.ToString(),
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { projectId, nodes = assignments.Count }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return roots;
    }

    public async Task<ProjectEvidenceResponse?> GetProjectEvidenceAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var project = await dbContext.Projects
            .AsNoTracking()
            .Where(x => x.Id == projectId && x.DeletedAt == null)
            .Select(x => new { x.Id, x.Name })
            .FirstOrDefaultAsync(cancellationToken);
        if (project is null)
        {
            return null;
        }

        var assignments = await BuildEvidenceAssignmentRows(projectId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var teamRegister = assignments
            .Where(x => x.Status == "Active")
            .OrderBy(x => x.UserDisplayName ?? x.UserEmail ?? x.UserId)
            .Select(x => new ProjectTeamRegisterRowResponse(
                x.Id,
                x.UserId,
                x.UserEmail,
                x.UserDisplayName,
                x.ProjectRoleName,
                x.ReportsToDisplayName,
                x.IsPrimary,
                x.Status,
                x.StartAt,
                x.EndAt))
            .ToList();

        var roleIds = assignments
            .Select(x => x.ProjectRoleId)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        var roleRows = roleIds.Length == 0
            ? []
            : await dbContext.ProjectRoles
                .AsNoTracking()
                .Where(x => roleIds.Contains(x.Id) && x.DeletedAt == null)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Code,
                    x.Description,
                    x.Responsibilities,
                    x.AuthorityScope
                })
                .ToListAsync(cancellationToken);

        var roleMemberCounts = roleIds.Length == 0
            ? new Dictionary<Guid, int>()
            : await dbContext.UserProjectAssignments
                .AsNoTracking()
                .Where(assignment => assignment.Status == "Active" && roleIds.Contains(assignment.ProjectRoleId))
                .GroupBy(assignment => assignment.ProjectRoleId)
                .Select(group => new { RoleId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(x => x.RoleId, x => x.Count, cancellationToken);

        var roleResponsibilities = roleRows
            .Select(x => new ProjectRoleResponsibilityRowResponse(
                x.Id,
                x.Name,
                x.Code,
                x.Description,
                x.Responsibilities,
                x.AuthorityScope,
                roleMemberCounts.TryGetValue(x.Id, out var count) ? count : 0))
            .ToList();

        var assignmentHistory = assignments
            .Select(x => new ProjectAssignmentHistoryRowResponse(
                x.Id,
                x.UserId,
                x.UserEmail,
                x.UserDisplayName,
                x.ProjectRoleName,
                x.Status,
                x.ChangeReason,
                x.ReportsToDisplayName,
                x.IsPrimary,
                x.StartAt,
                x.EndAt,
                x.CreatedAt,
                x.UpdatedAt))
            .ToList();

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "view_evidence",
            EntityType: "project",
            EntityId: projectId.ToString(),
            StatusCode: StatusCodes.Status200OK,
            Metadata: new
            {
                projectId,
                teamRegister = teamRegister.Count,
                roleResponsibilities = roleResponsibilities.Count,
                assignmentHistory = assignmentHistory.Count
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ProjectEvidenceResponse(project.Id, project.Name, teamRegister, roleResponsibilities, assignmentHistory);
    }

    public async Task<PagedResult<ProjectTeamRegisterRowResponse>?> ListProjectTeamRegisterAsync(ProjectEvidenceListQuery query, CancellationToken cancellationToken)
    {
        if (!await ProjectExistsAsync(query.ProjectId, cancellationToken))
        {
            return null;
        }

        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var baseQuery = BuildEvidenceAssignmentRows(query.ProjectId)
            .Where(x => x.Status == "Active");
        var total = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderBy(x => x.UserDisplayName ?? x.UserEmail ?? x.UserId)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new ProjectTeamRegisterRowResponse(
                x.Id,
                x.UserId,
                x.UserEmail,
                x.UserDisplayName,
                x.ProjectRoleName,
                x.ReportsToDisplayName,
                x.IsPrimary,
                x.Status,
                x.StartAt,
                x.EndAt))
            .ToListAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "list_team_register",
            EntityType: "project",
            EntityId: query.ProjectId.ToString(),
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { total, page, pageSize }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PagedResult<ProjectTeamRegisterRowResponse>(items, total, page, pageSize);
    }

    public async Task<PagedResult<ProjectRoleResponsibilityRowResponse>?> ListProjectRoleResponsibilitiesAsync(ProjectEvidenceListQuery query, CancellationToken cancellationToken)
    {
        if (!await ProjectExistsAsync(query.ProjectId, cancellationToken))
        {
            return null;
        }

        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var assignedRoleIds = await dbContext.UserProjectAssignments
            .AsNoTracking()
            .Where(assignment => assignment.ProjectId == query.ProjectId)
            .Select(assignment => assignment.ProjectRoleId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var source = dbContext.ProjectRoles
            .AsNoTracking()
            .Where(x => assignedRoleIds.Contains(x.Id) && x.DeletedAt == null);
        var total = await source.CountAsync(cancellationToken);

        var roles = await source
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Code,
                x.Description,
                x.Responsibilities,
                x.AuthorityScope
            })
            .ToListAsync(cancellationToken);

        var roleIds = roles.Select(x => x.Id).ToArray();
        var memberCounts = roleIds.Length == 0
            ? new Dictionary<Guid, int>()
            : await dbContext.UserProjectAssignments
                .AsNoTracking()
                .Where(assignment => assignment.Status == "Active" && roleIds.Contains(assignment.ProjectRoleId))
                .GroupBy(assignment => assignment.ProjectRoleId)
                .Select(group => new { RoleId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(x => x.RoleId, x => x.Count, cancellationToken);

        var items = roles
            .Select(x => new ProjectRoleResponsibilityRowResponse(
                x.Id,
                x.Name,
                x.Code,
                x.Description,
                x.Responsibilities,
                x.AuthorityScope,
                memberCounts.TryGetValue(x.Id, out var count) ? count : 0))
            .ToList();

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "list_role_responsibilities",
            EntityType: "project",
            EntityId: query.ProjectId.ToString(),
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { total, page, pageSize }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PagedResult<ProjectRoleResponsibilityRowResponse>(items, total, page, pageSize);
    }

    public async Task<PagedResult<ProjectAssignmentHistoryRowResponse>?> ListProjectAssignmentHistoryAsync(ProjectEvidenceListQuery query, CancellationToken cancellationToken)
    {
        if (!await ProjectExistsAsync(query.ProjectId, cancellationToken))
        {
            return null;
        }

        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var baseQuery = BuildEvidenceAssignmentRows(query.ProjectId);
        var total = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new ProjectAssignmentHistoryRowResponse(
                x.Id,
                x.UserId,
                x.UserEmail,
                x.UserDisplayName,
                x.ProjectRoleName,
                x.Status,
                x.ChangeReason,
                x.ReportsToDisplayName,
                x.IsPrimary,
                x.StartAt,
                x.EndAt,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "list_assignment_history",
            EntityType: "project",
            EntityId: query.ProjectId.ToString(),
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { total, page, pageSize }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PagedResult<ProjectAssignmentHistoryRowResponse>(items, total, page, pageSize);
    }

    public async Task<ProjectEvidenceExportResult?> GetProjectEvidenceExportAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var projectName = await dbContext.Projects
            .AsNoTracking()
            .Where(project => project.Id == projectId && project.DeletedAt == null)
            .Select(project => project.Name)
            .FirstOrDefaultAsync(cancellationToken);
        if (projectName is null)
        {
            return null;
        }

        var teamRegister = await (
            from assignment in dbContext.UserProjectAssignments.AsNoTracking()
            where assignment.ProjectId == projectId && assignment.Status == "Active"
            join user in dbContext.Users.AsNoTracking() on assignment.UserId equals user.Id into userJoin
            from user in userJoin.DefaultIfEmpty()
            join role in dbContext.ProjectRoles.AsNoTracking() on assignment.ProjectRoleId equals role.Id into roleJoin
            from role in roleJoin.DefaultIfEmpty()
            join reportsTo in dbContext.Users.AsNoTracking() on assignment.ReportsToUserId equals reportsTo.Id into reportsJoin
            from reportsTo in reportsJoin.DefaultIfEmpty()
            orderby user != null ? user.Id : assignment.UserId
            select new ProjectTeamRegisterRowResponse(
                assignment.Id,
                assignment.UserId,
                user != null ? user.Id : null,
                user != null ? user.Id : null,
                role != null ? role.Name : string.Empty,
                reportsTo != null ? reportsTo.Id : null,
                assignment.IsPrimary,
                assignment.Status,
                assignment.StartAt,
                assignment.EndAt))
            .ToListAsync(cancellationToken);

        var assignedRoleIds = await dbContext.UserProjectAssignments
            .AsNoTracking()
            .Where(assignment => assignment.ProjectId == projectId)
            .Select(assignment => assignment.ProjectRoleId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var roleRows = assignedRoleIds.Count == 0
            ? []
            : await dbContext.ProjectRoles
                .AsNoTracking()
                .Where(x => assignedRoleIds.Contains(x.Id) && x.DeletedAt == null)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Code,
                    x.Description,
                    x.Responsibilities,
                    x.AuthorityScope
                })
                .ToListAsync(cancellationToken);

        var roleIds = roleRows.Select(x => x.Id).ToArray();
        var roleMemberCounts = roleIds.Length == 0
            ? new Dictionary<Guid, int>()
            : await dbContext.UserProjectAssignments
                .AsNoTracking()
                .Where(assignment => assignment.Status == "Active" && roleIds.Contains(assignment.ProjectRoleId))
                .GroupBy(assignment => assignment.ProjectRoleId)
                .Select(group => new { RoleId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(x => x.RoleId, x => x.Count, cancellationToken);

        var roleResponsibilities = roleRows
            .Select(x => new ProjectRoleResponsibilityRowResponse(
                x.Id,
                x.Name,
                x.Code,
                x.Description,
                x.Responsibilities,
                x.AuthorityScope,
                roleMemberCounts.TryGetValue(x.Id, out var count) ? count : 0))
            .ToList();

        var assignmentHistory = await (
            from assignment in dbContext.UserProjectAssignments.AsNoTracking()
            where assignment.ProjectId == projectId
            join user in dbContext.Users.AsNoTracking() on assignment.UserId equals user.Id into userJoin
            from user in userJoin.DefaultIfEmpty()
            join role in dbContext.ProjectRoles.AsNoTracking() on assignment.ProjectRoleId equals role.Id into roleJoin
            from role in roleJoin.DefaultIfEmpty()
            join reportsTo in dbContext.Users.AsNoTracking() on assignment.ReportsToUserId equals reportsTo.Id into reportsJoin
            from reportsTo in reportsJoin.DefaultIfEmpty()
            orderby assignment.CreatedAt descending
            select new ProjectAssignmentHistoryRowResponse(
                assignment.Id,
                assignment.UserId,
                user != null ? user.Id : null,
                user != null ? user.Id : null,
                role != null ? role.Name : string.Empty,
                assignment.Status,
                assignment.ChangeReason,
                reportsTo != null ? reportsTo.Id : null,
                assignment.IsPrimary,
                assignment.StartAt,
                assignment.EndAt,
                assignment.CreatedAt,
                assignment.UpdatedAt))
            .ToListAsync(cancellationToken);

        static string Csv(string? value)
        {
            var normalized = value ?? string.Empty;
            return $"\"{normalized.Replace("\"", "\"\"")}\"";
        }

        var builder = new StringBuilder();
        builder.AppendLine("Section,Member,Email,Role,Reports To,Primary,Status,Reason,Start At,End At,Created At,Updated At,Role Code,Description,Responsibilities,Authority Scope,Member Count");

        foreach (var row in teamRegister)
        {
            builder.AppendLine(string.Join(",",
                Csv("Team Register"),
                Csv(row.UserDisplayName ?? row.UserId),
                Csv(row.UserEmail),
                Csv(row.ProjectRoleName),
                Csv(row.ReportsToDisplayName),
                Csv(row.IsPrimary ? "Yes" : "No"),
                Csv(row.Status),
                Csv(null),
                Csv(row.StartAt.ToString("O")),
                Csv(row.EndAt?.ToString("O")),
                Csv(null),
                Csv(null),
                Csv(null),
                Csv(null),
                Csv(null),
                Csv(null)));
        }

        foreach (var row in roleResponsibilities)
        {
            builder.AppendLine(string.Join(",",
                Csv("Role Responsibility"),
                Csv(null),
                Csv(null),
                Csv(row.ProjectRoleName),
                Csv(null),
                Csv(null),
                Csv(null),
                Csv(null),
                Csv(null),
                Csv(null),
                Csv(null),
                Csv(null),
                Csv(row.Code),
                Csv(row.Description),
                Csv(row.Responsibilities),
                Csv(row.AuthorityScope),
                Csv(row.MemberCount.ToString())));
        }

        foreach (var row in assignmentHistory)
        {
            builder.AppendLine(string.Join(",",
                Csv("Assignment History"),
                Csv(row.UserDisplayName ?? row.UserId),
                Csv(row.UserEmail),
                Csv(row.ProjectRoleName),
                Csv(row.ReportsToDisplayName),
                Csv(row.IsPrimary ? "Yes" : "No"),
                Csv(row.Status),
                Csv(row.ChangeReason),
                Csv(row.StartAt.ToString("O")),
                Csv(row.EndAt?.ToString("O")),
                Csv(row.CreatedAt.ToString("O")),
                Csv(row.UpdatedAt?.ToString("O")),
                Csv(null),
                Csv(null),
                Csv(null),
                Csv(null),
                Csv(null)));
        }

        var safeProjectName = string.Concat(projectName.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_')).Trim('_');
        var fileName = $"project-evidence-{safeProjectName}-{projectId:N}.csv";

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "export_evidence",
            EntityType: "project",
            EntityId: projectId.ToString(),
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { projectId, fileName }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ProjectEvidenceExportResult(fileName, "text/csv; charset=utf-8", Encoding.UTF8.GetBytes(builder.ToString()));
    }

    public async Task<ProjectComplianceResponse?> GetProjectComplianceAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var project = await dbContext.Projects
            .AsNoTracking()
            .Where(x => x.Id == projectId && x.DeletedAt == null)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.ProjectType,
                x.Status,
                x.OwnerUserId,
                x.SponsorUserId,
                x.PlannedStartAt,
                x.PlannedEndAt,
                x.StartAt,
                x.EndAt
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (project is null)
        {
            return null;
        }

        var assignments = await dbContext.UserProjectAssignments
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId && x.Status == "Active")
            .Select(x => new
            {
                x.UserId,
                x.ProjectRoleId,
                x.ReportsToUserId,
                x.IsPrimary
            })
            .ToListAsync(cancellationToken);

        var assignedRoleIds = assignments
            .Select(x => x.ProjectRoleId)
            .Distinct()
            .ToList();

        var roleFacts = assignedRoleIds.Count == 0
            ? []
            : await dbContext.ProjectRoles
                .AsNoTracking()
                .Where(x => assignedRoleIds.Contains(x.Id) && x.DeletedAt == null)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Code
                })
                .ToListAsync(cancellationToken);

        var template = await dbContext.ProjectTypeTemplates
            .AsNoTracking()
            .Where(x => x.ProjectType == project.ProjectType && x.DeletedAt == null)
            .Select(x => new
            {
                x.Id,
                x.RequireSponsor,
                x.RequirePlannedPeriod,
                x.RequireActiveTeam,
                x.RequirePrimaryAssignment,
                x.RequireReportingRoot
            })
            .FirstOrDefaultAsync(cancellationToken);

        var templateRoleRequirements = template is null
            ? []
            : await dbContext.ProjectTypeRoleRequirements
                .AsNoTracking()
                .Where(x => x.ProjectTypeTemplateId == template.Id && x.DeletedAt == null)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.RoleName)
                .Select(x => new
                {
                    x.RoleName,
                    x.RoleCode,
                    x.Description
                })
                .ToListAsync(cancellationToken);

        var hasOwner = !string.IsNullOrWhiteSpace(project.OwnerUserId);
        var requiresSponsor = template?.RequireSponsor ??
            (string.Equals(project.ProjectType, "Customer", StringComparison.OrdinalIgnoreCase)
            || string.Equals(project.ProjectType, "Compliance", StringComparison.OrdinalIgnoreCase));
        var hasSponsor = !string.IsNullOrWhiteSpace(project.SponsorUserId);
        var requirePlannedPeriod = template?.RequirePlannedPeriod ?? true;
        var requireActiveTeam = template?.RequireActiveTeam ?? true;
        var requirePrimaryAssignment = template?.RequirePrimaryAssignment ?? true;
        var requireReportingRoot = template?.RequireReportingRoot ?? true;
        var hasPlannedPeriod = project.PlannedStartAt.HasValue && project.PlannedEndAt.HasValue && project.PlannedStartAt <= project.PlannedEndAt;
        var hasActiveMembers = assignments.Count > 0;
        var hasReportingRoot = assignments.Any(x => string.IsNullOrWhiteSpace(x.ReportsToUserId));
        var assignedRoleIdSet = assignedRoleIds.ToHashSet();
        var assignedRoles = roleFacts.Where(x => assignedRoleIdSet.Contains(x.Id)).ToList();
        var hasPrimaryAssignment = assignments.Any(x => x.IsPrimary);

        List<ProjectComplianceCheckResponse> checks =
        [
            BuildComplianceCheck(
                code: "owner_assigned",
                title: "Project owner assigned",
                description: "The project must have a named owner who is accountable for delivery.",
                severity: "error",
                isPassing: hasOwner,
                passedDetail: $"Owner user id: {project.OwnerUserId}",
                failedDetail: "Assign a project owner."),
            BuildComplianceCheck(
                code: "sponsor_assigned",
                title: "Project sponsor assigned",
                description: requiresSponsor
                    ? "Customer and compliance projects must have a sponsor."
                    : "Sponsor is optional for this project type.",
                severity: requiresSponsor ? "warning" : "info",
                isPassing: !requiresSponsor || hasSponsor,
                passedDetail: hasSponsor ? $"Sponsor user id: {project.SponsorUserId}" : "Sponsor not required.",
                failedDetail: "Assign a sponsor for this project type."),
            BuildComplianceCheck(
                code: "planned_period_defined",
                title: "Planned period defined",
                description: "Planned start and end dates should be set for schedule governance.",
                severity: requirePlannedPeriod ? "warning" : "info",
                isPassing: !requirePlannedPeriod || hasPlannedPeriod,
                passedDetail: $"Planned: {project.PlannedStartAt:yyyy-MM-dd} to {project.PlannedEndAt:yyyy-MM-dd}",
                failedDetail: requirePlannedPeriod ? "Set valid planned start and end dates." : "Planned period not required."),
            BuildComplianceCheck(
                code: "active_team_exists",
                title: "Active project team exists",
                description: "The project should have at least one active member assignment.",
                severity: requireActiveTeam ? "error" : "info",
                isPassing: !requireActiveTeam || hasActiveMembers,
                passedDetail: $"Active members: {assignments.Count}",
                failedDetail: requireActiveTeam ? "Assign active members to the project." : "Active team not required."),
            BuildComplianceCheck(
                code: "primary_assignment_exists",
                title: "Primary assignment exists",
                description: "At least one member should be marked as the primary project assignment.",
                severity: requirePrimaryAssignment ? "warning" : "info",
                isPassing: !requirePrimaryAssignment || hasPrimaryAssignment,
                passedDetail: "Primary assignment found.",
                failedDetail: requirePrimaryAssignment ? "Mark one active member as primary." : "Primary assignment not required."),
            BuildComplianceCheck(
                code: "reporting_root_exists",
                title: "Reporting root exists",
                description: "The project org chart should have at least one top-level reporting root.",
                severity: requireReportingRoot ? "warning" : "info",
                isPassing: !requireReportingRoot || !hasActiveMembers || hasReportingRoot,
                passedDetail: "Reporting root found.",
                failedDetail: requireReportingRoot ? "Set at least one project member without a reports-to reference." : "Reporting root not required."),
            BuildComplianceCheck(
                code: "document_creator_role",
                title: "Document creator role assigned",
                description: "Document roles are defined in workflow configuration.",
                severity: "info",
                isPassing: true,
                passedDetail: "Workflow configuration will define document roles.",
                failedDetail: null),
            BuildComplianceCheck(
                code: "review_role_assigned",
                title: "Review role assigned",
                description: "Document roles are defined in workflow configuration.",
                severity: "info",
                isPassing: true,
                passedDetail: "Workflow configuration will define review roles.",
                failedDetail: null),
            BuildComplianceCheck(
                code: "approval_role_assigned",
                title: "Approval role assigned",
                description: "Document roles are defined in workflow configuration.",
                severity: "info",
                isPassing: true,
                passedDetail: "Workflow configuration will define approval roles.",
                failedDetail: null),
            BuildComplianceCheck(
                code: "release_role_assigned",
                title: "Release role assigned",
                description: "Document roles are defined in workflow configuration.",
                severity: "info",
                isPassing: true,
                passedDetail: "Workflow configuration will define release roles.",
                failedDetail: null)
        ];

        foreach (var requirement in templateRoleRequirements)
        {
            var matched = assignedRoles.Any(role =>
                (!string.IsNullOrWhiteSpace(requirement.RoleCode) && string.Equals(role.Code, requirement.RoleCode, StringComparison.OrdinalIgnoreCase)) ||
                string.Equals(role.Name, requirement.RoleName, StringComparison.OrdinalIgnoreCase));

            checks.Add(BuildComplianceCheck(
                code: $"required_role_{SanitizeComplianceCode(requirement.RoleCode ?? requirement.RoleName)}",
                title: $"Required role: {requirement.RoleName}",
                description: requirement.Description ?? "A required project role should be actively assigned.",
                severity: "error",
                isPassing: matched,
                passedDetail: $"Role '{requirement.RoleName}' is covered by an active assignment.",
                failedDetail: $"Assign the required role '{requirement.RoleName}' to an active project member."));
        }

        var passedChecks = checks.Count(x => x.Status == "passed");
        var warningChecks = checks.Count(x => x.Status == "warning");
        var failedChecks = checks.Count(x => x.Status == "failed");

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "view_compliance",
            EntityType: "project",
            EntityId: projectId.ToString(),
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { projectId, passedChecks, warningChecks, failedChecks }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ProjectComplianceResponse(
            project.Id,
            project.Name,
            project.ProjectType,
            project.Status,
            passedChecks,
            warningChecks,
            failedChecks,
            checks);
    }

    private static string SanitizeComplianceCode(string value)
    {
        Span<char> buffer = stackalloc char[value.Length];
        var index = 0;
        foreach (var character in value)
        {
            buffer[index++] = char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : '_';
        }

        return new string(buffer[..index]);
    }

    private static ProjectComplianceCheckResponse BuildComplianceCheck(
        string code,
        string title,
        string description,
        string severity,
        bool isPassing,
        string passedDetail,
        string failedDetail)
    {
        var status = isPassing ? "passed" : severity switch
        {
            "info" => "warning",
            _ => severity == "warning" ? "warning" : "failed"
        };

        return new ProjectComplianceCheckResponse(
            code,
            title,
            description,
            severity,
            status,
            isPassing ? passedDetail : failedDetail);
    }

    private static IQueryable<ProjectEntity> ApplyProjectSorting(IQueryable<ProjectEntity> source, string? sortBy, string? sortOrder)
    {
        var descending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "code" => descending ? source.OrderByDescending(x => x.Code) : source.OrderBy(x => x.Code),
            "projecttype" => descending ? source.OrderByDescending(x => x.ProjectType) : source.OrderBy(x => x.ProjectType),
            "phase" => descending ? source.OrderByDescending(x => x.Phase) : source.OrderBy(x => x.Phase),
            "status" => descending ? source.OrderByDescending(x => x.Status) : source.OrderBy(x => x.Status),
            "plannedstartat" => descending ? source.OrderByDescending(x => x.PlannedStartAt) : source.OrderBy(x => x.PlannedStartAt),
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

    private static (int Page, int PageSize, int Skip) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);
        var skip = (normalizedPage - 1) * normalizedPageSize;
        return (normalizedPage, normalizedPageSize, skip);
    }

    private Task<bool> ProjectExistsAsync(Guid projectId, CancellationToken cancellationToken) =>
        dbContext.Projects.AsNoTracking().AnyAsync(project => project.Id == projectId && project.DeletedAt == null, cancellationToken);

    private IQueryable<ProjectEvidenceAssignmentRow> BuildEvidenceAssignmentRows(Guid projectId)
    {
        return from assignment in dbContext.UserProjectAssignments.AsNoTracking()
               where assignment.ProjectId == projectId
               join user in dbContext.Users.AsNoTracking() on assignment.UserId equals user.Id into userJoin
               from user in userJoin.DefaultIfEmpty()
               join role in dbContext.ProjectRoles.AsNoTracking() on assignment.ProjectRoleId equals role.Id into roleJoin
               from role in roleJoin.DefaultIfEmpty()
               join reportsTo in dbContext.Users.AsNoTracking() on assignment.ReportsToUserId equals reportsTo.Id into reportsJoin
               from reportsTo in reportsJoin.DefaultIfEmpty()
               select new ProjectEvidenceAssignmentRow(
                   assignment.Id,
                   assignment.UserId,
                   user != null ? user.Id : null,
                   user != null ? user.Id : null,
                   assignment.ProjectRoleId,
                   role != null ? role.Name : string.Empty,
                   assignment.Status,
                   assignment.ChangeReason,
                   reportsTo != null ? reportsTo.Id : null,
                   assignment.IsPrimary,
                   assignment.StartAt,
                   assignment.EndAt,
                   assignment.CreatedAt,
                   assignment.UpdatedAt);
    }
}

sealed record ProjectEvidenceAssignmentRow(
    Guid Id,
    string UserId,
    string? UserEmail,
    string? UserDisplayName,
    Guid ProjectRoleId,
    string ProjectRoleName,
    string Status,
    string? ChangeReason,
    string? ReportsToDisplayName,
    bool IsPrimary,
    DateTimeOffset StartAt,
    DateTimeOffset? EndAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
