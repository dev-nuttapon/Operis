using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Documents.Application;
using Operis_API.Modules.Documents.Domain;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Modules;

namespace Operis_API.Modules.Documents;

public sealed class DocumentsModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDocumentQueries, DocumentQueries>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/documents")
            .WithTags("Documents")
            .RequireAuthorization();

        group.MapGet("/", ListDocumentsAsync)
            .WithName("Documents_List");

        return endpoints;
    }

    private static async Task<IResult> ListDocumentsAsync(
        IDocumentQueries queries,
        CancellationToken cancellationToken)
    {
        var items = await queries.ListDocumentsAsync(cancellationToken);
        return Results.Ok(items);
    }
}
