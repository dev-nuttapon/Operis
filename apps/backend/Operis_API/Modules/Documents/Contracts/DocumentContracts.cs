namespace Operis_API.Modules.Documents.Contracts;

public sealed record DocumentTypeListItem(
    Guid Id,
    string Code,
    string Name,
    string ModuleOwner,
    string ClassificationDefault,
    string RetentionClassDefault,
    string Status,
    bool ApprovalRequired,
    DateTimeOffset UpdatedAt);

public sealed record DocumentTypeResponse(
    Guid Id,
    string Code,
    string Name,
    string ModuleOwner,
    string ClassificationDefault,
    string RetentionClassDefault,
    string Status,
    bool ApprovalRequired,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record DocumentListItem(
    Guid Id,
    string Title,
    Guid? DocumentTypeId,
    string? DocumentTypeCode,
    string? DocumentTypeName,
    Guid? ProjectId,
    string? ProjectName,
    string? PhaseCode,
    string? OwnerUserId,
    string Status,
    string Classification,
    string RetentionClass,
    int? CurrentVersionNumber,
    string? CurrentVersionStatus,
    string? CurrentFileName,
    string? CurrentMimeType,
    long? CurrentFileSize,
    DateTimeOffset UpdatedAt);

public sealed record DocumentDetailResponse(
    Guid Id,
    string Title,
    Guid? DocumentTypeId,
    string? DocumentTypeCode,
    string? DocumentTypeName,
    Guid? ProjectId,
    string? ProjectName,
    string? PhaseCode,
    string? OwnerUserId,
    string Status,
    string Classification,
    string RetentionClass,
    IReadOnlyList<string> Tags,
    Guid? CurrentVersionId,
    IReadOnlyList<DocumentVersionListItem> Versions,
    IReadOnlyList<DocumentApprovalItem> Approvals,
    IReadOnlyList<DocumentLinkItem> Links,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record DocumentCreateRequest(
    Guid DocumentTypeId,
    Guid ProjectId,
    string PhaseCode,
    string OwnerUserId,
    string Classification,
    string RetentionClass,
    string Title,
    IReadOnlyList<string>? Tags);

public sealed record DocumentUpdateRequest(
    Guid DocumentTypeId,
    Guid ProjectId,
    string PhaseCode,
    string OwnerUserId,
    string Classification,
    string RetentionClass,
    string Title,
    IReadOnlyList<string>? Tags);
public sealed record DocumentDeleteRequest(string Reason);

public sealed record DocumentLookupRequest(IReadOnlyList<Guid> DocumentIds);

public sealed record DocumentVersionListItem(
    Guid Id,
    Guid DocumentId,
    int VersionNumber,
    string FileName,
    string MimeType,
    long FileSize,
    string UploadedBy,
    DateTimeOffset UploadedAt,
    string Status);

public sealed record DocumentApprovalItem(
    Guid Id,
    Guid DocumentVersionId,
    string StepName,
    string ReviewerUserId,
    string Decision,
    string? DecisionReason,
    DateTimeOffset? DecidedAt);

public sealed record DocumentLinkItem(
    Guid Id,
    Guid SourceDocumentId,
    string TargetEntityType,
    string TargetEntityId,
    string LinkType);

public sealed record DocumentTypeCreateRequest(
    string Code,
    string Name,
    string ModuleOwner,
    string ClassificationDefault,
    string RetentionClassDefault,
    bool ApprovalRequired);

public sealed record DocumentTypeUpdateRequest(
    string Code,
    string Name,
    string ModuleOwner,
    string ClassificationDefault,
    string RetentionClassDefault,
    bool ApprovalRequired,
    string Status);

public sealed record DocumentApprovalDecisionRequest(string StepName, string ReviewerUserId, string DecisionReason);

public sealed record DocumentLinkRequest(string TargetEntityType, string TargetEntityId, string LinkType);

public sealed record DocumentDownloadResult(
    string FileName,
    string ContentType,
    Stream Content);
