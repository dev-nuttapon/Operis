using Operis_API.Infrastructure.Persistence;

namespace Operis_API.Modules.Users.Infrastructure;

public interface IReferenceDataCache
{
    Task<IReadOnlyList<CachedDivisionItem>> GetDivisionsAsync(OperisDbContext dbContext, CancellationToken cancellationToken);
    Task<IReadOnlyList<CachedDepartmentItem>> GetDepartmentsAsync(OperisDbContext dbContext, CancellationToken cancellationToken);
    Task<IReadOnlyList<CachedJobTitleItem>> GetJobTitlesAsync(OperisDbContext dbContext, CancellationToken cancellationToken);
    Task<IReadOnlyList<CachedProjectRoleItem>> GetProjectRolesAsync(OperisDbContext dbContext, CancellationToken cancellationToken);
    Task<IReadOnlyList<CachedAppRoleItem>> GetAppRolesAsync(OperisDbContext dbContext, CancellationToken cancellationToken);
    Task InvalidateDivisionsAsync(CancellationToken cancellationToken);
    Task InvalidateDepartmentsAsync(CancellationToken cancellationToken);
    Task InvalidateJobTitlesAsync(CancellationToken cancellationToken);
    Task InvalidateProjectRolesAsync(CancellationToken cancellationToken);
    Task InvalidateAppRolesAsync(CancellationToken cancellationToken);
}

public sealed record CachedDivisionItem(
    Guid Id,
    string Name,
    int DisplayOrder,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? DeletedReason,
    string? DeletedBy,
    DateTimeOffset? DeletedAt);

public sealed record CachedDepartmentItem(
    Guid Id,
    string Name,
    int DisplayOrder,
    Guid? DivisionId,
    string? DivisionName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? DeletedReason,
    string? DeletedBy,
    DateTimeOffset? DeletedAt);

public sealed record CachedJobTitleItem(
    Guid Id,
    string Name,
    int DisplayOrder,
    Guid? DivisionId,
    string? DivisionName,
    Guid? DepartmentId,
    string? DepartmentName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? DeletedReason,
    string? DeletedBy,
    DateTimeOffset? DeletedAt);

public sealed record CachedProjectRoleItem(
    Guid Id,
    string Name,
    int DisplayOrder,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? DeletedReason,
    string? DeletedBy,
    DateTimeOffset? DeletedAt);
public sealed record CachedAppRoleItem(Guid Id, string Name, string KeycloakRoleName, string? Description, int DisplayOrder);
