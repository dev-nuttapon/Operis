using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Meetings.Contracts;
using Operis_API.Modules.Meetings.Infrastructure;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Meetings.Application;

public sealed class MeetingQueries(OperisDbContext dbContext) : IMeetingQueries
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<PagedResult<MeetingListItemResponse>> ListMeetingsAsync(MeetingListQuery query, bool canReadRestricted, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var source =
            from meeting in dbContext.Set<MeetingRecordEntity>().AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on meeting.ProjectId equals project.Id
            select new { Meeting = meeting, ProjectName = project.Name };

        if (!canReadRestricted)
        {
            source = source.Where(x => !x.Meeting.IsRestricted);
        }

        if (query.ProjectId.HasValue)
        {
            source = source.Where(x => x.Meeting.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.MeetingType))
        {
            var type = query.MeetingType.Trim().ToLowerInvariant();
            source = source.Where(x => x.Meeting.MeetingType == type);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToLowerInvariant();
            source = source.Where(x => x.Meeting.Status == status);
        }

        if (query.MeetingDateFrom.HasValue)
        {
            var from = query.MeetingDateFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            source = source.Where(x => x.Meeting.MeetingAt >= from);
        }

        if (query.MeetingDateTo.HasValue)
        {
            var toExclusive = query.MeetingDateTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            source = source.Where(x => x.Meeting.MeetingAt < toExclusive);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.Meeting.Title, search)
                || EF.Functions.ILike(x.Meeting.MeetingType, search)
                || EF.Functions.ILike(x.ProjectName, search));
        }

        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .OrderByDescending(x => x.Meeting.MeetingAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new MeetingListItemResponse(
                x.Meeting.Id,
                x.Meeting.ProjectId,
                x.ProjectName,
                x.Meeting.MeetingType,
                x.Meeting.Title,
                x.Meeting.MeetingAt,
                x.Meeting.FacilitatorUserId,
                x.Meeting.Status,
                x.Meeting.IsRestricted,
                x.Meeting.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<MeetingListItemResponse>(items, total, page, pageSize);
    }

    public async Task<MeetingDetailResponse?> GetMeetingAsync(Guid meetingId, bool canReadRestricted, CancellationToken cancellationToken)
    {
        var item = await (
            from meeting in dbContext.Set<MeetingRecordEntity>().AsNoTracking()
            where meeting.Id == meetingId
            join project in dbContext.Projects.AsNoTracking() on meeting.ProjectId equals project.Id
            select new { Meeting = meeting, ProjectName = project.Name })
            .SingleOrDefaultAsync(cancellationToken);

        if (item is null || (item.Meeting.IsRestricted && !canReadRestricted))
        {
            return null;
        }

        var minutes = await LoadMinutesAsync(meetingId, cancellationToken);
        var attendees = await dbContext.Set<MeetingAttendeeEntity>().AsNoTracking()
            .Where(x => x.MeetingRecordId == meetingId)
            .OrderBy(x => x.UserId)
            .Select(x => new MeetingAttendeeItemResponse(x.Id, x.MeetingRecordId, x.UserId, x.AttendanceStatus))
            .ToListAsync(cancellationToken);

        var decisions = await (
            from decision in dbContext.Set<DecisionEntity>().AsNoTracking()
            where decision.MeetingId == meetingId
            select new DecisionListItemResponse(
                decision.Id,
                decision.ProjectId,
                item.ProjectName,
                decision.MeetingId,
                item.Meeting.Title,
                decision.Code,
                decision.Title,
                decision.DecisionType,
                decision.ApprovedBy,
                decision.Status,
                decision.IsRestricted,
                decision.UpdatedAt))
            .ToListAsync(cancellationToken);

        var history = await dbContext.BusinessAuditEvents.AsNoTracking()
            .Where(x => (x.EntityType == "meeting" || x.EntityType == "meeting_minutes") && x.EntityId == meetingId.ToString())
            .OrderByDescending(x => x.OccurredAt)
            .Select(x => new MeetingHistoryItem(x.Id, x.EventType, x.Summary, x.Reason, x.ActorUserId, x.OccurredAt))
            .ToListAsync(cancellationToken);

        return new MeetingDetailResponse(
            item.Meeting.Id,
            item.Meeting.ProjectId,
            item.ProjectName,
            item.Meeting.MeetingType,
            item.Meeting.Title,
            item.Meeting.MeetingAt,
            item.Meeting.FacilitatorUserId,
            item.Meeting.Status,
            item.Meeting.Agenda,
            item.Meeting.DiscussionSummary,
            item.Meeting.IsRestricted,
            item.Meeting.Classification,
            minutes,
            attendees,
            decisions,
            history,
            item.Meeting.CreatedAt,
            item.Meeting.UpdatedAt);
    }

    public async Task<MeetingMinutesResponse?> GetMeetingMinutesAsync(Guid meetingId, bool canReadRestricted, CancellationToken cancellationToken)
    {
        var meeting = await dbContext.Set<MeetingRecordEntity>().AsNoTracking()
            .Where(x => x.Id == meetingId)
            .Select(x => new { x.Id, x.IsRestricted })
            .SingleOrDefaultAsync(cancellationToken);
        if (meeting is null || (meeting.IsRestricted && !canReadRestricted))
        {
            return null;
        }

        return await LoadMinutesAsync(meetingId, cancellationToken);
    }

    public async Task<PagedResult<DecisionListItemResponse>> ListDecisionsAsync(DecisionListQuery query, bool canReadRestricted, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var source =
            from decision in dbContext.Set<DecisionEntity>().AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on decision.ProjectId equals project.Id
            join meeting in dbContext.Set<MeetingRecordEntity>().AsNoTracking() on decision.MeetingId equals meeting.Id into meetingJoin
            from meeting in meetingJoin.DefaultIfEmpty()
            select new { Decision = decision, ProjectName = project.Name, MeetingTitle = meeting == null ? null : meeting.Title };

        if (!canReadRestricted)
        {
            source = source.Where(x => !x.Decision.IsRestricted);
        }

        if (query.ProjectId.HasValue)
        {
            source = source.Where(x => x.Decision.ProjectId == query.ProjectId.Value);
        }

        if (query.MeetingId.HasValue)
        {
            source = source.Where(x => x.Decision.MeetingId == query.MeetingId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.DecisionType))
        {
            var type = query.DecisionType.Trim().ToLowerInvariant();
            source = source.Where(x => x.Decision.DecisionType == type);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToLowerInvariant();
            source = source.Where(x => x.Decision.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.Decision.Code, search)
                || EF.Functions.ILike(x.Decision.Title, search)
                || EF.Functions.ILike(x.ProjectName, search));
        }

        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .OrderByDescending(x => x.Decision.UpdatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new DecisionListItemResponse(
                x.Decision.Id,
                x.Decision.ProjectId,
                x.ProjectName,
                x.Decision.MeetingId,
                x.MeetingTitle,
                x.Decision.Code,
                x.Decision.Title,
                x.Decision.DecisionType,
                x.Decision.ApprovedBy,
                x.Decision.Status,
                x.Decision.IsRestricted,
                x.Decision.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<DecisionListItemResponse>(items, total, page, pageSize);
    }

    public async Task<DecisionDetailResponse?> GetDecisionAsync(Guid decisionId, bool canReadRestricted, CancellationToken cancellationToken)
    {
        var item = await (
            from decision in dbContext.Set<DecisionEntity>().AsNoTracking()
            where decision.Id == decisionId
            join project in dbContext.Projects.AsNoTracking() on decision.ProjectId equals project.Id
            join meeting in dbContext.Set<MeetingRecordEntity>().AsNoTracking() on decision.MeetingId equals meeting.Id into meetingJoin
            from meeting in meetingJoin.DefaultIfEmpty()
            select new { Decision = decision, ProjectName = project.Name, MeetingTitle = meeting == null ? null : meeting.Title })
            .SingleOrDefaultAsync(cancellationToken);

        if (item is null || (item.Decision.IsRestricted && !canReadRestricted))
        {
            return null;
        }

        var history = await dbContext.BusinessAuditEvents.AsNoTracking()
            .Where(x => x.EntityType == "decision" && x.EntityId == decisionId.ToString())
            .OrderByDescending(x => x.OccurredAt)
            .Select(x => new MeetingHistoryItem(x.Id, x.EventType, x.Summary, x.Reason, x.ActorUserId, x.OccurredAt))
            .ToListAsync(cancellationToken);

        return new DecisionDetailResponse(
            item.Decision.Id,
            item.Decision.ProjectId,
            item.ProjectName,
            item.Decision.MeetingId,
            item.MeetingTitle,
            item.Decision.Code,
            item.Decision.Title,
            item.Decision.DecisionType,
            item.Decision.Rationale,
            item.Decision.AlternativesConsidered,
            DeserializeList(item.Decision.ImpactedArtifactsJson),
            item.Decision.ApprovedBy,
            item.Decision.ApprovedAt,
            item.Decision.Status,
            item.Decision.IsRestricted,
            item.Decision.Classification,
            history,
            item.Decision.CreatedAt,
            item.Decision.UpdatedAt);
    }

    private async Task<MeetingMinutesResponse> LoadMinutesAsync(Guid meetingId, CancellationToken cancellationToken)
    {
        var minutes = await dbContext.Set<MeetingMinutesEntity>().AsNoTracking()
            .Where(x => x.MeetingRecordId == meetingId)
            .Select(x => new MeetingMinutesResponse(x.Id, x.MeetingRecordId, x.Summary, x.DecisionsSummary, x.ActionsSummary, x.Status, x.UpdatedAt))
            .SingleAsync(cancellationToken);
        return minutes;
    }

    private static IReadOnlyList<string> DeserializeList(string json) =>
        JsonSerializer.Deserialize<IReadOnlyList<string>>(json, SerializerOptions) ?? [];

    private static (int Page, int PageSize, int Skip) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize switch
        {
            <= 0 => 25,
            > 100 => 100,
            _ => pageSize
        };

        return (normalizedPage, normalizedPageSize, (normalizedPage - 1) * normalizedPageSize);
    }
}
