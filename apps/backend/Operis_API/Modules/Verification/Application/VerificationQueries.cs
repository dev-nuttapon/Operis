using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Requirements.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Modules.Verification.Contracts;
using Operis_API.Modules.Verification.Infrastructure;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Verification.Application;

public sealed class VerificationQueries(OperisDbContext dbContext) : IVerificationQueries
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<PagedResult<TestPlanListItemResponse>> ListTestPlansAsync(TestPlanListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var plans =
            from plan in dbContext.Set<TestPlanEntity>().AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on plan.ProjectId equals project.Id
            select new { Plan = plan, ProjectName = project.Name };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            plans = plans.Where(x =>
                EF.Functions.ILike(x.Plan.Code, search)
                || EF.Functions.ILike(x.Plan.Title, search)
                || EF.Functions.ILike(x.Plan.ScopeSummary, search));
        }

        if (query.ProjectId.HasValue)
        {
            plans = plans.Where(x => x.Plan.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToLowerInvariant();
            plans = plans.Where(x => x.Plan.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.OwnerUserId))
        {
            var ownerUserId = query.OwnerUserId.Trim();
            plans = plans.Where(x => x.Plan.OwnerUserId == ownerUserId);
        }

        var rows = await plans
            .OrderBy(x => x.Plan.Code)
            .ToListAsync(cancellationToken);

        var planIds = rows.Select(x => x.Plan.Id).ToArray();
        var cases = await LoadCasesAsync(planIds, cancellationToken);
        var executions = await LoadLatestExecutionLookupAsync(cases.Select(x => x.Id).ToArray(), cancellationToken);

        var allItems = rows
            .Select(row =>
            {
                var linkedRequirementIds = DeserializeGuidList(row.Plan.LinkedRequirementIdsJson);
                var planCases = cases.Where(x => x.TestPlanId == row.Plan.Id).ToArray();
                var coveredRequirementIds = planCases
                    .Where(x => x.RequirementId.HasValue && executions.ContainsKey(x.Id))
                    .Select(x => x.RequirementId!.Value)
                    .Distinct()
                    .Count();
                var coverageStatus = DeriveCoverageStatus(linkedRequirementIds.Count, coveredRequirementIds);
                return new TestPlanListItemResponse(
                    row.Plan.Id,
                    row.Plan.ProjectId,
                    row.ProjectName,
                    row.Plan.Code,
                    row.Plan.Title,
                    row.Plan.OwnerUserId,
                    row.Plan.Status,
                    coverageStatus,
                    linkedRequirementIds.Count,
                    coveredRequirementIds,
                    row.Plan.UpdatedAt);
            })
            .ToList();

        var filteredItems = string.IsNullOrWhiteSpace(query.CoverageStatus)
            ? allItems
            : allItems.Where(item => string.Equals(item.CoverageStatus, query.CoverageStatus.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
        var total = filteredItems.Count;
        var items = filteredItems.Skip(skip).Take(pageSize).ToList();
        return new PagedResult<TestPlanListItemResponse>(items, total, page, pageSize);
    }

    public async Task<TestPlanDetailResponse?> GetTestPlanAsync(Guid testPlanId, CancellationToken cancellationToken)
    {
        var item = await (
            from plan in dbContext.Set<TestPlanEntity>().AsNoTracking()
            where plan.Id == testPlanId
            join project in dbContext.Projects.AsNoTracking() on plan.ProjectId equals project.Id
            select new { Plan = plan, ProjectName = project.Name }).SingleOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return null;
        }

        var cases = await LoadCasesAsync([testPlanId], cancellationToken);
        var requirementLookup = await LoadRequirementCodeLookupAsync(cases.Where(x => x.RequirementId.HasValue).Select(x => x.RequirementId!.Value).Distinct().ToArray(), cancellationToken);
        var executions = await LoadLatestExecutionLookupAsync(cases.Select(x => x.Id).ToArray(), cancellationToken);
        var linkedRequirementIds = DeserializeGuidList(item.Plan.LinkedRequirementIdsJson);
        var coveredRequirementIds = cases.Where(x => x.RequirementId.HasValue && executions.ContainsKey(x.Id)).Select(x => x.RequirementId!.Value).Distinct().Count();
        var testCases = cases
            .Select(entity =>
            {
                executions.TryGetValue(entity.Id, out var latestExecution);
                return new TestCaseListItemResponse(
                    entity.Id,
                    entity.TestPlanId,
                    item.Plan.Code,
                    item.Plan.ProjectId,
                    item.ProjectName,
                    entity.Code,
                    entity.Title,
                    entity.Status,
                    entity.RequirementId,
                    entity.RequirementId.HasValue ? requirementLookup.GetValueOrDefault(entity.RequirementId.Value) : null,
                    latestExecution?.Result,
                    latestExecution?.ExecutedAt,
                    entity.UpdatedAt);
            })
            .OrderBy(x => x.Code)
            .ToList();

        var history = await LoadHistoryAsync(["test_plan"], [testPlanId], cancellationToken);
        return new TestPlanDetailResponse(
            item.Plan.Id,
            item.Plan.ProjectId,
            item.ProjectName,
            item.Plan.Code,
            item.Plan.Title,
            item.Plan.ScopeSummary,
            item.Plan.OwnerUserId,
            item.Plan.Status,
            item.Plan.EntryCriteria,
            item.Plan.ExitCriteria,
            linkedRequirementIds,
            DeriveCoverageStatus(linkedRequirementIds.Count, coveredRequirementIds),
            testCases,
            history,
            item.Plan.CreatedAt,
            item.Plan.UpdatedAt);
    }

    public async Task<PagedResult<TestCaseListItemResponse>> ListTestCasesAsync(TestCaseListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var cases =
            from testCase in dbContext.Set<TestCaseEntity>().AsNoTracking()
            join plan in dbContext.Set<TestPlanEntity>().AsNoTracking() on testCase.TestPlanId equals plan.Id
            join project in dbContext.Projects.AsNoTracking() on plan.ProjectId equals project.Id
            select new { TestCase = testCase, TestPlan = plan, ProjectName = project.Name };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            cases = cases.Where(x =>
                EF.Functions.ILike(x.TestCase.Code, search)
                || EF.Functions.ILike(x.TestCase.Title, search)
                || EF.Functions.ILike(x.TestCase.ExpectedResult, search));
        }

        if (query.TestPlanId.HasValue)
        {
            cases = cases.Where(x => x.TestCase.TestPlanId == query.TestPlanId.Value);
        }

        if (query.RequirementId.HasValue)
        {
            cases = cases.Where(x => x.TestCase.RequirementId == query.RequirementId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToLowerInvariant();
            cases = cases.Where(x => x.TestCase.Status == status);
        }

        var rows = await cases
            .OrderBy(x => x.TestCase.Code)
            .ToListAsync(cancellationToken);

        var caseIds = rows.Select(x => x.TestCase.Id).ToArray();
        var requirementLookup = await LoadRequirementCodeLookupAsync(rows.Where(x => x.TestCase.RequirementId.HasValue).Select(x => x.TestCase.RequirementId!.Value).Distinct().ToArray(), cancellationToken);
        var latestExecutions = await LoadLatestExecutionLookupAsync(caseIds, cancellationToken);
        var allItems = rows
            .Select(row =>
            {
                latestExecutions.TryGetValue(row.TestCase.Id, out var latestExecution);
                return new TestCaseListItemResponse(
                    row.TestCase.Id,
                    row.TestCase.TestPlanId,
                    row.TestPlan.Code,
                    row.TestPlan.ProjectId,
                    row.ProjectName,
                    row.TestCase.Code,
                    row.TestCase.Title,
                    row.TestCase.Status,
                    row.TestCase.RequirementId,
                    row.TestCase.RequirementId.HasValue ? requirementLookup.GetValueOrDefault(row.TestCase.RequirementId.Value) : null,
                    latestExecution?.Result,
                    latestExecution?.ExecutedAt,
                    row.TestCase.UpdatedAt);
            })
            .ToList();

        var filteredItems = string.IsNullOrWhiteSpace(query.LatestResult)
            ? allItems
            : allItems.Where(x => string.Equals(x.LatestResult, query.LatestResult.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
        var total = filteredItems.Count;
        var items = filteredItems.Skip(skip).Take(pageSize).ToList();
        return new PagedResult<TestCaseListItemResponse>(items, total, page, pageSize);
    }

    public async Task<TestCaseDetailResponse?> GetTestCaseAsync(Guid testCaseId, bool canReadSensitiveEvidence, CancellationToken cancellationToken)
    {
        var item = await (
            from testCase in dbContext.Set<TestCaseEntity>().AsNoTracking()
            where testCase.Id == testCaseId
            join plan in dbContext.Set<TestPlanEntity>().AsNoTracking() on testCase.TestPlanId equals plan.Id
            join project in dbContext.Projects.AsNoTracking() on plan.ProjectId equals project.Id
            select new { TestCase = testCase, TestPlan = plan, ProjectName = project.Name }).SingleOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return null;
        }

        var requirementLookup = await LoadRequirementCodeLookupAsync(item.TestCase.RequirementId.HasValue ? [item.TestCase.RequirementId.Value] : [], cancellationToken);
        var executions = await MapExecutionsAsync(item.TestCase.Id, canReadSensitiveEvidence, cancellationToken);
        var history = await LoadHistoryAsync(["test_case"], [testCaseId], cancellationToken);

        return new TestCaseDetailResponse(
            item.TestCase.Id,
            item.TestCase.TestPlanId,
            item.TestPlan.Code,
            item.TestPlan.ProjectId,
            item.ProjectName,
            item.TestCase.Code,
            item.TestCase.Title,
            item.TestCase.Preconditions,
            DeserializeStringList(item.TestCase.StepsJson),
            item.TestCase.ExpectedResult,
            item.TestCase.RequirementId,
            item.TestCase.RequirementId.HasValue ? requirementLookup.GetValueOrDefault(item.TestCase.RequirementId.Value) : null,
            item.TestCase.Status,
            executions.FirstOrDefault()?.Result,
            executions,
            history,
            item.TestCase.CreatedAt,
            item.TestCase.UpdatedAt);
    }

    public async Task<PagedResult<TestExecutionListItemResponse>> ListTestExecutionsAsync(TestExecutionListQuery query, bool canReadSensitiveEvidence, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var executions =
            from execution in dbContext.Set<TestExecutionEntity>().AsNoTracking()
            join testCase in dbContext.Set<TestCaseEntity>().AsNoTracking() on execution.TestCaseId equals testCase.Id
            select new { Execution = execution, TestCaseCode = testCase.Code };

        if (query.TestCaseId.HasValue)
        {
            executions = executions.Where(x => x.Execution.TestCaseId == query.TestCaseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Result))
        {
            var result = query.Result.Trim().ToLowerInvariant();
            executions = executions.Where(x => x.Execution.Result == result);
        }

        if (!string.IsNullOrWhiteSpace(query.ExecutedBy))
        {
            var executedBy = query.ExecutedBy.Trim();
            executions = executions.Where(x => x.Execution.ExecutedBy == executedBy);
        }

        if (query.From.HasValue)
        {
            executions = executions.Where(x => x.Execution.ExecutedAt >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            executions = executions.Where(x => x.Execution.ExecutedAt <= query.To.Value);
        }

        if (!canReadSensitiveEvidence)
        {
            executions = executions.Where(x => !x.Execution.IsSensitiveEvidence);
        }

        var total = await executions.CountAsync(cancellationToken);
        var items = await executions
            .OrderByDescending(x => x.Execution.ExecutedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new TestExecutionListItemResponse(
                x.Execution.Id,
                x.Execution.TestCaseId,
                x.TestCaseCode,
                x.Execution.ExecutedBy,
                x.Execution.ExecutedAt,
                x.Execution.Result,
                x.Execution.EvidenceRef,
                x.Execution.IsSensitiveEvidence,
                x.Execution.EvidenceClassification,
                x.Execution.Notes))
            .ToListAsync(cancellationToken);

        return new PagedResult<TestExecutionListItemResponse>(items, total, page, pageSize);
    }

    public async Task<PagedResult<UatSignoffListItemResponse>> ListUatSignoffsAsync(UatSignoffListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var signoffs =
            from signoff in dbContext.Set<UatSignoffEntity>().AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on signoff.ProjectId equals project.Id
            select new { Signoff = signoff, ProjectName = project.Name };

        if (query.ProjectId.HasValue)
        {
            signoffs = signoffs.Where(x => x.Signoff.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToLowerInvariant();
            signoffs = signoffs.Where(x => x.Signoff.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.SubmittedBy))
        {
            var submittedBy = query.SubmittedBy.Trim();
            signoffs = signoffs.Where(x => x.Signoff.SubmittedBy == submittedBy);
        }

        var total = await signoffs.CountAsync(cancellationToken);
        var items = await signoffs
            .OrderByDescending(x => x.Signoff.UpdatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new UatSignoffListItemResponse(
                x.Signoff.Id,
                x.Signoff.ProjectId,
                x.ProjectName,
                x.Signoff.ReleaseId,
                x.Signoff.Status,
                x.Signoff.SubmittedBy,
                x.Signoff.ApprovedBy,
                DeserializeStringList(x.Signoff.EvidenceRefsJson).Count,
                x.Signoff.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<UatSignoffListItemResponse>(items, total, page, pageSize);
    }

    public async Task<UatSignoffDetailResponse?> GetUatSignoffAsync(Guid uatSignoffId, CancellationToken cancellationToken)
    {
        var item = await (
            from signoff in dbContext.Set<UatSignoffEntity>().AsNoTracking()
            where signoff.Id == uatSignoffId
            join project in dbContext.Projects.AsNoTracking() on signoff.ProjectId equals project.Id
            select new { Signoff = signoff, ProjectName = project.Name }).SingleOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return null;
        }

        var history = await LoadHistoryAsync(["uat_signoff"], [uatSignoffId], cancellationToken);
        return new UatSignoffDetailResponse(
            item.Signoff.Id,
            item.Signoff.ProjectId,
            item.ProjectName,
            item.Signoff.ReleaseId,
            item.Signoff.ScopeSummary,
            item.Signoff.SubmittedBy,
            item.Signoff.SubmittedAt,
            item.Signoff.ApprovedBy,
            item.Signoff.ApprovedAt,
            item.Signoff.Status,
            item.Signoff.DecisionReason,
            DeserializeStringList(item.Signoff.EvidenceRefsJson),
            history,
            item.Signoff.CreatedAt,
            item.Signoff.UpdatedAt);
    }

    private async Task<List<TestCaseEntity>> LoadCasesAsync(IReadOnlyCollection<Guid> testPlanIds, CancellationToken cancellationToken) =>
        await dbContext.Set<TestCaseEntity>().AsNoTracking()
            .Where(x => testPlanIds.Contains(x.TestPlanId))
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

    private async Task<Dictionary<Guid, TestExecutionEntity>> LoadLatestExecutionLookupAsync(IReadOnlyCollection<Guid> testCaseIds, CancellationToken cancellationToken)
    {
        if (testCaseIds.Count == 0)
        {
            return [];
        }

        return await dbContext.Set<TestExecutionEntity>().AsNoTracking()
            .Where(x => testCaseIds.Contains(x.TestCaseId))
            .GroupBy(x => x.TestCaseId)
            .Select(group => group.OrderByDescending(x => x.ExecutedAt).First())
            .ToDictionaryAsync(x => x.TestCaseId, cancellationToken);
    }

    private async Task<Dictionary<Guid, string>> LoadRequirementCodeLookupAsync(IReadOnlyCollection<Guid> requirementIds, CancellationToken cancellationToken)
    {
        if (requirementIds.Count == 0)
        {
            return [];
        }

        return await dbContext.Set<RequirementEntity>().AsNoTracking()
            .Where(x => requirementIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);
    }

    private async Task<IReadOnlyList<TestExecutionListItemResponse>> MapExecutionsAsync(Guid testCaseId, bool canReadSensitiveEvidence, CancellationToken cancellationToken)
    {
        var query =
            from execution in dbContext.Set<TestExecutionEntity>().AsNoTracking()
            join testCase in dbContext.Set<TestCaseEntity>().AsNoTracking() on execution.TestCaseId equals testCase.Id
            where execution.TestCaseId == testCaseId
            select new { Execution = execution, TestCaseCode = testCase.Code };

        if (!canReadSensitiveEvidence)
        {
            query = query.Where(x => !x.Execution.IsSensitiveEvidence);
        }

        return await query
            .OrderByDescending(x => x.Execution.ExecutedAt)
            .Select(x => new TestExecutionListItemResponse(
                x.Execution.Id,
                x.Execution.TestCaseId,
                x.TestCaseCode,
                x.Execution.ExecutedBy,
                x.Execution.ExecutedAt,
                x.Execution.Result,
                x.Execution.EvidenceRef,
                x.Execution.IsSensitiveEvidence,
                x.Execution.EvidenceClassification,
                x.Execution.Notes))
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<VerificationHistoryItem>> LoadHistoryAsync(IReadOnlyCollection<string> entityTypes, IReadOnlyCollection<Guid> entityIds, CancellationToken cancellationToken)
    {
        var entityIdStrings = entityIds.Select(x => x.ToString()).ToArray();
        return await dbContext.BusinessAuditEvents.AsNoTracking()
            .Where(x => entityTypes.Contains(x.EntityType) && entityIdStrings.Contains(x.EntityId))
            .OrderByDescending(x => x.OccurredAt)
            .Select(x => new VerificationHistoryItem(x.Id, x.EventType, x.Summary, x.Reason, x.ActorUserId, x.OccurredAt))
            .ToListAsync(cancellationToken);
    }

    private static string DeriveCoverageStatus(int linkedRequirementCount, int coveredRequirementCount) =>
        linkedRequirementCount switch
        {
            0 => "missing",
            _ when coveredRequirementCount == 0 => "missing",
            _ when coveredRequirementCount >= linkedRequirementCount => "complete",
            _ => "partial"
        };

    private static IReadOnlyList<Guid> DeserializeGuidList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        var values = JsonSerializer.Deserialize<List<Guid>>(json, SerializerOptions);
        return values ?? [];
    }

    private static IReadOnlyList<string> DeserializeStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        var values = JsonSerializer.Deserialize<List<string>>(json, SerializerOptions);
        return values ?? [];
    }

    private static (int Page, int PageSize, int Skip) NormalizePaging(int? page, int? pageSize)
    {
        var resolvedPage = page.GetValueOrDefault(1);
        if (resolvedPage < 1)
        {
            resolvedPage = 1;
        }

        var resolvedPageSize = pageSize.GetValueOrDefault(25);
        resolvedPageSize = Math.Clamp(resolvedPageSize, 1, 100);
        return (resolvedPage, resolvedPageSize, (resolvedPage - 1) * resolvedPageSize);
    }
}
