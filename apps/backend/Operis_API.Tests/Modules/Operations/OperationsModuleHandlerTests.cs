using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Operations;
using Operis_API.Modules.Operations.Application;
using Operis_API.Modules.Operations.Contracts;
using Operis_API.Shared.Security;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Operations;

public sealed class OperationsModuleHandlerTests
{
    [Fact]
    public async Task ApproveAccessReviewAsync_WithoutApprovePermission_ReturnsForbidden()
    {
        var result = await InvokeApproveAccessReviewAsync(CreateComplianceReaderPrincipal(), new FakeOperationsCommands());

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    private static async Task<IResult> InvokeApproveAccessReviewAsync(ClaimsPrincipal principal, IOperationsCommands commands)
    {
        var method = typeof(OperationsModule).GetMethod("ApproveAccessReviewAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("OperationsModule.ApproveAccessReviewAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [principal, Guid.NewGuid(), new ApproveAccessReviewRequest("approve", "Documented evidence"), commands, new PermissionMatrix(), CancellationToken.None])!;
        return await task;
    }

    private static ClaimsPrincipal CreateComplianceReaderPrincipal()
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "reader-1"),
            new Claim(ClaimTypes.Role, "operis:audit_auditor")
        ], "test");

        return new ClaimsPrincipal(identity);
    }

    private sealed class FakeOperationsCommands : IOperationsCommands
    {
        public Task<OperationsCommandResult<AccessReviewResponse>> CreateAccessReviewAsync(CreateAccessReviewRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<AccessReviewResponse>> UpdateAccessReviewAsync(Guid id, UpdateAccessReviewRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<AccessReviewResponse>> ApproveAccessReviewAsync(Guid id, ApproveAccessReviewRequest request, string? actor, CancellationToken cancellationToken) =>
            Task.FromResult(new OperationsCommandResult<AccessReviewResponse>(OperationsCommandStatus.Success, new AccessReviewResponse(id, "role", "finance-approver", "Q2-2026", "reviewer@example.com", "Approved", request.Decision, request.DecisionRationale, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)));
        public Task<OperationsCommandResult<SecurityReviewResponse>> CreateSecurityReviewAsync(CreateSecurityReviewRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<SecurityReviewResponse>> UpdateSecurityReviewAsync(Guid id, UpdateSecurityReviewRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<ExternalDependencyResponse>> CreateExternalDependencyAsync(CreateExternalDependencyRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<ExternalDependencyResponse>> UpdateExternalDependencyAsync(Guid id, UpdateExternalDependencyRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<ConfigurationAuditResponse>> CreateConfigurationAuditAsync(CreateConfigurationAuditRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}
