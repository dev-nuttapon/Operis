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
            .Select(x => new DocumentListItem(x.Id, x.DocumentName, x.FileName, x.ContentType ?? "application/octet-stream", x.SizeBytes, x.UploadedByUserId, x.UploadedAt))
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
