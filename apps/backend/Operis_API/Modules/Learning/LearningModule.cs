using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Operis_API.Modules.Learning.Application;
using Operis_API.Modules.Learning.Contracts;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;

namespace Operis_API.Modules.Learning;

public sealed class LearningModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ILearningQueries, LearningQueries>();
        services.AddScoped<ILearningCommands, LearningCommands>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/learning").WithTags("Learning").RequireAuthorization();
        group.MapGet("/courses", ListTrainingCoursesAsync);
        group.MapPost("/courses", CreateTrainingCourseAsync);
        group.MapPut("/courses/{courseId:guid}", UpdateTrainingCourseAsync);
        group.MapPost("/courses/{courseId:guid}/transition", TransitionTrainingCourseAsync);
        group.MapGet("/role-matrix", ListRoleTrainingRequirementsAsync);
        group.MapPost("/role-matrix", CreateRoleTrainingRequirementAsync);
        group.MapPut("/role-matrix/{requirementId:guid}", UpdateRoleTrainingRequirementAsync);
        group.MapGet("/completions", ListTrainingCompletionsAsync);
        group.MapPost("/completions", RecordTrainingCompletionAsync);
        group.MapPut("/completions/{completionId:guid}", UpdateTrainingCompletionAsync);
        group.MapGet("/competency-reviews", ListCompetencyReviewsAsync);
        group.MapPost("/competency-reviews", CreateCompetencyReviewAsync);
        group.MapPut("/competency-reviews/{reviewId:guid}", UpdateCompetencyReviewAsync);
        group.MapGet("/project-roles", ListProjectRoleOptionsAsync);
        return endpoints;
    }

    private static async Task<IResult> ListTrainingCoursesAsync(ClaimsPrincipal principal, [AsParameters] TrainingCourseListQuery query, ILearningQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!CanRead(principal, permissionMatrix))
        {
            return Forbidden("You do not have permission to read training courses.");
        }

        return Results.Ok(await queries.ListTrainingCoursesAsync(query, cancellationToken));
    }

    private static Task<IResult> CreateTrainingCourseAsync(ClaimsPrincipal principal, CreateTrainingCourseRequest request, ILearningCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        ExecuteAsync(principal, permissionMatrix, Permissions.Learning.Manage, "You do not have permission to manage training courses.", () => commands.CreateTrainingCourseAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static Task<IResult> UpdateTrainingCourseAsync(ClaimsPrincipal principal, Guid courseId, UpdateTrainingCourseRequest request, ILearningCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        ExecuteAsync(principal, permissionMatrix, Permissions.Learning.Manage, "You do not have permission to manage training courses.", () => commands.UpdateTrainingCourseAsync(courseId, request, ResolveActor(principal), cancellationToken));

    private static Task<IResult> TransitionTrainingCourseAsync(ClaimsPrincipal principal, Guid courseId, TransitionTrainingCourseRequest request, ILearningCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        ExecuteAsync(principal, permissionMatrix, Permissions.Learning.Approve, "You do not have permission to approve training courses.", () => commands.TransitionTrainingCourseAsync(courseId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListRoleTrainingRequirementsAsync(ClaimsPrincipal principal, [AsParameters] RoleTrainingMatrixQuery query, ILearningQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!CanRead(principal, permissionMatrix))
        {
            return Forbidden("You do not have permission to read role training requirements.");
        }

        return Results.Ok(await queries.ListRoleTrainingRequirementsAsync(query, cancellationToken));
    }

    private static Task<IResult> CreateRoleTrainingRequirementAsync(ClaimsPrincipal principal, CreateRoleTrainingRequirementRequest request, ILearningCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        ExecuteAsync(principal, permissionMatrix, Permissions.Learning.Manage, "You do not have permission to manage role training requirements.", () => commands.CreateRoleTrainingRequirementAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static Task<IResult> UpdateRoleTrainingRequirementAsync(ClaimsPrincipal principal, Guid requirementId, UpdateRoleTrainingRequirementRequest request, ILearningCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        ExecuteAsync(principal, permissionMatrix, Permissions.Learning.Manage, "You do not have permission to manage role training requirements.", () => commands.UpdateRoleTrainingRequirementAsync(requirementId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListTrainingCompletionsAsync(ClaimsPrincipal principal, [AsParameters] TrainingCompletionListQuery query, ILearningQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!CanRead(principal, permissionMatrix))
        {
            return Forbidden("You do not have permission to read training completions.");
        }

        return Results.Ok(await queries.ListTrainingCompletionsAsync(query, cancellationToken));
    }

    private static Task<IResult> RecordTrainingCompletionAsync(ClaimsPrincipal principal, RecordTrainingCompletionRequest request, ILearningCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        ExecuteAsync(principal, permissionMatrix, Permissions.Learning.Manage, "You do not have permission to manage training completions.", () => commands.RecordTrainingCompletionAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static Task<IResult> UpdateTrainingCompletionAsync(ClaimsPrincipal principal, Guid completionId, UpdateTrainingCompletionRequest request, ILearningCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        ExecuteAsync(principal, permissionMatrix, Permissions.Learning.Manage, "You do not have permission to manage training completions.", () => commands.UpdateTrainingCompletionAsync(completionId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListCompetencyReviewsAsync(ClaimsPrincipal principal, [AsParameters] CompetencyReviewListQuery query, ILearningQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!CanRead(principal, permissionMatrix))
        {
            return Forbidden("You do not have permission to read competency reviews.");
        }

        return Results.Ok(await queries.ListCompetencyReviewsAsync(query, cancellationToken));
    }

    private static Task<IResult> CreateCompetencyReviewAsync(ClaimsPrincipal principal, CreateCompetencyReviewRequest request, ILearningCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        ExecuteAsync(principal, permissionMatrix, Permissions.Learning.Manage, "You do not have permission to manage competency reviews.", () => commands.CreateCompetencyReviewAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static Task<IResult> UpdateCompetencyReviewAsync(ClaimsPrincipal principal, Guid reviewId, UpdateCompetencyReviewRequest request, ILearningCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        ExecuteAsync(principal, permissionMatrix, Permissions.Learning.Manage, "You do not have permission to manage competency reviews.", () => commands.UpdateCompetencyReviewAsync(reviewId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListProjectRoleOptionsAsync(ClaimsPrincipal principal, [FromQuery] Guid? projectId, ILearningQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!CanRead(principal, permissionMatrix))
        {
            return Forbidden("You do not have permission to read training role options.");
        }

        return Results.Ok(await queries.ListProjectRoleOptionsAsync(projectId, cancellationToken));
    }

    private static bool CanRead(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix) =>
        permissionMatrix.HasAnyPermission(principal, Permissions.Learning.Read, Permissions.Learning.Manage, Permissions.Learning.Approve);

    private static async Task<IResult> ExecuteAsync<T>(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission, string forbiddenDetail, Func<Task<LearningCommandResult<T>>> action, int successStatusCode = StatusCodes.Status200OK)
    {
        if (!permissionMatrix.HasPermission(principal, permission))
        {
            return Forbidden(forbiddenDetail);
        }

        var result = await action();
        return result.Status switch
        {
            LearningCommandStatus.Success when successStatusCode == StatusCodes.Status201Created => Results.Created(string.Empty, result.Value),
            LearningCommandStatus.Success => Results.Ok(result.Value),
            LearningCommandStatus.NotFound => Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, result.ErrorCode ?? ApiErrorCodes.ResourceNotFound, "Resource not found.", result.ErrorMessage)),
            LearningCommandStatus.ValidationError => Results.BadRequest(ApiProblemDetailsFactory.Create(StatusCodes.Status400BadRequest, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Validation failed.", result.ErrorMessage)),
            LearningCommandStatus.Conflict => Results.Conflict(ApiProblemDetailsFactory.Create(StatusCodes.Status409Conflict, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Request conflict.", result.ErrorMessage)),
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
