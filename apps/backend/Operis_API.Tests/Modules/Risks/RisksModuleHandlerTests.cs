using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Risks;
using Operis_API.Modules.Risks.Application;
using Operis_API.Modules.Risks.Contracts;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Security;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Risks;

public sealed class RisksModuleHandlerTests
{
    [Fact]
    public async Task CreateIssueAsync_SensitiveWithoutPermission_ReturnsForbidden()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = Guid.NewGuid();
        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = "PRJ-RISK",
            Name = "Risk Project",
            ProjectType = "Internal",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var commands = new RiskCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new RiskQueries(dbContext));
        var result = await InvokeCreateIssueAsync(commands, projectId, CreateManagerWithoutSensitivePermission());

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    private static async Task<IResult> InvokeCreateIssueAsync(IRiskCommands commands, Guid projectId, ClaimsPrincipal principal)
    {
        var method = typeof(RisksModule).GetMethod(
            "CreateIssueAsync",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("RisksModule.CreateIssueAsync was not found.");

        var request = new CreateIssueRequest(projectId, "ISSUE-SEC", "Sensitive issue", "Description", "owner@example.com", null, "high", null, null, true, "incident_linked");
        var task = (Task<IResult>)method.Invoke(
            null,
            [principal, request, commands, new PermissionMatrix(), CancellationToken.None])!;

        return await task;
    }

    private static ClaimsPrincipal CreateManagerWithoutSensitivePermission() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, "operis:risk_viewer")], "TestAuth"));
}
