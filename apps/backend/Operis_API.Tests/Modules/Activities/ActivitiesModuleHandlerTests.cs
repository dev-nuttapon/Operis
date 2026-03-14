using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Activities;
using Operis_API.Modules.Activities.Application;
using Operis_API.Shared.Security;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Activities;

public sealed class ActivitiesModuleHandlerTests
{
    [Fact]
    public async Task ListActivityLogsAsync_ReturnsOkResult()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.ActivityLogs.Add(new Operis_API.Shared.ActivityLogging.ActivityLogEntity
        {
            Id = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.UtcNow,
            Module = "users",
            Action = "create",
            EntityType = "user",
            Status = "success",
            Source = "api",
            ActorType = "user",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var queries = new ActivityLogQueries(dbContext, auditLogWriter);

        var result = await InvokeListActivityLogsAsync(queries);

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task ListActivityLogsAsync_WithoutPermission_ReturnsForbidden()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var auditLogWriter = new FakeAuditLogWriter();
        var queries = new ActivityLogQueries(dbContext, auditLogWriter);

        var result = await InvokeListActivityLogsAsync(queries, CreateUnprivilegedPrincipal());

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    private static async Task<IResult> InvokeListActivityLogsAsync(IActivityLogQueries queries, ClaimsPrincipal? principal = null)
    {
        var method = typeof(ActivitiesModule).GetMethod(
            "ListActivityLogsAsync",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("ActivitiesModule.ListActivityLogsAsync was not found.");

        var task = (Task<IResult>)method.Invoke(
            null,
            [principal ?? CreateAdminPrincipal(), new PermissionMatrix(), queries, null, null, null, null, null, null, null, null, null, null, 1, 10, CancellationToken.None])!;

        return await task;
    }

    private static ClaimsPrincipal CreateAdminPrincipal() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, "operis:super_admin")], "TestAuth"));

    private static ClaimsPrincipal CreateUnprivilegedPrincipal() =>
        new(new ClaimsIdentity([], "TestAuth"));
}
