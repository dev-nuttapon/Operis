namespace Operis_API.Modules.Documents.Contracts;

public sealed record DocumentTemplateListItem(
    Guid Id,
    string Name,
    int DocumentCount,
    DateTimeOffset CreatedAt);

public sealed record DocumentTemplateCreateRequest(
    string Name,
    IReadOnlyList<Guid> DocumentIds);

public sealed record DocumentTemplateUpdateRequest(
    string Name,
    IReadOnlyList<Guid> DocumentIds);

public sealed record DocumentTemplateResponse(
    Guid Id,
    string Name,
    IReadOnlyList<Guid> DocumentIds,
    DateTimeOffset CreatedAt);
