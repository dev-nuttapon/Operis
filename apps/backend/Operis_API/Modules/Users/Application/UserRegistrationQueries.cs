using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Domain;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Users.Application;

public sealed class UserRegistrationQueries(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter) : IUserRegistrationQueries
{
    public async Task<PagedResult<RegistrationRequestResponse>> ListRegistrationRequestsAsync(RegistrationQuery query, CancellationToken cancellationToken)
    {
        var (normalizedPage, normalizedPageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var registrationRequests = dbContext.UserRegistrationRequests.AsNoTracking();

        if (query.Status.HasValue)
        {
            registrationRequests = registrationRequests.Where(x => x.Status == query.Status.Value);
        }

        if (query.From.HasValue)
        {
            registrationRequests = registrationRequests.Where(x => x.RequestedAt >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            registrationRequests = registrationRequests.Where(x => x.RequestedAt <= query.To.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchPattern = $"%{query.Search.Trim()}%";
            registrationRequests = registrationRequests.Where(x =>
                EF.Functions.ILike(x.Email, searchPattern)
                || EF.Functions.ILike(x.FirstName, searchPattern)
                || EF.Functions.ILike(x.LastName, searchPattern));
        }

        registrationRequests = ApplyRegistrationSorting(registrationRequests, query.SortBy, query.SortOrder);

        var total = await registrationRequests.CountAsync(cancellationToken);
        var items = await registrationRequests
            .Skip(skip)
            .Take(normalizedPageSize)
            .ToListAsync(cancellationToken);

        var divisions = await dbContext.Divisions
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var departments = await dbContext.Departments
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .ToDictionaryAsync(
                x => x.Id,
                x => new CachedDepartmentItem(x.Id, x.Name, x.DisplayOrder, x.DivisionId, null, x.CreatedAt, x.UpdatedAt, x.DeletedReason, x.DeletedBy, x.DeletedAt),
                cancellationToken);

        var jobTitles = await dbContext.JobTitles
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "list",
            EntityType: "registration_request",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new
            {
                count = items.Count,
                total,
                query.Status,
                query.From,
                query.To,
                page = normalizedPage,
                pageSize = normalizedPageSize,
                query.Search,
                query.SortBy,
                query.SortOrder
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PagedResult<RegistrationRequestResponse>(
            items.Select(x => ToResponse(x, divisions, departments, jobTitles)).ToList(),
            total,
            normalizedPage,
            normalizedPageSize);
    }

    public async Task<RegistrationPasswordSetupQueryResult> GetRegistrationPasswordSetupAsync(string token, CancellationToken cancellationToken)
    {
        var registrationRequest = await dbContext.UserRegistrationRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PasswordSetupToken == token, cancellationToken);

        if (registrationRequest is null)
        {
            return new RegistrationPasswordSetupQueryResult(RegistrationPasswordSetupQueryStatus.NotFound);
        }

        var divisions = await dbContext.Divisions
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var departments = await dbContext.Departments
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .ToDictionaryAsync(
                x => x.Id,
                x => new CachedDepartmentItem(x.Id, x.Name, x.DisplayOrder, x.DivisionId, null, x.CreatedAt, x.UpdatedAt, x.DeletedReason, x.DeletedBy, x.DeletedAt),
                cancellationToken);

        var jobTitles = await dbContext.JobTitles
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "view_password_setup",
            EntityType: "registration_request",
            EntityId: registrationRequest.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            ActorType: "anonymous",
            ActorEmail: registrationRequest.Email,
            DepartmentId: registrationRequest.DepartmentId,
            Metadata: new
            {
                completed = registrationRequest.PasswordSetupCompletedAt.HasValue,
                expired = registrationRequest.PasswordSetupExpiresAt.HasValue
                    && registrationRequest.PasswordSetupExpiresAt.Value <= DateTimeOffset.UtcNow
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new RegistrationPasswordSetupQueryResult(
            RegistrationPasswordSetupQueryStatus.Success,
            ToPasswordSetupResponse(registrationRequest, divisions, departments, jobTitles));
    }

    private static IQueryable<UserRegistrationRequestEntity> ApplyRegistrationSorting(IQueryable<UserRegistrationRequestEntity> query, string? sortBy, string? sortOrder)
    {
        var desc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return sortBy?.ToLowerInvariant() switch
        {
            "email" => desc ? query.OrderByDescending(x => x.Email).ThenByDescending(x => x.RequestedAt) : query.OrderBy(x => x.Email).ThenByDescending(x => x.RequestedAt),
            "status" => desc ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.RequestedAt) : query.OrderBy(x => x.Status).ThenByDescending(x => x.RequestedAt),
            _ => desc ? query.OrderByDescending(x => x.RequestedAt) : query.OrderBy(x => x.RequestedAt)
        };
    }

    private static RegistrationRequestResponse ToResponse(
        UserRegistrationRequestEntity entity,
        IReadOnlyDictionary<Guid, string> divisions,
        IReadOnlyDictionary<Guid, CachedDepartmentItem> departments,
        IReadOnlyDictionary<Guid, string> jobTitles)
    {
        Guid? divisionId = null;
        string? divisionName = null;
        string? departmentName = null;

        if (entity.DepartmentId.HasValue && departments.TryGetValue(entity.DepartmentId.Value, out var department))
        {
            divisionId = department.DivisionId;
            departmentName = department.Name;

            if (divisionId.HasValue && divisions.TryGetValue(divisionId.Value, out var resolvedDivisionName))
            {
                divisionName = resolvedDivisionName;
            }
        }

        return new RegistrationRequestResponse(
            entity.Id,
            entity.Email,
            entity.FirstName,
            entity.LastName,
            divisionId,
            divisionName,
            entity.DepartmentId,
            departmentName,
            entity.JobTitleId,
            entity.JobTitleId.HasValue && jobTitles.TryGetValue(entity.JobTitleId.Value, out var jobTitleName) ? jobTitleName : null,
            entity.Status,
            entity.RequestedAt,
            entity.ReviewedAt,
            entity.ReviewedBy,
            entity.RejectionReason,
            !string.IsNullOrWhiteSpace(entity.PasswordSetupToken) ? $"/register/setup-password/{entity.PasswordSetupToken}" : null,
            entity.PasswordSetupExpiresAt,
            entity.PasswordSetupCompletedAt);
    }

    private static RegistrationPasswordSetupDetailResponse ToPasswordSetupResponse(
        UserRegistrationRequestEntity entity,
        IReadOnlyDictionary<Guid, string> divisions,
        IReadOnlyDictionary<Guid, CachedDepartmentItem> departments,
        IReadOnlyDictionary<Guid, string> jobTitles)
    {
        string? divisionName = null;
        string? departmentName = null;

        if (entity.DepartmentId.HasValue && departments.TryGetValue(entity.DepartmentId.Value, out var department))
        {
            departmentName = department.Name;

            if (department.DivisionId.HasValue && divisions.TryGetValue(department.DivisionId.Value, out var resolvedDivisionName))
            {
                divisionName = resolvedDivisionName;
            }
        }

        return new RegistrationPasswordSetupDetailResponse(
            entity.Email,
            entity.FirstName,
            entity.LastName,
            divisionName,
            departmentName,
            entity.JobTitleId.HasValue && jobTitles.TryGetValue(entity.JobTitleId.Value, out var jobTitleName) ? jobTitleName : null,
            entity.PasswordSetupExpiresAt.HasValue && entity.PasswordSetupExpiresAt.Value <= DateTimeOffset.UtcNow,
            entity.PasswordSetupCompletedAt.HasValue,
            entity.PasswordSetupExpiresAt);
    }

    private static (int Page, int PageSize, int Skip) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);
        var skip = (normalizedPage - 1) * normalizedPageSize;
        return (normalizedPage, normalizedPageSize, skip);
    }
}
