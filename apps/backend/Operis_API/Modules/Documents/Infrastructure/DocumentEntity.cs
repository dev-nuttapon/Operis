namespace Operis_API.Modules.Documents.Infrastructure;

public sealed class DocumentEntity
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public DateTimeOffset UploadedAt { get; init; }
}
