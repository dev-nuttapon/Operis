using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Requirements;
using Operis_API.Modules.Requirements.Application;
using Operis_API.Modules.Requirements.Contracts;
using Operis_API.Modules.Requirements.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Security;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Requirements;

public sealed class RequirementsModuleHandlerTests
{
    [Fact]
    public async Task BaselineRequirementAsync_WithoutPermission_ReturnsForbidden()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = Guid.NewGuid();
        var requirementId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = "PRJ-REQ",
            Name = "Requirements Project",
            ProjectType = "Internal",
            Status = "active",
            CreatedAt = now
        });
        dbContext.Requirements.Add(new RequirementEntity
        {
            Id = requirementId,
            ProjectId = projectId,
            Code = "REQ-401",
            Title = "Requirement",
            Description = "Description",
            Priority = "high",
            OwnerUserId = "owner@example.com",
            Status = "approved",
            CurrentVersionId = versionId,
            CreatedAt = now,
            UpdatedAt = now
        });
        dbContext.RequirementVersions.Add(new RequirementVersionEntity
        {
            Id = versionId,
            RequirementId = requirementId,
            VersionNumber = 1,
            BusinessReason = "Reason",
            AcceptanceCriteria = "Criteria",
            Status = "approved",
            CreatedAt = now
        });
        dbContext.TraceabilityLinks.Add(new TraceabilityLinkEntity
        {
            Id = Guid.NewGuid(),
            SourceType = "requirement",
            SourceId = requirementId.ToString(),
            TargetType = "document",
            TargetId = "DOC-1",
            LinkRule = "implements",
            Status = "created",
            CreatedBy = "seed",
            CreatedAt = now
        });
        dbContext.TraceabilityLinks.Add(new TraceabilityLinkEntity
        {
            Id = Guid.NewGuid(),
            SourceType = "requirement",
            SourceId = requirementId.ToString(),
            TargetType = "test",
            TargetId = "TEST-1",
            LinkRule = "verifies",
            Status = "created",
            CreatedBy = "seed",
            CreatedAt = now
        });
        await dbContext.SaveChangesAsync();

        var commands = new RequirementCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new RequirementQueries(dbContext));
        var result = await InvokeBaselineRequirementAsync(commands, requirementId, CreateUnprivilegedPrincipal());

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    private static async Task<IResult> InvokeBaselineRequirementAsync(IRequirementCommands commands, Guid requirementId, ClaimsPrincipal principal)
    {
        var method = typeof(RequirementsModule).GetMethod(
            "BaselineRequirementAsync",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("RequirementsModule.BaselineRequirementAsync was not found.");

        var task = (Task<IResult>)method.Invoke(
            null,
            [principal, requirementId, commands, new PermissionMatrix(), CancellationToken.None])!;

        return await task;
    }

    private static ClaimsPrincipal CreateUnprivilegedPrincipal() =>
        new(new ClaimsIdentity([], "TestAuth"));
}
