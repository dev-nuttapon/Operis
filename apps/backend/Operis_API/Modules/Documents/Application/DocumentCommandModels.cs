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

public sealed record DocumentVersionCreateCommand(
    Guid DocumentId,
    string VersionCode,
    string FileName,
    string ContentType,
    long Size,
    string? UploadedByUserId);

public sealed record DocumentVersionCreateResult(
    bool Succeeded,
    DocumentVersionListItem? Version,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static DocumentVersionCreateResult Success(DocumentVersionListItem version) => new(true, version, null, null);

    public static DocumentVersionCreateResult Fail(string errorCode, string errorMessage) => new(false, null, errorCode, errorMessage);
}
