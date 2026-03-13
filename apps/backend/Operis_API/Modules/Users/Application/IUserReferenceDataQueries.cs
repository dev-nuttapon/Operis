using Operis_API.Modules.Users.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Users.Application;

public interface IUserReferenceDataQueries
{
    Task<IReadOnlyList<AppRoleResponse>> ListRolesAsync(CancellationToken cancellationToken);
    Task<PagedResult<MasterDataResponse>> ListDivisionsAsync(ReferenceDataQuery query, CancellationToken cancellationToken);
    Task<PagedResult<MasterDataResponse>> ListDepartmentsAsync(ReferenceDataQuery query, CancellationToken cancellationToken);
    Task<PagedResult<MasterDataResponse>> ListJobTitlesAsync(ReferenceDataQuery query, CancellationToken cancellationToken);
    Task<PagedResult<MasterDataResponse>> ListProjectRolesAsync(ReferenceDataQuery query, CancellationToken cancellationToken);
}
