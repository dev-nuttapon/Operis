using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Exceptions.Contracts;
using Operis_API.Modules.Exceptions.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Exceptions.Application;

public sealed class ExceptionCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IExceptionQueries queries) : IExceptionCommands
{
    private static readonly string[] WaiverStates = ["draft", "submitted", "approved", "rejected", "expired", "closed"];

    public async Task<ExceptionCommandResult<WaiverDetailResponse>> CreateWaiverAsync(CreateWaiverRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var validation = await ValidateCreateOrUpdateAsync(request.ProjectId, request.ProcessArea, request.ScopeSummary, request.RequestedByUserId, request.Justification, request.EffectiveFrom, request.ExpiresAt, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var waiverCode = request.WaiverCode.Trim().ToUpperInvariant();
        if (await dbContext.Waivers.AnyAsync(x => x.WaiverCode == waiverCode, cancellationToken))
        {
            return Conflict<WaiverDetailResponse>(ApiErrorCodes.WaiverCodeDuplicate, "Waiver code already exists.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new WaiverEntity
        {
            Id = Guid.NewGuid(),
            WaiverCode = waiverCode,
            ProjectId = request.ProjectId,
            ProcessArea = request.ProcessArea.Trim().ToLowerInvariant(),
            ScopeSummary = request.ScopeSummary.Trim(),
            RequestedByUserId = request.RequestedByUserId.Trim(),
            Justification = request.Justification.Trim(),
            EffectiveFrom = request.EffectiveFrom!.Value,
            ExpiresAt = request.ExpiresAt!.Value,
            Status = "draft",
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Waivers.Add(entity);
        ApplyCompensatingControls(entity.Id, request.CompensatingControls, now);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("create", "waiver", entity.Id, StatusCodes.Status201Created, new { entity.WaiverCode, entity.Status });
        return Success((await queries.GetWaiverAsync(entity.Id, cancellationToken))!);
    }

    public async Task<ExceptionCommandResult<WaiverDetailResponse>> UpdateWaiverAsync(Guid waiverId, UpdateWaiverRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Waivers.SingleOrDefaultAsync(x => x.Id == waiverId, cancellationToken);
        if (entity is null)
        {
            return NotFound<WaiverDetailResponse>(ApiErrorCodes.WaiverNotFound, "Waiver not found.");
        }

        var validation = await ValidateCreateOrUpdateAsync(request.ProjectId, request.ProcessArea, request.ScopeSummary, request.RequestedByUserId, request.Justification, request.EffectiveFrom, request.ExpiresAt, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var now = DateTimeOffset.UtcNow;
        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            ProjectId = request.ProjectId,
            ProcessArea = request.ProcessArea.Trim().ToLowerInvariant(),
            ScopeSummary = request.ScopeSummary.Trim(),
            RequestedByUserId = request.RequestedByUserId.Trim(),
            Justification = request.Justification.Trim(),
            EffectiveFrom = request.EffectiveFrom!.Value,
            ExpiresAt = request.ExpiresAt!.Value,
            UpdatedAt = now
        });

        var existingControls = await dbContext.CompensatingControls.Where(x => x.WaiverId == waiverId).ToListAsync(cancellationToken);
        dbContext.CompensatingControls.RemoveRange(existingControls);
        ApplyCompensatingControls(waiverId, request.CompensatingControls, now);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("update", "waiver", waiverId, StatusCodes.Status200OK, new { Status = entity.Status });
        return Success((await queries.GetWaiverAsync(waiverId, cancellationToken))!);
    }

    public async Task<ExceptionCommandResult<WaiverDetailResponse>> TransitionWaiverAsync(Guid waiverId, TransitionWaiverRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Waivers.SingleOrDefaultAsync(x => x.Id == waiverId, cancellationToken);
        if (entity is null)
        {
            return NotFound<WaiverDetailResponse>(ApiErrorCodes.WaiverNotFound, "Waiver not found.");
        }

        var nextStatus = request.TargetStatus.Trim().ToLowerInvariant();
        if (!WaiverStates.Contains(nextStatus) || !IsValidTransition(entity.Status, nextStatus))
        {
            return Validation<WaiverDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Waiver transition is invalid.");
        }

        if ((nextStatus == "approved" || nextStatus == "submitted") && entity.ExpiresAt < entity.EffectiveFrom)
        {
            return Validation<WaiverDetailResponse>(ApiErrorCodes.WaiverExpiryRequired, "Waiver expiry date is invalid.");
        }

        if (nextStatus == "approved")
        {
            var hasControls = await dbContext.CompensatingControls.AnyAsync(x => x.WaiverId == waiverId, cancellationToken);
            if (!hasControls)
            {
                return Validation<WaiverDetailResponse>(ApiErrorCodes.WaiverCompensatingControlRequired, "Compensating control is required before approving a waiver.");
            }
        }

        if ((nextStatus == "rejected" || nextStatus == "closed") && string.IsNullOrWhiteSpace(request.Reason))
        {
            return Validation<WaiverDetailResponse>(ApiErrorCodes.ReasonRequired, "Transition reason is required.");
        }

        var now = DateTimeOffset.UtcNow;
        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Status = nextStatus,
            DecisionReason = nextStatus is "approved" or "rejected" ? TrimOrNull(request.Reason, 2000) : entity.DecisionReason,
            DecisionByUserId = nextStatus is "approved" or "rejected" ? actorUserId : entity.DecisionByUserId,
            DecisionAt = nextStatus is "approved" or "rejected" ? now : entity.DecisionAt,
            ClosureReason = nextStatus == "closed" ? TrimOrNull(request.Reason, 2000) : entity.ClosureReason,
            UpdatedAt = now
        });

        dbContext.WaiverReviews.Add(new WaiverReviewEntity
        {
            Id = Guid.NewGuid(),
            WaiverId = waiverId,
            ReviewType = nextStatus == "expired" ? "expiry" : "workflow",
            OutcomeStatus = nextStatus,
            ReviewerUserId = actorUserId ?? "system",
            Notes = TrimOrNull(request.Reason, 2000),
            ReviewedAt = now,
            NextReviewAt = request.NextReviewAt,
            CreatedAt = now
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("transition", "waiver", waiverId, StatusCodes.Status200OK, new { From = entity.Status, To = nextStatus }, request.Reason);
        return Success((await queries.GetWaiverAsync(waiverId, cancellationToken))!);
    }

    private async Task<ExceptionCommandResult<WaiverDetailResponse>?> ValidateCreateOrUpdateAsync(Guid? projectId, string processArea, string scopeSummary, string requestedByUserId, string justification, DateOnly? effectiveFrom, DateOnly? expiresAt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(processArea) || string.IsNullOrWhiteSpace(scopeSummary))
        {
            return Validation<WaiverDetailResponse>(ApiErrorCodes.WaiverScopeRequired, "Waiver process area and scope summary are required.");
        }

        if (string.IsNullOrWhiteSpace(requestedByUserId) || string.IsNullOrWhiteSpace(justification))
        {
            return Validation<WaiverDetailResponse>(ApiErrorCodes.RequestValidationFailed, "Requester and justification are required.");
        }

        if (!effectiveFrom.HasValue || !expiresAt.HasValue || expiresAt.Value < effectiveFrom.Value)
        {
            return Validation<WaiverDetailResponse>(ApiErrorCodes.WaiverExpiryRequired, "Waiver effective and expiry dates are required.");
        }

        if (projectId.HasValue && !await dbContext.Projects.AnyAsync(x => x.Id == projectId.Value, cancellationToken))
        {
            return NotFound<WaiverDetailResponse>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        return null;
    }

    private void ApplyCompensatingControls(Guid waiverId, IReadOnlyList<CompensatingControlInput>? inputs, DateTimeOffset now)
    {
        foreach (var input in inputs ?? [])
        {
            if (string.IsNullOrWhiteSpace(input.Description))
            {
                continue;
            }

            dbContext.CompensatingControls.Add(new CompensatingControlEntity
            {
                Id = Guid.NewGuid(),
                WaiverId = waiverId,
                ControlCode = string.IsNullOrWhiteSpace(input.ControlCode) ? $"CTRL-{Guid.NewGuid():N}"[..13].ToUpperInvariant() : input.ControlCode.Trim().ToUpperInvariant(),
                Description = input.Description.Trim(),
                OwnerUserId = input.OwnerUserId.Trim(),
                Status = string.IsNullOrWhiteSpace(input.Status) ? "active" : input.Status.Trim().ToLowerInvariant(),
                CreatedAt = now,
                UpdatedAt = now
            });
        }
    }

    private void AppendAudit(string action, string entityType, Guid entityId, int statusCode, object? metadata, string? reason = null) =>
        auditLogWriter.Append(new AuditLogEntry("exceptions", action, entityType, entityId.ToString(), StatusCode: statusCode, Reason: reason, Metadata: metadata, Audience: LogAudience.AuditOnly));

    private static bool IsValidTransition(string current, string next)
    {
        current = current.Trim().ToLowerInvariant();
        next = next.Trim().ToLowerInvariant();
        return current switch
        {
            "draft" => next == "submitted",
            "submitted" => next is "approved" or "rejected",
            "approved" => next is "expired" or "closed",
            "rejected" => next == "closed",
            "expired" => next == "closed",
            _ => false
        };
    }

    private static string? TrimOrNull(string? value, int maxLength = 2000) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().Length <= maxLength ? value.Trim() : value.Trim()[..maxLength];

    private static ExceptionCommandResult<T> Success<T>(T value) => new(ExceptionCommandStatus.Success, value);
    private static ExceptionCommandResult<T> Validation<T>(string code, string message) => new(ExceptionCommandStatus.ValidationError, default, message, code);
    private static ExceptionCommandResult<T> NotFound<T>(string code, string message) => new(ExceptionCommandStatus.NotFound, default, message, code);
    private static ExceptionCommandResult<T> Conflict<T>(string code, string message) => new(ExceptionCommandStatus.Conflict, default, message, code);
}
