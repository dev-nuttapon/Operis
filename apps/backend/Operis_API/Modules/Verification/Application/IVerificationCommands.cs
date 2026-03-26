using Operis_API.Modules.Verification.Contracts;

namespace Operis_API.Modules.Verification.Application;

public interface IVerificationCommands
{
    Task<VerificationCommandResult<TestPlanDetailResponse>> CreateTestPlanAsync(CreateTestPlanRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<VerificationCommandResult<TestPlanDetailResponse>> UpdateTestPlanAsync(Guid testPlanId, UpdateTestPlanRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<VerificationCommandResult<TestPlanDetailResponse>> SubmitTestPlanAsync(Guid testPlanId, string? actorUserId, CancellationToken cancellationToken);
    Task<VerificationCommandResult<TestPlanDetailResponse>> ApproveTestPlanAsync(Guid testPlanId, VerificationDecisionRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<VerificationCommandResult<TestPlanDetailResponse>> BaselineTestPlanAsync(Guid testPlanId, VerificationDecisionRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<VerificationCommandResult<TestCaseDetailResponse>> CreateTestCaseAsync(CreateTestCaseRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<VerificationCommandResult<TestCaseDetailResponse>> UpdateTestCaseAsync(Guid testCaseId, UpdateTestCaseRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<VerificationCommandResult<TestExecutionCreateResponse>> CreateTestExecutionAsync(CreateTestExecutionRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<VerificationCommandResult<ExecutionExportResponse>> ExportTestExecutionsAsync(ExecutionExportRequest request, bool canReadSensitiveEvidence, string? actorUserId, CancellationToken cancellationToken);
    Task<VerificationCommandResult<UatSignoffDetailResponse>> CreateUatSignoffAsync(CreateUatSignoffRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<VerificationCommandResult<UatSignoffDetailResponse>> UpdateUatSignoffAsync(Guid uatSignoffId, UpdateUatSignoffRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<VerificationCommandResult<UatSignoffDetailResponse>> SubmitUatSignoffAsync(Guid uatSignoffId, string? actorUserId, CancellationToken cancellationToken);
    Task<VerificationCommandResult<UatSignoffDetailResponse>> ApproveUatSignoffAsync(Guid uatSignoffId, VerificationDecisionRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<VerificationCommandResult<UatSignoffDetailResponse>> RejectUatSignoffAsync(Guid uatSignoffId, VerificationDecisionRequest request, string? actorUserId, CancellationToken cancellationToken);
}
