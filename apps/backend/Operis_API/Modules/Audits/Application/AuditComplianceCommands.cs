using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Audits.Contracts;
using Operis_API.Modules.Audits.Infrastructure;
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
