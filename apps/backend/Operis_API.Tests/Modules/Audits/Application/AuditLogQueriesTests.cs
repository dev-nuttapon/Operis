using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Audits.Application;
using Operis_API.Shared.Auditing;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Audits.Application;

public sealed class AuditLogQueriesTests
{
    [Fact]
    public async Task ListAuditLogsAsync_ReturnsPagedItemsSortedByOccurredAtDescendingByDefault()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.AuditLogs.AddRange(
            new AuditLogEntity
            {
                Id = Guid.NewGuid(),
                OccurredAt = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero),
                Module = "users",
                Action = "create",
                EntityType = "user",
                Status = "success",
                Source = "api",
                ActorType = "user",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new AuditLogEntity
            {
                Id = Guid.NewGuid(),
                OccurredAt = new DateTimeOffset(2026, 3, 2, 0, 0, 0, TimeSpan.Zero),
                Module = "documents",
                Action = "list",
                EntityType = "document",
                Status = "success",
                Source = "api",
                ActorType = "user",
                CreatedAt = DateTimeOffset.UtcNow
            });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var sut = new AuditLogQueries(dbContext, auditLogWriter);

        var result = await sut.ListAuditLogsAsync(
            new AuditLogListQuery(null, null, null, null, null, null, null, "desc", null, null, 1, 10),
            CancellationToken.None);

        Assert.Equal(2, result.Total);
        Assert.Equal("documents", result.Items[0].Module);
        Assert.Equal("users", result.Items[1].Module);
        Assert.Single(auditLogWriter.Entries);
    }
}
