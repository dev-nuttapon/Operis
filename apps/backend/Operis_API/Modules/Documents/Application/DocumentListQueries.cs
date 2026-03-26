namespace Operis_API.Modules.Documents.Application;

public sealed record DocumentListQuery(
    string? Search,
    Guid? DocumentTypeId = null,
    Guid? ProjectId = null,
    string? PhaseCode = null,
    string? Status = null,
    string? OwnerUserId = null,
    string? Classification = null,
    DateTimeOffset? UpdatedAfter = null,
    int Page = 1,
    int PageSize = 10);

public sealed record DocumentVersionListQuery(
    Guid DocumentId,
    string? Search,
    int Page = 1,
    int PageSize = 10);

public sealed record DocumentTypeListQuery(
    string? Search,
    string? Status,
    int Page = 1,
    int PageSize = 10);

public sealed record DocumentHistoryListQuery(
    Guid DocumentId,
    string? Search,
    int Page = 1,
    int PageSize = 10);
