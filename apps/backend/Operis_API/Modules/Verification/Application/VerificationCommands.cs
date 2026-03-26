using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Audits.Application;
using Operis_API.Modules.Requirements.Application;
using Operis_API.Modules.Requirements.Contracts;
using Operis_API.Modules.Requirements.Infrastructure;
using Operis_API.Modules.Verification.Contracts;
using Operis_API.Modules.Verification.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Verification.Application;

public sealed class VerificationCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IBusinessAuditEventWriter businessAuditEventWriter,
    IVerificationQueries queries,
    IRequirementCommands requirementCommands) : IVerificationCommands
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly string[] TestPlanStatuses = ["draft", "review", "approved", "baseline"];
    private static readonly string[] TestCaseStatuses = ["draft", "ready", "active", "retired"];
    private static readonly string[] TestExecutionResults = ["passed", "failed", "retest"];
    private static readonly string[] UatStatuses = ["draft", "submitted", "approved", "rejected"];

    public async Task<VerificationCommandResult<TestPlanDetailResponse>> CreateTestPlanAsync(CreateTestPlanRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var validation = await ValidateTestPlanAsync(request.ProjectId, request.Code, request.Title, request.ScopeSummary, request.OwnerUserId, request.LinkedRequirementIds, null, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new TestPlanEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Code = request.Code.Trim().ToUpperInvariant(),
            Title = request.Title.Trim(),
            ScopeSummary = request.ScopeSummary.Trim(),
            OwnerUserId = request.OwnerUserId.Trim(),
            Status = "draft",
            EntryCriteria = TrimOrNull(request.EntryCriteria),
            ExitCriteria = TrimOrNull(request.ExitCriteria),
            LinkedRequirementIdsJson = SerializeGuidList(request.LinkedRequirementIds),
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("create", "test_plan", entity.Id, StatusCodes.Status201Created, new { entity.Code, entity.Status }, cancellationToken);
        await AppendBusinessEventAsync("test_plan_created", "test_plan", entity.Id, actorUserId, "Created test plan", null, new { entity.Code }, cancellationToken);
        return await SuccessTestPlanAsync(entity.Id, cancellationToken);
    }

    public async Task<VerificationCommandResult<TestPlanDetailResponse>> UpdateTestPlanAsync(Guid testPlanId, UpdateTestPlanRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<TestPlanEntity>().SingleOrDefaultAsync(x => x.Id == testPlanId, cancellationToken);
        if (entity is null)
        {
            return NotFound<TestPlanDetailResponse>(ApiErrorCodes.TestPlanNotFound, "Test plan not found.");
        }

        if (string.Equals(entity.Status, "baseline", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<TestPlanDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Baselined test plans cannot be edited.");
        }

        var nextStatus = string.IsNullOrWhiteSpace(request.Status) ? entity.Status : request.Status.Trim().ToLowerInvariant();
        if (!TestPlanStatuses.Contains(nextStatus, StringComparer.OrdinalIgnoreCase))
        {
            return Validation<TestPlanDetailResponse>(ApiErrorCodes.RequestValidationFailed, "Test plan status is invalid.");
        }

        var validation = await ValidateTestPlanAsync(entity.ProjectId, entity.Code, request.Title, request.ScopeSummary, request.OwnerUserId, request.LinkedRequirementIds, testPlanId, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Title = request.Title.Trim(),
            ScopeSummary = request.ScopeSummary.Trim(),
            OwnerUserId = request.OwnerUserId.Trim(),
            EntryCriteria = TrimOrNull(request.EntryCriteria),
            ExitCriteria = TrimOrNull(request.ExitCriteria),
            LinkedRequirementIdsJson = SerializeGuidList(request.LinkedRequirementIds),
            Status = nextStatus,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("update", "test_plan", testPlanId, StatusCodes.Status200OK, new { Status = nextStatus }, cancellationToken);
        await AppendBusinessEventAsync("test_plan_updated", "test_plan", testPlanId, actorUserId, "Updated test plan", null, new { Status = nextStatus }, cancellationToken);
        return await SuccessTestPlanAsync(testPlanId, cancellationToken);
    }

    public async Task<VerificationCommandResult<TestPlanDetailResponse>> SubmitTestPlanAsync(Guid testPlanId, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<TestPlanEntity>().SingleOrDefaultAsync(x => x.Id == testPlanId, cancellationToken);
        if (entity is null)
        {
            return NotFound<TestPlanDetailResponse>(ApiErrorCodes.TestPlanNotFound, "Test plan not found.");
        }

        if (!string.Equals(entity.Status, "draft", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<TestPlanDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Only draft test plans can move to review.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with { Status = "review", UpdatedAt = DateTimeOffset.UtcNow });
        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("submit", "test_plan", testPlanId, StatusCodes.Status200OK, null, cancellationToken);
        await AppendBusinessEventAsync("test_plan_submitted", "test_plan", testPlanId, actorUserId, "Submitted test plan for review", null, null, cancellationToken);
        return await SuccessTestPlanAsync(testPlanId, cancellationToken);
    }

    public async Task<VerificationCommandResult<TestPlanDetailResponse>> ApproveTestPlanAsync(Guid testPlanId, VerificationDecisionRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<TestPlanEntity>().SingleOrDefaultAsync(x => x.Id == testPlanId, cancellationToken);
        if (entity is null)
        {
            return NotFound<TestPlanDetailResponse>(ApiErrorCodes.TestPlanNotFound, "Test plan not found.");
        }

        if (!string.Equals(entity.Status, "review", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<TestPlanDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Only reviewed test plans can be approved.");
        }

        if (string.IsNullOrWhiteSpace(entity.ScopeSummary))
        {
            return Validation<TestPlanDetailResponse>(ApiErrorCodes.TestPlanScopeRequired, "Test plan scope is required before approval.");
        }

        if (string.IsNullOrWhiteSpace(entity.EntryCriteria) || string.IsNullOrWhiteSpace(entity.ExitCriteria))
        {
            return Validation<TestPlanDetailResponse>(ApiErrorCodes.TestPlanCriteriaRequired, "Entry and exit criteria are required before approval.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Status = "approved",
            ApprovalReason = TrimOrNull(request.Reason),
            ApprovedBy = actorUserId,
            ApprovedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("approve", "test_plan", testPlanId, StatusCodes.Status200OK, null, cancellationToken, TrimOrNull(request.Reason));
        await AppendBusinessEventAsync("test_plan_approved", "test_plan", testPlanId, actorUserId, "Approved test plan", TrimOrNull(request.Reason), null, cancellationToken);
        return await SuccessTestPlanAsync(testPlanId, cancellationToken);
    }

    public async Task<VerificationCommandResult<TestPlanDetailResponse>> BaselineTestPlanAsync(Guid testPlanId, VerificationDecisionRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<TestPlanEntity>().SingleOrDefaultAsync(x => x.Id == testPlanId, cancellationToken);
        if (entity is null)
        {
            return NotFound<TestPlanDetailResponse>(ApiErrorCodes.TestPlanNotFound, "Test plan not found.");
        }

        if (!string.Equals(entity.Status, "approved", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<TestPlanDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Only approved test plans can be baselined.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Status = "baseline",
            BaselinedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("baseline", "test_plan", testPlanId, StatusCodes.Status200OK, null, cancellationToken, TrimOrNull(request.Reason));
        await AppendBusinessEventAsync("test_plan_baselined", "test_plan", testPlanId, actorUserId, "Baselined test plan", TrimOrNull(request.Reason), null, cancellationToken);
        return await SuccessTestPlanAsync(testPlanId, cancellationToken);
    }

    public async Task<VerificationCommandResult<TestCaseDetailResponse>> CreateTestCaseAsync(CreateTestCaseRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var validation = await ValidateTestCaseAsync(request.TestPlanId, request.Code, request.Title, request.ExpectedResult, request.RequirementId, request.Status, null, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var entity = new TestCaseEntity
        {
            Id = Guid.NewGuid(),
            TestPlanId = request.TestPlanId,
            Code = request.Code.Trim().ToUpperInvariant(),
            Title = request.Title.Trim(),
            Preconditions = TrimOrNull(request.Preconditions),
            StepsJson = SerializeStringList(request.Steps),
            ExpectedResult = request.ExpectedResult.Trim(),
            RequirementId = request.RequirementId,
            Status = NormalizeTestCaseStatus(request.Status),
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        var syncError = await SyncRequirementCoverageLinkAsync(entity.RequirementId, entity.Id, actorUserId, cancellationToken);
        if (syncError is not null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return syncError;
        }

        await transaction.CommitAsync(cancellationToken);
        await AppendAuditAsync("create", "test_case", entity.Id, StatusCodes.Status201Created, new { entity.Code, entity.Status }, cancellationToken);
        await AppendBusinessEventAsync("test_case_created", "test_case", entity.Id, actorUserId, "Created test case", null, new { entity.Code }, cancellationToken);
        return await SuccessTestCaseAsync(entity.Id, cancellationToken);
    }

    public async Task<VerificationCommandResult<TestCaseDetailResponse>> UpdateTestCaseAsync(Guid testCaseId, UpdateTestCaseRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<TestCaseEntity>().SingleOrDefaultAsync(x => x.Id == testCaseId, cancellationToken);
        if (entity is null)
        {
            return NotFound<TestCaseDetailResponse>(ApiErrorCodes.TestCaseNotFound, "Test case not found.");
        }

        var validation = await ValidateTestCaseAsync(entity.TestPlanId, entity.Code, request.Title, request.ExpectedResult, request.RequirementId, request.Status, testCaseId, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var previousRequirementId = entity.RequirementId;
        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Title = request.Title.Trim(),
            Preconditions = TrimOrNull(request.Preconditions),
            StepsJson = SerializeStringList(request.Steps),
            ExpectedResult = request.ExpectedResult.Trim(),
            RequirementId = request.RequirementId,
            Status = NormalizeTestCaseStatus(request.Status, entity.Status),
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        var syncError = await SyncRequirementCoverageLinkAsync(request.RequirementId, entity.Id, actorUserId, cancellationToken, previousRequirementId);
        if (syncError is not null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return syncError;
        }

        await transaction.CommitAsync(cancellationToken);
        await AppendAuditAsync("update", "test_case", testCaseId, StatusCodes.Status200OK, null, cancellationToken);
        await AppendBusinessEventAsync("test_case_updated", "test_case", testCaseId, actorUserId, "Updated test case", null, null, cancellationToken);
        return await SuccessTestCaseAsync(testCaseId, cancellationToken);
    }

    public async Task<VerificationCommandResult<TestExecutionCreateResponse>> CreateTestExecutionAsync(CreateTestExecutionRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var testCase = await dbContext.Set<TestCaseEntity>().SingleOrDefaultAsync(x => x.Id == request.TestCaseId, cancellationToken);
        if (testCase is null)
        {
            return NotFound<TestExecutionCreateResponse>(ApiErrorCodes.TestCaseNotFound, "Test case not found.");
        }

        var result = request.Result.Trim().ToLowerInvariant();
        if (!TestExecutionResults.Contains(result, StringComparer.OrdinalIgnoreCase))
        {
            return Validation<TestExecutionCreateResponse>(ApiErrorCodes.RequestValidationFailed, "Execution result is invalid.");
        }

        if (request.IsSensitiveEvidence && string.IsNullOrWhiteSpace(request.EvidenceClassification))
        {
            return Validation<TestExecutionCreateResponse>(ApiErrorCodes.RestrictedClassificationRequired, "Sensitive evidence requires classification.");
        }

        var entity = new TestExecutionEntity
        {
            Id = Guid.NewGuid(),
            TestCaseId = request.TestCaseId,
            ExecutedBy = actorUserId ?? "system",
            ExecutedAt = DateTimeOffset.UtcNow,
            Result = result,
            EvidenceRef = TrimOrNull(request.EvidenceRef),
            Notes = TrimOrNull(request.Notes),
            IsSensitiveEvidence = request.IsSensitiveEvidence,
            EvidenceClassification = request.IsSensitiveEvidence ? TrimOrNull(request.EvidenceClassification) : null
        };

        dbContext.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("execute", "test_execution", entity.Id, StatusCodes.Status201Created, new { entity.Result, entity.TestCaseId }, cancellationToken);
        await AppendBusinessEventAsync("test_execution_recorded", "test_execution", entity.Id, actorUserId, "Recorded test execution", null, new { entity.Result, entity.TestCaseId }, cancellationToken);
        return Success(new TestExecutionCreateResponse(entity.Id, entity.ExecutedAt, entity.Result));
    }

    public async Task<VerificationCommandResult<ExecutionExportResponse>> ExportTestExecutionsAsync(ExecutionExportRequest request, bool canReadSensitiveEvidence, string? actorUserId, CancellationToken cancellationToken)
    {
        var query = new TestExecutionListQuery(request.TestCaseId, request.Result, request.ExecutedBy, request.From, request.To, 1, 101);
        var result = await queries.ListTestExecutionsAsync(query, canReadSensitiveEvidence, cancellationToken);
        if (result.Total > 100)
        {
            await AppendAuditAsync("export", "test_execution", Guid.Empty, StatusCodes.Status202Accepted, new { result.Total }, cancellationToken, "queued_export_threshold");
            await AppendBusinessEventAsync("test_execution_export_queued", "test_execution", Guid.Empty, actorUserId, "Queued test execution export", null, new { result.Total }, cancellationToken);
            return Success(new ExecutionExportResponse("queued", result.Total, [], "Execution export exceeded synchronous threshold and was queued."));
        }

        await AppendAuditAsync("export", "test_execution", Guid.Empty, StatusCodes.Status200OK, new { result.Total }, cancellationToken);
        await AppendBusinessEventAsync("test_execution_exported", "test_execution", Guid.Empty, actorUserId, "Exported test executions", null, new { result.Total }, cancellationToken);
        return Success(new ExecutionExportResponse("completed", result.Total, result.Items, null));
    }

    public async Task<VerificationCommandResult<UatSignoffDetailResponse>> CreateUatSignoffAsync(CreateUatSignoffRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var validation = await ValidateUatAsync(request.ProjectId, request.ScopeSummary, request.ReleaseId, request.EvidenceRefs, null, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new UatSignoffEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            ReleaseId = TrimOrNull(request.ReleaseId),
            ScopeSummary = request.ScopeSummary.Trim(),
            Status = "draft",
            DecisionReason = TrimOrNull(request.DecisionReason),
            EvidenceRefsJson = SerializeStringList(request.EvidenceRefs),
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("create", "uat_signoff", entity.Id, StatusCodes.Status201Created, new { entity.Status }, cancellationToken);
        await AppendBusinessEventAsync("uat_signoff_created", "uat_signoff", entity.Id, actorUserId, "Created UAT sign-off draft", null, null, cancellationToken);
        return await SuccessUatAsync(entity.Id, cancellationToken);
    }

    public async Task<VerificationCommandResult<UatSignoffDetailResponse>> UpdateUatSignoffAsync(Guid uatSignoffId, UpdateUatSignoffRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<UatSignoffEntity>().SingleOrDefaultAsync(x => x.Id == uatSignoffId, cancellationToken);
        if (entity is null)
        {
            return NotFound<UatSignoffDetailResponse>(ApiErrorCodes.UatSignoffNotFound, "UAT sign-off not found.");
        }

        if (!string.Equals(entity.Status, "draft", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<UatSignoffDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Only draft UAT sign-offs can be updated.");
        }

        var validation = await ValidateUatAsync(entity.ProjectId, request.ScopeSummary, request.ReleaseId, request.EvidenceRefs, uatSignoffId, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            ReleaseId = TrimOrNull(request.ReleaseId),
            ScopeSummary = request.ScopeSummary.Trim(),
            DecisionReason = TrimOrNull(request.DecisionReason),
            EvidenceRefsJson = SerializeStringList(request.EvidenceRefs),
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("update", "uat_signoff", uatSignoffId, StatusCodes.Status200OK, null, cancellationToken);
        await AppendBusinessEventAsync("uat_signoff_updated", "uat_signoff", uatSignoffId, actorUserId, "Updated UAT sign-off draft", null, null, cancellationToken);
        return await SuccessUatAsync(uatSignoffId, cancellationToken);
    }

    public async Task<VerificationCommandResult<UatSignoffDetailResponse>> SubmitUatSignoffAsync(Guid uatSignoffId, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<UatSignoffEntity>().SingleOrDefaultAsync(x => x.Id == uatSignoffId, cancellationToken);
        if (entity is null)
        {
            return NotFound<UatSignoffDetailResponse>(ApiErrorCodes.UatSignoffNotFound, "UAT sign-off not found.");
        }

        if (!string.Equals(entity.Status, "draft", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<UatSignoffDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Only draft UAT sign-offs can be submitted.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Status = "submitted",
            SubmittedBy = actorUserId,
            SubmittedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("submit", "uat_signoff", uatSignoffId, StatusCodes.Status200OK, null, cancellationToken);
        await AppendBusinessEventAsync("uat_signoff_submitted", "uat_signoff", uatSignoffId, actorUserId, "Submitted UAT sign-off", null, null, cancellationToken);
        return await SuccessUatAsync(uatSignoffId, cancellationToken);
    }

    public async Task<VerificationCommandResult<UatSignoffDetailResponse>> ApproveUatSignoffAsync(Guid uatSignoffId, VerificationDecisionRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<UatSignoffEntity>().SingleOrDefaultAsync(x => x.Id == uatSignoffId, cancellationToken);
        if (entity is null)
        {
            return NotFound<UatSignoffDetailResponse>(ApiErrorCodes.UatSignoffNotFound, "UAT sign-off not found.");
        }

        if (!string.Equals(entity.Status, "submitted", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<UatSignoffDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Only submitted UAT sign-offs can be approved.");
        }

        if (string.IsNullOrWhiteSpace(entity.ReleaseId))
        {
            return Validation<UatSignoffDetailResponse>(ApiErrorCodes.UatReleaseRequired, "UAT release reference is required.");
        }

        if (DeserializeStringList(entity.EvidenceRefsJson).Count == 0)
        {
            return Validation<UatSignoffDetailResponse>(ApiErrorCodes.UatEvidenceRequired, "UAT evidence is required before approval.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Status = "approved",
            ApprovedBy = actorUserId,
            ApprovedAt = DateTimeOffset.UtcNow,
            DecisionReason = TrimOrNull(request.Reason) ?? entity.DecisionReason,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("approve", "uat_signoff", uatSignoffId, StatusCodes.Status200OK, null, cancellationToken, TrimOrNull(request.Reason));
        await AppendBusinessEventAsync("uat_signoff_approved", "uat_signoff", uatSignoffId, actorUserId, "Approved UAT sign-off", TrimOrNull(request.Reason), null, cancellationToken);
        return await SuccessUatAsync(uatSignoffId, cancellationToken);
    }

    public async Task<VerificationCommandResult<UatSignoffDetailResponse>> RejectUatSignoffAsync(Guid uatSignoffId, VerificationDecisionRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<UatSignoffEntity>().SingleOrDefaultAsync(x => x.Id == uatSignoffId, cancellationToken);
        if (entity is null)
        {
            return NotFound<UatSignoffDetailResponse>(ApiErrorCodes.UatSignoffNotFound, "UAT sign-off not found.");
        }

        if (!string.Equals(entity.Status, "submitted", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<UatSignoffDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Only submitted UAT sign-offs can be rejected.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return Validation<UatSignoffDetailResponse>(ApiErrorCodes.DecisionReasonRequired, "Rejection reason is required.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Status = "rejected",
            DecisionReason = request.Reason.Trim(),
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("reject", "uat_signoff", uatSignoffId, StatusCodes.Status200OK, null, cancellationToken, request.Reason.Trim());
        await AppendBusinessEventAsync("uat_signoff_rejected", "uat_signoff", uatSignoffId, actorUserId, "Rejected UAT sign-off", request.Reason.Trim(), null, cancellationToken);
        return await SuccessUatAsync(uatSignoffId, cancellationToken);
    }

    private async Task<VerificationCommandResult<TestPlanDetailResponse>?> ValidateTestPlanAsync(Guid projectId, string code, string title, string scopeSummary, string ownerUserId, IReadOnlyList<Guid>? linkedRequirementIds, Guid? existingId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(scopeSummary) || string.IsNullOrWhiteSpace(ownerUserId))
        {
            return Validation<TestPlanDetailResponse>(ApiErrorCodes.RequestValidationFailed, "Test plan code, title, scope, and owner are required.");
        }

        if (!await dbContext.Projects.AnyAsync(x => x.Id == projectId, cancellationToken))
        {
            return NotFound<TestPlanDetailResponse>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        var normalizedCode = code.Trim().ToUpperInvariant();
        var duplicateExists = await dbContext.Set<TestPlanEntity>().AnyAsync(
            x => x.ProjectId == projectId && x.Code == normalizedCode && (!existingId.HasValue || x.Id != existingId.Value),
            cancellationToken);
        if (duplicateExists)
        {
            return Conflict<TestPlanDetailResponse>(ApiErrorCodes.TestPlanCodeDuplicate, "Test plan code already exists.");
        }

        if (linkedRequirementIds is { Count: > 0 })
        {
            var requirementCount = await dbContext.Set<RequirementEntity>().CountAsync(
                x => x.ProjectId == projectId && linkedRequirementIds.Contains(x.Id),
                cancellationToken);
            if (requirementCount != linkedRequirementIds.Distinct().Count())
            {
                return Validation<TestPlanDetailResponse>(ApiErrorCodes.RequestValidationFailed, "All linked requirements must belong to the same project.");
            }
        }

        return null;
    }

    private async Task<VerificationCommandResult<TestCaseDetailResponse>?> ValidateTestCaseAsync(Guid testPlanId, string code, string title, string expectedResult, Guid? requirementId, string? status, Guid? existingId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(expectedResult))
        {
            return Validation<TestCaseDetailResponse>(ApiErrorCodes.RequestValidationFailed, "Test case code, title, and expected result are required.");
        }

        var plan = await dbContext.Set<TestPlanEntity>().SingleOrDefaultAsync(x => x.Id == testPlanId, cancellationToken);
        if (plan is null)
        {
            return NotFound<TestCaseDetailResponse>(ApiErrorCodes.TestPlanNotFound, "Test plan not found.");
        }

        var duplicateExists = await dbContext.Set<TestCaseEntity>().AnyAsync(
            x => x.TestPlanId == testPlanId && x.Code == code.Trim().ToUpperInvariant() && (!existingId.HasValue || x.Id != existingId.Value),
            cancellationToken);
        if (duplicateExists)
        {
            return Conflict<TestCaseDetailResponse>(ApiErrorCodes.TestCaseCodeDuplicate, "Test case code already exists.");
        }

        if (requirementId.HasValue)
        {
            var exists = await dbContext.Set<RequirementEntity>().AnyAsync(x => x.Id == requirementId.Value && x.ProjectId == plan.ProjectId, cancellationToken);
            if (!exists)
            {
                return Validation<TestCaseDetailResponse>(ApiErrorCodes.RequestValidationFailed, "Linked requirement must belong to the same project.");
            }
        }

        var normalizedStatus = NormalizeTestCaseStatus(status);
        if (!TestCaseStatuses.Contains(normalizedStatus, StringComparer.OrdinalIgnoreCase))
        {
            return Validation<TestCaseDetailResponse>(ApiErrorCodes.RequestValidationFailed, "Test case status is invalid.");
        }

        return null;
    }

    private async Task<VerificationCommandResult<UatSignoffDetailResponse>?> ValidateUatAsync(Guid projectId, string scopeSummary, string? releaseId, IReadOnlyList<string>? evidenceRefs, Guid? existingId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(scopeSummary))
        {
            return Validation<UatSignoffDetailResponse>(ApiErrorCodes.RequestValidationFailed, "UAT scope is required.");
        }

        if (!await dbContext.Projects.AnyAsync(x => x.Id == projectId, cancellationToken))
        {
            return NotFound<UatSignoffDetailResponse>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        var currentStatus = existingId.HasValue
            ? await dbContext.Set<UatSignoffEntity>().Where(x => x.Id == existingId.Value).Select(x => x.Status).SingleAsync(cancellationToken)
            : null;
        if (currentStatus is not null && !UatStatuses.Contains(currentStatus, StringComparer.OrdinalIgnoreCase))
        {
            return Validation<UatSignoffDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "UAT status is invalid.");
        }

        if (!string.IsNullOrWhiteSpace(releaseId) && releaseId.Trim().Length > 128)
        {
            return Validation<UatSignoffDetailResponse>(ApiErrorCodes.UatReleaseRequired, "Release reference is too long.");
        }

        if (evidenceRefs is { Count: > 0 } && evidenceRefs.Any(string.IsNullOrWhiteSpace))
        {
            return Validation<UatSignoffDetailResponse>(ApiErrorCodes.UatEvidenceRequired, "Evidence references cannot contain empty items.");
        }

        return null;
    }

    private async Task<VerificationCommandResult<TestCaseDetailResponse>?> SyncRequirementCoverageLinkAsync(Guid? nextRequirementId, Guid testCaseId, string? actorUserId, CancellationToken cancellationToken, Guid? previousRequirementId = null)
    {
        var currentLinks = await dbContext.Set<TraceabilityLinkEntity>()
            .Where(x => x.TargetType == "test" && x.TargetId == testCaseId.ToString())
            .ToListAsync(cancellationToken);

        foreach (var link in currentLinks.Where(link => !nextRequirementId.HasValue || !string.Equals(link.SourceId, nextRequirementId.Value.ToString(), StringComparison.OrdinalIgnoreCase)))
        {
            var deleteResult = await requirementCommands.DeleteTraceabilityLinkAsync(link.Id, actorUserId, cancellationToken);
            if (deleteResult.Status != RequirementCommandStatus.Success)
            {
                return Validation<TestCaseDetailResponse>(deleteResult.ErrorCode ?? ApiErrorCodes.TraceabilityLinkNotFound, deleteResult.ErrorMessage ?? "Unable to remove previous traceability link.");
            }
        }

        if (!nextRequirementId.HasValue)
        {
            return null;
        }

        var existingLink = currentLinks.FirstOrDefault(x => string.Equals(x.SourceId, nextRequirementId.Value.ToString(), StringComparison.OrdinalIgnoreCase));
        if (existingLink is not null)
        {
            return null;
        }

        var createResult = await requirementCommands.CreateTraceabilityLinkAsync(
            new CreateTraceabilityLinkRequest("requirement", nextRequirementId.Value.ToString(), "test", testCaseId.ToString(), "covers"),
            actorUserId,
            cancellationToken);
        return createResult.Status switch
        {
            RequirementCommandStatus.Success => null,
            RequirementCommandStatus.Conflict => null,
            _ => Validation<TestCaseDetailResponse>(createResult.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, createResult.ErrorMessage ?? "Unable to synchronize traceability.")
        };
    }

    private async Task<VerificationCommandResult<TestPlanDetailResponse>> SuccessTestPlanAsync(Guid id, CancellationToken cancellationToken)
    {
        var detail = await queries.GetTestPlanAsync(id, cancellationToken);
        return detail is null
            ? NotFound<TestPlanDetailResponse>(ApiErrorCodes.TestPlanNotFound, "Test plan not found.")
            : Success(detail);
    }

    private async Task<VerificationCommandResult<TestCaseDetailResponse>> SuccessTestCaseAsync(Guid id, CancellationToken cancellationToken)
    {
        var detail = await queries.GetTestCaseAsync(id, canReadSensitiveEvidence: true, cancellationToken);
        return detail is null
            ? NotFound<TestCaseDetailResponse>(ApiErrorCodes.TestCaseNotFound, "Test case not found.")
            : Success(detail);
    }

    private async Task<VerificationCommandResult<UatSignoffDetailResponse>> SuccessUatAsync(Guid id, CancellationToken cancellationToken)
    {
        var detail = await queries.GetUatSignoffAsync(id, cancellationToken);
        return detail is null
            ? NotFound<UatSignoffDetailResponse>(ApiErrorCodes.UatSignoffNotFound, "UAT sign-off not found.")
            : Success(detail);
    }

    private async Task AppendAuditAsync(string action, string entityType, Guid entityId, int statusCode, object? metadata, CancellationToken cancellationToken, string? reason = null)
    {
        auditLogWriter.Append(new AuditLogEntry(
            "verification",
            action,
            entityType,
            entityId == Guid.Empty ? null : entityId.ToString(),
            StatusCode: statusCode,
            Reason: reason,
            Metadata: metadata));
        await Task.CompletedTask;
    }

    private async Task AppendBusinessEventAsync(string eventType, string entityType, Guid entityId, string? actorUserId, string summary, string? reason, object? metadata, CancellationToken cancellationToken)
    {
        await businessAuditEventWriter.AppendAsync(
            "verification",
            eventType,
            entityType,
            entityId == Guid.Empty ? "export" : entityId.ToString(),
            summary,
            reason,
            metadata,
            cancellationToken);
    }

    private static string NormalizeTestCaseStatus(string? status, string fallback = "draft") =>
        string.IsNullOrWhiteSpace(status) ? fallback : status.Trim().ToLowerInvariant();

    private static string? TrimOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string SerializeGuidList(IReadOnlyList<Guid>? values) =>
        JsonSerializer.Serialize(values?.Distinct().ToArray() ?? [], SerializerOptions);

    private static string SerializeStringList(IReadOnlyList<string>? values) =>
        JsonSerializer.Serialize(values?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray() ?? [], SerializerOptions);

    private static IReadOnlyList<string> DeserializeStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<string>>(json, SerializerOptions) ?? [];
    }

    private static VerificationCommandResult<T> Success<T>(T value) => new(VerificationCommandStatus.Success, value);
    private static VerificationCommandResult<T> NotFound<T>(string code, string message) => new(VerificationCommandStatus.NotFound, default, code, message);
    private static VerificationCommandResult<T> Validation<T>(string code, string message) => new(VerificationCommandStatus.ValidationError, default, code, message);
    private static VerificationCommandResult<T> Conflict<T>(string code, string message) => new(VerificationCommandStatus.Conflict, default, code, message);
}
