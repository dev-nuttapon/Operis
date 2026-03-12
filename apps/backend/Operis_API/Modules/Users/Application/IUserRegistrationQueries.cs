using Operis_API.Modules.Users.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Users.Application;

public interface IUserRegistrationQueries
{
    Task<PagedResult<RegistrationRequestResponse>> ListRegistrationRequestsAsync(RegistrationQuery query, CancellationToken cancellationToken);
    Task<RegistrationPasswordSetupQueryResult> GetRegistrationPasswordSetupAsync(string token, CancellationToken cancellationToken);
}
