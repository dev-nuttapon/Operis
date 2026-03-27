using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Governance;
using Operis_API.Shared.Security;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Governance;

public sealed class GovernanceOperationsHandlerTests
{
    [Fact]
    public async Task RejectWorkflowOverrideMutationAsync_ReturnsConflict()
    {
        var method = typeof(GovernanceModule).GetMethod("RejectWorkflowOverrideMutationAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("GovernanceModule.RejectWorkflowOverrideMutationAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, null)!;
        var result = await task;
        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status409Conflict, httpContext.Response.StatusCode);
    }
}
