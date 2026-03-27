using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Exceptions;
using Operis_API.Modules.Exceptions.Application;
using Operis_API.Modules.Exceptions.Contracts;
using Operis_API.Shared.Security;

namespace Operis_API.Tests.Modules.Exceptions;

public sealed class ExceptionsModuleHandlerTests
{
    [Fact]
    public async Task TransitionWaiverAsync_ApproveWithoutApprovePermission_ReturnsForbidden()
    {
        var result = await InvokeTransitionWaiverAsync(CreateExceptionManagerPrincipal(), new FakeExceptionCommands());

        var httpContext = Operis_API.Tests.Support.TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    private static async Task<IResult> InvokeTransitionWaiverAsync(ClaimsPrincipal principal, IExceptionCommands commands)
    {
        var method = typeof(ExceptionsModule).GetMethod("TransitionWaiverAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("ExceptionsModule.TransitionWaiverAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [principal, Guid.NewGuid(), new TransitionWaiverRequest("approved", "Reviewed", null), commands, new PermissionMatrix(), CancellationToken.None])!;
        return await task;
    }

    private static ClaimsPrincipal CreateExceptionManagerPrincipal() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, "operis:exception_manager")], "TestAuth"));

    private sealed class FakeExceptionCommands : IExceptionCommands
    {
        public Task<ExceptionCommandResult<WaiverDetailResponse>> CreateWaiverAsync(CreateWaiverRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<ExceptionCommandResult<WaiverDetailResponse>> UpdateWaiverAsync(Guid waiverId, UpdateWaiverRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<ExceptionCommandResult<WaiverDetailResponse>> TransitionWaiverAsync(Guid waiverId, TransitionWaiverRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}
