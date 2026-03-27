using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Assessment;
using Operis_API.Modules.Assessment.Application;
using Operis_API.Modules.Assessment.Contracts;
using Operis_API.Shared.Security;

namespace Operis_API.Tests.Modules.Assessment;

public sealed class AssessmentModuleHandlerTests
{
    [Fact]
    public async Task CreatePackageAsync_WithoutManagePermission_ReturnsForbidden()
    {
        var result = await InvokeCreatePackageAsync(CreateAssessmentViewerPrincipal(), new FakeAssessmentCommands());

        var httpContext = Operis_API.Tests.Support.TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task CreateFindingAsync_WithoutReviewPermission_ReturnsForbidden()
    {
        var result = await InvokeCreateFindingAsync(CreateAssessmentManagerPrincipal(), new FakeAssessmentCommands());

        var httpContext = Operis_API.Tests.Support.TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task CreateControlCatalogItemAsync_WithoutManagePermission_ReturnsForbidden()
    {
        var result = await InvokeCreateControlCatalogItemAsync(CreateAssessmentViewerPrincipal(), new FakeAssessmentCommands());

        var httpContext = Operis_API.Tests.Support.TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task CreateControlMappingAsync_WithoutManagePermission_ReturnsForbidden()
    {
        var result = await InvokeCreateControlMappingAsync(CreateAssessmentViewerPrincipal(), new FakeAssessmentCommands());

        var httpContext = Operis_API.Tests.Support.TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    private static async Task<IResult> InvokeCreatePackageAsync(ClaimsPrincipal principal, IAssessmentCommands commands)
    {
        var method = typeof(AssessmentModule).GetMethod("CreatePackageAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("AssessmentModule.CreatePackageAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [principal, new CreateAssessmentPackageRequest(Guid.NewGuid(), null, "Evidence scope"), commands, new PermissionMatrix(), CancellationToken.None])!;
        return await task;
    }

    private static async Task<IResult> InvokeCreateFindingAsync(ClaimsPrincipal principal, IAssessmentCommands commands)
    {
        var method = typeof(AssessmentModule).GetMethod("CreateFindingAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("AssessmentModule.CreateFindingAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [principal, new CreateAssessmentFindingRequest(Guid.NewGuid(), "Gap", "Need evidence", "medium", "document", Guid.NewGuid().ToString(), null, null), commands, new PermissionMatrix(), CancellationToken.None])!;
        return await task;
    }

    private static async Task<IResult> InvokeCreateControlCatalogItemAsync(ClaimsPrincipal principal, IAssessmentCommands commands)
    {
        var method = typeof(AssessmentModule).GetMethod("CreateControlCatalogItemAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("AssessmentModule.CreateControlCatalogItemAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [principal, new CreateControlCatalogItemRequest("CTRL-001", "Control title", "cmmi", "project_governance", null, null), commands, new PermissionMatrix(), CancellationToken.None])!;
        return await task;
    }

    private static async Task<IResult> InvokeCreateControlMappingAsync(ClaimsPrincipal principal, IAssessmentCommands commands)
    {
        var method = typeof(AssessmentModule).GetMethod("CreateControlMappingAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("AssessmentModule.CreateControlMappingAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [principal, new CreateControlMappingRequest(Guid.NewGuid(), null, "documents", "document", Guid.NewGuid().ToString(), "/app/documents/1", "referenced", null), commands, new PermissionMatrix(), CancellationToken.None])!;
        return await task;
    }

    private static ClaimsPrincipal CreateAssessmentViewerPrincipal() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, "operis:assessment_viewer")], "TestAuth"));

    private static ClaimsPrincipal CreateAssessmentManagerPrincipal() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, "operis:assessment_manager")], "TestAuth"));

    private sealed class FakeAssessmentCommands : IAssessmentCommands
    {
        public Task<AssessmentCommandResult<AssessmentPackageDetailResponse>> CreatePackageAsync(CreateAssessmentPackageRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<AssessmentCommandResult<AssessmentPackageDetailResponse>> TransitionPackageAsync(Guid packageId, TransitionAssessmentPackageRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<AssessmentCommandResult<AssessmentPackageNoteResponse>> AddPackageNoteAsync(Guid packageId, CreateAssessmentNoteRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<AssessmentCommandResult<AssessmentFindingDetailResponse>> CreateFindingAsync(CreateAssessmentFindingRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<AssessmentCommandResult<AssessmentFindingDetailResponse>> TransitionFindingAsync(Guid findingId, TransitionAssessmentFindingRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<AssessmentCommandResult<ControlCatalogItemResponse>> CreateControlCatalogItemAsync(CreateControlCatalogItemRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<AssessmentCommandResult<ControlCatalogItemResponse>> UpdateControlCatalogItemAsync(Guid controlId, UpdateControlCatalogItemRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<AssessmentCommandResult<ControlMappingDetailResponse>> CreateControlMappingAsync(CreateControlMappingRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<AssessmentCommandResult<ControlMappingDetailResponse>> TransitionControlMappingAsync(Guid mappingId, TransitionControlMappingRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}
