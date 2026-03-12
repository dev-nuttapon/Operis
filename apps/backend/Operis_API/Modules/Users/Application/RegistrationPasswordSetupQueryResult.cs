using Operis_API.Modules.Users.Contracts;

namespace Operis_API.Modules.Users.Application;

public enum RegistrationPasswordSetupQueryStatus
{
    Success = 1,
    NotFound = 2
}

public sealed record RegistrationPasswordSetupQueryResult(
    RegistrationPasswordSetupQueryStatus Status,
    RegistrationPasswordSetupDetailResponse? Response = null);
