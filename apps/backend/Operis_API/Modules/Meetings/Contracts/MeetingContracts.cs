using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Meetings.Contracts;

public sealed record MeetingListItemResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string MeetingType,
    string Title,
    DateTimeOffset MeetingAt,
    string FacilitatorUserId,
    string Status,
    bool IsRestricted,
    DateTimeOffset UpdatedAt);

public sealed record MeetingAttendeeItemResponse(
    Guid Id,
    Guid MeetingRecordId,
    string UserId,
    string AttendanceStatus);

public sealed record MeetingMinutesResponse(
    Guid Id,
    Guid MeetingRecordId,
    string? Summary,
    string? DecisionsSummary,
    string? ActionsSummary,
    string Status,
    DateTimeOffset UpdatedAt);

public sealed record MeetingHistoryItem(
    Guid Id,
    string EventType,
    string? Summary,
    string? Reason,
    string? ActorUserId,
    DateTimeOffset OccurredAt);

public sealed record MeetingDetailResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string MeetingType,
    string Title,
    DateTimeOffset MeetingAt,
    string FacilitatorUserId,
    string Status,
    string? Agenda,
    string? DiscussionSummary,
    bool IsRestricted,
    string? Classification,
    MeetingMinutesResponse Minutes,
    IReadOnlyList<MeetingAttendeeItemResponse> Attendees,
    IReadOnlyList<DecisionListItemResponse> Decisions,
    IReadOnlyList<MeetingHistoryItem> History,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record DecisionListItemResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    Guid? MeetingId,
    string? MeetingTitle,
    string Code,
    string Title,
    string DecisionType,
    string? ApprovedBy,
    string Status,
    bool IsRestricted,
    DateTimeOffset UpdatedAt);

public sealed record DecisionDetailResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    Guid? MeetingId,
    string? MeetingTitle,
    string Code,
    string Title,
    string DecisionType,
    string Rationale,
    string? AlternativesConsidered,
    IReadOnlyList<string> ImpactedArtifacts,
    string? ApprovedBy,
    DateTimeOffset? ApprovedAt,
    string Status,
    bool IsRestricted,
    string? Classification,
    IReadOnlyList<MeetingHistoryItem> History,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateMeetingRequest(
    Guid ProjectId,
    string MeetingType,
    string Title,
    DateTimeOffset MeetingAt,
    string FacilitatorUserId,
    IReadOnlyList<string>? AttendeeUserIds,
    string? Agenda,
    string? DiscussionSummary,
    bool IsRestricted,
    string? Classification);

public sealed record UpdateMeetingRequest(
    string MeetingType,
    string Title,
    DateTimeOffset MeetingAt,
    string FacilitatorUserId,
    IReadOnlyList<string>? AttendeeUserIds,
    string? Agenda,
    string? DiscussionSummary,
    bool IsRestricted,
    string? Classification);

public sealed record MeetingApprovalRequest(string? Reason);

public sealed record UpdateMeetingMinutesRequest(
    string? Summary,
    string? DecisionsSummary,
    string? ActionsSummary,
    string? Status,
    IReadOnlyList<string>? AttendeeUserIds);

public sealed record CreateDecisionRequest(
    Guid ProjectId,
    Guid? MeetingId,
    string Code,
    string Title,
    string DecisionType,
    string Rationale,
    string? AlternativesConsidered,
    IReadOnlyList<string>? ImpactedArtifacts,
    bool IsRestricted,
    string? Classification);

public sealed record UpdateDecisionRequest(
    string Title,
    string DecisionType,
    string Rationale,
    string? AlternativesConsidered,
    IReadOnlyList<string>? ImpactedArtifacts,
    bool IsRestricted,
    string? Classification);

public sealed record DecisionTransitionRequest(string? Reason);

public sealed record MeetingListQuery(
    string? Search,
    Guid? ProjectId,
    string? MeetingType,
    DateOnly? MeetingDateFrom,
    DateOnly? MeetingDateTo,
    string? Status,
    int Page = 1,
    int PageSize = 25);

public sealed record DecisionListQuery(
    string? Search,
    Guid? ProjectId,
    string? DecisionType,
    string? Status,
    Guid? MeetingId,
    int Page = 1,
    int PageSize = 25);
