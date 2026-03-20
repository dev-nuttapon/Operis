using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Audits.Application;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Users.Application;

public interface IProjectCommands
{
    Task<(bool Success, string? Error, string? ErrorCode, ProjectResponse? Response)> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken);
    Task<(bool Success, string? Error, string? ErrorCode, ProjectResponse? Response, bool NotFound)> UpdateProjectAsync(Guid projectId, UpdateProjectRequest request, CancellationToken cancellationToken);
    Task<(bool Success, bool NotFound)> DeleteProjectAsync(Guid projectId, SoftDeleteRequest request, string actor, CancellationToken cancellationToken);
    Task<(bool Success, string? Error, string? ErrorCode, ProjectRoleResponse? Response)> CreateProjectRoleAsync(CreateProjectRoleRequest request, CancellationToken cancellationToken);
    Task<(bool Success, string? Error, string? ErrorCode, ProjectRoleResponse? Response, bool NotFound)> UpdateProjectRoleAsync(Guid projectRoleId, UpdateProjectRoleRequest request, CancellationToken cancellationToken);
    Task<(bool Success, bool NotFound)> DeleteProjectRoleAsync(Guid projectRoleId, SoftDeleteRequest request, string actor, CancellationToken cancellationToken);
    Task<(bool Success, string? Error, string? ErrorCode, ProjectAssignmentResponse? Response)> CreateProjectAssignmentAsync(CreateProjectAssignmentRequest request, CancellationToken cancellationToken);
    Task<(bool Success, string? Error, string? ErrorCode, ProjectAssignmentResponse? Response, bool NotFound)> UpdateProjectAssignmentAsync(Guid assignmentId, UpdateProjectAssignmentRequest request, CancellationToken cancellationToken);
    Task<(bool Success, bool NotFound)> DeleteProjectAssignmentAsync(Guid assignmentId, SoftDeleteRequest request, CancellationToken cancellationToken);
}

public sealed class ProjectCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IReferenceDataCache referenceDataCache,
    IBusinessAuditEventWriter businessAuditEventWriter,
    ProjectHistoryWriter historyWriter) : IProjectCommands
{
    public async Task<(bool Success, string? Error, string? ErrorCode, ProjectResponse? Response)> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var code = NormalizeRequired(request.Code, 120);
        var name = NormalizeRequired(request.Name, 200);
        var projectType = NormalizeRequired(request.ProjectType, 80);
        if (code is null || name is null)
        {
            return (false, "Project code and name are required.", ApiErrorCodes.ProjectRequiredFields, null);
        }
        if (projectType is null)
        {
            return (false, "Project type is required.", ApiErrorCodes.ProjectTypeRequired, null);
        }

        var ownerUserId = NormalizeOptional(request.OwnerUserId, 64);
        var sponsorUserId = NormalizeOptional(request.SponsorUserId, 64);
        if (await ValidateProjectUsersAsync(ownerUserId, sponsorUserId, cancellationToken) is { } userError)
        {
            return (false, userError, ApiErrorCodeResolver.Resolve(userError, ApiErrorCodes.RequestValidationFailed), null);
        }

        if (await ValidateWorkflowDefinitionAsync(request.WorkflowDefinitionId, cancellationToken) is { } workflowError)
        {
            return (false, workflowError, ApiErrorCodes.RequestValidationFailed, null);
        }

        if (await ValidateDocumentTemplateAsync(request.DocumentTemplateId, cancellationToken) is { } templateError)
        {
            return (false, templateError, ApiErrorCodes.RequestValidationFailed, null);
        }

        var exists = await dbContext.Projects.AnyAsync(x => x.Code == code && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return (false, "Project code already exists.", ApiErrorCodes.ProjectCodeExists, null);
        }

        var entity = new ProjectEntity
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            ProjectType = projectType,
            OwnerUserId = ownerUserId,
            SponsorUserId = sponsorUserId,
            Methodology = NormalizeOptional(request.Methodology, 80),
            Phase = NormalizeOptional(request.Phase, 80),
            Status = NormalizeStatus(request.Status),
            StatusReason = NormalizeOptional(request.StatusReason, 500),
            WorkflowDefinitionId = request.WorkflowDefinitionId,
            DocumentTemplateId = request.DocumentTemplateId,
            PlannedStartAt = request.PlannedStartAt,
            PlannedEndAt = request.PlannedEndAt,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Projects.Add(entity);
        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "create", EntityType: "project", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status201Created, After: ToProjectState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        await historyWriter.AppendAsync(
            entity.Id,
            "created",
            null,
            ToProjectState(entity),
            "Created project",
            null,
            new { entity.Code, entity.Name, entity.ProjectType },
            cancellationToken);
        await TryAppendBusinessEventAsync(
            "projects",
            "project.created",
            "project",
            entity.Id.ToString(),
            "Created project",
            null,
            new { entity.Code, entity.Name, entity.ProjectType },
            cancellationToken);
        return (true, null, null, await ToProjectResponseAsync(entity, cancellationToken));
    }

    public async Task<(bool Success, string? Error, string? ErrorCode, ProjectResponse? Response, bool NotFound)> UpdateProjectAsync(Guid projectId, UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Projects.FirstOrDefaultAsync(x => x.Id == projectId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return (false, null, null, null, true);
        }

        var code = NormalizeRequired(request.Code, 120);
        var name = NormalizeRequired(request.Name, 200);
        var projectType = NormalizeRequired(request.ProjectType, 80);
        if (code is null || name is null)
        {
            return (false, "Project code and name are required.", ApiErrorCodes.ProjectRequiredFields, null, false);
        }
        if (projectType is null)
        {
            return (false, "Project type is required.", ApiErrorCodes.ProjectTypeRequired, null, false);
        }

        var exists = await dbContext.Projects.AnyAsync(x => x.Id != projectId && x.Code == code && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return (false, "Project code already exists.", ApiErrorCodes.ProjectCodeExists, null, false);
        }

        var ownerUserId = NormalizeOptional(request.OwnerUserId, 64);
        var sponsorUserId = NormalizeOptional(request.SponsorUserId, 64);
        if (await ValidateProjectUsersAsync(ownerUserId, sponsorUserId, cancellationToken) is { } userError)
        {
            return (false, userError, ApiErrorCodeResolver.Resolve(userError, ApiErrorCodes.RequestValidationFailed), null, false);
        }

        if (await ValidateWorkflowDefinitionAsync(request.WorkflowDefinitionId, cancellationToken) is { } workflowError)
        {
            return (false, workflowError, ApiErrorCodes.RequestValidationFailed, null, false);
        }

        if (await ValidateDocumentTemplateAsync(request.DocumentTemplateId, cancellationToken) is { } templateError)
        {
            return (false, templateError, ApiErrorCodes.RequestValidationFailed, null, false);
        }

        var before = ToProjectState(entity);
        entity.Code = code;
        entity.Name = name;
        entity.ProjectType = projectType;
        entity.OwnerUserId = ownerUserId;
        entity.SponsorUserId = sponsorUserId;
        entity.Methodology = NormalizeOptional(request.Methodology, 80);
        entity.Phase = NormalizeOptional(request.Phase, 80);
        entity.Status = NormalizeStatus(request.Status);
        entity.StatusReason = NormalizeOptional(request.StatusReason, 500);
        entity.WorkflowDefinitionId = request.WorkflowDefinitionId;
        entity.DocumentTemplateId = request.DocumentTemplateId;
        entity.PlannedStartAt = request.PlannedStartAt;
        entity.PlannedEndAt = request.PlannedEndAt;
        entity.StartAt = request.StartAt;
        entity.EndAt = request.EndAt;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "update", EntityType: "project", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status200OK, Before: before, After: ToProjectState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        await historyWriter.AppendAsync(
            entity.Id,
            "updated",
            before,
            ToProjectState(entity),
            "Updated project",
            null,
            null,
            cancellationToken);
        await TryAppendBusinessEventAsync(
            "projects",
            "project.updated",
            "project",
            entity.Id.ToString(),
            "Updated project",
            null,
            new { before, after = ToProjectState(entity) },
            cancellationToken);
        return (true, null, null, await ToProjectResponseAsync(entity, cancellationToken), false);
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
        await historyWriter.AppendAsync(
            entity.Id,
            "deleted",
            before,
            ToProjectState(entity),
            "Deleted project",
            entity.DeletedReason,
            null,
            cancellationToken);
        await TryAppendBusinessEventAsync(
            "projects",
            "project.deleted",
            "project",
            entity.Id.ToString(),
            "Deleted project",
            entity.DeletedReason,
            new { before },
            cancellationToken);
        await referenceDataCache.InvalidateProjectRolesAsync(cancellationToken);
        return (true, false);
    }

    public async Task<(bool Success, string? Error, string? ErrorCode, ProjectRoleResponse? Response)> CreateProjectRoleAsync(CreateProjectRoleRequest request, CancellationToken cancellationToken)
    {
        var name = NormalizeRequired(request.Name, 120);
        var code = NormalizeOptional(request.Code, 80);
        if (name is null)
        {
            return (false, "Project role name is required.", ApiErrorCodes.ProjectTypeRoleRequirementRequired, null);
        }

        var exists = await dbContext.ProjectRoles.AnyAsync(x => x.Name == name && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return (false, "Project role already exists.", ApiErrorCodes.ProjectRoleExists, null);
        }
        if (!string.IsNullOrWhiteSpace(code))
        {
            var codeExists = await dbContext.ProjectRoles.AnyAsync(x => x.Code == code && x.DeletedAt == null, cancellationToken);
            if (codeExists)
            {
                return (false, "Project role code already exists.", ApiErrorCodes.ProjectRoleCodeExists, null);
            }
        }

        var entity = new ProjectRoleEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            Description = NormalizeOptional(request.Description, 500),
            Responsibilities = NormalizeOptional(request.Responsibilities, 2000),
            AuthorityScope = NormalizeOptional(request.AuthorityScope, 500),
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.ProjectRoles.Add(entity);
        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "create", EntityType: "project_role", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status201Created, After: ToProjectRoleState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        await TryAppendBusinessEventAsync(
            "projects",
            "project.role.created",
            "project_role",
            entity.Id.ToString(),
            "Created project role",
            null,
            new { entity.Name, entity.Code },
            cancellationToken);
        await referenceDataCache.InvalidateProjectRolesAsync(cancellationToken);
        return (true, null, null, ToProjectRoleResponse(entity, null));
    }

    public async Task<(bool Success, string? Error, string? ErrorCode, ProjectRoleResponse? Response, bool NotFound)> UpdateProjectRoleAsync(Guid projectRoleId, UpdateProjectRoleRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ProjectRoles.FirstOrDefaultAsync(x => x.Id == projectRoleId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return (false, null, null, null, true);
        }

        var name = NormalizeRequired(request.Name, 120);
        var code = NormalizeOptional(request.Code, 80);
        if (name is null)
        {
            return (false, "Project role name is required.", ApiErrorCodes.ProjectTypeRoleRequirementRequired, null, false);
        }

        var exists = await dbContext.ProjectRoles.AnyAsync(x => x.Id != projectRoleId && x.Name == name && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return (false, "Project role already exists.", ApiErrorCodes.ProjectRoleExists, null, false);
        }
        if (!string.IsNullOrWhiteSpace(code))
        {
            var codeExists = await dbContext.ProjectRoles.AnyAsync(x => x.Id != projectRoleId && x.Code == code && x.DeletedAt == null, cancellationToken);
            if (codeExists)
            {
                return (false, "Project role code already exists.", ApiErrorCodes.ProjectRoleCodeExists, null, false);
            }
        }

        var before = ToProjectRoleState(entity);
        entity.Name = name;
        entity.Code = code;
        entity.Description = NormalizeOptional(request.Description, 500);
        entity.Responsibilities = NormalizeOptional(request.Responsibilities, 2000);
        entity.AuthorityScope = NormalizeOptional(request.AuthorityScope, 500);
        entity.DisplayOrder = request.DisplayOrder;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "update", EntityType: "project_role", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status200OK, Before: before, After: ToProjectRoleState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        await TryAppendBusinessEventAsync(
            "projects",
            "project.role.updated",
            "project_role",
            entity.Id.ToString(),
            "Updated project role",
            null,
            new { before, after = ToProjectRoleState(entity) },
            cancellationToken);
        await referenceDataCache.InvalidateProjectRolesAsync(cancellationToken);
        return (true, null, null, ToProjectRoleResponse(entity, null), false);
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
        await TryAppendBusinessEventAsync(
            "projects",
            "project.role.deleted",
            "project_role",
            entity.Id.ToString(),
            "Deleted project role",
            entity.DeletedReason,
            new { before },
            cancellationToken);
        await referenceDataCache.InvalidateProjectRolesAsync(cancellationToken);
        return (true, false);
    }

    public async Task<(bool Success, string? Error, string? ErrorCode, ProjectAssignmentResponse? Response)> CreateProjectAssignmentAsync(CreateProjectAssignmentRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateProjectAssignmentAsync(request.UserId, request.ProjectId, request.ProjectRoleId, request.ReportsToUserId, null, cancellationToken);
        if (validation is not null)
        {
            return (false, validation, ApiErrorCodeResolver.Resolve(validation, ApiErrorCodes.RequestValidationFailed), null);
        }

        var entity = new UserProjectAssignmentEntity
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ProjectId = request.ProjectId,
            ProjectRoleId = request.ProjectRoleId,
            ReportsToUserId = request.ReportsToUserId,
            IsPrimary = request.IsPrimary,
            Status = "Active",
            StartAt = request.StartAt ?? DateTimeOffset.UtcNow,
            EndAt = request.EndAt,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.UserProjectAssignments.Add(entity);
        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "create", EntityType: "project_assignment", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status201Created, After: ToProjectAssignmentState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        await TryAppendBusinessEventAsync(
            "projects",
            "project.assignment.created",
            "project_assignment",
            entity.Id.ToString(),
            "Assigned member to project",
            null,
            new { entity.ProjectId, entity.UserId, entity.ProjectRoleId },
            cancellationToken);
        return (true, null, null, await BuildProjectAssignmentResponseAsync(entity.Id, cancellationToken));
    }

    public async Task<(bool Success, string? Error, string? ErrorCode, ProjectAssignmentResponse? Response, bool NotFound)> UpdateProjectAssignmentAsync(Guid assignmentId, UpdateProjectAssignmentRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.UserProjectAssignments.FirstOrDefaultAsync(x => x.Id == assignmentId, cancellationToken);
        if (entity is null)
        {
            return (false, null, null, null, true);
        }
        if (!string.Equals(entity.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Only active assignments can be updated.", ApiErrorCodes.ProjectAssignmentActiveOnly, null, false);
        }

        var validation = await ValidateProjectAssignmentAsync(request.UserId, request.ProjectId, request.ProjectRoleId, request.ReportsToUserId, assignmentId, cancellationToken);
        if (validation is not null)
        {
            return (false, validation, ApiErrorCodeResolver.Resolve(validation, ApiErrorCodes.RequestValidationFailed), null, false);
        }
        var reason = NormalizeRequired(request.Reason, 500);
        if (reason is null)
        {
            return (false, "Change reason is required.", ApiErrorCodes.ProjectAssignmentChangeReasonRequired, null, false);
        }

        var before = ToProjectAssignmentState(entity);
        var replacement = new UserProjectAssignmentEntity
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ProjectId = request.ProjectId,
            ProjectRoleId = request.ProjectRoleId,
            ReportsToUserId = request.ReportsToUserId,
            IsPrimary = request.IsPrimary,
            Status = "Active",
            ChangeReason = reason,
            StartAt = request.StartAt ?? DateTimeOffset.UtcNow,
            EndAt = request.EndAt,
            CreatedAt = DateTimeOffset.UtcNow
        };

        entity.Status = "Superseded";
        entity.ChangeReason = reason;
        entity.ReplacedByAssignmentId = replacement.Id;
        entity.EndAt = request.StartAt ?? entity.EndAt ?? DateTimeOffset.UtcNow;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.UserProjectAssignments.Add(replacement);
        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "update", EntityType: "project_assignment", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status200OK, Reason: reason, Before: before, After: ToProjectAssignmentState(replacement)));
        await dbContext.SaveChangesAsync(cancellationToken);
        await TryAppendBusinessEventAsync(
            "projects",
            "project.assignment.updated",
            "project_assignment",
            replacement.Id.ToString(),
            "Updated project assignment",
            reason,
            new { before, after = ToProjectAssignmentState(replacement) },
            cancellationToken);
        return (true, null, null, await BuildProjectAssignmentResponseAsync(replacement.Id, cancellationToken), false);
    }

    public async Task<(bool Success, bool NotFound)> DeleteProjectAssignmentAsync(Guid assignmentId, SoftDeleteRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.UserProjectAssignments.FirstOrDefaultAsync(x => x.Id == assignmentId, cancellationToken);
        if (entity is null)
        {
            return (false, true);
        }

        var before = ToProjectAssignmentState(entity);
        entity.Status = "Removed";
        entity.ChangeReason = NormalizeRequired(request.Reason, 500) ?? "No reason provided";
        entity.EndAt = entity.EndAt ?? DateTimeOffset.UtcNow;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "delete", EntityType: "project_assignment", EntityId: assignmentId.ToString(), StatusCode: StatusCodes.Status204NoContent, Reason: entity.ChangeReason, Before: before, After: ToProjectAssignmentState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        await TryAppendBusinessEventAsync(
            "projects",
            "project.assignment.removed",
            "project_assignment",
            assignmentId.ToString(),
            "Removed project assignment",
            entity.ChangeReason,
            new { before, after = ToProjectAssignmentState(entity) },
            cancellationToken);
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

        var roleExists = await dbContext.ProjectRoles.AnyAsync(x => x.Id == projectRoleId && x.DeletedAt == null, cancellationToken);
        if (!roleExists)
        {
            return "Project role does not exist.";
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
            entity.Status,
            entity.ChangeReason,
            entity.ReplacedByAssignmentId,
            entity.StartAt,
            entity.EndAt,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    private async Task TryAppendBusinessEventAsync(
        string module,
        string eventType,
        string entityType,
        string? entityId,
        string? summary,
        string? reason,
        object? metadata,
        CancellationToken cancellationToken)
    {
        try
        {
            await businessAuditEventWriter.AppendAsync(
                module,
                eventType,
                entityType,
                entityId,
                summary,
                reason,
                metadata,
                cancellationToken);
        }
        catch
        {
            // Best-effort business audit; avoid failing business flow on audit write errors.
        }
    }

    private async Task<string?> ValidateProjectUsersAsync(string? ownerUserId, string? sponsorUserId, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(ownerUserId))
        {
            var ownerExists = await dbContext.Users.AnyAsync(x => x.Id == ownerUserId && x.DeletedAt == null, cancellationToken);
            if (!ownerExists)
            {
                return "Project owner does not exist.";
            }
        }

        if (!string.IsNullOrWhiteSpace(sponsorUserId))
        {
            var sponsorExists = await dbContext.Users.AnyAsync(x => x.Id == sponsorUserId && x.DeletedAt == null, cancellationToken);
            if (!sponsorExists)
            {
                return "Project sponsor does not exist.";
            }
        }

        return null;
    }

    private async Task<string?> ValidateWorkflowDefinitionAsync(Guid? workflowDefinitionId, CancellationToken cancellationToken)
    {
        if (!workflowDefinitionId.HasValue)
        {
            return null;
        }

        var workflow = await dbContext.WorkflowDefinitions
            .AsNoTracking()
            .Where(x => x.Id == workflowDefinitionId.Value)
            .Select(x => x.Status)
            .FirstOrDefaultAsync(cancellationToken);

        if (workflow is null)
        {
            return "Workflow definition does not exist.";
        }

        if (string.Equals(workflow, "archived", StringComparison.OrdinalIgnoreCase))
        {
            return "Workflow definition is archived.";
        }

        return null;
    }

    private async Task<string?> ValidateDocumentTemplateAsync(Guid? documentTemplateId, CancellationToken cancellationToken)
    {
        if (!documentTemplateId.HasValue)
        {
            return null;
        }

        var exists = await dbContext.DocumentTemplates
            .AsNoTracking()
            .AnyAsync(x => x.Id == documentTemplateId.Value && !x.IsDeleted, cancellationToken);

        return exists ? null : "Document template does not exist.";
    }

    private async Task<ProjectResponse> ToProjectResponseAsync(ProjectEntity entity, CancellationToken cancellationToken)
    {
        var ownerDisplayName = await ResolveUserDisplayNameAsync(entity.OwnerUserId, cancellationToken);
        var sponsorDisplayName = await ResolveUserDisplayNameAsync(entity.SponsorUserId, cancellationToken);

        return new ProjectResponse(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.ProjectType,
            entity.OwnerUserId,
            ownerDisplayName,
            entity.SponsorUserId,
            sponsorDisplayName,
            entity.Methodology,
            entity.Phase,
            entity.Status,
            entity.StatusReason,
            entity.WorkflowDefinitionId,
            entity.DocumentTemplateId,
            entity.PlannedStartAt,
            entity.PlannedEndAt,
            entity.StartAt,
            entity.EndAt,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.DeletedReason,
            entity.DeletedBy,
            entity.DeletedAt);
    }

    private async Task<string?> ResolveUserDisplayNameAsync(string? userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return await dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == userId && x.DeletedAt == null)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);
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

    private static string? NormalizeOptional(string? value, int maxLength)
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
        entity.ProjectType,
        entity.OwnerUserId,
        entity.SponsorUserId,
        entity.Methodology,
        entity.Phase,
        entity.Status,
        entity.StatusReason,
        entity.WorkflowDefinitionId,
        entity.DocumentTemplateId,
        entity.PlannedStartAt,
        entity.PlannedEndAt,
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
        entity.Name,
        entity.Code,
        entity.Description,
        entity.Responsibilities,
        entity.AuthorityScope,
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
        entity.Status,
        entity.ChangeReason,
        entity.ReplacedByAssignmentId,
        entity.StartAt,
        entity.EndAt,
        entity.CreatedAt,
        entity.UpdatedAt
    };

    private static ProjectRoleResponse ToProjectRoleResponse(ProjectRoleEntity entity, string? projectName) =>
        new(
            entity.Id,
            null,
            null,
            entity.Name,
            entity.Code,
        entity.Description,
        entity.Responsibilities,
        entity.AuthorityScope,
        entity.DisplayOrder,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.DeletedReason,
            entity.DeletedBy,
            entity.DeletedAt);
}
