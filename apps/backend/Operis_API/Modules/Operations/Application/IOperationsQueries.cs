using Operis_API.Modules.Operations.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Operations.Application;

public sealed record AccessReviewListQuery(string? ScopeType, string? Status, string? Search, string? SortBy, string? SortOrder, int Page = 1, int PageSize = 25);
public sealed record SecurityReviewListQuery(string? ScopeType, string? Status, string? Search, string? SortBy, string? SortOrder, int Page = 1, int PageSize = 25);
public sealed record ExternalDependencyListQuery(string? DependencyType, Guid? SupplierId, string? Criticality, string? Status, string? Search, string? SortBy, string? SortOrder, int Page = 1, int PageSize = 25);
public sealed record ConfigurationAuditListQuery(string? Status, string? ScopeRef, string? Search, string? SortBy, string? SortOrder, int Page = 1, int PageSize = 25);
public sealed record SupplierListQuery(string? SupplierType, string? OwnerUserId, string? Criticality, string? Status, DateTimeOffset? ReviewDueBefore, string? Search, string? SortBy, string? SortOrder, int Page = 1, int PageSize = 25);
public sealed record SupplierAgreementListQuery(Guid? SupplierId, string? AgreementType, string? Status, DateOnly? EffectiveToBefore, string? Search, string? SortBy, string? SortOrder, int Page = 1, int PageSize = 25);
public sealed record AccessRecertificationListQuery(string? ScopeType, string? ReviewOwnerUserId, string? Status, DateTimeOffset? PlannedBefore, string? Search, string? SortBy, string? SortOrder, int Page = 1, int PageSize = 25);
public sealed record SecurityIncidentListQuery(Guid? ProjectId, string? Severity, string? OwnerUserId, string? Status, string? Search, string? SortBy, string? SortOrder, int Page = 1, int PageSize = 25);
public sealed record VulnerabilityListQuery(string? Severity, string? OwnerUserId, string? Status, string? Search, string? SortBy, string? SortOrder, int Page = 1, int PageSize = 25);
public sealed record SecretRotationListQuery(string? SecretScope, string? VerifiedBy, string? Status, string? Search, string? SortBy, string? SortOrder, int Page = 1, int PageSize = 25);
public sealed record PrivilegedAccessEventListQuery(string? RequestedBy, string? ApprovedBy, string? UsedBy, string? Status, string? Search, string? SortBy, string? SortOrder, int Page = 1, int PageSize = 25);
public sealed record ClassificationPolicyListQuery(string? ClassificationLevel, string? Scope, string? Status, string? Search, string? SortBy, string? SortOrder, int Page = 1, int PageSize = 25);

public interface IOperationsQueries
{
    Task<PagedResult<AccessReviewResponse>> ListAccessReviewsAsync(AccessReviewListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<SecurityReviewResponse>> ListSecurityReviewsAsync(SecurityReviewListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<ExternalDependencyResponse>> ListExternalDependenciesAsync(ExternalDependencyListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<ConfigurationAuditResponse>> ListConfigurationAuditsAsync(ConfigurationAuditListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<SupplierResponse>> ListSuppliersAsync(SupplierListQuery query, CancellationToken cancellationToken);
    Task<SupplierResponse?> GetSupplierAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<SupplierAgreementResponse>> ListSupplierAgreementsAsync(SupplierAgreementListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<AccessRecertificationResponse>> ListAccessRecertificationsAsync(AccessRecertificationListQuery query, CancellationToken cancellationToken);
    Task<AccessRecertificationResponse?> GetAccessRecertificationAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<SecurityIncidentResponse>> ListSecurityIncidentsAsync(SecurityIncidentListQuery query, CancellationToken cancellationToken);
    Task<SecurityIncidentResponse?> GetSecurityIncidentAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<VulnerabilityResponse>> ListVulnerabilitiesAsync(VulnerabilityListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<SecretRotationResponse>> ListSecretRotationsAsync(SecretRotationListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<PrivilegedAccessEventResponse>> ListPrivilegedAccessEventsAsync(PrivilegedAccessEventListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<ClassificationPolicyResponse>> ListClassificationPoliciesAsync(ClassificationPolicyListQuery query, CancellationToken cancellationToken);
}
