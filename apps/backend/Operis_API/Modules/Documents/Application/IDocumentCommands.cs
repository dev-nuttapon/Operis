using Operis_API.Modules.Documents.Contracts;

namespace Operis_API.Modules.Documents.Application;

public interface IDocumentCommands
{
    Task<DocumentUploadResult> CreateDocumentAsync(DocumentCreateCommand request, CancellationToken cancellationToken);
    Task<DocumentVersionCreateResult> CreateDocumentVersionAsync(DocumentVersionCreateCommand request, Stream content, CancellationToken cancellationToken);
    Task<DocumentVersionDeleteResult> DeleteDocumentVersionAsync(DocumentVersionDeleteCommand request, CancellationToken cancellationToken);
    Task<DocumentUpdateResult> UpdateDocumentAsync(DocumentUpdateCommand request, CancellationToken cancellationToken);
    Task<DocumentDeleteResult> DeleteDocumentAsync(DocumentDeleteCommand request, CancellationToken cancellationToken);
    Task<DocumentTypeCommandResult> CreateDocumentTypeAsync(DocumentTypeCreateCommand request, CancellationToken cancellationToken);
    Task<DocumentTypeCommandResult> UpdateDocumentTypeAsync(DocumentTypeUpdateCommand request, CancellationToken cancellationToken);
    Task<DocumentWorkflowResult> SubmitDocumentAsync(DocumentWorkflowCommand request, CancellationToken cancellationToken);
    Task<DocumentWorkflowResult> ApproveDocumentAsync(DocumentWorkflowCommand request, CancellationToken cancellationToken);
    Task<DocumentWorkflowResult> RejectDocumentAsync(DocumentWorkflowCommand request, CancellationToken cancellationToken);
    Task<DocumentWorkflowResult> BaselineDocumentAsync(DocumentWorkflowCommand request, CancellationToken cancellationToken);
    Task<DocumentWorkflowResult> ArchiveDocumentAsync(DocumentWorkflowCommand request, CancellationToken cancellationToken);
    Task<DocumentLinkCommandResult> CreateDocumentLinkAsync(DocumentLinkCreateCommand request, CancellationToken cancellationToken);
}
