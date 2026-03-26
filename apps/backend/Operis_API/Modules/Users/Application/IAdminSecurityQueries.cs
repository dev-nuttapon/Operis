using Operis_API.Modules.Users.Contracts;

namespace Operis_API.Modules.Users.Application;

public interface IAdminSecurityQueries
{
    Task<PermissionMatrixResponse> GetPermissionMatrixAsync(CancellationToken cancellationToken);
    Task<SystemSettingsResponse> GetSystemSettingsAsync(CancellationToken cancellationToken);
}
