using System.Reflection;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Documents;
using Operis_API.Modules.Documents.Application;
using Operis_API.Modules.Documents.Infrastructure;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Documents;

public sealed class DocumentsModuleHandlerTests
{
    [Fact]
    public async Task ListDocumentsAsync_ReturnsOkResult()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.Documents.Add(new DocumentEntity
        {
            Id = Guid.NewGuid(),
            FileName = "doc-01.pdf",
            UploadedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var queries = new DocumentQueries(dbContext, auditLogWriter);

        var result = await InvokeListDocumentsAsync(queries);

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    private static async Task<IResult> InvokeListDocumentsAsync(IDocumentQueries queries)
    {
        var method = typeof(DocumentsModule).GetMethod(
            "ListDocumentsAsync",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("DocumentsModule.ListDocumentsAsync was not found.");

        var task = (Task<IResult>)method.Invoke(
            null,
            [queries, CancellationToken.None])!;

        return await task;
    }
}
