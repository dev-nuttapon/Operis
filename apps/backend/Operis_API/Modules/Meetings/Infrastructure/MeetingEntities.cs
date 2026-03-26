namespace Operis_API.Modules.Meetings.Infrastructure;

public sealed record MeetingRecordEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string MeetingType { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public DateTimeOffset MeetingAt { get; init; }
    public string FacilitatorUserId { get; init; } = string.Empty;
    public string Status { get; init; } = "draft";
    public string? Agenda { get; init; }
    public string? DiscussionSummary { get; init; }
    public bool IsRestricted { get; init; }
    public string? Classification { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record MeetingMinutesEntity
{
    public Guid Id { get; init; }
    public Guid MeetingRecordId { get; init; }
    public string? Summary { get; init; }
    public string? DecisionsSummary { get; init; }
    public string? ActionsSummary { get; init; }
    public string Status { get; init; } = "draft";
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record MeetingAttendeeEntity
{
    public Guid Id { get; init; }
    public Guid MeetingRecordId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string AttendanceStatus { get; init; } = "invited";
}

public sealed record DecisionEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public Guid? MeetingId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string DecisionType { get; init; } = string.Empty;
    public string Rationale { get; init; } = string.Empty;
    public string? AlternativesConsidered { get; init; }
    public string ImpactedArtifactsJson { get; init; } = "[]";
    public string? ApprovedBy { get; init; }
    public DateTimeOffset? ApprovedAt { get; init; }
    public string Status { get; init; } = "proposed";
    public bool IsRestricted { get; init; }
    public string? Classification { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
