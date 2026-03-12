namespace Operis_API.Modules.Audits.Application;

public sealed record AuditLogListQuery(
    string? Module,
    string? Action,
    string? EntityType,
    string? EntityId,
    string? Actor,
    string? Status,
    string? SortBy,
    string? SortOrder,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page = 1,
    int PageSize = 10);
