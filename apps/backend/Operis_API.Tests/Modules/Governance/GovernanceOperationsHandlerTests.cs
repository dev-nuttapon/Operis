using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Governance;
using Operis_API.Modules.Governance.Application;
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

    [Fact]
    public async Task GetComplianceDashboardAsync_WithoutPermission_ReturnsForbidden()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var queries = new GovernanceOperationsQueries(dbContext, new FakeAuditLogWriter());
        var method = typeof(GovernanceModule).GetMethod("GetComplianceDashboardAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("GovernanceModule.GetComplianceDashboardAsync was not found.");

        var task = (Task<IResult>)method.Invoke(
            null,
            [CreateUnauthorizedPrincipal(), new Operis_API.Modules.Governance.Contracts.ComplianceDashboardQuery(null, null, 30, false), queries, new PermissionMatrix(), CancellationToken.None])!;

        var result = await task;
        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task ListManagementReviewsAsync_WithoutPermission_ReturnsForbidden()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var queries = new GovernanceOperationsQueries(dbContext, new FakeAuditLogWriter());
        var method = typeof(GovernanceModule).GetMethod("ListManagementReviewsAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("GovernanceModule.ListManagementReviewsAsync was not found.");

        var task = (Task<IResult>)method.Invoke(
            null,
            [CreateUnauthorizedPrincipal(), new Operis_API.Modules.Governance.Contracts.ManagementReviewListQuery(null, null, null, null, null, null, 1, 25), queries, new PermissionMatrix(), CancellationToken.None])!;

        var result = await task;
        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    private static ClaimsPrincipal CreateUnauthorizedPrincipal() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, "operis:employee"), new Claim(ClaimTypes.Email, "user@example.com")], "TestAuth"));
}
