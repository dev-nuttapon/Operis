using Operis_API.Modules.Meetings.Contracts;

namespace Operis_API.Modules.Meetings.Application;

public interface IMeetingCommands
{
    Task<MeetingCommandResult<MeetingDetailResponse>> CreateMeetingAsync(CreateMeetingRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MeetingCommandResult<MeetingDetailResponse>> UpdateMeetingAsync(Guid meetingId, UpdateMeetingRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MeetingCommandResult<MeetingDetailResponse>> ApproveMeetingAsync(Guid meetingId, MeetingApprovalRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MeetingCommandResult<MeetingMinutesResponse>> UpdateMeetingMinutesAsync(Guid meetingId, UpdateMeetingMinutesRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MeetingCommandResult<DecisionDetailResponse>> CreateDecisionAsync(CreateDecisionRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MeetingCommandResult<DecisionDetailResponse>> UpdateDecisionAsync(Guid decisionId, UpdateDecisionRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MeetingCommandResult<DecisionDetailResponse>> ApproveDecisionAsync(Guid decisionId, DecisionTransitionRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MeetingCommandResult<DecisionDetailResponse>> ApplyDecisionAsync(Guid decisionId, DecisionTransitionRequest request, string? actorUserId, CancellationToken cancellationToken);
}
