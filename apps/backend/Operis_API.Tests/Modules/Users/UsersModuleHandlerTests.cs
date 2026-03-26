using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Users;
using Operis_API.Modules.Users.Application;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Security;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Users;

public sealed class UsersModuleHandlerTests
{
    [Fact]
    public async Task CreateDepartmentAsync_WhenServiceSucceeds_ReturnsCreatedResult()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var commands = new UserReferenceDataCommands(dbContext, new FakeAuditLogWriter(), new TestReferenceDataCache());

        var result = await InvokeCreateDepartmentAsync(
            new CreateDepartmentRequest("Information Technology", 1, null),
            commands);

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status201Created, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task CreateDepartmentAsync_WhenServiceConflicts_ReturnsConflictResult()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.Departments.Add(new DepartmentEntity
        {
            Id = Guid.NewGuid(),
            Name = "Information Technology",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var commands = new UserReferenceDataCommands(dbContext, new FakeAuditLogWriter(), new TestReferenceDataCache());

        var result = await InvokeCreateDepartmentAsync(
            new CreateDepartmentRequest("Information Technology", 1, null),
            commands);

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status409Conflict, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task CreateProjectAsync_WhenProjectCodeExists_ReturnsConflictCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.Projects.Add(new ProjectEntity
        {
            Id = Guid.NewGuid(),
            Code = "PRJ-001",
            Name = "Existing Project",
            ProjectType = "Internal",
            Status = "planned",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var commands = new ProjectCommands(dbContext, new FakeAuditLogWriter(), new TestReferenceDataCache(), new FakeBusinessAuditEventWriter(), new ProjectHistoryWriter(dbContext, TestHttpContextFactory.CreateAccessor()), new FakeWorkflowInstanceCommands());

        var result = await InvokeCreateProjectAsync(
            new CreateProjectRequest(
                "PRJ-001",
                "Duplicate Project",
                "Internal",
                null,
                null,
                null,
                null,
                "planned",
                null,
                null,
                null,
                null,
                null,
                null,
                null),
            commands);

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task CreateDepartmentAsync_WithoutPermission_ReturnsForbidden()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var commands = new UserReferenceDataCommands(dbContext, new FakeAuditLogWriter(), new TestReferenceDataCache());

        var result = await InvokeCreateDepartmentAsync(
            new CreateDepartmentRequest("Information Technology", 1, null),
            commands,
            CreateUnprivilegedPrincipal());

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    private static async Task<IResult> InvokeCreateDepartmentAsync(
        CreateDepartmentRequest request,
        IUserReferenceDataCommands commands,
        ClaimsPrincipal? principal = null)
    {
        var method = typeof(UsersModule).GetMethod(
            "CreateDepartmentAsync",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("UsersModule.CreateDepartmentAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [principal ?? CreateAdminPrincipal(), new PermissionMatrix(), request, commands, CancellationToken.None])!;
        return await task;
    }

    private static async Task<IResult> InvokeCreateProjectAsync(
        CreateProjectRequest request,
        IProjectCommands commands,
        ClaimsPrincipal? principal = null)
    {
        var method = typeof(UsersModule).GetMethod(
            "CreateProjectAsync",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("UsersModule.CreateProjectAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [principal ?? CreateAdminPrincipal(), new PermissionMatrix(), request, commands, CancellationToken.None])!;
        return await task;
    }

    private static ClaimsPrincipal CreateAdminPrincipal() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, "operis:super_admin")], "TestAuth"));

    private static ClaimsPrincipal CreateUnprivilegedPrincipal() =>
        new(new ClaimsIdentity([], "TestAuth"));
}
