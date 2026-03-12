using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Domain;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Users.Application;

public sealed class UserInvitationQueries(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter) : IUserInvitationQueries
{
    public async Task<PagedResult<InvitationResponse>> ListInvitationsAsync(InvitationQuery query, CancellationToken cancellationToken)
    {
        var (normalizedPage, normalizedPageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var invitations = dbContext.UserInvitations.AsNoTracking();

        if (query.Status.HasValue)
        {
            invitations = invitations.Where(x => x.Status == query.Status.Value);
        }

        if (query.From.HasValue)
        {
            invitations = invitations.Where(x => x.InvitedAt >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            invitations = invitations.Where(x => x.InvitedAt <= query.To.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchPattern = $"%{query.Search.Trim()}%";
            invitations = invitations.Where(x =>
                EF.Functions.ILike(x.Email, searchPattern)
                || EF.Functions.ILike(x.InvitedBy, searchPattern));
        }

        invitations = ApplyInvitationSorting(invitations, query.SortBy, query.SortOrder);

        var total = await invitations.CountAsync(cancellationToken);
        var items = await invitations
            .Skip(skip)
            .Take(normalizedPageSize)
            .ToListAsync(cancellationToken);

        var (departments, jobTitles) = await LoadReferenceMapsAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "list",
            EntityType: "invitation",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new
            {
                count = items.Count,
                total,
                query.Status,
                query.From,
                query.To,
                page = normalizedPage,
                pageSize = normalizedPageSize,
                query.Search,
                query.SortBy,
                query.SortOrder
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PagedResult<InvitationResponse>(
            items.Select(x => ToResponse(x, departments, jobTitles)).ToList(),
            total,
            normalizedPage,
            normalizedPageSize);
    }

    public async Task<InvitationDetailQueryResult> GetInvitationByTokenAsync(string token, CancellationToken cancellationToken)
    {
        var invitation = await dbContext.UserInvitations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.InvitationToken == token, cancellationToken);
        if (invitation is null)
        {
            return new InvitationDetailQueryResult(InvitationDetailQueryStatus.NotFound);
        }

        var (departments, jobTitles) = await LoadReferenceMapsAsync(cancellationToken);
        var status = GetInvitationStatus(invitation);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "view_invitation",
            EntityType: "invitation",
            EntityId: invitation.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            ActorType: "anonymous",
            ActorEmail: invitation.Email,
            DepartmentId: invitation.DepartmentId,
            Metadata: new { status }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new InvitationDetailQueryResult(
            InvitationDetailQueryStatus.Success,
            new InvitationDetailResponse(
                invitation.Id,
                invitation.Email,
                invitation.DepartmentId,
                invitation.DepartmentId.HasValue && departments.TryGetValue(invitation.DepartmentId.Value, out var departmentName) ? departmentName : null,
                invitation.JobTitleId,
                invitation.JobTitleId.HasValue && jobTitles.TryGetValue(invitation.JobTitleId.Value, out var jobTitleName) ? jobTitleName : null,
                status,
                invitation.InvitedAt,
                invitation.ExpiresAt));
    }

    private async Task<(IReadOnlyDictionary<Guid, string> Departments, IReadOnlyDictionary<Guid, string> JobTitles)> LoadReferenceMapsAsync(CancellationToken cancellationToken)
    {
        var departments = await dbContext.Departments
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var jobTitles = await dbContext.JobTitles
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        return (departments, jobTitles);
    }

    private static IQueryable<UserInvitationEntity> ApplyInvitationSorting(IQueryable<UserInvitationEntity> query, string? sortBy, string? sortOrder)
    {
        var desc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return sortBy?.ToLowerInvariant() switch
        {
            "email" => desc ? query.OrderByDescending(x => x.Email).ThenByDescending(x => x.InvitedAt) : query.OrderBy(x => x.Email).ThenByDescending(x => x.InvitedAt),
            "status" => desc ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.InvitedAt) : query.OrderBy(x => x.Status).ThenByDescending(x => x.InvitedAt),
            "expiresat" => desc ? query.OrderByDescending(x => x.ExpiresAt).ThenByDescending(x => x.InvitedAt) : query.OrderBy(x => x.ExpiresAt).ThenByDescending(x => x.InvitedAt),
            _ => desc ? query.OrderByDescending(x => x.InvitedAt) : query.OrderBy(x => x.InvitedAt)
        };
    }

    private static InvitationStatus GetInvitationStatus(UserInvitationEntity entity)
    {
        if (entity.Status == InvitationStatus.Pending && entity.ExpiresAt.HasValue && entity.ExpiresAt.Value <= DateTimeOffset.UtcNow)
        {
            return InvitationStatus.Expired;
        }

        return entity.Status;
    }

    private static InvitationResponse ToResponse(
        UserInvitationEntity entity,
        IReadOnlyDictionary<Guid, string> departments,
        IReadOnlyDictionary<Guid, string> jobTitles) =>
        new(
            entity.Id,
            entity.Email,
            entity.InvitationToken,
            entity.InvitedBy,
            entity.DepartmentId,
            entity.DepartmentId.HasValue && departments.TryGetValue(entity.DepartmentId.Value, out var departmentName) ? departmentName : null,
            entity.JobTitleId,
            entity.JobTitleId.HasValue && jobTitles.TryGetValue(entity.JobTitleId.Value, out var jobTitleName) ? jobTitleName : null,
            GetInvitationStatus(entity),
            entity.InvitedAt,
            entity.ExpiresAt,
            entity.AcceptedAt,
            entity.RejectedAt,
            $"/invite/{entity.InvitationToken}");

    private static (int Page, int PageSize, int Skip) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = Math.Clamp(pageSize, 10, 100);
        var skip = (normalizedPage - 1) * normalizedPageSize;
        return (normalizedPage, normalizedPageSize, skip);
    }
}
