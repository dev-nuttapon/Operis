using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
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
}
