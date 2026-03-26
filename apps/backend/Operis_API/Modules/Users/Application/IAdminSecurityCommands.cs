using Operis_API.Modules.Users.Contracts;

namespace Operis_API.Modules.Users.Application;

public interface IAdminSecurityCommands
{
    Task<AdminSecurityCommandResult<PermissionMatrixResponse>> ApplyPermissionMatrixAsync(string actor, ApplyPermissionMatrixRequest request, CancellationToken cancellationToken);
    Task<AdminSecurityCommandResult<SystemSettingsResponse>> UpdateSystemSettingsAsync(string actor, UpdateSystemSettingsRequest request, CancellationToken cancellationToken);
}
