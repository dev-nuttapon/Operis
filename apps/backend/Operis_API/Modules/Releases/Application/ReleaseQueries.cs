using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Releases.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Releases.Application;

public sealed class ReleaseQueries(OperisDbContext dbContext) : IReleaseQueries
{
    public async Task<PagedResult<ReleaseListItem>> ListReleasesAsync(ReleaseListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var baseQuery =
            from release in dbContext.Releases.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on release.ProjectId equals project.Id
            join gate in dbContext.QualityGateResults.AsNoTracking() on release.QualityGateResultId equals gate.Id into gateJoin
            from gate in gateJoin.DefaultIfEmpty()
            select new { Release = release, ProjectName = project.Name, GateResult = gate != null ? gate.Result : null };

        if (query.ProjectId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Release.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToLowerInvariant();
            baseQuery = baseQuery.Where(x => x.Release.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            baseQuery = baseQuery.Where(x =>
                EF.Functions.ILike(x.Release.ReleaseCode, $"%{search}%") ||
                EF.Functions.ILike(x.Release.Title, $"%{search}%") ||
                EF.Functions.ILike(x.ProjectName, $"%{search}%"));
        }

        var rows = await baseQuery
            .OrderByDescending(x => x.Release.PlannedAt ?? x.Release.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var total = await baseQuery.CountAsync(cancellationToken);

        var releaseIds = rows.Select(x => x.Release.Id).ToArray();
        var checklistStats = await dbContext.DeploymentChecklists.AsNoTracking()
            .Where(x => releaseIds.Contains(x.ReleaseId))
            .GroupBy(x => x.ReleaseId)
            .Select(group => new
            {
                ReleaseId = group.Key,
                Total = group.Count(),
                Completed = group.Count(item => item.Status == "executed")
            })
            .ToDictionaryAsync(x => x.ReleaseId, cancellationToken);

        var items = rows.Select(x =>
        {
            checklistStats.TryGetValue(x.Release.Id, out var stats);
            return new ReleaseListItem(
                x.Release.Id,
                x.Release.ProjectId,
                x.ProjectName,
                x.Release.ReleaseCode,
                x.Release.Title,
                x.Release.PlannedAt,
                x.Release.ReleasedAt,
                x.Release.Status,
                x.GateResult,
                stats?.Completed ?? 0,
                stats?.Total ?? 0,
                x.Release.UpdatedAt);
        }).ToList();

        return new PagedResult<ReleaseListItem>(items, total, page, pageSize);
    }

    public async Task<ReleaseDetailResponse?> GetReleaseAsync(Guid releaseId, CancellationToken cancellationToken)
    {
        var releaseRow = await (
            from release in dbContext.Releases.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on release.ProjectId equals project.Id
            join gate in dbContext.QualityGateResults.AsNoTracking() on release.QualityGateResultId equals gate.Id into gateJoin
            from gate in gateJoin.DefaultIfEmpty()
            where release.Id == releaseId
            select new
            {
                Release = release,
                ProjectName = project.Name,
                GateResult = gate != null ? gate.Result : null
            }).SingleOrDefaultAsync(cancellationToken);

        if (releaseRow is null)
        {
            return null;
        }

        var checklist = await dbContext.DeploymentChecklists.AsNoTracking()
            .Where(x => x.ReleaseId == releaseId)
            .OrderBy(x => x.ChecklistItem)
            .Select(x => new DeploymentChecklistItem(x.Id, x.ReleaseId, releaseRow.Release.ReleaseCode, x.ChecklistItem, x.OwnerUserId, x.Status, x.CompletedAt, x.EvidenceRef, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var notes = await dbContext.ReleaseNotes.AsNoTracking()
            .Where(x => x.ReleaseId == releaseId)
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new ReleaseNoteItem(x.Id, x.ReleaseId, releaseRow.Release.ReleaseCode, x.Summary, x.IncludedChanges, x.KnownIssues, x.Status, x.PublishedAt, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new ReleaseDetailResponse(
            releaseRow.Release.Id,
            releaseRow.Release.ProjectId,
            releaseRow.ProjectName,
            releaseRow.Release.ReleaseCode,
            releaseRow.Release.Title,
            releaseRow.Release.PlannedAt,
            releaseRow.Release.ReleasedAt,
            releaseRow.Release.Status,
            releaseRow.GateResult,
            releaseRow.Release.QualityGateOverrideReason,
            releaseRow.Release.ApprovedByUserId,
            releaseRow.Release.ApprovedAt,
            checklist,
            notes,
            releaseRow.Release.CreatedAt,
            releaseRow.Release.UpdatedAt);
    }

    public async Task<PagedResult<DeploymentChecklistItem>> ListDeploymentChecklistsAsync(DeploymentChecklistListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var baseQuery =
            from checklist in dbContext.DeploymentChecklists.AsNoTracking()
            join release in dbContext.Releases.AsNoTracking() on checklist.ReleaseId equals release.Id
            select new { Checklist = checklist, release.ReleaseCode };

        if (query.ReleaseId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Checklist.ReleaseId == query.ReleaseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToLowerInvariant();
            baseQuery = baseQuery.Where(x => x.Checklist.Status == status);
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderBy(x => x.ReleaseCode)
            .ThenBy(x => x.Checklist.ChecklistItem)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new DeploymentChecklistItem(x.Checklist.Id, x.Checklist.ReleaseId, x.ReleaseCode, x.Checklist.ChecklistItem, x.Checklist.OwnerUserId, x.Checklist.Status, x.Checklist.CompletedAt, x.Checklist.EvidenceRef, x.Checklist.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<DeploymentChecklistItem>(items, total, page, pageSize);
    }

    public async Task<PagedResult<ReleaseNoteItem>> ListReleaseNotesAsync(ReleaseNoteListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var baseQuery =
            from note in dbContext.ReleaseNotes.AsNoTracking()
            join release in dbContext.Releases.AsNoTracking() on note.ReleaseId equals release.Id
            select new { Note = note, release.ReleaseCode };

        if (query.ReleaseId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Note.ReleaseId == query.ReleaseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToLowerInvariant();
            baseQuery = baseQuery.Where(x => x.Note.Status == status);
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderByDescending(x => x.Note.UpdatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new ReleaseNoteItem(x.Note.Id, x.Note.ReleaseId, x.ReleaseCode, x.Note.Summary, x.Note.IncludedChanges, x.Note.KnownIssues, x.Note.Status, x.Note.PublishedAt, x.Note.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<ReleaseNoteItem>(items, total, page, pageSize);
    }

    private static (int Page, int PageSize, int Skip) NormalizePaging(int? page, int? pageSize)
    {
        var normalizedPage = Math.Max(page.GetValueOrDefault(1), 1);
        var normalizedPageSize = Math.Clamp(pageSize.GetValueOrDefault(25), 1, 100);
        return (normalizedPage, normalizedPageSize, (normalizedPage - 1) * normalizedPageSize);
    }
}
