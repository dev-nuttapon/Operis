using Operis_API.Modules.Documents.Contracts;

namespace Operis_API.Modules.Documents.Application;

public interface IDocumentCommands
{
    Task<DocumentUploadResult> CreateDocumentAsync(DocumentCreateCommand request, CancellationToken cancellationToken);
    Task<DocumentVersionCreateResult> CreateDocumentVersionAsync(DocumentVersionCreateCommand request, Stream content, CancellationToken cancellationToken);
    Task<DocumentVersionDeleteResult> DeleteDocumentVersionAsync(DocumentVersionDeleteCommand request, CancellationToken cancellationToken);
    Task<DocumentUpdateResult> UpdateDocumentAsync(DocumentUpdateCommand request, CancellationToken cancellationToken);
    Task<DocumentDeleteResult> DeleteDocumentAsync(DocumentDeleteCommand request, CancellationToken cancellationToken);
}
