namespace Operis_API.Modules.Documents.Infrastructure;

public sealed class DocumentEntity
{
    public Guid Id { get; init; }
    public string DocumentName { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string? ObjectKey { get; init; }
    public string? BucketName { get; init; }
    public string ContentType { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public string? UploadedByUserId { get; init; }
    public DateTimeOffset UploadedAt { get; init; }
}
