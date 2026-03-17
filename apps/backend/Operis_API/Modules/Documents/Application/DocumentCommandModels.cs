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

public sealed record DocumentVersionDeleteCommand(
    Guid DocumentId,
    Guid VersionId,
    string? DeletedByUserId);

public sealed record DocumentVersionDeleteResult(
    bool Succeeded,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static DocumentVersionDeleteResult Success() => new(true, null, null);

    public static DocumentVersionDeleteResult Fail(string errorCode, string errorMessage) => new(false, errorCode, errorMessage);
}

public sealed record DocumentUpdateCommand(
    Guid DocumentId,
    string DocumentName,
    string? UpdatedByUserId);

public sealed record DocumentUpdateResult(
    bool Succeeded,
    DocumentListItem? Document,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static DocumentUpdateResult Success(DocumentListItem document) => new(true, document, null, null);

    public static DocumentUpdateResult Fail(string errorCode, string errorMessage) => new(false, null, errorCode, errorMessage);
}

public sealed record DocumentDeleteCommand(
    Guid DocumentId,
    string? DeletedByUserId,
    string? DeletedReason);

public sealed record DocumentDeleteResult(
    bool Succeeded,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static DocumentDeleteResult Success() => new(true, null, null);

    public static DocumentDeleteResult Fail(string errorCode, string errorMessage) => new(false, errorCode, errorMessage);
}
