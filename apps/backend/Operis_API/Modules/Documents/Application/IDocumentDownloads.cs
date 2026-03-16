using Operis_API.Modules.Documents.Contracts;

namespace Operis_API.Modules.Documents.Application;

public interface IDocumentDownloads
{
    Task<DocumentDownloadResult?> GetDownloadAsync(Guid documentId, CancellationToken cancellationToken);
}
