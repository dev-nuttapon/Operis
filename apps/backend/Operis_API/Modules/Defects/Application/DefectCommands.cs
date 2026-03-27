using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Defects.Contracts;
using Operis_API.Modules.Defects.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Defects.Application;

public sealed class DefectCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IDefectQueries queries) : IDefectCommands
{
    private static readonly string[] DefectStatuses = ["open", "in_progress", "resolved", "closed"];
    private static readonly string[] NonConformanceStatuses = ["open", "in_review", "corrective_action", "closed"];

    public async Task<DefectCommandResult<DefectCommandResponse>> CreateDefectAsync(CreateDefectRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Projects.AnyAsync(x => x.Id == request.ProjectId, cancellationToken))
        {
            return NotFound<DefectCommandResponse>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        var code = request.Code.Trim().ToUpperInvariant();
        if (await dbContext.Defects.AnyAsync(x => x.ProjectId == request.ProjectId && x.Code == code, cancellationToken))
        {
            return Conflict<DefectCommandResponse>(ApiErrorCodes.DefectCodeDuplicate, "Defect code already exists.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new DefectEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Code = code,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = request.Severity.Trim().ToLowerInvariant(),
            OwnerUserId = request.OwnerUserId.Trim(),
            Status = "open",
            DetectedInPhase = TrimOrNull(request.DetectedInPhase),
            CorrectiveActionRef = TrimOrNull(request.CorrectiveActionRef),
            AffectedArtifactRefsJson = WriteArray(request.AffectedArtifactRefs),
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Defects.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("create", "defect", entity.Id, actorUserId, new { entity.Code, entity.Status });
        return Success(new DefectCommandResponse(entity.Id, entity.Code, entity.Status));
    }

    public async Task<DefectCommandResult<DefectDetailResponse>> UpdateDefectAsync(Guid defectId, UpdateDefectRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Defects.SingleOrDefaultAsync(x => x.Id == defectId, cancellationToken);
        if (entity is null)
        {
            return NotFound<DefectDetailResponse>(ApiErrorCodes.DefectNotFound, "Defect not found.");
        }

        var nextStatus = request.Status.Trim().ToLowerInvariant();
        if (!DefectStatuses.Contains(nextStatus) || !IsValidTransition(entity.Status, nextStatus, DefectStatuses))
        {
            return Validation<DefectDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Defect transition is invalid.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = request.Severity.Trim().ToLowerInvariant(),
            OwnerUserId = request.OwnerUserId.Trim(),
            DetectedInPhase = TrimOrNull(request.DetectedInPhase),
            CorrectiveActionRef = TrimOrNull(request.CorrectiveActionRef),
            AffectedArtifactRefsJson = WriteArray(request.AffectedArtifactRefs),
            Status = nextStatus,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("update", "defect", entity.Id, actorUserId, new { entity.Code, Status = nextStatus });
        return await BuildDefectDetailAsync(entity.Id, cancellationToken);
    }

    public async Task<DefectCommandResult<DefectDetailResponse>> ResolveDefectAsync(Guid defectId, ResolveDefectRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Defects.SingleOrDefaultAsync(x => x.Id == defectId, cancellationToken);
        if (entity is null)
        {
            return NotFound<DefectDetailResponse>(ApiErrorCodes.DefectNotFound, "Defect not found.");
        }

        if (!IsValidTransition(entity.Status, "resolved", DefectStatuses))
        {
            return Validation<DefectDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Defect cannot move to resolved.");
        }

        if (string.IsNullOrWhiteSpace(request.ResolutionSummary))
        {
            return Validation<DefectDetailResponse>(ApiErrorCodes.DefectResolutionRequired, "Resolution summary is required.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Status = "resolved",
            ResolutionSummary = request.ResolutionSummary.Trim(),
            CorrectiveActionRef = TrimOrNull(request.CorrectiveActionRef) ?? entity.CorrectiveActionRef,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("resolve", "defect", entity.Id, actorUserId, new { entity.Code, Status = "resolved" }, request.ResolutionSummary.Trim());
        return await BuildDefectDetailAsync(entity.Id, cancellationToken);
    }

    public async Task<DefectCommandResult<DefectDetailResponse>> CloseDefectAsync(Guid defectId, CloseDefectRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Defects.SingleOrDefaultAsync(x => x.Id == defectId, cancellationToken);
        if (entity is null)
        {
            return NotFound<DefectDetailResponse>(ApiErrorCodes.DefectNotFound, "Defect not found.");
        }

        if (!IsValidTransition(entity.Status, "closed", DefectStatuses))
        {
            return Validation<DefectDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Defect cannot move to closed.");
        }

        var resolutionSummary = TrimOrNull(request.ResolutionSummary) ?? entity.ResolutionSummary;
        if (string.IsNullOrWhiteSpace(resolutionSummary))
        {
            return Validation<DefectDetailResponse>(ApiErrorCodes.DefectResolutionRequired, "Resolution summary is required.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Status = "closed",
            ResolutionSummary = resolutionSummary,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("close", "defect", entity.Id, actorUserId, new { entity.Code, Status = "closed" }, resolutionSummary);
        return await BuildDefectDetailAsync(entity.Id, cancellationToken);
    }

    public async Task<DefectCommandResult<NonConformanceCommandResponse>> CreateNonConformanceAsync(CreateNonConformanceRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Projects.AnyAsync(x => x.Id == request.ProjectId, cancellationToken))
        {
            return NotFound<NonConformanceCommandResponse>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        var code = request.Code.Trim().ToUpperInvariant();
        if (await dbContext.NonConformances.AnyAsync(x => x.ProjectId == request.ProjectId && x.Code == code, cancellationToken))
        {
            return Conflict<NonConformanceCommandResponse>(ApiErrorCodes.NonConformanceCodeDuplicate, "Non-conformance code already exists.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new NonConformanceEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Code = code,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            SourceType = request.SourceType.Trim().ToLowerInvariant(),
            OwnerUserId = request.OwnerUserId.Trim(),
            CorrectiveActionRef = TrimOrNull(request.CorrectiveActionRef),
            RootCause = TrimOrNull(request.RootCause),
            LinkedFindingRefsJson = WriteArray(request.LinkedFindingRefs),
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.NonConformances.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("create", "non_conformance", entity.Id, actorUserId, new { entity.Code, entity.Status });
        return Success(new NonConformanceCommandResponse(entity.Id, entity.Code, entity.Status));
    }

    public async Task<DefectCommandResult<NonConformanceDetailResponse>> UpdateNonConformanceAsync(Guid nonConformanceId, UpdateNonConformanceRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.NonConformances.SingleOrDefaultAsync(x => x.Id == nonConformanceId, cancellationToken);
        if (entity is null)
        {
            return NotFound<NonConformanceDetailResponse>(ApiErrorCodes.NonConformanceNotFound, "Non-conformance not found.");
        }

        var nextStatus = request.Status.Trim().ToLowerInvariant();
        if (!NonConformanceStatuses.Contains(nextStatus) || !IsValidTransition(entity.Status, nextStatus, NonConformanceStatuses))
        {
            return Validation<NonConformanceDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Non-conformance transition is invalid.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            SourceType = request.SourceType.Trim().ToLowerInvariant(),
            OwnerUserId = request.OwnerUserId.Trim(),
            CorrectiveActionRef = TrimOrNull(request.CorrectiveActionRef),
            RootCause = TrimOrNull(request.RootCause),
            ResolutionSummary = TrimOrNull(request.ResolutionSummary),
            AcceptedDisposition = TrimOrNull(request.AcceptedDisposition),
            LinkedFindingRefsJson = WriteArray(request.LinkedFindingRefs),
            Status = nextStatus,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("update", "non_conformance", entity.Id, actorUserId, new { entity.Code, Status = nextStatus });
        return await BuildNonConformanceDetailAsync(entity.Id, cancellationToken);
    }

    public async Task<DefectCommandResult<NonConformanceDetailResponse>> CloseNonConformanceAsync(Guid nonConformanceId, CloseNonConformanceRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.NonConformances.SingleOrDefaultAsync(x => x.Id == nonConformanceId, cancellationToken);
        if (entity is null)
        {
            return NotFound<NonConformanceDetailResponse>(ApiErrorCodes.NonConformanceNotFound, "Non-conformance not found.");
        }

        if (!IsValidTransition(entity.Status, "closed", NonConformanceStatuses))
        {
            return Validation<NonConformanceDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Non-conformance cannot move to closed.");
        }

        var correctiveActionRef = TrimOrNull(request.CorrectiveActionRef) ?? entity.CorrectiveActionRef;
        var acceptedDisposition = TrimOrNull(request.AcceptedDisposition) ?? entity.AcceptedDisposition;
        if (string.IsNullOrWhiteSpace(correctiveActionRef) && string.IsNullOrWhiteSpace(acceptedDisposition))
        {
            return Validation<NonConformanceDetailResponse>(ApiErrorCodes.NonConformanceCorrectiveActionRequired, "Corrective action reference or accepted disposition is required.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Status = "closed",
            CorrectiveActionRef = correctiveActionRef,
            AcceptedDisposition = acceptedDisposition,
            ResolutionSummary = TrimOrNull(request.ResolutionSummary) ?? entity.ResolutionSummary,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("close", "non_conformance", entity.Id, actorUserId, new { entity.Code, Status = "closed" }, acceptedDisposition ?? correctiveActionRef);
        return await BuildNonConformanceDetailAsync(entity.Id, cancellationToken);
    }

    private async Task<DefectCommandResult<DefectDetailResponse>> BuildDefectDetailAsync(Guid defectId, CancellationToken cancellationToken)
    {
        var detail = await queries.GetDefectAsync(defectId, cancellationToken);
        return detail is null
            ? NotFound<DefectDetailResponse>(ApiErrorCodes.DefectNotFound, "Defect not found.")
            : Success(detail);
    }

    private async Task<DefectCommandResult<NonConformanceDetailResponse>> BuildNonConformanceDetailAsync(Guid nonConformanceId, CancellationToken cancellationToken)
    {
        var detail = await queries.GetNonConformanceAsync(nonConformanceId, cancellationToken);
        return detail is null
            ? NotFound<NonConformanceDetailResponse>(ApiErrorCodes.NonConformanceNotFound, "Non-conformance not found.")
            : Success(detail);
    }

    private void AppendAudit(string action, string entityType, Guid entityId, string? actorUserId, object? metadata, string? reason = null) =>
        auditLogWriter.Append(new AuditLogEntry("defects", action, entityType, entityId.ToString(), StatusCode: 200, Reason: reason, ActorUserId: actorUserId, Metadata: metadata, Audience: LogAudience.AuditOnly));

    private static bool IsValidTransition(string currentStatus, string nextStatus, IReadOnlyList<string> statuses)
    {
        if (string.Equals(currentStatus, nextStatus, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var order = statuses.ToArray();
        var currentIndex = Array.IndexOf(order, currentStatus);
        var nextIndex = Array.IndexOf(order, nextStatus);
        return currentIndex >= 0 && nextIndex == currentIndex + 1;
    }

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string? WriteArray(IReadOnlyList<string>? values) => values is null || values.Count == 0 ? null : JsonSerializer.Serialize(values.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray());

    private static DefectCommandResult<T> Success<T>(T value) => new(DefectCommandStatus.Success, value);
    private static DefectCommandResult<T> NotFound<T>(string errorCode, string message) => new(DefectCommandStatus.NotFound, default, errorCode, message);
    private static DefectCommandResult<T> Validation<T>(string errorCode, string message) => new(DefectCommandStatus.ValidationError, default, errorCode, message);
    private static DefectCommandResult<T> Conflict<T>(string errorCode, string message) => new(DefectCommandStatus.Conflict, default, errorCode, message);
}
