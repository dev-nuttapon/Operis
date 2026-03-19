using Operis_API.Modules.Users.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Users.Application;

public interface IUserQueries
{
    Task<PagedResult<UserResponse>> ListUsersAsync(UserListQuery query, CancellationToken cancellationToken);
    Task<UserResponse?> GetUserAsync(string userId, bool includeIdentity, CancellationToken cancellationToken);
}
