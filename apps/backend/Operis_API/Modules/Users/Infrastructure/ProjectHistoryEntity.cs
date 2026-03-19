namespace Operis_API.Modules.Users.Infrastructure;

public sealed record ProjectHistoryEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public string? Reason { get; init; }
    public string? ActorUserId { get; init; }
    public string? ActorEmail { get; init; }
    public string? ActorDisplayName { get; init; }
    public string? BeforeJson { get; init; }
    public string? AfterJson { get; init; }
    public string? MetadataJson { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
