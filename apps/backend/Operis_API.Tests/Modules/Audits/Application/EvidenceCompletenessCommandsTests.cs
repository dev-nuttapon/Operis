using Operis_API.Modules.Audits.Application;
using Operis_API.Modules.Audits.Contracts;
using Operis_API.Modules.Audits.Infrastructure;
using Operis_API.Modules.Requirements.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Audits.Application;

public sealed class EvidenceCompletenessCommandsTests
{
    [Fact]
    public async Task CreateEvidenceRuleAsync_WithInvalidExpression_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var sut = new AuditComplianceCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new AuditComplianceQueries(dbContext));

        var result = await sut.CreateEvidenceRuleAsync(
            new CreateEvidenceRuleRequest("EV-001", "Need baseline", "requirements-traceability", "requirement_baseline", null, "draft", "optional", "reason"),
            "auditor@example.com",
            CancellationToken.None);

        Assert.Equal(AuditComplianceCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.EvidenceRuleExpressionInvalid, result.ErrorCode);
    }

    [Fact]
    public async Task EvaluateEvidenceRulesAsync_WithMissingRequirementBaseline_CreatesMissingItem()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = Guid.NewGuid();
        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = "EV-PRJ-001",
            Name = "Evidence Project",
            ProjectType = "internal",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });
        dbContext.Requirements.Add(new RequirementEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Code = "REQ-001",
            Title = "Approved requirement",
            Description = "Needs baseline",
            Priority = "high",
            OwnerUserId = "ba@example.com",
            Status = "approved",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        dbContext.EvidenceRules.Add(new EvidenceRuleEntity
        {
            Id = Guid.NewGuid(),
            RuleCode = "EV-BASE-001",
            Title = "Requirement baseline must exist",
            ProcessArea = "requirements-traceability",
            ArtifactType = "requirement_baseline",
            Status = "active",
            ExpressionType = "required",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new AuditComplianceCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new AuditComplianceQueries(dbContext));
        var result = await sut.EvaluateEvidenceRulesAsync(new EvaluateEvidenceRulesRequest(projectId, "requirements-traceability", null, null), "auditor@example.com", CancellationToken.None);

        Assert.Equal(AuditComplianceCommandStatus.Success, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value!.MissingItemCount);
        Assert.Contains(result.Value.MissingItems, item => item.ReasonCode == ApiErrorCodes.EvidenceMissingBaseline && item.ProcessArea == "requirements-traceability");
    }
}
