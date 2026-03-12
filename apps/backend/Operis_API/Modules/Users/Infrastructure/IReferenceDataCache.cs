using Operis_API.Infrastructure.Persistence;

namespace Operis_API.Modules.Users.Infrastructure;

public interface IReferenceDataCache
{
    Task<IReadOnlyList<CachedDepartmentItem>> GetDepartmentsAsync(OperisDbContext dbContext, CancellationToken cancellationToken);
    Task<IReadOnlyList<CachedJobTitleItem>> GetJobTitlesAsync(OperisDbContext dbContext, CancellationToken cancellationToken);
    Task<IReadOnlyList<CachedAppRoleItem>> GetAppRolesAsync(OperisDbContext dbContext, CancellationToken cancellationToken);
    Task InvalidateDepartmentsAsync(CancellationToken cancellationToken);
    Task InvalidateJobTitlesAsync(CancellationToken cancellationToken);
    Task InvalidateAppRolesAsync(CancellationToken cancellationToken);
}

public sealed record CachedDepartmentItem(
    Guid Id,
    string Name,
    int DisplayOrder,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? DeletedReason,
    string? DeletedBy,
    DateTimeOffset? DeletedAt);

public sealed record CachedJobTitleItem(
    Guid Id,
    string Name,
    int DisplayOrder,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? DeletedReason,
    string? DeletedBy,
    DateTimeOffset? DeletedAt);
public sealed record CachedAppRoleItem(Guid Id, string Name, string KeycloakRoleName, string? Description, int DisplayOrder);
