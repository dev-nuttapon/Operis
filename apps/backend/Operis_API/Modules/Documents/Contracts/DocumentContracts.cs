namespace Operis_API.Modules.Documents.Contracts;

public sealed record DocumentListItem(
    Guid Id,
    string DocumentName,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? UploadedByUserId,
    DateTimeOffset UploadedAt);

public sealed record DocumentCreateRequest(string DocumentName);

public sealed record DocumentDownloadResult(
    string FileName,
    string ContentType,
    Stream Content);
