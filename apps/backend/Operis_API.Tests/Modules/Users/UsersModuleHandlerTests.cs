using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Users;
using Operis_API.Modules.Users.Application;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Security;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Users;

public sealed class UsersModuleHandlerTests
{
    [Fact]
    public async Task CreateDepartmentAsync_WhenServiceSucceeds_ReturnsCreatedResult()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var auditLogWriter = new FakeAuditLogWriter();
        var referenceDataCache = new TestReferenceDataCache();
        var commands = new UserReferenceDataCommands(dbContext, auditLogWriter, referenceDataCache);

        var result = await InvokeCreateDepartmentAsync(
            new CreateDepartmentRequest("Quality", 1, null),
            commands);

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status201Created, httpContext.Response.StatusCode);
        Assert.StartsWith("/api/v1/users/departments/", httpContext.Response.Headers.Location.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateDepartmentAsync_WhenServiceConflicts_ReturnsConflictResult()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.Departments.Add(new DepartmentEntity
        {
            Id = Guid.NewGuid(),
            Name = "Quality",
            DisplayOrder = 1,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var referenceDataCache = new TestReferenceDataCache();
        var commands = new UserReferenceDataCommands(dbContext, auditLogWriter, referenceDataCache);

        var result = await InvokeCreateDepartmentAsync(
            new CreateDepartmentRequest("Quality", 1, null),
            commands);

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status409Conflict, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task CreateDepartmentAsync_WithoutPermission_ReturnsForbidden()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var auditLogWriter = new FakeAuditLogWriter();
        var referenceDataCache = new TestReferenceDataCache();
        var commands = new UserReferenceDataCommands(dbContext, auditLogWriter, referenceDataCache);

        var result = await InvokeCreateDepartmentAsync(
            new CreateDepartmentRequest("Quality", 1, null),
            commands,
            CreateUnprivilegedPrincipal());

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    private static async Task<IResult> InvokeCreateDepartmentAsync(CreateDepartmentRequest request, IUserReferenceDataCommands commands, ClaimsPrincipal? principal = null)
    {
        var method = typeof(UsersModule).GetMethod(
            "CreateDepartmentAsync",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("UsersModule.CreateDepartmentAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [principal ?? CreateAdminPrincipal(), new PermissionMatrix(), request, commands, CancellationToken.None])!;
        return await task;
    }

    private static ClaimsPrincipal CreateAdminPrincipal() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, "operis:super_admin")], "TestAuth"));

    private static ClaimsPrincipal CreateUnprivilegedPrincipal() =>
        new(new ClaimsIdentity([], "TestAuth"));
}
