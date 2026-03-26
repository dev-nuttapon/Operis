using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Users.Application;

public sealed class MasterDataCatalogCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IMasterDataCatalogQueries queries) : IMasterDataCatalogCommands
{
    public async Task<(bool Success, string? Error, string? ErrorCode, MasterDataCatalogResponse? Response)> CreateAsync(CreateMasterDataCatalogRequest request, string actorUserId, CancellationToken cancellationToken)
    {
        var domain = NormalizeRequired(request.Domain, 128);
        var code = NormalizeRequired(request.Code, 128);
        var name = NormalizeRequired(request.Name, 256);
        var reason = NormalizeRequired(request.Reason, 2000);
        if (domain is null || code is null || name is null || reason is null)
        {
            return (false, "Domain, code, name, and reason are required.", ApiErrorCodes.RequestValidationFailed, null);
        }

        var exists = await dbContext.MasterDataItems.AnyAsync(x => x.Domain == domain && x.Code == code, cancellationToken);
        if (exists)
        {
            return (false, "Master data code already exists in this domain.", ApiErrorCodes.MasterDataCodeDuplicate, null);
        }

        var entity = new MasterDataItemEntity
        {
            Id = Guid.NewGuid(),
            Domain = domain,
            Code = code,
            Name = name,
            Status = "Active",
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.MasterDataItems.Add(entity);
        dbContext.MasterDataChanges.Add(new MasterDataChangeEntity
        {
            Id = Guid.NewGuid(),
            MasterDataItemId = entity.Id,
            ChangeType = "created",
            ChangedBy = actorUserId,
            ChangedAt = entity.UpdatedAt,
            Reason = reason
        });
        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "create", EntityType: "master_data_item", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status201Created, Reason: reason, After: ToState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        return (true, null, null, await queries.GetAsync(entity.Id, cancellationToken));
    }

    public async Task<(bool Success, string? Error, string? ErrorCode, MasterDataCatalogResponse? Response, bool NotFound)> UpdateAsync(Guid id, UpdateMasterDataCatalogRequest request, string actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.MasterDataItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return (false, null, null, null, true);
        }

        var domain = NormalizeRequired(request.Domain, 128);
        var code = NormalizeRequired(request.Code, 128);
        var name = NormalizeRequired(request.Name, 256);
        var reason = NormalizeRequired(request.Reason, 2000);
        var status = NormalizeStatus(request.Status);
        if (domain is null || code is null || name is null || reason is null)
        {
            return (false, "Domain, code, name, and reason are required.", ApiErrorCodes.RequestValidationFailed, null, false);
        }

        var duplicate = await dbContext.MasterDataItems.AnyAsync(x => x.Id != id && x.Domain == domain && x.Code == code, cancellationToken);
        if (duplicate)
        {
            return (false, "Master data code already exists in this domain.", ApiErrorCodes.MasterDataCodeDuplicate, null, false);
        }

        var before = ToState(entity);
        entity.Domain = domain;
        entity.Code = code;
        entity.Name = name;
        entity.Status = status;
        entity.DisplayOrder = request.DisplayOrder;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.MasterDataChanges.Add(new MasterDataChangeEntity
        {
            Id = Guid.NewGuid(),
            MasterDataItemId = entity.Id,
            ChangeType = "updated",
            ChangedBy = actorUserId,
            ChangedAt = entity.UpdatedAt,
            Reason = reason
        });
        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "update", EntityType: "master_data_item", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status200OK, Reason: reason, Before: before, After: ToState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        return (true, null, null, await queries.GetAsync(entity.Id, cancellationToken), false);
    }

    public async Task<(bool Success, string? Error, string? ErrorCode, MasterDataCatalogResponse? Response, bool NotFound)> ArchiveAsync(Guid id, SoftDeleteRequest request, string actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.MasterDataItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return (false, null, null, null, true);
        }

        if (entity.Status == "Archived")
        {
            return (false, "Master data item is already archived.", ApiErrorCodes.InvalidWorkflowTransition, null, false);
        }

        var reason = NormalizeRequired(request.Reason, 2000);
        if (reason is null)
        {
            return (false, "Archive reason is required.", ApiErrorCodes.ReasonRequired, null, false);
        }

        if (await HasActiveReferencesAsync(entity, cancellationToken))
        {
            return (false, "Master data item is still referenced by active records.", ApiErrorCodes.MasterDataInUse, null, false);
        }

        var before = ToState(entity);
        entity.Status = "Archived";
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.MasterDataChanges.Add(new MasterDataChangeEntity
        {
            Id = Guid.NewGuid(),
            MasterDataItemId = entity.Id,
            ChangeType = "archived",
            ChangedBy = actorUserId,
            ChangedAt = entity.UpdatedAt,
            Reason = reason
        });
        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "archive", EntityType: "master_data_item", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status200OK, Reason: reason, Before: before, After: ToState(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        return (true, null, null, await queries.GetAsync(entity.Id, cancellationToken), false);
    }

    private async Task<bool> HasActiveReferencesAsync(MasterDataItemEntity entity, CancellationToken cancellationToken)
    {
        return entity.Domain switch
        {
            "project_type" => await dbContext.Projects.AnyAsync(x => x.ProjectType == entity.Code && x.DeletedAt == null, cancellationToken),
            "methodology" => await dbContext.Projects.AnyAsync(x => x.Methodology == entity.Code && x.DeletedAt == null, cancellationToken),
            "phase_code" => await dbContext.Projects.AnyAsync(x => x.Phase == entity.Code && x.DeletedAt == null, cancellationToken)
                || await dbContext.PhaseApprovalRequests.AnyAsync(x => x.PhaseCode == entity.Code && x.Status != "Archived", cancellationToken),
            _ => false
        };
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

    private static string NormalizeStatus(string? value)
    {
        return string.Equals(value?.Trim(), "Archived", StringComparison.OrdinalIgnoreCase) ? "Archived" : "Active";
    }

    private static object ToState(MasterDataItemEntity entity) => new
    {
        entity.Id,
        entity.Domain,
        entity.Code,
        entity.Name,
        entity.Status,
        entity.DisplayOrder,
        entity.CreatedAt,
        entity.UpdatedAt
    };
}
