namespace Operis_API.Modules.Documents.Contracts;

public sealed record DocumentHistoryItem(
    Guid Id,
    Guid DocumentId,
    string EventType,
    string? Summary,
    string? Reason,
    string? ActorUserId,
    string? ActorEmail,
    string? ActorDisplayName,
    string? Status,
    int? StatusCode,
    string? Source,
    string? BeforeJson,
    string? AfterJson,
    string? MetadataJson,
    DateTimeOffset OccurredAt);
