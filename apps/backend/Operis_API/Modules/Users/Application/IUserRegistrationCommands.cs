using Operis_API.Modules.Users.Contracts;

namespace Operis_API.Modules.Users.Application;

public interface IUserRegistrationCommands
{
    Task<RegistrationCommandResult> CreateRegistrationRequestAsync(CreateRegistrationRequest request, CancellationToken cancellationToken);
    Task<RegistrationCommandResult> ApproveRegistrationRequestAsync(Guid requestId, ReviewRegistrationRequest request, CancellationToken cancellationToken);
    Task<RegistrationCommandResult> RejectRegistrationRequestAsync(Guid requestId, RejectRegistrationRequest request, CancellationToken cancellationToken);
    Task<RegistrationCommandResult> CompleteRegistrationPasswordSetupAsync(string token, CompleteRegistrationPasswordSetupRequest request, CancellationToken cancellationToken);
}
