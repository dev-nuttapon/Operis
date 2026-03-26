using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Audits.Application;
using Operis_API.Modules.Meetings.Contracts;
using Operis_API.Modules.Meetings.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Meetings.Application;

public sealed class MeetingCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IBusinessAuditEventWriter businessAuditEventWriter,
    IMeetingQueries queries) : IMeetingCommands
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly string[] MeetingStatuses = ["draft", "approved", "archived"];
    private static readonly string[] MinuteStatuses = ["draft", "reviewed", "approved", "archived"];
    private static readonly string[] DecisionStatuses = ["proposed", "approved", "applied", "archived"];

    public async Task<MeetingCommandResult<MeetingDetailResponse>> CreateMeetingAsync(CreateMeetingRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var validationError = await ValidateMeetingRequestAsync(request.ProjectId, request.MeetingType, request.Title, request.FacilitatorUserId, request.Classification, request.IsRestricted, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var now = DateTimeOffset.UtcNow;
        var meetingId = Guid.NewGuid();
        var meeting = new MeetingRecordEntity
        {
            Id = meetingId,
            ProjectId = request.ProjectId,
            MeetingType = request.MeetingType.Trim().ToLowerInvariant(),
            Title = request.Title.Trim(),
            MeetingAt = request.MeetingAt,
            FacilitatorUserId = request.FacilitatorUserId.Trim(),
            Status = "draft",
            Agenda = TrimOrNull(request.Agenda),
            DiscussionSummary = TrimOrNull(request.DiscussionSummary),
            IsRestricted = request.IsRestricted,
            Classification = request.IsRestricted ? request.Classification?.Trim() : null,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Add(meeting);
        dbContext.Add(new MeetingMinutesEntity
        {
            Id = Guid.NewGuid(),
            MeetingRecordId = meetingId,
            Status = "draft",
            UpdatedAt = now
        });
        UpsertAttendees(meetingId, request.AttendeeUserIds);
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("create", "meeting", meetingId, StatusCodes.Status201Created, new { meeting.MeetingType, meeting.Status }, cancellationToken);
        await AppendBusinessEventAsync("meeting_created", "meeting", meetingId, actorUserId, "Created meeting", null, null, cancellationToken);
        return await SuccessMeetingAsync(meetingId, cancellationToken);
    }

    public async Task<MeetingCommandResult<MeetingDetailResponse>> UpdateMeetingAsync(Guid meetingId, UpdateMeetingRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var meeting = await dbContext.Set<MeetingRecordEntity>().SingleOrDefaultAsync(x => x.Id == meetingId, cancellationToken);
        if (meeting is null)
        {
            return NotFound<MeetingDetailResponse>(ApiErrorCodes.MeetingNotFound, "Meeting not found.");
        }

        if (string.Equals(meeting.Status, "archived", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<MeetingDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Archived meetings cannot be updated.");
        }

        var validationError = await ValidateMeetingRequestAsync(meeting.ProjectId, request.MeetingType, request.Title, request.FacilitatorUserId, request.Classification, request.IsRestricted, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        dbContext.Entry(meeting).CurrentValues.SetValues(meeting with
        {
            MeetingType = request.MeetingType.Trim().ToLowerInvariant(),
            Title = request.Title.Trim(),
            MeetingAt = request.MeetingAt,
            FacilitatorUserId = request.FacilitatorUserId.Trim(),
            Agenda = TrimOrNull(request.Agenda),
            DiscussionSummary = TrimOrNull(request.DiscussionSummary),
            IsRestricted = request.IsRestricted,
            Classification = request.IsRestricted ? request.Classification?.Trim() : null,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        ReplaceAttendees(meetingId, request.AttendeeUserIds);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("update", "meeting", meetingId, StatusCodes.Status200OK, null, cancellationToken);
        await AppendBusinessEventAsync("meeting_updated", "meeting", meetingId, actorUserId, "Updated meeting", null, null, cancellationToken);
        return await SuccessMeetingAsync(meetingId, cancellationToken);
    }

    public async Task<MeetingCommandResult<MeetingDetailResponse>> ApproveMeetingAsync(Guid meetingId, MeetingApprovalRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var meeting = await dbContext.Set<MeetingRecordEntity>().SingleOrDefaultAsync(x => x.Id == meetingId, cancellationToken);
        if (meeting is null)
        {
            return NotFound<MeetingDetailResponse>(ApiErrorCodes.MeetingNotFound, "Meeting not found.");
        }

        if (!string.Equals(meeting.Status, "draft", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<MeetingDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Only draft meetings can be approved.");
        }

        dbContext.Entry(meeting).CurrentValues.SetValues(meeting with
        {
            Status = "approved",
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("approve", "meeting", meetingId, StatusCodes.Status200OK, null, cancellationToken, TrimOrNull(request.Reason));
        await AppendBusinessEventAsync("meeting_approved", "meeting", meetingId, actorUserId, "Approved meeting", TrimOrNull(request.Reason), null, cancellationToken);
        return await SuccessMeetingAsync(meetingId, cancellationToken);
    }

    public async Task<MeetingCommandResult<MeetingMinutesResponse>> UpdateMeetingMinutesAsync(Guid meetingId, UpdateMeetingMinutesRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var meeting = await dbContext.Set<MeetingRecordEntity>().SingleOrDefaultAsync(x => x.Id == meetingId, cancellationToken);
        if (meeting is null)
        {
            return NotFound<MeetingMinutesResponse>(ApiErrorCodes.MeetingNotFound, "Meeting not found.");
        }

        var minutes = await dbContext.Set<MeetingMinutesEntity>().SingleAsync(x => x.MeetingRecordId == meetingId, cancellationToken);
        var nextStatus = string.IsNullOrWhiteSpace(request.Status) ? minutes.Status : request.Status.Trim().ToLowerInvariant();
        if (!MinuteStatuses.Contains(nextStatus, StringComparer.OrdinalIgnoreCase))
        {
            return Validation<MeetingMinutesResponse>(ApiErrorCodes.RequestValidationFailed, "Meeting minutes status is invalid.");
        }

        if (request.AttendeeUserIds is { Count: > 0 })
        {
            ReplaceAttendees(meetingId, request.AttendeeUserIds);
        }

        var attendeeCount = await dbContext.Set<MeetingAttendeeEntity>().CountAsync(x => x.MeetingRecordId == meetingId, cancellationToken);
        var summary = TrimOrNull(request.Summary) ?? minutes.Summary;

        if (string.Equals(nextStatus, "approved", StringComparison.OrdinalIgnoreCase))
        {
            if (attendeeCount == 0)
            {
                return Validation<MeetingMinutesResponse>(ApiErrorCodes.MeetingAttendeesRequired, "Meeting attendees are required before approving minutes.");
            }

            if (string.IsNullOrWhiteSpace(summary))
            {
                return Validation<MeetingMinutesResponse>(ApiErrorCodes.MeetingSummaryRequired, "Meeting summary is required before approving minutes.");
            }
        }

        dbContext.Entry(minutes).CurrentValues.SetValues(minutes with
        {
            Summary = summary,
            DecisionsSummary = TrimOrNull(request.DecisionsSummary) ?? minutes.DecisionsSummary,
            ActionsSummary = TrimOrNull(request.ActionsSummary) ?? minutes.ActionsSummary,
            Status = nextStatus,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("update_minutes", "meeting_minutes", meetingId, StatusCodes.Status200OK, new { Status = nextStatus }, cancellationToken);
        await AppendBusinessEventAsync("meeting_minutes_updated", "meeting_minutes", meetingId, actorUserId, "Updated meeting minutes", null, new { Status = nextStatus }, cancellationToken);
        return await SuccessMinutesAsync(meetingId, cancellationToken);
    }

    public async Task<MeetingCommandResult<DecisionDetailResponse>> CreateDecisionAsync(CreateDecisionRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var validationError = await ValidateDecisionRequestAsync(request.ProjectId, request.MeetingId, request.Code, request.Title, request.DecisionType, request.Rationale, request.IsRestricted, request.Classification, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new DecisionEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            MeetingId = request.MeetingId,
            Code = request.Code.Trim().ToUpperInvariant(),
            Title = request.Title.Trim(),
            DecisionType = request.DecisionType.Trim().ToLowerInvariant(),
            Rationale = request.Rationale.Trim(),
            AlternativesConsidered = TrimOrNull(request.AlternativesConsidered),
            ImpactedArtifactsJson = SerializeList(request.ImpactedArtifacts),
            Status = "proposed",
            IsRestricted = request.IsRestricted,
            Classification = request.IsRestricted ? request.Classification?.Trim() : null,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("create", "decision", entity.Id, StatusCodes.Status201Created, new { entity.Code, entity.Status }, cancellationToken);
        await AppendBusinessEventAsync("decision_created", "decision", entity.Id, actorUserId, "Created decision", null, null, cancellationToken);
        return await SuccessDecisionAsync(entity.Id, cancellationToken);
    }

    public async Task<MeetingCommandResult<DecisionDetailResponse>> UpdateDecisionAsync(Guid decisionId, UpdateDecisionRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var decision = await dbContext.Set<DecisionEntity>().SingleOrDefaultAsync(x => x.Id == decisionId, cancellationToken);
        if (decision is null)
        {
            return NotFound<DecisionDetailResponse>(ApiErrorCodes.DecisionNotFound, "Decision not found.");
        }

        if (string.Equals(decision.Status, "applied", StringComparison.OrdinalIgnoreCase)
            || string.Equals(decision.Status, "archived", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<DecisionDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Applied or archived decisions cannot be edited.");
        }

        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.DecisionType) || string.IsNullOrWhiteSpace(request.Rationale))
        {
            return Validation<DecisionDetailResponse>(ApiErrorCodes.RequestValidationFailed, "Decision title, type, and rationale are required.");
        }

        dbContext.Entry(decision).CurrentValues.SetValues(decision with
        {
            Title = request.Title.Trim(),
            DecisionType = request.DecisionType.Trim().ToLowerInvariant(),
            Rationale = request.Rationale.Trim(),
            AlternativesConsidered = TrimOrNull(request.AlternativesConsidered),
            ImpactedArtifactsJson = SerializeList(request.ImpactedArtifacts),
            IsRestricted = request.IsRestricted,
            Classification = request.IsRestricted ? request.Classification?.Trim() : null,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("update", "decision", decisionId, StatusCodes.Status200OK, null, cancellationToken);
        await AppendBusinessEventAsync("decision_updated", "decision", decisionId, actorUserId, "Updated decision", null, null, cancellationToken);
        return await SuccessDecisionAsync(decisionId, cancellationToken);
    }

    public async Task<MeetingCommandResult<DecisionDetailResponse>> ApproveDecisionAsync(Guid decisionId, DecisionTransitionRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var decision = await dbContext.Set<DecisionEntity>().SingleOrDefaultAsync(x => x.Id == decisionId, cancellationToken);
        if (decision is null)
        {
            return NotFound<DecisionDetailResponse>(ApiErrorCodes.DecisionNotFound, "Decision not found.");
        }

        if (!string.Equals(decision.Status, "proposed", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<DecisionDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Only proposed decisions can be approved.");
        }

        dbContext.Entry(decision).CurrentValues.SetValues(decision with
        {
            Status = "approved",
            ApprovedBy = actorUserId,
            ApprovedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("approve", "decision", decisionId, StatusCodes.Status200OK, null, cancellationToken, TrimOrNull(request.Reason));
        await AppendBusinessEventAsync("decision_approved", "decision", decisionId, actorUserId, "Approved decision", TrimOrNull(request.Reason), null, cancellationToken);
        return await SuccessDecisionAsync(decisionId, cancellationToken);
    }

    public async Task<MeetingCommandResult<DecisionDetailResponse>> ApplyDecisionAsync(Guid decisionId, DecisionTransitionRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var decision = await dbContext.Set<DecisionEntity>().SingleOrDefaultAsync(x => x.Id == decisionId, cancellationToken);
        if (decision is null)
        {
            return NotFound<DecisionDetailResponse>(ApiErrorCodes.DecisionNotFound, "Decision not found.");
        }

        if (!string.Equals(decision.Status, "approved", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<DecisionDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Only approved decisions can be applied.");
        }

        if (string.IsNullOrWhiteSpace(decision.Rationale))
        {
            return Validation<DecisionDetailResponse>(ApiErrorCodes.DecisionRationaleRequired, "Decision rationale is required before apply.");
        }

        dbContext.Entry(decision).CurrentValues.SetValues(decision with
        {
            Status = "applied",
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("apply", "decision", decisionId, StatusCodes.Status200OK, null, cancellationToken, TrimOrNull(request.Reason));
        await AppendBusinessEventAsync("decision_applied", "decision", decisionId, actorUserId, "Applied decision", TrimOrNull(request.Reason), null, cancellationToken);
        return await SuccessDecisionAsync(decisionId, cancellationToken);
    }

    private async Task<MeetingCommandResult<MeetingDetailResponse>?> ValidateMeetingRequestAsync(Guid projectId, string meetingType, string title, string facilitatorUserId, string? classification, bool isRestricted, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(meetingType) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(facilitatorUserId))
        {
            return Validation<MeetingDetailResponse>(ApiErrorCodes.RequestValidationFailed, "Project, meeting type, title, and facilitator are required.");
        }

        var projectExists = await dbContext.Projects.AsNoTracking().AnyAsync(x => x.Id == projectId, cancellationToken);
        if (!projectExists)
        {
            return NotFound<MeetingDetailResponse>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        if (isRestricted && string.IsNullOrWhiteSpace(classification))
        {
            return Validation<MeetingDetailResponse>(ApiErrorCodes.RestrictedClassificationRequired, "Restricted meetings require classification.");
        }

        return null;
    }

    private async Task<MeetingCommandResult<DecisionDetailResponse>?> ValidateDecisionRequestAsync(Guid projectId, Guid? meetingId, string code, string title, string decisionType, string rationale, bool isRestricted, string? classification, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(decisionType) || string.IsNullOrWhiteSpace(rationale))
        {
            return Validation<DecisionDetailResponse>(ApiErrorCodes.RequestValidationFailed, "Project, code, title, decision type, and rationale are required.");
        }

        var projectExists = await dbContext.Projects.AsNoTracking().AnyAsync(x => x.Id == projectId, cancellationToken);
        if (!projectExists)
        {
            return NotFound<DecisionDetailResponse>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        if (meetingId.HasValue)
        {
            var meeting = await dbContext.Set<MeetingRecordEntity>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == meetingId.Value, cancellationToken);
            if (meeting is null)
            {
                return NotFound<DecisionDetailResponse>(ApiErrorCodes.MeetingNotFound, "Linked meeting not found.");
            }

            if (meeting.ProjectId != projectId)
            {
                return Validation<DecisionDetailResponse>(ApiErrorCodes.RequestValidationFailed, "Meeting must belong to the same project.");
            }
        }

        var normalizedCode = code.Trim().ToUpperInvariant();
        var duplicate = await dbContext.Set<DecisionEntity>().AsNoTracking()
            .AnyAsync(x => x.ProjectId == projectId && x.Code == normalizedCode, cancellationToken);
        if (duplicate)
        {
            return Conflict<DecisionDetailResponse>(ApiErrorCodes.DecisionCodeDuplicate, "Decision code already exists in the project.");
        }

        if (isRestricted && string.IsNullOrWhiteSpace(classification))
        {
            return Validation<DecisionDetailResponse>(ApiErrorCodes.RestrictedClassificationRequired, "Restricted decisions require classification.");
        }

        return null;
    }

    private void UpsertAttendees(Guid meetingId, IReadOnlyList<string>? attendeeUserIds)
    {
        foreach (var userId in attendeeUserIds?.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase) ?? [])
        {
            dbContext.Add(new MeetingAttendeeEntity
            {
                Id = Guid.NewGuid(),
                MeetingRecordId = meetingId,
                UserId = userId.Trim(),
                AttendanceStatus = "confirmed"
            });
        }
    }

    private void ReplaceAttendees(Guid meetingId, IReadOnlyList<string>? attendeeUserIds)
    {
        var existing = dbContext.Set<MeetingAttendeeEntity>().Where(x => x.MeetingRecordId == meetingId).ToList();
        if (existing.Count > 0)
        {
            dbContext.RemoveRange(existing);
        }

        UpsertAttendees(meetingId, attendeeUserIds);
    }

    private async Task<MeetingCommandResult<MeetingDetailResponse>> SuccessMeetingAsync(Guid meetingId, CancellationToken cancellationToken)
    {
        var value = await queries.GetMeetingAsync(meetingId, true, cancellationToken);
        return new MeetingCommandResult<MeetingDetailResponse>(MeetingCommandStatus.Success, value);
    }

    private async Task<MeetingCommandResult<MeetingMinutesResponse>> SuccessMinutesAsync(Guid meetingId, CancellationToken cancellationToken)
    {
        var value = await queries.GetMeetingMinutesAsync(meetingId, true, cancellationToken);
        return new MeetingCommandResult<MeetingMinutesResponse>(MeetingCommandStatus.Success, value);
    }

    private async Task<MeetingCommandResult<DecisionDetailResponse>> SuccessDecisionAsync(Guid decisionId, CancellationToken cancellationToken)
    {
        var value = await queries.GetDecisionAsync(decisionId, true, cancellationToken);
        return new MeetingCommandResult<DecisionDetailResponse>(MeetingCommandStatus.Success, value);
    }

    private async Task AppendAuditAsync(string action, string entityType, Guid entityId, int statusCode, object? metadata, CancellationToken cancellationToken, string? reason = null)
    {
        auditLogWriter.Append(new AuditLogEntry(
            Module: "meetings",
            Action: action,
            EntityType: entityType,
            EntityId: entityId.ToString(),
            StatusCode: statusCode,
            Reason: reason,
            Metadata: metadata));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task AppendBusinessEventAsync(string eventType, string entityType, Guid entityId, string? actorUserId, string summary, string? reason, object? metadata, CancellationToken cancellationToken)
    {
        try
        {
            await businessAuditEventWriter.AppendAsync("meetings", eventType, entityType, entityId.ToString(), summary, reason, new { ActorUserId = actorUserId, Metadata = metadata }, cancellationToken);
        }
        catch
        {
            // Best-effort business audit.
        }
    }

    private static string SerializeList(IReadOnlyList<string>? values) =>
        JsonSerializer.Serialize(values?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray() ?? [], SerializerOptions);

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static MeetingCommandResult<T> Validation<T>(string errorCode, string message) =>
        new(MeetingCommandStatus.ValidationError, default, message, errorCode);

    private static MeetingCommandResult<T> NotFound<T>(string errorCode, string message) =>
        new(MeetingCommandStatus.NotFound, default, message, errorCode);

    private static MeetingCommandResult<T> Conflict<T>(string errorCode, string message) =>
        new(MeetingCommandStatus.Conflict, default, message, errorCode);
}
