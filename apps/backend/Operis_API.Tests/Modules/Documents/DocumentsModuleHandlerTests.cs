using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Documents;
using Operis_API.Modules.Documents.Application;
using Operis_API.Modules.Documents.Infrastructure;
using Operis_API.Shared.Security;
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
            Title = "doc-01.pdf",
            PhaseCode = "DEV",
            OwnerUserId = "user-1",
            Classification = "internal",
            RetentionClass = "standard",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var queries = new DocumentQueries(dbContext, auditLogWriter);

        var result = await InvokeListDocumentsAsync(queries);

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task ListDocumentsAsync_WithoutPermission_ReturnsForbidden()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var auditLogWriter = new FakeAuditLogWriter();
        var queries = new DocumentQueries(dbContext, auditLogWriter);

        var result = await InvokeListDocumentsAsync(queries, CreateUnprivilegedPrincipal());

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    private static async Task<IResult> InvokeListDocumentsAsync(IDocumentQueries queries, ClaimsPrincipal? principal = null)
    {
        var method = typeof(DocumentsModule).GetMethod(
            "ListDocumentsAsync",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("DocumentsModule.ListDocumentsAsync was not found.");

        var task = (Task<IResult>)method.Invoke(
            null,
            [principal ?? CreateAdminPrincipal(), new PermissionMatrix(), queries, CancellationToken.None, null, null, null, null, null, null, null, null, 1, 10])!;

        return await task;
    }

    private static ClaimsPrincipal CreateAdminPrincipal() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, "operis:super_admin")], "TestAuth"));

    private static ClaimsPrincipal CreateUnprivilegedPrincipal() =>
        new(new ClaimsIdentity([], "TestAuth"));
}
