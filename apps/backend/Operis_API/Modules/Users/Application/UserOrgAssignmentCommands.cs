using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Users.Application;

public sealed class UserOrgAssignmentCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter) : IUserOrgAssignmentCommands
{
    public async Task<UserCommandResult> UpsertPrimaryAssignmentAsync(string userId, UpsertUserOrgAssignmentRequest request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId && x.DeletedAt == null, cancellationToken);
        if (user is null)
        {
            return new UserCommandResult(UserCommandStatus.NotFound);
        }

        var departmentValidation = await ValidateDepartmentSelectionAsync(request.DivisionId, request.DepartmentId, cancellationToken);
        if (!departmentValidation.Success)
        {
            return new UserCommandResult(UserCommandStatus.ValidationError, departmentValidation.ErrorMessage, ApiErrorCodeResolver.Resolve(departmentValidation.ErrorMessage, ApiErrorCodes.RequestValidationFailed));
        }

        var positionValidation = await ValidatePositionSelectionAsync(request.DepartmentId, request.PositionId, cancellationToken);
        if (!positionValidation.Success)
        {
            return new UserCommandResult(UserCommandStatus.ValidationError, positionValidation.ErrorMessage, ApiErrorCodeResolver.Resolve(positionValidation.ErrorMessage, ApiErrorCodes.RequestValidationFailed));
        }

        var existing = await dbContext.UserOrgAssignments
            .FirstOrDefaultAsync(x => x.UserId == userId && x.IsPrimary, cancellationToken);

        var before = existing is null
            ? null
            : new
            {
                existing.DivisionId,
                existing.DepartmentId,
                existing.PositionId,
                existing.IsPrimary,
                existing.StartAt,
                existing.EndAt
            };

        if (existing is null)
        {
            existing = new UserOrgAssignmentEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                IsPrimary = true,
                StartAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow
            };
            dbContext.UserOrgAssignments.Add(existing);
        }
        else
        {
            existing.UpdatedAt = DateTimeOffset.UtcNow;
        }

        existing.DivisionId = request.DivisionId;
        existing.DepartmentId = request.DepartmentId;
        existing.PositionId = request.PositionId;

        user.DepartmentId = request.DepartmentId;
        user.JobTitleId = request.PositionId;

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "assign_org",
            EntityType: "user_org_assignment",
            EntityId: existing.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            ActorUserId: userId,
            DepartmentId: existing.DepartmentId,
            Before: before,
            After: new
            {
                existing.DivisionId,
                existing.DepartmentId,
                existing.PositionId,
                existing.IsPrimary,
                existing.StartAt,
                existing.EndAt
            },
            Changes: new
            {
                existing.DivisionId,
                existing.DepartmentId,
                existing.PositionId
            }));

        await dbContext.SaveChangesAsync(cancellationToken);
        return new UserCommandResult(UserCommandStatus.Success);
    }

    private async Task<DepartmentValidationResult> ValidateDepartmentSelectionAsync(Guid? divisionId, Guid? departmentId, CancellationToken cancellationToken)
    {
        if (!departmentId.HasValue)
        {
            return !divisionId.HasValue
                ? DepartmentValidationResult.Valid()
                : DepartmentValidationResult.Invalid("Department is required when division is selected.");
        }

        var department = await dbContext.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == departmentId.Value && x.DeletedAt == null, cancellationToken);
        if (department is null)
        {
            return DepartmentValidationResult.Invalid("Department does not exist.");
        }

        if (divisionId.HasValue && department.DivisionId != divisionId)
        {
            return DepartmentValidationResult.Invalid("Department does not belong to the selected division.");
        }

        return DepartmentValidationResult.Valid();
    }

    private async Task<PositionValidationResult> ValidatePositionSelectionAsync(Guid? departmentId, Guid? positionId, CancellationToken cancellationToken)
    {
        if (!positionId.HasValue)
        {
            return PositionValidationResult.Valid();
        }

        if (!departmentId.HasValue)
        {
            return PositionValidationResult.Invalid("Department is required when job title is selected.");
        }

        var position = await dbContext.JobTitles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == positionId.Value && x.DeletedAt == null, cancellationToken);
        if (position is null)
        {
            return PositionValidationResult.Invalid("Job title does not exist.");
        }

        if (position.DepartmentId != departmentId)
        {
            return PositionValidationResult.Invalid("Job title does not belong to the selected department.");
        }

        return PositionValidationResult.Valid();
    }

    private sealed record DepartmentValidationResult(bool Success, string? ErrorMessage)
    {
        public static DepartmentValidationResult Valid() => new(true, null);
        public static DepartmentValidationResult Invalid(string errorMessage) => new(false, errorMessage);
    }

    private sealed record PositionValidationResult(bool Success, string? ErrorMessage)
    {
        public static PositionValidationResult Valid() => new(true, null);
        public static PositionValidationResult Invalid(string errorMessage) => new(false, errorMessage);
    }
}
