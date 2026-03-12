using Operis_API.Modules.Users.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Users.Application;

public interface IUserInvitationQueries
{
    Task<PagedResult<InvitationResponse>> ListInvitationsAsync(InvitationQuery query, CancellationToken cancellationToken);
    Task<InvitationDetailQueryResult> GetInvitationByTokenAsync(string token, CancellationToken cancellationToken);
}
