using Operis_API.Modules.Users.Contracts;

namespace Operis_API.Modules.Users.Application;

public interface IMasterDataCatalogCommands
{
    Task<(bool Success, string? Error, string? ErrorCode, MasterDataCatalogResponse? Response)> CreateAsync(CreateMasterDataCatalogRequest request, string actorUserId, CancellationToken cancellationToken);
    Task<(bool Success, string? Error, string? ErrorCode, MasterDataCatalogResponse? Response, bool NotFound)> UpdateAsync(Guid id, UpdateMasterDataCatalogRequest request, string actorUserId, CancellationToken cancellationToken);
    Task<(bool Success, string? Error, string? ErrorCode, MasterDataCatalogResponse? Response, bool NotFound)> ArchiveAsync(Guid id, SoftDeleteRequest request, string actorUserId, CancellationToken cancellationToken);
}
