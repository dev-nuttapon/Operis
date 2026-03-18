using Operis_API.Modules.Documents.Contracts;

namespace Operis_API.Modules.Documents.Application;

public interface IDocumentTemplateCommands
{
    Task<DocumentTemplateResponse> CreateTemplateAsync(DocumentTemplateCreateCommand command, CancellationToken cancellationToken);
    Task<DocumentTemplateResponse> UpdateTemplateAsync(DocumentTemplateUpdateCommand command, CancellationToken cancellationToken);
}

public sealed record DocumentTemplateCreateCommand(
    string Name,
    IReadOnlyList<Guid> DocumentIds,
    string? ActorUserId);

public sealed record DocumentTemplateUpdateCommand(
    Guid TemplateId,
    string Name,
    IReadOnlyList<Guid> DocumentIds,
    string? ActorUserId);
