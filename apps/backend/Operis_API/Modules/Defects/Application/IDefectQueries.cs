using Operis_API.Modules.Defects.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Defects.Application;

public interface IDefectQueries
{
    Task<PagedResult<DefectListItem>> ListDefectsAsync(DefectListQuery query, CancellationToken cancellationToken);
    Task<DefectDetailResponse?> GetDefectAsync(Guid defectId, CancellationToken cancellationToken);
    Task<PagedResult<NonConformanceListItem>> ListNonConformancesAsync(NonConformanceListQuery query, CancellationToken cancellationToken);
    Task<NonConformanceDetailResponse?> GetNonConformanceAsync(Guid nonConformanceId, CancellationToken cancellationToken);
}
