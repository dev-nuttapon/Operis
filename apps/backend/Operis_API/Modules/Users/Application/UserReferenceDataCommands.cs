using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;

namespace Operis_API.Modules.Users.Application;

public sealed class UserReferenceDataCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IReferenceDataCache referenceDataCache) : IUserReferenceDataCommands
{
    public async Task<MasterDataCommandResult> CreateDivisionAsync(CreateMasterDataRequest request, CancellationToken cancellationToken)
    {
        var name = NormalizeRequiredName(request.Name);
        if (name is null)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.ValidationError, "Division name is required.");
        }

        var exists = await dbContext.Divisions.AnyAsync(x => x.Name == name && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.Conflict, "Division already exists.");
        }

        var entity = new DivisionEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Divisions.Add(entity);
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "create",
            EntityType: "division",
            EntityId: entity.Id.ToString(),
            StatusCode: StatusCodes.Status201Created,
            After: ToDivisionAuditState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        await referenceDataCache.InvalidateDivisionsAsync(cancellationToken);

        return new MasterDataCommandResult(MasterDataCommandStatus.Success, Response: ToResponse(entity));
    }

    public async Task<MasterDataCommandResult> UpdateDivisionAsync(Guid divisionId, UpdateMasterDataRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Divisions.FirstOrDefaultAsync(x => x.Id == divisionId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.NotFound);
        }

        var name = NormalizeRequiredName(request.Name);
        if (name is null)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.ValidationError, "Division name is required.");
        }

        var exists = await dbContext.Divisions.AnyAsync(x => x.Id != divisionId && x.Name == name && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.Conflict, "Division already exists.");
        }

        var before = ToDivisionAuditState(entity);
        entity.Name = name;
        entity.DisplayOrder = request.DisplayOrder;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "update",
            EntityType: "division",
            EntityId: entity.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            Before: before,
            After: ToDivisionAuditState(entity),
            Changes: new { entity.Name, entity.DisplayOrder, entity.UpdatedAt }));
        await dbContext.SaveChangesAsync(cancellationToken);
        await referenceDataCache.InvalidateDivisionsAsync(cancellationToken);

        return new MasterDataCommandResult(MasterDataCommandStatus.Success, Response: ToResponse(entity));
    }

    public async Task<MasterDataCommandResult> DeleteDivisionAsync(Guid divisionId, SoftDeleteRequest request, string actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Divisions.FirstOrDefaultAsync(x => x.Id == divisionId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.NotFound);
        }

        var before = ToDivisionAuditState(entity);
        entity.DeletedAt = DateTimeOffset.UtcNow;
        entity.DeletedBy = actor;
        entity.DeletedReason = NormalizeDeleteReason(request.Reason);
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "soft_delete",
            EntityType: "division",
            EntityId: entity.Id.ToString(),
            StatusCode: StatusCodes.Status204NoContent,
            Reason: entity.DeletedReason,
            Before: before,
            After: ToDivisionAuditState(entity),
            Changes: new { entity.DeletedAt, entity.DeletedBy, entity.DeletedReason }));
        await dbContext.SaveChangesAsync(cancellationToken);
        await referenceDataCache.InvalidateDivisionsAsync(cancellationToken);

        return new MasterDataCommandResult(MasterDataCommandStatus.Success);
    }

    public async Task<MasterDataCommandResult> CreateDepartmentAsync(CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var name = NormalizeRequiredName(request.Name);
        if (name is null)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.ValidationError, "Department name is required.");
        }

        if (request.DivisionId.HasValue)
        {
            var divisionExists = await dbContext.Divisions.AnyAsync(
                x => x.Id == request.DivisionId.Value && x.DeletedAt == null,
                cancellationToken);
            if (!divisionExists)
            {
                return new MasterDataCommandResult(MasterDataCommandStatus.ValidationError, "Division does not exist.");
            }
        }

        var exists = await dbContext.Departments.AnyAsync(x => x.Name == name && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.Conflict, "Department already exists.");
        }

        var entity = new DepartmentEntity
        {
            Id = Guid.NewGuid(),
            DivisionId = request.DivisionId,
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

        var divisionName = entity.DivisionId.HasValue
            ? await dbContext.Divisions
                .Where(x => x.Id == entity.DivisionId.Value)
                .Select(x => x.Name)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        return new MasterDataCommandResult(MasterDataCommandStatus.Success, Response: ToResponse(entity, divisionName));
    }

    public async Task<MasterDataCommandResult> UpdateDepartmentAsync(Guid departmentId, UpdateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Departments.FirstOrDefaultAsync(x => x.Id == departmentId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.NotFound);
        }

        var name = NormalizeRequiredName(request.Name);
        if (name is null)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.ValidationError, "Department name is required.");
        }

        if (request.DivisionId.HasValue)
        {
            var divisionExists = await dbContext.Divisions.AnyAsync(
                x => x.Id == request.DivisionId.Value && x.DeletedAt == null,
                cancellationToken);
            if (!divisionExists)
            {
                return new MasterDataCommandResult(MasterDataCommandStatus.ValidationError, "Division does not exist.");
            }
        }

        var exists = await dbContext.Departments.AnyAsync(x => x.Id != departmentId && x.Name == name && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.Conflict, "Department already exists.");
        }

        var before = ToDepartmentAuditState(entity);
        entity.DivisionId = request.DivisionId;
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
                entity.DivisionId,
                entity.Name,
                entity.DisplayOrder,
                entity.UpdatedAt
            }));
        await dbContext.SaveChangesAsync(cancellationToken);
        await referenceDataCache.InvalidateDepartmentsAsync(cancellationToken);

        var divisionName = entity.DivisionId.HasValue
            ? await dbContext.Divisions
                .Where(x => x.Id == entity.DivisionId.Value)
                .Select(x => x.Name)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        return new MasterDataCommandResult(MasterDataCommandStatus.Success, Response: ToResponse(entity, divisionName));
    }

    public async Task<MasterDataCommandResult> DeleteDepartmentAsync(Guid departmentId, SoftDeleteRequest request, string actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Departments.FirstOrDefaultAsync(x => x.Id == departmentId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.NotFound);
        }

        var before = ToDepartmentAuditState(entity);
        entity.DeletedAt = DateTimeOffset.UtcNow;
        entity.DeletedBy = actor;
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

        return new MasterDataCommandResult(MasterDataCommandStatus.Success);
    }

    public async Task<MasterDataCommandResult> CreateJobTitleAsync(CreateJobTitleRequest request, CancellationToken cancellationToken)
    {
        var name = NormalizeRequiredName(request.Name);
        if (name is null)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.ValidationError, "Job title name is required.");
        }

        if (!request.DepartmentId.HasValue)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.ValidationError, "Department is required when job title is selected.");
        }

        var department = await dbContext.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.DepartmentId.Value && x.DeletedAt == null, cancellationToken);
        if (department is null)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.ValidationError, "Department does not exist.");
        }

        var exists = await dbContext.JobTitles.AnyAsync(x => x.Name == name && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.Conflict, "Job title already exists.");
        }

        var entity = new JobTitleEntity
        {
            Id = Guid.NewGuid(),
            DepartmentId = request.DepartmentId,
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

        var divisionName = department.DivisionId.HasValue
            ? await dbContext.Divisions
                .Where(x => x.Id == department.DivisionId.Value)
                .Select(x => x.Name)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        return new MasterDataCommandResult(MasterDataCommandStatus.Success, Response: ToResponse(entity, department.Name, department.DivisionId, divisionName));
    }

    public async Task<MasterDataCommandResult> UpdateJobTitleAsync(Guid jobTitleId, UpdateJobTitleRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.JobTitles.FirstOrDefaultAsync(x => x.Id == jobTitleId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.NotFound);
        }

        var name = NormalizeRequiredName(request.Name);
        if (name is null)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.ValidationError, "Job title name is required.");
        }

        if (!request.DepartmentId.HasValue)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.ValidationError, "Department is required when job title is selected.");
        }

        var department = await dbContext.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.DepartmentId.Value && x.DeletedAt == null, cancellationToken);
        if (department is null)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.ValidationError, "Department does not exist.");
        }

        var exists = await dbContext.JobTitles.AnyAsync(x => x.Id != jobTitleId && x.Name == name && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.Conflict, "Job title already exists.");
        }

        var before = ToJobTitleAuditState(entity);
        entity.DepartmentId = request.DepartmentId;
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
                entity.DepartmentId,
                entity.Name,
                entity.DisplayOrder,
                entity.UpdatedAt
            }));
        await dbContext.SaveChangesAsync(cancellationToken);
        await referenceDataCache.InvalidateJobTitlesAsync(cancellationToken);

        var divisionName = department.DivisionId.HasValue
            ? await dbContext.Divisions
                .Where(x => x.Id == department.DivisionId.Value)
                .Select(x => x.Name)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        return new MasterDataCommandResult(MasterDataCommandStatus.Success, Response: ToResponse(entity, department.Name, department.DivisionId, divisionName));
    }

    public async Task<MasterDataCommandResult> DeleteJobTitleAsync(Guid jobTitleId, SoftDeleteRequest request, string actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.JobTitles.FirstOrDefaultAsync(x => x.Id == jobTitleId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.NotFound);
        }

        var before = ToJobTitleAuditState(entity);
        entity.DeletedAt = DateTimeOffset.UtcNow;
        entity.DeletedBy = actor;
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

        return new MasterDataCommandResult(MasterDataCommandStatus.Success);
    }

    public async Task<MasterDataCommandResult> CreateProjectRoleAsync(CreateMasterDataRequest request, CancellationToken cancellationToken)
    {
        var name = NormalizeRequiredName(request.Name);
        if (name is null)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.ValidationError, "Project role name is required.");
        }

        var exists = await dbContext.ProjectRoles.AnyAsync(x => x.Name == name && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.Conflict, "Project role already exists.");
        }

        var entity = new ProjectRoleEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.ProjectRoles.Add(entity);
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "create",
            EntityType: "project_role",
            EntityId: entity.Id.ToString(),
            StatusCode: StatusCodes.Status201Created,
            After: ToProjectRoleAuditState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        await referenceDataCache.InvalidateProjectRolesAsync(cancellationToken);

        return new MasterDataCommandResult(MasterDataCommandStatus.Success, Response: ToResponse(entity));
    }

    public async Task<MasterDataCommandResult> UpdateProjectRoleAsync(Guid projectRoleId, UpdateMasterDataRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ProjectRoles.FirstOrDefaultAsync(x => x.Id == projectRoleId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.NotFound);
        }

        var name = NormalizeRequiredName(request.Name);
        if (name is null)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.ValidationError, "Project role name is required.");
        }

        var exists = await dbContext.ProjectRoles.AnyAsync(x => x.Id != projectRoleId && x.Name == name && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.Conflict, "Project role already exists.");
        }

        var before = ToProjectRoleAuditState(entity);
        entity.Name = name;
        entity.DisplayOrder = request.DisplayOrder;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "update",
            EntityType: "project_role",
            EntityId: entity.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            Before: before,
            After: ToProjectRoleAuditState(entity),
            Changes: new { entity.Name, entity.DisplayOrder, entity.UpdatedAt }));
        await dbContext.SaveChangesAsync(cancellationToken);
        await referenceDataCache.InvalidateProjectRolesAsync(cancellationToken);

        return new MasterDataCommandResult(MasterDataCommandStatus.Success, Response: ToResponse(entity));
    }

    public async Task<MasterDataCommandResult> DeleteProjectRoleAsync(Guid projectRoleId, SoftDeleteRequest request, string actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ProjectRoles.FirstOrDefaultAsync(x => x.Id == projectRoleId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.NotFound);
        }

        var before = ToProjectRoleAuditState(entity);
        entity.DeletedAt = DateTimeOffset.UtcNow;
        entity.DeletedBy = actor;
        entity.DeletedReason = NormalizeDeleteReason(request.Reason);
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "soft_delete",
            EntityType: "project_role",
            EntityId: entity.Id.ToString(),
            StatusCode: StatusCodes.Status204NoContent,
            Reason: entity.DeletedReason,
            Before: before,
            After: ToProjectRoleAuditState(entity),
            Changes: new { entity.DeletedAt, entity.DeletedBy, entity.DeletedReason }));
        await dbContext.SaveChangesAsync(cancellationToken);
        await referenceDataCache.InvalidateProjectRolesAsync(cancellationToken);

        return new MasterDataCommandResult(MasterDataCommandStatus.Success);
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

    private static object ToDivisionAuditState(DivisionEntity entity) => new
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

    private static object ToDepartmentAuditState(DepartmentEntity entity) => new
    {
        entity.Id,
        entity.DivisionId,
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
        entity.DepartmentId,
        entity.Name,
        entity.DisplayOrder,
        entity.CreatedAt,
        entity.UpdatedAt,
        entity.DeletedReason,
        entity.DeletedBy,
        entity.DeletedAt
    };

    private static object ToProjectRoleAuditState(ProjectRoleEntity entity) => new
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

    private static MasterDataResponse ToResponse(DivisionEntity entity) =>
        new(entity.Id, entity.Name, entity.DisplayOrder, null, null, null, null, entity.CreatedAt, entity.UpdatedAt, entity.DeletedReason, entity.DeletedBy, entity.DeletedAt);

    private static MasterDataResponse ToResponse(DepartmentEntity entity, string? divisionName) =>
        new(entity.Id, entity.Name, entity.DisplayOrder, entity.DivisionId, divisionName, null, null, entity.CreatedAt, entity.UpdatedAt, entity.DeletedReason, entity.DeletedBy, entity.DeletedAt);

    private static MasterDataResponse ToResponse(JobTitleEntity entity, string? departmentName, Guid? divisionId, string? divisionName) =>
        new(entity.Id, entity.Name, entity.DisplayOrder, divisionId, divisionName, entity.DepartmentId, departmentName, entity.CreatedAt, entity.UpdatedAt, entity.DeletedReason, entity.DeletedBy, entity.DeletedAt);

    private static MasterDataResponse ToResponse(ProjectRoleEntity entity) =>
        new(entity.Id, entity.Name, entity.DisplayOrder, null, null, null, null, entity.CreatedAt, entity.UpdatedAt, entity.DeletedReason, entity.DeletedBy, entity.DeletedAt);
}
