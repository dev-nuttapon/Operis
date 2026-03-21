using Microsoft.AspNetCore.Mvc;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;
using System.Security.Claims;

namespace Operis_API.Modules.Notifications;

public sealed class NotificationsModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<INotificationQueries, NotificationQueries>();
        services.AddScoped<INotificationCommands, NotificationCommands>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/notifications")
            .WithTags("Notifications")
            .RequireAuthorization();

        group.MapGet("/", ListNotificationsAsync)
            .WithName("Notifications_List");
        group.MapGet("/{notificationId:guid}", GetNotificationAsync)
            .WithName("Notifications_Get");
        group.MapPost("/{notificationId:guid}/read", MarkNotificationReadAsync)
            .WithName("Notifications_MarkRead");
        group.MapPost("/read-all", MarkAllReadAsync)
            .WithName("Notifications_MarkAllRead");
        group.MapPost("/seed", SeedNotificationsAsync)
            .WithName("Notifications_Seed");

        return endpoints;
    }

    private static async Task<IResult> ListNotificationsAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        INotificationQueries queries,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? unreadOnly = null)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Notifications.Read))
        {
            return Results.Forbid();
        }

        var currentUserId = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await queries.ListAsync(new NotificationListQuery(page, pageSize, unreadOnly), currentUserId, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetNotificationAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        INotificationQueries queries,
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Notifications.Read))
        {
            return Results.Forbid();
        }

        var currentUserId = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var item = await queries.GetByIdAsync(notificationId, currentUserId, cancellationToken);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }

    private static async Task<IResult> MarkNotificationReadAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        INotificationCommands commands,
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Notifications.Read))
        {
            return Results.Forbid();
        }

        var currentUserId = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await commands.MarkReadAsync(notificationId, currentUserId, cancellationToken);

        if (result.NotFound)
        {
            return Results.NotFound();
        }

        return result.Succeeded
            ? Results.Ok(new { updated = result.UpdatedCount })
            : Results.BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
    }

    private static async Task<IResult> MarkAllReadAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        INotificationCommands commands,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Notifications.Read))
        {
            return Results.Forbid();
        }

        var currentUserId = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await commands.MarkAllReadAsync(currentUserId, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { updated = result.UpdatedCount })
            : Results.BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
    }

    private static async Task<IResult> SeedNotificationsAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        INotificationCommands commands,
        IHostEnvironment environment,
        NotificationSeedRequest request,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Notifications.Read))
        {
            return Results.Forbid();
        }

        if (!environment.IsDevelopment() && !environment.IsEnvironment("Local"))
        {
            return Results.NotFound();
        }

        var currentUserId = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await commands.SeedAsync(currentUserId, request.Count, cancellationToken);

        return result.Succeeded
            ? Results.Ok(new { inserted = result.UpdatedCount })
            : Results.BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
    }
}
