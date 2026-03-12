namespace Operis_API.Modules.Users.Application;

public enum UserPreferenceCommandStatus
{
    Success = 1,
    NotFound = 2
}

public sealed record UserPreferenceCommandResult(UserPreferenceCommandStatus Status);
