using Operis_API.Modules.Documents.Contracts;

namespace Operis_API.Modules.Documents.Application;

public interface IDocumentCommands
{
    Task<DocumentUploadResult> UploadDocumentAsync(DocumentUploadRequest request, Stream content, CancellationToken cancellationToken);
}
