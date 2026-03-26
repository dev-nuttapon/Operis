using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.ChangeControl;
using Operis_API.Modules.ChangeControl.Application;
using Operis_API.Modules.ChangeControl.Contracts;
using Operis_API.Modules.ChangeControl.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Security;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.ChangeControl;

public sealed class ChangeControlModuleHandlerTests
{
    [Fact]
    public async Task CreateBaselineRegistryAsync_WithoutPermission_ReturnsForbidden()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = Guid.NewGuid();
        var changeRequestId = Guid.NewGuid();
        var requirementBaselineId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = "PRJ-CC",
            Name = "Change Control Project",
            ProjectType = "Internal",
            Status = "active",
            CreatedAt = now
        });
        dbContext.Set<Operis_API.Modules.Requirements.Infrastructure.RequirementBaselineEntity>().Add(new Operis_API.Modules.Requirements.Infrastructure.RequirementBaselineEntity
        {
            Id = requirementBaselineId,
            ProjectId = projectId,
            BaselineName = "REQ BL",
            RequirementIdsJson = "[]",
            ApprovedBy = "approver@example.com",
            ApprovedAt = now,
            Status = "locked"
        });
        dbContext.ChangeRequests.Add(new ChangeRequestEntity
        {
            Id = changeRequestId,
            ProjectId = projectId,
            Code = "CR-401",
            Title = "Approved CR",
            RequestedBy = "pm@example.com",
            Reason = "Change",
            Status = "approved",
            Priority = "high",
            CreatedAt = now,
            UpdatedAt = now
        });
        dbContext.ChangeImpacts.Add(new ChangeImpactEntity
        {
            Id = Guid.NewGuid(),
            ChangeRequestId = changeRequestId,
            ScopeImpact = "Scope",
            ScheduleImpact = "Schedule",
            QualityImpact = "Quality",
            SecurityImpact = "Security",
            PerformanceImpact = "Performance",
            RiskImpact = "Risk"
        });
        await dbContext.SaveChangesAsync();

        var commands = new ChangeControlCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new ChangeControlQueries(dbContext));
        var result = await InvokeCreateBaselineRegistryAsync(commands, new CreateBaselineRegistryRequest(projectId, "BL-401", "requirements", "requirement_baseline", requirementBaselineId.ToString(), changeRequestId), CreateUnprivilegedPrincipal());

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    private static async Task<IResult> InvokeCreateBaselineRegistryAsync(IChangeControlCommands commands, CreateBaselineRegistryRequest request, ClaimsPrincipal principal)
    {
        var method = typeof(ChangeControlModule).GetMethod("CreateBaselineRegistryAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("ChangeControlModule.CreateBaselineRegistryAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [principal, request, commands, new PermissionMatrix(), CancellationToken.None])!;
        return await task;
    }

    private static ClaimsPrincipal CreateUnprivilegedPrincipal() =>
        new(new ClaimsIdentity([], "TestAuth"));
}
