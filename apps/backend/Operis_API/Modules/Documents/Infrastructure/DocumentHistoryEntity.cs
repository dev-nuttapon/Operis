namespace Operis_API.Modules.Documents.Infrastructure;

public sealed record DocumentHistoryEntity
{
    public Guid Id { get; init; }
    public Guid DocumentId { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public string? Reason { get; init; }
    public string? ActorUserId { get; init; }
    public string? ActorEmail { get; init; }
    public string? ActorDisplayName { get; init; }
    public string? Status { get; init; }
    public int? StatusCode { get; init; }
    public string? Source { get; init; }
    public string? BeforeJson { get; init; }
    public string? AfterJson { get; init; }
    public string? MetadataJson { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
