using Operis_API.Modules.Verification.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Verification.Application;

public interface IVerificationQueries
{
    Task<PagedResult<TestPlanListItemResponse>> ListTestPlansAsync(TestPlanListQuery query, CancellationToken cancellationToken);
    Task<TestPlanDetailResponse?> GetTestPlanAsync(Guid testPlanId, CancellationToken cancellationToken);
    Task<PagedResult<TestCaseListItemResponse>> ListTestCasesAsync(TestCaseListQuery query, CancellationToken cancellationToken);
    Task<TestCaseDetailResponse?> GetTestCaseAsync(Guid testCaseId, bool canReadSensitiveEvidence, CancellationToken cancellationToken);
    Task<PagedResult<TestExecutionListItemResponse>> ListTestExecutionsAsync(TestExecutionListQuery query, bool canReadSensitiveEvidence, CancellationToken cancellationToken);
    Task<PagedResult<UatSignoffListItemResponse>> ListUatSignoffsAsync(UatSignoffListQuery query, CancellationToken cancellationToken);
    Task<UatSignoffDetailResponse?> GetUatSignoffAsync(Guid uatSignoffId, CancellationToken cancellationToken);
}

