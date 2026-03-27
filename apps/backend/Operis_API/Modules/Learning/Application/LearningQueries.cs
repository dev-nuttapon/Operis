using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Learning.Contracts;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Learning.Application;

public sealed class LearningQueries(OperisDbContext dbContext) : ILearningQueries
{
    public async Task<PagedResult<TrainingCourseResponse>> ListTrainingCoursesAsync(TrainingCourseListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var source = dbContext.TrainingCourses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.Title, search) ||
                (x.CourseCode != null && EF.Functions.ILike(x.CourseCode, search)) ||
                (x.Provider != null && EF.Functions.ILike(x.Provider, search)));
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = NormalizeKey(query.Status);
            source = source.Where(x => x.Status == status);
        }

        var total = await source.CountAsync(cancellationToken);
        var requirementCounts = await dbContext.RoleTrainingRequirements.AsNoTracking()
            .GroupBy(x => x.CourseId)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        var items = await source
            .OrderBy(x => x.Title)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new TrainingCourseResponse(
                x.Id,
                x.CourseCode,
                x.Title,
                x.Description,
                x.Provider,
                x.DeliveryMode,
                x.AudienceScope,
                x.ValidityMonths,
                x.Status,
                0,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<TrainingCourseResponse>(
            items.Select(item => item with { RequirementCount = requirementCounts.GetValueOrDefault(item.Id) }).ToList(),
            total,
            page,
            pageSize);
    }

    public async Task<PagedResult<RoleTrainingRequirementResponse>> ListRoleTrainingRequirementsAsync(RoleTrainingMatrixQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var source =
            from requirement in dbContext.RoleTrainingRequirements.AsNoTracking()
            join course in dbContext.TrainingCourses.AsNoTracking() on requirement.CourseId equals course.Id
            join role in dbContext.ProjectRoles.AsNoTracking() on requirement.ProjectRoleId equals role.Id
            join project in dbContext.Projects.AsNoTracking() on role.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            where role.DeletedAt == null
            select new { requirement, course, role, project };

        if (query.ProjectId.HasValue)
        {
            source = source.Where(x => x.role.ProjectId == query.ProjectId.Value);
        }

        if (query.ProjectRoleId.HasValue)
        {
            source = source.Where(x => x.requirement.ProjectRoleId == query.ProjectRoleId.Value);
        }

        if (query.CourseId.HasValue)
        {
            source = source.Where(x => x.requirement.CourseId == query.CourseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = NormalizeKey(query.Status);
            source = source.Where(x => x.requirement.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.course.Title, search) ||
                EF.Functions.ILike(x.role.Name, search) ||
                (x.project != null && EF.Functions.ILike(x.project.Name, search)));
        }

        var rows = await source
            .OrderBy(x => x.course.Title)
            .ThenBy(x => x.role.Name)
            .ToListAsync(cancellationToken);

        var roleIds = rows.Select(x => x.requirement.ProjectRoleId).Distinct().ToArray();
        var assignments = await dbContext.UserProjectAssignments.AsNoTracking()
            .Where(x => roleIds.Contains(x.ProjectRoleId) && x.Status == "Active")
            .Select(x => new AssignmentRow(x.ProjectRoleId, x.ProjectId, x.UserId, x.StartAt))
            .ToListAsync(cancellationToken);

        var courseIds = rows.Select(x => x.requirement.CourseId).Distinct().ToArray();
        var completionRows = await dbContext.TrainingCompletions.AsNoTracking()
            .Where(x => courseIds.Contains(x.CourseId))
            .Select(x => new CompletionRow(x.CourseId, x.ProjectRoleId, x.ProjectId, x.UserId, x.DueAt, x.CompletionDate, x.ExpiryDate))
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var items = rows.Select(row =>
        {
            var roleAssignments = assignments.Where(x => x.ProjectRoleId == row.requirement.ProjectRoleId).ToList();
            var assignedUserCount = roleAssignments.Count;
            var overdueUserCount = 0;
            var expiredUserCount = 0;

            foreach (var assignment in roleAssignments)
            {
                var completion = completionRows.FirstOrDefault(x =>
                    x.CourseId == row.requirement.CourseId &&
                    x.ProjectRoleId == row.requirement.ProjectRoleId &&
                    x.ProjectId == assignment.ProjectId &&
                    string.Equals(x.UserId, assignment.UserId, StringComparison.Ordinal));
                var dueAt = completion?.DueAt ?? assignment.StartAt.AddDays(Math.Max(row.requirement.RequiredWithinDays, 0));
                var expiryAt = completion?.ExpiryDate ?? ComputeExpiry(completion?.CompletionDate, row.requirement.RenewalIntervalMonths, row.course.ValidityMonths);
                if (expiryAt.HasValue && expiryAt.Value < now)
                {
                    expiredUserCount++;
                    continue;
                }

                if (completion?.CompletionDate is null && dueAt < now)
                {
                    overdueUserCount++;
                }
            }

            return new RoleTrainingRequirementResponse(
                row.requirement.Id,
                row.requirement.CourseId,
                row.course.Title,
                row.course.CourseCode,
                row.course.Status,
                row.requirement.ProjectRoleId,
                row.role.Name,
                row.role.ProjectId,
                row.project?.Name,
                row.requirement.RequiredWithinDays,
                row.requirement.RenewalIntervalMonths,
                row.requirement.Status,
                row.requirement.Notes,
                assignedUserCount,
                overdueUserCount,
                expiredUserCount,
                row.requirement.CreatedAt,
                row.requirement.UpdatedAt);
        }).ToList();

        return new PagedResult<RoleTrainingRequirementResponse>(items.Skip(skip).Take(pageSize).ToList(), items.Count, page, pageSize);
    }

    public async Task<PagedResult<TrainingCompletionResponse>> ListTrainingCompletionsAsync(TrainingCompletionListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var source =
            from requirement in dbContext.RoleTrainingRequirements.AsNoTracking()
            join course in dbContext.TrainingCourses.AsNoTracking() on requirement.CourseId equals course.Id
            join role in dbContext.ProjectRoles.AsNoTracking() on requirement.ProjectRoleId equals role.Id
            join assignment in dbContext.UserProjectAssignments.AsNoTracking() on role.Id equals assignment.ProjectRoleId
            join project in dbContext.Projects.AsNoTracking() on assignment.ProjectId equals project.Id
            where role.DeletedAt == null
                  && assignment.Status == "Active"
                  && requirement.Status == "active"
            select new { requirement, course, role, assignment, project };

        if (query.ProjectId.HasValue)
        {
            source = source.Where(x => x.assignment.ProjectId == query.ProjectId.Value);
        }

        if (query.ProjectRoleId.HasValue)
        {
            source = source.Where(x => x.assignment.ProjectRoleId == query.ProjectRoleId.Value);
        }

        if (query.CourseId.HasValue)
        {
            source = source.Where(x => x.requirement.CourseId == query.CourseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.UserId))
        {
            source = source.Where(x => x.assignment.UserId == query.UserId!.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.course.Title, search) ||
                EF.Functions.ILike(x.role.Name, search) ||
                EF.Functions.ILike(x.project.Name, search) ||
                EF.Functions.ILike(x.assignment.UserId, search));
        }

        var rows = await source.ToListAsync(cancellationToken);
        var courseIds = rows.Select(x => x.requirement.CourseId).Distinct().ToArray();
        var projectRoleIds = rows.Select(x => x.assignment.ProjectRoleId).Distinct().ToArray();
        var projectIds = rows.Select(x => x.assignment.ProjectId).Distinct().ToArray();
        var userIds = rows.Select(x => x.assignment.UserId).Distinct().ToArray();

        var completions = await dbContext.TrainingCompletions.AsNoTracking()
            .Where(x => courseIds.Contains(x.CourseId) && projectRoleIds.Contains(x.ProjectRoleId) && projectIds.Contains(x.ProjectId) && userIds.Contains(x.UserId))
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var items = rows.Select(row =>
        {
            var completion = completions.FirstOrDefault(x =>
                x.CourseId == row.requirement.CourseId &&
                x.ProjectRoleId == row.assignment.ProjectRoleId &&
                x.ProjectId == row.assignment.ProjectId &&
                string.Equals(x.UserId, row.assignment.UserId, StringComparison.Ordinal));
            var assignedAt = completion?.AssignedAt ?? row.assignment.StartAt;
            var dueAt = completion?.DueAt ?? row.assignment.StartAt.AddDays(Math.Max(row.requirement.RequiredWithinDays, 0));
            var expiryAt = completion?.ExpiryDate ?? ComputeExpiry(completion?.CompletionDate, row.requirement.RenewalIntervalMonths, row.course.ValidityMonths);
            var isExpired = expiryAt.HasValue && expiryAt.Value < now;
            var isOverdue = !isExpired && completion?.CompletionDate is null && dueAt < now;
            var status = isExpired ? "expired" : NormalizeKey(completion?.Status) ?? "assigned";

            return new TrainingCompletionResponse(
                completion?.Id ?? Guid.Empty,
                row.requirement.CourseId,
                row.course.Title,
                row.course.CourseCode,
                row.assignment.ProjectRoleId,
                row.role.Name,
                row.assignment.ProjectId,
                row.project.Name,
                row.assignment.UserId,
                status,
                isOverdue,
                isExpired,
                assignedAt,
                dueAt,
                completion?.CompletionDate,
                expiryAt,
                completion?.EvidenceRef,
                completion?.Notes,
                completion?.UpdatedAt ?? assignedAt);
        }).ToList();

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = NormalizeKey(query.Status);
            items = items.Where(x => x.Status == status).ToList();
        }

        if (query.OnlyOverdue)
        {
            items = items.Where(x => x.IsOverdue || x.IsExpired).ToList();
        }

        return new PagedResult<TrainingCompletionResponse>(
            items.OrderByDescending(x => x.IsExpired).ThenByDescending(x => x.IsOverdue).ThenBy(x => x.DueAt).Skip(skip).Take(pageSize).ToList(),
            items.Count,
            page,
            pageSize);
    }

    public async Task<PagedResult<CompetencyReviewResponse>> ListCompetencyReviewsAsync(CompetencyReviewListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var source =
            from review in dbContext.CompetencyReviews.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on review.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            select new { review, project };

        if (query.ProjectId.HasValue)
        {
            source = source.Where(x => x.review.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.UserId))
        {
            source = source.Where(x => x.review.UserId == query.UserId!.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = NormalizeKey(query.Status);
            source = source.Where(x => x.review.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.review.UserId, search) ||
                EF.Functions.ILike(x.review.ReviewPeriod, search) ||
                (x.project != null && EF.Functions.ILike(x.project.Name, search)));
        }

        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .OrderByDescending(x => x.review.PlannedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new CompetencyReviewResponse(
                x.review.Id,
                x.review.UserId,
                x.review.ProjectId,
                x.project != null ? x.project.Name : null,
                x.review.ReviewPeriod,
                x.review.ReviewerUserId,
                x.review.Status,
                x.review.Summary,
                x.review.PlannedAt,
                x.review.CompletedAt,
                x.review.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<CompetencyReviewResponse>(items, total, page, pageSize);
    }

    public async Task<IReadOnlyList<ProjectRoleOptionResponse>> ListProjectRoleOptionsAsync(Guid? projectId, CancellationToken cancellationToken)
    {
        var source =
            from role in dbContext.ProjectRoles.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on role.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            where role.DeletedAt == null
            select new { role, project };

        if (projectId.HasValue)
        {
            source = source.Where(x => x.role.ProjectId == projectId.Value);
        }

        return await source
            .OrderBy(x => x.project != null ? x.project.Name : string.Empty)
            .ThenBy(x => x.role.Name)
            .Select(x => new ProjectRoleOptionResponse(
                x.role.Id,
                x.role.ProjectId,
                x.project != null ? x.project.Name : null,
                x.role.Name,
                x.role.Status))
            .ToListAsync(cancellationToken);
    }

    private static (int Page, int PageSize, int Skip) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 25 : Math.Min(pageSize, 100);
        return (normalizedPage, normalizedPageSize, (normalizedPage - 1) * normalizedPageSize);
    }

    private static string? NormalizeKey(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static DateTimeOffset? ComputeExpiry(DateTimeOffset? completionDate, int renewalIntervalMonths, int validityMonths)
    {
        if (!completionDate.HasValue)
        {
            return null;
        }

        var months = renewalIntervalMonths > 0 ? renewalIntervalMonths : Math.Max(validityMonths, 0);
        return months > 0 ? completionDate.Value.AddMonths(months) : null;
    }

    private sealed record AssignmentRow(Guid ProjectRoleId, Guid ProjectId, string UserId, DateTimeOffset StartAt);
    private sealed record CompletionRow(Guid CourseId, Guid ProjectRoleId, Guid ProjectId, string UserId, DateTimeOffset? DueAt, DateTimeOffset? CompletionDate, DateTimeOffset? ExpiryDate);
}
