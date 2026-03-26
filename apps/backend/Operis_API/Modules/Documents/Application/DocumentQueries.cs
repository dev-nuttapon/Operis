using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Documents.Contracts;
using Operis_API.Modules.Documents.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Documents.Application;

public sealed class DocumentQueries(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter) : IDocumentQueries
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<PagedResult<DocumentListItem>> ListDocumentsAsync(DocumentListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var baseQuery =
            from document in dbContext.Documents.AsNoTracking()
            where !document.IsDeleted
            join documentType in dbContext.DocumentTypes.AsNoTracking() on document.DocumentTypeId equals documentType.Id into typeJoin
            from documentType in typeJoin.DefaultIfEmpty()
            join project in dbContext.Projects.AsNoTracking() on document.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            join version in dbContext.DocumentVersions.AsNoTracking() on document.CurrentVersionId equals version.Id into versionJoin
            from version in versionJoin.DefaultIfEmpty()
            select new { Document = document, DocumentType = documentType, Project = project, Version = version };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            baseQuery = baseQuery.Where(x =>
                EF.Functions.ILike(x.Document.Title, search)
                || (x.DocumentType != null && EF.Functions.ILike(x.DocumentType.Name, search))
                || (x.Project != null && EF.Functions.ILike(x.Project.Name, search)));
        }

        if (query.DocumentTypeId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Document.DocumentTypeId == query.DocumentTypeId.Value);
        }

        if (query.ProjectId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Document.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.PhaseCode))
        {
            baseQuery = baseQuery.Where(x => x.Document.PhaseCode == query.PhaseCode.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            baseQuery = baseQuery.Where(x => x.Document.Status == query.Status.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.OwnerUserId))
        {
            baseQuery = baseQuery.Where(x => x.Document.OwnerUserId == query.OwnerUserId.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Classification))
        {
            baseQuery = baseQuery.Where(x => x.Document.Classification == query.Classification.Trim());
        }

        if (query.UpdatedAfter.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Document.UpdatedAt >= query.UpdatedAfter.Value);
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderByDescending(x => x.Document.UpdatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new DocumentListItem(
                x.Document.Id,
                x.Document.Title,
                x.Document.DocumentTypeId,
                x.DocumentType != null ? x.DocumentType.Code : null,
                x.DocumentType != null ? x.DocumentType.Name : null,
                x.Document.ProjectId,
                x.Project != null ? x.Project.Name : null,
                x.Document.PhaseCode,
                x.Document.OwnerUserId,
                x.Document.Status,
                x.Document.Classification,
                x.Document.RetentionClass,
                x.Version != null ? x.Version.VersionNumber : null,
                x.Version != null ? x.Version.Status : null,
                x.Version != null ? x.Version.FileName : null,
                x.Version != null ? x.Version.MimeType : null,
                x.Version != null ? x.Version.FileSize : null,
                x.Document.UpdatedAt))
            .ToListAsync(cancellationToken);

        await WriteAuditAsync("list", "document", new { total, page, pageSize, query.Search, query.ProjectId, query.Status, query.Classification }, cancellationToken);
        return new PagedResult<DocumentListItem>(items, total, page, pageSize);
    }

    public async Task<IReadOnlyList<DocumentListItem>> GetDocumentsByIdsAsync(IReadOnlyList<Guid> documentIds, CancellationToken cancellationToken)
    {
        if (documentIds.Count == 0)
        {
            return [];
        }

        return await (
            from document in dbContext.Documents.AsNoTracking()
            where documentIds.Contains(document.Id) && !document.IsDeleted
            join documentType in dbContext.DocumentTypes.AsNoTracking() on document.DocumentTypeId equals documentType.Id into typeJoin
            from documentType in typeJoin.DefaultIfEmpty()
            join project in dbContext.Projects.AsNoTracking() on document.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            join version in dbContext.DocumentVersions.AsNoTracking() on document.CurrentVersionId equals version.Id into versionJoin
            from version in versionJoin.DefaultIfEmpty()
            select new DocumentListItem(
                document.Id,
                document.Title,
                document.DocumentTypeId,
                documentType != null ? documentType.Code : null,
                documentType != null ? documentType.Name : null,
                document.ProjectId,
                project != null ? project.Name : null,
                document.PhaseCode,
                document.OwnerUserId,
                document.Status,
                document.Classification,
                document.RetentionClass,
                version != null ? version.VersionNumber : null,
                version != null ? version.Status : null,
                version != null ? version.FileName : null,
                version != null ? version.MimeType : null,
                version != null ? version.FileSize : null,
                document.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<DocumentDetailResponse?> GetDocumentAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var item = await (
            from document in dbContext.Documents.AsNoTracking()
            where document.Id == documentId && !document.IsDeleted
            join documentType in dbContext.DocumentTypes.AsNoTracking() on document.DocumentTypeId equals documentType.Id into typeJoin
            from documentType in typeJoin.DefaultIfEmpty()
            join project in dbContext.Projects.AsNoTracking() on document.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            select new
            {
                Document = document,
                DocumentTypeCode = documentType != null ? documentType.Code : null,
                DocumentTypeName = documentType != null ? documentType.Name : null,
                ProjectName = project != null ? project.Name : null
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return null;
        }

        var versions = await dbContext.DocumentVersions.AsNoTracking()
            .Where(x => x.DocumentId == documentId && !x.IsDeleted)
            .OrderByDescending(x => x.VersionNumber)
            .Select(x => new DocumentVersionListItem(
                x.Id,
                x.DocumentId,
                x.VersionNumber,
                x.FileName,
                x.MimeType,
                x.FileSize,
                x.UploadedBy,
                x.UploadedAt,
                x.Status))
            .ToListAsync(cancellationToken);

        var approvals = await dbContext.DocumentApprovals.AsNoTracking()
            .Where(x => versions.Select(version => version.Id).Contains(x.DocumentVersionId))
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new DocumentApprovalItem(x.Id, x.DocumentVersionId, x.StepName, x.ReviewerUserId, x.Decision, x.DecisionReason, x.DecidedAt))
            .ToListAsync(cancellationToken);

        var links = await dbContext.DocumentLinks.AsNoTracking()
            .Where(x => x.SourceDocumentId == documentId)
            .OrderBy(x => x.LinkType)
            .Select(x => new DocumentLinkItem(x.Id, x.SourceDocumentId, x.TargetEntityType, x.TargetEntityId, x.LinkType))
            .ToListAsync(cancellationToken);

        await WriteAuditAsync("read", "document", new { documentId }, cancellationToken);
        return new DocumentDetailResponse(
            item.Document.Id,
            item.Document.Title,
            item.Document.DocumentTypeId,
            item.DocumentTypeCode,
            item.DocumentTypeName,
            item.Document.ProjectId,
            item.ProjectName,
            item.Document.PhaseCode,
            item.Document.OwnerUserId,
            item.Document.Status,
            item.Document.Classification,
            item.Document.RetentionClass,
            DeserializeTags(item.Document.TagsJson),
            item.Document.CurrentVersionId,
            versions,
            approvals,
            links,
            item.Document.CreatedAt,
            item.Document.UpdatedAt);
    }

    public async Task<PagedResult<DocumentVersionListItem>> ListDocumentVersionsAsync(DocumentVersionListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var baseQuery = dbContext.DocumentVersions.AsNoTracking()
            .Where(x => x.DocumentId == query.DocumentId && !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            baseQuery = baseQuery.Where(x => EF.Functions.ILike(x.FileName, search) || EF.Functions.ILike(x.Status, search));
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderByDescending(x => x.VersionNumber)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new DocumentVersionListItem(
                x.Id,
                x.DocumentId,
                x.VersionNumber,
                x.FileName,
                x.MimeType,
                x.FileSize,
                x.UploadedBy,
                x.UploadedAt,
                x.Status))
            .ToListAsync(cancellationToken);

        await WriteAuditAsync("list_versions", "document_version", new { query.DocumentId, total, page, pageSize }, cancellationToken);
        return new PagedResult<DocumentVersionListItem>(items, total, page, pageSize);
    }

    public async Task<PagedResult<DocumentTypeListItem>> ListDocumentTypesAsync(DocumentTypeListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var baseQuery = dbContext.DocumentTypes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            baseQuery = baseQuery.Where(x => EF.Functions.ILike(x.Code, search) || EF.Functions.ILike(x.Name, search));
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            baseQuery = baseQuery.Where(x => x.Status == query.Status.Trim());
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderBy(x => x.Code)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new DocumentTypeListItem(
                x.Id,
                x.Code,
                x.Name,
                x.ModuleOwner,
                x.ClassificationDefault,
                x.RetentionClassDefault,
                x.Status,
                x.ApprovalRequired,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        await WriteAuditAsync("list", "document_type", new { total, page, pageSize, query.Search, query.Status }, cancellationToken);
        return new PagedResult<DocumentTypeListItem>(items, total, page, pageSize);
    }

    public async Task<DocumentTypeResponse?> GetDocumentTypeAsync(Guid documentTypeId, CancellationToken cancellationToken)
    {
        var item = await dbContext.DocumentTypes.AsNoTracking()
            .Where(x => x.Id == documentTypeId)
            .Select(x => new DocumentTypeResponse(
                x.Id,
                x.Code,
                x.Name,
                x.ModuleOwner,
                x.ClassificationDefault,
                x.RetentionClassDefault,
                x.Status,
                x.ApprovalRequired,
                x.CreatedAt,
                x.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);

        if (item is not null)
        {
            await WriteAuditAsync("read", "document_type", new { documentTypeId }, cancellationToken);
        }

        return item;
    }

    private async Task WriteAuditAsync(string action, string entityType, object metadata, CancellationToken cancellationToken)
    {
        auditLogWriter.Append(new AuditLogEntry(
            Module: "documents",
            Action: action,
            EntityType: entityType,
            StatusCode: StatusCodes.Status200OK,
            Metadata: metadata));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyList<string> DeserializeTags(string? json) =>
        string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<string>>(json, SerializerOptions) ?? [];

    private static (int Page, int PageSize, int Skip) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = Math.Clamp(pageSize, 5, 100);
        return (normalizedPage, normalizedPageSize, (normalizedPage - 1) * normalizedPageSize);
    }
}
