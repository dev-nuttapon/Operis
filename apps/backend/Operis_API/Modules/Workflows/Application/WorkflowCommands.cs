using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Workflows.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;
using System.Text;

namespace Operis_API.Modules.Workflows;

public sealed class WorkflowCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter) : IWorkflowCommands
{
    public async Task<WorkflowCommandResult> CreateDefinitionAsync(CreateWorkflowDefinitionRequest request, CancellationToken cancellationToken)
    {
        var name = NormalizeName(request.Name);
        if (name is null)
        {
            return new WorkflowCommandResult(WorkflowCommandStatus.ValidationError, "Workflow definition name is required.", ApiErrorCodes.WorkflowDefinitionNameRequired);
        }

        var uniqueness = await ValidateUniqueCodeAsync(name, null, cancellationToken);
        if (uniqueness is not null)
        {
            return uniqueness;
        }

        var code = ToCode(name)!;

        var entity = new WorkflowDefinitionEntity
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Status = "draft",
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.WorkflowDefinitions.Add(entity);
        auditLogWriter.Append(new AuditLogEntry(
            Module: "workflows",
            Action: "create",
            EntityType: "workflow_definition",
            EntityId: entity.Id.ToString(),
            StatusCode: StatusCodes.Status201Created,
            After: new
            {
                entity.Id,
                entity.Code,
                entity.Name,
                entity.Status,
                entity.CreatedAt
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new WorkflowCommandResult(
            WorkflowCommandStatus.Success,
            Response: ToContract(entity));
    }

    public async Task<WorkflowCommandResult> UpdateDefinitionAsync(Guid workflowDefinitionId, UpdateWorkflowDefinitionRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.WorkflowDefinitions.FirstOrDefaultAsync(x => x.Id == workflowDefinitionId, cancellationToken);
        if (entity is null)
        {
            return new WorkflowCommandResult(WorkflowCommandStatus.ValidationError, "Workflow definition does not exist.", ApiErrorCodes.WorkflowDefinitionNotFound);
        }

        var name = NormalizeName(request.Name);
        if (name is null)
        {
            return new WorkflowCommandResult(WorkflowCommandStatus.ValidationError, "Workflow definition name is required.", ApiErrorCodes.WorkflowDefinitionNameRequired);
        }

        var uniqueness = await ValidateUniqueCodeAsync(name, workflowDefinitionId, cancellationToken);
        if (uniqueness is not null)
        {
            return uniqueness;
        }

        var before = ToContract(entity);
        entity.Name = name;
        entity.Code = ToCode(name)!;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        auditLogWriter.Append(new AuditLogEntry(
            Module: "workflows",
            Action: "update",
            EntityType: "workflow_definition",
            EntityId: entity.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            Before: before,
            After: ToContract(entity),
            Changes: new
            {
                entity.Name,
                entity.Code,
                entity.UpdatedAt
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new WorkflowCommandResult(
            WorkflowCommandStatus.Success,
            Response: ToContract(entity));
    }

    public async Task<WorkflowCommandResult> ActivateDefinitionAsync(Guid workflowDefinitionId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.WorkflowDefinitions.FirstOrDefaultAsync(x => x.Id == workflowDefinitionId, cancellationToken);
        if (entity is null)
        {
            return new WorkflowCommandResult(WorkflowCommandStatus.ValidationError, "Workflow definition does not exist.", ApiErrorCodes.WorkflowDefinitionNotFound);
        }

        if (entity.Status == "active")
        {
            return new WorkflowCommandResult(WorkflowCommandStatus.Conflict, "Workflow definition is already active.", ApiErrorCodes.WorkflowDefinitionAlreadyActive);
        }

        return await UpdateStatusAsync(entity, "active", "activate", StatusCodes.Status200OK, cancellationToken);
    }

    public async Task<WorkflowCommandResult> ArchiveDefinitionAsync(Guid workflowDefinitionId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.WorkflowDefinitions.FirstOrDefaultAsync(x => x.Id == workflowDefinitionId, cancellationToken);
        if (entity is null)
        {
            return new WorkflowCommandResult(WorkflowCommandStatus.ValidationError, "Workflow definition does not exist.", ApiErrorCodes.WorkflowDefinitionNotFound);
        }

        if (entity.Status == "archived")
        {
            return new WorkflowCommandResult(WorkflowCommandStatus.Conflict, "Workflow definition is already archived.", ApiErrorCodes.WorkflowDefinitionAlreadyArchived);
        }

        return await UpdateStatusAsync(entity, "archived", "archive", StatusCodes.Status200OK, cancellationToken);
    }

    private static string? NormalizeName(string? input)
    {
        return string.IsNullOrWhiteSpace(input) ? null : input.Trim();
    }

    private static string ToCode(string name)
    {
        var builder = new StringBuilder();
        var previousWasDash = false;

        foreach (var character in name.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousWasDash = false;
                continue;
            }

            if (previousWasDash || builder.Length == 0)
            {
                continue;
            }

            builder.Append('-');
            previousWasDash = true;
        }

        return builder.ToString().Trim('-');
    }

    private static WorkflowDefinitionContract ToContract(WorkflowDefinitionEntity entity)
    {
        return new WorkflowDefinitionContract(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.Status);
    }

    private async Task<WorkflowCommandResult?> ValidateUniqueCodeAsync(
        string name,
        Guid? currentWorkflowDefinitionId,
        CancellationToken cancellationToken)
    {
        var code = ToCode(name);
        if (string.IsNullOrWhiteSpace(code))
        {
            return new WorkflowCommandResult(WorkflowCommandStatus.ValidationError, "Workflow definition name is required.", ApiErrorCodes.WorkflowDefinitionNameRequired);
        }

        var exists = await dbContext.WorkflowDefinitions
            .AnyAsync(x => x.Code == code && x.Id != currentWorkflowDefinitionId, cancellationToken);
        if (exists)
        {
            return new WorkflowCommandResult(WorkflowCommandStatus.Conflict, "Workflow definition already exists.", ApiErrorCodes.WorkflowDefinitionAlreadyExists);
        }

        return null;
    }

    private async Task<WorkflowCommandResult> UpdateStatusAsync(
        WorkflowDefinitionEntity entity,
        string status,
        string action,
        int statusCode,
        CancellationToken cancellationToken)
    {
        var before = ToContract(entity);
        entity.Status = status;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        auditLogWriter.Append(new AuditLogEntry(
            Module: "workflows",
            Action: action,
            EntityType: "workflow_definition",
            EntityId: entity.Id.ToString(),
            StatusCode: statusCode,
            Before: before,
            After: ToContract(entity),
            Changes: new
            {
                entity.Status,
                entity.UpdatedAt
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new WorkflowCommandResult(
            WorkflowCommandStatus.Success,
            Response: ToContract(entity));
    }
}
