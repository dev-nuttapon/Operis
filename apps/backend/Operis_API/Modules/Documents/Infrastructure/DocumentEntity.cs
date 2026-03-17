namespace Operis_API.Modules.Documents.Infrastructure;

public sealed record DocumentEntity
{
    public Guid Id { get; init; }
    public string DocumentName { get; init; } = string.Empty;
    public Guid? PublishedVersionId { get; init; }
    public string? UploadedByUserId { get; init; }
    public DateTimeOffset UploadedAt { get; init; }
    public bool IsDeleted { get; init; }
    public string? DeletedByUserId { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
    public string? DeletedReason { get; init; }
}

public sealed record DocumentVersionEntity
{
    public Guid Id { get; init; }
    public Guid DocumentId { get; init; }
    public int Revision { get; init; }
    public string VersionCode { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string? ObjectKey { get; init; }
    public string? BucketName { get; init; }
    public string ContentType { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public string? UploadedByUserId { get; init; }
    public DateTimeOffset UploadedAt { get; init; }
    public bool IsDeleted { get; init; }
    public string? DeletedByUserId { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
    public string? DeletedReason { get; init; }
}
