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
    public async Task<MasterDataCommandResult> CreateDepartmentAsync(CreateMasterDataRequest request, CancellationToken cancellationToken)
    {
        var name = NormalizeRequiredName(request.Name);
        if (name is null)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.ValidationError, "Department name is required.");
        }

        var exists = await dbContext.Departments.AnyAsync(x => x.Name == name && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.Conflict, "Department already exists.");
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

        return new MasterDataCommandResult(MasterDataCommandStatus.Success, Response: ToResponse(entity));
    }

    public async Task<MasterDataCommandResult> UpdateDepartmentAsync(Guid departmentId, UpdateMasterDataRequest request, CancellationToken cancellationToken)
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

        var exists = await dbContext.Departments.AnyAsync(x => x.Id != departmentId && x.Name == name && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.Conflict, "Department already exists.");
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

        return new MasterDataCommandResult(MasterDataCommandStatus.Success, Response: ToResponse(entity));
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

    public async Task<MasterDataCommandResult> CreateJobTitleAsync(CreateMasterDataRequest request, CancellationToken cancellationToken)
    {
        var name = NormalizeRequiredName(request.Name);
        if (name is null)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.ValidationError, "Job title name is required.");
        }

        var exists = await dbContext.JobTitles.AnyAsync(x => x.Name == name && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.Conflict, "Job title already exists.");
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

        return new MasterDataCommandResult(MasterDataCommandStatus.Success, Response: ToResponse(entity));
    }

    public async Task<MasterDataCommandResult> UpdateJobTitleAsync(Guid jobTitleId, UpdateMasterDataRequest request, CancellationToken cancellationToken)
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

        var exists = await dbContext.JobTitles.AnyAsync(x => x.Id != jobTitleId && x.Name == name && x.DeletedAt == null, cancellationToken);
        if (exists)
        {
            return new MasterDataCommandResult(MasterDataCommandStatus.Conflict, "Job title already exists.");
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

        return new MasterDataCommandResult(MasterDataCommandStatus.Success, Response: ToResponse(entity));
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
