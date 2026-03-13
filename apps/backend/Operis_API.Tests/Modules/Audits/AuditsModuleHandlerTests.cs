using System.Reflection;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Audits;
using Operis_API.Modules.Audits.Application;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Audits;

public sealed class AuditsModuleHandlerTests
{
    [Fact]
    public async Task ListAuditLogsAsync_ReturnsOkResult()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.AuditLogs.Add(new Shared.Auditing.AuditLogEntity
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
        var queries = new AuditLogQueries(dbContext, auditLogWriter);

        var result = await InvokeListAuditLogsAsync(queries);

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    private static async Task<IResult> InvokeListAuditLogsAsync(IAuditLogQueries queries)
    {
        var method = typeof(AuditsModule).GetMethod(
            "ListAuditLogsAsync",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("AuditsModule.ListAuditLogsAsync was not found.");

        var task = (Task<IResult>)method.Invoke(
            null,
            [queries, null, null, null, null, null, null, null, null, null, null, 1, 10, CancellationToken.None])!;

        return await task;
    }
}
