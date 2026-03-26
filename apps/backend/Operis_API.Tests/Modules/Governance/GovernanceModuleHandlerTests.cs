using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Governance;
using Operis_API.Modules.Governance.Application;
using Operis_API.Modules.Governance.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Security;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Governance;

public sealed class GovernanceModuleHandlerTests
{
    [Fact]
    public async Task ApproveTailoringRecordAsync_WithoutApprovePermission_ReturnsForbidden()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = Guid.NewGuid();
        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = "PRJ-100",
            Name = "Tailoring Project",
            ProjectType = "Internal",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });
        dbContext.Set<TailoringRecordEntity>().Add(new TailoringRecordEntity
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            ProjectId = projectId,
            RequesterUserId = "pm@example.com",
            RequestedChange = "Change workflow",
            Reason = "Pilot",
            ImpactSummary = "Low",
            Status = "submitted",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var commands = new GovernanceCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter());
        var result = await InvokeApproveTailoringAsync(commands, CreatePmPrincipal());

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    private static async Task<IResult> InvokeApproveTailoringAsync(IGovernanceCommands commands, ClaimsPrincipal principal)
    {
        var method = typeof(GovernanceModule).GetMethod("ApproveTailoringRecordAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("GovernanceModule.ApproveTailoringRecordAsync was not found.");

        var task = (Task<IResult>)method.Invoke(
            null,
            [principal, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new Operis_API.Modules.Governance.Contracts.TailoringDecisionRequest("approved", "No reason"), commands, new PermissionMatrix(), CancellationToken.None])!;

        return await task;
    }

    private static ClaimsPrincipal CreatePmPrincipal() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, "operis:pm"), new Claim(ClaimTypes.Email, "pm@example.com")], "TestAuth"));
}
