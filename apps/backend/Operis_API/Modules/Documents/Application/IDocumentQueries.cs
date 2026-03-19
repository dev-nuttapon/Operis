using Operis_API.Modules.Documents.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Documents.Application;

public interface IDocumentQueries
{
    Task<PagedResult<DocumentListItem>> ListDocumentsAsync(DocumentListQuery query, CancellationToken cancellationToken);
    Task<IReadOnlyList<DocumentListItem>> GetDocumentsByIdsAsync(IReadOnlyList<Guid> documentIds, CancellationToken cancellationToken);
    Task<PagedResult<DocumentVersionListItem>> ListDocumentVersionsAsync(DocumentVersionListQuery query, CancellationToken cancellationToken);
}
