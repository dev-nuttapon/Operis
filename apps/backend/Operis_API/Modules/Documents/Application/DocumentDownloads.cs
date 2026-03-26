using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Documents.Contracts;
using Operis_API.Modules.Documents.Infrastructure;
using Operis_API.Shared.Auditing;

namespace Operis_API.Modules.Documents.Application;

public sealed class DocumentDownloads(
    OperisDbContext dbContext,
    IDocumentObjectStorage documentObjectStorage,
    IAuditLogWriter auditLogWriter) : IDocumentDownloads
{
    public async Task<DocumentDownloadResult?> GetDownloadAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Documents
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == documentId && !x.IsDeleted, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (entity.CurrentVersionId is null)
        {
            return null;
        }

        var publishedVersion = await dbContext.DocumentVersions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == entity.CurrentVersionId && !x.IsDeleted, cancellationToken);

        if (publishedVersion is null || string.IsNullOrWhiteSpace(publishedVersion.StorageKey))
        {
            return null;
        }

        var content = await documentObjectStorage.OpenReadAsync(publishedVersion.StorageKey, cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "documents",
            Action: "read",
            EntityType: "document",
            EntityId: entity.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { publishedVersion.FileName, publishedVersion.MimeType, publishedVersion.FileSize }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new DocumentDownloadResult(publishedVersion.FileName, publishedVersion.MimeType, content);
    }
}
