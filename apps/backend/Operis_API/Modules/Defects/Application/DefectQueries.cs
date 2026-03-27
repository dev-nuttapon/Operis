using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Defects.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Defects.Application;

public sealed class DefectQueries(OperisDbContext dbContext) : IDefectQueries
{
    public async Task<PagedResult<DefectListItem>> ListDefectsAsync(DefectListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var baseQuery =
            from defect in dbContext.Defects.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on defect.ProjectId equals project.Id
            select new { Defect = defect, ProjectName = project.Name };

        if (query.ProjectId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Defect.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Severity))
        {
            var severity = query.Severity.Trim().ToLowerInvariant();
            baseQuery = baseQuery.Where(x => x.Defect.Severity == severity);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToLowerInvariant();
            baseQuery = baseQuery.Where(x => x.Defect.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.OwnerUserId))
        {
            baseQuery = baseQuery.Where(x => x.Defect.OwnerUserId == query.OwnerUserId.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            baseQuery = baseQuery.Where(x =>
                EF.Functions.ILike(x.Defect.Code, $"%{search}%") ||
                EF.Functions.ILike(x.Defect.Title, $"%{search}%") ||
                EF.Functions.ILike(x.ProjectName, $"%{search}%"));
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderByDescending(x => x.Defect.UpdatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new DefectListItem(x.Defect.Id, x.Defect.ProjectId, x.ProjectName, x.Defect.Code, x.Defect.Title, x.Defect.Severity, x.Defect.OwnerUserId, x.Defect.Status, x.Defect.DetectedInPhase, x.Defect.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<DefectListItem>(items, total, page, pageSize);
    }

    public async Task<DefectDetailResponse?> GetDefectAsync(Guid defectId, CancellationToken cancellationToken)
    {
        var row = await (
            from defect in dbContext.Defects.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on defect.ProjectId equals project.Id
            where defect.Id == defectId
            select new { Defect = defect, ProjectName = project.Name }).SingleOrDefaultAsync(cancellationToken);

        return row is null
            ? null
            : new DefectDetailResponse(
                row.Defect.Id,
                row.Defect.ProjectId,
                row.ProjectName,
                row.Defect.Code,
                row.Defect.Title,
                row.Defect.Description,
                row.Defect.Severity,
                row.Defect.OwnerUserId,
                row.Defect.Status,
                row.Defect.DetectedInPhase,
                row.Defect.ResolutionSummary,
                row.Defect.CorrectiveActionRef,
                ReadArray(row.Defect.AffectedArtifactRefsJson),
                row.Defect.CreatedAt,
                row.Defect.UpdatedAt);
    }

    public async Task<PagedResult<NonConformanceListItem>> ListNonConformancesAsync(NonConformanceListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var baseQuery =
            from item in dbContext.NonConformances.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on item.ProjectId equals project.Id
            select new { NonConformance = item, ProjectName = project.Name };

        if (query.ProjectId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.NonConformance.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToLowerInvariant();
            baseQuery = baseQuery.Where(x => x.NonConformance.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.OwnerUserId))
        {
            baseQuery = baseQuery.Where(x => x.NonConformance.OwnerUserId == query.OwnerUserId.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            baseQuery = baseQuery.Where(x =>
                EF.Functions.ILike(x.NonConformance.Code, $"%{search}%") ||
                EF.Functions.ILike(x.NonConformance.Title, $"%{search}%") ||
                EF.Functions.ILike(x.NonConformance.SourceType, $"%{search}%") ||
                EF.Functions.ILike(x.ProjectName, $"%{search}%"));
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderByDescending(x => x.NonConformance.UpdatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new NonConformanceListItem(x.NonConformance.Id, x.NonConformance.ProjectId, x.ProjectName, x.NonConformance.Code, x.NonConformance.Title, x.NonConformance.SourceType, x.NonConformance.OwnerUserId, x.NonConformance.Status, x.NonConformance.CorrectiveActionRef, x.NonConformance.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<NonConformanceListItem>(items, total, page, pageSize);
    }

    public async Task<NonConformanceDetailResponse?> GetNonConformanceAsync(Guid nonConformanceId, CancellationToken cancellationToken)
    {
        var row = await (
            from item in dbContext.NonConformances.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on item.ProjectId equals project.Id
            where item.Id == nonConformanceId
            select new { NonConformance = item, ProjectName = project.Name }).SingleOrDefaultAsync(cancellationToken);

        return row is null
            ? null
            : new NonConformanceDetailResponse(
                row.NonConformance.Id,
                row.NonConformance.ProjectId,
                row.ProjectName,
                row.NonConformance.Code,
                row.NonConformance.Title,
                row.NonConformance.Description,
                row.NonConformance.SourceType,
                row.NonConformance.OwnerUserId,
                row.NonConformance.Status,
                row.NonConformance.CorrectiveActionRef,
                row.NonConformance.RootCause,
                row.NonConformance.ResolutionSummary,
                row.NonConformance.AcceptedDisposition,
                ReadArray(row.NonConformance.LinkedFindingRefsJson),
                row.NonConformance.CreatedAt,
                row.NonConformance.UpdatedAt);
    }

    private static IReadOnlyList<string> ReadArray(string? json) =>
        string.IsNullOrWhiteSpace(json) ? [] : JsonSerializer.Deserialize<IReadOnlyList<string>>(json) ?? [];

    private static (int Page, int PageSize, int Skip) NormalizePaging(int? page, int? pageSize)
    {
        var normalizedPage = Math.Max(page.GetValueOrDefault(1), 1);
        var normalizedPageSize = Math.Clamp(pageSize.GetValueOrDefault(25), 1, 100);
        return (normalizedPage, normalizedPageSize, (normalizedPage - 1) * normalizedPageSize);
    }
}
