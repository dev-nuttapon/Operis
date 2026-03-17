using Operis_API.Modules.Documents.Contracts;

namespace Operis_API.Modules.Documents.Application;

public sealed record DocumentCreateCommand(
    string DocumentName,
    string? CreatedByUserId);

public sealed record DocumentUploadResult(
    bool Succeeded,
    DocumentListItem? Document,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static DocumentUploadResult Success(DocumentListItem document) => new(true, document, null, null);

    public static DocumentUploadResult Fail(string errorCode, string errorMessage) => new(false, null, errorCode, errorMessage);
}
