using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Operis_API.Modules.Verification.Application;
using Operis_API.Modules.Verification.Contracts;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;

namespace Operis_API.Modules.Verification;

public sealed class VerificationModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IVerificationQueries, VerificationQueries>();
        services.AddScoped<IVerificationCommands, VerificationCommands>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var testPlans = endpoints.MapGroup("/api/v1/test-plans")
            .WithTags("Verification")
            .RequireAuthorization();

        testPlans.MapGet("/", ListTestPlansAsync);
        testPlans.MapPost("/", CreateTestPlanAsync);
        testPlans.MapGet("/{testPlanId:guid}", GetTestPlanAsync);
        testPlans.MapPut("/{testPlanId:guid}", UpdateTestPlanAsync);
        testPlans.MapPut("/{testPlanId:guid}/submit", SubmitTestPlanAsync);
        testPlans.MapPut("/{testPlanId:guid}/approve", ApproveTestPlanAsync);
        testPlans.MapPut("/{testPlanId:guid}/baseline", BaselineTestPlanAsync);

        var testCases = endpoints.MapGroup("/api/v1/test-cases")
            .WithTags("Verification")
            .RequireAuthorization();

        testCases.MapGet("/", ListTestCasesAsync);
        testCases.MapPost("/", CreateTestCaseAsync);
        testCases.MapGet("/{testCaseId:guid}", GetTestCaseAsync);
        testCases.MapPut("/{testCaseId:guid}", UpdateTestCaseAsync);

        var testExecutions = endpoints.MapGroup("/api/v1/test-executions")
            .WithTags("Verification")
            .RequireAuthorization();

        testExecutions.MapGet("/", ListTestExecutionsAsync);
        testExecutions.MapPost("/", CreateTestExecutionAsync);
        testExecutions.MapPost("/export", ExportTestExecutionsAsync);

        var uat = endpoints.MapGroup("/api/v1/uat-signoffs")
            .WithTags("Verification")
            .RequireAuthorization();

        uat.MapGet("/", ListUatSignoffsAsync);
        uat.MapPost("/", CreateUatSignoffAsync);
        uat.MapGet("/{uatSignoffId:guid}", GetUatSignoffAsync);
        uat.MapPut("/{uatSignoffId:guid}", UpdateUatSignoffAsync);
        uat.MapPut("/{uatSignoffId:guid}/submit", SubmitUatSignoffAsync);
        uat.MapPut("/{uatSignoffId:guid}/approve", ApproveUatSignoffAsync);
        uat.MapPut("/{uatSignoffId:guid}/reject", RejectUatSignoffAsync);

        return endpoints;
    }

    private static async Task<IResult> ListTestPlansAsync(ClaimsPrincipal principal, [AsParameters] TestPlanListQuery query, IVerificationQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Verification.Read))
        {
            return Forbidden("You do not have permission to read test plans.");
        }

        return Results.Ok(await queries.ListTestPlansAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateTestPlanAsync(ClaimsPrincipal principal, CreateTestPlanRequest request, IVerificationCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Verification.Manage, "You do not have permission to manage test plans.", () => commands.CreateTestPlanAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> GetTestPlanAsync(ClaimsPrincipal principal, Guid testPlanId, IVerificationQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ReadSingleAsync(principal, permissionMatrix, Permissions.Verification.Read, "You do not have permission to read test plans.", () => queries.GetTestPlanAsync(testPlanId, cancellationToken));

    private static async Task<IResult> UpdateTestPlanAsync(ClaimsPrincipal principal, Guid testPlanId, UpdateTestPlanRequest request, IVerificationCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Verification.Manage, "You do not have permission to manage test plans.", () => commands.UpdateTestPlanAsync(testPlanId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> SubmitTestPlanAsync(ClaimsPrincipal principal, Guid testPlanId, IVerificationCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Verification.Manage, "You do not have permission to submit test plans.", () => commands.SubmitTestPlanAsync(testPlanId, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ApproveTestPlanAsync(ClaimsPrincipal principal, Guid testPlanId, VerificationDecisionRequest request, IVerificationCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Verification.Approve, "You do not have permission to approve test plans.", () => commands.ApproveTestPlanAsync(testPlanId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> BaselineTestPlanAsync(ClaimsPrincipal principal, Guid testPlanId, VerificationDecisionRequest request, IVerificationCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Verification.Approve, "You do not have permission to baseline test plans.", () => commands.BaselineTestPlanAsync(testPlanId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListTestCasesAsync(ClaimsPrincipal principal, [AsParameters] TestCaseListQuery query, IVerificationQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Verification.Read))
        {
            return Forbidden("You do not have permission to read test cases.");
        }

        return Results.Ok(await queries.ListTestCasesAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateTestCaseAsync(ClaimsPrincipal principal, CreateTestCaseRequest request, IVerificationCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Verification.Manage, "You do not have permission to manage test cases.", () => commands.CreateTestCaseAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> GetTestCaseAsync(ClaimsPrincipal principal, Guid testCaseId, IVerificationQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Verification.Read))
        {
            return Forbidden("You do not have permission to read test cases.");
        }

        var item = await queries.GetTestCaseAsync(testCaseId, permissionMatrix.HasPermission(principal, Permissions.Verification.ReadSensitiveEvidence), cancellationToken);
        return item is null ? NotFound() : Results.Ok(item);
    }

    private static async Task<IResult> UpdateTestCaseAsync(ClaimsPrincipal principal, Guid testCaseId, UpdateTestCaseRequest request, IVerificationCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Verification.Manage, "You do not have permission to manage test cases.", () => commands.UpdateTestCaseAsync(testCaseId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListTestExecutionsAsync(ClaimsPrincipal principal, [AsParameters] TestExecutionListQuery query, IVerificationQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Verification.Read))
        {
            return Forbidden("You do not have permission to read test executions.");
        }

        return Results.Ok(await queries.ListTestExecutionsAsync(query, permissionMatrix.HasPermission(principal, Permissions.Verification.ReadSensitiveEvidence), cancellationToken));
    }

    private static async Task<IResult> CreateTestExecutionAsync(ClaimsPrincipal principal, CreateTestExecutionRequest request, IVerificationCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (request.IsSensitiveEvidence && LacksPermission(principal, permissionMatrix, Permissions.Verification.ReadSensitiveEvidence))
        {
            return Forbidden("You do not have permission to record sensitive evidence.");
        }

        return await ExecuteAsync(principal, permissionMatrix, Permissions.Verification.Manage, "You do not have permission to record test executions.", () => commands.CreateTestExecutionAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);
    }

    private static async Task<IResult> ExportTestExecutionsAsync(ClaimsPrincipal principal, ExecutionExportRequest request, IVerificationCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Verification.Export, "You do not have permission to export test executions.", () => commands.ExportTestExecutionsAsync(request, permissionMatrix.HasPermission(principal, Permissions.Verification.ReadSensitiveEvidence), ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListUatSignoffsAsync(ClaimsPrincipal principal, [AsParameters] UatSignoffListQuery query, IVerificationQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasAnyPermission(principal, Permissions.Verification.Read, Permissions.Verification.SubmitUat, Permissions.Verification.Approve))
        {
            return Forbidden("You do not have permission to read UAT sign-offs.");
        }

        return Results.Ok(await queries.ListUatSignoffsAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateUatSignoffAsync(ClaimsPrincipal principal, CreateUatSignoffRequest request, IVerificationCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Verification.SubmitUat, "You do not have permission to create UAT sign-offs.", () => commands.CreateUatSignoffAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> GetUatSignoffAsync(ClaimsPrincipal principal, Guid uatSignoffId, IVerificationQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasAnyPermission(principal, Permissions.Verification.Read, Permissions.Verification.SubmitUat, Permissions.Verification.Approve))
        {
            return Forbidden("You do not have permission to read UAT sign-offs.");
        }

        var item = await queries.GetUatSignoffAsync(uatSignoffId, cancellationToken);
        return item is null ? NotFound() : Results.Ok(item);
    }

    private static async Task<IResult> UpdateUatSignoffAsync(ClaimsPrincipal principal, Guid uatSignoffId, UpdateUatSignoffRequest request, IVerificationCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Verification.SubmitUat, "You do not have permission to update UAT sign-offs.", () => commands.UpdateUatSignoffAsync(uatSignoffId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> SubmitUatSignoffAsync(ClaimsPrincipal principal, Guid uatSignoffId, IVerificationCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Verification.SubmitUat, "You do not have permission to submit UAT sign-offs.", () => commands.SubmitUatSignoffAsync(uatSignoffId, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ApproveUatSignoffAsync(ClaimsPrincipal principal, Guid uatSignoffId, VerificationDecisionRequest request, IVerificationCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Verification.Approve, "You do not have permission to approve UAT sign-offs.", () => commands.ApproveUatSignoffAsync(uatSignoffId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> RejectUatSignoffAsync(ClaimsPrincipal principal, Guid uatSignoffId, VerificationDecisionRequest request, IVerificationCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Verification.Approve, "You do not have permission to reject UAT sign-offs.", () => commands.RejectUatSignoffAsync(uatSignoffId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ReadSingleAsync<T>(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission, string forbiddenDetail, Func<Task<T?>> loader)
        where T : class
    {
        if (LacksPermission(principal, permissionMatrix, permission))
        {
            return Forbidden(forbiddenDetail);
        }

        var item = await loader();
        return item is null ? NotFound() : Results.Ok(item);
    }

    private static async Task<IResult> ExecuteAsync<T>(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission, string forbiddenDetail, Func<Task<VerificationCommandResult<T>>> action, int successStatusCode = StatusCodes.Status200OK)
    {
        if (LacksPermission(principal, permissionMatrix, permission))
        {
            return Forbidden(forbiddenDetail);
        }

        var result = await action();
        return result.Status switch
        {
            VerificationCommandStatus.Success when successStatusCode == StatusCodes.Status201Created => Results.Created(string.Empty, result.Value),
            VerificationCommandStatus.Success => Results.Ok(result.Value),
            VerificationCommandStatus.NotFound => Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, result.ErrorCode ?? ApiErrorCodes.ResourceNotFound, "Resource not found.", result.ErrorMessage)),
            VerificationCommandStatus.ValidationError => Results.BadRequest(ApiProblemDetailsFactory.Create(StatusCodes.Status400BadRequest, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Validation failed.", result.ErrorMessage)),
            VerificationCommandStatus.Conflict => Results.Conflict(ApiProblemDetailsFactory.Create(StatusCodes.Status409Conflict, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Request conflict.", result.ErrorMessage)),
            _ => Results.Problem(ApiProblemDetailsFactory.Create(StatusCodes.Status500InternalServerError, ApiErrorCodes.InternalFailure, "Request failed.", result.ErrorMessage))
        };
    }

    private static bool LacksPermission(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission) =>
        !permissionMatrix.HasPermission(principal, permission);

    private static string? ResolveActor(ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.Email)
        ?? principal.FindFirstValue("preferred_username")
        ?? principal.FindFirstValue("sub")
        ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

    private static IResult Forbidden(string detail) =>
        Results.Json(ApiProblemDetailsFactory.Create(StatusCodes.Status403Forbidden, "forbidden", "Forbidden.", detail), statusCode: StatusCodes.Status403Forbidden);

    private static IResult NotFound() =>
        Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, ApiErrorCodes.ResourceNotFound, "Resource not found.", null));
}
