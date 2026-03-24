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
using Operis_API.Shared.Security;

namespace Operis_API.Modules.Users;

public sealed class UsersModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KeycloakOptions>(configuration.GetSection(KeycloakOptions.SectionName));
        services.AddScoped<IUserManagementCommands, UserManagementCommands>();
        services.AddScoped<IUserPreferenceCommands, UserPreferenceCommands>();
        services.AddScoped<IUserQueries, UserQueries>();
        services.AddScoped<IUserInvitationCommands, UserInvitationCommands>();
        services.AddScoped<IUserInvitationQueries, UserInvitationQueries>();
        services.AddScoped<IUserRegistrationCommands, UserRegistrationCommands>();
        services.AddScoped<IUserRegistrationQueries, UserRegistrationQueries>();
        services.AddScoped<IUserOrgAssignmentCommands, UserOrgAssignmentCommands>();
        services.AddScoped<IUserReferenceDataCommands, UserReferenceDataCommands>();
        services.AddScoped<IUserReferenceDataQueries, UserReferenceDataQueries>();
        services.AddScoped<IUserSelfServiceCommands, UserSelfServiceCommands>();
        services.AddScoped<IProjectCommands, ProjectCommands>();
        services.AddScoped<IProjectQueries, ProjectQueries>();
        services.AddScoped<IProjectTemplateCommands, ProjectTemplateCommands>();
        services.AddScoped<IProjectTemplateQueries, ProjectTemplateQueries>();
        services.AddScoped<IProjectHistoryQueries, ProjectHistoryQueries>();
        services.AddScoped<ProjectHistoryWriter>();
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

        group.MapGet("/me", GetCurrentUserAsync)
            .WithName("Users_GetCurrent");

        group.MapGet("/{userId}", GetUserAsync)
            .WithName("Users_Get");

        group.MapDelete("/{userId}", DeleteUserAsync)
            .WithName("Users_Delete");

        group.MapPut("/{userId}", UpdateUserAsync)
            .WithName("Users_Update");

        group.MapPut("/{userId}/org-assignment", UpsertUserOrgAssignmentAsync)
            .WithName("Users_UpsertOrgAssignment");

        group.MapGet("/departments", ListDepartmentsAsync)
            .AllowAnonymous()
            .WithName("Users_ListDepartments");

        group.MapGet("/divisions", ListDivisionsAsync)
            .AllowAnonymous()
            .WithName("Users_ListDivisions");

        group.MapGet("/roles", ListRolesAsync)
            .WithName("Users_ListRoles");

        group.MapPost("/divisions", CreateDivisionAsync)
            .WithName("Users_CreateDivision");

        group.MapPut("/divisions/{divisionId:guid}", UpdateDivisionAsync)
            .WithName("Users_UpdateDivision");

        group.MapDelete("/divisions/{divisionId:guid}", DeleteDivisionAsync)
            .WithName("Users_DeleteDivision");

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

        group.MapGet("/project-roles", ListProjectRolesAsync)
            .AllowAnonymous()
            .WithName("Users_ListProjectRoles");

        group.MapGet("/project-roles/{projectRoleId:guid}", GetProjectRoleAsync)
            .WithName("Users_GetProjectRole");

        group.MapGet("/projects", ListProjectsAsync)
            .WithName("Users_ListProjects");

        group.MapGet("/projects/{projectId:guid}", GetProjectAsync)
            .WithName("Users_GetProject");
        
        group.MapGet("/projects/{projectId:guid}/history", ListProjectHistoryAsync)
            .WithName("Users_ListProjectHistory");

        group.MapGet("/project-type-templates", ListProjectTypeTemplatesAsync)
            .WithName("Users_ListProjectTypeTemplates");

        group.MapGet("/project-type-templates/{templateId:guid}", GetProjectTypeTemplateAsync)
            .WithName("Users_GetProjectTypeTemplate");

        group.MapPost("/project-type-templates", CreateProjectTypeTemplateAsync)
            .WithName("Users_CreateProjectTypeTemplate");

        group.MapPut("/project-type-templates/{templateId:guid}", UpdateProjectTypeTemplateAsync)
            .WithName("Users_UpdateProjectTypeTemplate");

        group.MapDelete("/project-type-templates/{templateId:guid}", DeleteProjectTypeTemplateAsync)
            .WithName("Users_DeleteProjectTypeTemplate");

        group.MapGet("/project-type-role-requirements", ListProjectTypeRoleRequirementsAsync)
            .WithName("Users_ListProjectTypeRoleRequirements");

        group.MapGet("/project-type-role-requirements/{requirementId:guid}", GetProjectTypeRoleRequirementAsync)
            .WithName("Users_GetProjectTypeRoleRequirement");

        group.MapPost("/project-type-role-requirements", CreateProjectTypeRoleRequirementAsync)
            .WithName("Users_CreateProjectTypeRoleRequirement");

        group.MapPut("/project-type-role-requirements/{requirementId:guid}", UpdateProjectTypeRoleRequirementAsync)
            .WithName("Users_UpdateProjectTypeRoleRequirement");

        group.MapDelete("/project-type-role-requirements/{requirementId:guid}", DeleteProjectTypeRoleRequirementAsync)
            .WithName("Users_DeleteProjectTypeRoleRequirement");

        group.MapPost("/projects", CreateProjectAsync)
            .WithName("Users_CreateProject");

        group.MapPut("/projects/{projectId:guid}", UpdateProjectAsync)
            .WithName("Users_UpdateProject");

        group.MapDelete("/projects/{projectId:guid}", DeleteProjectAsync)
            .WithName("Users_DeleteProject");

        group.MapPost("/project-roles", CreateProjectRoleAsync)
            .WithName("Users_CreateProjectRole");

        group.MapPut("/project-roles/{projectRoleId:guid}", UpdateProjectRoleAsync)
            .WithName("Users_UpdateProjectRole");

        group.MapDelete("/project-roles/{projectRoleId:guid}", DeleteProjectRoleAsync)
            .WithName("Users_DeleteProjectRole");

        group.MapGet("/project-assignments", ListProjectAssignmentsAsync)
            .WithName("Users_ListProjectAssignments");

        group.MapGet("/project-assignments/{assignmentId:guid}", GetProjectAssignmentAsync)
            .WithName("Users_GetProjectAssignment");

        group.MapGet("/projects/{projectId:guid}/org-chart", GetProjectOrgChartAsync)
            .WithName("Users_GetProjectOrgChart");

        group.MapGet("/projects/{projectId:guid}/evidence", GetProjectEvidenceAsync)
            .WithName("Users_GetProjectEvidence");

        group.MapGet("/projects/{projectId:guid}/evidence/team-register", ListProjectEvidenceTeamRegisterAsync)
            .WithName("Users_ListProjectEvidenceTeamRegister");

        group.MapGet("/projects/{projectId:guid}/evidence/role-responsibilities", ListProjectEvidenceRoleResponsibilitiesAsync)
            .WithName("Users_ListProjectEvidenceRoleResponsibilities");

        group.MapGet("/projects/{projectId:guid}/evidence/assignment-history", ListProjectEvidenceAssignmentHistoryAsync)
            .WithName("Users_ListProjectEvidenceAssignmentHistory");

        group.MapGet("/projects/{projectId:guid}/evidence/export", ExportProjectEvidenceAsync)
            .WithName("Users_ExportProjectEvidence");

        group.MapGet("/projects/{projectId:guid}/compliance", GetProjectComplianceAsync)
            .WithName("Users_GetProjectCompliance");

        group.MapPost("/project-assignments", CreateProjectAssignmentAsync)
            .WithName("Users_CreateProjectAssignment");

        group.MapPut("/project-assignments/{assignmentId:guid}", UpdateProjectAssignmentAsync)
            .WithName("Users_UpdateProjectAssignment");

        group.MapDelete("/project-assignments/{assignmentId:guid}", DeleteProjectAssignmentAsync)
            .WithName("Users_DeleteProjectAssignment");

        group.MapPut("/me/preferences", UpdateCurrentUserPreferencesAsync)
            .WithName("Users_UpdateCurrentUserPreferences");

        group.MapPost("/me/change-password", ChangeCurrentUserPasswordAsync)
            .WithName("Users_ChangePassword");

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
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IUserQueries queries,
        bool includeIdentity = true,
        UserStatus? status = null,
        Guid? divisionId = null,
        Guid? departmentId = null,
        Guid? jobTitleId = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        string? search = null,
        string? sortBy = null,
        string? sortOrder = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Users.Read))
        {
            return Results.Forbid();
        }

        var result = await queries.ListUsersAsync(
            new UserListQuery(includeIdentity, status, divisionId, departmentId, jobTitleId, from, to, search, sortBy, sortOrder, page, pageSize),
            cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetUserAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IUserQueries queries,
        string userId,
        bool includeIdentity = true,
        CancellationToken cancellationToken = default)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Users.Read))
        {
            return Results.Forbid();
        }

        var result = await queries.GetUserAsync(userId, includeIdentity, cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> GetCurrentUserAsync(
        ClaimsPrincipal principal,
        IUserQueries queries,
        bool includeIdentity = false,
        CancellationToken cancellationToken = default)
    {
        var userId = ResolveCurrentUserId(principal);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Results.Unauthorized();
        }

        var result = await queries.GetUserAsync(userId, includeIdentity, cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> ListRolesAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IUserReferenceDataQueries queries,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Users.Read))
        {
            return Results.Forbid();
        }

        var roles = await queries.ListRolesAsync(cancellationToken);
        return Results.Ok(roles);
    }

    private static async Task<IResult> ListDivisionsAsync(
        IUserReferenceDataQueries queries,
        string? search = null,
        string? sortBy = null,
        string? sortOrder = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await queries.ListDivisionsAsync(
            new ReferenceDataQuery(search, sortBy, sortOrder, null, null, page, pageSize),
            cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateDivisionAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        CreateMasterDataRequest request,
        IUserReferenceDataCommands commands,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.MasterData.ManagePermanentOrg))
        {
            return Results.Forbid();
        }

        var result = await commands.CreateDivisionAsync(request, cancellationToken);
        return result.Status switch
        {
            MasterDataCommandStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
            MasterDataCommandStatus.Conflict => ConflictWithCode(result.ErrorMessage, result.ErrorCode),
            _ => Results.Created($"/api/v1/users/divisions/{result.Response!.Id}", result.Response)
        };
    }

    private static async Task<IResult> UpdateDivisionAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid divisionId,
        UpdateMasterDataRequest request,
        IUserReferenceDataCommands commands,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.MasterData.ManagePermanentOrg))
        {
            return Results.Forbid();
        }

        var result = await commands.UpdateDivisionAsync(divisionId, request, cancellationToken);
        return result.Status switch
        {
            MasterDataCommandStatus.NotFound => NotFoundWithCode(),
            MasterDataCommandStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
            MasterDataCommandStatus.Conflict => ConflictWithCode(result.ErrorMessage, result.ErrorCode),
            _ => Results.Ok(result.Response)
        };
    }

    private static async Task<IResult> DeleteDivisionAsync(
        Guid divisionId,
        [FromBody] SoftDeleteRequest request,
        ClaimsPrincipal principal,
        [FromServices] IPermissionMatrix permissionMatrix,
        [FromServices] IUserReferenceDataCommands commands,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.MasterData.ManagePermanentOrg))
        {
            return Results.Forbid();
        }

        var result = await commands.DeleteDivisionAsync(divisionId, request, ResolveActor(principal), cancellationToken);
        return result.Status switch
        {
            MasterDataCommandStatus.NotFound => NotFoundWithCode(),
            _ => Results.NoContent()
        };
    }

    private static async Task<IResult> ListDepartmentsAsync(
        IUserReferenceDataQueries queries,
        string? search = null,
        string? sortBy = null,
        string? sortOrder = null,
        Guid? divisionId = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await queries.ListDepartmentsAsync(
            new ReferenceDataQuery(search, sortBy, sortOrder, divisionId, null, page, pageSize),
            cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateDepartmentAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        CreateDepartmentRequest request,
        IUserReferenceDataCommands commands,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.MasterData.ManagePermanentOrg))
        {
            return Results.Forbid();
        }

        var result = await commands.CreateDepartmentAsync(request, cancellationToken);
        return result.Status switch
        {
            MasterDataCommandStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
            MasterDataCommandStatus.Conflict => ConflictWithCode(result.ErrorMessage, result.ErrorCode),
            _ => Results.Created($"/api/v1/users/departments/{result.Response!.Id}", result.Response)
        };
    }

    private static async Task<IResult> UpdateDepartmentAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid departmentId,
        UpdateDepartmentRequest request,
        IUserReferenceDataCommands commands,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.MasterData.ManagePermanentOrg))
        {
            return Results.Forbid();
        }

        var result = await commands.UpdateDepartmentAsync(departmentId, request, cancellationToken);
        return result.Status switch
        {
            MasterDataCommandStatus.NotFound => NotFoundWithCode(),
            MasterDataCommandStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
            MasterDataCommandStatus.Conflict => ConflictWithCode(result.ErrorMessage, result.ErrorCode),
            _ => Results.Ok(result.Response)
        };
    }

    private static async Task<IResult> DeleteDepartmentAsync(
        Guid departmentId,
        [FromBody] SoftDeleteRequest request,
        ClaimsPrincipal principal,
        [FromServices] IPermissionMatrix permissionMatrix,
        [FromServices] IUserReferenceDataCommands commands,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.MasterData.ManagePermanentOrg))
        {
            return Results.Forbid();
        }

        var result = await commands.DeleteDepartmentAsync(departmentId, request, ResolveActor(principal), cancellationToken);
        return result.Status switch
        {
            MasterDataCommandStatus.NotFound => NotFoundWithCode(),
            _ => Results.NoContent()
        };
    }

    private static async Task<IResult> ListJobTitlesAsync(
        IUserReferenceDataQueries queries,
        string? search = null,
        string? sortBy = null,
        string? sortOrder = null,
        Guid? departmentId = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await queries.ListJobTitlesAsync(
            new ReferenceDataQuery(search, sortBy, sortOrder, null, departmentId, page, pageSize),
            cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateJobTitleAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        CreateJobTitleRequest request,
        IUserReferenceDataCommands commands,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.MasterData.ManagePermanentOrg))
        {
            return Results.Forbid();
        }

        var result = await commands.CreateJobTitleAsync(request, cancellationToken);
        return result.Status switch
        {
            MasterDataCommandStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
            MasterDataCommandStatus.Conflict => ConflictWithCode(result.ErrorMessage, result.ErrorCode),
            _ => Results.Created($"/api/v1/users/job-titles/{result.Response!.Id}", result.Response)
        };
    }

    private static async Task<IResult> UpdateJobTitleAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid jobTitleId,
        UpdateJobTitleRequest request,
        IUserReferenceDataCommands commands,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.MasterData.ManagePermanentOrg))
        {
            return Results.Forbid();
        }

        var result = await commands.UpdateJobTitleAsync(jobTitleId, request, cancellationToken);
        return result.Status switch
        {
            MasterDataCommandStatus.NotFound => NotFoundWithCode(),
            MasterDataCommandStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
            MasterDataCommandStatus.Conflict => ConflictWithCode(result.ErrorMessage, result.ErrorCode),
            _ => Results.Ok(result.Response)
        };
    }

    private static async Task<IResult> DeleteJobTitleAsync(
        Guid jobTitleId,
        [FromBody] SoftDeleteRequest request,
        ClaimsPrincipal principal,
        [FromServices] IPermissionMatrix permissionMatrix,
        [FromServices] IUserReferenceDataCommands commands,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.MasterData.ManagePermanentOrg))
        {
            return Results.Forbid();
        }

        var result = await commands.DeleteJobTitleAsync(jobTitleId, request, ResolveActor(principal), cancellationToken);
        return result.Status switch
        {
            MasterDataCommandStatus.NotFound => NotFoundWithCode(),
            _ => Results.NoContent()
        };
    }

    private static async Task<IResult> ListProjectRolesAsync(
        IProjectQueries queries,
        string? search = null,
        string? sortBy = null,
        string? sortOrder = null,
        Guid? projectId = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await queries.ListProjectRolesAsync(
            new ReferenceDataQuery(search, sortBy, sortOrder, projectId, null, page, pageSize),
            cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetProjectRoleAsync(
        IProjectQueries queries,
        Guid projectRoleId,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetProjectRoleAsync(projectRoleId, cancellationToken);
        return result is null ? NotFoundWithCode() : Results.Ok(result);
    }

    private static async Task<IResult> ListProjectsAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IProjectQueries queries,
        bool assignedOnly = false,
        string? search = null,
        string? sortBy = null,
        string? sortOrder = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var canReadAllProjects = permissionMatrix.HasPermission(principal, Permissions.Projects.Read);
        if (!canReadAllProjects && !assignedOnly)
        {
            return Results.Forbid();
        }

        var assignedUserId = assignedOnly || !canReadAllProjects ? ResolveCurrentUserId(principal) : null;
        if ((assignedOnly || !canReadAllProjects) && string.IsNullOrWhiteSpace(assignedUserId))
        {
            return Results.Forbid();
        }

        var result = await queries.ListProjectsAsync(new ProjectListQuery(search, sortBy, sortOrder, assignedUserId, page, pageSize), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetProjectAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid projectId,
        IProjectQueries queries,
        CancellationToken cancellationToken)
    {
        if (!await HasProjectReadAccessAsync(principal, permissionMatrix, queries, projectId, cancellationToken))
        {
            return Results.Forbid();
        }

        var result = await queries.GetProjectAsync(projectId, cancellationToken);
        return result is null ? NotFoundWithCode() : Results.Ok(result);
    }

    private static async Task<IResult> ListProjectHistoryAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid projectId,
        IProjectQueries projectQueries,
        IProjectHistoryQueries queries,
        CancellationToken cancellationToken,
        string? search = null,
        int page = 1,
        int pageSize = 10)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.ActivityLogs.Read))
        {
            return Results.Forbid();
        }

        if (!await HasProjectReadAccessAsync(principal, permissionMatrix, projectQueries, projectId, cancellationToken))
        {
            return Results.Forbid();
        }

        var items = await queries.ListAsync(new ProjectHistoryListQuery(projectId, search, page, pageSize), cancellationToken);
        return Results.Ok(items);
    }

    private static async Task<IResult> ListProjectTypeTemplatesAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IProjectTemplateQueries queries,
        string? search = null,
        string? sortBy = null,
        string? sortOrder = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Projects.ManageTemplates))
        {
            return Results.Forbid();
        }

        var result = await queries.ListProjectTypeTemplatesAsync(
            new ProjectListQuery(search, sortBy, sortOrder, Page: page, PageSize: pageSize),
            cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetProjectTypeTemplateAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IProjectTemplateQueries queries,
        Guid templateId,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Projects.ManageTemplates))
        {
            return Results.Forbid();
        }

        var result = await queries.GetProjectTypeTemplateAsync(templateId, cancellationToken);
        return result is null ? NotFoundWithCode() : Results.Ok(result);
    }

    private static async Task<IResult> CreateProjectTypeTemplateAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        CreateProjectTypeTemplateRequest request,
        IProjectTemplateCommands commands,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Projects.ManageTemplates))
        {
            return Results.Forbid();
        }

        var result = await commands.CreateProjectTypeTemplateAsync(request, cancellationToken);
        return !result.Success
            ? BadRequestWithCode(result.Error, result.ErrorCode)
            : Results.Created($"/api/v1/users/project-type-templates/{result.Response!.Id}", result.Response);
    }

    private static async Task<IResult> UpdateProjectTypeTemplateAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid templateId,
        UpdateProjectTypeTemplateRequest request,
        IProjectTemplateCommands commands,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Projects.ManageTemplates))
        {
            return Results.Forbid();
        }

        var result = await commands.UpdateProjectTypeTemplateAsync(templateId, request, cancellationToken);
        if (result.NotFound)
        {
            return NotFoundWithCode();
        }

        return !result.Success ? BadRequestWithCode(result.Error, result.ErrorCode) : Results.Ok(result.Response);
    }

    private static async Task<IResult> DeleteProjectTypeTemplateAsync(
        Guid templateId,
        [FromBody] SoftDeleteRequest request,
        ClaimsPrincipal principal,
        [FromServices] IPermissionMatrix permissionMatrix,
        [FromServices] IProjectTemplateCommands commands,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Projects.ManageTemplates))
        {
            return Results.Forbid();
        }

        var result = await commands.DeleteProjectTypeTemplateAsync(templateId, request, ResolveActor(principal), cancellationToken);
        return result.NotFound ? NotFoundWithCode() : Results.NoContent();
    }

    private static async Task<IResult> ListProjectTypeRoleRequirementsAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IProjectTemplateQueries queries,
        Guid templateId,
        string? search = null,
        string? sortBy = null,
        string? sortOrder = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Projects.ManageTemplates))
        {
            return Results.Forbid();
        }

        var result = await queries.ListProjectTypeRoleRequirementsAsync(templateId, search, sortBy, sortOrder, page, pageSize, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetProjectTypeRoleRequirementAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IProjectTemplateQueries queries,
        Guid requirementId,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Projects.ManageTemplates))
        {
            return Results.Forbid();
        }

        var result = await queries.GetProjectTypeRoleRequirementAsync(requirementId, cancellationToken);
        return result is null ? NotFoundWithCode() : Results.Ok(result);
    }

    private static async Task<IResult> CreateProjectTypeRoleRequirementAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        CreateProjectTypeRoleRequirementRequest request,
        IProjectTemplateCommands commands,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Projects.ManageTemplates))
        {
            return Results.Forbid();
        }

        var result = await commands.CreateProjectTypeRoleRequirementAsync(request, cancellationToken);
        return !result.Success
            ? BadRequestWithCode(result.Error, result.ErrorCode)
            : Results.Created($"/api/v1/users/project-type-role-requirements/{result.Response!.Id}", result.Response);
    }

    private static async Task<IResult> UpdateProjectTypeRoleRequirementAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid requirementId,
        UpdateProjectTypeRoleRequirementRequest request,
        IProjectTemplateCommands commands,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Projects.ManageTemplates))
        {
            return Results.Forbid();
        }

        var result = await commands.UpdateProjectTypeRoleRequirementAsync(requirementId, request, cancellationToken);
        if (result.NotFound)
        {
            return NotFoundWithCode();
        }

        return !result.Success ? BadRequestWithCode(result.Error, result.ErrorCode) : Results.Ok(result.Response);
    }

    private static async Task<IResult> DeleteProjectTypeRoleRequirementAsync(
        Guid requirementId,
        [FromBody] SoftDeleteRequest request,
        ClaimsPrincipal principal,
        [FromServices] IPermissionMatrix permissionMatrix,
        [FromServices] IProjectTemplateCommands commands,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Projects.ManageTemplates))
        {
            return Results.Forbid();
        }

        var result = await commands.DeleteProjectTypeRoleRequirementAsync(requirementId, request, ResolveActor(principal), cancellationToken);
        return result.NotFound ? NotFoundWithCode() : Results.NoContent();
    }

    private static async Task<IResult> CreateProjectAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        CreateProjectRequest request,
        IProjectCommands commands,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Projects.Manage))
        {
            return Results.Forbid();
        }

        var result = await commands.CreateProjectAsync(request, cancellationToken);
        if (!result.Success)
        {
            return BadRequestWithCode(result.Error, result.ErrorCode);
        }

        return Results.Created($"/api/v1/users/projects/{result.Response!.Id}", result.Response);
    }

    private static async Task<IResult> UpdateProjectAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid projectId,
        UpdateProjectRequest request,
        IProjectCommands commands,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Projects.Manage))
        {
            return Results.Forbid();
        }

        var result = await commands.UpdateProjectAsync(projectId, request, cancellationToken);
        if (result.NotFound)
        {
            return NotFoundWithCode();
        }

        if (!result.Success)
        {
            return BadRequestWithCode(result.Error, result.ErrorCode);
        }

        return Results.Ok(result.Response);
    }

    private static async Task<IResult> DeleteProjectAsync(
        Guid projectId,
        [FromBody] SoftDeleteRequest request,
        ClaimsPrincipal principal,
        [FromServices] IPermissionMatrix permissionMatrix,
        [FromServices] IProjectCommands commands,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Projects.Manage))
        {
            return Results.Forbid();
        }

        var result = await commands.DeleteProjectAsync(projectId, request, ResolveActor(principal), cancellationToken);
        return result.NotFound ? NotFoundWithCode() : Results.NoContent();
    }

    private static async Task<IResult> CreateProjectRoleAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        CreateProjectRoleRequest request,
        IProjectCommands commands,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Projects.ManageRoles))
        {
            return Results.Forbid();
        }

        var result = await commands.CreateProjectRoleAsync(request, cancellationToken);
        return !result.Success
            ? BadRequestWithCode(result.Error, result.ErrorCode)
            : Results.Created($"/api/v1/users/project-roles/{result.Response!.Id}", result.Response);
    }

    private static async Task<IResult> UpdateProjectRoleAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid projectRoleId,
        UpdateProjectRoleRequest request,
        IProjectCommands commands,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Projects.ManageRoles))
        {
            return Results.Forbid();
        }

        var result = await commands.UpdateProjectRoleAsync(projectRoleId, request, cancellationToken);
        if (result.NotFound)
        {
            return NotFoundWithCode();
        }

        return !result.Success ? BadRequestWithCode(result.Error, result.ErrorCode) : Results.Ok(result.Response);
    }

    private static async Task<IResult> DeleteProjectRoleAsync(
        Guid projectRoleId,
        [FromBody] SoftDeleteRequest request,
        ClaimsPrincipal principal,
        [FromServices] IPermissionMatrix permissionMatrix,
        [FromServices] IProjectCommands commands,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Projects.ManageRoles))
        {
            return Results.Forbid();
        }

        var result = await commands.DeleteProjectRoleAsync(projectRoleId, request, ResolveActor(principal), cancellationToken);
        return result.NotFound ? NotFoundWithCode() : Results.NoContent();
    }

    private static async Task<IResult> ListProjectAssignmentsAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IProjectQueries queries,
        Guid projectId,
        string? search = null,
        string? sortBy = null,
        string? sortOrder = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (!await HasProjectReadAccessAsync(principal, permissionMatrix, queries, projectId, cancellationToken))
        {
            return Results.Forbid();
        }

        var result = await queries.ListProjectAssignmentsAsync(new ProjectAssignmentListQuery(projectId, search, sortBy, sortOrder, page, pageSize), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetProjectAssignmentAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IProjectQueries queries,
        Guid assignmentId,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetProjectAssignmentAsync(assignmentId, cancellationToken);
        if (result is null)
        {
            return NotFoundWithCode();
        }

        if (!await HasProjectReadAccessAsync(principal, permissionMatrix, queries, result.ProjectId, cancellationToken))
        {
            return Results.Forbid();
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> CreateProjectAssignmentAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        CreateProjectAssignmentRequest request,
        IProjectCommands commands,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasAnyPermission(principal, Permissions.Projects.ManageMembers, Permissions.Projects.Manage))
        {
            return Results.Forbid();
        }

        var result = await commands.CreateProjectAssignmentAsync(request, cancellationToken);
        return !result.Success
            ? BadRequestWithCode(result.Error, result.ErrorCode)
            : Results.Created($"/api/v1/users/project-assignments/{result.Response!.Id}", result.Response);
    }

    private static async Task<IResult> GetProjectOrgChartAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid projectId,
        IProjectQueries queries,
        CancellationToken cancellationToken)
    {
        if (!await HasProjectReadAccessAsync(principal, permissionMatrix, queries, projectId, cancellationToken))
        {
            return Results.Forbid();
        }

        var result = await queries.GetProjectOrgChartAsync(projectId, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetProjectEvidenceAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid projectId,
        IProjectQueries queries,
        CancellationToken cancellationToken)
    {
        if (!await HasProjectReadAccessAsync(principal, permissionMatrix, queries, projectId, cancellationToken))
        {
            return Results.Forbid();
        }

        var result = await queries.GetProjectEvidenceAsync(projectId, cancellationToken);
        return result is null ? NotFoundWithCode() : Results.Ok(result);
    }

    private static async Task<IResult> ListProjectEvidenceTeamRegisterAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid projectId,
        IProjectQueries queries,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (!await HasProjectReadAccessAsync(principal, permissionMatrix, queries, projectId, cancellationToken))
        {
            return Results.Forbid();
        }

        var result = await queries.ListProjectTeamRegisterAsync(new ProjectEvidenceListQuery(projectId, page, pageSize), cancellationToken);
        return result is null ? NotFoundWithCode() : Results.Ok(result);
    }

    private static async Task<IResult> ListProjectEvidenceRoleResponsibilitiesAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid projectId,
        IProjectQueries queries,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (!await HasProjectReadAccessAsync(principal, permissionMatrix, queries, projectId, cancellationToken))
        {
            return Results.Forbid();
        }

        var result = await queries.ListProjectRoleResponsibilitiesAsync(new ProjectEvidenceListQuery(projectId, page, pageSize), cancellationToken);
        return result is null ? NotFoundWithCode() : Results.Ok(result);
    }

    private static async Task<IResult> ListProjectEvidenceAssignmentHistoryAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid projectId,
        IProjectQueries queries,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (!await HasProjectReadAccessAsync(principal, permissionMatrix, queries, projectId, cancellationToken))
        {
            return Results.Forbid();
        }

        var result = await queries.ListProjectAssignmentHistoryAsync(new ProjectEvidenceListQuery(projectId, page, pageSize), cancellationToken);
        return result is null ? NotFoundWithCode() : Results.Ok(result);
    }

    private static async Task<IResult> GetProjectComplianceAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid projectId,
        IProjectQueries queries,
        CancellationToken cancellationToken)
    {
        if (!await HasProjectReadAccessAsync(principal, permissionMatrix, queries, projectId, cancellationToken))
        {
            return Results.Forbid();
        }

        var result = await queries.GetProjectComplianceAsync(projectId, cancellationToken);
        return result is null ? NotFoundWithCode() : Results.Ok(result);
    }

    private static async Task<IResult> UpdateProjectAssignmentAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid assignmentId,
        UpdateProjectAssignmentRequest request,
        IProjectCommands commands,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasAnyPermission(principal, Permissions.Projects.ManageMembers, Permissions.Projects.Manage))
        {
            return Results.Forbid();
        }

        var result = await commands.UpdateProjectAssignmentAsync(assignmentId, request, cancellationToken);
        if (result.NotFound)
        {
            return NotFoundWithCode();
        }

        return !result.Success ? BadRequestWithCode(result.Error, result.ErrorCode) : Results.Ok(result.Response);
    }

    private static async Task<IResult> DeleteProjectAssignmentAsync(
        Guid assignmentId,
        [FromBody] SoftDeleteRequest request,
        ClaimsPrincipal principal,
        [FromServices] IPermissionMatrix permissionMatrix,
        IProjectCommands commands,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasAnyPermission(principal, Permissions.Projects.ManageMembers, Permissions.Projects.Manage))
        {
            return Results.Forbid();
        }

        var result = await commands.DeleteProjectAssignmentAsync(assignmentId, request, cancellationToken);
        return result.NotFound ? NotFoundWithCode() : Results.NoContent();
    }

    private static async Task<IResult> DeleteUserAsync(
        string userId,
        [FromBody] SoftDeleteRequest request,
        ClaimsPrincipal principal,
        [FromServices] IPermissionMatrix permissionMatrix,
        [FromServices] IUserManagementCommands commands,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Users.Delete))
        {
            return Results.Forbid();
        }

        var result = await commands.DeleteUserAsync(userId, request, ResolveActor(principal), cancellationToken);
        return result.Status switch
        {
            UserCommandStatus.NotFound => NotFoundWithCode(),
            UserCommandStatus.ExternalFailure => ProblemWithCode(result.ProblemTitle, result.ErrorMessage, result.ErrorCode, result.ProblemStatusCode, ApiErrorCodes.ExternalDependencyFailure),
            _ => Results.NoContent()
        };
    }

    private static async Task<IResult> UpdateUserAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        string userId,
        UpdateUserRequest request,
        IUserManagementCommands commands,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Users.Update))
        {
            return Results.Forbid();
        }

        var result = await commands.UpdateUserAsync(userId, request, cancellationToken);
        return result.Status switch
        {
            UserCommandStatus.NotFound => NotFoundWithCode(),
            UserCommandStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
            UserCommandStatus.Conflict => ConflictWithCode(result.ErrorMessage, result.ErrorCode),
            UserCommandStatus.ExternalFailure => ProblemWithCode(result.ProblemTitle, result.ErrorMessage, result.ErrorCode, result.ProblemStatusCode, ApiErrorCodes.ExternalDependencyFailure),
            _ => Results.Ok(result.Response)
        };
    }

    private static async Task<IResult> UpsertUserOrgAssignmentAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        string userId,
        UpsertUserOrgAssignmentRequest request,
        IUserOrgAssignmentCommands commands,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Users.Update))
        {
            return Results.Forbid();
        }

        var result = await commands.UpsertPrimaryAssignmentAsync(userId, request, cancellationToken);
        return result.Status switch
        {
            UserCommandStatus.NotFound => NotFoundWithCode(),
            UserCommandStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
            _ => Results.NoContent()
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
            RegistrationCommandStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
            RegistrationCommandStatus.Conflict => ConflictWithCode(result.ErrorMessage, result.ErrorCode),
            _ => Results.Created($"/api/v1/users/registration-requests/{result.Response!.Id}", result.Response)
        };
    }

    private static async Task<IResult> UpdateCurrentUserPreferencesAsync(
        UpdateUserPreferencesRequest request,
        ClaimsPrincipal principal,
        IUserPreferenceCommands commands,
        CancellationToken cancellationToken)
    {
        var currentUserId = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Results.Json(ApiProblemDetailsFactory.Create(StatusCodes.Status401Unauthorized, "unauthorized", "Unauthorized.", "The current user identity is missing."), statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await commands.UpdateCurrentUserPreferencesAsync(currentUserId, request, cancellationToken);
        return result.Status switch
        {
            UserPreferenceCommandStatus.NotFound => Results.NotFound(),
            _ => Results.NoContent()
        };
    }

    private static async Task<IResult> ChangeCurrentUserPasswordAsync(
        ChangePasswordRequest request,
        ClaimsPrincipal principal,
        IUserSelfServiceCommands commands,
        CancellationToken cancellationToken)
    {
        var currentUserId = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Results.Json(ApiProblemDetailsFactory.Create(StatusCodes.Status401Unauthorized, "unauthorized", "Unauthorized.", "The current user identity is missing."), statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await commands.ChangePasswordAsync(currentUserId, request, cancellationToken);
        return result.Status switch
        {
            UserPasswordChangeStatus.NotFound => NotFoundWithCode(),
            UserPasswordChangeStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
            UserPasswordChangeStatus.ExternalFailure => ProblemWithCode(result.ProblemTitle, result.ErrorMessage, result.ErrorCode, result.ProblemStatusCode, ApiErrorCodes.ExternalDependencyFailure),
            _ => Results.NoContent()
        };
    }

    private static async Task<IResult> ListRegistrationRequestsAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
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
        if (LacksPermission(principal, permissionMatrix, Permissions.Users.ReviewRegistrations))
        {
            return Results.Forbid();
        }

        var result = await queries.ListRegistrationRequestsAsync(
            new RegistrationQuery(status, from, to, search, sortBy, sortOrder, page, pageSize),
            cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> ExportProjectEvidenceAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid projectId,
        IProjectQueries queries,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Projects.ExportEvidence))
        {
            return Results.Forbid();
        }

        var result = await queries.GetProjectEvidenceExportAsync(projectId, cancellationToken);
        if (result is null)
        {
            return NotFoundWithCode();
        }

        return Results.File(result.Content, result.ContentType, result.FileName);
    }

    private static async Task<IResult> ApproveRegistrationRequestAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid requestId,
        ReviewRegistrationRequest request,
        IUserRegistrationCommands commands,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Users.ReviewRegistrations))
        {
            return Results.Forbid();
        }

        var result = await commands.ApproveRegistrationRequestAsync(requestId, request, cancellationToken);
        return result.Status switch
        {
            RegistrationCommandStatus.NotFound => NotFoundWithCode(),
            RegistrationCommandStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
            RegistrationCommandStatus.Conflict => ConflictWithCode(result.ErrorMessage, result.ErrorCode),
            RegistrationCommandStatus.ExternalFailure => ProblemWithCode(result.ProblemTitle, result.ErrorMessage, result.ErrorCode, result.ProblemStatusCode, ApiErrorCodes.ExternalDependencyFailure),
            _ => Results.Ok(result.Response)
        };
    }

    private static async Task<IResult> RejectRegistrationRequestAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid requestId,
        RejectRegistrationRequest request,
        IUserRegistrationCommands commands,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Users.ReviewRegistrations))
        {
            return Results.Forbid();
        }

        var result = await commands.RejectRegistrationRequestAsync(requestId, request, cancellationToken);
        return result.Status switch
        {
            RegistrationCommandStatus.NotFound => NotFoundWithCode(),
            RegistrationCommandStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
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
            RegistrationPasswordSetupQueryStatus.NotFound => NotFoundWithCode(),
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
            RegistrationCommandStatus.NotFound => NotFoundWithCode(),
            RegistrationCommandStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
            RegistrationCommandStatus.Conflict => ConflictWithCode(result.ErrorMessage, result.ErrorCode),
            RegistrationCommandStatus.InternalFailure => ProblemWithCode(result.ProblemTitle, result.ErrorMessage, result.ErrorCode, result.ProblemStatusCode, ApiErrorCodes.InternalFailure),
            RegistrationCommandStatus.ExternalFailure => ProblemWithCode(result.ProblemTitle, result.ErrorMessage, result.ErrorCode, result.ProblemStatusCode, ApiErrorCodes.ExternalDependencyFailure),
            _ => Results.NoContent()
        };
    }

    private static async Task<IResult> ListInvitationsAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
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
        if (LacksPermission(principal, permissionMatrix, Permissions.Users.Read))
        {
            return Results.Forbid();
        }

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
            InvitationDetailQueryStatus.NotFound => NotFoundWithCode(),
            _ => Results.Ok(result.Response)
        };
    }

    private static async Task<IResult> CreateInvitationAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        CreateInvitationRequest request,
        IUserInvitationCommands commands,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Users.Invite))
        {
            return Results.Forbid();
        }

        var result = await commands.CreateInvitationAsync(request, cancellationToken);
        return result.Status switch
        {
            InvitationCommandStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
            InvitationCommandStatus.Conflict => ConflictWithCode(result.ErrorMessage, result.ErrorCode),
            _ => Results.Created($"/api/v1/users/invitations/{result.Response!.Id}", result.Response)
        };
    }

    private static async Task<IResult> UpdateInvitationAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid invitationId,
        UpdateInvitationRequest request,
        IUserInvitationCommands commands,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Users.Invite))
        {
            return Results.Forbid();
        }

        var result = await commands.UpdateInvitationAsync(invitationId, request, cancellationToken);
        return result.Status switch
        {
            InvitationCommandStatus.NotFound => NotFoundWithCode(),
            InvitationCommandStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
            InvitationCommandStatus.Conflict => ConflictWithCode(result.ErrorMessage, result.ErrorCode),
            _ => Results.Ok(result.Response)
        };
    }

    private static async Task<IResult> CancelInvitationAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid invitationId,
        IUserInvitationCommands commands,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Users.Invite))
        {
            return Results.Forbid();
        }

        var result = await commands.CancelInvitationAsync(invitationId, cancellationToken);
        return result.Status switch
        {
            InvitationCommandStatus.NotFound => NotFoundWithCode(),
            InvitationCommandStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
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
            InvitationCommandStatus.NotFound => NotFoundWithCode(),
            InvitationCommandStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
            InvitationCommandStatus.Conflict => ConflictWithCode(result.ErrorMessage, result.ErrorCode),
            InvitationCommandStatus.ExternalFailure => ProblemWithCode(result.ProblemTitle, result.ErrorMessage, result.ErrorCode, result.ProblemStatusCode, ApiErrorCodes.ExternalDependencyFailure),
            _ => Results.Ok(result.Response)
        };
    }

    private static async Task<IResult> CreateUserAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        CreateUserRequest request,
        IUserManagementCommands commands,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Users.Create))
        {
            return Results.Forbid();
        }

        var result = await commands.CreateUserAsync(request, cancellationToken);
        return result.Status switch
        {
            UserCommandStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
            UserCommandStatus.Conflict => ConflictWithCode(result.ErrorMessage, result.ErrorCode),
            UserCommandStatus.ExternalFailure => ProblemWithCode(result.ProblemTitle, result.ErrorMessage, result.ErrorCode, result.ProblemStatusCode, ApiErrorCodes.ExternalDependencyFailure),
            _ => Results.Created($"/api/v1/users/{result.Response!.Id}", result.Response)
        };
    }

    private static string ResolveActor(ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.Email)
        ?? principal.FindFirstValue("preferred_username")
        ?? principal.FindFirstValue("name")
        ?? principal.FindFirstValue("sub")
        ?? "system";

    private static string? ResolveCurrentUserId(ClaimsPrincipal principal) =>
        principal.FindFirstValue("sub")
        ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

    private static bool LacksPermission(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission) =>
        !permissionMatrix.HasPermission(principal, permission);

    private static async Task<bool> HasProjectReadAccessAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IProjectQueries queries,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        if (permissionMatrix.HasAnyPermission(
                principal,
                Permissions.Projects.Read,
                Permissions.Projects.Manage,
                Permissions.Projects.ManageRoles,
                Permissions.Projects.ManageMembers,
                Permissions.Projects.ReadEvidence,
                Permissions.Projects.ReadCompliance,
                Permissions.Projects.ExportEvidence))
        {
            return true;
        }

        var currentUserId = ResolveCurrentUserId(principal);
        return !string.IsNullOrWhiteSpace(currentUserId)
               && await queries.HasProjectAccessAsync(projectId, currentUserId, cancellationToken);
    }

    private static IResult BadRequestWithCode(string? detail, string? code = null) =>
        Results.BadRequest(ApiProblemDetailsFactory.Create(
            StatusCodes.Status400BadRequest,
            code ?? ApiErrorCodeResolver.Resolve(detail, ApiErrorCodes.RequestValidationFailed),
            "Validation failed.",
            detail));

    private static IResult ConflictWithCode(string? detail, string? code = null) =>
        Results.Conflict(ApiProblemDetailsFactory.Create(
            StatusCodes.Status409Conflict,
            code ?? ApiErrorCodeResolver.Resolve(detail, ApiErrorCodes.RequestValidationFailed),
            "Request conflict.",
            detail));

    private static IResult NotFoundWithCode(string? detail = null) =>
        Results.NotFound(ApiProblemDetailsFactory.Create(
            StatusCodes.Status404NotFound,
            ApiErrorCodes.ResourceNotFound,
            "Resource not found.",
            detail));

    private static IResult ProblemWithCode(string? title, string? detail, string? code, int? statusCode, string fallbackCode) =>
        Results.Problem(ApiProblemDetailsFactory.Create(
            statusCode ?? StatusCodes.Status500InternalServerError,
            code ?? ApiErrorCodeResolver.Resolve(detail, fallbackCode),
            title ?? "Request failed.",
            detail));

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
        new(entity.Id, entity.Name, entity.DisplayOrder, entity.DivisionId, null, null, null, entity.CreatedAt, entity.UpdatedAt, entity.DeletedReason, entity.DeletedBy, entity.DeletedAt);

    private static MasterDataResponse ToResponse(JobTitleEntity entity) =>
        new(entity.Id, entity.Name, entity.DisplayOrder, null, null, entity.DepartmentId, null, entity.CreatedAt, entity.UpdatedAt, entity.DeletedReason, entity.DeletedBy, entity.DeletedAt);
}
