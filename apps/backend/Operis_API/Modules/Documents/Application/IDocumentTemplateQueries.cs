using Operis_API.Modules.Documents.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Documents.Application;

public interface IDocumentTemplateQueries
{
    Task<PagedResult<DocumentTemplateListItem>> ListTemplatesAsync(DocumentTemplateListQuery query, CancellationToken cancellationToken);
    Task<DocumentTemplateResponse?> GetTemplateAsync(Guid templateId, CancellationToken cancellationToken);
    Task<DocumentTemplateDocumentValidationResult> ValidateTemplateDocumentsAsync(IReadOnlyList<Guid> documentIds, CancellationToken cancellationToken);
}

public sealed record DocumentTemplateDocumentValidationResult(
    bool IsValid,
    string? ErrorMessage);
