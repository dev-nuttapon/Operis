namespace Operis_API.Modules.Audits.Infrastructure;

public sealed record BusinessAuditEventEntity
{
    public Guid Id { get; init; }
    public string Module { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public string? EntityId { get; init; }
    public string? Summary { get; init; }
    public string? Reason { get; init; }
    public string? ActorUserId { get; init; }
    public string? ActorEmail { get; init; }
    public string? ActorDisplayName { get; init; }
    public string? MetadataJson { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
