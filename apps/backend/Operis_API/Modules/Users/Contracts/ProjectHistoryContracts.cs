namespace Operis_API.Modules.Users.Contracts;

public sealed record ProjectHistoryItem(
    Guid Id,
    Guid ProjectId,
    string EventType,
    string? Summary,
    string? Reason,
    string? ActorUserId,
    string? ActorEmail,
    string? ActorDisplayName,
    string? BeforeJson,
    string? AfterJson,
    string? MetadataJson,
    DateTimeOffset OccurredAt);
