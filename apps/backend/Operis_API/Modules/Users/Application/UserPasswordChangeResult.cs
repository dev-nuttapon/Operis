namespace Operis_API.Modules.Users.Application;

public enum UserPasswordChangeStatus
{
    Success = 1,
    NotFound = 2,
    ValidationError = 3,
    ExternalFailure = 4
}

public sealed record UserPasswordChangeResult(
    UserPasswordChangeStatus Status,
    string? ErrorMessage = null,
    string? ErrorCode = null,
    string? ProblemTitle = null,
    int? ProblemStatusCode = null);
