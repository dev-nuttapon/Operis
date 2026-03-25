using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Documents.Contracts;

namespace Operis_API.Modules.Documents.Infrastructure;

public interface IDocumentTemplateCache
{
    Task<IReadOnlyList<DocumentTemplateListItem>> GetTemplatesAsync(OperisDbContext dbContext, CancellationToken cancellationToken);
    Task<int> RefreshAsync(OperisDbContext dbContext, CancellationToken cancellationToken);
    Task InvalidateAsync(CancellationToken cancellationToken);
}
