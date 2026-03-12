using Operis_API.Modules.Users.Contracts;

namespace Operis_API.Modules.Users.Application;

public interface IUserInvitationCommands
{
    Task<InvitationCommandResult> CreateInvitationAsync(CreateInvitationRequest request, CancellationToken cancellationToken);
    Task<InvitationCommandResult> UpdateInvitationAsync(Guid invitationId, UpdateInvitationRequest request, CancellationToken cancellationToken);
    Task<InvitationCommandResult> CancelInvitationAsync(Guid invitationId, CancellationToken cancellationToken);
    Task<InvitationCommandResult> AcceptInvitationAsync(string token, AcceptInvitationRequest request, CancellationToken cancellationToken);
}
