using Operis_API.Modules.Users.Contracts;

namespace Operis_API.Modules.Users.Application;

public interface IUserReferenceDataCommands
{
    Task<MasterDataCommandResult> CreateDivisionAsync(CreateMasterDataRequest request, CancellationToken cancellationToken);
    Task<MasterDataCommandResult> UpdateDivisionAsync(Guid divisionId, UpdateMasterDataRequest request, CancellationToken cancellationToken);
    Task<MasterDataCommandResult> DeleteDivisionAsync(Guid divisionId, SoftDeleteRequest request, string actor, CancellationToken cancellationToken);
    Task<MasterDataCommandResult> CreateDepartmentAsync(CreateDepartmentRequest request, CancellationToken cancellationToken);
    Task<MasterDataCommandResult> UpdateDepartmentAsync(Guid departmentId, UpdateDepartmentRequest request, CancellationToken cancellationToken);
    Task<MasterDataCommandResult> DeleteDepartmentAsync(Guid departmentId, SoftDeleteRequest request, string actor, CancellationToken cancellationToken);
    Task<MasterDataCommandResult> CreateJobTitleAsync(CreateJobTitleRequest request, CancellationToken cancellationToken);
    Task<MasterDataCommandResult> UpdateJobTitleAsync(Guid jobTitleId, UpdateJobTitleRequest request, CancellationToken cancellationToken);
    Task<MasterDataCommandResult> DeleteJobTitleAsync(Guid jobTitleId, SoftDeleteRequest request, string actor, CancellationToken cancellationToken);
    Task<MasterDataCommandResult> CreateProjectRoleAsync(CreateMasterDataRequest request, CancellationToken cancellationToken);
    Task<MasterDataCommandResult> UpdateProjectRoleAsync(Guid projectRoleId, UpdateMasterDataRequest request, CancellationToken cancellationToken);
    Task<MasterDataCommandResult> DeleteProjectRoleAsync(Guid projectRoleId, SoftDeleteRequest request, string actor, CancellationToken cancellationToken);
}
