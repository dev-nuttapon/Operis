using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Audits;
using Operis_API.Modules.Audits.Application;
using Operis_API.Modules.Audits.Contracts;
using Operis_API.Shared.Security;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Audits;

public sealed class AuditsModuleHandlerTests
{
    [Fact]
    public async Task ListAuditLogsAsync_ReturnsOkResult()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.AuditLogs.Add(new Shared.Auditing.AuditLogEntity
        {
            Id = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.UtcNow,
            Module = "users",
            Action = "create",
            EntityType = "user",
            Status = "success",
            Source = "api",
            ActorType = "user",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var queries = new AuditLogQueries(dbContext, auditLogWriter);

        var result = await InvokeListAuditLogsAsync(queries);

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task ListAuditLogsAsync_WithoutPermission_ReturnsForbidden()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var auditLogWriter = new FakeAuditLogWriter();
        var queries = new AuditLogQueries(dbContext, auditLogWriter);

        var result = await InvokeListAuditLogsAsync(queries, CreateUnprivilegedPrincipal());

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task CreateAuditPlanAsync_WithoutManagePermission_ReturnsForbidden()
    {
        var result = await InvokeCreateAuditPlanAsync(CreateUnprivilegedPrincipal(), new FakeAuditComplianceCommands());

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    private static async Task<IResult> InvokeListAuditLogsAsync(IAuditLogQueries queries, ClaimsPrincipal? principal = null)
    {
        var method = typeof(AuditsModule).GetMethod(
            "ListAuditLogsAsync",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("AuditsModule.ListAuditLogsAsync was not found.");

        var task = (Task<IResult>)method.Invoke(
            null,
            [principal ?? CreateAdminPrincipal(), new PermissionMatrix(), queries, null, null, null, null, null, null, null, null, null, null, 1, 10, CancellationToken.None])!;

        return await task;
    }

    private static ClaimsPrincipal CreateAdminPrincipal() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, "operis:super_admin")], "TestAuth"));

    private static ClaimsPrincipal CreateUnprivilegedPrincipal() =>
        new(new ClaimsIdentity([], "TestAuth"));

    private static async Task<IResult> InvokeCreateAuditPlanAsync(ClaimsPrincipal principal, IAuditComplianceCommands commands)
    {
        var method = typeof(AuditsModule).GetMethod(
            "CreateAuditPlanAsync",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("AuditsModule.CreateAuditPlanAsync was not found.");

        var task = (Task<IResult>)method.Invoke(
            null,
            [principal, new CreateAuditPlanRequest(Guid.NewGuid(), "Plan", "Scope", "Criteria", DateTimeOffset.UtcNow, "auditor@example.com"), commands, new PermissionMatrix(), CancellationToken.None])!;

        return await task;
    }

    private sealed class FakeAuditComplianceCommands : IAuditComplianceCommands
    {
        public Task<AuditComplianceCommandResult<AuditPlanDetailResponse>> CreateAuditPlanAsync(CreateAuditPlanRequest request, string? actorUserId, CancellationToken cancellationToken) =>
            Task.FromResult(new AuditComplianceCommandResult<AuditPlanDetailResponse>(AuditComplianceCommandStatus.Success, new AuditPlanDetailResponse(Guid.NewGuid(), Guid.NewGuid(), "Project", request.Title, request.Scope, request.Criteria, request.PlannedAt, "planned", request.OwnerUserId, [], [], DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)));

        public Task<AuditComplianceCommandResult<AuditPlanDetailResponse>> UpdateAuditPlanAsync(Guid auditPlanId, UpdateAuditPlanRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<AuditComplianceCommandResult<AuditFindingItem>> CreateAuditFindingAsync(CreateAuditFindingRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<AuditComplianceCommandResult<AuditFindingItem>> UpdateAuditFindingAsync(Guid auditFindingId, UpdateAuditFindingRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<AuditComplianceCommandResult<AuditFindingItem>> CloseAuditFindingAsync(Guid auditFindingId, CloseAuditFindingRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<AuditComplianceCommandResult<EvidenceExportDetailResponse>> CreateEvidenceExportAsync(CreateEvidenceExportRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}
