using Operis_API.Modules.Documents.Contracts;

namespace Operis_API.Modules.Documents.Application;

public interface IDocumentHistoryQueries
{
    Task<IReadOnlyList<DocumentHistoryItem>> ListAsync(Guid documentId, CancellationToken cancellationToken);
}
