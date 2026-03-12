using Operis_API.Modules.Users.Contracts;

namespace Operis_API.Modules.Users.Application;

public enum InvitationCommandStatus
{
    Success = 1,
    NotFound = 2,
    ValidationError = 3,
    Conflict = 4,
    ExternalFailure = 5
}

public sealed record InvitationCommandResult(
    InvitationCommandStatus Status,
    string? ErrorMessage = null,
    string? ProblemTitle = null,
    int? ProblemStatusCode = null,
    InvitationResponse? Response = null);
