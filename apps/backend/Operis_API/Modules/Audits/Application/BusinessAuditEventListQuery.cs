namespace Operis_API.Modules.Audits.Application;

public sealed record BusinessAuditEventListQuery(
    string? Module,
    string? EventType,
    string? EntityType,
    string? EntityId,
    string? Actor,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page,
    int PageSize);
