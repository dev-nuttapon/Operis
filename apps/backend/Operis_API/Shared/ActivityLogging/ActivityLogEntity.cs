namespace Operis_API.Shared.ActivityLogging;

public sealed class ActivityLogEntity
{
    public Guid Id { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
    public string Module { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public string? EntityId { get; init; }
    public string ActorType { get; init; } = string.Empty;
    public string? ActorUserId { get; init; }
    public string? ActorEmail { get; init; }
    public string? ActorDisplayName { get; init; }
    public Guid? DepartmentId { get; init; }
    public string? TenantId { get; init; }
    public string? RequestId { get; init; }
    public string? TraceId { get; init; }
    public string? CorrelationId { get; init; }
    public string? HttpMethod { get; init; }
    public string? RequestPath { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string Status { get; init; } = string.Empty;
    public int? StatusCode { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public string? Reason { get; init; }
    public string Source { get; init; } = string.Empty;
    public string? BeforeJson { get; init; }
    public string? AfterJson { get; init; }
    public string? ChangesJson { get; init; }
    public string? MetadataJson { get; init; }
    public bool IsSensitive { get; init; }
    public string? RetentionClass { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
