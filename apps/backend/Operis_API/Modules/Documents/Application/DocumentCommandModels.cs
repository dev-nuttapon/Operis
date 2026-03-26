using Operis_API.Modules.Documents.Contracts;

namespace Operis_API.Modules.Documents.Application;

public sealed record DocumentCreateCommand(
    Guid DocumentTypeId,
    Guid ProjectId,
    string PhaseCode,
    string OwnerUserId,
    string Classification,
    string RetentionClass,
    string Title,
    IReadOnlyList<string> Tags,
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

public sealed record DocumentWorkflowCommand(
    Guid DocumentId,
    string? ActorUserId,
    string? StepName = null,
    string? ReviewerUserId = null,
    string? Reason = null);

public sealed record DocumentUpdateCommand(
    Guid DocumentId,
    Guid DocumentTypeId,
    Guid ProjectId,
    string PhaseCode,
    string OwnerUserId,
    string Classification,
    string RetentionClass,
    string Title,
    IReadOnlyList<string> Tags,
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

public sealed record DocumentTypeCreateCommand(
    string Code,
    string Name,
    string ModuleOwner,
    string ClassificationDefault,
    string RetentionClassDefault,
    bool ApprovalRequired);

public sealed record DocumentTypeUpdateCommand(
    Guid DocumentTypeId,
    string Code,
    string Name,
    string ModuleOwner,
    string ClassificationDefault,
    string RetentionClassDefault,
    bool ApprovalRequired,
    string Status);

public sealed record DocumentLinkCreateCommand(
    Guid DocumentId,
    string TargetEntityType,
    string TargetEntityId,
    string LinkType,
    string? CreatedByUserId);

public sealed record DocumentTypeCommandResult(
    bool Succeeded,
    DocumentTypeResponse? DocumentType,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static DocumentTypeCommandResult Success(DocumentTypeResponse documentType) => new(true, documentType, null, null);

    public static DocumentTypeCommandResult Fail(string errorCode, string errorMessage) => new(false, null, errorCode, errorMessage);
}

public sealed record DocumentLinkCommandResult(
    bool Succeeded,
    DocumentLinkItem? Link,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static DocumentLinkCommandResult Success(DocumentLinkItem link) => new(true, link, null, null);

    public static DocumentLinkCommandResult Fail(string errorCode, string errorMessage) => new(false, null, errorCode, errorMessage);
}

public sealed record DocumentWorkflowResult(
    bool Succeeded,
    DocumentDetailResponse? Document,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static DocumentWorkflowResult Success(DocumentDetailResponse document) => new(true, document, null, null);

    public static DocumentWorkflowResult Fail(string errorCode, string errorMessage) => new(false, null, errorCode, errorMessage);
}
