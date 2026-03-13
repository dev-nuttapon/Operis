using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Documents.Application;
using Operis_API.Modules.Documents.Infrastructure;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Documents.Application;

public sealed class DocumentQueriesTests
{
    [Fact]
    public async Task ListDocumentsAsync_ReturnsLatest50InDescendingOrder()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var now = DateTimeOffset.UtcNow;
        dbContext.Documents.AddRange(
            Enumerable.Range(1, 60).Select(index => new DocumentEntity
            {
                Id = Guid.NewGuid(),
                FileName = $"doc-{index:D2}.pdf",
                UploadedAt = now.AddMinutes(-index)
            }));
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var sut = new DocumentQueries(dbContext, auditLogWriter);

        var result = await sut.ListDocumentsAsync(CancellationToken.None);

        Assert.Equal(50, result.Count);
        Assert.Equal("doc-01.pdf", result[0].FileName);
        Assert.Equal("doc-50.pdf", result[^1].FileName);
        Assert.Single(auditLogWriter.Entries);
    }
}
