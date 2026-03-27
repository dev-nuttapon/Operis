using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Releases.Contracts;
using Operis_API.Modules.Releases.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Releases.Application;

public sealed class ReleaseCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IReleaseQueries queries) : IReleaseCommands
{
    private static readonly string[] ReleaseStatuses = ["draft", "approved", "released", "archived"];
    private static readonly string[] ChecklistStatuses = ["draft", "reviewed", "approved", "executed"];

    public async Task<ReleaseCommandResult<ReleaseCommandResponse>> CreateReleaseAsync(CreateReleaseRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Projects.AnyAsync(x => x.Id == request.ProjectId, cancellationToken))
        {
            return NotFound<ReleaseCommandResponse>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        var releaseCode = request.ReleaseCode.Trim().ToUpperInvariant();
        if (await dbContext.Releases.AnyAsync(x => x.ProjectId == request.ProjectId && x.ReleaseCode == releaseCode, cancellationToken))
        {
            return Conflict<ReleaseCommandResponse>(ApiErrorCodes.ReleaseCodeDuplicate, "Release code already exists for this project.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new ReleaseEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            ReleaseCode = releaseCode,
            Title = request.Title.Trim(),
            PlannedAt = request.PlannedAt,
            Status = "draft",
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Releases.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("create", "release", entity.Id, actorUserId, new { entity.ReleaseCode, entity.Status });
        return Success(new ReleaseCommandResponse(entity.Id, entity.ReleaseCode, entity.Status));
    }

    public async Task<ReleaseCommandResult<ReleaseDetailResponse>> UpdateReleaseAsync(Guid releaseId, UpdateReleaseRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Releases.SingleOrDefaultAsync(x => x.Id == releaseId, cancellationToken);
        if (entity is null)
        {
            return NotFound<ReleaseDetailResponse>(ApiErrorCodes.ReleaseNotFound, "Release not found.");
        }

        if (entity.Status is "released" or "archived")
        {
            return Validation<ReleaseDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Released or archived releases cannot be edited.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Title = request.Title.Trim(),
            PlannedAt = request.PlannedAt,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("update", "release", entity.Id, actorUserId, new { entity.ReleaseCode });
        return await BuildReleaseDetailAsync(entity.Id, cancellationToken);
    }

    public async Task<ReleaseCommandResult<ReleaseDetailResponse>> ApproveReleaseAsync(Guid releaseId, ApproveReleaseRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Releases.SingleOrDefaultAsync(x => x.Id == releaseId, cancellationToken);
        if (entity is null)
        {
            return NotFound<ReleaseDetailResponse>(ApiErrorCodes.ReleaseNotFound, "Release not found.");
        }

        if (!IsValidTransition(entity.Status, "approved", ReleaseStatuses))
        {
            return Validation<ReleaseDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Release cannot move to approved.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Status = "approved",
            ApprovedAt = DateTimeOffset.UtcNow,
            ApprovedByUserId = actorUserId,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("approve", "release", entity.Id, actorUserId, new { entity.ReleaseCode, Status = "approved" }, request.Reason);
        return await BuildReleaseDetailAsync(entity.Id, cancellationToken);
    }

    public async Task<ReleaseCommandResult<ReleaseDetailResponse>> ExecuteReleaseAsync(Guid releaseId, ExecuteReleaseRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Releases.SingleOrDefaultAsync(x => x.Id == releaseId, cancellationToken);
        if (entity is null)
        {
            return NotFound<ReleaseDetailResponse>(ApiErrorCodes.ReleaseNotFound, "Release not found.");
        }

        if (!string.Equals(entity.Status, "approved", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<ReleaseDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Only approved releases can be executed.");
        }

        var checklistItems = await dbContext.DeploymentChecklists
            .Where(x => x.ReleaseId == releaseId)
            .ToListAsync(cancellationToken);

        if (checklistItems.Count == 0 || checklistItems.Any(x => !string.Equals(x.Status, "executed", StringComparison.OrdinalIgnoreCase)))
        {
            return Validation<ReleaseDetailResponse>(ApiErrorCodes.ReleaseChecklistIncomplete, "Release checklist is incomplete.");
        }

        var gate = await dbContext.QualityGateResults.AsNoTracking()
            .Where(x => x.ProjectId == entity.ProjectId && x.GateType == "release_readiness")
            .OrderByDescending(x => x.EvaluatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (gate is null || gate.Result == "failed")
        {
            if (string.IsNullOrWhiteSpace(request.OverrideReason))
            {
                return Validation<ReleaseDetailResponse>(ApiErrorCodes.ReleaseQualityGateFailed, "Release quality gate failed.");
            }
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Status = "released",
            ReleasedAt = DateTimeOffset.UtcNow,
            QualityGateResultId = gate?.Id,
            QualityGateOverrideReason = string.IsNullOrWhiteSpace(request.OverrideReason) ? null : request.OverrideReason.Trim(),
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("release", "release", entity.Id, actorUserId, new { entity.ReleaseCode, Status = "released", QualityGate = gate?.Result }, request.OverrideReason);
        return await BuildReleaseDetailAsync(entity.Id, cancellationToken);
    }

    public async Task<ReleaseCommandResult<DeploymentChecklistItem>> CreateDeploymentChecklistAsync(CreateDeploymentChecklistRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var release = await dbContext.Releases.SingleOrDefaultAsync(x => x.Id == request.ReleaseId, cancellationToken);
        if (release is null)
        {
            return NotFound<DeploymentChecklistItem>(ApiErrorCodes.ReleaseNotFound, "Release not found.");
        }

        var status = NormalizeChecklistStatus(request.Status);
        if (!ChecklistStatuses.Contains(status))
        {
            return Validation<DeploymentChecklistItem>(ApiErrorCodes.InvalidWorkflowTransition, "Checklist status is invalid.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new DeploymentChecklistEntity
        {
            Id = Guid.NewGuid(),
            ReleaseId = request.ReleaseId,
            ChecklistItem = request.ChecklistItem.Trim(),
            OwnerUserId = request.OwnerUserId.Trim(),
            Status = status,
            CompletedAt = status == "executed" ? now : null,
            EvidenceRef = TrimOrNull(request.EvidenceRef),
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.DeploymentChecklists.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("create", "deployment_checklist", entity.Id, actorUserId, new { release.ReleaseCode, entity.Status });
        return Success(new DeploymentChecklistItem(entity.Id, entity.ReleaseId, release.ReleaseCode, entity.ChecklistItem, entity.OwnerUserId, entity.Status, entity.CompletedAt, entity.EvidenceRef, entity.UpdatedAt));
    }

    public async Task<ReleaseCommandResult<DeploymentChecklistItem>> UpdateDeploymentChecklistAsync(Guid checklistId, UpdateDeploymentChecklistRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.DeploymentChecklists.SingleOrDefaultAsync(x => x.Id == checklistId, cancellationToken);
        if (entity is null)
        {
            return NotFound<DeploymentChecklistItem>(ApiErrorCodes.DeploymentChecklistNotFound, "Deployment checklist item not found.");
        }

        var release = await dbContext.Releases.AsNoTracking().SingleAsync(x => x.Id == entity.ReleaseId, cancellationToken);
        var status = NormalizeChecklistStatus(request.Status);
        if (!ChecklistStatuses.Contains(status) || !IsValidTransition(entity.Status, status, ChecklistStatuses))
        {
            return Validation<DeploymentChecklistItem>(ApiErrorCodes.InvalidWorkflowTransition, "Checklist transition is invalid.");
        }

        DateTimeOffset? completedAt = status == "executed" ? request.CompletedAt ?? entity.CompletedAt ?? DateTimeOffset.UtcNow : null;
        var updatedAt = DateTimeOffset.UtcNow;
        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            ChecklistItem = request.ChecklistItem.Trim(),
            OwnerUserId = request.OwnerUserId.Trim(),
            Status = status,
            CompletedAt = completedAt,
            EvidenceRef = TrimOrNull(request.EvidenceRef),
            UpdatedAt = updatedAt
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("execute", "deployment_checklist", entity.Id, actorUserId, new { release.ReleaseCode, Status = status }, status == "executed" ? TrimOrNull(request.EvidenceRef) : null);
        return Success(new DeploymentChecklistItem(entity.Id, entity.ReleaseId, release.ReleaseCode, request.ChecklistItem.Trim(), request.OwnerUserId.Trim(), status, completedAt, TrimOrNull(request.EvidenceRef), updatedAt));
    }

    public async Task<ReleaseCommandResult<ReleaseNoteItem>> CreateReleaseNoteAsync(CreateReleaseNoteRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var release = await dbContext.Releases.SingleOrDefaultAsync(x => x.Id == request.ReleaseId, cancellationToken);
        if (release is null)
        {
            return NotFound<ReleaseNoteItem>(ApiErrorCodes.ReleaseNotFound, "Release not found.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new ReleaseNoteEntity
        {
            Id = Guid.NewGuid(),
            ReleaseId = request.ReleaseId,
            Summary = request.Summary.Trim(),
            IncludedChanges = request.IncludedChanges.Trim(),
            KnownIssues = TrimOrNull(request.KnownIssues),
            Status = "draft",
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.ReleaseNotes.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("create", "release_note", entity.Id, actorUserId, new { release.ReleaseCode, entity.Status });
        return Success(new ReleaseNoteItem(entity.Id, entity.ReleaseId, release.ReleaseCode, entity.Summary, entity.IncludedChanges, entity.KnownIssues, entity.Status, entity.PublishedAt, entity.UpdatedAt));
    }

    public async Task<ReleaseCommandResult<ReleaseNotePublishResponse>> PublishReleaseNoteAsync(Guid releaseNoteId, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ReleaseNotes.SingleOrDefaultAsync(x => x.Id == releaseNoteId, cancellationToken);
        if (entity is null)
        {
            return NotFound<ReleaseNotePublishResponse>(ApiErrorCodes.ReleaseNoteNotFound, "Release note not found.");
        }

        var release = await dbContext.Releases.SingleAsync(x => x.Id == entity.ReleaseId, cancellationToken);
        if (release.Status is not ("approved" or "released"))
        {
            return Validation<ReleaseNotePublishResponse>(ApiErrorCodes.ReleaseNotesReleaseRequired, "Release notes require an approved release.");
        }

        var publishedAt = DateTimeOffset.UtcNow;
        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Status = "published",
            PublishedAt = publishedAt,
            UpdatedAt = publishedAt
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("publish", "release_note", entity.Id, actorUserId, new { release.ReleaseCode, Status = "published" });
        return Success(new ReleaseNotePublishResponse(entity.Id, entity.ReleaseId, "published", publishedAt));
    }

    private async Task<ReleaseCommandResult<ReleaseDetailResponse>> BuildReleaseDetailAsync(Guid releaseId, CancellationToken cancellationToken)
    {
        var detail = await queries.GetReleaseAsync(releaseId, cancellationToken);
        return detail is null
            ? NotFound<ReleaseDetailResponse>(ApiErrorCodes.ReleaseNotFound, "Release not found.")
            : Success(detail);
    }

    private void AppendAudit(string action, string entityType, Guid entityId, string? actorUserId, object? metadata, string? reason = null) =>
        auditLogWriter.Append(new AuditLogEntry("releases", action, entityType, entityId.ToString(), StatusCode: 200, Reason: reason, ActorUserId: actorUserId, Metadata: metadata, Audience: LogAudience.AuditOnly));

    private static bool IsValidTransition(string currentStatus, string nextStatus, IReadOnlyList<string> statuses)
    {
        if (string.Equals(currentStatus, nextStatus, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var currentIndex = Array.IndexOf(statuses.ToArray(), currentStatus);
        var nextIndex = Array.IndexOf(statuses.ToArray(), nextStatus);
        return currentIndex >= 0 && nextIndex == currentIndex + 1;
    }

    private static string NormalizeChecklistStatus(string status) => status.Trim().ToLowerInvariant();
    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ReleaseCommandResult<T> Success<T>(T value) => new(ReleaseCommandStatus.Success, value);
    private static ReleaseCommandResult<T> NotFound<T>(string errorCode, string message) => new(ReleaseCommandStatus.NotFound, default, errorCode, message);
    private static ReleaseCommandResult<T> Validation<T>(string errorCode, string message) => new(ReleaseCommandStatus.ValidationError, default, errorCode, message);
    private static ReleaseCommandResult<T> Conflict<T>(string errorCode, string message) => new(ReleaseCommandStatus.Conflict, default, errorCode, message);
}
