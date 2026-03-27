using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Notifications.Infrastructure;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Notifications;

public sealed class NotificationQueries(OperisDbContext dbContext) : INotificationQueries
{
    public Task<PagedResult<NotificationListItem>> ListAsync(
        NotificationListQuery query,
        string? currentUserId,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = query.PageSize <= 0 ? 10 : Math.Min(query.PageSize, 100);
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Task.FromResult(new PagedResult<NotificationListItem>(Array.Empty<NotificationListItem>(), 0, page, pageSize));
        }

        return ListInternalAsync(currentUserId, query.UnreadOnly, page, pageSize, cancellationToken);
    }

    public async Task<NotificationDetailContract?> GetByIdAsync(
        Guid notificationId,
        string? currentUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return null;
        }

        return await dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.Id == notificationId && x.RecipientUserId == currentUserId)
            .Select(x => new NotificationDetailContract(
                x.Id,
                x.Title,
                x.Description,
                x.Source,
                x.Status,
                x.CreatedAt,
                x.ReadAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<PagedResult<NotificationListItem>> ListInternalAsync(
        string currentUserId,
        bool? unreadOnly,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var baseQuery = dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.RecipientUserId == currentUserId);

        if (unreadOnly == true)
        {
            baseQuery = baseQuery.Where(x => x.Status == "unread");
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new NotificationListItem(
                x.Id,
                x.Title,
                x.Description,
                x.Source,
                x.Status,
                x.CreatedAt,
                x.ReadAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<NotificationListItem>(items, total, page, pageSize);
    }

    public async Task<PagedResult<NotificationQueueItemContract>> ListQueueAsync(NotificationQueueListQuery query, CancellationToken cancellationToken)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 25 : Math.Min(query.PageSize, 100);
        var source = dbContext.NotificationQueue.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Channel))
        {
            var channel = query.Channel.Trim().ToLowerInvariant();
            source = source.Where(x => x.Channel == channel);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToLowerInvariant();
            source = source.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.Channel, search) ||
                EF.Functions.ILike(x.TargetRef, search) ||
                EF.Functions.ILike(x.PayloadRef, search) ||
                (x.LastError != null && EF.Functions.ILike(x.LastError, search)));
        }

        source = source.OrderByDescending(x => x.QueuedAt);
        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new NotificationQueueItemContract(x.Id, x.Channel, x.TargetRef, x.PayloadRef, x.QueuedAt, x.Status, x.RetryCount, x.LastError, x.LastRetriedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<NotificationQueueItemContract>(items, total, page, pageSize);
    }
}
