using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Requirements.Application;
using Operis_API.Modules.Verification;
using Operis_API.Modules.Verification.Application;
using Operis_API.Modules.Verification.Contracts;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Security;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Verification;

public sealed class VerificationModuleHandlerTests
{
    [Fact]
    public async Task CreateTestPlanAsync_WithoutManagePermission_ReturnsForbidden()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = Guid.NewGuid();
        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = "PRJ-VER",
            Name = "Verification Project",
            ProjectType = "Internal",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var requirementQueries = new RequirementQueries(dbContext);
        var commands = new VerificationCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new VerificationQueries(dbContext), new RequirementCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), requirementQueries));
        var result = await InvokeCreateTestPlanAsync(commands, projectId, CreateVerificationViewerPrincipal());

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    private static async Task<IResult> InvokeCreateTestPlanAsync(IVerificationCommands commands, Guid projectId, ClaimsPrincipal principal)
    {
        var method = typeof(VerificationModule).GetMethod("CreateTestPlanAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("VerificationModule.CreateTestPlanAsync was not found.");

        var request = new CreateTestPlanRequest(projectId, "TP-001", "System test plan", "Scope", "qa@example.com", "Entry", "Exit", []);
        var task = (Task<IResult>)method.Invoke(null, [principal, request, commands, new PermissionMatrix(), CancellationToken.None])!;
        return await task;
    }

    private static ClaimsPrincipal CreateVerificationViewerPrincipal() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, "operis:verification_viewer")], "TestAuth"));
}
