using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Documents.Contracts;
using Operis_API.Shared.Contracts;
using Operis_API.Modules.Documents.Infrastructure;

namespace Operis_API.Modules.Documents.Application;

public sealed class DocumentTemplateQueries(OperisDbContext dbContext) : IDocumentTemplateQueries
{
    public async Task<PagedResult<DocumentTemplateListItem>> ListTemplatesAsync(
        DocumentTemplateListQuery query,
        CancellationToken cancellationToken)
    {
        var normalizedPage = query.Page <= 0 ? 1 : query.Page;
        var normalizedPageSize = query.PageSize <= 0 ? 10 : query.PageSize;

        var templateQuery = dbContext.DocumentTemplates
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            templateQuery = templateQuery.Where(x => EF.Functions.ILike(x.Name, $"%{search}%"));
        }

        var total = await templateQuery.CountAsync(cancellationToken);

        var items = await templateQuery
            .OrderByDescending(x => x.CreatedAt)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(x => new DocumentTemplateListItem(
                x.Id,
                x.Name,
                dbContext.DocumentTemplateItems.Count(item => item.TemplateId == x.Id),
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<DocumentTemplateListItem>(items, total, normalizedPage, normalizedPageSize);
    }

    public async Task<DocumentTemplateResponse?> GetTemplateAsync(Guid templateId, CancellationToken cancellationToken)
    {
        var template = await dbContext.DocumentTemplates
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Id == templateId)
            .Select(x => new DocumentTemplateResponse(
                x.Id,
                x.Name,
                dbContext.DocumentTemplateItems
                    .Where(item => item.TemplateId == x.Id)
                    .OrderBy(item => item.DisplayOrder)
                    .Select(item => item.DocumentId)
                    .ToList(),
                x.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return template;
    }
}
