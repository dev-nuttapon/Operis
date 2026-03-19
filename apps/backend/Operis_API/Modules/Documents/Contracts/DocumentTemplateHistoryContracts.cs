namespace Operis_API.Modules.Documents.Contracts;

public sealed record DocumentTemplateHistoryItem(
    Guid Id,
    Guid TemplateId,
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
