using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Documents.Domain;
using Operis_API.Shared.Auditing;
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
            .WithTags("Documents")
            .RequireAuthorization();

        group.MapGet("/", async (OperisDbContext dbContext, IAuditLogWriter auditLogWriter, CancellationToken cancellationToken) =>
            {
                var items = await dbContext.Documents
                    .AsNoTracking()
                    .OrderByDescending(x => x.UploadedAt)
                    .Take(50)
                    .Select(x => new DocumentListItem(x.Id, x.FileName, x.UploadedAt))
                    .ToListAsync(cancellationToken);

                auditLogWriter.Append(new AuditLogEntry(
                    Module: "documents",
                    Action: "list",
                    EntityType: "document",
                    StatusCode: StatusCodes.Status200OK,
                    Metadata: new { count = items.Count }));
                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.Ok(items);
            })
            .WithName("Documents_List");

        return endpoints;
    }
}
