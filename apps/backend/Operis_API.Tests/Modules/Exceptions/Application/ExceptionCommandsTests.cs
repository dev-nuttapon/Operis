using Operis_API.Modules.Exceptions.Application;
using Operis_API.Modules.Exceptions.Contracts;
using Operis_API.Modules.Exceptions.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Exceptions.Application;

public sealed class ExceptionCommandsTests
{
    [Fact]
    public async Task CreateWaiverAsync_WithoutExpiry_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var sut = new ExceptionCommands(dbContext, new FakeAuditLogWriter(), new ExceptionQueries(dbContext));

        var result = await sut.CreateWaiverAsync(
            new CreateWaiverRequest("WVR-001", null, "project_governance", "Missing tailoring sign-off", "pm@example.com", "Temporary deviation", DateOnly.FromDateTime(DateTime.UtcNow), null, []),
            "pm@example.com",
            CancellationToken.None);

        Assert.Equal(ExceptionCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.WaiverExpiryRequired, result.ErrorCode);
    }

    [Fact]
    public async Task TransitionWaiverAsync_ApproveWithoutControls_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var waiverId = Guid.NewGuid();
        dbContext.Waivers.Add(new WaiverEntity
        {
            Id = waiverId,
            WaiverCode = "WVR-001",
            ProjectId = projectId,
            ProcessArea = "project_governance",
            ScopeSummary = "Tailoring review pending",
            RequestedByUserId = "pm@example.com",
            Justification = "Need temporary bypass",
            EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow),
            ExpiresAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = "submitted",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new ExceptionCommands(dbContext, new FakeAuditLogWriter(), new ExceptionQueries(dbContext));
        var result = await sut.TransitionWaiverAsync(waiverId, new TransitionWaiverRequest("approved", "Reviewed", null), "approver@example.com", CancellationToken.None);

        Assert.Equal(ExceptionCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.WaiverCompensatingControlRequired, result.ErrorCode);
    }

    private static Guid SeedProject(Operis_API.Infrastructure.Persistence.OperisDbContext dbContext)
    {
        var projectId = Guid.NewGuid();
        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = $"PRJ-{projectId.ToString()[..8]}",
            Name = "Exceptions Project",
            ProjectType = "Internal",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });

        return projectId;
    }
}
