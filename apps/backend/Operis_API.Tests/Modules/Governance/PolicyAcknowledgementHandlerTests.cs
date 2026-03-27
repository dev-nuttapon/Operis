using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Governance;
using Operis_API.Modules.Governance.Application;
using Operis_API.Modules.Governance.Contracts;
using Operis_API.Shared.Security;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Governance;

public sealed class PolicyAcknowledgementHandlerTests
{
    [Fact]
    public async Task ListPoliciesAsync_WithoutPermission_ReturnsForbidden()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var queries = new GovernanceOperationsQueries(dbContext, new FakeAuditLogWriter());
        var method = typeof(GovernanceModule).GetMethod("ListPoliciesAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("GovernanceModule.ListPoliciesAsync was not found.");

        var task = (Task<IResult>)method.Invoke(
            null,
            [CreateUnauthorizedPrincipal(), new PolicyListQuery(null, null, 1, 25), queries, new PermissionMatrix(), CancellationToken.None])!;

        var result = await task;
        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    private static ClaimsPrincipal CreateUnauthorizedPrincipal() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, "operis:employee"), new Claim(ClaimTypes.Email, "user@example.com")], "TestAuth"));
}
