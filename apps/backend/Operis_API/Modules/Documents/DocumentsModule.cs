using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Documents.Domain;
using Operis_API.Shared.Modules;

namespace Operis_API.Modules.Documents;

public sealed class DocumentsModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/documents")
            .WithTags("Documents");

        group.MapGet("/", async (OperisDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var items = await dbContext.Documents
                    .AsNoTracking()
                    .OrderByDescending(x => x.UploadedAt)
                    .Take(50)
                    .Select(x => new DocumentListItem(x.Id, x.FileName, x.UploadedAt))
                    .ToListAsync(cancellationToken);

                return Results.Ok(items);
            })
            .WithName("Documents_List");

        return endpoints;
    }
}
