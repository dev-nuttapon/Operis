using Operis_API.Modules.Users.Contracts;

namespace Operis_API.Modules.Users.Application;

public interface IUserOrgAssignmentCommands
{
    Task<UserCommandResult> UpsertPrimaryAssignmentAsync(string userId, UpsertUserOrgAssignmentRequest request, CancellationToken cancellationToken);
}
