namespace Operis_API.Modules.Documents.Infrastructure;

public sealed class DocumentStorageOptions
{
    public const string SectionName = "Minio";

    public string Endpoint { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string BucketName { get; init; } = "operis-documents";
    public bool UseSsl { get; init; }
    public long MaxFileSizeBytes { get; init; } = 25 * 1024 * 1024;
}
