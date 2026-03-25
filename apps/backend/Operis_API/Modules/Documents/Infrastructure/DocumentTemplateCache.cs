using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Documents.Contracts;

namespace Operis_API.Modules.Documents.Infrastructure;

public sealed class DocumentTemplateCache(IDistributedCache cache) : IDocumentTemplateCache
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    };

    private const string CacheKey = "document-templates:list";

    public Task<IReadOnlyList<DocumentTemplateListItem>> GetTemplatesAsync(
        OperisDbContext dbContext,
        CancellationToken cancellationToken) =>
        GetOrCreateAsync(() => LoadTemplatesAsync(dbContext, cancellationToken), cancellationToken);

    public async Task<int> RefreshAsync(OperisDbContext dbContext, CancellationToken cancellationToken)
    {
        var items = await LoadTemplatesAsync(dbContext, cancellationToken);
        await cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(items), CacheOptions, cancellationToken);
        return items.Count;
    }

    public Task InvalidateAsync(CancellationToken cancellationToken) =>
        cache.RemoveAsync(CacheKey, cancellationToken);

    private async Task<IReadOnlyList<DocumentTemplateListItem>> GetOrCreateAsync(
        Func<Task<IReadOnlyList<DocumentTemplateListItem>>> factory,
        CancellationToken cancellationToken)
    {
        var cached = await cache.GetStringAsync(CacheKey, cancellationToken);
        if (!string.IsNullOrWhiteSpace(cached))
        {
            var restored = JsonSerializer.Deserialize<List<DocumentTemplateListItem>>(cached);
            if (restored is not null)
            {
                return restored;
            }
        }

        var items = await factory();
        await cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(items), CacheOptions, cancellationToken);
        return items;
    }

    private static async Task<IReadOnlyList<DocumentTemplateListItem>> LoadTemplatesAsync(
        OperisDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var counts = await dbContext.DocumentTemplateItems
            .AsNoTracking()
            .GroupBy(x => x.TemplateId)
            .Select(group => new { TemplateId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.TemplateId, x => x.Count, cancellationToken);

        var templates = await dbContext.DocumentTemplates
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new { x.Id, x.Name, x.CreatedAt })
            .ToListAsync(cancellationToken);

        return templates
            .Select(x => new DocumentTemplateListItem(
                x.Id,
                x.Name,
                counts.TryGetValue(x.Id, out var count) ? count : 0,
                x.CreatedAt))
            .ToList();
    }
}
