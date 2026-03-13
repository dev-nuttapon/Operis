using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;

namespace Operis_API.Modules.Users.Application;

public interface IProjectCommands
{
    Task<(bool Success, string? Error, ProjectResponse? Response)> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken);
    Task<(bool Success, string? Error, ProjectResponse? Response, bool NotFound)> UpdateProjectAsync(Guid projectId, UpdateProjectRequest request, CancellationToken cancellationToken);
    Task<(bool Success, bool NotFound)> DeleteProjectAsync(Guid projectId, SoftDeleteRequest request, string actor, CancellationToken cancellationToken);
    Task<(bool Success, string? Error, ProjectRoleResponse? Response)> CreateProjectRoleAsync(CreateProjectRoleRequest request, CancellationToken cancellationToken);
    Task<(bool Success, string? Error, ProjectRoleResponse? Response, bool NotFound)> UpdateProjectRoleAsync(Guid projectRoleId, UpdateProjectRoleRequest request, CancellationToken cancellationToken);
    Task<(bool Success, bool NotFound)> DeleteProjectRoleAsync(Guid projectRoleId, SoftDeleteRequest request, string actor, CancellationToken cancellationToken);
    Task<(bool Success, string? Error, ProjectAssignmentResponse? Response)> CreateProjectAssignmentAsync(CreateProjectAssignmentRequest request, CancellationToken cancellationToken);
    Task<(bool Success, string? Error, ProjectAssignmentResponse? Response, bool NotFound)> UpdateProjectAssignmentAsync(Guid assignmentId, UpdateProjectAssignmentRequest request, CancellationToken cancellationToken);
    Task<(bool Success, bool NotFound)> DeleteProjectAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken);
}

public sealed class ProjectCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IReferenceDataCache referenceDataCache) : IProjectCommands
{
    public async Task<(bool Success, string? Error, ProjectResponse? Response)> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var code = NormalizeRequired(request.Code, 120);
        var name = NormalizeRequired(request.Name, 200);
        if (code is null || name is null)
        {
            return (false, "Project code and name are required.", null);
        }

        var exists = await dbContext.Projects.AnyAsync(x => x.Code == code && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return (false, "Project code already exists.", null);
        }

        var entity = new ProjectEntity
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Status = NormalizeStatus(request.Status),
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Projects.Add(entity);
        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "create", EntityType: "project", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status201Created, After: ToProjectState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        return (true, null, ToProjectResponse(entity));
    }

    public async Task<(bool Success, string? Error, ProjectResponse? Response, bool NotFound)> UpdateProjectAsync(Guid projectId, UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Projects.FirstOrDefaultAsync(x => x.Id == projectId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return (false, null, null, true);
        }

        var code = NormalizeRequired(request.Code, 120);
        var name = NormalizeRequired(request.Name, 200);
        if (code is null || name is null)
        {
            return (false, "Project code and name are required.", null, false);
        }

        var exists = await dbContext.Projects.AnyAsync(x => x.Id != projectId && x.Code == code && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return (false, "Project code already exists.", null, false);
        }

        var before = ToProjectState(entity);
        entity.Code = code;
        entity.Name = name;
        entity.Status = NormalizeStatus(request.Status);
        entity.StartAt = request.StartAt;
        entity.EndAt = request.EndAt;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "update", EntityType: "project", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status200OK, Before: before, After: ToProjectState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        return (true, null, ToProjectResponse(entity), false);
    }

    public async Task<(bool Success, bool NotFound)> DeleteProjectAsync(Guid projectId, SoftDeleteRequest request, string actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Projects.FirstOrDefaultAsync(x => x.Id == projectId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return (false, true);
        }

        var before = ToProjectState(entity);
        entity.DeletedAt = DateTimeOffset.UtcNow;
        entity.DeletedBy = actor;
        entity.DeletedReason = NormalizeRequired(request.Reason, 500) ?? "No reason provided";
        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "soft_delete", EntityType: "project", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status204NoContent, Reason: entity.DeletedReason, Before: before, After: ToProjectState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        await referenceDataCache.InvalidateProjectRolesAsync(cancellationToken);
        return (true, false);
    }

    public async Task<(bool Success, string? Error, ProjectRoleResponse? Response)> CreateProjectRoleAsync(CreateProjectRoleRequest request, CancellationToken cancellationToken)
    {
        var name = NormalizeRequired(request.Name, 120);
        if (name is null)
        {
            return (false, "Project role name is required.", null);
        }

        var project = await dbContext.Projects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ProjectId && x.DeletedAt == null, cancellationToken);
        if (project is null)
        {
            return (false, "Project does not exist.", null);
        }

        var exists = await dbContext.ProjectRoles.AnyAsync(x => x.ProjectId == request.ProjectId && x.Name == name && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return (false, "Project role already exists in this project.", null);
        }

        var entity = new ProjectRoleEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Name = name,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.ProjectRoles.Add(entity);
        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "create", EntityType: "project_role", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status201Created, After: ToProjectRoleState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        await referenceDataCache.InvalidateProjectRolesAsync(cancellationToken);
        return (true, null, ToProjectRoleResponse(entity, project.Name));
    }

    public async Task<(bool Success, string? Error, ProjectRoleResponse? Response, bool NotFound)> UpdateProjectRoleAsync(Guid projectRoleId, UpdateProjectRoleRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ProjectRoles.FirstOrDefaultAsync(x => x.Id == projectRoleId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return (false, null, null, true);
        }

        var name = NormalizeRequired(request.Name, 120);
        if (name is null)
        {
            return (false, "Project role name is required.", null, false);
        }

        var project = await dbContext.Projects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ProjectId && x.DeletedAt == null, cancellationToken);
        if (project is null)
        {
            return (false, "Project does not exist.", null, false);
        }

        var exists = await dbContext.ProjectRoles.AnyAsync(x => x.Id != projectRoleId && x.ProjectId == request.ProjectId && x.Name == name && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return (false, "Project role already exists in this project.", null, false);
        }

        var before = ToProjectRoleState(entity);
        entity.ProjectId = request.ProjectId;
        entity.Name = name;
        entity.DisplayOrder = request.DisplayOrder;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "update", EntityType: "project_role", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status200OK, Before: before, After: ToProjectRoleState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        await referenceDataCache.InvalidateProjectRolesAsync(cancellationToken);
        return (true, null, ToProjectRoleResponse(entity, project.Name), false);
    }

    public async Task<(bool Success, bool NotFound)> DeleteProjectRoleAsync(Guid projectRoleId, SoftDeleteRequest request, string actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ProjectRoles.FirstOrDefaultAsync(x => x.Id == projectRoleId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return (false, true);
        }

        var before = ToProjectRoleState(entity);
        entity.DeletedAt = DateTimeOffset.UtcNow;
        entity.DeletedBy = actor;
        entity.DeletedReason = NormalizeRequired(request.Reason, 500) ?? "No reason provided";
        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "soft_delete", EntityType: "project_role", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status204NoContent, Reason: entity.DeletedReason, Before: before, After: ToProjectRoleState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        await referenceDataCache.InvalidateProjectRolesAsync(cancellationToken);
        return (true, false);
    }

    public async Task<(bool Success, string? Error, ProjectAssignmentResponse? Response)> CreateProjectAssignmentAsync(CreateProjectAssignmentRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateProjectAssignmentAsync(request.UserId, request.ProjectId, request.ProjectRoleId, request.ReportsToUserId, null, cancellationToken);
        if (validation is not null)
        {
            return (false, validation, null);
        }

        var entity = new UserProjectAssignmentEntity
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ProjectId = request.ProjectId,
            ProjectRoleId = request.ProjectRoleId,
            ReportsToUserId = request.ReportsToUserId,
            IsPrimary = request.IsPrimary,
            StartAt = request.StartAt ?? DateTimeOffset.UtcNow,
            EndAt = request.EndAt,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.UserProjectAssignments.Add(entity);
        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "create", EntityType: "project_assignment", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status201Created, After: ToProjectAssignmentState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        return (true, null, await BuildProjectAssignmentResponseAsync(entity.Id, cancellationToken));
    }

    public async Task<(bool Success, string? Error, ProjectAssignmentResponse? Response, bool NotFound)> UpdateProjectAssignmentAsync(Guid assignmentId, UpdateProjectAssignmentRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.UserProjectAssignments.FirstOrDefaultAsync(x => x.Id == assignmentId, cancellationToken);
        if (entity is null)
        {
            return (false, null, null, true);
        }

        var validation = await ValidateProjectAssignmentAsync(request.UserId, request.ProjectId, request.ProjectRoleId, request.ReportsToUserId, assignmentId, cancellationToken);
        if (validation is not null)
        {
            return (false, validation, null, false);
        }

        var before = ToProjectAssignmentState(entity);
        entity.UserId = request.UserId;
        entity.ProjectId = request.ProjectId;
        entity.ProjectRoleId = request.ProjectRoleId;
        entity.ReportsToUserId = request.ReportsToUserId;
        entity.IsPrimary = request.IsPrimary;
        entity.StartAt = request.StartAt ?? entity.StartAt;
        entity.EndAt = request.EndAt;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "update", EntityType: "project_assignment", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status200OK, Before: before, After: ToProjectAssignmentState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        return (true, null, await BuildProjectAssignmentResponseAsync(entity.Id, cancellationToken), false);
    }

    public async Task<(bool Success, bool NotFound)> DeleteProjectAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.UserProjectAssignments.FirstOrDefaultAsync(x => x.Id == assignmentId, cancellationToken);
        if (entity is null)
        {
            return (false, true);
        }

        var before = ToProjectAssignmentState(entity);
        dbContext.UserProjectAssignments.Remove(entity);
        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "delete", EntityType: "project_assignment", EntityId: assignmentId.ToString(), StatusCode: StatusCodes.Status204NoContent, Before: before));
        await dbContext.SaveChangesAsync(cancellationToken);
        return (true, false);
    }

    private async Task<string?> ValidateProjectAssignmentAsync(string userId, Guid projectId, Guid projectRoleId, string? reportsToUserId, Guid? assignmentId, CancellationToken cancellationToken)
    {
        var userExists = await dbContext.Users.AnyAsync(x => x.Id == userId && x.DeletedAt == null, cancellationToken);
        if (!userExists)
        {
            return "User does not exist.";
        }

        var projectExists = await dbContext.Projects.AnyAsync(x => x.Id == projectId && x.DeletedAt == null, cancellationToken);
        if (!projectExists)
        {
            return "Project does not exist.";
        }

        var roleExists = await dbContext.ProjectRoles.AnyAsync(x => x.Id == projectRoleId && x.ProjectId == projectId && x.DeletedAt == null, cancellationToken);
        if (!roleExists)
        {
            return "Project role does not exist in this project.";
        }

        if (!string.IsNullOrWhiteSpace(reportsToUserId))
        {
            var leaderExists = await dbContext.UserProjectAssignments.AnyAsync(
                x => x.ProjectId == projectId && x.UserId == reportsToUserId && (!assignmentId.HasValue || x.Id != assignmentId.Value),
                cancellationToken);
            if (!leaderExists)
            {
                return "Reporting line user must already be assigned to this project.";
            }
        }

        return null;
    }

    private async Task<ProjectAssignmentResponse> BuildProjectAssignmentResponseAsync(Guid assignmentId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.UserProjectAssignments.AsNoTracking().FirstAsync(x => x.Id == assignmentId, cancellationToken);
        var projectName = await dbContext.Projects.Where(x => x.Id == entity.ProjectId).Select(x => x.Name).FirstAsync(cancellationToken);
        var roleName = await dbContext.ProjectRoles.Where(x => x.Id == entity.ProjectRoleId).Select(x => x.Name).FirstAsync(cancellationToken);
        var userDisplay = entity.UserId;
        var reportsToDisplay = string.IsNullOrWhiteSpace(entity.ReportsToUserId) ? null : entity.ReportsToUserId;
        return new ProjectAssignmentResponse(
            entity.Id,
            entity.UserId,
            entity.UserId,
            userDisplay,
            entity.ProjectId,
            projectName,
            entity.ProjectRoleId,
            roleName,
            entity.ReportsToUserId,
            reportsToDisplay,
            entity.IsPrimary,
            entity.StartAt,
            entity.EndAt,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    private static string NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return "active";
        }

        var normalized = status.Trim().ToLowerInvariant();
        return normalized.Length > 32 ? normalized[..32] : normalized;
    }

    private static string? NormalizeRequired(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        return normalized.Length > maxLength ? normalized[..maxLength] : normalized;
    }

    private static object ToProjectState(ProjectEntity entity) => new
    {
        entity.Id,
        entity.Code,
        entity.Name,
        entity.Status,
        entity.StartAt,
        entity.EndAt,
        entity.CreatedAt,
        entity.UpdatedAt,
        entity.DeletedReason,
        entity.DeletedBy,
        entity.DeletedAt
    };

    private static object ToProjectRoleState(ProjectRoleEntity entity) => new
    {
        entity.Id,
        entity.ProjectId,
        entity.Name,
        entity.DisplayOrder,
        entity.CreatedAt,
        entity.UpdatedAt,
        entity.DeletedReason,
        entity.DeletedBy,
        entity.DeletedAt
    };

    private static object ToProjectAssignmentState(UserProjectAssignmentEntity entity) => new
    {
        entity.Id,
        entity.UserId,
        entity.ProjectId,
        entity.ProjectRoleId,
        entity.ReportsToUserId,
        entity.IsPrimary,
        entity.StartAt,
        entity.EndAt,
        entity.CreatedAt,
        entity.UpdatedAt
    };

    private static ProjectResponse ToProjectResponse(ProjectEntity entity) =>
        new(entity.Id, entity.Code, entity.Name, entity.Status, entity.StartAt, entity.EndAt, entity.CreatedAt, entity.UpdatedAt, entity.DeletedReason, entity.DeletedBy, entity.DeletedAt);

    private static ProjectRoleResponse ToProjectRoleResponse(ProjectRoleEntity entity, string? projectName) =>
        new(entity.Id, entity.ProjectId, projectName, entity.Name, entity.DisplayOrder, entity.CreatedAt, entity.UpdatedAt, entity.DeletedReason, entity.DeletedBy, entity.DeletedAt);
}
