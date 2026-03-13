using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;

namespace Operis_API.Modules.Users.Application;

public interface IProjectTemplateCommands
{
    Task<(bool Success, string? Error, ProjectTypeTemplateResponse? Response)> CreateProjectTypeTemplateAsync(CreateProjectTypeTemplateRequest request, CancellationToken cancellationToken);
    Task<(bool Success, string? Error, ProjectTypeTemplateResponse? Response, bool NotFound)> UpdateProjectTypeTemplateAsync(Guid templateId, UpdateProjectTypeTemplateRequest request, CancellationToken cancellationToken);
    Task<(bool Success, bool NotFound)> DeleteProjectTypeTemplateAsync(Guid templateId, SoftDeleteRequest request, string actor, CancellationToken cancellationToken);
    Task<(bool Success, string? Error, ProjectTypeRoleRequirementResponse? Response)> CreateProjectTypeRoleRequirementAsync(CreateProjectTypeRoleRequirementRequest request, CancellationToken cancellationToken);
    Task<(bool Success, string? Error, ProjectTypeRoleRequirementResponse? Response, bool NotFound)> UpdateProjectTypeRoleRequirementAsync(Guid requirementId, UpdateProjectTypeRoleRequirementRequest request, CancellationToken cancellationToken);
    Task<(bool Success, bool NotFound)> DeleteProjectTypeRoleRequirementAsync(Guid requirementId, SoftDeleteRequest request, string actor, CancellationToken cancellationToken);
}

public sealed class ProjectTemplateCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter) : IProjectTemplateCommands
{
    public async Task<(bool Success, string? Error, ProjectTypeTemplateResponse? Response)> CreateProjectTypeTemplateAsync(CreateProjectTypeTemplateRequest request, CancellationToken cancellationToken)
    {
        var projectType = NormalizeRequired(request.ProjectType, 80);
        if (projectType is null)
        {
            return (false, "Project type is required.", null);
        }

        var exists = await dbContext.ProjectTypeTemplates.AnyAsync(x => x.ProjectType == projectType && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return (false, "Project type template already exists.", null);
        }

        var entity = new ProjectTypeTemplateEntity
        {
            Id = Guid.NewGuid(),
            ProjectType = projectType,
            RequireSponsor = request.RequireSponsor,
            RequirePlannedPeriod = request.RequirePlannedPeriod,
            RequireActiveTeam = request.RequireActiveTeam,
            RequirePrimaryAssignment = request.RequirePrimaryAssignment,
            RequireReportingRoot = request.RequireReportingRoot,
            RequireDocumentCreator = request.RequireDocumentCreator,
            RequireReviewer = request.RequireReviewer,
            RequireApprover = request.RequireApprover,
            RequireReleaseRole = request.RequireReleaseRole,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        dbContext.ProjectTypeTemplates.Add(entity);
        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "create", EntityType: "project_type_template", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status201Created, After: ToProjectTypeTemplateState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        return (true, null, ToProjectTypeTemplateResponse(entity));
    }

    public async Task<(bool Success, string? Error, ProjectTypeTemplateResponse? Response, bool NotFound)> UpdateProjectTypeTemplateAsync(Guid templateId, UpdateProjectTypeTemplateRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ProjectTypeTemplates.FirstOrDefaultAsync(x => x.Id == templateId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return (false, null, null, true);
        }

        var projectType = NormalizeRequired(request.ProjectType, 80);
        if (projectType is null)
        {
            return (false, "Project type is required.", null, false);
        }

        var exists = await dbContext.ProjectTypeTemplates.AnyAsync(x => x.Id != templateId && x.ProjectType == projectType && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return (false, "Project type template already exists.", null, false);
        }

        var before = ToProjectTypeTemplateState(entity);
        entity.ProjectType = projectType;
        entity.RequireSponsor = request.RequireSponsor;
        entity.RequirePlannedPeriod = request.RequirePlannedPeriod;
        entity.RequireActiveTeam = request.RequireActiveTeam;
        entity.RequirePrimaryAssignment = request.RequirePrimaryAssignment;
        entity.RequireReportingRoot = request.RequireReportingRoot;
        entity.RequireDocumentCreator = request.RequireDocumentCreator;
        entity.RequireReviewer = request.RequireReviewer;
        entity.RequireApprover = request.RequireApprover;
        entity.RequireReleaseRole = request.RequireReleaseRole;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "update", EntityType: "project_type_template", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status200OK, Before: before, After: ToProjectTypeTemplateState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        return (true, null, ToProjectTypeTemplateResponse(entity), false);
    }

    public async Task<(bool Success, bool NotFound)> DeleteProjectTypeTemplateAsync(Guid templateId, SoftDeleteRequest request, string actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ProjectTypeTemplates.FirstOrDefaultAsync(x => x.Id == templateId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return (false, true);
        }

        var before = ToProjectTypeTemplateState(entity);
        entity.DeletedAt = DateTimeOffset.UtcNow;
        entity.DeletedBy = actor;
        entity.DeletedReason = NormalizeRequired(request.Reason, 500) ?? "No reason provided";
        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "soft_delete", EntityType: "project_type_template", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status204NoContent, Reason: entity.DeletedReason, Before: before, After: ToProjectTypeTemplateState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        return (true, false);
    }

    public async Task<(bool Success, string? Error, ProjectTypeRoleRequirementResponse? Response)> CreateProjectTypeRoleRequirementAsync(CreateProjectTypeRoleRequirementRequest request, CancellationToken cancellationToken)
    {
        var template = await dbContext.ProjectTypeTemplates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ProjectTypeTemplateId && x.DeletedAt == null, cancellationToken);
        if (template is null)
        {
            return (false, "Project type template does not exist.", null);
        }

        var roleName = NormalizeRequired(request.RoleName, 120);
        var roleCode = NormalizeOptional(request.RoleCode, 80);
        if (roleName is null)
        {
            return (false, "Role name is required.", null);
        }

        var exists = await dbContext.ProjectTypeRoleRequirements.AnyAsync(x => x.ProjectTypeTemplateId == request.ProjectTypeTemplateId && x.RoleName == roleName && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return (false, "Role requirement already exists for this project type.", null);
        }

        if (!string.IsNullOrWhiteSpace(roleCode))
        {
            var codeExists = await dbContext.ProjectTypeRoleRequirements.AnyAsync(x => x.ProjectTypeTemplateId == request.ProjectTypeTemplateId && x.RoleCode == roleCode && x.DeletedAt == null, cancellationToken);
            if (codeExists)
            {
                return (false, "Role requirement code already exists for this project type.", null);
            }
        }

        var entity = new ProjectTypeRoleRequirementEntity
        {
            Id = Guid.NewGuid(),
            ProjectTypeTemplateId = request.ProjectTypeTemplateId,
            RoleName = roleName,
            RoleCode = roleCode,
            Description = NormalizeOptional(request.Description, 500),
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        dbContext.ProjectTypeRoleRequirements.Add(entity);
        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "create", EntityType: "project_type_role_requirement", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status201Created, After: ToProjectTypeRoleRequirementState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        return (true, null, ToProjectTypeRoleRequirementResponse(entity, template.ProjectType));
    }

    public async Task<(bool Success, string? Error, ProjectTypeRoleRequirementResponse? Response, bool NotFound)> UpdateProjectTypeRoleRequirementAsync(Guid requirementId, UpdateProjectTypeRoleRequirementRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ProjectTypeRoleRequirements.FirstOrDefaultAsync(x => x.Id == requirementId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return (false, null, null, true);
        }

        var template = await dbContext.ProjectTypeTemplates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ProjectTypeTemplateId && x.DeletedAt == null, cancellationToken);
        if (template is null)
        {
            return (false, "Project type template does not exist.", null, false);
        }

        var roleName = NormalizeRequired(request.RoleName, 120);
        var roleCode = NormalizeOptional(request.RoleCode, 80);
        if (roleName is null)
        {
            return (false, "Role name is required.", null, false);
        }

        var exists = await dbContext.ProjectTypeRoleRequirements.AnyAsync(x => x.Id != requirementId && x.ProjectTypeTemplateId == request.ProjectTypeTemplateId && x.RoleName == roleName && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return (false, "Role requirement already exists for this project type.", null, false);
        }

        if (!string.IsNullOrWhiteSpace(roleCode))
        {
            var codeExists = await dbContext.ProjectTypeRoleRequirements.AnyAsync(x => x.Id != requirementId && x.ProjectTypeTemplateId == request.ProjectTypeTemplateId && x.RoleCode == roleCode && x.DeletedAt == null, cancellationToken);
            if (codeExists)
            {
                return (false, "Role requirement code already exists for this project type.", null, false);
            }
        }

        var before = ToProjectTypeRoleRequirementState(entity);
        entity.ProjectTypeTemplateId = request.ProjectTypeTemplateId;
        entity.RoleName = roleName;
        entity.RoleCode = roleCode;
        entity.Description = NormalizeOptional(request.Description, 500);
        entity.DisplayOrder = request.DisplayOrder;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "update", EntityType: "project_type_role_requirement", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status200OK, Before: before, After: ToProjectTypeRoleRequirementState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        return (true, null, ToProjectTypeRoleRequirementResponse(entity, template.ProjectType), false);
    }

    public async Task<(bool Success, bool NotFound)> DeleteProjectTypeRoleRequirementAsync(Guid requirementId, SoftDeleteRequest request, string actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ProjectTypeRoleRequirements.FirstOrDefaultAsync(x => x.Id == requirementId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return (false, true);
        }

        var before = ToProjectTypeRoleRequirementState(entity);
        entity.DeletedAt = DateTimeOffset.UtcNow;
        entity.DeletedBy = actor;
        entity.DeletedReason = NormalizeRequired(request.Reason, 500) ?? "No reason provided";
        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "soft_delete", EntityType: "project_type_role_requirement", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status204NoContent, Reason: entity.DeletedReason, Before: before, After: ToProjectTypeRoleRequirementState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        return (true, false);
    }

    private static string? NormalizeRequired(string? value, int maxLength)
    {
        var normalized = NormalizeOptional(value, maxLength);
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static object ToProjectTypeTemplateState(ProjectTypeTemplateEntity entity) => new
    {
        entity.ProjectType,
        entity.RequireSponsor,
        entity.RequirePlannedPeriod,
        entity.RequireActiveTeam,
        entity.RequirePrimaryAssignment,
        entity.RequireReportingRoot,
        entity.RequireDocumentCreator,
        entity.RequireReviewer,
        entity.RequireApprover,
        entity.RequireReleaseRole,
        entity.DeletedReason,
        entity.DeletedBy,
        entity.DeletedAt
    };

    private static object ToProjectTypeRoleRequirementState(ProjectTypeRoleRequirementEntity entity) => new
    {
        entity.ProjectTypeTemplateId,
        entity.RoleName,
        entity.RoleCode,
        entity.Description,
        entity.DisplayOrder,
        entity.DeletedReason,
        entity.DeletedBy,
        entity.DeletedAt
    };

    private static ProjectTypeTemplateResponse ToProjectTypeTemplateResponse(ProjectTypeTemplateEntity entity) =>
        new(
            entity.Id,
            entity.ProjectType,
            entity.RequireSponsor,
            entity.RequirePlannedPeriod,
            entity.RequireActiveTeam,
            entity.RequirePrimaryAssignment,
            entity.RequireReportingRoot,
            entity.RequireDocumentCreator,
            entity.RequireReviewer,
            entity.RequireApprover,
            entity.RequireReleaseRole,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.DeletedReason,
            entity.DeletedBy,
            entity.DeletedAt);

    private static ProjectTypeRoleRequirementResponse ToProjectTypeRoleRequirementResponse(ProjectTypeRoleRequirementEntity entity, string projectType) =>
        new(
            entity.Id,
            entity.ProjectTypeTemplateId,
            projectType,
            entity.RoleName,
            entity.RoleCode,
            entity.Description,
            entity.DisplayOrder,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.DeletedReason,
            entity.DeletedBy,
            entity.DeletedAt);
}
