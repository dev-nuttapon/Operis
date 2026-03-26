using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Requirements.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Requirements.Application;

public sealed class RequirementQueries(OperisDbContext dbContext) : IRequirementQueries
{
    private static readonly string[] RequiredTargetTypes = ["document", "test"];
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<PagedResult<RequirementListItem>> ListRequirementsAsync(RequirementListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var requirements =
            from requirement in dbContext.Set<Infrastructure.RequirementEntity>().AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on requirement.ProjectId equals project.Id
            join version in dbContext.Set<Infrastructure.RequirementVersionEntity>().AsNoTracking() on requirement.CurrentVersionId equals version.Id into versionJoin
            from version in versionJoin.DefaultIfEmpty()
            select new
            {
                Requirement = requirement,
                ProjectName = project.Name,
                CurrentVersion = version
            };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            requirements = requirements.Where(x =>
                EF.Functions.ILike(x.Requirement.Code, search)
                || EF.Functions.ILike(x.Requirement.Title, search)
                || EF.Functions.ILike(x.Requirement.Description, search));
        }

        if (query.ProjectId.HasValue)
        {
            requirements = requirements.Where(x => x.Requirement.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Priority))
        {
            requirements = requirements.Where(x => x.Requirement.Priority == query.Priority.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            requirements = requirements.Where(x => x.Requirement.Status == query.Status.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.OwnerUserId))
        {
            requirements = requirements.Where(x => x.Requirement.OwnerUserId == query.OwnerUserId.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.BaselineStatus))
        {
            var normalizedBaselineStatus = query.BaselineStatus.Trim();
            requirements = string.Equals(normalizedBaselineStatus, "locked", StringComparison.OrdinalIgnoreCase)
                ? requirements.Where(x => x.Requirement.Status == "baselined")
                : requirements.Where(x => x.Requirement.Status != "baselined");
        }

        if (query.MissingDownstreamLinks.HasValue)
        {
            requirements = query.MissingDownstreamLinks.Value
                ? requirements.Where(x =>
                    !dbContext.Set<Infrastructure.TraceabilityLinkEntity>().Any(link =>
                        link.SourceType == "requirement"
                        && link.SourceId == x.Requirement.Id.ToString()
                        && link.TargetType == "document"
                        && link.Status != "broken")
                    || !dbContext.Set<Infrastructure.TraceabilityLinkEntity>().Any(link =>
                        link.SourceType == "requirement"
                        && link.SourceId == x.Requirement.Id.ToString()
                        && link.TargetType == "test"
                        && link.Status != "broken"))
                : requirements.Where(x =>
                    dbContext.Set<Infrastructure.TraceabilityLinkEntity>().Any(link =>
                        link.SourceType == "requirement"
                        && link.SourceId == x.Requirement.Id.ToString()
                        && link.TargetType == "document"
                        && link.Status != "broken")
                    && dbContext.Set<Infrastructure.TraceabilityLinkEntity>().Any(link =>
                        link.SourceType == "requirement"
                        && link.SourceId == x.Requirement.Id.ToString()
                        && link.TargetType == "test"
                        && link.Status != "broken"));
        }

        var requirementRows = await requirements
            .OrderBy(x => x.Requirement.Code)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var requirementIds = requirementRows.Select(x => x.Requirement.Id).ToList();
        var linkLookup = await BuildLinkLookupAsync(requirementIds, cancellationToken);

        var pageItems = requirementRows
            .Select(x => new RequirementListItem(
                x.Requirement.Id,
                x.Requirement.ProjectId,
                x.ProjectName,
                x.Requirement.Code,
                x.Requirement.Title,
                x.Requirement.Priority,
                x.Requirement.OwnerUserId,
                x.Requirement.Status,
                DeriveBaselineStatus(x.Requirement.Status),
                GetMissingLinkCount(linkLookup.GetValueOrDefault(x.Requirement.Id)),
                x.Requirement.CurrentVersionId,
                x.CurrentVersion?.VersionNumber,
                x.Requirement.UpdatedAt))
            .ToList();

        var total = await requirements.CountAsync(cancellationToken);
        return new PagedResult<RequirementListItem>(pageItems, total, page, pageSize);
    }

    public async Task<RequirementDetailResponse?> GetRequirementAsync(Guid requirementId, CancellationToken cancellationToken)
    {
        var item = await (
            from requirement in dbContext.Set<Infrastructure.RequirementEntity>().AsNoTracking()
            where requirement.Id == requirementId
            join project in dbContext.Projects.AsNoTracking() on requirement.ProjectId equals project.Id
            select new
            {
                Requirement = requirement,
                ProjectName = project.Name
            }).SingleOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return null;
        }

        var versions = await dbContext.Set<Infrastructure.RequirementVersionEntity>().AsNoTracking()
            .Where(x => x.RequirementId == requirementId)
            .OrderByDescending(x => x.VersionNumber)
            .Select(x => new RequirementVersionItem(x.Id, x.RequirementId, x.VersionNumber, x.BusinessReason, x.AcceptanceCriteria, x.SecurityImpact, x.PerformanceImpact, x.Status, x.CreatedAt))
            .ToListAsync(cancellationToken);

        var links = await dbContext.Set<Infrastructure.TraceabilityLinkEntity>().AsNoTracking()
            .Where(x => x.SourceType == "requirement" && x.SourceId == requirementId.ToString())
            .OrderBy(x => x.TargetType)
            .Select(x => new TraceabilityLinkItem(x.Id, x.SourceType, x.SourceId, x.TargetType, x.TargetId, x.LinkRule, x.Status, x.CreatedBy, x.CreatedAt))
            .ToListAsync(cancellationToken);

        var history = await dbContext.BusinessAuditEvents.AsNoTracking()
            .Where(x => x.EntityType == "requirement" && x.EntityId == requirementId.ToString())
            .OrderByDescending(x => x.OccurredAt)
            .Select(x => new RequirementHistoryItem(x.Id, x.EventType, x.Summary, x.Reason, x.ActorUserId, x.OccurredAt))
            .ToListAsync(cancellationToken);

        return new RequirementDetailResponse(
            item.Requirement.Id,
            item.Requirement.ProjectId,
            item.ProjectName,
            item.Requirement.Code,
            item.Requirement.Title,
            item.Requirement.Description,
            item.Requirement.Priority,
            item.Requirement.OwnerUserId,
            item.Requirement.Status,
            item.Requirement.CurrentVersionId,
            versions,
            links,
            history,
            item.Requirement.CreatedAt,
            item.Requirement.UpdatedAt);
    }

    public async Task<PagedResult<RequirementBaselineItem>> ListBaselinesAsync(RequirementBaselineListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var baselines =
            from baseline in dbContext.Set<Infrastructure.RequirementBaselineEntity>().AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on baseline.ProjectId equals project.Id
            select new { Baseline = baseline, ProjectName = project.Name };

        if (query.ProjectId.HasValue)
        {
            baselines = baselines.Where(x => x.Baseline.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            baselines = baselines.Where(x => x.Baseline.Status == query.Status.Trim());
        }

        var total = await baselines.CountAsync(cancellationToken);
        var items = await baselines
            .OrderByDescending(x => x.Baseline.ApprovedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new RequirementBaselineItem(
                x.Baseline.Id,
                x.Baseline.ProjectId,
                x.ProjectName,
                x.Baseline.BaselineName,
                DeserializeRequirementIds(x.Baseline.RequirementIdsJson),
                x.Baseline.Status,
                x.Baseline.ApprovedBy,
                x.Baseline.ApprovedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<RequirementBaselineItem>(items, total, page, pageSize);
    }

    public async Task<PagedResult<TraceabilityMatrixRow>> ListTraceabilityAsync(TraceabilityListQuery query, CancellationToken cancellationToken)
    {
        var requirementPage = await ListRequirementsAsync(
            new RequirementListQuery(null, query.ProjectId, null, null, null, query.BaselineStatus, null, query.Page, query.PageSize),
            cancellationToken);

        var requirementIds = requirementPage.Items.Select(x => x.Id).ToList();
        var linkLookup = await BuildLinkLookupAsync(requirementIds, cancellationToken);

        var rows = requirementPage.Items
            .Select(item => new TraceabilityMatrixRow(
                item.Id,
                item.Code,
                item.Title,
                item.ProjectId,
                item.ProjectName,
                item.Status,
                item.BaselineStatus,
                item.MissingLinkCount,
                linkLookup.GetValueOrDefault(item.Id, [])))
            .Where(row => query.MissingCoverage is null || (query.MissingCoverage.Value ? row.MissingLinkCount > 0 : row.MissingLinkCount == 0))
            .ToList();

        return new PagedResult<TraceabilityMatrixRow>(rows, requirementPage.Total, requirementPage.Page, requirementPage.PageSize);
    }

    private async Task<Dictionary<Guid, IReadOnlyList<TraceabilityLinkItem>>> BuildLinkLookupAsync(IReadOnlyCollection<Guid> requirementIds, CancellationToken cancellationToken)
    {
        if (requirementIds.Count == 0)
        {
            return [];
        }

        var sourceIds = requirementIds.Select(id => id.ToString()).ToArray();
        var links = await dbContext.Set<Infrastructure.TraceabilityLinkEntity>().AsNoTracking()
            .Where(x => x.SourceType == "requirement" && sourceIds.Contains(x.SourceId))
            .Select(x => new TraceabilityLinkItem(x.Id, x.SourceType, x.SourceId, x.TargetType, x.TargetId, x.LinkRule, x.Status, x.CreatedBy, x.CreatedAt))
            .ToListAsync(cancellationToken);

        return links
            .GroupBy(x => Guid.Parse(x.SourceId))
            .ToDictionary(x => x.Key, x => (IReadOnlyList<TraceabilityLinkItem>)x.ToList());
    }

    private static int GetMissingLinkCount(IReadOnlyList<TraceabilityLinkItem>? links)
    {
        var targetTypes = links?.Where(x => !string.Equals(x.Status, "broken", StringComparison.OrdinalIgnoreCase)).Select(x => x.TargetType).ToHashSet(StringComparer.OrdinalIgnoreCase)
            ?? [];
        return RequiredTargetTypes.Count(targetType => !targetTypes.Contains(targetType));
    }

    private static string? DeriveBaselineStatus(string requirementStatus) =>
        string.Equals(requirementStatus, "baselined", StringComparison.OrdinalIgnoreCase) ? "locked" : null;

    private static IReadOnlyList<Guid> DeserializeRequirementIds(string json) =>
        JsonSerializer.Deserialize<List<Guid>>(json, SerializerOptions) ?? [];

    private static (int Page, int PageSize, int Skip) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = Math.Clamp(pageSize, 5, 200);
        return (normalizedPage, normalizedPageSize, (normalizedPage - 1) * normalizedPageSize);
    }
}
