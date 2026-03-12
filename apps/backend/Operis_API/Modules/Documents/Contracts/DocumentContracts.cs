namespace Operis_API.Modules.Documents.Contracts;

public sealed record DocumentListItem(Guid Id, string FileName, DateTimeOffset UploadedAt);
