using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Operis_API.Infrastructure.Persistence;

namespace Operis_API.Modules.Workflows.Infrastructure;

public sealed class WorkflowDefinitionCache(IDistributedCache cache) : IWorkflowDefinitionCache
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    };

    private const string CacheKey = "workflows:definitions";

    public Task<IReadOnlyList<WorkflowDefinitionContract>> GetDefinitionsAsync(
        OperisDbContext dbContext,
        CancellationToken cancellationToken) =>
        GetOrCreateAsync(() => LoadDefinitionsAsync(dbContext, cancellationToken), cancellationToken);

    public async Task<int> RefreshAsync(OperisDbContext dbContext, CancellationToken cancellationToken)
    {
        var definitions = await LoadDefinitionsAsync(dbContext, cancellationToken);
        await cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(definitions), CacheOptions, cancellationToken);
        return definitions.Count;
    }

    public Task InvalidateAsync(CancellationToken cancellationToken) =>
        cache.RemoveAsync(CacheKey, cancellationToken);

    private async Task<IReadOnlyList<WorkflowDefinitionContract>> GetOrCreateAsync(
        Func<Task<IReadOnlyList<WorkflowDefinitionContract>>> factory,
        CancellationToken cancellationToken)
    {
        var cached = await cache.GetStringAsync(CacheKey, cancellationToken);
        if (!string.IsNullOrWhiteSpace(cached))
        {
            var restored = JsonSerializer.Deserialize<List<WorkflowDefinitionContract>>(cached);
            if (restored is not null)
            {
                return restored;
            }
        }

        var items = await factory();
        await cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(items), CacheOptions, cancellationToken);
        return items;
    }

    private static async Task<IReadOnlyList<WorkflowDefinitionContract>> LoadDefinitionsAsync(
        OperisDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return await dbContext.WorkflowDefinitions
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new WorkflowDefinitionContract(
                x.Id,
                x.Code,
                x.Name,
                x.Status,
                x.DocumentTemplateId))
            .ToListAsync(cancellationToken);
    }
}
