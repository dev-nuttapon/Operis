using System.Security.Claims;
using Operis_API.Modules.Activities.Application;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;

namespace Operis_API.Modules.Activities;

public sealed class ActivitiesModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IActivityLogQueries, ActivityLogQueries>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/activity-logs")
            .WithTags("Activity Logs")
            .RequireAuthorization();

        group.MapGet("/", ListActivityLogsAsync)
            .WithName("ActivityLogs_List");

        return endpoints;
    }

    private static async Task<IResult> ListActivityLogsAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IActivityLogQueries queries,
        string? module,
        string? action,
        string? entityType,
        string? entityId,
        string? actor,
        string? status,
        string? sortBy,
        string? sortOrder,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.ActivityLogs.Read))
        {
            return Results.Forbid();
        }

        var result = await queries.ListActivityLogsAsync(
            new ActivityLogListQuery(module, action, entityType, entityId, actor, status, sortBy, sortOrder, from, to, page, pageSize),
            cancellationToken);
        return Results.Ok(result);
    }
}
