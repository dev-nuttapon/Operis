namespace Operis_API.Modules.Documents.Domain;

public sealed record DocumentListItem(Guid Id, string FileName, DateTimeOffset UploadedAt);
