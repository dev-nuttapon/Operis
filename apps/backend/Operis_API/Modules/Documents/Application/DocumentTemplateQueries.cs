using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Documents.Contracts;
using Operis_API.Shared.Contracts;
using Operis_API.Modules.Documents.Infrastructure;

namespace Operis_API.Modules.Documents.Application;

public sealed class DocumentTemplateQueries(
    OperisDbContext dbContext,
    IDocumentTemplateCache templateCache) : IDocumentTemplateQueries
{
    public async Task<PagedResult<DocumentTemplateListItem>> ListTemplatesAsync(
        DocumentTemplateListQuery query,
        CancellationToken cancellationToken)
    {
        var normalizedPage = query.Page <= 0 ? 1 : query.Page;
        var normalizedPageSize = query.PageSize <= 0 ? 10 : query.PageSize;

        var templates = await templateCache.GetTemplatesAsync(dbContext, cancellationToken);
        var filtered = templates.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();
            filtered = filtered.Where(x => x.Name.ToLowerInvariant().Contains(search));
        }

        var total = filtered.Count();
        var items = filtered
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToList();

        return new PagedResult<DocumentTemplateListItem>(items, total, normalizedPage, normalizedPageSize);
    }

    public async Task<DocumentTemplateResponse?> GetTemplateAsync(Guid templateId, CancellationToken cancellationToken)
    {
        var template = await dbContext.DocumentTemplates
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Id == templateId)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.CreatedAt,
                Items = (from item in dbContext.DocumentTemplateItems
                    join doc in dbContext.Documents on item.DocumentId equals doc.Id
                    join version in dbContext.DocumentVersions on item.DocumentVersionId equals version.Id into versions
                    from version in versions.DefaultIfEmpty()
                    where item.TemplateId == x.Id
                    orderby item.DisplayOrder
                    select new DocumentTemplateItemResponse(
                        item.DocumentId,
                        item.DocumentVersionId,
                        doc.Title,
                        version != null ? $"v{version.VersionNumber}" : null,
                        version != null ? (int?)version.VersionNumber : null))
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return template is null
            ? null
            : new DocumentTemplateResponse(
                template.Id,
                template.Name,
                template.Items.Select(item => item.DocumentId).ToList(),
                template.Items,
                template.CreatedAt);
    }

    public async Task<DocumentTemplateDocumentValidationResult> ValidateTemplateDocumentsAsync(
        IReadOnlyList<Guid> documentIds,
        CancellationToken cancellationToken)
    {
        var normalized = documentIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (normalized.Count == 0)
        {
            return new DocumentTemplateDocumentValidationResult(false, "Template documents are required.");
        }

        var documents = await dbContext.Documents
            .AsNoTracking()
            .Where(x => normalized.Contains(x.Id) && !x.IsDeleted)
            .Select(x => new { x.Id, x.CurrentVersionId })
            .ToListAsync(cancellationToken);

        if (documents.Count != normalized.Count)
        {
            return new DocumentTemplateDocumentValidationResult(false, "Document does not exist.");
        }

        if (documents.Any(x => x.CurrentVersionId is null))
        {
            return new DocumentTemplateDocumentValidationResult(false, "Document must have a current approved version.");
        }

        return new DocumentTemplateDocumentValidationResult(true, null);
    }
}
