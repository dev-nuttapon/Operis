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
            .OrderByDescending(x => x.UploadedAt)
            .Take(50)
            .GroupJoin(
                dbContext.DocumentVersions.AsNoTracking(),
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
}
