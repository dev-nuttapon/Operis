using Operis_API.Modules.Users.Contracts;

namespace Operis_API.Modules.Users.Application;

public interface IUserReferenceDataCommands
{
    Task<MasterDataCommandResult> CreateDepartmentAsync(CreateMasterDataRequest request, CancellationToken cancellationToken);
    Task<MasterDataCommandResult> UpdateDepartmentAsync(Guid departmentId, UpdateMasterDataRequest request, CancellationToken cancellationToken);
    Task<MasterDataCommandResult> DeleteDepartmentAsync(Guid departmentId, SoftDeleteRequest request, string actor, CancellationToken cancellationToken);
    Task<MasterDataCommandResult> CreateJobTitleAsync(CreateMasterDataRequest request, CancellationToken cancellationToken);
    Task<MasterDataCommandResult> UpdateJobTitleAsync(Guid jobTitleId, UpdateMasterDataRequest request, CancellationToken cancellationToken);
    Task<MasterDataCommandResult> DeleteJobTitleAsync(Guid jobTitleId, SoftDeleteRequest request, string actor, CancellationToken cancellationToken);
}
