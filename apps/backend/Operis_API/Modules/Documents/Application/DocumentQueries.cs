using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Documents.Contracts;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Documents.Application;

public sealed class DocumentQueries(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter) : IDocumentQueries
{
    public async Task<PagedResult<DocumentListItem>> ListDocumentsAsync(DocumentListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var baseQuery = dbContext.Documents
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            baseQuery = baseQuery.Where(x => EF.Functions.ILike(x.DocumentName, search));
        }

        var total = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderByDescending(x => x.UploadedAt)
            .Skip(skip)
            .Take(pageSize)
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
                        .FirstOrDefault(),
                    published = versions
                        .Where(version => document.PublishedVersionId != null && version.Id == document.PublishedVersionId)
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
                x.latest != null ? x.latest.Revision : null,
                x.published != null ? x.published.VersionCode : null,
                x.published != null ? x.published.Revision : null))
            .ToListAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "documents",
            Action: "list",
            EntityType: "document",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { count = items.Count, total, page, pageSize, query.Search }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PagedResult<DocumentListItem>(items, total, page, pageSize);
    }

    public async Task<IReadOnlyList<DocumentListItem>> GetDocumentsByIdsAsync(IReadOnlyList<Guid> documentIds, CancellationToken cancellationToken)
    {
        if (documentIds.Count == 0)
        {
            return Array.Empty<DocumentListItem>();
        }

        var baseQuery = dbContext.Documents
            .AsNoTracking()
            .Where(x => !x.IsDeleted && documentIds.Contains(x.Id));

        var items = await baseQuery
            .OrderByDescending(x => x.UploadedAt)
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
                        .FirstOrDefault(),
                    published = versions
                        .Where(version => document.PublishedVersionId != null && version.Id == document.PublishedVersionId)
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
                x.latest != null ? x.latest.Revision : null,
                x.published != null ? x.published.VersionCode : null,
                x.published != null ? x.published.Revision : null))
            .ToListAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "documents",
            Action: "lookup",
            EntityType: "document",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { count = items.Count, documentIds }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return items;
    }

    public async Task<PagedResult<DocumentVersionListItem>> ListDocumentVersionsAsync(DocumentVersionListQuery query, CancellationToken cancellationToken)
    {
        var publishedVersionId = await dbContext.Documents
            .AsNoTracking()
            .Where(x => x.Id == query.DocumentId && !x.IsDeleted)
            .Select(x => x.PublishedVersionId)
            .SingleOrDefaultAsync(cancellationToken);

        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var baseQuery = dbContext.DocumentVersions
            .AsNoTracking()
            .Where(x => x.DocumentId == query.DocumentId && !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            baseQuery = baseQuery.Where(x =>
                EF.Functions.ILike(x.VersionCode, search)
                || EF.Functions.ILike(x.FileName, search));
        }

        var total = await baseQuery.CountAsync(cancellationToken);

        var versions = await baseQuery
            .OrderByDescending(x => x.Revision)
            .ThenByDescending(x => x.UploadedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new DocumentVersionListItem(
                x.Id,
                x.DocumentId,
                x.Revision,
                x.VersionCode,
                x.FileName,
                x.ContentType ?? "application/octet-stream",
                x.SizeBytes,
                x.UploadedByUserId,
                x.UploadedAt,
                publishedVersionId != null && x.Id == publishedVersionId))
            .ToListAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "documents",
            Action: "list_versions",
            EntityType: "document_version",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { documentId = query.DocumentId, count = versions.Count, total, page, pageSize, query.Search }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PagedResult<DocumentVersionListItem>(versions, total, page, pageSize);
    }

    private static (int Page, int PageSize, int Skip) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = Math.Clamp(pageSize, 5, 100);
        var skip = (normalizedPage - 1) * normalizedPageSize;
        return (normalizedPage, normalizedPageSize, skip);
    }
}
