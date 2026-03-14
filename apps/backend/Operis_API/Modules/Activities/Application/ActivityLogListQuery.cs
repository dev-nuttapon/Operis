namespace Operis_API.Modules.Activities.Application;

public sealed record ActivityLogListQuery(
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
    int Page,
    int PageSize);
