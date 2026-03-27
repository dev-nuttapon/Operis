using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Notifications;
using Operis_API.Shared.Security;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Notifications;

public sealed class NotificationsModuleHandlerTests
{
    [Fact]
    public async Task RetryNotificationAsync_WithoutManagePermission_ReturnsForbidden()
    {
        var result = await InvokeRetryNotificationAsync(CreateReadOnlyPrincipal(), new FakeNotificationCommands());

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    private static async Task<IResult> InvokeRetryNotificationAsync(ClaimsPrincipal principal, INotificationCommands commands)
    {
        var method = typeof(NotificationsModule).GetMethod("RetryNotificationAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("NotificationsModule.RetryNotificationAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [principal, new PermissionMatrix(), commands, Guid.NewGuid(), CancellationToken.None])!;
        return await task;
    }

    private static ClaimsPrincipal CreateReadOnlyPrincipal()
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "reader-1"),
            new Claim(ClaimTypes.Role, "operis:audit_auditor")
        ], "test");

        return new ClaimsPrincipal(identity);
    }

    private sealed class FakeNotificationCommands : INotificationCommands
    {
        public Task<NotificationUpdateResult> MarkReadAsync(Guid notificationId, string? currentUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<NotificationUpdateResult> MarkAllReadAsync(string? currentUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<NotificationUpdateResult> SeedAsync(string? currentUserId, int count, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<NotificationQueueCommandResult> EnqueueAsync(CreateNotificationQueueRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<NotificationQueueCommandResult> RetryAsync(Guid id, string? actor, CancellationToken cancellationToken) =>
            Task.FromResult(NotificationQueueCommandResult.Success(new NotificationQueueItemContract(id, "email", "ops@example.com", "minio://notifications/queue.json", DateTimeOffset.UtcNow, "retried", 1, "smtp timeout", DateTimeOffset.UtcNow)));
    }
}
