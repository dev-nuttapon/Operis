using Operis_API.Modules.Documents.Contracts;

namespace Operis_API.Modules.Documents.Application;

public interface IDocumentQueries
{
    Task<IReadOnlyList<DocumentListItem>> ListDocumentsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<DocumentVersionListItem>> ListDocumentVersionsAsync(Guid documentId, CancellationToken cancellationToken);
}
