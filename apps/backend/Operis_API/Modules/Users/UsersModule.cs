using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Domain;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Modules;

namespace Operis_API.Modules.Users;

public sealed class UsersModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KeycloakOptions>(configuration.GetSection(KeycloakOptions.SectionName));
        services.AddSingleton<IReferenceDataCache, ReferenceDataCache>();
        services.AddHttpClient<IKeycloakAdminClient, KeycloakAdminClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/", ListUsersAsync)
            .WithName("Users_List");

        group.MapDelete("/{userId}", DeleteUserAsync)
            .WithName("Users_Delete");

        group.MapPut("/{userId}", UpdateUserAsync)
            .WithName("Users_Update");

        group.MapGet("/departments", ListDepartmentsAsync)
            .AllowAnonymous()
            .WithName("Users_ListDepartments");

        group.MapGet("/roles", ListRolesAsync)
            .WithName("Users_ListRoles");

        group.MapPost("/departments", CreateDepartmentAsync)
            .WithName("Users_CreateDepartment");

        group.MapPut("/departments/{departmentId:guid}", UpdateDepartmentAsync)
            .WithName("Users_UpdateDepartment");

        group.MapDelete("/departments/{departmentId:guid}", DeleteDepartmentAsync)
            .WithName("Users_DeleteDepartment");

        group.MapGet("/job-titles", ListJobTitlesAsync)
            .AllowAnonymous()
            .WithName("Users_ListJobTitles");

        group.MapPost("/job-titles", CreateJobTitleAsync)
            .WithName("Users_CreateJobTitle");

        group.MapPut("/job-titles/{jobTitleId:guid}", UpdateJobTitleAsync)
            .WithName("Users_UpdateJobTitle");

        group.MapDelete("/job-titles/{jobTitleId:guid}", DeleteJobTitleAsync)
            .WithName("Users_DeleteJobTitle");

        group.MapPut("/me/preferences", UpdateCurrentUserPreferencesAsync)
            .WithName("Users_UpdateCurrentUserPreferences");

        group.MapPost("/register", CreateRegistrationRequestAsync)
            .AllowAnonymous()
            .WithName("Users_CreateRegistrationRequest");

        group.MapGet("/registration-requests", ListRegistrationRequestsAsync)
            .WithName("Users_ListRegistrationRequests");

        group.MapGet("/registration-requests/{token}/setup-password", GetRegistrationPasswordSetupAsync)
            .AllowAnonymous()
            .WithName("Users_GetRegistrationPasswordSetup");

        group.MapPost("/registration-requests/{token}/setup-password", CompleteRegistrationPasswordSetupAsync)
            .AllowAnonymous()
            .WithName("Users_CompleteRegistrationPasswordSetup");

        group.MapGet("/invitations", ListInvitationsAsync)
            .WithName("Users_ListInvitations");

        group.MapGet("/invitations/{token}", GetInvitationByTokenAsync)
            .AllowAnonymous()
            .WithName("Users_GetInvitationByToken");

        group.MapPost("/registration-requests/{requestId:guid}/approve", ApproveRegistrationRequestAsync)
            .WithName("Users_ApproveRegistrationRequest");

        group.MapPost("/registration-requests/{requestId:guid}/reject", RejectRegistrationRequestAsync)
            .WithName("Users_RejectRegistrationRequest");

        group.MapPost("/invitations", CreateInvitationAsync)
            .WithName("Users_CreateInvitation");

        group.MapPut("/invitations/{invitationId:guid}", UpdateInvitationAsync)
            .WithName("Users_UpdateInvitation");

        group.MapPost("/invitations/{invitationId:guid}/cancel", CancelInvitationAsync)
            .WithName("Users_CancelInvitation");

        group.MapPost("/invitations/{token}/accept", AcceptInvitationAsync)
            .AllowAnonymous()
            .WithName("Users_AcceptInvitation");

        group.MapPost("/", CreateUserAsync)
            .WithName("Users_CreateUser");

        return endpoints;
    }

    private static async Task<IResult> ListUsersAsync(
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        IKeycloakAdminClient keycloakAdminClient,
        IReferenceDataCache referenceDataCache,
        bool includeIdentity = true,
        UserStatus? status = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        string? search = null,
        string? sortBy = null,
        string? sortOrder = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var (normalizedPage, normalizedPageSize, skip) = NormalizePaging(page, pageSize);
        var baseQuery = dbContext.Users.Where(x => x.DeletedAt == null);
        if (status.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Status == status.Value);
        }
        if (from.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.CreatedAt >= from.Value);
        }
        if (to.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.CreatedAt <= to.Value);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLowerInvariant();
            baseQuery = baseQuery.Where(x =>
                x.Id.ToLower().Contains(normalizedSearch)
                || x.CreatedBy.ToLower().Contains(normalizedSearch)
                || (x.DeletedBy != null && x.DeletedBy.ToLower().Contains(normalizedSearch)));
        }
        baseQuery = ApplyUserSorting(baseQuery, sortBy, sortOrder);
        var total = await baseQuery.CountAsync(cancellationToken);
        var users = await baseQuery
            .Skip(skip)
            .Take(normalizedPageSize)
            .ToListAsync(cancellationToken);

        var departments = (await referenceDataCache.GetDepartmentsAsync(dbContext, cancellationToken))
            .ToDictionary(x => x.Id, x => x.Name);

        var jobTitles = (await referenceDataCache.GetJobTitlesAsync(dbContext, cancellationToken))
            .ToDictionary(x => x.Id, x => x.Name);

        var appRoles = await referenceDataCache.GetAppRolesAsync(dbContext, cancellationToken);

        if (!includeIdentity)
        {
            var localOnly = users.Select(x => ToResponse(x, null, [], departments, jobTitles)).ToList();
            auditLogWriter.Append(new AuditLogEntry(
                Module: "users",
                Action: "list",
                EntityType: "user",
                StatusCode: StatusCodes.Status200OK,
                Metadata: new { count = localOnly.Count, total, includeIdentity = false, status, from, to, page = normalizedPage, pageSize = normalizedPageSize, search, sortBy, sortOrder }));
            await dbContext.SaveChangesAsync(cancellationToken);
            return Results.Ok(new PagedResult<UserResponse>(localOnly, total, normalizedPage, normalizedPageSize));
        }

        var responses = new List<UserResponse>(users.Count);

        foreach (var user in users)
        {
            var profile = await ResolveKeycloakProfileAsync(user, keycloakAdminClient, cancellationToken);
            var keycloakRoles = await keycloakAdminClient.GetUserRealmRolesAsync(user.Id, cancellationToken);
            var mappedRoles = appRoles
                .Where(appRole => keycloakRoles.Any(keycloakRole => string.Equals(keycloakRole.Name, appRole.KeycloakRoleName, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .Select(x => x.Name)
                .ToArray();
            responses.Add(ToResponse(user, profile, mappedRoles, departments, jobTitles));
        }

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "list",
            EntityType: "user",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { count = responses.Count, total, includeIdentity = true, status, from, to, page = normalizedPage, pageSize = normalizedPageSize, search, sortBy, sortOrder }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(new PagedResult<UserResponse>(responses, total, normalizedPage, normalizedPageSize));
    }

    private static async Task<IResult> ListRolesAsync(
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        IReferenceDataCache referenceDataCache,
        CancellationToken cancellationToken)
    {
        var roles = (await referenceDataCache.GetAppRolesAsync(dbContext, cancellationToken))
            .Select(x => new AppRoleResponse(x.Id, x.Name, x.KeycloakRoleName, x.Description, x.DisplayOrder))
            .ToList();
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "list",
            EntityType: "app_role",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { count = roles.Count }));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(roles);
    }

    private static async Task<IResult> ListDepartmentsAsync(
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        IReferenceDataCache referenceDataCache,
        string? search = null,
        string? sortBy = null,
        string? sortOrder = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var (normalizedPage, normalizedPageSize, skip) = NormalizePaging(page, pageSize);
        var items = (await referenceDataCache.GetDepartmentsAsync(dbContext, cancellationToken))
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLowerInvariant();
            items = items.Where(x => x.Name.ToLowerInvariant().Contains(normalizedSearch));
        }

        items = ApplyDepartmentSorting(items, sortBy, sortOrder);
        var total = items.Count();
        var pagedItems = items
            .Skip(skip)
            .Take(normalizedPageSize)
            .Select(x => new MasterDataResponse(x.Id, x.Name, x.DisplayOrder, x.CreatedAt, x.UpdatedAt, x.DeletedReason, x.DeletedBy, x.DeletedAt))
            .ToList();

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "list",
            EntityType: "department",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { count = pagedItems.Count, total, page = normalizedPage, pageSize = normalizedPageSize, search, sortBy, sortOrder }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(new PagedResult<MasterDataResponse>(pagedItems, total, normalizedPage, normalizedPageSize));
    }

    private static async Task<IResult> CreateDepartmentAsync(
        CreateMasterDataRequest request,
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        IReferenceDataCache referenceDataCache,
        CancellationToken cancellationToken)
    {
        var name = NormalizeRequiredName(request.Name);
        if (name is null)
        {
            return Results.BadRequest("Department name is required.");
        }

        var exists = await dbContext.Departments.AnyAsync(x => x.Name == name && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return Results.Conflict("Department already exists.");
        }

        var entity = new DepartmentEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Departments.Add(entity);
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "create",
            EntityType: "department",
            EntityId: entity.Id.ToString(),
            StatusCode: StatusCodes.Status201Created,
            After: ToDepartmentAuditState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        await referenceDataCache.InvalidateDepartmentsAsync(cancellationToken);

        return Results.Created($"/api/v1/users/departments/{entity.Id}", ToResponse(entity));
    }

    private static async Task<IResult> UpdateDepartmentAsync(
        Guid departmentId,
        UpdateMasterDataRequest request,
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        IReferenceDataCache referenceDataCache,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.Departments.FirstOrDefaultAsync(x => x.Id == departmentId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return Results.NotFound();
        }

        var name = NormalizeRequiredName(request.Name);
        if (name is null)
        {
            return Results.BadRequest("Department name is required.");
        }

        var exists = await dbContext.Departments.AnyAsync(x => x.Id != departmentId && x.Name == name && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return Results.Conflict("Department already exists.");
        }

        var before = ToDepartmentAuditState(entity);
        entity.Name = name;
        entity.DisplayOrder = request.DisplayOrder;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "update",
            EntityType: "department",
            EntityId: entity.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            Before: before,
            After: ToDepartmentAuditState(entity),
            Changes: new
            {
                entity.Name,
                entity.DisplayOrder,
                entity.UpdatedAt
            }));
        await dbContext.SaveChangesAsync(cancellationToken);
        await referenceDataCache.InvalidateDepartmentsAsync(cancellationToken);

        return Results.Ok(ToResponse(entity));
    }

    private static async Task<IResult> DeleteDepartmentAsync(
        Guid departmentId,
        [FromBody] SoftDeleteRequest request,
        ClaimsPrincipal principal,
        [FromServices] OperisDbContext dbContext,
        [FromServices] IAuditLogWriter auditLogWriter,
        [FromServices] IReferenceDataCache referenceDataCache,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.Departments.FirstOrDefaultAsync(x => x.Id == departmentId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return Results.NotFound();
        }

        var before = ToDepartmentAuditState(entity);
        entity.DeletedAt = DateTimeOffset.UtcNow;
        entity.DeletedBy = ResolveActor(principal);
        entity.DeletedReason = NormalizeDeleteReason(request.Reason);
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "soft_delete",
            EntityType: "department",
            EntityId: entity.Id.ToString(),
            StatusCode: StatusCodes.Status204NoContent,
            Reason: entity.DeletedReason,
            Before: before,
            After: ToDepartmentAuditState(entity),
            Changes: new
            {
                entity.DeletedAt,
                entity.DeletedBy,
                entity.DeletedReason
            }));
        await dbContext.SaveChangesAsync(cancellationToken);
        await referenceDataCache.InvalidateDepartmentsAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ListJobTitlesAsync(
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        IReferenceDataCache referenceDataCache,
        string? search = null,
        string? sortBy = null,
        string? sortOrder = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var (normalizedPage, normalizedPageSize, skip) = NormalizePaging(page, pageSize);
        var items = (await referenceDataCache.GetJobTitlesAsync(dbContext, cancellationToken))
            .AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLowerInvariant();
            items = items.Where(x => x.Name.ToLowerInvariant().Contains(normalizedSearch));
        }
        items = ApplyJobTitleSorting(items, sortBy, sortOrder);
        var total = items.Count();
        var pagedItems = items
            .Skip(skip)
            .Take(normalizedPageSize)
            .Select(x => new MasterDataResponse(x.Id, x.Name, x.DisplayOrder, x.CreatedAt, x.UpdatedAt, x.DeletedReason, x.DeletedBy, x.DeletedAt))
            .ToList();

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "list",
            EntityType: "job_title",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { count = pagedItems.Count, total, page = normalizedPage, pageSize = normalizedPageSize, search, sortBy, sortOrder }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(new PagedResult<MasterDataResponse>(pagedItems, total, normalizedPage, normalizedPageSize));
    }

    private static async Task<IResult> CreateJobTitleAsync(
        CreateMasterDataRequest request,
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        IReferenceDataCache referenceDataCache,
        CancellationToken cancellationToken)
    {
        var name = NormalizeRequiredName(request.Name);
        if (name is null)
        {
            return Results.BadRequest("Job title name is required.");
        }

        var exists = await dbContext.JobTitles.AnyAsync(x => x.Name == name && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return Results.Conflict("Job title already exists.");
        }

        var entity = new JobTitleEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.JobTitles.Add(entity);
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "create",
            EntityType: "job_title",
            EntityId: entity.Id.ToString(),
            StatusCode: StatusCodes.Status201Created,
            After: ToJobTitleAuditState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        await referenceDataCache.InvalidateJobTitlesAsync(cancellationToken);

        return Results.Created($"/api/v1/users/job-titles/{entity.Id}", ToResponse(entity));
    }

    private static async Task<IResult> UpdateJobTitleAsync(
        Guid jobTitleId,
        UpdateMasterDataRequest request,
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        IReferenceDataCache referenceDataCache,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.JobTitles.FirstOrDefaultAsync(x => x.Id == jobTitleId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return Results.NotFound();
        }

        var name = NormalizeRequiredName(request.Name);
        if (name is null)
        {
            return Results.BadRequest("Job title name is required.");
        }

        var exists = await dbContext.JobTitles.AnyAsync(x => x.Id != jobTitleId && x.Name == name && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return Results.Conflict("Job title already exists.");
        }

        var before = ToJobTitleAuditState(entity);
        entity.Name = name;
        entity.DisplayOrder = request.DisplayOrder;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "update",
            EntityType: "job_title",
            EntityId: entity.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            Before: before,
            After: ToJobTitleAuditState(entity),
            Changes: new
            {
                entity.Name,
                entity.DisplayOrder,
                entity.UpdatedAt
            }));
        await dbContext.SaveChangesAsync(cancellationToken);
        await referenceDataCache.InvalidateJobTitlesAsync(cancellationToken);

        return Results.Ok(ToResponse(entity));
    }

    private static async Task<IResult> DeleteJobTitleAsync(
        Guid jobTitleId,
        [FromBody] SoftDeleteRequest request,
        ClaimsPrincipal principal,
        [FromServices] OperisDbContext dbContext,
        [FromServices] IAuditLogWriter auditLogWriter,
        [FromServices] IReferenceDataCache referenceDataCache,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.JobTitles.FirstOrDefaultAsync(x => x.Id == jobTitleId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return Results.NotFound();
        }

        var before = ToJobTitleAuditState(entity);
        entity.DeletedAt = DateTimeOffset.UtcNow;
        entity.DeletedBy = ResolveActor(principal);
        entity.DeletedReason = NormalizeDeleteReason(request.Reason);
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "soft_delete",
            EntityType: "job_title",
            EntityId: entity.Id.ToString(),
            StatusCode: StatusCodes.Status204NoContent,
            Reason: entity.DeletedReason,
            Before: before,
            After: ToJobTitleAuditState(entity),
            Changes: new
            {
                entity.DeletedAt,
                entity.DeletedBy,
                entity.DeletedReason
            }));
        await dbContext.SaveChangesAsync(cancellationToken);
        await referenceDataCache.InvalidateJobTitlesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteUserAsync(
        string userId,
        [FromBody] SoftDeleteRequest request,
        ClaimsPrincipal principal,
        [FromServices] OperisDbContext dbContext,
        [FromServices] IAuditLogWriter auditLogWriter,
        IKeycloakAdminClient keycloakAdminClient,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return Results.NotFound();
        }

        var before = ToUserAuditState(entity);
        var keycloakResult = await keycloakAdminClient.DisableUserAsync(entity.Id, cancellationToken);
        if (!keycloakResult.Success)
        {
            return Results.Problem(
                title: "Unable to disable user in Keycloak.",
                detail: keycloakResult.ErrorMessage,
                statusCode: StatusCodes.Status502BadGateway);
        }

        entity.DeletedAt = DateTimeOffset.UtcNow;
        entity.DeletedBy = ResolveActor(principal);
        entity.DeletedReason = NormalizeDeleteReason(request.Reason);
        entity.Status = UserStatus.Deleted;

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "soft_delete",
            EntityType: "user",
            EntityId: entity.Id,
            StatusCode: StatusCodes.Status204NoContent,
            Reason: entity.DeletedReason,
            DepartmentId: entity.DepartmentId,
            Before: before,
            After: ToUserAuditState(entity),
            Changes: new
            {
                status = entity.Status,
                entity.DeletedAt,
                entity.DeletedBy,
                entity.DeletedReason
            }));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> UpdateUserAsync(
        string userId,
        UpdateUserRequest request,
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        IKeycloakAdminClient keycloakAdminClient,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId && x.DeletedAt == null, cancellationToken);
        if (user is null)
        {
            return Results.NotFound();
        }

        var email = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return Results.BadRequest("Email is required.");
        }

        if (request.DepartmentId.HasValue)
        {
            var departmentExists = await dbContext.Departments.AnyAsync(x => x.Id == request.DepartmentId.Value && x.DeletedAt == null, cancellationToken);
            if (!departmentExists)
            {
                return Results.BadRequest("Department does not exist.");
            }
        }

        if (request.JobTitleId.HasValue)
        {
            var jobTitleExists = await dbContext.JobTitles.AnyAsync(x => x.Id == request.JobTitleId.Value && x.DeletedAt == null, cancellationToken);
            if (!jobTitleExists)
            {
                return Results.BadRequest("Job title does not exist.");
            }
        }

        var roleIds = request.RoleIds ?? [];
        var selectedRoles = roleIds.Count == 0
            ? []
            : await dbContext.AppRoles
                .Where(x => roleIds.Contains(x.Id) && x.DeletedAt == null)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);
        if (selectedRoles.Count != roleIds.Count)
        {
            return Results.BadRequest("One or more selected roles do not exist.");
        }

        var existingProfile = await ResolveKeycloakProfileAsync(user, keycloakAdminClient, cancellationToken);
        var before = ToUserAuditState(user, existingProfile, null);
        var existingKeycloakUser = await keycloakAdminClient.FindUserByEmailAsync(email, cancellationToken);
        if (existingKeycloakUser is not null && !string.Equals(existingKeycloakUser.Id, user.Id, StringComparison.Ordinal))
        {
            return Results.Conflict("User already exists.");
        }

        var keycloakResult = await keycloakAdminClient.UpdateUserAsync(
            user.Id,
            email,
            request.FirstName.Trim(),
            request.LastName.Trim(),
            cancellationToken);
        if (!keycloakResult.Success)
        {
            return keycloakResult.Conflict
                ? Results.Conflict("User already exists.")
                : Results.Problem(
                    title: "Unable to update user in Keycloak.",
                    detail: keycloakResult.ErrorMessage,
                    statusCode: StatusCodes.Status502BadGateway);
        }

        var managedRoleNames = await dbContext.AppRoles
            .Where(x => x.DeletedAt == null)
            .Select(x => x.KeycloakRoleName)
            .ToListAsync(cancellationToken);
        var desiredRoleNames = selectedRoles.Select(x => x.KeycloakRoleName).ToArray();
        var rolesUpdated = await keycloakAdminClient.SetManagedRolesAsync(user.Id, managedRoleNames, desiredRoleNames, cancellationToken);
        if (!rolesUpdated)
        {
            return Results.Problem(
                title: "Unable to update roles in Keycloak.",
                detail: "The selected roles could not be synchronized in Keycloak.",
                statusCode: StatusCodes.Status502BadGateway);
        }

        user.DepartmentId = request.DepartmentId;
        user.JobTitleId = request.JobTitleId;
        var selectedRoleNames = selectedRoles.Select(x => x.Name).ToArray();
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "update",
            EntityType: "user",
            EntityId: user.Id,
            StatusCode: StatusCodes.Status200OK,
            DepartmentId: user.DepartmentId,
            Before: before,
            After: ToUserAuditState(
                user,
                new KeycloakUserProfile(user.Id, email, email, request.FirstName.Trim(), request.LastName.Trim(), true, true),
                selectedRoleNames),
            Changes: new
            {
                email,
                firstName = request.FirstName.Trim(),
                lastName = request.LastName.Trim(),
                departmentId = user.DepartmentId,
                jobTitleId = user.JobTitleId,
                roleNames = selectedRoleNames
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ToResponse(user, null, selectedRoleNames, null, null));
    }

    private static async Task<IResult> CreateRegistrationRequestAsync(
        CreateRegistrationRequest request,
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        IKeycloakAdminClient keycloakAdminClient,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return Results.BadRequest("Email is required.");
        }

        var emailConflict = await ValidateEmailAvailabilityAsync(email, dbContext, keycloakAdminClient, cancellationToken: cancellationToken);
        if (emailConflict is not null)
        {
            return Results.Conflict(emailConflict);
        }

        if (request.DepartmentId.HasValue)
        {
            var departmentExists = await dbContext.Departments
                .AnyAsync(x => x.Id == request.DepartmentId.Value && x.DeletedAt == null, cancellationToken);
            if (!departmentExists)
            {
                return Results.BadRequest("Department does not exist.");
            }
        }

        if (request.JobTitleId.HasValue)
        {
            var jobTitleExists = await dbContext.JobTitles
                .AnyAsync(x => x.Id == request.JobTitleId.Value && x.DeletedAt == null, cancellationToken);
            if (!jobTitleExists)
            {
                return Results.BadRequest("Job title does not exist.");
            }
        }

        var registrationRequest = new UserRegistrationRequestEntity
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            DepartmentId = request.DepartmentId,
            JobTitleId = request.JobTitleId,
            Status = RegistrationRequestStatus.Pending,
            RequestedAt = DateTimeOffset.UtcNow
        };

        dbContext.UserRegistrationRequests.Add(registrationRequest);
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "register",
            EntityType: "registration_request",
            EntityId: registrationRequest.Id.ToString(),
            StatusCode: StatusCodes.Status201Created,
            ActorType: "anonymous",
            ActorEmail: registrationRequest.Email,
            DepartmentId: registrationRequest.DepartmentId,
            After: ToRegistrationRequestAuditState(registrationRequest)));
        await dbContext.SaveChangesAsync(cancellationToken);

        var departments = await dbContext.Departments
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var jobTitles = await dbContext.JobTitles
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        return Results.Created(
            $"/api/v1/users/registration-requests/{registrationRequest.Id}",
            ToResponse(registrationRequest, departments, jobTitles));
    }

    private static async Task<IResult> UpdateCurrentUserPreferencesAsync(
        UpdateUserPreferencesRequest request,
        ClaimsPrincipal principal,
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        CancellationToken cancellationToken)
    {
        var currentUserId = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Results.Unauthorized();
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == currentUserId, cancellationToken);
        if (user is null)
        {
            return Results.NotFound();
        }

        var before = new
        {
            user.Id,
            user.PreferredLanguage,
            user.PreferredTheme
        };
        user.PreferredLanguage = NormalizeLanguage(request.PreferredLanguage);
        user.PreferredTheme = NormalizeTheme(request.PreferredTheme);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "update_preferences",
            EntityType: "user",
            EntityId: user.Id,
            StatusCode: StatusCodes.Status204NoContent,
            DepartmentId: user.DepartmentId,
            Before: before,
            After: new
            {
                user.Id,
                user.PreferredLanguage,
                user.PreferredTheme
            },
            Changes: new
            {
                user.PreferredLanguage,
                user.PreferredTheme
            }));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ListRegistrationRequestsAsync(
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        RegistrationRequestStatus? status,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        string? search = null,
        string? sortBy = null,
        string? sortOrder = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var (normalizedPage, normalizedPageSize, skip) = NormalizePaging(page, pageSize);
        var query = dbContext.UserRegistrationRequests.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }
        if (from.HasValue)
        {
            query = query.Where(x => x.RequestedAt >= from.Value);
        }
        if (to.HasValue)
        {
            query = query.Where(x => x.RequestedAt <= to.Value);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Email.ToLower().Contains(normalizedSearch)
                || x.FirstName.ToLower().Contains(normalizedSearch)
                || x.LastName.ToLower().Contains(normalizedSearch));
        }
        query = ApplyRegistrationSorting(query, sortBy, sortOrder);

        var total = await query.CountAsync(cancellationToken);
        var requests = await query
            .Skip(skip)
            .Take(normalizedPageSize)
            .ToListAsync(cancellationToken);

        var departments = await dbContext.Departments
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var jobTitles = await dbContext.JobTitles
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .ToListAsync(cancellationToken);

        var jobTitleMap = jobTitles.ToDictionary(x => x.Id, x => x.Name);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "list",
            EntityType: "registration_request",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { count = requests.Count, total, status, from, to, page = normalizedPage, pageSize = normalizedPageSize, search, sortBy, sortOrder }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(new PagedResult<RegistrationRequestResponse>(
            requests.Select(x => ToResponse(x, departments, jobTitleMap)).ToList(),
            total,
            normalizedPage,
            normalizedPageSize));
    }

    private static async Task<IResult> ApproveRegistrationRequestAsync(
        Guid requestId,
        ReviewRegistrationRequest request,
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        IKeycloakAdminClient keycloakAdminClient,
        CancellationToken cancellationToken)
    {
        var registrationRequest = await dbContext.UserRegistrationRequests
            .FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);

        if (registrationRequest is null)
        {
            return Results.NotFound();
        }

        if (registrationRequest.Status != RegistrationRequestStatus.Pending)
        {
            return Results.BadRequest("Registration request has already been reviewed.");
        }

        var before = ToRegistrationRequestAuditState(registrationRequest);
        var existingKeycloakUser = await keycloakAdminClient.FindUserByEmailAsync(registrationRequest.Email, cancellationToken);
        var userExists = existingKeycloakUser is not null
            && await dbContext.Users.AnyAsync(x => x.Id == existingKeycloakUser.Id, cancellationToken);
        if (userExists)
        {
            return Results.Conflict("User already exists.");
        }

        var keycloakResult = await keycloakAdminClient.CreateUserAsync(
            registrationRequest.Email,
            registrationRequest.FirstName,
            registrationRequest.LastName,
            null,
            cancellationToken);
        if (!keycloakResult.Success)
        {
            return Results.Problem(
                title: "Unable to provision user in Keycloak.",
                detail: keycloakResult.ErrorMessage,
                statusCode: StatusCodes.Status502BadGateway);
        }

        var now = DateTimeOffset.UtcNow;
        var passwordSetupToken = GenerateRegistrationPasswordSetupToken();
        var passwordSetupExpiresAt = now.AddDays(7);
        registrationRequest.Status = RegistrationRequestStatus.Approved;
        registrationRequest.ReviewedAt = now;
        registrationRequest.ReviewedBy = request.ReviewedBy.Trim();
        registrationRequest.ProvisionedUserId = keycloakResult.UserId;
        registrationRequest.PasswordSetupToken = passwordSetupToken;
        registrationRequest.PasswordSetupExpiresAt = passwordSetupExpiresAt;
        registrationRequest.PasswordSetupCompletedAt = null;

        var user = new UserEntity
        {
            Id = keycloakResult.UserId ?? throw new InvalidOperationException("Keycloak user id is required."),
            Status = UserStatus.Active,
            CreatedAt = now,
            CreatedBy = registrationRequest.ReviewedBy,
            DepartmentId = registrationRequest.DepartmentId,
            JobTitleId = registrationRequest.JobTitleId
        };

        dbContext.Users.Add(user);
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "approve",
            EntityType: "registration_request",
            EntityId: registrationRequest.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            ActorEmail: request.ReviewedBy.Trim(),
            DepartmentId: registrationRequest.DepartmentId,
            Before: before,
            After: ToRegistrationRequestAuditState(registrationRequest),
            Changes: new
            {
                status = registrationRequest.Status,
                registrationRequest.ReviewedBy,
                registrationRequest.ReviewedAt,
                registrationRequest.ProvisionedUserId,
                registrationRequest.PasswordSetupToken,
                registrationRequest.PasswordSetupExpiresAt
            },
            Metadata: new
            {
                userId = user.Id,
                setupPath = $"/register/setup-password/{passwordSetupToken}"
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        var departments = await dbContext.Departments
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var jobTitles = await dbContext.JobTitles
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        return Results.Ok(ToResponse(registrationRequest, departments, jobTitles));
    }

    private static async Task<IResult> RejectRegistrationRequestAsync(
        Guid requestId,
        RejectRegistrationRequest request,
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        CancellationToken cancellationToken)
    {
        var registrationRequest = await dbContext.UserRegistrationRequests
            .FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);

        if (registrationRequest is null)
        {
            return Results.NotFound();
        }

        if (registrationRequest.Status != RegistrationRequestStatus.Pending)
        {
            return Results.BadRequest("Registration request has already been reviewed.");
        }

        var before = ToRegistrationRequestAuditState(registrationRequest);
        registrationRequest.Status = RegistrationRequestStatus.Rejected;
        registrationRequest.ReviewedBy = request.ReviewedBy.Trim();
        registrationRequest.ReviewedAt = DateTimeOffset.UtcNow;
        registrationRequest.RejectionReason = request.Reason.Trim();

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "reject",
            EntityType: "registration_request",
            EntityId: registrationRequest.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            ActorEmail: registrationRequest.ReviewedBy,
            DepartmentId: registrationRequest.DepartmentId,
            Reason: registrationRequest.RejectionReason,
            Before: before,
            After: ToRegistrationRequestAuditState(registrationRequest),
            Changes: new
            {
                status = registrationRequest.Status,
                registrationRequest.ReviewedBy,
                registrationRequest.ReviewedAt,
                registrationRequest.RejectionReason
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        var departments = await dbContext.Departments
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var jobTitles = await dbContext.JobTitles
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        return Results.Ok(ToResponse(registrationRequest, departments, jobTitles));
    }

    private static async Task<IResult> GetRegistrationPasswordSetupAsync(
        string token,
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        CancellationToken cancellationToken)
    {
        var registrationRequest = await dbContext.UserRegistrationRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PasswordSetupToken == token, cancellationToken);

        if (registrationRequest is null)
        {
            return Results.NotFound();
        }

        var departments = await dbContext.Departments
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

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
                expired = registrationRequest.PasswordSetupExpiresAt.HasValue && registrationRequest.PasswordSetupExpiresAt.Value <= DateTimeOffset.UtcNow
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ToPasswordSetupResponse(registrationRequest, departments, jobTitles));
    }

    private static async Task<IResult> CompleteRegistrationPasswordSetupAsync(
        string token,
        CompleteRegistrationPasswordSetupRequest request,
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        IKeycloakAdminClient keycloakAdminClient,
        CancellationToken cancellationToken)
    {
        var registrationRequest = await dbContext.UserRegistrationRequests
            .FirstOrDefaultAsync(x => x.PasswordSetupToken == token, cancellationToken);

        if (registrationRequest is null)
        {
            return Results.NotFound();
        }

        if (registrationRequest.Status != RegistrationRequestStatus.Approved)
        {
            return Results.BadRequest("Registration request is not approved.");
        }

        if (registrationRequest.PasswordSetupCompletedAt.HasValue)
        {
            return Results.Conflict("Password setup has already been completed.");
        }

        if (registrationRequest.PasswordSetupExpiresAt.HasValue &&
            registrationRequest.PasswordSetupExpiresAt.Value <= DateTimeOffset.UtcNow)
        {
            return Results.BadRequest("Password setup link has expired.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest("Password is required.");
        }

        if (request.Password.Length < 8)
        {
            return Results.BadRequest("Password must be at least 8 characters.");
        }

        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return Results.BadRequest("Password and confirmation do not match.");
        }

        if (string.IsNullOrWhiteSpace(registrationRequest.ProvisionedUserId))
        {
            return Results.Problem(
                title: "Unable to resolve provisioned user.",
                detail: "The approved registration is missing a provisioned Keycloak user reference.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var before = ToRegistrationRequestAuditState(registrationRequest);
        var passwordUpdated = await keycloakAdminClient.UpdatePasswordAsync(
            registrationRequest.ProvisionedUserId,
            request.Password,
            temporary: false,
            cancellationToken);
        if (!passwordUpdated.Success)
        {
            return Results.Problem(
                title: "Unable to update password in Keycloak.",
                detail: passwordUpdated.ErrorMessage,
                statusCode: StatusCodes.Status502BadGateway);
        }

        registrationRequest.PasswordSetupCompletedAt = DateTimeOffset.UtcNow;
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "complete_password_setup",
            EntityType: "registration_request",
            EntityId: registrationRequest.Id.ToString(),
            StatusCode: StatusCodes.Status204NoContent,
            ActorType: "anonymous",
            ActorEmail: registrationRequest.Email,
            DepartmentId: registrationRequest.DepartmentId,
            Before: before,
            After: ToRegistrationRequestAuditState(registrationRequest),
            Changes: new
            {
                registrationRequest.PasswordSetupCompletedAt
            },
            Metadata: new
            {
                provisionedUserId = registrationRequest.ProvisionedUserId
            },
            IsSensitive: true));
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ListInvitationsAsync(
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        InvitationStatus? status,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        string? search = null,
        string? sortBy = null,
        string? sortOrder = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var (normalizedPage, normalizedPageSize, skip) = NormalizePaging(page, pageSize);
        var query = dbContext.UserInvitations.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }
        if (from.HasValue)
        {
            query = query.Where(x => x.InvitedAt >= from.Value);
        }
        if (to.HasValue)
        {
            query = query.Where(x => x.InvitedAt <= to.Value);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Email.ToLower().Contains(normalizedSearch)
                || x.InvitedBy.ToLower().Contains(normalizedSearch));
        }
        query = ApplyInvitationSorting(query, sortBy, sortOrder);

        var departments = await dbContext.Departments
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var jobTitles = await dbContext.JobTitles
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var total = await query.CountAsync(cancellationToken);
        var invitations = await query
            .Skip(skip)
            .Take(normalizedPageSize)
            .ToListAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "list",
            EntityType: "invitation",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { count = invitations.Count, total, status, from, to, page = normalizedPage, pageSize = normalizedPageSize, search, sortBy, sortOrder }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(new PagedResult<InvitationResponse>(
            invitations.Select(x => ToResponse(x, departments, jobTitles)).ToList(),
            total,
            normalizedPage,
            normalizedPageSize));
    }

    private static async Task<IResult> GetInvitationByTokenAsync(
        string token,
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        CancellationToken cancellationToken)
    {
        var invitation = await dbContext.UserInvitations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.InvitationToken == token, cancellationToken);
        if (invitation is null)
        {
            return Results.NotFound();
        }

        var departments = await dbContext.Departments
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var jobTitles = await dbContext.JobTitles
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var status = GetInvitationStatus(invitation);
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "view_invitation",
            EntityType: "invitation",
            EntityId: invitation.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            ActorType: "anonymous",
            ActorEmail: invitation.Email,
            DepartmentId: invitation.DepartmentId,
            Metadata: new { status }));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(new InvitationDetailResponse(
            invitation.Id,
            invitation.Email,
            invitation.DepartmentId,
            invitation.DepartmentId.HasValue && departments.TryGetValue(invitation.DepartmentId.Value, out var departmentName) ? departmentName : null,
            invitation.JobTitleId,
            invitation.JobTitleId.HasValue && jobTitles.TryGetValue(invitation.JobTitleId.Value, out var jobTitleName) ? jobTitleName : null,
            status,
            invitation.InvitedAt,
            invitation.ExpiresAt));
    }

    private static async Task<IResult> CreateInvitationAsync(
        CreateInvitationRequest request,
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        IKeycloakAdminClient keycloakAdminClient,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return Results.BadRequest("Email is required.");
        }

        var invitedBy = request.InvitedBy?.Trim();
        if (string.IsNullOrWhiteSpace(invitedBy))
        {
            return Results.BadRequest("Invited by is required.");
        }

        var emailConflict = await ValidateEmailAvailabilityAsync(email, dbContext, keycloakAdminClient, cancellationToken: cancellationToken);
        if (emailConflict is not null)
        {
            return Results.Conflict(emailConflict);
        }

        if (request.ExpiresAt.HasValue && request.ExpiresAt.Value <= DateTimeOffset.UtcNow)
        {
            return Results.BadRequest("Expiration date must be in the future.");
        }

        if (request.DepartmentId.HasValue)
        {
            var departmentExists = await dbContext.Departments.AnyAsync(x => x.Id == request.DepartmentId.Value && x.DeletedAt == null, cancellationToken);
            if (!departmentExists)
            {
                return Results.BadRequest("Department does not exist.");
            }
        }

        if (request.JobTitleId.HasValue)
        {
            var jobTitleExists = await dbContext.JobTitles.AnyAsync(x => x.Id == request.JobTitleId.Value && x.DeletedAt == null, cancellationToken);
            if (!jobTitleExists)
            {
                return Results.BadRequest("Job title does not exist.");
            }
        }

        var invitation = new UserInvitationEntity
        {
            Id = Guid.NewGuid(),
            Email = email,
            InvitationToken = GenerateInvitationToken(),
            InvitedBy = invitedBy,
            DepartmentId = request.DepartmentId,
            JobTitleId = request.JobTitleId,
            Status = InvitationStatus.Pending,
            InvitedAt = DateTimeOffset.UtcNow,
            ExpiresAt = request.ExpiresAt
        };

        dbContext.UserInvitations.Add(invitation);
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "invite",
            EntityType: "invitation",
            EntityId: invitation.Id.ToString(),
            StatusCode: StatusCodes.Status201Created,
            ActorEmail: invitedBy,
            DepartmentId: invitation.DepartmentId,
            After: ToInvitationAuditState(invitation),
            Metadata: new
            {
                setupPath = $"/invite/{invitation.InvitationToken}"
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        var departments = await dbContext.Departments
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var jobTitles = await dbContext.JobTitles
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        return Results.Created($"/api/v1/users/invitations/{invitation.Id}", ToResponse(invitation, departments, jobTitles));
    }

    private static async Task<IResult> UpdateInvitationAsync(
        Guid invitationId,
        UpdateInvitationRequest request,
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        IKeycloakAdminClient keycloakAdminClient,
        CancellationToken cancellationToken)
    {
        var invitation = await dbContext.UserInvitations.FirstOrDefaultAsync(x => x.Id == invitationId, cancellationToken);
        if (invitation is null)
        {
            return Results.NotFound();
        }

        var status = GetInvitationStatus(invitation);
        if (status == InvitationStatus.Accepted)
        {
            return Results.BadRequest("Accepted invitation cannot be updated.");
        }

        if (status == InvitationStatus.Cancelled)
        {
            return Results.BadRequest("Cancelled invitation cannot be updated.");
        }

        if (status == InvitationStatus.Rejected)
        {
            return Results.BadRequest("Rejected invitation cannot be updated.");
        }

        var email = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return Results.BadRequest("Email is required.");
        }

        if (request.ExpiresAt.HasValue && request.ExpiresAt.Value <= DateTimeOffset.UtcNow)
        {
            return Results.BadRequest("Expiration date must be in the future.");
        }

        if (request.DepartmentId.HasValue)
        {
            var departmentExists = await dbContext.Departments.AnyAsync(x => x.Id == request.DepartmentId.Value && x.DeletedAt == null, cancellationToken);
            if (!departmentExists)
            {
                return Results.BadRequest("Department does not exist.");
            }
        }

        if (request.JobTitleId.HasValue)
        {
            var jobTitleExists = await dbContext.JobTitles.AnyAsync(x => x.Id == request.JobTitleId.Value && x.DeletedAt == null, cancellationToken);
            if (!jobTitleExists)
            {
                return Results.BadRequest("Job title does not exist.");
            }
        }

        var before = ToInvitationAuditState(invitation);
        var emailChanged = !string.Equals(invitation.Email, email, StringComparison.OrdinalIgnoreCase);
        if (emailChanged)
        {
            var emailConflict = await ValidateEmailAvailabilityAsync(
                email,
                dbContext,
                keycloakAdminClient,
                cancellationToken,
                ignoredInvitationId: invitation.Id);
            if (emailConflict is not null)
            {
                return Results.Conflict(emailConflict);
            }
        }

        invitation.Email = email;
        invitation.DepartmentId = request.DepartmentId;
        invitation.JobTitleId = request.JobTitleId;
        invitation.ExpiresAt = request.ExpiresAt;
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "update",
            EntityType: "invitation",
            EntityId: invitation.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            ActorEmail: invitation.InvitedBy,
            DepartmentId: invitation.DepartmentId,
            Before: before,
            After: ToInvitationAuditState(invitation),
            Changes: new
            {
                invitation.Email,
                invitation.DepartmentId,
                invitation.JobTitleId,
                invitation.ExpiresAt
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        var departments = await dbContext.Departments
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var jobTitles = await dbContext.JobTitles
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        return Results.Ok(ToResponse(invitation, departments, jobTitles));
    }

    private static async Task<IResult> CancelInvitationAsync(
        Guid invitationId,
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        CancellationToken cancellationToken)
    {
        var invitation = await dbContext.UserInvitations.FirstOrDefaultAsync(x => x.Id == invitationId, cancellationToken);
        if (invitation is null)
        {
            return Results.NotFound();
        }

        var before = ToInvitationAuditState(invitation);
        var status = GetInvitationStatus(invitation);
        if (status == InvitationStatus.Accepted)
        {
            return Results.BadRequest("Accepted invitation cannot be cancelled.");
        }

        invitation.Status = InvitationStatus.Cancelled;
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "cancel_invitation",
            EntityType: "invitation",
            EntityId: invitation.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            ActorEmail: invitation.InvitedBy,
            DepartmentId: invitation.DepartmentId,
            Before: before,
            After: ToInvitationAuditState(invitation),
            Changes: new
            {
                invitation.Status
            }));
        await dbContext.SaveChangesAsync(cancellationToken);
        var departments = await dbContext.Departments
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var jobTitles = await dbContext.JobTitles
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        return Results.Ok(ToResponse(invitation, departments, jobTitles));
    }

    private static async Task<IResult> AcceptInvitationAsync(
        string token,
        AcceptInvitationRequest request,
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        IKeycloakAdminClient keycloakAdminClient,
        CancellationToken cancellationToken)
    {
        var invitation = await dbContext.UserInvitations
            .FirstOrDefaultAsync(x => x.InvitationToken == token, cancellationToken);
        if (invitation is null)
        {
            return Results.NotFound();
        }

        var before = ToInvitationAuditState(invitation);
        var status = GetInvitationStatus(invitation);
        if (status == InvitationStatus.Accepted)
        {
            return Results.Conflict("Invitation has already been accepted.");
        }

        if (status == InvitationStatus.Rejected)
        {
            return Results.Conflict("Invitation has already been rejected.");
        }

        if (status == InvitationStatus.Cancelled)
        {
            return Results.BadRequest("Invitation has been cancelled.");
        }

        if (status == InvitationStatus.Expired)
        {
            return Results.BadRequest("Invitation has expired.");
        }

        var password = request.Password?.Trim();
        if (string.IsNullOrWhiteSpace(password))
        {
            return Results.BadRequest("Password is required.");
        }

        if (password.Length < 8)
        {
            return Results.BadRequest("Password must be at least 8 characters.");
        }

        if (!string.Equals(password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return Results.BadRequest("Password and confirmation do not match.");
        }

        var emailConflict = await ValidateEmailAvailabilityAsync(
            invitation.Email,
            dbContext,
            keycloakAdminClient,
            ignoredInvitationId: invitation.Id,
            cancellationToken: cancellationToken);
        if (emailConflict is not null)
        {
            return Results.Conflict(emailConflict);
        }

        var keycloakResult = await keycloakAdminClient.CreateUserAsync(
            invitation.Email,
            request.FirstName.Trim(),
            request.LastName.Trim(),
            password,
            cancellationToken);
        if (!keycloakResult.Success)
        {
            return Results.Problem(
                title: "Unable to provision user in Keycloak.",
                detail: keycloakResult.ErrorMessage,
                statusCode: StatusCodes.Status502BadGateway);
        }

        var user = new UserEntity
        {
            Id = keycloakResult.UserId ?? throw new InvalidOperationException("Keycloak user id is required."),
            Status = UserStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = invitation.InvitedBy,
            DepartmentId = invitation.DepartmentId,
            JobTitleId = invitation.JobTitleId
        };

        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = DateTimeOffset.UtcNow;

        dbContext.Users.Add(user);
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "accept_invitation",
            EntityType: "invitation",
            EntityId: invitation.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            ActorType: "anonymous",
            ActorUserId: user.Id,
            ActorEmail: invitation.Email,
            DepartmentId: invitation.DepartmentId,
            Before: before,
            After: ToInvitationAuditState(invitation),
            Changes: new
            {
                invitation.Status,
                invitation.AcceptedAt
            },
            Metadata: new
            {
                userId = user.Id,
                firstName = request.FirstName.Trim(),
                lastName = request.LastName.Trim()
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        var departments = await dbContext.Departments
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var jobTitles = await dbContext.JobTitles
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        return Results.Ok(ToResponse(invitation, departments, jobTitles));
    }

    private static async Task<IResult> CreateUserAsync(
        CreateUserRequest request,
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
        IKeycloakAdminClient keycloakAdminClient,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return Results.BadRequest("Email is required.");
        }

        var password = request.Password?.Trim();
        if (string.IsNullOrWhiteSpace(password))
        {
            return Results.BadRequest("Password is required.");
        }

        if (password.Length < 8)
        {
            return Results.BadRequest("Password must be at least 8 characters.");
        }

        if (!string.Equals(password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return Results.BadRequest("Password and confirmation do not match.");
        }

        if (request.DepartmentId.HasValue)
        {
            var departmentExists = await dbContext.Departments.AnyAsync(x => x.Id == request.DepartmentId.Value, cancellationToken);
            if (!departmentExists)
            {
                return Results.BadRequest("Department does not exist.");
            }
        }

        if (request.JobTitleId.HasValue)
        {
            var jobTitleExists = await dbContext.JobTitles.AnyAsync(x => x.Id == request.JobTitleId.Value, cancellationToken);
            if (!jobTitleExists)
            {
                return Results.BadRequest("Job title does not exist.");
            }
        }

        var roleIds = request.RoleIds ?? [];
        var selectedRoles = roleIds.Count == 0
            ? []
            : await dbContext.AppRoles
                .Where(x => roleIds.Contains(x.Id) && x.DeletedAt == null)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);

        if (selectedRoles.Count != roleIds.Count)
        {
            return Results.BadRequest("One or more selected roles do not exist.");
        }

        var existingKeycloakUser = await keycloakAdminClient.FindUserByEmailAsync(email, cancellationToken);
        var userExists = existingKeycloakUser is not null
            && await dbContext.Users.AnyAsync(x => x.Id == existingKeycloakUser.Id, cancellationToken);
        if (userExists)
        {
            return Results.Conflict("User already exists.");
        }

        var keycloakResult = await keycloakAdminClient.CreateUserAsync(
            email,
            request.FirstName.Trim(),
            request.LastName.Trim(),
            password,
            cancellationToken);
        if (!keycloakResult.Success)
        {
            return Results.Problem(
                title: "Unable to provision user in Keycloak.",
                detail: keycloakResult.ErrorMessage,
                statusCode: StatusCodes.Status502BadGateway);
        }

        var now = DateTimeOffset.UtcNow;
        var user = new UserEntity
        {
            Id = keycloakResult.UserId ?? throw new InvalidOperationException("Keycloak user id is required."),
            Status = UserStatus.Active,
            CreatedAt = now,
            CreatedBy = request.CreatedBy.Trim(),
            DepartmentId = request.DepartmentId,
            JobTitleId = request.JobTitleId
        };

        var keycloakRoleNames = selectedRoles.Select(x => x.KeycloakRoleName).ToArray();
        if (keycloakRoleNames.Length > 0)
        {
            var roleAssigned = await keycloakAdminClient.AssignRealmRolesAsync(user.Id, keycloakRoleNames, cancellationToken);
            if (!roleAssigned)
            {
                return Results.Problem(
                    title: "Unable to assign roles in Keycloak.",
                    detail: "The selected roles could not be mapped in Keycloak.",
                    statusCode: StatusCodes.Status502BadGateway);
            }
        }

        dbContext.Users.Add(user);
        var selectedRoleNames = selectedRoles.Select(x => x.Name).ToArray();
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "create",
            EntityType: "user",
            EntityId: user.Id,
            StatusCode: StatusCodes.Status201Created,
            DepartmentId: user.DepartmentId,
            After: ToUserAuditState(
                user,
                new KeycloakUserProfile(user.Id, email, email, request.FirstName.Trim(), request.LastName.Trim(), true, true),
                selectedRoleNames),
            Metadata: new
            {
                roleNames = selectedRoleNames
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/v1/users/{user.Id}", ToResponse(user, null, selectedRoleNames, null, null));
    }

    private static async Task<KeycloakUserProfile?> ResolveKeycloakProfileAsync(
        UserEntity user,
        IKeycloakAdminClient keycloakAdminClient,
        CancellationToken cancellationToken)
    {
        return string.IsNullOrWhiteSpace(user.Id)
            ? null
            : await keycloakAdminClient.GetUserByIdAsync(user.Id, cancellationToken);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string ResolveActor(ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.Email)
        ?? principal.FindFirstValue("preferred_username")
        ?? principal.FindFirstValue("name")
        ?? principal.FindFirstValue("sub")
        ?? "system";

    private static string? NormalizeLanguage(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().ToLowerInvariant();
        return normalized.Length > 16 ? normalized[..16] : normalized;
    }

    private static string? NormalizeTheme(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().ToLowerInvariant();
        return normalized is "light" or "dark" or "system" ? normalized : null;
    }

    private static string? NormalizeRequiredName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        return normalized.Length > 120 ? normalized[..120] : normalized;
    }

    private static string NormalizeDeleteReason(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "No reason provided";
        }

        var normalized = value.Trim();
        return normalized.Length > 500 ? normalized[..500] : normalized;
    }

    private static string GenerateInvitationToken()
    {
        Span<byte> bytes = stackalloc byte[24];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string GenerateRegistrationPasswordSetupToken()
    {
        Span<byte> bytes = stackalloc byte[24];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static InvitationStatus GetInvitationStatus(UserInvitationEntity entity)
    {
        if (entity.Status == InvitationStatus.Pending && entity.ExpiresAt.HasValue && entity.ExpiresAt.Value <= DateTimeOffset.UtcNow)
        {
            return InvitationStatus.Expired;
        }

        return entity.Status;
    }

    private static async Task<string?> ValidateEmailAvailabilityAsync(
        string email,
        OperisDbContext dbContext,
        IKeycloakAdminClient keycloakAdminClient,
        CancellationToken cancellationToken,
        Guid? ignoredInvitationId = null)
    {
        var userExists = await LocalUserExistsForEmailAsync(email, dbContext, keycloakAdminClient, cancellationToken);
        if (userExists)
        {
            return "User already exists.";
        }

        var pendingInvitationExists = await dbContext.UserInvitations
            .AnyAsync(
                x => x.Email == email
                    && x.Status == InvitationStatus.Pending
                    && (!x.ExpiresAt.HasValue || x.ExpiresAt > DateTimeOffset.UtcNow)
                    && (!ignoredInvitationId.HasValue || x.Id != ignoredInvitationId.Value),
                cancellationToken);
        if (pendingInvitationExists)
        {
            return "Pending invitation already exists.";
        }

        var pendingRequestExists = await dbContext.UserRegistrationRequests
            .AnyAsync(x => x.Email == email && x.Status == RegistrationRequestStatus.Pending, cancellationToken);
        if (pendingRequestExists)
        {
            return "Pending registration request already exists.";
        }

        return null;
    }

    private static async Task<bool> LocalUserExistsForEmailAsync(
        string email,
        OperisDbContext dbContext,
        IKeycloakAdminClient keycloakAdminClient,
        CancellationToken cancellationToken)
    {
        var keycloakUser = await keycloakAdminClient.FindUserByEmailAsync(email, cancellationToken);
        return keycloakUser is not null;
    }

    private static (int page, int pageSize, int skip) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = Math.Clamp(pageSize, 10, 100);
        var skip = (normalizedPage - 1) * normalizedPageSize;
        return (normalizedPage, normalizedPageSize, skip);
    }

    private static bool IsDescending(string? sortOrder) =>
        string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

    private static IQueryable<UserEntity> ApplyUserSorting(IQueryable<UserEntity> query, string? sortBy, string? sortOrder)
    {
        var desc = IsDescending(sortOrder);
        return sortBy?.ToLowerInvariant() switch
        {
            "status" => desc ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.CreatedAt) : query.OrderBy(x => x.Status).ThenByDescending(x => x.CreatedAt),
            "createdby" => desc ? query.OrderByDescending(x => x.CreatedBy).ThenByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedBy).ThenByDescending(x => x.CreatedAt),
            _ => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt)
        };
    }

    private static IQueryable<DepartmentEntity> ApplyDepartmentSorting(IQueryable<DepartmentEntity> query, string? sortBy, string? sortOrder)
    {
        var desc = IsDescending(sortOrder);
        return sortBy?.ToLowerInvariant() switch
        {
            "name" => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "createdat" => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            _ => desc ? query.OrderByDescending(x => x.DisplayOrder).ThenByDescending(x => x.Name) : query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
        };
    }

    private static IEnumerable<CachedDepartmentItem> ApplyDepartmentSorting(IEnumerable<CachedDepartmentItem> query, string? sortBy, string? sortOrder)
    {
        var desc = IsDescending(sortOrder);
        return sortBy?.ToLowerInvariant() switch
        {
            "name" => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "createdat" => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            _ => desc ? query.OrderByDescending(x => x.DisplayOrder).ThenByDescending(x => x.Name) : query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
        };
    }

    private static IQueryable<JobTitleEntity> ApplyJobTitleSorting(IQueryable<JobTitleEntity> query, string? sortBy, string? sortOrder)
    {
        var desc = IsDescending(sortOrder);
        return sortBy?.ToLowerInvariant() switch
        {
            "name" => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "createdat" => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            _ => desc ? query.OrderByDescending(x => x.DisplayOrder).ThenByDescending(x => x.Name) : query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
        };
    }

    private static IEnumerable<CachedJobTitleItem> ApplyJobTitleSorting(IEnumerable<CachedJobTitleItem> query, string? sortBy, string? sortOrder)
    {
        var desc = IsDescending(sortOrder);
        return sortBy?.ToLowerInvariant() switch
        {
            "name" => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "createdat" => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            _ => desc ? query.OrderByDescending(x => x.DisplayOrder).ThenByDescending(x => x.Name) : query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
        };
    }

    private static IQueryable<UserRegistrationRequestEntity> ApplyRegistrationSorting(IQueryable<UserRegistrationRequestEntity> query, string? sortBy, string? sortOrder)
    {
        var desc = IsDescending(sortOrder);
        return sortBy?.ToLowerInvariant() switch
        {
            "email" => desc ? query.OrderByDescending(x => x.Email).ThenByDescending(x => x.RequestedAt) : query.OrderBy(x => x.Email).ThenByDescending(x => x.RequestedAt),
            "status" => desc ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.RequestedAt) : query.OrderBy(x => x.Status).ThenByDescending(x => x.RequestedAt),
            _ => desc ? query.OrderByDescending(x => x.RequestedAt) : query.OrderBy(x => x.RequestedAt)
        };
    }

    private static IQueryable<UserInvitationEntity> ApplyInvitationSorting(IQueryable<UserInvitationEntity> query, string? sortBy, string? sortOrder)
    {
        var desc = IsDescending(sortOrder);
        return sortBy?.ToLowerInvariant() switch
        {
            "email" => desc ? query.OrderByDescending(x => x.Email).ThenByDescending(x => x.InvitedAt) : query.OrderBy(x => x.Email).ThenByDescending(x => x.InvitedAt),
            "status" => desc ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.InvitedAt) : query.OrderBy(x => x.Status).ThenByDescending(x => x.InvitedAt),
            "expiresat" => desc ? query.OrderByDescending(x => x.ExpiresAt).ThenByDescending(x => x.InvitedAt) : query.OrderBy(x => x.ExpiresAt).ThenByDescending(x => x.InvitedAt),
            _ => desc ? query.OrderByDescending(x => x.InvitedAt) : query.OrderBy(x => x.InvitedAt)
        };
    }

    private static object ToDepartmentAuditState(DepartmentEntity entity) => new
    {
        entity.Id,
        entity.Name,
        entity.DisplayOrder,
        entity.CreatedAt,
        entity.UpdatedAt,
        entity.DeletedReason,
        entity.DeletedBy,
        entity.DeletedAt
    };

    private static object ToJobTitleAuditState(JobTitleEntity entity) => new
    {
        entity.Id,
        entity.Name,
        entity.DisplayOrder,
        entity.CreatedAt,
        entity.UpdatedAt,
        entity.DeletedReason,
        entity.DeletedBy,
        entity.DeletedAt
    };

    private static object ToUserAuditState(
        UserEntity entity,
        KeycloakUserProfile? keycloakProfile = null,
        IReadOnlyList<string>? roleNames = null) => new
    {
        entity.Id,
        entity.Status,
        entity.CreatedAt,
        entity.CreatedBy,
        entity.DepartmentId,
        entity.JobTitleId,
        entity.PreferredLanguage,
        entity.PreferredTheme,
        entity.DeletedReason,
        entity.DeletedBy,
        entity.DeletedAt,
        Email = keycloakProfile?.Email,
        Username = keycloakProfile?.Username,
        FirstName = keycloakProfile?.FirstName,
        LastName = keycloakProfile?.LastName,
        Enabled = keycloakProfile?.Enabled,
        EmailVerified = keycloakProfile?.EmailVerified,
        Roles = roleNames ?? []
    };

    private static object ToRegistrationRequestAuditState(UserRegistrationRequestEntity entity) => new
    {
        entity.Id,
        entity.Email,
        entity.FirstName,
        entity.LastName,
        entity.DepartmentId,
        entity.JobTitleId,
        entity.ProvisionedUserId,
        entity.PasswordSetupToken,
        entity.PasswordSetupExpiresAt,
        entity.PasswordSetupCompletedAt,
        entity.Status,
        entity.RequestedAt,
        entity.ReviewedAt,
        entity.ReviewedBy,
        entity.RejectionReason
    };

    private static object ToInvitationAuditState(UserInvitationEntity entity) => new
    {
        entity.Id,
        entity.Email,
        entity.InvitationToken,
        entity.InvitedBy,
        entity.DepartmentId,
        entity.JobTitleId,
        Status = GetInvitationStatus(entity),
        entity.InvitedAt,
        entity.ExpiresAt,
        entity.AcceptedAt,
        entity.RejectedAt
    };

    private static RegistrationRequestResponse ToResponse(
        UserRegistrationRequestEntity entity,
        IReadOnlyDictionary<Guid, string> departments,
        IReadOnlyDictionary<Guid, string> jobTitles) =>
        new(
            entity.Id,
            entity.Email,
            entity.FirstName,
            entity.LastName,
            entity.DepartmentId,
            entity.DepartmentId.HasValue && departments.TryGetValue(entity.DepartmentId.Value, out var departmentName) ? departmentName : null,
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

    private static RegistrationPasswordSetupDetailResponse ToPasswordSetupResponse(
        UserRegistrationRequestEntity entity,
        IReadOnlyDictionary<Guid, string> departments,
        IReadOnlyDictionary<Guid, string> jobTitles) =>
        new(
            entity.Email,
            entity.FirstName,
            entity.LastName,
            entity.DepartmentId.HasValue && departments.TryGetValue(entity.DepartmentId.Value, out var departmentName) ? departmentName : null,
            entity.JobTitleId.HasValue && jobTitles.TryGetValue(entity.JobTitleId.Value, out var jobTitleName) ? jobTitleName : null,
            entity.PasswordSetupExpiresAt.HasValue && entity.PasswordSetupExpiresAt.Value <= DateTimeOffset.UtcNow,
            entity.PasswordSetupCompletedAt.HasValue,
            entity.PasswordSetupExpiresAt);

    private static InvitationResponse ToResponse(
        UserInvitationEntity entity,
        IReadOnlyDictionary<Guid, string> departments,
        IReadOnlyDictionary<Guid, string> jobTitles) =>
        new(
            entity.Id,
            entity.Email,
            entity.InvitationToken,
            entity.InvitedBy,
            entity.DepartmentId,
            entity.DepartmentId.HasValue && departments.TryGetValue(entity.DepartmentId.Value, out var departmentName) ? departmentName : null,
            entity.JobTitleId,
            entity.JobTitleId.HasValue && jobTitles.TryGetValue(entity.JobTitleId.Value, out var jobTitleName) ? jobTitleName : null,
            GetInvitationStatus(entity),
            entity.InvitedAt,
            entity.ExpiresAt,
            entity.AcceptedAt,
            entity.RejectedAt,
            $"/invite/{entity.InvitationToken}");

    private static MasterDataResponse ToResponse(DepartmentEntity entity) =>
        new(entity.Id, entity.Name, entity.DisplayOrder, entity.CreatedAt, entity.UpdatedAt, entity.DeletedReason, entity.DeletedBy, entity.DeletedAt);

    private static MasterDataResponse ToResponse(JobTitleEntity entity) =>
        new(entity.Id, entity.Name, entity.DisplayOrder, entity.CreatedAt, entity.UpdatedAt, entity.DeletedReason, entity.DeletedBy, entity.DeletedAt);

    private static UserResponse ToResponse(
        UserEntity entity,
        KeycloakUserProfile? keycloakProfile,
        IReadOnlyList<string> roles,
        IReadOnlyDictionary<Guid, string>? departments,
        IReadOnlyDictionary<Guid, string>? jobTitles) =>
        new(
            entity.Id,
            entity.Status,
            entity.CreatedAt,
            entity.CreatedBy,
            entity.DepartmentId,
            entity.DepartmentId.HasValue && departments is not null && departments.TryGetValue(entity.DepartmentId.Value, out var departmentName) ? departmentName : null,
            entity.JobTitleId,
            entity.JobTitleId.HasValue && jobTitles is not null && jobTitles.TryGetValue(entity.JobTitleId.Value, out var jobTitleName) ? jobTitleName : null,
            roles,
            entity.PreferredLanguage,
            entity.PreferredTheme,
            entity.DeletedReason,
            entity.DeletedBy,
            entity.DeletedAt,
            keycloakProfile is null
                ? null
                : new KeycloakUserSummary(
                    keycloakProfile.Id,
                    keycloakProfile.Email,
                    keycloakProfile.Username,
                    keycloakProfile.FirstName,
                    keycloakProfile.LastName,
                    keycloakProfile.Enabled,
                    keycloakProfile.EmailVerified));
}
