using Operis_API.Modules.Documents.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Documents.Application;

public interface IDocumentTemplateHistoryQueries
{
    Task<PagedResult<DocumentTemplateHistoryItem>> ListAsync(DocumentTemplateHistoryListQuery query, CancellationToken cancellationToken);
}
