using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Documents.Infrastructure;

namespace Operis_API.Modules.Documents.Application;

public sealed class DocumentTemplateCacheCommands(OperisDbContext dbContext, IDocumentTemplateCache cache) : IDocumentTemplateCacheCommands
{
    public Task<int> RefreshAsync(CancellationToken cancellationToken) => cache.RefreshAsync(dbContext, cancellationToken);
}
