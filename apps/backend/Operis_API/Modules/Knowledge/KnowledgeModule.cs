using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Operis_API.Modules.Knowledge.Application;
using Operis_API.Modules.Knowledge.Contracts;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;

namespace Operis_API.Modules.Knowledge;

public sealed class KnowledgeModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IKnowledgeQueries, KnowledgeQueries>();
        services.AddScoped<IKnowledgeCommands, KnowledgeCommands>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/lessons-learned").WithTags("Knowledge").RequireAuthorization();
        group.MapGet("/", ListLessonsLearnedAsync);
        group.MapPost("/", CreateLessonLearnedAsync);
        group.MapGet("/{lessonId:guid}", GetLessonLearnedAsync);
        group.MapPut("/{lessonId:guid}", UpdateLessonLearnedAsync);
        group.MapPut("/{lessonId:guid}/publish", PublishLessonLearnedAsync);
        return endpoints;
    }

    private static async Task<IResult> ListLessonsLearnedAsync(ClaimsPrincipal principal, [AsParameters] LessonLearnedListQuery query, IKnowledgeQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Knowledge.Read))
        {
            return Forbidden("You do not have permission to read lessons learned.");
        }

        return Results.Ok(await queries.ListLessonsLearnedAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetLessonLearnedAsync(ClaimsPrincipal principal, Guid lessonId, IKnowledgeQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Knowledge.Read))
        {
            return Forbidden("You do not have permission to read lessons learned.");
        }

        var detail = await queries.GetLessonLearnedAsync(lessonId, cancellationToken);
        return detail is null
            ? Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, ApiErrorCodes.ResourceNotFound, "Lesson not found.", "Lesson not found."))
            : Results.Ok(detail);
    }

    private static Task<IResult> CreateLessonLearnedAsync(ClaimsPrincipal principal, CreateLessonLearnedRequest request, IKnowledgeCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        ExecuteAsync(principal, permissionMatrix, Permissions.Knowledge.Manage, "You do not have permission to manage lessons learned.", () => commands.CreateLessonLearnedAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static Task<IResult> UpdateLessonLearnedAsync(ClaimsPrincipal principal, Guid lessonId, UpdateLessonLearnedRequest request, IKnowledgeCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        ExecuteAsync(principal, permissionMatrix, Permissions.Knowledge.Manage, "You do not have permission to manage lessons learned.", () => commands.UpdateLessonLearnedAsync(lessonId, request, ResolveActor(principal), cancellationToken));

    private static Task<IResult> PublishLessonLearnedAsync(ClaimsPrincipal principal, Guid lessonId, PublishLessonLearnedRequest request, IKnowledgeCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        ExecuteAsync(principal, permissionMatrix, Permissions.Knowledge.Manage, "You do not have permission to publish lessons learned.", () => commands.PublishLessonLearnedAsync(lessonId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ExecuteAsync<T>(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission, string forbiddenDetail, Func<Task<KnowledgeCommandResult<T>>> action, int successStatusCode = StatusCodes.Status200OK)
    {
        if (!permissionMatrix.HasPermission(principal, permission))
        {
            return Forbidden(forbiddenDetail);
        }

        var result = await action();
        return result.Status switch
        {
            KnowledgeCommandStatus.Success when successStatusCode == StatusCodes.Status201Created => Results.Created(string.Empty, result.Value),
            KnowledgeCommandStatus.Success => Results.Ok(result.Value),
            KnowledgeCommandStatus.NotFound => Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, result.ErrorCode ?? ApiErrorCodes.ResourceNotFound, "Resource not found.", result.ErrorMessage)),
            KnowledgeCommandStatus.ValidationError => Results.BadRequest(ApiProblemDetailsFactory.Create(StatusCodes.Status400BadRequest, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Validation failed.", result.ErrorMessage)),
            KnowledgeCommandStatus.Conflict => Results.Conflict(ApiProblemDetailsFactory.Create(StatusCodes.Status409Conflict, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Request conflict.", result.ErrorMessage)),
            _ => Results.Problem(ApiProblemDetailsFactory.Create(StatusCodes.Status500InternalServerError, ApiErrorCodes.InternalFailure, "Request failed.", result.ErrorMessage))
        };
    }

    private static string? ResolveActor(ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.Email)
        ?? principal.FindFirstValue("preferred_username")
        ?? principal.FindFirstValue("sub")
        ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

    private static IResult Forbidden(string detail) =>
        Results.Json(ApiProblemDetailsFactory.Create(StatusCodes.Status403Forbidden, "forbidden", "Forbidden.", detail), statusCode: StatusCodes.Status403Forbidden);
}
