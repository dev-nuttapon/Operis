namespace Operis_API.Modules.Documents.Contracts;

public sealed record DocumentListItem(
    Guid Id,
    string DocumentName,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? UploadedByUserId,
    DateTimeOffset UploadedAt,
    string? VersionCode,
    int? Revision,
    string? PublishedVersionCode,
    int? PublishedRevision);

public sealed record DocumentCreateRequest(string DocumentName);

public sealed record DocumentUpdateRequest(string DocumentName);
public sealed record DocumentDeleteRequest(string Reason);

public sealed record DocumentVersionListItem(
    Guid Id,
    Guid DocumentId,
    int Revision,
    string VersionCode,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? UploadedByUserId,
    DateTimeOffset UploadedAt,
    bool IsPublished);

public sealed record DocumentDownloadResult(
    string FileName,
    string ContentType,
    Stream Content);
