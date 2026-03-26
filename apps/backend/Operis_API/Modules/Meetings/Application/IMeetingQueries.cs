using Operis_API.Modules.Meetings.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Meetings.Application;

public interface IMeetingQueries
{
    Task<PagedResult<MeetingListItemResponse>> ListMeetingsAsync(MeetingListQuery query, bool canReadRestricted, CancellationToken cancellationToken);
    Task<MeetingDetailResponse?> GetMeetingAsync(Guid meetingId, bool canReadRestricted, CancellationToken cancellationToken);
    Task<MeetingMinutesResponse?> GetMeetingMinutesAsync(Guid meetingId, bool canReadRestricted, CancellationToken cancellationToken);
    Task<PagedResult<DecisionListItemResponse>> ListDecisionsAsync(DecisionListQuery query, bool canReadRestricted, CancellationToken cancellationToken);
    Task<DecisionDetailResponse?> GetDecisionAsync(Guid decisionId, bool canReadRestricted, CancellationToken cancellationToken);
}
