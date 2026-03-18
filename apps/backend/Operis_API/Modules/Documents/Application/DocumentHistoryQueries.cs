using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Documents.Contracts;

namespace Operis_API.Modules.Documents.Application;

public sealed class DocumentHistoryQueries(OperisDbContext dbContext) : IDocumentHistoryQueries
{
    public async Task<IReadOnlyList<DocumentHistoryItem>> ListAsync(Guid documentId, CancellationToken cancellationToken)
    {
        return await dbContext.DocumentHistories
            .AsNoTracking()
            .Where(x => x.DocumentId == documentId)
            .OrderByDescending(x => x.OccurredAt)
            .Select(x => new DocumentHistoryItem(
                x.Id,
                x.DocumentId,
                x.EventType,
                x.Summary,
                x.Reason,
                x.ActorUserId,
                x.ActorEmail,
                x.ActorDisplayName,
                x.BeforeJson,
                x.AfterJson,
                x.MetadataJson,
                x.OccurredAt))
            .ToListAsync(cancellationToken);
    }
}
