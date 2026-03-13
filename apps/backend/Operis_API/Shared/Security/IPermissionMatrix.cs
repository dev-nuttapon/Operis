using System.Security.Claims;

namespace Operis_API.Shared.Security;

public interface IPermissionMatrix
{
    IReadOnlyList<string> GetPermissions(IEnumerable<string> roles);
    bool HasPermission(ClaimsPrincipal? principal, string permission);
    bool HasAnyPermission(ClaimsPrincipal? principal, params string[] permissions);
}
