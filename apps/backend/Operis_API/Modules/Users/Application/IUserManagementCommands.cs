using Operis_API.Modules.Users.Contracts;

namespace Operis_API.Modules.Users.Application;

public interface IUserManagementCommands
{
    Task<UserCommandResult> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<UserCommandResult> UpdateUserAsync(string userId, UpdateUserRequest request, CancellationToken cancellationToken);
    Task<UserCommandResult> DeleteUserAsync(string userId, SoftDeleteRequest request, string actor, CancellationToken cancellationToken);
}
