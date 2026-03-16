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
            .SingleOrDefaultAsync(x => x.Id == documentId, cancellationToken);

        if (entity is null || string.IsNullOrWhiteSpace(entity.ObjectKey))
        {
            return null;
        }

        var content = await documentObjectStorage.OpenReadAsync(entity.ObjectKey, cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "documents",
            Action: "read",
            EntityType: "document",
            EntityId: entity.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { entity.FileName, entity.ContentType, entity.SizeBytes }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new DocumentDownloadResult(entity.FileName, entity.ContentType, content);
    }
}
