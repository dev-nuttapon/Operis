namespace Operis_API.Modules.Users.Application;

public enum AdminSecurityCommandStatus
{
    Success = 0,
    ValidationError = 1
}

public sealed record AdminSecurityCommandResult<TResponse>(
    AdminSecurityCommandStatus Status,
    TResponse? Response = default,
    string? ErrorMessage = null,
    string? ErrorCode = null);
