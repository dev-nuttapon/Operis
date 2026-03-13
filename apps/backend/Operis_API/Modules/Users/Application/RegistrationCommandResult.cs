using Operis_API.Modules.Users.Contracts;

namespace Operis_API.Modules.Users.Application;

public enum RegistrationCommandStatus
{
    Success = 1,
    NotFound = 2,
    ValidationError = 3,
    Conflict = 4,
    ExternalFailure = 5,
    InternalFailure = 6
}

public sealed record RegistrationCommandResult(
    RegistrationCommandStatus Status,
    string? ErrorMessage = null,
    string? ErrorCode = null,
    string? ProblemTitle = null,
    int? ProblemStatusCode = null,
    RegistrationRequestResponse? Response = null,
    RegistrationPasswordSetupDetailResponse? PasswordSetup = null);
