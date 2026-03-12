using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Application;
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
        services.AddScoped<IUserManagementCommands, UserManagementCommands>();
        services.AddScoped<IUserQueries, UserQueries>();
        services.AddScoped<IUserInvitationCommands, UserInvitationCommands>();
        services.AddScoped<IUserInvitationQueries, UserInvitationQueries>();
        services.AddScoped<IUserRegistrationCommands, UserRegistrationCommands>();
        services.AddScoped<IUserRegistrationQueries, UserRegistrationQueries>();
        services.AddScoped<IUserReferenceDataCommands, UserReferenceDataCommands>();
        services.AddScoped<IUserReferenceDataQueries, UserReferenceDataQueries>();
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
        IUserQueries queries,
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
        var result = await queries.ListUsersAsync(
            new UserListQuery(includeIdentity, status, from, to, search, sortBy, sortOrder, page, pageSize),
            cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> ListRolesAsync(
        OperisDbContext dbContext,
        IUserReferenceDataQueries queries,
        IAuditLogWriter auditLogWriter,
        CancellationToken cancellationToken)
    {
        var roles = await queries.ListRolesAsync(cancellationToken);
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
        IUserReferenceDataQueries queries,
        IAuditLogWriter auditLogWriter,
        string? search = null,
        string? sortBy = null,
        string? sortOrder = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await queries.ListDepartmentsAsync(
            new ReferenceDataQuery(search, sortBy, sortOrder, page, pageSize),
            cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "list",
            EntityType: "department",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { count = result.Items.Count, total = result.Total, page = result.Page, pageSize = result.PageSize, search, sortBy, sortOrder }));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateDepartmentAsync(
        CreateMasterDataRequest request,
        IUserReferenceDataCommands commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.CreateDepartmentAsync(request, cancellationToken);
        return result.Status switch
        {
            MasterDataCommandStatus.ValidationError => Results.BadRequest(result.ErrorMessage),
            MasterDataCommandStatus.Conflict => Results.Conflict(result.ErrorMessage),
            _ => Results.Created($"/api/v1/users/departments/{result.Response!.Id}", result.Response)
        };
    }

    private static async Task<IResult> UpdateDepartmentAsync(
        Guid departmentId,
        UpdateMasterDataRequest request,
        IUserReferenceDataCommands commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.UpdateDepartmentAsync(departmentId, request, cancellationToken);
        return result.Status switch
        {
            MasterDataCommandStatus.NotFound => Results.NotFound(),
            MasterDataCommandStatus.ValidationError => Results.BadRequest(result.ErrorMessage),
            MasterDataCommandStatus.Conflict => Results.Conflict(result.ErrorMessage),
            _ => Results.Ok(result.Response)
        };
    }

    private static async Task<IResult> DeleteDepartmentAsync(
        Guid departmentId,
        [FromBody] SoftDeleteRequest request,
        ClaimsPrincipal principal,
        [FromServices] IUserReferenceDataCommands commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.DeleteDepartmentAsync(departmentId, request, ResolveActor(principal), cancellationToken);
        return result.Status switch
        {
            MasterDataCommandStatus.NotFound => Results.NotFound(),
            _ => Results.NoContent()
        };
    }

    private static async Task<IResult> ListJobTitlesAsync(
        OperisDbContext dbContext,
        IUserReferenceDataQueries queries,
        IAuditLogWriter auditLogWriter,
        string? search = null,
        string? sortBy = null,
        string? sortOrder = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await queries.ListJobTitlesAsync(
            new ReferenceDataQuery(search, sortBy, sortOrder, page, pageSize),
            cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "list",
            EntityType: "job_title",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { count = result.Items.Count, total = result.Total, page = result.Page, pageSize = result.PageSize, search, sortBy, sortOrder }));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateJobTitleAsync(
        CreateMasterDataRequest request,
        IUserReferenceDataCommands commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.CreateJobTitleAsync(request, cancellationToken);
        return result.Status switch
        {
            MasterDataCommandStatus.ValidationError => Results.BadRequest(result.ErrorMessage),
            MasterDataCommandStatus.Conflict => Results.Conflict(result.ErrorMessage),
            _ => Results.Created($"/api/v1/users/job-titles/{result.Response!.Id}", result.Response)
        };
    }

    private static async Task<IResult> UpdateJobTitleAsync(
        Guid jobTitleId,
        UpdateMasterDataRequest request,
        IUserReferenceDataCommands commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.UpdateJobTitleAsync(jobTitleId, request, cancellationToken);
        return result.Status switch
        {
            MasterDataCommandStatus.NotFound => Results.NotFound(),
            MasterDataCommandStatus.ValidationError => Results.BadRequest(result.ErrorMessage),
            MasterDataCommandStatus.Conflict => Results.Conflict(result.ErrorMessage),
            _ => Results.Ok(result.Response)
        };
    }

    private static async Task<IResult> DeleteJobTitleAsync(
        Guid jobTitleId,
        [FromBody] SoftDeleteRequest request,
        ClaimsPrincipal principal,
        [FromServices] IUserReferenceDataCommands commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.DeleteJobTitleAsync(jobTitleId, request, ResolveActor(principal), cancellationToken);
        return result.Status switch
        {
            MasterDataCommandStatus.NotFound => Results.NotFound(),
            _ => Results.NoContent()
        };
    }

    private static async Task<IResult> DeleteUserAsync(
        string userId,
        [FromBody] SoftDeleteRequest request,
        ClaimsPrincipal principal,
        [FromServices] IUserManagementCommands commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.DeleteUserAsync(userId, request, ResolveActor(principal), cancellationToken);
        return result.Status switch
        {
            UserCommandStatus.NotFound => Results.NotFound(),
            UserCommandStatus.ExternalFailure => Results.Problem(
                title: result.ProblemTitle,
                detail: result.ErrorMessage,
                statusCode: result.ProblemStatusCode),
            _ => Results.NoContent()
        };
    }

    private static async Task<IResult> UpdateUserAsync(
        string userId,
        UpdateUserRequest request,
        IUserManagementCommands commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.UpdateUserAsync(userId, request, cancellationToken);
        return result.Status switch
        {
            UserCommandStatus.NotFound => Results.NotFound(),
            UserCommandStatus.ValidationError => Results.BadRequest(result.ErrorMessage),
            UserCommandStatus.Conflict => Results.Conflict(result.ErrorMessage),
            UserCommandStatus.ExternalFailure => Results.Problem(
                title: result.ProblemTitle,
                detail: result.ErrorMessage,
                statusCode: result.ProblemStatusCode),
            _ => Results.Ok(result.Response)
        };
    }

    private static async Task<IResult> CreateRegistrationRequestAsync(
        CreateRegistrationRequest request,
        IUserRegistrationCommands commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.CreateRegistrationRequestAsync(request, cancellationToken);
        return result.Status switch
        {
            RegistrationCommandStatus.ValidationError => Results.BadRequest(result.ErrorMessage),
            RegistrationCommandStatus.Conflict => Results.Conflict(result.ErrorMessage),
            _ => Results.Created($"/api/v1/users/registration-requests/{result.Response!.Id}", result.Response)
        };
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
        IUserRegistrationQueries queries,
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
        var result = await queries.ListRegistrationRequestsAsync(
            new RegistrationQuery(status, from, to, search, sortBy, sortOrder, page, pageSize),
            cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> ApproveRegistrationRequestAsync(
        Guid requestId,
        ReviewRegistrationRequest request,
        IUserRegistrationCommands commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.ApproveRegistrationRequestAsync(requestId, request, cancellationToken);
        return result.Status switch
        {
            RegistrationCommandStatus.NotFound => Results.NotFound(),
            RegistrationCommandStatus.ValidationError => Results.BadRequest(result.ErrorMessage),
            RegistrationCommandStatus.Conflict => Results.Conflict(result.ErrorMessage),
            RegistrationCommandStatus.ExternalFailure => Results.Problem(
                title: result.ProblemTitle,
                detail: result.ErrorMessage,
                statusCode: result.ProblemStatusCode),
            _ => Results.Ok(result.Response)
        };
    }

    private static async Task<IResult> RejectRegistrationRequestAsync(
        Guid requestId,
        RejectRegistrationRequest request,
        IUserRegistrationCommands commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.RejectRegistrationRequestAsync(requestId, request, cancellationToken);
        return result.Status switch
        {
            RegistrationCommandStatus.NotFound => Results.NotFound(),
            RegistrationCommandStatus.ValidationError => Results.BadRequest(result.ErrorMessage),
            _ => Results.Ok(result.Response)
        };
    }

    private static async Task<IResult> GetRegistrationPasswordSetupAsync(
        string token,
        IUserRegistrationQueries queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetRegistrationPasswordSetupAsync(token, cancellationToken);
        return result.Status switch
        {
            RegistrationPasswordSetupQueryStatus.NotFound => Results.NotFound(),
            _ => Results.Ok(result.Response)
        };
    }

    private static async Task<IResult> CompleteRegistrationPasswordSetupAsync(
        string token,
        CompleteRegistrationPasswordSetupRequest request,
        IUserRegistrationCommands commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.CompleteRegistrationPasswordSetupAsync(token, request, cancellationToken);
        return result.Status switch
        {
            RegistrationCommandStatus.NotFound => Results.NotFound(),
            RegistrationCommandStatus.ValidationError => Results.BadRequest(result.ErrorMessage),
            RegistrationCommandStatus.Conflict => Results.Conflict(result.ErrorMessage),
            RegistrationCommandStatus.InternalFailure => Results.Problem(
                title: result.ProblemTitle,
                detail: result.ErrorMessage,
                statusCode: result.ProblemStatusCode),
            RegistrationCommandStatus.ExternalFailure => Results.Problem(
                title: result.ProblemTitle,
                detail: result.ErrorMessage,
                statusCode: result.ProblemStatusCode),
            _ => Results.NoContent()
        };
    }

    private static async Task<IResult> ListInvitationsAsync(
        IUserInvitationQueries queries,
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
        var result = await queries.ListInvitationsAsync(
            new InvitationQuery(status, from, to, search, sortBy, sortOrder, page, pageSize),
            cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetInvitationByTokenAsync(
        string token,
        IUserInvitationQueries queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetInvitationByTokenAsync(token, cancellationToken);
        return result.Status switch
        {
            InvitationDetailQueryStatus.NotFound => Results.NotFound(),
            _ => Results.Ok(result.Response)
        };
    }

    private static async Task<IResult> CreateInvitationAsync(
        CreateInvitationRequest request,
        IUserInvitationCommands commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.CreateInvitationAsync(request, cancellationToken);
        return result.Status switch
        {
            InvitationCommandStatus.ValidationError => Results.BadRequest(result.ErrorMessage),
            InvitationCommandStatus.Conflict => Results.Conflict(result.ErrorMessage),
            _ => Results.Created($"/api/v1/users/invitations/{result.Response!.Id}", result.Response)
        };
    }

    private static async Task<IResult> UpdateInvitationAsync(
        Guid invitationId,
        UpdateInvitationRequest request,
        IUserInvitationCommands commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.UpdateInvitationAsync(invitationId, request, cancellationToken);
        return result.Status switch
        {
            InvitationCommandStatus.NotFound => Results.NotFound(),
            InvitationCommandStatus.ValidationError => Results.BadRequest(result.ErrorMessage),
            InvitationCommandStatus.Conflict => Results.Conflict(result.ErrorMessage),
            _ => Results.Ok(result.Response)
        };
    }

    private static async Task<IResult> CancelInvitationAsync(
        Guid invitationId,
        IUserInvitationCommands commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.CancelInvitationAsync(invitationId, cancellationToken);
        return result.Status switch
        {
            InvitationCommandStatus.NotFound => Results.NotFound(),
            InvitationCommandStatus.ValidationError => Results.BadRequest(result.ErrorMessage),
            _ => Results.Ok(result.Response)
        };
    }

    private static async Task<IResult> AcceptInvitationAsync(
        string token,
        AcceptInvitationRequest request,
        IUserInvitationCommands commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.AcceptInvitationAsync(token, request, cancellationToken);
        return result.Status switch
        {
            InvitationCommandStatus.NotFound => Results.NotFound(),
            InvitationCommandStatus.ValidationError => Results.BadRequest(result.ErrorMessage),
            InvitationCommandStatus.Conflict => Results.Conflict(result.ErrorMessage),
            InvitationCommandStatus.ExternalFailure => Results.Problem(
                title: result.ProblemTitle,
                detail: result.ErrorMessage,
                statusCode: result.ProblemStatusCode),
            _ => Results.Ok(result.Response)
        };
    }

    private static async Task<IResult> CreateUserAsync(
        CreateUserRequest request,
        IUserManagementCommands commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.CreateUserAsync(request, cancellationToken);
        return result.Status switch
        {
            UserCommandStatus.ValidationError => Results.BadRequest(result.ErrorMessage),
            UserCommandStatus.Conflict => Results.Conflict(result.ErrorMessage),
            UserCommandStatus.ExternalFailure => Results.Problem(
                title: result.ProblemTitle,
                detail: result.ErrorMessage,
                statusCode: result.ProblemStatusCode),
            _ => Results.Created($"/api/v1/users/{result.Response!.Id}", result.Response)
        };
    }

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

    private static (int page, int pageSize, int skip) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = Math.Clamp(pageSize, 10, 100);
        var skip = (normalizedPage - 1) * normalizedPageSize;
        return (normalizedPage, normalizedPageSize, skip);
    }

    private static bool IsDescending(string? sortOrder) =>
        string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

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

    private static MasterDataResponse ToResponse(DepartmentEntity entity) =>
        new(entity.Id, entity.Name, entity.DisplayOrder, entity.CreatedAt, entity.UpdatedAt, entity.DeletedReason, entity.DeletedBy, entity.DeletedAt);

    private static MasterDataResponse ToResponse(JobTitleEntity entity) =>
        new(entity.Id, entity.Name, entity.DisplayOrder, entity.CreatedAt, entity.UpdatedAt, entity.DeletedReason, entity.DeletedBy, entity.DeletedAt);
}
