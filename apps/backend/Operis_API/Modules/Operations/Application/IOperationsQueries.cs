using Operis_API.Modules.Operations.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Operations.Application;

public sealed record AccessReviewListQuery(string? ScopeType, string? Status, string? Search, string? SortBy, string? SortOrder, int Page = 1, int PageSize = 25);
public sealed record SecurityReviewListQuery(string? ScopeType, string? Status, string? Search, string? SortBy, string? SortOrder, int Page = 1, int PageSize = 25);
public sealed record ExternalDependencyListQuery(string? DependencyType, string? Criticality, string? Status, string? Search, string? SortBy, string? SortOrder, int Page = 1, int PageSize = 25);
public sealed record ConfigurationAuditListQuery(string? Status, string? ScopeRef, string? Search, string? SortBy, string? SortOrder, int Page = 1, int PageSize = 25);

public interface IOperationsQueries
{
    Task<PagedResult<AccessReviewResponse>> ListAccessReviewsAsync(AccessReviewListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<SecurityReviewResponse>> ListSecurityReviewsAsync(SecurityReviewListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<ExternalDependencyResponse>> ListExternalDependenciesAsync(ExternalDependencyListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<ConfigurationAuditResponse>> ListConfigurationAuditsAsync(ConfigurationAuditListQuery query, CancellationToken cancellationToken);
}
