using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Audits.Application;
using Operis_API.Modules.Workflows.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;
using System.Text;

namespace Operis_API.Modules.Workflows;

public sealed class WorkflowCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IBusinessAuditEventWriter businessAuditEventWriter) : IWorkflowCommands
{
    private static readonly HashSet<string> AllowedStepTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "submit",
        "peer_review",
        "review",
        "approve"
    };

    public async Task<WorkflowCommandResult> CreateDefinitionAsync(CreateWorkflowDefinitionRequest request, CancellationToken cancellationToken)
    {
        var name = NormalizeName(request.Name);
        if (name is null)
        {
            return new WorkflowCommandResult(WorkflowCommandStatus.ValidationError, "Workflow definition name is required.", ApiErrorCodes.WorkflowDefinitionNameRequired);
        }

        var stepValidation = await ValidateStepsAsync(request.Steps, cancellationToken);
        if (stepValidation is not null)
        {
            return stepValidation;
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

        var steps = request.Steps
            .OrderBy(step => step.DisplayOrder)
            .Select(step => new WorkflowStepEntity
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = entity.Id,
                Name = step.Name.Trim(),
                StepType = step.StepType.Trim().ToLowerInvariant(),
                DisplayOrder = step.DisplayOrder,
                IsRequired = step.IsRequired,
                CreatedAt = DateTimeOffset.UtcNow
            })
            .ToList();

        dbContext.WorkflowSteps.AddRange(steps);

        var stepRoles = steps
            .Join(request.Steps, entityStep => entityStep.DisplayOrder, requestStep => requestStep.DisplayOrder,
                (entityStep, requestStep) => new { entityStep, requestStep })
            .SelectMany(pair => pair.requestStep.RoleIds.Distinct().Select(roleId => new WorkflowStepRoleEntity
            {
                Id = Guid.NewGuid(),
                WorkflowStepId = pair.entityStep.Id,
                ProjectRoleId = roleId,
                CreatedAt = DateTimeOffset.UtcNow
            }))
            .ToList();

        dbContext.WorkflowStepRoles.AddRange(stepRoles);
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
                entity.CreatedAt,
                Steps = steps.Select(step => new
                {
                    step.Id,
                    step.Name,
                    step.StepType,
                    step.DisplayOrder,
                    step.IsRequired
                })
            }));
        await dbContext.SaveChangesAsync(cancellationToken);
        await TryAppendBusinessEventAsync(
            "workflows",
            "workflow.definition.created",
            "workflow_definition",
            entity.Id.ToString(),
            "Created workflow definition",
            null,
            new { entity.Code, entity.Name, entity.Status },
            cancellationToken);

        return new WorkflowCommandResult(
            WorkflowCommandStatus.Success,
            Response: BuildDetailContract(entity, steps, stepRoles));
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

        var stepValidation = await ValidateStepsAsync(request.Steps, cancellationToken);
        if (stepValidation is not null)
        {
            return stepValidation;
        }

        var uniqueness = await ValidateUniqueCodeAsync(name, workflowDefinitionId, cancellationToken);
        if (uniqueness is not null)
        {
            return uniqueness;
        }

        var before = await LoadDetailContractAsync(entity.Id, cancellationToken);
        entity.Name = name;
        entity.Code = ToCode(name)!;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        var existingSteps = await dbContext.WorkflowSteps
            .Where(x => x.WorkflowDefinitionId == entity.Id)
            .ToListAsync(cancellationToken);

        if (existingSteps.Count > 0)
        {
            var stepIds = existingSteps.Select(x => x.Id).ToList();
            var existingRoles = await dbContext.WorkflowStepRoles
                .Where(x => stepIds.Contains(x.WorkflowStepId))
                .ToListAsync(cancellationToken);
            dbContext.WorkflowStepRoles.RemoveRange(existingRoles);
            dbContext.WorkflowSteps.RemoveRange(existingSteps);
        }

        var steps = request.Steps
            .OrderBy(step => step.DisplayOrder)
            .Select(step => new WorkflowStepEntity
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = entity.Id,
                Name = step.Name.Trim(),
                StepType = step.StepType.Trim().ToLowerInvariant(),
                DisplayOrder = step.DisplayOrder,
                IsRequired = step.IsRequired,
                CreatedAt = DateTimeOffset.UtcNow
            })
            .ToList();

        dbContext.WorkflowSteps.AddRange(steps);

        var stepRoles = steps
            .Join(request.Steps, entityStep => entityStep.DisplayOrder, requestStep => requestStep.DisplayOrder,
                (entityStep, requestStep) => new { entityStep, requestStep })
            .SelectMany(pair => pair.requestStep.RoleIds.Distinct().Select(roleId => new WorkflowStepRoleEntity
            {
                Id = Guid.NewGuid(),
                WorkflowStepId = pair.entityStep.Id,
                ProjectRoleId = roleId,
                CreatedAt = DateTimeOffset.UtcNow
            }))
            .ToList();

        dbContext.WorkflowStepRoles.AddRange(stepRoles);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "workflows",
            Action: "update",
            EntityType: "workflow_definition",
            EntityId: entity.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            Before: before,
            After: BuildDetailContract(entity, steps, stepRoles),
            Changes: new
            {
                entity.Name,
                entity.Code,
                entity.UpdatedAt,
                Steps = steps.Select(step => new
                {
                    step.Id,
                    step.Name,
                    step.StepType,
                    step.DisplayOrder,
                    step.IsRequired
                })
            }));
        await dbContext.SaveChangesAsync(cancellationToken);
        await TryAppendBusinessEventAsync(
            "workflows",
            "workflow.definition.updated",
            "workflow_definition",
            entity.Id.ToString(),
            "Updated workflow definition",
            null,
            new { before, after = BuildDetailContract(entity, steps, stepRoles) },
            cancellationToken);

        return new WorkflowCommandResult(
            WorkflowCommandStatus.Success,
            Response: BuildDetailContract(entity, steps, stepRoles));
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

    private static WorkflowDefinitionDetailContract BuildDetailContract(
        WorkflowDefinitionEntity entity,
        IReadOnlyList<WorkflowStepEntity> steps,
        IReadOnlyList<WorkflowStepRoleEntity> stepRoles)
    {
        var roleLookup = stepRoles
            .GroupBy(role => role.WorkflowStepId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<Guid>)group.Select(x => x.ProjectRoleId).ToList());

        var stepContracts = steps
            .OrderBy(step => step.DisplayOrder)
            .Select(step => new WorkflowStepContract(
                step.Id,
                step.Name,
                step.StepType,
                step.DisplayOrder,
                step.IsRequired,
                roleLookup.TryGetValue(step.Id, out var roles) ? roles : []))
            .ToList();

        return new WorkflowDefinitionDetailContract(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.Status,
            stepContracts);
    }

    private async Task<WorkflowDefinitionDetailContract?> LoadDetailContractAsync(Guid workflowDefinitionId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.WorkflowDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == workflowDefinitionId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var steps = await dbContext.WorkflowSteps
            .AsNoTracking()
            .Where(x => x.WorkflowDefinitionId == workflowDefinitionId)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);

        if (steps.Count == 0)
        {
            return new WorkflowDefinitionDetailContract(entity.Id, entity.Code, entity.Name, entity.Status, []);
        }

        var stepIds = steps.Select(x => x.Id).ToList();
        var stepRoles = await dbContext.WorkflowStepRoles
            .AsNoTracking()
            .Where(x => stepIds.Contains(x.WorkflowStepId))
            .ToListAsync(cancellationToken);

        return BuildDetailContract(entity, steps, stepRoles);
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
        var before = await LoadDetailContractAsync(entity.Id, cancellationToken);
        var steps = await dbContext.WorkflowSteps
            .AsNoTracking()
            .Where(x => x.WorkflowDefinitionId == entity.Id)
            .ToListAsync(cancellationToken);
        var stepIds = steps.Select(x => x.Id).ToList();
        var stepRoles = stepIds.Count == 0
            ? []
            : await dbContext.WorkflowStepRoles
                .AsNoTracking()
                .Where(x => stepIds.Contains(x.WorkflowStepId))
                .ToListAsync(cancellationToken);
        entity.Status = status;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        var after = BuildDetailContract(entity, steps, stepRoles);
        auditLogWriter.Append(new AuditLogEntry(
            Module: "workflows",
            Action: action,
            EntityType: "workflow_definition",
            EntityId: entity.Id.ToString(),
            StatusCode: statusCode,
            Before: before,
            After: after,
            Changes: new
            {
                entity.Status,
                entity.UpdatedAt
            }));
        await dbContext.SaveChangesAsync(cancellationToken);
        await TryAppendBusinessEventAsync(
            "workflows",
            $"workflow.definition.{action}d",
            "workflow_definition",
            entity.Id.ToString(),
            action == "activate" ? "Activated workflow definition" : "Archived workflow definition",
            null,
            new { before, after },
            cancellationToken);

        return new WorkflowCommandResult(
            WorkflowCommandStatus.Success,
            Response: after);
    }

    private async Task<WorkflowCommandResult?> ValidateStepsAsync(
        IReadOnlyList<WorkflowStepRequest> steps,
        CancellationToken cancellationToken)
    {
        if (steps is null || steps.Count == 0)
        {
            return new WorkflowCommandResult(WorkflowCommandStatus.ValidationError, "Workflow steps are required.", ApiErrorCodes.RequestValidationFailed);
        }

        var displayOrders = new HashSet<int>();
        var roleIds = new HashSet<Guid>();

        foreach (var step in steps)
        {
            if (string.IsNullOrWhiteSpace(step.Name))
            {
                return new WorkflowCommandResult(WorkflowCommandStatus.ValidationError, "Workflow step name is required.", ApiErrorCodes.RequestValidationFailed);
            }

            if (string.IsNullOrWhiteSpace(step.StepType) || !AllowedStepTypes.Contains(step.StepType.Trim()))
            {
                return new WorkflowCommandResult(WorkflowCommandStatus.ValidationError, $"Invalid workflow step type: {step.StepType}", ApiErrorCodes.RequestValidationFailed);
            }

            if (step.DisplayOrder <= 0 || !displayOrders.Add(step.DisplayOrder))
            {
                return new WorkflowCommandResult(WorkflowCommandStatus.ValidationError, "Workflow step display order must be unique and positive.", ApiErrorCodes.RequestValidationFailed);
            }

            if (step.RoleIds is null || step.RoleIds.Count == 0)
            {
                return new WorkflowCommandResult(WorkflowCommandStatus.ValidationError, "Workflow step roles are required.", ApiErrorCodes.RequestValidationFailed);
            }

            foreach (var roleId in step.RoleIds)
            {
                roleIds.Add(roleId);
            }
        }

        if (roleIds.Count > 0)
        {
            var existingRoles = await dbContext.ProjectRoles
                .AsNoTracking()
                .Where(x => roleIds.Contains(x.Id) && x.DeletedAt == null)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            if (existingRoles.Count != roleIds.Count)
            {
                return new WorkflowCommandResult(WorkflowCommandStatus.ValidationError, "Some workflow step roles do not exist.", ApiErrorCodes.RequestValidationFailed);
            }
        }

        return null;
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
}
