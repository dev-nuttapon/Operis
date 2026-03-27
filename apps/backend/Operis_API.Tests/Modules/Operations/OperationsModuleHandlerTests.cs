using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Operations;
using Operis_API.Modules.Operations.Application;
using Operis_API.Modules.Operations.Contracts;
using Operis_API.Shared.Contracts;
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

    [Fact]
    public async Task CompleteAccessRecertificationAsync_WithoutApprovePermission_ReturnsForbidden()
    {
        var result = await InvokeCompleteAccessRecertificationAsync(CreateComplianceReaderPrincipal(), new FakeOperationsCommands());

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task GetSecurityIncidentAsync_WithoutReadPermission_ReturnsForbidden()
    {
        var result = await InvokeGetSecurityIncidentAsync(CreateUnauthorizedPrincipal(), new FakeOperationsQueries());

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task ReleaseLegalHoldAsync_WithoutApprovePermission_ReturnsForbidden()
    {
        var result = await InvokeReleaseLegalHoldAsync(CreateComplianceReaderPrincipal(), new FakeOperationsCommands());

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

    private static async Task<IResult> InvokeCompleteAccessRecertificationAsync(ClaimsPrincipal principal, IOperationsCommands commands)
    {
        var method = typeof(OperationsModule).GetMethod("CompleteAccessRecertificationAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("OperationsModule.CompleteAccessRecertificationAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [principal, Guid.NewGuid(), commands, new PermissionMatrix(), CancellationToken.None])!;
        return await task;
    }

    private static async Task<IResult> InvokeGetSecurityIncidentAsync(ClaimsPrincipal principal, IOperationsQueries queries)
    {
        var method = typeof(OperationsModule).GetMethod("GetSecurityIncidentAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("OperationsModule.GetSecurityIncidentAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [principal, Guid.NewGuid(), queries, new PermissionMatrix(), CancellationToken.None])!;
        return await task;
    }

    private static async Task<IResult> InvokeReleaseLegalHoldAsync(ClaimsPrincipal principal, IOperationsCommands commands)
    {
        var method = typeof(OperationsModule).GetMethod("ReleaseLegalHoldAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("OperationsModule.ReleaseLegalHoldAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [principal, Guid.NewGuid(), new ReleaseLegalHoldRequest("case closed"), commands, new PermissionMatrix(), CancellationToken.None])!;
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

    private static ClaimsPrincipal CreateUnauthorizedPrincipal() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "reader-2")], "test"));

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
        public Task<OperationsCommandResult<SupplierResponse>> CreateSupplierAsync(CreateSupplierRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<SupplierResponse>> UpdateSupplierAsync(Guid id, UpdateSupplierRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<SupplierAgreementResponse>> CreateSupplierAgreementAsync(CreateSupplierAgreementRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<SupplierAgreementResponse>> UpdateSupplierAgreementAsync(Guid id, UpdateSupplierAgreementRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<AccessRecertificationResponse>> CreateAccessRecertificationAsync(CreateAccessRecertificationRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<AccessRecertificationResponse>> UpdateAccessRecertificationAsync(Guid id, UpdateAccessRecertificationRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<AccessRecertificationDecisionResponse>> AddAccessRecertificationDecisionAsync(Guid id, AddAccessRecertificationDecisionRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<AccessRecertificationResponse>> CompleteAccessRecertificationAsync(Guid id, string? actor, CancellationToken cancellationToken) =>
            Task.FromResult(new OperationsCommandResult<AccessRecertificationResponse>(OperationsCommandStatus.Success, new AccessRecertificationResponse(id, "role", "finance-approver", DateTimeOffset.UtcNow, "owner@example.com", "completed", ["user-1"], [new AccessRecertificationDecisionResponse(Guid.NewGuid(), id, "user-1", "kept", "Still required", actor ?? "owner@example.com", DateTimeOffset.UtcNow)], null, 1, 0, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)));
        public Task<OperationsCommandResult<SecurityIncidentResponse>> CreateSecurityIncidentAsync(CreateSecurityIncidentRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<SecurityIncidentResponse>> UpdateSecurityIncidentAsync(Guid id, UpdateSecurityIncidentRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<VulnerabilityResponse>> CreateVulnerabilityAsync(CreateVulnerabilityRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<VulnerabilityResponse>> UpdateVulnerabilityAsync(Guid id, UpdateVulnerabilityRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<SecretRotationResponse>> CreateSecretRotationAsync(CreateSecretRotationRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<SecretRotationResponse>> UpdateSecretRotationAsync(Guid id, UpdateSecretRotationRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<PrivilegedAccessEventResponse>> CreatePrivilegedAccessEventAsync(CreatePrivilegedAccessEventRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<PrivilegedAccessEventResponse>> UpdatePrivilegedAccessEventAsync(Guid id, UpdatePrivilegedAccessEventRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<ClassificationPolicyResponse>> CreateClassificationPolicyAsync(CreateClassificationPolicyRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<ClassificationPolicyResponse>> UpdateClassificationPolicyAsync(Guid id, UpdateClassificationPolicyRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<BackupEvidenceResponse>> CreateBackupEvidenceAsync(CreateBackupEvidenceRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<RestoreVerificationResponse>> CreateRestoreVerificationAsync(CreateRestoreVerificationRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<DrDrillResponse>> CreateDrDrillAsync(CreateDrDrillRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<DrDrillResponse>> UpdateDrDrillAsync(Guid id, UpdateDrDrillRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<LegalHoldResponse>> CreateLegalHoldAsync(CreateLegalHoldRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<OperationsCommandResult<LegalHoldResponse>> ReleaseLegalHoldAsync(Guid id, ReleaseLegalHoldRequest request, string? actor, CancellationToken cancellationToken) =>
            Task.FromResult(new OperationsCommandResult<LegalHoldResponse>(OperationsCommandStatus.Success, new LegalHoldResponse(id, "document", "DOC-1", DateTimeOffset.UtcNow.AddDays(-1), "legal@example.com", "released", "Preserve evidence", DateTimeOffset.UtcNow, actor, request.Reason, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow)));
    }

    private sealed class FakeOperationsQueries : IOperationsQueries
    {
        public Task<PagedResult<AccessReviewResponse>> ListAccessReviewsAsync(AccessReviewListQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<PagedResult<SecurityReviewResponse>> ListSecurityReviewsAsync(SecurityReviewListQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<PagedResult<ExternalDependencyResponse>> ListExternalDependenciesAsync(ExternalDependencyListQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<PagedResult<ConfigurationAuditResponse>> ListConfigurationAuditsAsync(ConfigurationAuditListQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<PagedResult<SupplierResponse>> ListSuppliersAsync(SupplierListQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<SupplierResponse?> GetSupplierAsync(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<PagedResult<SupplierAgreementResponse>> ListSupplierAgreementsAsync(SupplierAgreementListQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<PagedResult<AccessRecertificationResponse>> ListAccessRecertificationsAsync(AccessRecertificationListQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<AccessRecertificationResponse?> GetAccessRecertificationAsync(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<PagedResult<SecurityIncidentResponse>> ListSecurityIncidentsAsync(SecurityIncidentListQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<SecurityIncidentResponse?> GetSecurityIncidentAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<SecurityIncidentResponse?>(new SecurityIncidentResponse(id, null, null, "SEC-1", "Incident", "high", DateTimeOffset.UtcNow, "owner@example.com", "reported", null, DateTimeOffset.UtcNow, null));
        public Task<PagedResult<VulnerabilityResponse>> ListVulnerabilitiesAsync(VulnerabilityListQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<PagedResult<SecretRotationResponse>> ListSecretRotationsAsync(SecretRotationListQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<PagedResult<PrivilegedAccessEventResponse>> ListPrivilegedAccessEventsAsync(PrivilegedAccessEventListQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<PagedResult<ClassificationPolicyResponse>> ListClassificationPoliciesAsync(ClassificationPolicyListQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<PagedResult<BackupEvidenceResponse>> ListBackupEvidenceAsync(BackupEvidenceListQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<PagedResult<RestoreVerificationResponse>> ListRestoreVerificationsAsync(RestoreVerificationListQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<PagedResult<DrDrillResponse>> ListDrDrillsAsync(DrDrillListQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<PagedResult<LegalHoldResponse>> ListLegalHoldsAsync(LegalHoldListQuery query, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}
