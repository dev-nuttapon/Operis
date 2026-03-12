using Operis_API.Modules.Users.Contracts;

namespace Operis_API.Modules.Users.Application;

public enum MasterDataCommandStatus
{
    Success = 1,
    ValidationError = 2,
    NotFound = 3,
    Conflict = 4
}

public sealed record MasterDataCommandResult(
    MasterDataCommandStatus Status,
    string? ErrorMessage = null,
    MasterDataResponse? Response = null);
