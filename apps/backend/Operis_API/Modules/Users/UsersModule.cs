using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Domain;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Modules;

namespace Operis_API.Modules.Users;

public sealed class UsersModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KeycloakOptions>(configuration.GetSection(KeycloakOptions.SectionName));
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

        group.MapGet("/departments", ListDepartmentsAsync)
            .WithName("Users_ListDepartments");

        group.MapPost("/departments", CreateDepartmentAsync)
            .WithName("Users_CreateDepartment");

        group.MapPut("/departments/{departmentId:guid}", UpdateDepartmentAsync)
            .WithName("Users_UpdateDepartment");

        group.MapDelete("/departments/{departmentId:guid}", DeleteDepartmentAsync)
            .WithName("Users_DeleteDepartment");

        group.MapGet("/job-titles", ListJobTitlesAsync)
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

        group.MapGet("/invitations", ListInvitationsAsync)
            .WithName("Users_ListInvitations");

        group.MapPost("/registration-requests/{requestId:guid}/approve", ApproveRegistrationRequestAsync)
            .WithName("Users_ApproveRegistrationRequest");

        group.MapPost("/registration-requests/{requestId:guid}/reject", RejectRegistrationRequestAsync)
            .WithName("Users_RejectRegistrationRequest");

        group.MapPost("/invitations", CreateInvitationAsync)
            .WithName("Users_CreateInvitation");

        group.MapPost("/", CreateUserAsync)
            .WithName("Users_CreateUser");

        return endpoints;
    }

    private static async Task<IResult> ListUsersAsync(
        OperisDbContext dbContext,
        IKeycloakAdminClient keycloakAdminClient,
        bool includeIdentity = true,
        CancellationToken cancellationToken = default)
    {
        var users = await dbContext.Users
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        var departments = await dbContext.Departments
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var jobTitles = await dbContext.JobTitles
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        if (!includeIdentity)
        {
            var localOnly = users.Select(x => ToResponse(x, null, departments, jobTitles)).ToList();
            return Results.Ok(localOnly);
        }

        var responses = new List<UserResponse>(users.Count);

        foreach (var user in users)
        {
            var profile = await ResolveKeycloakProfileAsync(user, keycloakAdminClient, cancellationToken);
            responses.Add(ToResponse(user, profile, departments, jobTitles));
        }

        return Results.Ok(responses);
    }

    private static async Task<IResult> ListDepartmentsAsync(
        OperisDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var items = await dbContext.Departments
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new MasterDataResponse(x.Id, x.Name, x.CreatedAt, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Results.Ok(items);
    }

    private static async Task<IResult> CreateDepartmentAsync(
        CreateMasterDataRequest request,
        OperisDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var name = NormalizeRequiredName(request.Name);
        if (name is null)
        {
            return Results.BadRequest("Department name is required.");
        }

        var exists = await dbContext.Departments.AnyAsync(x => x.Name == name, cancellationToken);
        if (exists)
        {
            return Results.Conflict("Department already exists.");
        }

        var entity = new DepartmentEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Departments.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/v1/users/departments/{entity.Id}", ToResponse(entity));
    }

    private static async Task<IResult> UpdateDepartmentAsync(
        Guid departmentId,
        UpdateMasterDataRequest request,
        OperisDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.Departments.FirstOrDefaultAsync(x => x.Id == departmentId, cancellationToken);
        if (entity is null)
        {
            return Results.NotFound();
        }

        var name = NormalizeRequiredName(request.Name);
        if (name is null)
        {
            return Results.BadRequest("Department name is required.");
        }

        var exists = await dbContext.Departments.AnyAsync(x => x.Id != departmentId && x.Name == name, cancellationToken);
        if (exists)
        {
            return Results.Conflict("Department already exists.");
        }

        entity.Name = name;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ToResponse(entity));
    }

    private static async Task<IResult> DeleteDepartmentAsync(
        Guid departmentId,
        OperisDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var inUse = await dbContext.Users.AnyAsync(x => x.DepartmentId == departmentId, cancellationToken);
        if (inUse)
        {
            return Results.Conflict("Department is in use by users.");
        }

        var entity = await dbContext.Departments.FirstOrDefaultAsync(x => x.Id == departmentId, cancellationToken);
        if (entity is null)
        {
            return Results.NotFound();
        }

        dbContext.Departments.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ListJobTitlesAsync(
        OperisDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var items = await dbContext.JobTitles
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new MasterDataResponse(x.Id, x.Name, x.CreatedAt, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Results.Ok(items);
    }

    private static async Task<IResult> CreateJobTitleAsync(
        CreateMasterDataRequest request,
        OperisDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var name = NormalizeRequiredName(request.Name);
        if (name is null)
        {
            return Results.BadRequest("Job title name is required.");
        }

        var exists = await dbContext.JobTitles.AnyAsync(x => x.Name == name, cancellationToken);
        if (exists)
        {
            return Results.Conflict("Job title already exists.");
        }

        var entity = new JobTitleEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.JobTitles.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/v1/users/job-titles/{entity.Id}", ToResponse(entity));
    }

    private static async Task<IResult> UpdateJobTitleAsync(
        Guid jobTitleId,
        UpdateMasterDataRequest request,
        OperisDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.JobTitles.FirstOrDefaultAsync(x => x.Id == jobTitleId, cancellationToken);
        if (entity is null)
        {
            return Results.NotFound();
        }

        var name = NormalizeRequiredName(request.Name);
        if (name is null)
        {
            return Results.BadRequest("Job title name is required.");
        }

        var exists = await dbContext.JobTitles.AnyAsync(x => x.Id != jobTitleId && x.Name == name, cancellationToken);
        if (exists)
        {
            return Results.Conflict("Job title already exists.");
        }

        entity.Name = name;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ToResponse(entity));
    }

    private static async Task<IResult> DeleteJobTitleAsync(
        Guid jobTitleId,
        OperisDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var inUse = await dbContext.Users.AnyAsync(x => x.JobTitleId == jobTitleId, cancellationToken);
        if (inUse)
        {
            return Results.Conflict("Job title is in use by users.");
        }

        var entity = await dbContext.JobTitles.FirstOrDefaultAsync(x => x.Id == jobTitleId, cancellationToken);
        if (entity is null)
        {
            return Results.NotFound();
        }

        dbContext.JobTitles.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> CreateRegistrationRequestAsync(
        CreateRegistrationRequest request,
        OperisDbContext dbContext,
        IKeycloakAdminClient keycloakAdminClient,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return Results.BadRequest("Email is required.");
        }

        var userExists = await LocalUserExistsForEmailAsync(email, dbContext, keycloakAdminClient, cancellationToken);
        if (userExists)
        {
            return Results.Conflict("User already exists.");
        }

        var pendingRequestExists = await dbContext.UserRegistrationRequests
            .AnyAsync(x => x.Email == email && x.Status == RegistrationRequestStatus.Pending, cancellationToken);
        if (pendingRequestExists)
        {
            return Results.Conflict("Pending registration request already exists.");
        }

        var registrationRequest = new UserRegistrationRequestEntity
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Status = RegistrationRequestStatus.Pending,
            RequestedAt = DateTimeOffset.UtcNow
        };

        dbContext.UserRegistrationRequests.Add(registrationRequest);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/v1/users/registration-requests/{registrationRequest.Id}",
            ToResponse(registrationRequest));
    }

    private static async Task<IResult> UpdateCurrentUserPreferencesAsync(
        UpdateUserPreferencesRequest request,
        ClaimsPrincipal principal,
        OperisDbContext dbContext,
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

        user.PreferredLanguage = NormalizeLanguage(request.PreferredLanguage);
        user.PreferredTheme = NormalizeTheme(request.PreferredTheme);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ListRegistrationRequestsAsync(
        OperisDbContext dbContext,
        RegistrationRequestStatus? status,
        CancellationToken cancellationToken)
    {
        var query = dbContext.UserRegistrationRequests.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        var requests = await query
            .OrderByDescending(x => x.RequestedAt)
            .Take(100)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);

        return Results.Ok(requests);
    }

    private static async Task<IResult> ApproveRegistrationRequestAsync(
        Guid requestId,
        ReviewRegistrationRequest request,
        OperisDbContext dbContext,
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
            cancellationToken);
        if (!keycloakResult.Success)
        {
            return Results.Problem(
                title: "Unable to provision user in Keycloak.",
                detail: keycloakResult.ErrorMessage,
                statusCode: StatusCodes.Status502BadGateway);
        }

        var now = DateTimeOffset.UtcNow;
        registrationRequest.Status = RegistrationRequestStatus.Approved;
        registrationRequest.ReviewedAt = now;
        registrationRequest.ReviewedBy = request.ReviewedBy.Trim();

        var user = new UserEntity
        {
            Id = keycloakResult.UserId ?? throw new InvalidOperationException("Keycloak user id is required."),
            Status = UserStatus.Active,
            CreatedAt = now,
            CreatedBy = registrationRequest.ReviewedBy
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ToResponse(user, null, null, null));
    }

    private static async Task<IResult> RejectRegistrationRequestAsync(
        Guid requestId,
        RejectRegistrationRequest request,
        OperisDbContext dbContext,
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

        registrationRequest.Status = RegistrationRequestStatus.Rejected;
        registrationRequest.ReviewedBy = request.ReviewedBy.Trim();
        registrationRequest.ReviewedAt = DateTimeOffset.UtcNow;
        registrationRequest.RejectionReason = request.Reason.Trim();

        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ToResponse(registrationRequest));
    }

    private static async Task<IResult> ListInvitationsAsync(
        OperisDbContext dbContext,
        InvitationStatus? status,
        CancellationToken cancellationToken)
    {
        var query = dbContext.UserInvitations.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        var invitations = await query
            .OrderByDescending(x => x.InvitedAt)
            .Take(100)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);

        return Results.Ok(invitations);
    }

    private static async Task<IResult> CreateInvitationAsync(
        CreateInvitationRequest request,
        OperisDbContext dbContext,
        IKeycloakAdminClient keycloakAdminClient,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return Results.BadRequest("Email is required.");
        }

        var userExists = await LocalUserExistsForEmailAsync(email, dbContext, keycloakAdminClient, cancellationToken);
        if (userExists)
        {
            return Results.Conflict("User already exists.");
        }

        var invitation = new UserInvitationEntity
        {
            Id = Guid.NewGuid(),
            Email = email,
            InvitedBy = request.InvitedBy.Trim(),
            Status = InvitationStatus.Pending,
            InvitedAt = DateTimeOffset.UtcNow,
            ExpiresAt = request.ExpiresInDays.HasValue
                ? DateTimeOffset.UtcNow.AddDays(Math.Max(1, request.ExpiresInDays.Value))
                : null
        };

        dbContext.UserInvitations.Add(invitation);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/v1/users/invitations/{invitation.Id}", ToResponse(invitation));
    }

    private static async Task<IResult> CreateUserAsync(
        CreateUserRequest request,
        OperisDbContext dbContext,
        IKeycloakAdminClient keycloakAdminClient,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return Results.BadRequest("Email is required.");
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
            cancellationToken);
        if (!keycloakResult.Success)
        {
            return Results.Problem(
                title: "Unable to provision user in Keycloak.",
                detail: keycloakResult.ErrorMessage,
                statusCode: StatusCodes.Status502BadGateway);
        }

        var now = DateTimeOffset.UtcNow;
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

        var user = new UserEntity
        {
            Id = keycloakResult.UserId ?? throw new InvalidOperationException("Keycloak user id is required."),
            Status = UserStatus.Active,
            CreatedAt = now,
            CreatedBy = request.CreatedBy.Trim(),
            DepartmentId = request.DepartmentId,
            JobTitleId = request.JobTitleId
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/v1/users/{user.Id}", ToResponse(user, null, null, null));
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

    private static async Task<bool> LocalUserExistsForEmailAsync(
        string email,
        OperisDbContext dbContext,
        IKeycloakAdminClient keycloakAdminClient,
        CancellationToken cancellationToken)
    {
        var keycloakUser = await keycloakAdminClient.FindUserByEmailAsync(email, cancellationToken);
        if (keycloakUser is null)
        {
            return false;
        }

        return await dbContext.Users.AnyAsync(x => x.Id == keycloakUser.Id, cancellationToken);
    }

    private static RegistrationRequestResponse ToResponse(UserRegistrationRequestEntity entity) =>
        new(
            entity.Id,
            entity.Email,
            entity.FirstName,
            entity.LastName,
            entity.Status,
            entity.RequestedAt,
            entity.ReviewedAt,
            entity.ReviewedBy,
            entity.RejectionReason);

    private static InvitationResponse ToResponse(UserInvitationEntity entity) =>
        new(
            entity.Id,
            entity.Email,
            entity.InvitedBy,
            entity.Status,
            entity.InvitedAt,
            entity.ExpiresAt,
            entity.AcceptedAt,
            entity.RejectedAt);

    private static MasterDataResponse ToResponse(DepartmentEntity entity) =>
        new(entity.Id, entity.Name, entity.CreatedAt, entity.UpdatedAt);

    private static MasterDataResponse ToResponse(JobTitleEntity entity) =>
        new(entity.Id, entity.Name, entity.CreatedAt, entity.UpdatedAt);

    private static UserResponse ToResponse(
        UserEntity entity,
        KeycloakUserProfile? keycloakProfile,
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
            entity.PreferredLanguage,
            entity.PreferredTheme,
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
