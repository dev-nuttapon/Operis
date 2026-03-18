using Operis_API.Modules.Documents.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Documents.Application;

public interface IDocumentHistoryQueries
{
    Task<PagedResult<DocumentHistoryItem>> ListAsync(DocumentHistoryListQuery query, CancellationToken cancellationToken);
}
