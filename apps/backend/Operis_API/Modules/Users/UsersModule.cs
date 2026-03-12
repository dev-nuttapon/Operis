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

        group.MapPost("/register", CreateRegistrationRequestAsync)
            .AllowAnonymous()
            .WithName("Users_CreateRegistrationRequest");

        group.MapGet("/registration-requests", ListRegistrationRequestsAsync)
            .WithName("Users_ListRegistrationRequests");

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

        if (!includeIdentity)
        {
            var localOnly = users.Select(x => ToResponse(x, null)).ToList();
            return Results.Ok(localOnly);
        }

        var responses = new List<UserResponse>(users.Count);
        var updated = false;

        foreach (var user in users)
        {
            var profile = await ResolveKeycloakProfileAsync(user, keycloakAdminClient, cancellationToken);
            if (profile is not null && string.IsNullOrWhiteSpace(user.KeycloakUserId))
            {
                user.KeycloakUserId = profile.Id;
                updated = true;
            }

            responses.Add(ToResponse(user, profile));
        }

        if (updated)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Results.Ok(responses);
    }

    private static async Task<IResult> CreateRegistrationRequestAsync(
        CreateRegistrationRequest request,
        OperisDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return Results.BadRequest("Email is required.");
        }

        var userExists = await dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);
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

        var userExists = await dbContext.Users.AnyAsync(x => x.Email == registrationRequest.Email, cancellationToken);
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
            Id = Guid.NewGuid(),
            KeycloakUserId = keycloakResult.UserId,
            Email = registrationRequest.Email,
            FirstName = registrationRequest.FirstName,
            LastName = registrationRequest.LastName,
            Status = UserStatus.Active,
            CreatedAt = now,
            CreatedBy = registrationRequest.ReviewedBy,
            ApprovedAt = now
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ToResponse(user, null));
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

    private static async Task<IResult> CreateInvitationAsync(
        CreateInvitationRequest request,
        OperisDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return Results.BadRequest("Email is required.");
        }

        var userExists = await dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);
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

        var userExists = await dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);
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
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            KeycloakUserId = keycloakResult.UserId,
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Status = UserStatus.Active,
            CreatedAt = now,
            CreatedBy = request.CreatedBy.Trim(),
            ApprovedAt = now
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/v1/users/{user.Id}", ToResponse(user, null));
    }

    private static async Task<KeycloakUserProfile?> ResolveKeycloakProfileAsync(
        UserEntity user,
        IKeycloakAdminClient keycloakAdminClient,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(user.KeycloakUserId))
        {
            var byId = await keycloakAdminClient.GetUserByIdAsync(user.KeycloakUserId, cancellationToken);
            if (byId is not null)
            {
                return byId;
            }
        }

        return await keycloakAdminClient.FindUserByEmailAsync(user.Email, cancellationToken);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

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

    private static UserResponse ToResponse(UserEntity entity, KeycloakUserProfile? keycloakProfile) =>
        new(
            entity.Id,
            entity.KeycloakUserId,
            entity.Email,
            entity.FirstName,
            entity.LastName,
            entity.Status,
            entity.CreatedAt,
            entity.CreatedBy,
            entity.ApprovedAt,
            keycloakProfile is null
                ? null
                : new KeycloakUserSummary(
                    keycloakProfile.Id,
                    keycloakProfile.Email,
                    keycloakProfile.Username,
                    keycloakProfile.Enabled,
                    keycloakProfile.EmailVerified));
}
