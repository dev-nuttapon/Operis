using Operis_API.Modules.Users.Contracts;

namespace Operis_API.Modules.Users.Application;

public enum InvitationDetailQueryStatus
{
    Success = 1,
    NotFound = 2
}

public sealed record InvitationDetailQueryResult(
    InvitationDetailQueryStatus Status,
    InvitationDetailResponse? Response = null);
