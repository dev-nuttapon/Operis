using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Meetings;
using Operis_API.Modules.Meetings.Application;
using Operis_API.Modules.Meetings.Contracts;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Security;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Meetings;

public sealed class MeetingsModuleHandlerTests
{
    [Fact]
    public async Task CreateMeetingAsync_RestrictedWithoutPermission_ReturnsForbidden()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = Guid.NewGuid();
        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = "PRJ-MTG",
            Name = "Meeting Project",
            ProjectType = "Internal",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var commands = new MeetingCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new MeetingQueries(dbContext));
        var result = await InvokeCreateMeetingAsync(commands, projectId, CreateMeetingViewerPrincipal());

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    private static async Task<IResult> InvokeCreateMeetingAsync(IMeetingCommands commands, Guid projectId, ClaimsPrincipal principal)
    {
        var method = typeof(MeetingsModule).GetMethod("CreateMeetingAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("MeetingsModule.CreateMeetingAsync was not found.");

        var request = new CreateMeetingRequest(projectId, "review", "Restricted review", DateTimeOffset.UtcNow, "pm@example.com", ["pm@example.com"], null, null, true, "confidential");
        var task = (Task<IResult>)method.Invoke(null, [principal, request, commands, new PermissionMatrix(), CancellationToken.None])!;
        return await task;
    }

    private static ClaimsPrincipal CreateMeetingViewerPrincipal() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, "operis:meeting_viewer")], "TestAuth"));
}
