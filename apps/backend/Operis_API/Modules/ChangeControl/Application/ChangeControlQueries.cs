using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.ChangeControl.Contracts;
using Operis_API.Modules.ChangeControl.Infrastructure;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.ChangeControl.Application;

public sealed class ChangeControlQueries(OperisDbContext dbContext) : IChangeControlQueries
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<PagedResult<ChangeRequestListItemResponse>> ListChangeRequestsAsync(ChangeControlListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var source =
            from request in dbContext.Set<ChangeRequestEntity>().AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on request.ProjectId equals project.Id
            join baseline in dbContext.Set<BaselineRegistryEntity>().AsNoTracking() on request.TargetBaselineId equals baseline.Id into baselineJoin
            from baseline in baselineJoin.DefaultIfEmpty()
            select new { Request = request, ProjectName = project.Name, TargetBaselineName = baseline == null ? null : baseline.BaselineName };

        if (query.ProjectId.HasValue)
        {
            source = source.Where(x => x.Request.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            source = source.Where(x => x.Request.Status == query.Status.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Priority))
        {
            source = source.Where(x => x.Request.Priority == query.Priority.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.Request.Code, search)
                || EF.Functions.ILike(x.Request.Title, search)
                || EF.Functions.ILike(x.ProjectName, search));
        }

        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .OrderByDescending(x => x.Request.UpdatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new ChangeRequestListItemResponse(
                x.Request.Id,
                x.Request.ProjectId,
                x.ProjectName,
                x.Request.Code,
                x.Request.Title,
                x.Request.Priority,
                x.Request.RequestedBy,
                x.Request.Status,
                x.TargetBaselineName,
                x.Request.ApprovedAt.HasValue ? x.Request.Status : null,
                x.Request.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<ChangeRequestListItemResponse>(items, total, page, pageSize);
    }

    public async Task<ChangeRequestResponse?> GetChangeRequestAsync(Guid changeRequestId, CancellationToken cancellationToken)
    {
        var item = await (
            from request in dbContext.Set<ChangeRequestEntity>().AsNoTracking()
            where request.Id == changeRequestId
            join project in dbContext.Projects.AsNoTracking() on request.ProjectId equals project.Id
            join baseline in dbContext.Set<BaselineRegistryEntity>().AsNoTracking() on request.TargetBaselineId equals baseline.Id into baselineJoin
            from baseline in baselineJoin.DefaultIfEmpty()
            select new { Request = request, ProjectName = project.Name, TargetBaselineName = baseline == null ? null : baseline.BaselineName })
            .SingleOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return null;
        }

        var impact = await dbContext.Set<ChangeImpactEntity>().AsNoTracking()
            .Where(x => x.ChangeRequestId == changeRequestId)
            .Select(x => new ChangeImpactResponse(x.Id, x.ChangeRequestId, x.ScopeImpact, x.ScheduleImpact, x.QualityImpact, x.SecurityImpact, x.PerformanceImpact, x.RiskImpact))
            .SingleOrDefaultAsync(cancellationToken);

        var history = await dbContext.BusinessAuditEvents.AsNoTracking()
            .Where(x => x.EntityType == "change_request" && x.EntityId == changeRequestId.ToString())
            .OrderByDescending(x => x.OccurredAt)
            .Select(x => new ChangeHistoryItem(x.Id, x.EventType, x.Summary, x.Reason, x.ActorUserId, x.OccurredAt))
            .ToListAsync(cancellationToken);

        if (impact is null)
        {
            return null;
        }

        return new ChangeRequestResponse(
            item.Request.Id,
            item.Request.ProjectId,
            item.ProjectName,
            item.Request.Code,
            item.Request.Title,
            item.Request.RequestedBy,
            item.Request.Reason,
            item.Request.Status,
            item.Request.Priority,
            item.Request.TargetBaselineId,
            item.TargetBaselineName,
            DeserializeGuidList(item.Request.LinkedRequirementIdsJson),
            DeserializeGuidList(item.Request.LinkedConfigurationItemIdsJson),
            impact,
            item.Request.DecisionRationale,
            item.Request.ImplementationSummary,
            item.Request.ApprovedBy,
            item.Request.ApprovedAt,
            history,
            item.Request.CreatedAt,
            item.Request.UpdatedAt);
    }

    public async Task<PagedResult<ConfigurationItemListItemResponse>> ListConfigurationItemsAsync(ChangeControlListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var source =
            from item in dbContext.Set<ConfigurationItemEntity>().AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on item.ProjectId equals project.Id
            select new { Item = item, ProjectName = project.Name };

        if (query.ProjectId.HasValue)
        {
            source = source.Where(x => x.Item.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            source = source.Where(x => x.Item.Status == query.Status.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.Item.Code, search)
                || EF.Functions.ILike(x.Item.Name, search)
                || EF.Functions.ILike(x.Item.OwnerModule, search));
        }

        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .OrderBy(x => x.Item.Code)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new ConfigurationItemListItemResponse(
                x.Item.Id,
                x.Item.ProjectId,
                x.ProjectName,
                x.Item.Code,
                x.Item.Name,
                x.Item.ItemType,
                x.Item.OwnerModule,
                x.Item.Status,
                x.Item.BaselineRef,
                x.Item.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<ConfigurationItemListItemResponse>(items, total, page, pageSize);
    }

    public async Task<ConfigurationItemResponse?> GetConfigurationItemAsync(Guid configurationItemId, CancellationToken cancellationToken)
    {
        var item = await (
            from configuration in dbContext.Set<ConfigurationItemEntity>().AsNoTracking()
            where configuration.Id == configurationItemId
            join project in dbContext.Projects.AsNoTracking() on configuration.ProjectId equals project.Id
            select new ConfigurationItemResponse(
                configuration.Id,
                configuration.ProjectId,
                project.Name,
                configuration.Code,
                configuration.Name,
                configuration.ItemType,
                configuration.OwnerModule,
                configuration.Status,
                configuration.BaselineRef,
                configuration.CreatedAt,
                configuration.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);

        return item;
    }

    public async Task<PagedResult<BaselineRegistryListItemResponse>> ListBaselineRegistryAsync(ChangeControlListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var source =
            from baseline in dbContext.Set<BaselineRegistryEntity>().AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on baseline.ProjectId equals project.Id
            select new { Baseline = baseline, ProjectName = project.Name };

        if (query.ProjectId.HasValue)
        {
            source = source.Where(x => x.Baseline.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            source = source.Where(x => x.Baseline.Status == query.Status.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.Baseline.BaselineName, search)
                || EF.Functions.ILike(x.Baseline.BaselineType, search)
                || EF.Functions.ILike(x.Baseline.SourceEntityType, search)
                || EF.Functions.ILike(x.Baseline.SourceEntityId, search));
        }

        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .OrderByDescending(x => x.Baseline.UpdatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new BaselineRegistryListItemResponse(
                x.Baseline.Id,
                x.Baseline.ProjectId,
                x.ProjectName,
                x.Baseline.BaselineName,
                x.Baseline.BaselineType,
                x.Baseline.SourceEntityType,
                x.Baseline.SourceEntityId,
                x.Baseline.Status,
                x.Baseline.ApprovedBy,
                x.Baseline.ApprovedAt,
                x.Baseline.ChangeRequestId,
                x.Baseline.SupersededByBaselineId,
                x.Baseline.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<BaselineRegistryListItemResponse>(items, total, page, pageSize);
    }

    public async Task<BaselineRegistryResponse?> GetBaselineRegistryAsync(Guid baselineRegistryId, CancellationToken cancellationToken)
    {
        var item = await (
            from baseline in dbContext.Set<BaselineRegistryEntity>().AsNoTracking()
            where baseline.Id == baselineRegistryId
            join project in dbContext.Projects.AsNoTracking() on baseline.ProjectId equals project.Id
            select new BaselineRegistryResponse(
                baseline.Id,
                baseline.ProjectId,
                project.Name,
                baseline.BaselineName,
                baseline.BaselineType,
                baseline.SourceEntityType,
                baseline.SourceEntityId,
                baseline.Status,
                baseline.ApprovedBy,
                baseline.ApprovedAt,
                baseline.ChangeRequestId,
                baseline.SupersededByBaselineId,
                baseline.OverrideReason,
                baseline.CreatedAt,
                baseline.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);

        return item;
    }

    private static IReadOnlyList<Guid> DeserializeGuidList(string json) =>
        JsonSerializer.Deserialize<List<Guid>>(json, SerializerOptions) ?? [];

    private static (int Page, int PageSize, int Skip) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = Math.Clamp(pageSize, 5, 200);
        return (normalizedPage, normalizedPageSize, (normalizedPage - 1) * normalizedPageSize);
    }
}
