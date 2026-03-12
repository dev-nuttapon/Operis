using Operis_API.Modules.Users.Contracts;

namespace Operis_API.Modules.Users.Application;

public interface IUserPreferenceCommands
{
    Task<UserPreferenceCommandResult> UpdateCurrentUserPreferencesAsync(string currentUserId, UpdateUserPreferencesRequest request, CancellationToken cancellationToken);
}
