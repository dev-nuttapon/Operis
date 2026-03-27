using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Metrics.Contracts;
using Operis_API.Modules.Metrics.Infrastructure;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Metrics.Application;

public sealed class MetricsQueries(OperisDbContext dbContext) : IMetricsQueries
{
    public async Task<PagedResult<MetricDefinitionListItem>> ListMetricDefinitionsAsync(MetricDefinitionListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var baseQuery = dbContext.MetricDefinitions.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            baseQuery = baseQuery.Where(x => EF.Functions.ILike(x.Code, $"%{search}%") || EF.Functions.ILike(x.Name, $"%{search}%"));
        }

        if (!string.IsNullOrWhiteSpace(query.MetricType))
        {
            baseQuery = baseQuery.Where(x => x.MetricType == query.MetricType.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            baseQuery = baseQuery.Where(x => x.Status == query.Status.Trim().ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(query.OwnerUserId))
        {
            baseQuery = baseQuery.Where(x => x.OwnerUserId == query.OwnerUserId.Trim());
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderBy(x => x.Code)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new MetricDefinitionListItem(x.Id, x.Code, x.Name, x.MetricType, x.OwnerUserId, x.TargetValue, x.ThresholdValue, x.Status, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<MetricDefinitionListItem>(items, total, page, pageSize);
    }

    public async Task<PagedResult<MetricCollectionScheduleItem>> ListMetricCollectionSchedulesAsync(MetricCollectionScheduleListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var baseQuery =
            from schedule in dbContext.MetricCollectionSchedules.AsNoTracking()
            join definition in dbContext.MetricDefinitions.AsNoTracking() on schedule.MetricDefinitionId equals definition.Id
            select new { Schedule = schedule, Definition = definition };

        if (query.MetricDefinitionId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Schedule.MetricDefinitionId == query.MetricDefinitionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            baseQuery = baseQuery.Where(x => x.Schedule.Status == query.Status.Trim().ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(query.CollectorType))
        {
            baseQuery = baseQuery.Where(x => x.Schedule.CollectorType == query.CollectorType.Trim().ToLowerInvariant());
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderBy(x => x.Schedule.NextRunAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new MetricCollectionScheduleItem(x.Schedule.Id, x.Definition.Id, x.Definition.Code, x.Definition.Name, x.Schedule.CollectionFrequency, x.Schedule.CollectorType, x.Schedule.NextRunAt, x.Schedule.Status, x.Schedule.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<MetricCollectionScheduleItem>(items, total, page, pageSize);
    }

    public async Task<MetricResultsResponse> ListMetricResultsAsync(MetricResultListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var baseQuery =
            from result in dbContext.MetricResults.AsNoTracking()
            join definition in dbContext.MetricDefinitions.AsNoTracking() on result.MetricDefinitionId equals definition.Id
            join gate in dbContext.QualityGateResults.AsNoTracking() on result.QualityGateResultId equals gate.Id into gateJoin
            from gate in gateJoin.DefaultIfEmpty()
            select new { Result = result, Definition = definition, Gate = gate };

        if (query.MetricDefinitionId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Result.MetricDefinitionId == query.MetricDefinitionId.Value);
        }

        if (query.ProjectId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Gate != null && x.Gate.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            baseQuery = baseQuery.Where(x => x.Result.Status == query.Status.Trim().ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(query.GateType))
        {
            baseQuery = baseQuery.Where(x => x.Gate != null && x.Gate.GateType == query.GateType.Trim().ToLowerInvariant());
        }

        if (query.From.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Result.MeasuredAt >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Result.MeasuredAt <= query.To.Value);
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderByDescending(x => x.Result.MeasuredAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new MetricResultItem(x.Result.Id, x.Definition.Id, x.Definition.Code, x.Definition.Name, x.Definition.MetricType, x.Result.MeasuredAt, x.Result.MeasuredValue, x.Definition.TargetValue, x.Definition.ThresholdValue, x.Result.Status, x.Result.SourceRef, x.Result.QualityGateResultId))
            .ToListAsync(cancellationToken);

        var latestPerMetric = await dbContext.MetricResults.AsNoTracking()
            .GroupBy(x => x.MetricDefinitionId)
            .Select(group => group.OrderByDescending(x => x.MeasuredAt).First())
            .Join(dbContext.MetricDefinitions.AsNoTracking(),
                result => result.MetricDefinitionId,
                definition => definition.Id,
                (result, definition) => new MetricCurrentVsTargetItem(definition.Id, definition.Code, definition.Name, result.MeasuredValue, definition.TargetValue, definition.ThresholdValue, result.Status))
            .OrderBy(x => x.MetricCode)
            .ToListAsync(cancellationToken);

        var trend = await dbContext.MetricResults.AsNoTracking()
            .Join(dbContext.MetricDefinitions.AsNoTracking(),
                result => result.MetricDefinitionId,
                definition => definition.Id,
                (result, definition) => new MetricTrendPoint(definition.Id, definition.Code, result.MeasuredAt, result.MeasuredValue, result.Status))
            .OrderByDescending(x => x.MeasuredAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        var breachCount = await dbContext.MetricResults.AsNoTracking()
            .CountAsync(x => x.Status == "threshold_breached", cancellationToken);

        var openActions = await dbContext.QualityGateResults.AsNoTracking()
            .CountAsync(x => x.Result == "failed", cancellationToken);

        return new MetricResultsResponse(items, total, page, pageSize, new MetricsDashboardSummary(breachCount, openActions, trend, latestPerMetric));
    }

    public async Task<PagedResult<QualityGateResultItem>> ListQualityGatesAsync(QualityGateListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var baseQuery =
            from gate in dbContext.QualityGateResults.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on gate.ProjectId equals project.Id
            select new { Gate = gate, ProjectName = project.Name };

        if (query.ProjectId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Gate.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.GateType))
        {
            baseQuery = baseQuery.Where(x => x.Gate.GateType == query.GateType.Trim().ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(query.Result))
        {
            baseQuery = baseQuery.Where(x => x.Gate.Result == query.Result.Trim().ToLowerInvariant());
        }

        var rows = await baseQuery
            .OrderByDescending(x => x.Gate.EvaluatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var total = await baseQuery.CountAsync(cancellationToken);

        var gateIds = rows.Select(x => x.Gate.Id).ToArray();
        var metrics = await dbContext.MetricResults.AsNoTracking()
            .Where(x => x.QualityGateResultId.HasValue && gateIds.Contains(x.QualityGateResultId.Value))
            .Join(dbContext.MetricDefinitions.AsNoTracking(),
                result => result.MetricDefinitionId,
                definition => definition.Id,
                (result, definition) => new { result, definition })
            .ToListAsync(cancellationToken);

        var metricLookup = metrics
            .GroupBy(x => x.result.QualityGateResultId!.Value)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<MetricResultItem>)group.Select(x => new MetricResultItem(x.result.Id, x.definition.Id, x.definition.Code, x.definition.Name, x.definition.MetricType, x.result.MeasuredAt, x.result.MeasuredValue, x.definition.TargetValue, x.definition.ThresholdValue, x.result.Status, x.result.SourceRef, x.result.QualityGateResultId)).ToList());

        var items = rows.Select(x => new QualityGateResultItem(x.Gate.Id, x.Gate.ProjectId, x.ProjectName, x.Gate.GateType, x.Gate.EvaluatedAt, x.Gate.Result, x.Gate.Reason, x.Gate.OverrideReason, x.Gate.EvaluatedByUserId, x.Gate.OverriddenByUserId, metricLookup.GetValueOrDefault(x.Gate.Id, []))).ToList();
        return new PagedResult<QualityGateResultItem>(items, total, page, pageSize);
    }

    public async Task<PagedResult<MetricReviewItem>> ListMetricReviewsAsync(MetricReviewListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var baseQuery =
            from review in dbContext.MetricReviews.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on review.ProjectId equals project.Id
            select new { Review = review, ProjectName = project.Name };

        if (query.ProjectId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Review.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            baseQuery = baseQuery.Where(x => x.Review.Status == query.Status.Trim().ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(query.ReviewedBy))
        {
            baseQuery = baseQuery.Where(x => x.Review.ReviewedBy == query.ReviewedBy.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            baseQuery = baseQuery.Where(x =>
                EF.Functions.ILike(x.ProjectName, $"%{search}%") ||
                EF.Functions.ILike(x.Review.ReviewPeriod, $"%{search}%") ||
                EF.Functions.ILike(x.Review.ReviewedBy, $"%{search}%"));
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderByDescending(x => x.Review.UpdatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new MetricReviewItem(x.Review.Id, x.Review.ProjectId, x.ProjectName, x.Review.ReviewPeriod, x.Review.ReviewedBy, x.Review.Status, x.Review.Summary, x.Review.OpenActionCount, x.Review.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<MetricReviewItem>(items, total, page, pageSize);
    }

    public async Task<PagedResult<TrendReportItem>> ListTrendReportsAsync(TrendReportListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var baseQuery =
            from report in dbContext.TrendReports.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on report.ProjectId equals project.Id
            join definition in dbContext.MetricDefinitions.AsNoTracking() on report.MetricDefinitionId equals definition.Id
            select new { Report = report, ProjectName = project.Name, Definition = definition };

        if (query.ProjectId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Report.ProjectId == query.ProjectId.Value);
        }

        if (query.MetricDefinitionId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Report.MetricDefinitionId == query.MetricDefinitionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            baseQuery = baseQuery.Where(x => x.Report.Status == query.Status.Trim().ToLowerInvariant());
        }

        if (query.PeriodFrom.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Report.PeriodFrom >= query.PeriodFrom.Value);
        }

        if (query.PeriodTo.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Report.PeriodTo <= query.PeriodTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            baseQuery = baseQuery.Where(x =>
                EF.Functions.ILike(x.ProjectName, $"%{search}%") ||
                EF.Functions.ILike(x.Definition.Code, $"%{search}%") ||
                EF.Functions.ILike(x.Definition.Name, $"%{search}%"));
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderByDescending(x => x.Report.UpdatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new TrendReportItem(x.Report.Id, x.Report.ProjectId, x.ProjectName, x.Report.MetricDefinitionId, x.Definition.Code, x.Definition.Name, x.Report.PeriodFrom, x.Report.PeriodTo, x.Report.Status, x.Report.ReportRef, x.Report.TrendDirection, x.Report.Variance, x.Report.RecommendedAction, x.Report.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<TrendReportItem>(items, total, page, pageSize);
    }

    public async Task<TrendReportItem?> GetTrendReportAsync(Guid trendReportId, CancellationToken cancellationToken)
    {
        return await (
            from report in dbContext.TrendReports.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on report.ProjectId equals project.Id
            join definition in dbContext.MetricDefinitions.AsNoTracking() on report.MetricDefinitionId equals definition.Id
            where report.Id == trendReportId
            select new TrendReportItem(report.Id, report.ProjectId, project.Name, report.MetricDefinitionId, definition.Code, definition.Name, report.PeriodFrom, report.PeriodTo, report.Status, report.ReportRef, report.TrendDirection, report.Variance, report.RecommendedAction, report.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static (int Page, int PageSize, int Skip) NormalizePaging(int? page, int? pageSize)
    {
        var normalizedPage = Math.Max(page.GetValueOrDefault(1), 1);
        var normalizedPageSize = Math.Clamp(pageSize.GetValueOrDefault(25), 1, 100);
        return (normalizedPage, normalizedPageSize, (normalizedPage - 1) * normalizedPageSize);
    }
}
