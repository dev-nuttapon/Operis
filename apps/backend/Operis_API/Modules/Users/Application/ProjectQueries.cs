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

file sealed record ProjectOrgChartRow(
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

file sealed record ProjectAssignmentRow(
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
    Task<PagedResult<ProjectResponse>> ListProjectsAsync(ProjectListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<ProjectRoleResponse>> ListProjectRolesAsync(ReferenceDataQuery query, CancellationToken cancellationToken);
    Task<PagedResult<ProjectAssignmentResponse>> ListProjectAssignmentsAsync(ProjectAssignmentListQuery query, CancellationToken cancellationToken);
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
    IAuditLogWriter auditLogWriter) : IProjectQueries
{
    public async Task<PagedResult<ProjectResponse>> ListProjectsAsync(ProjectListQuery query, CancellationToken cancellationToken)
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
            var search = query.Search.Trim().ToLowerInvariant();
            source = source.Where(x => x.Name.ToLower().Contains(search) || x.Code.ToLower().Contains(search));
        }

        source = ApplyProjectSorting(source, query.SortBy, query.SortOrder);
        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new
            {
                Entity = x,
                OwnerDisplayName = dbContext.Users
                    .Where(user => user.Id == x.OwnerUserId && user.DeletedAt == null)
                    .Select(user => user.Id)
                    .FirstOrDefault(),
                SponsorDisplayName = dbContext.Users
                    .Where(user => user.Id == x.SponsorUserId && user.DeletedAt == null)
                    .Select(user => user.Id)
                    .FirstOrDefault()
            })
            .Select(x => new ProjectResponse(
                x.Entity.Id,
                x.Entity.Code,
                x.Entity.Name,
                x.Entity.ProjectType,
                x.Entity.OwnerUserId,
                x.OwnerDisplayName,
                x.Entity.SponsorUserId,
                x.SponsorDisplayName,
                x.Entity.Methodology,
                x.Entity.Phase,
                x.Entity.Status,
                x.Entity.StatusReason,
                x.Entity.PlannedStartAt,
                x.Entity.PlannedEndAt,
                x.Entity.StartAt,
                x.Entity.EndAt,
                x.Entity.CreatedAt,
                x.Entity.UpdatedAt,
                x.Entity.DeletedReason,
                x.Entity.DeletedBy,
                x.Entity.DeletedAt))
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
                x.Code,
                x.Description,
                x.Responsibilities,
                x.AuthorityScope,
                x.CanCreateDocuments,
                x.CanReviewDocuments,
                x.CanApproveDocuments,
                x.CanReleaseDocuments,
                x.IsReviewRole,
                x.IsApprovalRole,
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
        var source = from assignment in dbContext.UserProjectAssignments.AsNoTracking()
                     where assignment.ProjectId == query.ProjectId && assignment.Status == "Active"
                     join user in dbContext.Users.AsNoTracking() on assignment.UserId equals user.Id into userJoin
                     from user in userJoin.DefaultIfEmpty()
                     join role in dbContext.ProjectRoles.AsNoTracking() on assignment.ProjectRoleId equals role.Id into roleJoin
                     from role in roleJoin.DefaultIfEmpty()
                     join project in dbContext.Projects.AsNoTracking() on assignment.ProjectId equals project.Id into projectJoin
                     from project in projectJoin.DefaultIfEmpty()
                     join reportsTo in dbContext.Users.AsNoTracking() on assignment.ReportsToUserId equals reportsTo.Id into reportsJoin
                     from reportsTo in reportsJoin.DefaultIfEmpty()
                     select new ProjectAssignmentRow(
                         assignment.Id,
                         assignment.UserId,
                         user != null ? user.Id : null,
                         user != null ? user.Id : null,
                         assignment.ProjectId,
                         project != null ? project.Name : string.Empty,
                         assignment.ProjectRoleId,
                         role != null ? role.Name : string.Empty,
                         assignment.ReportsToUserId,
                         reportsTo != null ? reportsTo.Id : null,
                         assignment.IsPrimary,
                         assignment.Status,
                         assignment.ChangeReason,
                         assignment.ReplacedByAssignmentId,
                         assignment.StartAt,
                         assignment.EndAt,
                         assignment.CreatedAt,
                         assignment.UpdatedAt);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchPattern = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.UserId, searchPattern)
                || (x.UserEmail != null && EF.Functions.ILike(x.UserEmail, searchPattern))
                || EF.Functions.ILike(x.ProjectRoleName, searchPattern));
        }

        source = ApplyProjectAssignmentSorting(source, query.SortBy, query.SortOrder);
        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new ProjectAssignmentResponse(
                x.Id,
                x.UserId,
                x.UserEmail,
                x.UserDisplayName,
                x.ProjectId,
                x.ProjectName,
                x.ProjectRoleId,
                x.ProjectRoleName,
                x.ReportsToUserId,
                x.ReportsToDisplayName,
                x.IsPrimary,
                x.Status,
                x.ChangeReason,
                x.ReplacedByAssignmentId,
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

        var roleRows = await dbContext.ProjectRoles
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId && x.DeletedAt == null)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Code,
                x.Description,
                x.Responsibilities,
                x.AuthorityScope,
                x.CanCreateDocuments,
                x.CanReviewDocuments,
                x.CanApproveDocuments,
                x.CanReleaseDocuments,
                x.IsReviewRole,
                x.IsApprovalRole,
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
                x.CanCreateDocuments,
                x.CanReviewDocuments,
                x.CanApproveDocuments,
                x.CanReleaseDocuments,
                x.IsReviewRole,
                x.IsApprovalRole,
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
        var source = dbContext.ProjectRoles
            .AsNoTracking()
            .Where(x => x.ProjectId == query.ProjectId && x.DeletedAt == null);
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
                x.AuthorityScope,
                x.CanCreateDocuments,
                x.CanReviewDocuments,
                x.CanApproveDocuments,
                x.CanReleaseDocuments,
                x.IsReviewRole,
                x.IsApprovalRole,
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
                x.CanCreateDocuments,
                x.CanReviewDocuments,
                x.CanApproveDocuments,
                x.CanReleaseDocuments,
                x.IsReviewRole,
                x.IsApprovalRole,
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
        var evidence = await GetProjectEvidenceAsync(projectId, cancellationToken);
        if (evidence is null)
        {
            return null;
        }

        static string Csv(string? value)
        {
            var normalized = value ?? string.Empty;
            return $"\"{normalized.Replace("\"", "\"\"")}\"";
        }

        var builder = new StringBuilder();
        builder.AppendLine("Section,Member,Email,Role,Reports To,Primary,Status,Reason,Start At,End At,Created At,Updated At,Role Code,Description,Responsibilities,Authority Scope,Can Create,Can Review,Can Approve,Can Release,Is Review Role,Is Approval Role,Member Count");

        foreach (var row in evidence.TeamRegister)
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
                Csv(null),
                Csv(null),
                Csv(null),
                Csv(null),
                Csv(null),
                Csv(null),
                Csv(null),
                Csv(null)));
        }

        foreach (var row in evidence.RoleResponsibilities)
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
                Csv(row.CanCreateDocuments ? "Yes" : "No"),
                Csv(row.CanReviewDocuments ? "Yes" : "No"),
                Csv(row.CanApproveDocuments ? "Yes" : "No"),
                Csv(row.CanReleaseDocuments ? "Yes" : "No"),
                Csv(row.IsReviewRole ? "Yes" : "No"),
                Csv(row.IsApprovalRole ? "Yes" : "No"),
                Csv(row.MemberCount.ToString())));
        }

        foreach (var row in evidence.AssignmentHistory)
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
                Csv(null),
                Csv(null),
                Csv(null),
                Csv(null),
                Csv(null),
                Csv(null),
                Csv(null)));
        }

        var safeProjectName = string.Concat(evidence.ProjectName.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_')).Trim('_');
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

        var roleFacts = await dbContext.ProjectRoles
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId && x.DeletedAt == null)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Code,
                x.CanCreateDocuments,
                x.CanReviewDocuments,
                x.CanApproveDocuments,
                x.CanReleaseDocuments,
                x.IsReviewRole,
                x.IsApprovalRole
            })
            .ToListAsync(cancellationToken);

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
                x.RequireReportingRoot,
                x.RequireDocumentCreator,
                x.RequireReviewer,
                x.RequireApprover,
                x.RequireReleaseRole
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
        var requireDocumentCreator = template?.RequireDocumentCreator ?? true;
        var requireReviewer = template?.RequireReviewer ?? true;
        var requireApprover = template?.RequireApprover ?? true;
        var requireReleaseRole = template?.RequireReleaseRole ?? false;
        var hasPlannedPeriod = project.PlannedStartAt.HasValue && project.PlannedEndAt.HasValue && project.PlannedStartAt <= project.PlannedEndAt;
        var hasActiveMembers = assignments.Count > 0;
        var hasReportingRoot = assignments.Any(x => string.IsNullOrWhiteSpace(x.ReportsToUserId));
        var assignedRoleIds = assignments.Select(x => x.ProjectRoleId).ToHashSet();
        var assignedRoles = roleFacts.Where(x => assignedRoleIds.Contains(x.Id)).ToList();
        var hasDocumentCreator = assignedRoles.Any(x => x.CanCreateDocuments);
        var hasReviewer = assignedRoles.Any(x => x.CanReviewDocuments || x.IsReviewRole);
        var hasApprover = assignedRoles.Any(x => x.CanApproveDocuments || x.IsApprovalRole);
        var hasReleaseRole = assignedRoles.Any(x => x.CanReleaseDocuments);
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
                description: "An active role should be able to create project documents.",
                severity: requireDocumentCreator ? "error" : "info",
                isPassing: !requireDocumentCreator || hasDocumentCreator,
                passedDetail: "Document creation capability covered.",
                failedDetail: requireDocumentCreator ? "Assign a role with document creation permission to an active member." : "Document creator role not required."),
            BuildComplianceCheck(
                code: "review_role_assigned",
                title: "Review role assigned",
                description: "An active role should be able to review project deliverables.",
                severity: requireReviewer ? "warning" : "info",
                isPassing: !requireReviewer || hasReviewer,
                passedDetail: "Review capability covered.",
                failedDetail: requireReviewer ? "Assign a review-capable role to an active member." : "Review role not required."),
            BuildComplianceCheck(
                code: "approval_role_assigned",
                title: "Approval role assigned",
                description: "An active role should be able to approve project deliverables.",
                severity: requireApprover ? "warning" : "info",
                isPassing: !requireApprover || hasApprover,
                passedDetail: "Approval capability covered.",
                failedDetail: requireApprover ? "Assign an approval-capable role to an active member." : "Approval role not required."),
            BuildComplianceCheck(
                code: "release_role_assigned",
                title: "Release role assigned",
                description: "An active role should be able to release controlled documents when required.",
                severity: requireReleaseRole ? "warning" : "info",
                isPassing: !requireReleaseRole || hasReleaseRole,
                passedDetail: "Release capability covered.",
                failedDetail: requireReleaseRole ? "Assign a release-capable role for controlled releases." : "Release role not required.")
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

    private static IQueryable<ProjectAssignmentRow> ApplyProjectAssignmentSorting(IQueryable<ProjectAssignmentRow> source, string? sortBy, string? sortOrder)
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

file sealed record ProjectEvidenceAssignmentRow(
    Guid Id,
    string UserId,
    string? UserEmail,
    string? UserDisplayName,
    string ProjectRoleName,
    string Status,
    string? ChangeReason,
    string? ReportsToDisplayName,
    bool IsPrimary,
    DateTimeOffset StartAt,
    DateTimeOffset? EndAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
