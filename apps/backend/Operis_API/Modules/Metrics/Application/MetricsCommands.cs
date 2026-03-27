using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Audits.Application;
using Operis_API.Modules.Audits.Contracts;
using Operis_API.Modules.Metrics.Contracts;
using Operis_API.Modules.Metrics.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Metrics.Application;

public sealed class MetricsCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IBusinessAuditEventWriter businessAuditEventWriter,
    IMetricsQueries queries) : IMetricsCommands
{
    private static readonly string[] MetricStatuses = ["draft", "approved", "active", "deprecated"];
    private static readonly string[] ScheduleStatuses = ["draft", "active", "archived"];
    private static readonly string[] MetricReviewStatuses = ["planned", "reviewed", "actions_tracked", "closed"];
    private static readonly string[] TrendReportStatuses = ["draft", "approved", "archived"];
    private static readonly string[] PerformanceBaselineStatuses = ["draft", "approved", "active", "superseded"];
    private static readonly string[] CapacityReviewStatuses = ["planned", "reviewed", "actioned", "closed"];
    private static readonly string[] SlowOperationStatuses = ["open", "investigating", "optimized", "verified", "closed"];
    private static readonly string[] PerformanceGateStatuses = ["pending", "passed", "failed", "overridden"];

    public async Task<MetricsCommandResult<MetricDefinitionCommandResponse>> CreateMetricDefinitionAsync(CreateMetricDefinitionRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (!request.TargetValue.HasValue)
        {
            return Validation<MetricDefinitionCommandResponse>(ApiErrorCodes.MetricTargetRequired, "Metric target value is required.");
        }

        if (!request.ThresholdValue.HasValue)
        {
            return Validation<MetricDefinitionCommandResponse>(ApiErrorCodes.MetricThresholdRequired, "Metric threshold value is required.");
        }

        var code = request.Code.Trim().ToUpperInvariant();
        if (await dbContext.MetricDefinitions.AnyAsync(x => x.Code == code, cancellationToken))
        {
            return Conflict<MetricDefinitionCommandResponse>(ApiErrorCodes.MetricCodeDuplicate, "Metric code already exists.");
        }

        var entity = new MetricDefinitionEntity
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = request.Name.Trim(),
            MetricType = request.MetricType.Trim().ToLowerInvariant(),
            OwnerUserId = request.OwnerUserId.Trim(),
            TargetValue = request.TargetValue.Value,
            ThresholdValue = request.ThresholdValue.Value,
            Status = "draft",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.MetricDefinitions.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("create", "metric_definition", entity.Id, 201, new { entity.Code, entity.Status });
        await AppendBusinessAsync("metric_definition_created", "metric_definition", entity.Id, $"Created metric definition {entity.Code}", actorUserId, null, new { entity.Code }, cancellationToken);
        return Success(new MetricDefinitionCommandResponse(entity.Id, entity.Code, entity.Status));
    }

    public async Task<MetricsCommandResult<MetricDefinitionCommandResponse>> UpdateMetricDefinitionAsync(Guid metricDefinitionId, UpdateMetricDefinitionRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.MetricDefinitions.SingleOrDefaultAsync(x => x.Id == metricDefinitionId, cancellationToken);
        if (entity is null)
        {
            return NotFound<MetricDefinitionCommandResponse>(ApiErrorCodes.MetricDefinitionNotFound, "Metric definition not found.");
        }

        if (!request.TargetValue.HasValue)
        {
            return Validation<MetricDefinitionCommandResponse>(ApiErrorCodes.MetricTargetRequired, "Metric target value is required.");
        }

        if (!request.ThresholdValue.HasValue)
        {
            return Validation<MetricDefinitionCommandResponse>(ApiErrorCodes.MetricThresholdRequired, "Metric threshold value is required.");
        }

        var nextStatus = request.Status.Trim().ToLowerInvariant();
        if (!MetricStatuses.Contains(nextStatus))
        {
            return Validation<MetricDefinitionCommandResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Metric definition status is invalid.");
        }

        if (!IsValidTransition(entity.Status, nextStatus, MetricStatuses))
        {
            return Validation<MetricDefinitionCommandResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Metric definition transition is invalid.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Name = request.Name.Trim(),
            MetricType = request.MetricType.Trim().ToLowerInvariant(),
            OwnerUserId = request.OwnerUserId.Trim(),
            TargetValue = request.TargetValue.Value,
            ThresholdValue = request.ThresholdValue.Value,
            Status = nextStatus,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("update", "metric_definition", entity.Id, 200, new { entity.Code, Status = nextStatus });
        await AppendBusinessAsync("metric_definition_updated", "metric_definition", entity.Id, $"Updated metric definition {entity.Code}", actorUserId, null, new { Status = nextStatus }, cancellationToken);
        return Success(new MetricDefinitionCommandResponse(entity.Id, entity.Code, nextStatus));
    }

    public async Task<MetricsCommandResult<MetricCollectionScheduleItem>> CreateMetricCollectionScheduleAsync(CreateMetricCollectionScheduleRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var definition = await dbContext.MetricDefinitions.SingleOrDefaultAsync(x => x.Id == request.MetricDefinitionId, cancellationToken);
        if (definition is null)
        {
            return NotFound<MetricCollectionScheduleItem>(ApiErrorCodes.MetricDefinitionNotFound, "Metric definition not found.");
        }

        var status = request.Status.Trim().ToLowerInvariant();
        if (!ScheduleStatuses.Contains(status))
        {
            return Validation<MetricCollectionScheduleItem>(ApiErrorCodes.InvalidWorkflowTransition, "Collection schedule status is invalid.");
        }

        var frequency = request.CollectionFrequency.Trim().ToLowerInvariant();
        var nextRunAt = CalculateNextRun(DateTimeOffset.UtcNow, frequency);
        var entity = new MetricCollectionScheduleEntity
        {
            Id = Guid.NewGuid(),
            MetricDefinitionId = request.MetricDefinitionId,
            CollectionFrequency = frequency,
            CollectorType = request.CollectorType.Trim().ToLowerInvariant(),
            NextRunAt = nextRunAt,
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.MetricCollectionSchedules.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("create", "metric_collection_schedule", entity.Id, 201, new { definition.Code, entity.Status });
        await AppendBusinessAsync("metric_schedule_created", "metric_collection_schedule", entity.Id, $"Created collection schedule for {definition.Code}", actorUserId, null, new { definition.Code, entity.Status }, cancellationToken);
        return Success(new MetricCollectionScheduleItem(entity.Id, definition.Id, definition.Code, definition.Name, entity.CollectionFrequency, entity.CollectorType, entity.NextRunAt, entity.Status, entity.UpdatedAt));
    }

    public async Task<MetricsCommandResult<QualityGateResultItem>> EvaluateQualityGateAsync(EvaluateQualityGateRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Projects.AnyAsync(x => x.Id == request.ProjectId, cancellationToken))
        {
            return NotFound<QualityGateResultItem>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        var inputs = request.MetricInputs ?? [];
        var metricIds = inputs.Select(x => x.MetricDefinitionId).Distinct().ToArray();
        var definitions = await dbContext.MetricDefinitions
            .Where(x => metricIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        if (definitions.Count != metricIds.Length)
        {
            return NotFound<QualityGateResultItem>(ApiErrorCodes.MetricDefinitionNotFound, "One or more metric definitions were not found.");
        }

        var now = DateTimeOffset.UtcNow;
        var gate = new QualityGateResultEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            GateType = request.GateType.Trim().ToLowerInvariant(),
            EvaluatedAt = now,
            Result = "pending",
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
            EvaluatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        var metricRows = new List<MetricResultEntity>();
        foreach (var input in inputs)
        {
            var definition = definitions[input.MetricDefinitionId];
            var status = input.MeasuredValue > definition.ThresholdValue ? "threshold_breached" : "within_target";
            metricRows.Add(new MetricResultEntity
            {
                Id = Guid.NewGuid(),
                MetricDefinitionId = definition.Id,
                QualityGateResultId = gate.Id,
                MeasuredAt = input.MeasuredAt ?? now,
                MeasuredValue = input.MeasuredValue,
                Status = status,
                SourceRef = input.SourceRef.Trim(),
                CreatedAt = now
            });
        }

        var gateResult = metricRows.Any(x => x.Status == "threshold_breached") ? "failed" : "passed";
        gate = gate with
        {
            Result = gateResult,
            Reason = gateResult == "failed"
                ? string.Join(", ", metricRows.Where(x => x.Status == "threshold_breached").Select(x => $"{definitions[x.MetricDefinitionId].Code} breached threshold"))
                : gate.Reason
        };

        dbContext.QualityGateResults.Add(gate);
        dbContext.MetricResults.AddRange(metricRows);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("evaluate", "quality_gate_result", gate.Id, 201, new { gate.GateType, gate.Result });
        await AppendBusinessAsync("quality_gate_evaluated", "quality_gate_result", gate.Id, $"Evaluated quality gate {gate.GateType}", actorUserId, gate.Reason, new { gate.Result }, cancellationToken);

        var item = await BuildQualityGateItemAsync(gate.Id, cancellationToken);
        return item is null
            ? NotFound<QualityGateResultItem>(ApiErrorCodes.QualityGateResultNotFound, "Quality gate result not found.")
            : Success(item);
    }

    public async Task<MetricsCommandResult<QualityGateOverrideResponse>> OverrideQualityGateAsync(Guid qualityGateResultId, OverrideQualityGateRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.QualityGateResults.SingleOrDefaultAsync(x => x.Id == qualityGateResultId, cancellationToken);
        if (entity is null)
        {
            return NotFound<QualityGateOverrideResponse>(ApiErrorCodes.QualityGateResultNotFound, "Quality gate result not found.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return Validation<QualityGateOverrideResponse>(ApiErrorCodes.QualityGateOverrideReasonRequired, "Quality gate override reason is required.");
        }

        if (!string.Equals(entity.Result, "failed", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<QualityGateOverrideResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Only failed quality gates can be overridden.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Result = "overridden",
            OverrideReason = request.Reason.Trim(),
            OverriddenByUserId = actorUserId,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("override", "quality_gate_result", entity.Id, 200, new { Result = "overridden" }, request.Reason.Trim());
        await AppendBusinessAsync("quality_gate_overridden", "quality_gate_result", entity.Id, $"Overrode quality gate {entity.GateType}", actorUserId, request.Reason.Trim(), new { Result = "overridden" }, cancellationToken);
        return Success(new QualityGateOverrideResponse(entity.Id, "overridden", request.Reason.Trim()));
    }

    public async Task<MetricsCommandResult<MetricReviewItem>> CreateMetricReviewAsync(CreateMetricReviewRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Projects.AnyAsync(x => x.Id == request.ProjectId, cancellationToken))
        {
            return NotFound<MetricReviewItem>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        var reviewPeriod = request.ReviewPeriod.Trim();
        var reviewedBy = request.ReviewedBy.Trim();
        if (string.IsNullOrWhiteSpace(reviewPeriod) || string.IsNullOrWhiteSpace(reviewedBy))
        {
            return Validation<MetricReviewItem>(ApiErrorCodes.RequestValidationFailed, "Review period and reviewer are required.");
        }

        var entity = new MetricReviewEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            ReviewPeriod = reviewPeriod,
            ReviewedBy = reviewedBy,
            Status = "planned",
            Summary = string.IsNullOrWhiteSpace(request.Summary) ? null : request.Summary.Trim(),
            OpenActionCount = Math.Max(0, request.OpenActionCount),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.MetricReviews.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("create", "metric_review", entity.Id, 201, new { entity.Status, entity.ReviewPeriod });
        await AppendBusinessAsync("metric_review_created", "metric_review", entity.Id, $"Created metric review for {entity.ReviewPeriod}", actorUserId, null, new { entity.Status }, cancellationToken);
        return Success(await BuildMetricReviewItemAsync(entity.Id, cancellationToken)
            ?? new MetricReviewItem(entity.Id, entity.ProjectId, string.Empty, entity.ReviewPeriod, entity.ReviewedBy, entity.Status, entity.Summary, entity.OpenActionCount, entity.UpdatedAt));
    }

    public async Task<MetricsCommandResult<MetricReviewItem>> UpdateMetricReviewAsync(Guid metricReviewId, UpdateMetricReviewRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.MetricReviews.SingleOrDefaultAsync(x => x.Id == metricReviewId, cancellationToken);
        if (entity is null)
        {
            return NotFound<MetricReviewItem>(ApiErrorCodes.ResourceNotFound, "Metric review not found.");
        }

        var nextStatus = request.Status.Trim().ToLowerInvariant();
        if (!MetricReviewStatuses.Contains(nextStatus) || !IsValidTransition(entity.Status, nextStatus, MetricReviewStatuses))
        {
            return Validation<MetricReviewItem>(ApiErrorCodes.InvalidWorkflowTransition, "Metric review transition is invalid.");
        }

        var openActionCount = Math.Max(0, request.OpenActionCount);
        if (nextStatus == "closed" && openActionCount > 0)
        {
            return Validation<MetricReviewItem>(ApiErrorCodes.MetricReviewOpenActionsExist, "Metric review cannot close while open follow-up actions remain.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            ReviewPeriod = request.ReviewPeriod.Trim(),
            ReviewedBy = request.ReviewedBy.Trim(),
            Status = nextStatus,
            Summary = string.IsNullOrWhiteSpace(request.Summary) ? null : request.Summary.Trim(),
            OpenActionCount = openActionCount,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("update", "metric_review", entity.Id, 200, new { Status = nextStatus, OpenActionCount = openActionCount });
        await AppendBusinessAsync("metric_review_updated", "metric_review", entity.Id, $"Updated metric review {entity.ReviewPeriod}", actorUserId, null, new { Status = nextStatus }, cancellationToken);
        return Success((await BuildMetricReviewItemAsync(entity.Id, cancellationToken))!);
    }

    public async Task<MetricsCommandResult<TrendReportItem>> CreateTrendReportAsync(CreateTrendReportRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var validation = await ValidateTrendReportAsync(request.ProjectId, request.MetricDefinitionId, request.PeriodFrom, request.PeriodTo, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var entity = new TrendReportEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            MetricDefinitionId = request.MetricDefinitionId!.Value,
            PeriodFrom = request.PeriodFrom!.Value,
            PeriodTo = request.PeriodTo!.Value,
            Status = NormalizeTrendStatus(request.Status),
            ReportRef = NormalizeOptional(request.ReportRef, 512),
            TrendDirection = NormalizeOptional(request.TrendDirection, 64),
            Variance = request.Variance,
            RecommendedAction = NormalizeOptional(request.RecommendedAction, 2000),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.TrendReports.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("create", "trend_report", entity.Id, 201, new { entity.Status, entity.MetricDefinitionId });
        await AppendBusinessAsync("trend_report_created", "trend_report", entity.Id, $"Created trend report for metric {entity.MetricDefinitionId}", actorUserId, null, new { entity.Status }, cancellationToken);
        return Success((await queries.GetTrendReportAsync(entity.Id, cancellationToken))!);
    }

    public async Task<MetricsCommandResult<TrendReportItem>> UpdateTrendReportAsync(Guid trendReportId, UpdateTrendReportRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.TrendReports.SingleOrDefaultAsync(x => x.Id == trendReportId, cancellationToken);
        if (entity is null)
        {
            return NotFound<TrendReportItem>(ApiErrorCodes.ResourceNotFound, "Trend report not found.");
        }

        var validation = await ValidateTrendReportAsync(request.ProjectId, request.MetricDefinitionId, request.PeriodFrom, request.PeriodTo, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var nextStatus = NormalizeTrendStatus(request.Status);
        if (!IsValidTransition(entity.Status, nextStatus, TrendReportStatuses))
        {
            return Validation<TrendReportItem>(ApiErrorCodes.InvalidWorkflowTransition, "Trend report transition is invalid.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            ProjectId = request.ProjectId,
            MetricDefinitionId = request.MetricDefinitionId!.Value,
            PeriodFrom = request.PeriodFrom!.Value,
            PeriodTo = request.PeriodTo!.Value,
            Status = nextStatus,
            ReportRef = NormalizeOptional(request.ReportRef, 512),
            TrendDirection = NormalizeOptional(request.TrendDirection, 64),
            Variance = request.Variance,
            RecommendedAction = NormalizeOptional(request.RecommendedAction, 2000),
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("update", "trend_report", entity.Id, 200, new { Status = nextStatus, entity.MetricDefinitionId });
        await AppendBusinessAsync("trend_report_updated", "trend_report", entity.Id, $"Updated trend report {entity.Id}", actorUserId, null, new { Status = nextStatus }, cancellationToken);
        return Success((await queries.GetTrendReportAsync(entity.Id, cancellationToken))!);
    }

    public async Task<MetricsCommandResult<PerformanceBaselineCommandResponse>> CreatePerformanceBaselineAsync(CreatePerformanceBaselineRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (!request.TargetValue.HasValue)
        {
            return Validation<PerformanceBaselineCommandResponse>(ApiErrorCodes.MetricTargetRequired, "Performance baseline target value is required.");
        }

        if (!request.ThresholdValue.HasValue)
        {
            return Validation<PerformanceBaselineCommandResponse>(ApiErrorCodes.MetricThresholdRequired, "Performance baseline threshold value is required.");
        }

        var entity = new PerformanceBaselineEntity
        {
            Id = Guid.NewGuid(),
            ScopeType = request.ScopeType.Trim().ToLowerInvariant(),
            ScopeRef = request.ScopeRef.Trim(),
            MetricName = request.MetricName.Trim(),
            TargetValue = request.TargetValue.Value,
            ThresholdValue = request.ThresholdValue.Value,
            Status = "draft",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.PerformanceBaselines.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("create", "performance_baseline", entity.Id, 201, new { entity.ScopeType, entity.MetricName, entity.Status });
        await AppendBusinessAsync("performance_baseline_created", "performance_baseline", entity.Id, $"Created performance baseline {entity.MetricName}", actorUserId, null, new { entity.Status }, cancellationToken);
        return Success(new PerformanceBaselineCommandResponse(entity.Id, entity.Status));
    }

    public async Task<MetricsCommandResult<PerformanceBaselineCommandResponse>> UpdatePerformanceBaselineAsync(Guid performanceBaselineId, UpdatePerformanceBaselineRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.PerformanceBaselines.SingleOrDefaultAsync(x => x.Id == performanceBaselineId, cancellationToken);
        if (entity is null)
        {
            return NotFound<PerformanceBaselineCommandResponse>(ApiErrorCodes.ResourceNotFound, "Performance baseline not found.");
        }

        if (!request.TargetValue.HasValue)
        {
            return Validation<PerformanceBaselineCommandResponse>(ApiErrorCodes.MetricTargetRequired, "Performance baseline target value is required.");
        }

        if (!request.ThresholdValue.HasValue)
        {
            return Validation<PerformanceBaselineCommandResponse>(ApiErrorCodes.MetricThresholdRequired, "Performance baseline threshold value is required.");
        }

        var nextStatus = NormalizePerformanceBaselineStatus(request.Status);
        if (!IsValidTransition(entity.Status, nextStatus, PerformanceBaselineStatuses))
        {
            return Validation<PerformanceBaselineCommandResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Performance baseline transition is invalid.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            ScopeType = request.ScopeType.Trim().ToLowerInvariant(),
            ScopeRef = request.ScopeRef.Trim(),
            MetricName = request.MetricName.Trim(),
            TargetValue = request.TargetValue.Value,
            ThresholdValue = request.ThresholdValue.Value,
            Status = nextStatus,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("update", "performance_baseline", entity.Id, 200, new { entity.MetricName, Status = nextStatus });
        await AppendBusinessAsync("performance_baseline_updated", "performance_baseline", entity.Id, $"Updated performance baseline {entity.MetricName}", actorUserId, null, new { Status = nextStatus }, cancellationToken);
        return Success(new PerformanceBaselineCommandResponse(entity.Id, nextStatus));
    }

    public async Task<MetricsCommandResult<CapacityReviewItem>> CreateCapacityReviewAsync(CreateCapacityReviewRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = new CapacityReviewEntity
        {
            Id = Guid.NewGuid(),
            ScopeRef = request.ScopeRef.Trim(),
            ReviewPeriod = request.ReviewPeriod.Trim(),
            ReviewedBy = request.ReviewedBy.Trim(),
            Summary = request.Summary.Trim(),
            ActionCount = Math.Max(0, request.ActionCount),
            Status = "planned",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.CapacityReviews.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("create", "capacity_review", entity.Id, 201, new { entity.ScopeRef, entity.Status });
        await AppendBusinessAsync("capacity_review_created", "capacity_review", entity.Id, $"Created capacity review for {entity.ScopeRef}", actorUserId, null, new { entity.Status }, cancellationToken);
        return Success(new CapacityReviewItem(entity.Id, entity.ScopeRef, entity.ReviewPeriod, entity.ReviewedBy, entity.Status, entity.Summary, entity.ActionCount, entity.UpdatedAt));
    }

    public async Task<MetricsCommandResult<CapacityReviewItem>> UpdateCapacityReviewAsync(Guid capacityReviewId, UpdateCapacityReviewRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.CapacityReviews.SingleOrDefaultAsync(x => x.Id == capacityReviewId, cancellationToken);
        if (entity is null)
        {
            return NotFound<CapacityReviewItem>(ApiErrorCodes.ResourceNotFound, "Capacity review not found.");
        }

        var nextStatus = NormalizeCapacityReviewStatus(request.Status);
        if (!IsValidTransition(entity.Status, nextStatus, CapacityReviewStatuses))
        {
            return Validation<CapacityReviewItem>(ApiErrorCodes.InvalidWorkflowTransition, "Capacity review transition is invalid.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            ScopeRef = request.ScopeRef.Trim(),
            ReviewPeriod = request.ReviewPeriod.Trim(),
            ReviewedBy = request.ReviewedBy.Trim(),
            Summary = request.Summary.Trim(),
            ActionCount = Math.Max(0, request.ActionCount),
            Status = nextStatus,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("update", "capacity_review", entity.Id, 200, new { entity.ScopeRef, Status = nextStatus });
        await AppendBusinessAsync("capacity_review_updated", "capacity_review", entity.Id, $"Updated capacity review for {entity.ScopeRef}", actorUserId, null, new { Status = nextStatus }, cancellationToken);
        return Success(new CapacityReviewItem(entity.Id, request.ScopeRef.Trim(), request.ReviewPeriod.Trim(), request.ReviewedBy.Trim(), nextStatus, request.Summary.Trim(), Math.Max(0, request.ActionCount), DateTimeOffset.UtcNow));
    }

    public async Task<MetricsCommandResult<SlowOperationReviewItem>> CreateSlowOperationReviewAsync(CreateSlowOperationReviewRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = new SlowOperationReviewEntity
        {
            Id = Guid.NewGuid(),
            OperationType = request.OperationType.Trim().ToLowerInvariant(),
            OperationKey = request.OperationKey.Trim(),
            ObservedLatencyMs = Math.Max(0, request.ObservedLatencyMs),
            FrequencyPerHour = request.FrequencyPerHour,
            Status = NormalizeSlowOperationStatus(request.Status),
            OwnerUserId = request.OwnerUserId.Trim(),
            OptimizationSummary = NormalizeOptional(request.OptimizationSummary, 4000),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.SlowOperationReviews.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("create", "slow_operation_review", entity.Id, 201, new { entity.OperationType, entity.Status });
        await AppendBusinessAsync("slow_operation_review_created", "slow_operation_review", entity.Id, $"Created slow operation review {entity.OperationKey}", actorUserId, null, new { entity.Status }, cancellationToken);
        return Success(new SlowOperationReviewItem(entity.Id, entity.OperationType, entity.OperationKey, entity.ObservedLatencyMs, entity.FrequencyPerHour, entity.Status, entity.OwnerUserId, entity.OptimizationSummary, entity.UpdatedAt));
    }

    public async Task<MetricsCommandResult<SlowOperationReviewItem>> UpdateSlowOperationReviewAsync(Guid slowOperationReviewId, UpdateSlowOperationReviewRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.SlowOperationReviews.SingleOrDefaultAsync(x => x.Id == slowOperationReviewId, cancellationToken);
        if (entity is null)
        {
            return NotFound<SlowOperationReviewItem>(ApiErrorCodes.ResourceNotFound, "Slow operation review not found.");
        }

        var nextStatus = NormalizeSlowOperationStatus(request.Status);
        if (!IsValidTransition(entity.Status, nextStatus, SlowOperationStatuses))
        {
            return Validation<SlowOperationReviewItem>(ApiErrorCodes.InvalidWorkflowTransition, "Slow operation review transition is invalid.");
        }

        var optimizationSummary = NormalizeOptional(request.OptimizationSummary, 4000);
        if (nextStatus == "closed" && entity.Status != "verified")
        {
            return Validation<SlowOperationReviewItem>(ApiErrorCodes.SlowOperationVerificationRequired, "Slow operation cannot close without verification.");
        }

        if (nextStatus is "verified" or "closed" && string.IsNullOrWhiteSpace(optimizationSummary))
        {
            return Validation<SlowOperationReviewItem>(ApiErrorCodes.SlowOperationVerificationRequired, "Optimization verification summary is required before verification or closure.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            OperationType = request.OperationType.Trim().ToLowerInvariant(),
            OperationKey = request.OperationKey.Trim(),
            ObservedLatencyMs = Math.Max(0, request.ObservedLatencyMs),
            FrequencyPerHour = request.FrequencyPerHour,
            Status = nextStatus,
            OwnerUserId = request.OwnerUserId.Trim(),
            OptimizationSummary = optimizationSummary,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("update", "slow_operation_review", entity.Id, 200, new { entity.OperationType, Status = nextStatus });
        await AppendBusinessAsync("slow_operation_review_updated", "slow_operation_review", entity.Id, $"Updated slow operation review {entity.OperationKey}", actorUserId, null, new { Status = nextStatus }, cancellationToken);
        return Success(new SlowOperationReviewItem(entity.Id, request.OperationType.Trim().ToLowerInvariant(), request.OperationKey.Trim(), Math.Max(0, request.ObservedLatencyMs), request.FrequencyPerHour, nextStatus, request.OwnerUserId.Trim(), optimizationSummary, DateTimeOffset.UtcNow));
    }

    public async Task<MetricsCommandResult<PerformanceGateItem>> EvaluatePerformanceGateAsync(EvaluatePerformanceGateRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var result = NormalizePerformanceGateResult(request.Result);
        var entity = new PerformanceGateResultEntity
        {
            Id = Guid.NewGuid(),
            ScopeRef = request.ScopeRef.Trim(),
            EvaluatedAt = DateTimeOffset.UtcNow,
            Result = result,
            Reason = NormalizeOptional(request.Reason, 2000),
            EvidenceRef = NormalizeOptional(request.EvidenceRef, 512),
            EvaluatedByUserId = actorUserId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.PerformanceGateResults.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("evaluate", "performance_gate_result", entity.Id, 201, new { entity.ScopeRef, entity.Result });
        await AppendBusinessAsync("performance_gate_evaluated", "performance_gate_result", entity.Id, $"Evaluated performance gate for {entity.ScopeRef}", actorUserId, entity.Reason, new { entity.Result }, cancellationToken);
        return Success(new PerformanceGateItem(entity.Id, entity.ScopeRef, entity.EvaluatedAt, entity.Result, entity.Reason, entity.OverrideReason, entity.EvidenceRef, entity.EvaluatedByUserId, entity.OverriddenByUserId));
    }

    public async Task<MetricsCommandResult<PerformanceGateOverrideResponse>> OverridePerformanceGateAsync(Guid performanceGateId, OverridePerformanceGateRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.PerformanceGateResults.SingleOrDefaultAsync(x => x.Id == performanceGateId, cancellationToken);
        if (entity is null)
        {
            return NotFound<PerformanceGateOverrideResponse>(ApiErrorCodes.ResourceNotFound, "Performance gate not found.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return Validation<PerformanceGateOverrideResponse>(ApiErrorCodes.PerformanceGateOverrideReasonRequired, "Performance gate override reason is required.");
        }

        if (entity.Result != "failed")
        {
            return Validation<PerformanceGateOverrideResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Only failed performance gates can be overridden.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Result = "overridden",
            OverrideReason = request.Reason.Trim(),
            OverriddenByUserId = actorUserId,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("override", "performance_gate_result", entity.Id, 200, new { Result = "overridden" }, request.Reason.Trim());
        await AppendBusinessAsync("performance_gate_overridden", "performance_gate_result", entity.Id, $"Overrode performance gate for {entity.ScopeRef}", actorUserId, request.Reason.Trim(), new { Result = "overridden" }, cancellationToken);
        return Success(new PerformanceGateOverrideResponse(entity.Id, "overridden", request.Reason.Trim()));
    }

    private async Task<MetricsCommandResult<TrendReportItem>?> ValidateTrendReportAsync(Guid projectId, Guid? metricDefinitionId, DateOnly? periodFrom, DateOnly? periodTo, CancellationToken cancellationToken)
    {
        if (!await dbContext.Projects.AnyAsync(x => x.Id == projectId, cancellationToken))
        {
            return NotFound<TrendReportItem>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        if (!metricDefinitionId.HasValue)
        {
            return Validation<TrendReportItem>(ApiErrorCodes.TrendMetricRequired, "Trend report metric is required.");
        }

        if (!await dbContext.MetricDefinitions.AnyAsync(x => x.Id == metricDefinitionId.Value, cancellationToken))
        {
            return NotFound<TrendReportItem>(ApiErrorCodes.MetricDefinitionNotFound, "Metric definition not found.");
        }

        if (!periodFrom.HasValue || !periodTo.HasValue || periodTo.Value < periodFrom.Value)
        {
            return Validation<TrendReportItem>(ApiErrorCodes.TrendPeriodRequired, "Trend report period is required.");
        }

        return null;
    }

    private async Task<QualityGateResultItem?> BuildQualityGateItemAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await queries.ListQualityGatesAsync(new QualityGateListQuery(null, null, null, 1, 100), cancellationToken);
        return result.Items.SingleOrDefault(x => x.Id == id);
    }

    private async Task<MetricReviewItem?> BuildMetricReviewItemAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await queries.ListMetricReviewsAsync(new MetricReviewListQuery(null, null, null, null, 1, 100), cancellationToken);
        return result.Items.SingleOrDefault(x => x.Id == id);
    }

    private void AppendAudit(string action, string entityType, Guid entityId, int statusCode, object? metadata, string? reason = null) =>
        auditLogWriter.Append(new AuditLogEntry("metrics", action, entityType, entityId.ToString(), StatusCode: statusCode, Reason: reason, Metadata: metadata, Audience: LogAudience.AuditOnly));

    private Task AppendBusinessAsync(string eventType, string entityType, Guid entityId, string summary, string? actorUserId, string? reason, object? metadata, CancellationToken cancellationToken) =>
        businessAuditEventWriter.AppendAsync("metrics", eventType, entityType, entityId.ToString(), summary, reason, metadata, cancellationToken);

    private static DateTimeOffset CalculateNextRun(DateTimeOffset now, string frequency) =>
        frequency switch
        {
            "hourly" => now.AddHours(1),
            "daily" => now.AddDays(1),
            "weekly" => now.AddDays(7),
            _ => now.AddDays(1)
        };

    private static bool IsValidTransition(string current, string next, IReadOnlyList<string> states)
    {
        var currentIndex = states.ToList().IndexOf(current.ToLowerInvariant());
        var nextIndex = states.ToList().IndexOf(next.ToLowerInvariant());
        return currentIndex >= 0 && nextIndex >= 0 && nextIndex >= currentIndex && nextIndex - currentIndex <= 1;
    }

    private static string NormalizeTrendStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        "approved" => "approved",
        "archived" => "archived",
        _ => "draft"
    };

    private static string NormalizePerformanceBaselineStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        "approved" => "approved",
        "active" => "active",
        "superseded" => "superseded",
        _ => "draft"
    };

    private static string NormalizeCapacityReviewStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        "reviewed" => "reviewed",
        "actioned" => "actioned",
        "closed" => "closed",
        _ => "planned"
    };

    private static string NormalizeSlowOperationStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        "investigating" => "investigating",
        "optimized" => "optimized",
        "verified" => "verified",
        "closed" => "closed",
        _ => "open"
    };

    private static string NormalizePerformanceGateResult(string result) => result.Trim().ToLowerInvariant() switch
    {
        "passed" => "passed",
        "failed" => "failed",
        _ => "pending"
    };

    private static string? NormalizeOptional(string? value, int maxLength) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim().Length <= maxLength
                ? value.Trim()
                : value.Trim()[..maxLength];

    private static MetricsCommandResult<T> Success<T>(T value) => new(MetricsCommandStatus.Success, value);
    private static MetricsCommandResult<T> Validation<T>(string code, string message) => new(MetricsCommandStatus.ValidationError, default, code, message);
    private static MetricsCommandResult<T> NotFound<T>(string code, string message) => new(MetricsCommandStatus.NotFound, default, code, message);
    private static MetricsCommandResult<T> Conflict<T>(string code, string message) => new(MetricsCommandStatus.Conflict, default, code, message);
}
