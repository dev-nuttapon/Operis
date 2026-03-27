using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Audits.Contracts;
using Operis_API.Modules.Audits.Infrastructure;
using Operis_API.Modules.ChangeControl.Infrastructure;
using Operis_API.Modules.Documents.Infrastructure;
using Operis_API.Modules.Governance.Infrastructure;
using Operis_API.Modules.Operations.Infrastructure;
using Operis_API.Modules.Requirements.Infrastructure;
using Operis_API.Modules.Verification.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Audits.Application;

public sealed class AuditComplianceCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IBusinessAuditEventWriter businessAuditEventWriter,
    IAuditComplianceQueries queries) : IAuditComplianceCommands
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly string[] AuditPlanStatuses = ["planned", "in_review", "findings_issued", "closed"];
    private static readonly string[] AuditFindingStatuses = ["open", "action_planned", "in_progress", "verified", "closed"];
    private static readonly string[] EvidenceRuleStatuses = ["draft", "active", "retired"];
    private static readonly HashSet<string> SupportedProcessAreas = new(StringComparer.OrdinalIgnoreCase)
    {
        "process-assets-planning",
        "requirements-traceability",
        "document-governance",
        "change-configuration",
        "verification-release",
        "audit-capa",
        "security-resilience"
    };
    private static readonly HashSet<string> SupportedArtifactTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "project_plan_baseline",
        "tailoring_approval",
        "requirement_baseline",
        "requirement_test_traceability",
        "approved_document",
        "approved_change_request",
        "baseline_registry_link",
        "approved_uat_signoff",
        "resolved_audit_finding",
        "security_review_completion"
    };

    public async Task<AuditComplianceCommandResult<AuditPlanDetailResponse>> CreateAuditPlanAsync(CreateAuditPlanRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Projects.AnyAsync(x => x.Id == request.ProjectId, cancellationToken))
        {
            return NotFound<AuditPlanDetailResponse>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Scope) || string.IsNullOrWhiteSpace(request.Criteria) || string.IsNullOrWhiteSpace(request.OwnerUserId))
        {
            return Validation<AuditPlanDetailResponse>(ApiErrorCodes.RequestValidationFailed, "Audit plan title, scope, criteria, and owner are required.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new AuditPlanEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Title = request.Title.Trim(),
            Scope = request.Scope.Trim(),
            Criteria = request.Criteria.Trim(),
            PlannedAt = request.PlannedAt,
            Status = "planned",
            OwnerUserId = request.OwnerUserId.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("create", "audit_plan", entity.Id, StatusCodes.Status201Created, new { entity.Status });
        await AppendBusinessAsync("audit_plan_created", "audit_plan", entity.Id, "Created audit plan", actorUserId, null, null, cancellationToken);
        return await SuccessPlanAsync(entity.Id, cancellationToken);
    }

    public async Task<AuditComplianceCommandResult<AuditPlanDetailResponse>> UpdateAuditPlanAsync(Guid auditPlanId, UpdateAuditPlanRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<AuditPlanEntity>().SingleOrDefaultAsync(x => x.Id == auditPlanId, cancellationToken);
        if (entity is null)
        {
            return NotFound<AuditPlanDetailResponse>(ApiErrorCodes.AuditPlanNotFound, "Audit plan not found.");
        }

        var nextStatus = request.Status.Trim().ToLowerInvariant();
        if (!AuditPlanStatuses.Contains(nextStatus, StringComparer.OrdinalIgnoreCase))
        {
            return Validation<AuditPlanDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Audit plan status is invalid.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Title = request.Title.Trim(),
            Scope = request.Scope.Trim(),
            Criteria = request.Criteria.Trim(),
            PlannedAt = request.PlannedAt,
            Status = nextStatus,
            OwnerUserId = request.OwnerUserId.Trim(),
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("update", "audit_plan", auditPlanId, StatusCodes.Status200OK, new { Status = nextStatus });
        await AppendBusinessAsync("audit_plan_updated", "audit_plan", auditPlanId, "Updated audit plan", actorUserId, null, new { Status = nextStatus }, cancellationToken);
        return await SuccessPlanAsync(auditPlanId, cancellationToken);
    }

    public async Task<AuditComplianceCommandResult<AuditFindingItem>> CreateAuditFindingAsync(CreateAuditFindingRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var plan = await dbContext.Set<AuditPlanEntity>().SingleOrDefaultAsync(x => x.Id == request.AuditPlanId, cancellationToken);
        if (plan is null)
        {
            return NotFound<AuditFindingItem>(ApiErrorCodes.AuditPlanNotFound, "Audit plan not found.");
        }

        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description) || string.IsNullOrWhiteSpace(request.Severity) || string.IsNullOrWhiteSpace(request.OwnerUserId))
        {
            return Validation<AuditFindingItem>(ApiErrorCodes.RequestValidationFailed, "Audit finding code, title, description, severity, and owner are required.");
        }

        var duplicate = await dbContext.Set<AuditFindingEntity>().AnyAsync(x => x.AuditPlanId == request.AuditPlanId && x.Code == request.Code.Trim().ToUpperInvariant(), cancellationToken);
        if (duplicate)
        {
            return Conflict<AuditFindingItem>(ApiErrorCodes.AuditFindingCodeDuplicate, "Audit finding code already exists.");
        }

        var entity = new AuditFindingEntity
        {
            Id = Guid.NewGuid(),
            AuditPlanId = request.AuditPlanId,
            Code = request.Code.Trim().ToUpperInvariant(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = request.Severity.Trim().ToLowerInvariant(),
            Status = "open",
            OwnerUserId = request.OwnerUserId.Trim(),
            DueDate = request.DueDate,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Add(entity);
        if (string.Equals(plan.Status, "planned", StringComparison.OrdinalIgnoreCase) || string.Equals(plan.Status, "in_review", StringComparison.OrdinalIgnoreCase))
        {
            dbContext.Entry(plan).CurrentValues.SetValues(plan with { Status = "findings_issued", UpdatedAt = DateTimeOffset.UtcNow });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("create", "audit_finding", entity.Id, StatusCodes.Status201Created, new { entity.Code, entity.Status });
        await AppendBusinessAsync("audit_finding_created", "audit_finding", entity.Id, "Created audit finding", actorUserId, null, new { entity.Code }, cancellationToken);
        return Success(MapFinding(entity, plan.Title));
    }

    public async Task<AuditComplianceCommandResult<AuditFindingItem>> UpdateAuditFindingAsync(Guid auditFindingId, UpdateAuditFindingRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var item = await (
            from finding in dbContext.Set<AuditFindingEntity>()
            where finding.Id == auditFindingId
            join plan in dbContext.Set<AuditPlanEntity>() on finding.AuditPlanId equals plan.Id
            select new { Finding = finding, Plan = plan }).SingleOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return NotFound<AuditFindingItem>(ApiErrorCodes.AuditFindingNotFound, "Audit finding not found.");
        }

        var nextStatus = request.Status.Trim().ToLowerInvariant();
        if (!AuditFindingStatuses.Contains(nextStatus, StringComparer.OrdinalIgnoreCase))
        {
            return Validation<AuditFindingItem>(ApiErrorCodes.InvalidWorkflowTransition, "Audit finding status is invalid.");
        }

        dbContext.Entry(item.Finding).CurrentValues.SetValues(item.Finding with
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = request.Severity.Trim().ToLowerInvariant(),
            Status = nextStatus,
            OwnerUserId = request.OwnerUserId.Trim(),
            DueDate = request.DueDate,
            ResolutionSummary = TrimOrNull(request.ResolutionSummary),
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("update", "audit_finding", auditFindingId, StatusCodes.Status200OK, new { Status = nextStatus });
        await AppendBusinessAsync("audit_finding_updated", "audit_finding", auditFindingId, "Updated audit finding", actorUserId, null, new { Status = nextStatus }, cancellationToken);
        return Success(MapFinding(item.Finding with
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = request.Severity.Trim().ToLowerInvariant(),
            Status = nextStatus,
            OwnerUserId = request.OwnerUserId.Trim(),
            DueDate = request.DueDate,
            ResolutionSummary = TrimOrNull(request.ResolutionSummary),
            UpdatedAt = DateTimeOffset.UtcNow
        }, item.Plan.Title));
    }

    public async Task<AuditComplianceCommandResult<AuditFindingItem>> CloseAuditFindingAsync(Guid auditFindingId, CloseAuditFindingRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var item = await (
            from finding in dbContext.Set<AuditFindingEntity>()
            where finding.Id == auditFindingId
            join plan in dbContext.Set<AuditPlanEntity>() on finding.AuditPlanId equals plan.Id
            select new { Finding = finding, Plan = plan }).SingleOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return NotFound<AuditFindingItem>(ApiErrorCodes.AuditFindingNotFound, "Audit finding not found.");
        }

        if (string.IsNullOrWhiteSpace(request.ResolutionSummary))
        {
            return Validation<AuditFindingItem>(ApiErrorCodes.AuditFindingResolutionRequired, "Audit finding cannot close without resolution summary.");
        }

        dbContext.Entry(item.Finding).CurrentValues.SetValues(item.Finding with
        {
            Status = "closed",
            ResolutionSummary = request.ResolutionSummary.Trim(),
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("close", "audit_finding", auditFindingId, StatusCodes.Status200OK, null, request.ResolutionSummary.Trim());
        await AppendBusinessAsync("audit_finding_closed", "audit_finding", auditFindingId, "Closed audit finding", actorUserId, request.ResolutionSummary.Trim(), null, cancellationToken);
        return Success(MapFinding(item.Finding with { Status = "closed", ResolutionSummary = request.ResolutionSummary.Trim(), UpdatedAt = DateTimeOffset.UtcNow }, item.Plan.Title));
    }

    public async Task<AuditComplianceCommandResult<EvidenceExportDetailResponse>> CreateEvidenceExportAsync(CreateEvidenceExportRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ScopeType) || string.IsNullOrWhiteSpace(request.ScopeRef))
        {
            return Validation<EvidenceExportDetailResponse>(ApiErrorCodes.ExportScopeRequired, "Evidence export scope is required.");
        }

        if (!request.From.HasValue || !request.To.HasValue || request.From > request.To)
        {
            return Validation<EvidenceExportDetailResponse>(ApiErrorCodes.ExportDateRangeRequired, "Evidence export requires a valid date range.");
        }

        var artifactTypes = request.IncludedArtifactTypes?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray() ?? [];
        var requestedAt = DateTimeOffset.UtcNow;
        var isAsync = artifactTypes.Length > 5 || request.To.Value - request.From.Value > TimeSpan.FromDays(31);
        var entity = new EvidenceExportEntity
        {
            Id = Guid.NewGuid(),
            RequestedBy = actorUserId ?? "system",
            ScopeType = request.ScopeType.Trim(),
            ScopeRef = request.ScopeRef.Trim(),
            RequestedAt = requestedAt,
            Status = isAsync ? "requested" : "generated",
            OutputRef = isAsync ? null : $"evidence/{requestedAt:yyyyMMdd}/{Guid.NewGuid():N}.zip",
            From = request.From,
            To = request.To,
            IncludedArtifactTypesJson = JsonSerializer.Serialize(artifactTypes, SerializerOptions)
        };

        dbContext.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("export", "evidence_export", entity.Id, isAsync ? StatusCodes.Status202Accepted : StatusCodes.Status201Created, new { entity.ScopeType, entity.ScopeRef, entity.Status });
        await AppendBusinessAsync(isAsync ? "evidence_export_requested" : "evidence_export_generated", "evidence_export", entity.Id, isAsync ? "Requested evidence export" : "Generated evidence export", actorUserId, null, new { entity.ScopeType, entity.ScopeRef, entity.Status }, cancellationToken);
        return await SuccessExportAsync(entity.Id, cancellationToken);
    }

    public async Task<AuditComplianceCommandResult<EvidenceRuleDetailResponse>> CreateEvidenceRuleAsync(CreateEvidenceRuleRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var validation = await ValidateEvidenceRuleRequestAsync(request.RuleCode, request.Title, request.ProcessArea, request.ArtifactType, request.ProjectId, request.Status, request.ExpressionType, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var normalizedCode = request.RuleCode.Trim().ToUpperInvariant();
        if (await dbContext.EvidenceRules.AnyAsync(x => x.RuleCode == normalizedCode, cancellationToken))
        {
            return Conflict<EvidenceRuleDetailResponse>(ApiErrorCodes.ChangeRequestCodeDuplicate, "Evidence rule code already exists.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new EvidenceRuleEntity
        {
            Id = Guid.NewGuid(),
            RuleCode = normalizedCode,
            Title = request.Title.Trim(),
            ProcessArea = request.ProcessArea.Trim().ToLowerInvariant(),
            ArtifactType = request.ArtifactType.Trim().ToLowerInvariant(),
            ProjectId = request.ProjectId,
            Status = request.Status.Trim().ToLowerInvariant(),
            ExpressionType = request.ExpressionType.Trim().ToLowerInvariant(),
            Reason = TrimOrNull(request.Reason),
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.EvidenceRules.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("create", "evidence_rule", entity.Id, StatusCodes.Status201Created, new { entity.ProcessArea, entity.ArtifactType, entity.Status }, entity.Reason);
        await AppendBusinessAsync("evidence_rule_created", "evidence_rule", entity.Id, "Created evidence completeness rule", actorUserId, entity.Reason, new { entity.RuleCode, entity.ProcessArea, entity.ArtifactType, entity.Status }, cancellationToken);
        return await SuccessRuleAsync(entity.Id, cancellationToken);
    }

    public async Task<AuditComplianceCommandResult<EvidenceRuleDetailResponse>> UpdateEvidenceRuleAsync(Guid ruleId, UpdateEvidenceRuleRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.EvidenceRules.SingleOrDefaultAsync(x => x.Id == ruleId, cancellationToken);
        if (entity is null)
        {
            return NotFound<EvidenceRuleDetailResponse>(ApiErrorCodes.ResourceNotFound, "Evidence rule not found.");
        }

        var validation = await ValidateEvidenceRuleRequestAsync(request.RuleCode, request.Title, request.ProcessArea, request.ArtifactType, request.ProjectId, request.Status, request.ExpressionType, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var normalizedCode = request.RuleCode.Trim().ToUpperInvariant();
        if (await dbContext.EvidenceRules.AnyAsync(x => x.Id != ruleId && x.RuleCode == normalizedCode, cancellationToken))
        {
            return Conflict<EvidenceRuleDetailResponse>(ApiErrorCodes.ChangeRequestCodeDuplicate, "Evidence rule code already exists.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            RuleCode = normalizedCode,
            Title = request.Title.Trim(),
            ProcessArea = request.ProcessArea.Trim().ToLowerInvariant(),
            ArtifactType = request.ArtifactType.Trim().ToLowerInvariant(),
            ProjectId = request.ProjectId,
            Status = request.Status.Trim().ToLowerInvariant(),
            ExpressionType = request.ExpressionType.Trim().ToLowerInvariant(),
            Reason = TrimOrNull(request.Reason),
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("update", "evidence_rule", ruleId, StatusCodes.Status200OK, new { request.ProcessArea, request.ArtifactType, request.Status }, request.Reason);
        await AppendBusinessAsync("evidence_rule_updated", "evidence_rule", ruleId, "Updated evidence completeness rule", actorUserId, request.Reason, new { RuleCode = normalizedCode, request.ProcessArea, request.ArtifactType, request.Status }, cancellationToken);
        return await SuccessRuleAsync(ruleId, cancellationToken);
    }

    public async Task<AuditComplianceCommandResult<EvidenceRuleResultDetailResponse>> EvaluateEvidenceRulesAsync(EvaluateEvidenceRulesRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.ProcessArea) && !SupportedProcessAreas.Contains(request.ProcessArea.Trim()))
        {
            return Validation<EvidenceRuleResultDetailResponse>(ApiErrorCodes.EvidenceRuleProcessAreaInvalid, "Evidence process area is invalid.");
        }

        if (request.ProjectId.HasValue && !await dbContext.Projects.AnyAsync(x => x.Id == request.ProjectId.Value, cancellationToken))
        {
            return NotFound<EvidenceRuleResultDetailResponse>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        var rules = await dbContext.EvidenceRules.AsNoTracking()
            .Where(x => x.Status == "active")
            .Where(x => !request.ProjectId.HasValue || x.ProjectId == null || x.ProjectId == request.ProjectId.Value)
            .Where(x => string.IsNullOrWhiteSpace(request.ProcessArea) || x.ProcessArea == request.ProcessArea!.Trim().ToLowerInvariant())
            .OrderBy(x => x.ProcessArea)
            .ThenBy(x => x.RuleCode)
            .ToListAsync(cancellationToken);

        var scopeType = string.IsNullOrWhiteSpace(request.ScopeType)
            ? (request.ProjectId.HasValue ? "project" : "portfolio")
            : request.ScopeType.Trim().ToLowerInvariant();
        var scopeRef = !string.IsNullOrWhiteSpace(request.ScopeRef)
            ? request.ScopeRef.Trim()
            : request.ProjectId?.ToString() ?? "all-projects";
        var startedAt = DateTimeOffset.UtcNow;

        var result = new EvidenceRuleResultEntity
        {
            Id = Guid.NewGuid(),
            ScopeType = scopeType,
            ScopeRef = scopeRef,
            ProjectId = request.ProjectId,
            ProcessArea = string.IsNullOrWhiteSpace(request.ProcessArea) ? null : request.ProcessArea.Trim().ToLowerInvariant(),
            Status = "queued",
            EvaluatedRuleCount = rules.Count,
            MissingItemCount = 0,
            StartedAt = startedAt,
            CompletedAt = startedAt,
            CreatedAt = startedAt,
            UpdatedAt = startedAt
        };

        dbContext.EvidenceRuleResults.Add(result);
        await dbContext.SaveChangesAsync(cancellationToken);

        var olderResults = await dbContext.EvidenceRuleResults
            .Where(x => x.Id != result.Id && x.ScopeType == scopeType && x.ScopeRef == scopeRef && x.Status == "completed")
            .ToListAsync(cancellationToken);

        foreach (var previous in olderResults)
        {
            dbContext.Entry(previous).CurrentValues.SetValues(previous with
            {
                Status = "superseded",
                SupersededByResultId = result.Id,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }

        var missingItems = await BuildEvidenceMissingItemsAsync(result.Id, rules, request.ProjectId, cancellationToken);
        if (missingItems.Count > 0)
        {
            dbContext.EvidenceMissingItems.AddRange(missingItems);
        }

        dbContext.Entry(result).CurrentValues.SetValues(result with
        {
            Status = "completed",
            MissingItemCount = missingItems.Count,
            CompletedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("evaluate", "evidence_rule_result", result.Id, StatusCodes.Status201Created, new { scopeType, scopeRef, EvaluatedRuleCount = rules.Count, MissingItemCount = missingItems.Count });
        await AppendBusinessAsync("evidence_rules_evaluated", "evidence_rule_result", result.Id, "Completed evidence completeness evaluation", actorUserId, null, new { scopeType, scopeRef, EvaluatedRuleCount = rules.Count, MissingItemCount = missingItems.Count }, cancellationToken);
        return await SuccessEvidenceResultAsync(result.Id, cancellationToken);
    }

    private async Task<AuditComplianceCommandResult<AuditPlanDetailResponse>> SuccessPlanAsync(Guid id, CancellationToken cancellationToken)
    {
        var detail = await queries.GetAuditPlanAsync(id, cancellationToken);
        return detail is null ? NotFound<AuditPlanDetailResponse>(ApiErrorCodes.AuditPlanNotFound, "Audit plan not found.") : Success(detail);
    }

    private async Task<AuditComplianceCommandResult<EvidenceExportDetailResponse>> SuccessExportAsync(Guid id, CancellationToken cancellationToken)
    {
        var detail = await queries.GetEvidenceExportAsync(id, cancellationToken);
        return detail is null ? NotFound<EvidenceExportDetailResponse>(ApiErrorCodes.ResourceNotFound, "Evidence export not found.") : Success(detail);
    }

    private async Task<AuditComplianceCommandResult<EvidenceRuleDetailResponse>> SuccessRuleAsync(Guid id, CancellationToken cancellationToken)
    {
        var detail = await queries.GetEvidenceRuleAsync(id, cancellationToken);
        return detail is null ? NotFound<EvidenceRuleDetailResponse>(ApiErrorCodes.ResourceNotFound, "Evidence rule not found.") : Success(detail);
    }

    private async Task<AuditComplianceCommandResult<EvidenceRuleResultDetailResponse>> SuccessEvidenceResultAsync(Guid id, CancellationToken cancellationToken)
    {
        var detail = await queries.GetEvidenceRuleResultAsync(id, cancellationToken);
        return detail is null ? NotFound<EvidenceRuleResultDetailResponse>(ApiErrorCodes.EvidenceResultNotFound, "Evidence evaluation result not found.") : Success(detail);
    }

    private async Task<AuditComplianceCommandResult<EvidenceRuleDetailResponse>?> ValidateEvidenceRuleRequestAsync(
        string ruleCode,
        string title,
        string processArea,
        string artifactType,
        Guid? projectId,
        string status,
        string expressionType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ruleCode) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(processArea) || string.IsNullOrWhiteSpace(artifactType))
        {
            return Validation<EvidenceRuleDetailResponse>(ApiErrorCodes.EvidenceRuleTargetRequired, "Evidence rule target is required.");
        }

        if (!SupportedProcessAreas.Contains(processArea.Trim()))
        {
            return Validation<EvidenceRuleDetailResponse>(ApiErrorCodes.EvidenceRuleProcessAreaInvalid, "Evidence process area is invalid.");
        }

        if (!SupportedArtifactTypes.Contains(artifactType.Trim()))
        {
            return Validation<EvidenceRuleDetailResponse>(ApiErrorCodes.EvidenceRuleTargetRequired, "Evidence artifact target is invalid.");
        }

        if (!EvidenceRuleStatuses.Contains(status.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            return Validation<EvidenceRuleDetailResponse>(ApiErrorCodes.EvidenceRuleStatusInvalid, "Evidence rule status is invalid.");
        }

        if (!string.Equals(expressionType.Trim(), "required", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<EvidenceRuleDetailResponse>(ApiErrorCodes.EvidenceRuleExpressionInvalid, "Evidence rule expression is invalid.");
        }

        if (projectId.HasValue && !await dbContext.Projects.AnyAsync(x => x.Id == projectId.Value, cancellationToken))
        {
            return NotFound<EvidenceRuleDetailResponse>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        return null;
    }

    private async Task<List<EvidenceMissingItemEntity>> BuildEvidenceMissingItemsAsync(Guid resultId, IReadOnlyList<EvidenceRuleEntity> rules, Guid? scopedProjectId, CancellationToken cancellationToken)
    {
        if (rules.Count == 0)
        {
            return [];
        }

        var projects = await dbContext.Projects.AsNoTracking()
            .Where(x => !scopedProjectId.HasValue || x.Id == scopedProjectId.Value)
            .Select(x => new { x.Id, x.Code, x.Name })
            .ToListAsync(cancellationToken);

        var projectIds = projects.Select(x => x.Id).ToArray();
        var projectPlans = await dbContext.ProjectPlans.AsNoTracking().Where(x => projectIds.Contains(x.ProjectId)).ToListAsync(cancellationToken);
        var tailoringRecords = await dbContext.TailoringRecords.AsNoTracking().Where(x => projectIds.Contains(x.ProjectId)).ToListAsync(cancellationToken);
        var requirements = await dbContext.Requirements.AsNoTracking().Where(x => projectIds.Contains(x.ProjectId)).ToListAsync(cancellationToken);
        var requirementBaselines = await dbContext.RequirementBaselines.AsNoTracking().Where(x => projectIds.Contains(x.ProjectId)).ToListAsync(cancellationToken);
        var traceabilityLinks = await dbContext.TraceabilityLinks.AsNoTracking().ToListAsync(cancellationToken);
        var documents = await dbContext.Documents.AsNoTracking().Where(x => x.ProjectId.HasValue && projectIds.Contains(x.ProjectId.Value)).ToListAsync(cancellationToken);
        var changeRequests = await dbContext.ChangeRequests.AsNoTracking().Where(x => projectIds.Contains(x.ProjectId)).ToListAsync(cancellationToken);
        var baselines = await dbContext.BaselineRegistry.AsNoTracking().Where(x => projectIds.Contains(x.ProjectId)).ToListAsync(cancellationToken);
        var uatSignoffs = await dbContext.UatSignoffs.AsNoTracking().Where(x => projectIds.Contains(x.ProjectId)).ToListAsync(cancellationToken);
        var auditPlanIds = await dbContext.AuditPlans.AsNoTracking().Where(x => projectIds.Contains(x.ProjectId)).Select(x => x.Id).ToListAsync(cancellationToken);
        var auditFindings = await dbContext.AuditFindings.AsNoTracking().Where(x => auditPlanIds.Contains(x.AuditPlanId)).ToListAsync(cancellationToken);
        var securityReviews = await dbContext.SecurityReviews.AsNoTracking()
            .Where(x => x.ScopeType == "project" && projectIds.Select(id => id.ToString()).Contains(x.ScopeRef))
            .ToListAsync(cancellationToken);

        var detectedAt = DateTimeOffset.UtcNow;
        var items = new List<EvidenceMissingItemEntity>();

        foreach (var project in projects)
        {
            foreach (var rule in rules.Where(x => !x.ProjectId.HasValue || x.ProjectId == project.Id))
            {
                var missing = TryBuildMissingItem(rule, project.Id, project.Code, project.Name, projectPlans, tailoringRecords, requirements, requirementBaselines, traceabilityLinks, documents, changeRequests, baselines, uatSignoffs, auditFindings, securityReviews, detectedAt);
                if (missing is not null)
                {
                    items.Add(missing with
                    {
                        Id = Guid.NewGuid(),
                        ResultId = resultId,
                        RuleId = rule.Id,
                        ProjectId = project.Id,
                        CreatedAt = detectedAt
                    });
                }
            }
        }

        return items;
    }

    private static EvidenceMissingItemEntity? TryBuildMissingItem(
        EvidenceRuleEntity rule,
        Guid projectId,
        string projectCode,
        string projectName,
        IReadOnlyList<ProjectPlanEntity> projectPlans,
        IReadOnlyList<TailoringRecordEntity> tailoringRecords,
        IReadOnlyList<RequirementEntity> requirements,
        IReadOnlyList<RequirementBaselineEntity> requirementBaselines,
        IReadOnlyList<TraceabilityLinkEntity> traceabilityLinks,
        IReadOnlyList<DocumentEntity> documents,
        IReadOnlyList<ChangeRequestEntity> changeRequests,
        IReadOnlyList<BaselineRegistryEntity> baselines,
        IReadOnlyList<UatSignoffEntity> uatSignoffs,
        IReadOnlyList<AuditFindingEntity> auditFindings,
        IReadOnlyList<SecurityReviewEntity> securityReviews,
        DateTimeOffset detectedAt)
    {
        var scope = $"{projectCode} · {projectName}";
        return rule.ArtifactType switch
        {
            "project_plan_baseline" when !projectPlans.Any(x => x.ProjectId == projectId && x.Status == "baseline")
                => BuildMissing(rule, ApiErrorCodes.EvidenceMissingBaseline, "Project plan baseline is missing.", "governance", $"/app/projects/{projectId}", scope, "project_plan", null, null, detectedAt),
            "tailoring_approval" when !tailoringRecords.Any(x => x.ProjectId == projectId && x.Status == "approved")
                => BuildMissing(rule, ApiErrorCodes.EvidenceMissingApproval, "Approved tailoring record is missing.", "governance", "/app/governance/tailoring", scope, "tailoring_record", null, null, detectedAt),
            "requirement_baseline" when requirements.Any(x => x.ProjectId == projectId && x.Status == "approved") && !requirementBaselines.Any(x => x.ProjectId == projectId && x.Status == "locked")
                => BuildMissing(rule, ApiErrorCodes.EvidenceMissingBaseline, "Requirement baseline is missing.", "requirements", "/app/requirements/baselines", scope, "requirement_baseline", null, null, detectedAt),
            "requirement_test_traceability" when requirements.Any(x => x.ProjectId == projectId && x.Status == "approved") && !traceabilityLinks.Any(x => x.SourceType == "requirement" && x.TargetType == "test" && requirements.Select(r => r.Id.ToString()).Contains(x.SourceId))
                => BuildMissing(rule, ApiErrorCodes.EvidenceMissingTraceability, "Requirement-to-test traceability is missing.", "requirements", "/app/requirements/traceability", scope, "traceability_link", null, null, detectedAt),
            "approved_document" when !documents.Any(x => x.ProjectId == projectId && (x.Status == "approved" || x.Status == "baselined" || x.Status == "published"))
                => BuildMissing(rule, ApiErrorCodes.EvidenceMissingDocument, "Approved governed document is missing.", "documents", "/app/documents", scope, "document", null, null, detectedAt),
            "approved_change_request" when !changeRequests.Any(x => x.ProjectId == projectId && x.Status == "approved")
                => BuildMissing(rule, ApiErrorCodes.EvidenceMissingApproval, "Approved change request is missing.", "change-control", "/app/change-control/change-requests", scope, "change_request", null, null, detectedAt),
            "baseline_registry_link" when changeRequests.Any(x => x.ProjectId == projectId && x.Status == "approved") && !baselines.Any(x => x.ProjectId == projectId && x.ChangeRequestId != null)
                => BuildMissing(rule, ApiErrorCodes.EvidenceMissingBaseline, "Baseline registry link for approved change is missing.", "change-control", "/app/change-control/baseline-registry", scope, "baseline_registry", null, null, detectedAt),
            "approved_uat_signoff" when !uatSignoffs.Any(x => x.ProjectId == projectId && x.Status == "approved")
                => BuildMissing(rule, ApiErrorCodes.EvidenceMissingApproval, "Approved UAT signoff is missing.", "verification", "/app/verification/uat-signoff", scope, "uat_signoff", null, null, detectedAt),
            "resolved_audit_finding" when auditFindings.Any(x => x.Status != "closed")
                => BuildMissing(rule, ApiErrorCodes.AuditFindingResolutionRequired, "Open audit finding remains unresolved.", "audits", "/app/audits/audit-plans", scope, "audit_finding", null, null, detectedAt),
            "security_review_completion" when !securityReviews.Any(x => x.ScopeRef == projectId.ToString() && string.Equals(x.Status, "completed", StringComparison.OrdinalIgnoreCase))
                => BuildMissing(rule, ApiErrorCodes.EvidenceMissingSecurityReview, "Completed security review is missing.", "operations", "/app/operations/security-reviews", scope, "security_review", null, null, detectedAt),
            _ => null
        };
    }

    private static EvidenceMissingItemEntity BuildMissing(
        EvidenceRuleEntity rule,
        string reasonCode,
        string title,
        string module,
        string route,
        string scope,
        string? entityType,
        string? entityId,
        string? metadata,
        DateTimeOffset detectedAt) =>
        new()
        {
            ProcessArea = rule.ProcessArea,
            ArtifactType = rule.ArtifactType,
            ReasonCode = reasonCode,
            Title = title,
            Module = module,
            Route = route,
            Scope = scope,
            EntityType = entityType,
            EntityId = entityId,
            Metadata = metadata,
            DetectedAt = detectedAt
        };

    private void AppendAudit(string action, string entityType, Guid entityId, int statusCode, object? metadata, string? reason = null) =>
        auditLogWriter.Append(new AuditLogEntry("audits", action, entityType, entityId.ToString(), StatusCode: statusCode, Reason: reason, Metadata: metadata, Audience: LogAudience.AuditOnly));

    private async Task AppendBusinessAsync(string eventType, string entityType, Guid entityId, string summary, string? actorUserId, string? reason, object? metadata, CancellationToken cancellationToken) =>
        await businessAuditEventWriter.AppendAsync("audits", eventType, entityType, entityId.ToString(), summary, reason, metadata, cancellationToken);

    private static AuditFindingItem MapFinding(AuditFindingEntity finding, string auditPlanTitle) =>
        new(finding.Id, finding.AuditPlanId, auditPlanTitle, finding.Code, finding.Title, finding.Severity, finding.Status, finding.OwnerUserId, finding.DueDate, finding.ResolutionSummary, finding.UpdatedAt);

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static AuditComplianceCommandResult<T> Success<T>(T value) => new(AuditComplianceCommandStatus.Success, value);
    private static AuditComplianceCommandResult<T> NotFound<T>(string code, string message) => new(AuditComplianceCommandStatus.NotFound, default, code, message);
    private static AuditComplianceCommandResult<T> Validation<T>(string code, string message) => new(AuditComplianceCommandStatus.ValidationError, default, code, message);
    private static AuditComplianceCommandResult<T> Conflict<T>(string code, string message) => new(AuditComplianceCommandStatus.Conflict, default, code, message);
}
