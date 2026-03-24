using Operis_API.Modules.Users.Contracts;

namespace Operis_API.Modules.Users.Application;

public interface IUserSelfServiceCommands
{
    Task<UserPasswordChangeResult> ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken cancellationToken);
}
