using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Documents.Contracts;
using Operis_API.Shared.Auditing;

namespace Operis_API.Modules.Documents.Application;

public sealed class DocumentQueries(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter) : IDocumentQueries
{
    public async Task<IReadOnlyList<DocumentListItem>> ListDocumentsAsync(CancellationToken cancellationToken)
    {
        var items = await dbContext.Documents
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.UploadedAt)
            .Take(50)
            .GroupJoin(
                dbContext.DocumentVersions.AsNoTracking().Where(x => !x.IsDeleted),
                document => document.Id,
                version => version.DocumentId,
                (document, versions) => new
                {
                    document,
                    latest = versions
                        .OrderByDescending(version => version.Revision)
                        .ThenByDescending(version => version.UploadedAt)
                        .FirstOrDefault()
                })
            .Select(x => new DocumentListItem(
                x.document.Id,
                x.document.DocumentName,
                x.latest != null ? x.latest.FileName : string.Empty,
                x.latest != null ? x.latest.ContentType : "application/octet-stream",
                x.latest != null ? x.latest.SizeBytes : 0,
                x.latest != null ? x.latest.UploadedByUserId : x.document.UploadedByUserId,
                x.latest != null ? x.latest.UploadedAt : x.document.UploadedAt,
                x.latest != null ? x.latest.VersionCode : null,
                x.latest != null ? x.latest.Revision : null))
            .ToListAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "documents",
            Action: "list",
            EntityType: "document",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { count = items.Count }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return items;
    }

    public async Task<IReadOnlyList<DocumentVersionListItem>> ListDocumentVersionsAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var versions = await dbContext.DocumentVersions
            .AsNoTracking()
            .Where(x => x.DocumentId == documentId && !x.IsDeleted)
            .OrderByDescending(x => x.Revision)
            .ThenByDescending(x => x.UploadedAt)
            .Select(x => new DocumentVersionListItem(
                x.Id,
                x.DocumentId,
                x.Revision,
                x.VersionCode,
                x.FileName,
                x.ContentType ?? "application/octet-stream",
                x.SizeBytes,
                x.UploadedByUserId,
                x.UploadedAt))
            .ToListAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "documents",
            Action: "list_versions",
            EntityType: "document_version",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { documentId, count = versions.Count }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return versions;
    }
}
