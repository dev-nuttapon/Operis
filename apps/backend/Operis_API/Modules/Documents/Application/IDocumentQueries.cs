using Operis_API.Modules.Documents.Domain;

namespace Operis_API.Modules.Documents.Application;

public interface IDocumentQueries
{
    Task<IReadOnlyList<DocumentListItem>> ListDocumentsAsync(CancellationToken cancellationToken);
}
