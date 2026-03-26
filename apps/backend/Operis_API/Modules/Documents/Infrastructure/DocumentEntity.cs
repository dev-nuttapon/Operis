namespace Operis_API.Modules.Documents.Infrastructure;

public sealed record DocumentEntity
{
    public Guid Id { get; init; }
    public Guid? DocumentTypeId { get; init; }
    public Guid? ProjectId { get; init; }
    public string? PhaseCode { get; init; }
    public string? OwnerUserId { get; init; }
    public Guid? CurrentVersionId { get; init; }
    public string Status { get; init; } = "draft";
    public string Classification { get; init; } = "internal";
    public string RetentionClass { get; init; } = "standard";
    public string Title { get; init; } = string.Empty;
    public string TagsJson { get; init; } = "[]";
    public bool IsDeleted { get; init; }
    public string? DeletedByUserId { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
    public string? DeletedReason { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record DocumentVersionEntity
{
    public Guid Id { get; init; }
    public Guid DocumentId { get; init; }
    public int VersionNumber { get; init; }
    public string StorageKey { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string MimeType { get; init; } = string.Empty;
    public string UploadedBy { get; init; } = string.Empty;
    public DateTimeOffset UploadedAt { get; init; }
    public string Status { get; init; } = "uploaded";
    public bool IsDeleted { get; init; }
    public string? DeletedByUserId { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
    public string? DeletedReason { get; init; }
}

public sealed record DocumentTypeEntity
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string ModuleOwner { get; init; } = string.Empty;
    public string ClassificationDefault { get; init; } = string.Empty;
    public string RetentionClassDefault { get; init; } = string.Empty;
    public string Status { get; init; } = "active";
    public bool ApprovalRequired { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record DocumentApprovalEntity
{
    public Guid Id { get; init; }
    public Guid DocumentVersionId { get; init; }
    public string StepName { get; init; } = string.Empty;
    public string ReviewerUserId { get; init; } = string.Empty;
    public string Decision { get; init; } = "pending";
    public string? DecisionReason { get; init; }
    public DateTimeOffset? DecidedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record DocumentLinkEntity
{
    public Guid Id { get; init; }
    public Guid SourceDocumentId { get; init; }
    public string TargetEntityType { get; init; } = string.Empty;
    public string TargetEntityId { get; init; } = string.Empty;
    public string LinkType { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}
