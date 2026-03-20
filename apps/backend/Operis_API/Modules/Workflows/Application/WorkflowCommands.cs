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

        if (request.DocumentTemplateId.HasValue)
        {
            var templateExists = await dbContext.DocumentTemplates
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.DocumentTemplateId.Value && !x.IsDeleted, cancellationToken);
            if (!templateExists)
            {
                return new WorkflowCommandResult(WorkflowCommandStatus.ValidationError, "Document template does not exist.", ApiErrorCodes.RequestValidationFailed);
            }
        }

        var stepValidation = await ValidateStepsAsync(request.Steps, request.DocumentTemplateId, cancellationToken);
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
            DocumentTemplateId = request.DocumentTemplateId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.WorkflowDefinitions.Add(entity);

        var steps = request.Steps
            .OrderBy(step => step.DisplayOrder)
            .Select(step => new WorkflowStepEntity
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = entity.Id,
                DocumentId = step.DocumentId,
                Name = step.Name.Trim(),
                StepType = step.StepType.Trim().ToLowerInvariant(),
                DisplayOrder = step.DisplayOrder,
                IsRequired = step.IsRequired,
                MinApprovals = NormalizeMinApprovals(step.StepType, step.MinApprovals),
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

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.WorkflowStepRoles.AddRange(stepRoles);
        var stepRoutes = BuildStepRoutes(request.Steps, steps);
        dbContext.WorkflowStepRoutes.AddRange(stepRoutes);
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
        await transaction.CommitAsync(cancellationToken);
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
            Response: BuildDetailContract(entity, steps, stepRoles, stepRoutes));
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

        if (request.DocumentTemplateId.HasValue)
        {
            var templateExists = await dbContext.DocumentTemplates
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.DocumentTemplateId.Value && !x.IsDeleted, cancellationToken);
            if (!templateExists)
            {
                return new WorkflowCommandResult(WorkflowCommandStatus.ValidationError, "Document template does not exist.", ApiErrorCodes.RequestValidationFailed);
            }
        }

        var stepValidation = await ValidateStepsAsync(request.Steps, request.DocumentTemplateId, cancellationToken);
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
        entity.DocumentTemplateId = request.DocumentTemplateId;
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
            var existingRoutes = await dbContext.WorkflowStepRoutes
                .Where(x => stepIds.Contains(x.WorkflowStepId))
                .ToListAsync(cancellationToken);
            dbContext.WorkflowStepRoles.RemoveRange(existingRoles);
            dbContext.WorkflowStepRoutes.RemoveRange(existingRoutes);
            dbContext.WorkflowSteps.RemoveRange(existingSteps);
        }

        var steps = request.Steps
            .OrderBy(step => step.DisplayOrder)
            .Select(step => new WorkflowStepEntity
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = entity.Id,
                DocumentId = step.DocumentId,
                Name = step.Name.Trim(),
                StepType = step.StepType.Trim().ToLowerInvariant(),
                DisplayOrder = step.DisplayOrder,
                IsRequired = step.IsRequired,
                MinApprovals = NormalizeMinApprovals(step.StepType, step.MinApprovals),
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

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.WorkflowStepRoles.AddRange(stepRoles);
        var stepRoutes = BuildStepRoutes(request.Steps, steps);
        dbContext.WorkflowStepRoutes.AddRange(stepRoutes);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "workflows",
            Action: "update",
            EntityType: "workflow_definition",
            EntityId: entity.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            Before: before,
            After: BuildDetailContract(entity, steps, stepRoles, stepRoutes),
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
        await transaction.CommitAsync(cancellationToken);
        await TryAppendBusinessEventAsync(
            "workflows",
            "workflow.definition.updated",
            "workflow_definition",
            entity.Id.ToString(),
            "Updated workflow definition",
            null,
            new { before, after = BuildDetailContract(entity, steps, stepRoles, stepRoutes) },
            cancellationToken);

        return new WorkflowCommandResult(
            WorkflowCommandStatus.Success,
            Response: BuildDetailContract(entity, steps, stepRoles, stepRoutes));
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
            entity.Status,
            entity.DocumentTemplateId);
    }

    private static WorkflowDefinitionDetailContract BuildDetailContract(
        WorkflowDefinitionEntity entity,
        IReadOnlyList<WorkflowStepEntity> steps,
        IReadOnlyList<WorkflowStepRoleEntity> stepRoles,
        IReadOnlyList<WorkflowStepRouteEntity> stepRoutes)
    {
        var roleLookup = stepRoles
            .GroupBy(role => role.WorkflowStepId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<Guid>)group.Select(x => x.ProjectRoleId).ToList());

        var routeLookup = stepRoutes
            .GroupBy(route => route.WorkflowStepId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<WorkflowStepRouteEntity>)group.ToList());

        var stepContracts = steps
            .OrderBy(step => step.DisplayOrder)
            .Select(step => new WorkflowStepContract(
                step.Id,
                step.Name,
                step.StepType,
                step.DisplayOrder,
                step.IsRequired,
                step.DocumentId,
                step.MinApprovals,
                roleLookup.TryGetValue(step.Id, out var roles) ? roles : [],
                routeLookup.TryGetValue(step.Id, out var routes)
                    ? routes.Select(route => new WorkflowStepRouteContract(
                        route.Action,
                        route.NextStepId,
                        steps.FirstOrDefault(x => x.Id == route.NextStepId)?.DisplayOrder)).ToList()
                    : []))
            .ToList();

        return new WorkflowDefinitionDetailContract(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.Status,
            entity.DocumentTemplateId,
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
            return new WorkflowDefinitionDetailContract(entity.Id, entity.Code, entity.Name, entity.Status, entity.DocumentTemplateId, []);
        }

        var stepIds = steps.Select(x => x.Id).ToList();
        var stepRoles = await dbContext.WorkflowStepRoles
            .AsNoTracking()
            .Where(x => stepIds.Contains(x.WorkflowStepId))
            .ToListAsync(cancellationToken);

        var stepRoutes = await dbContext.WorkflowStepRoutes
            .AsNoTracking()
            .Where(x => stepIds.Contains(x.WorkflowStepId))
            .ToListAsync(cancellationToken);

        return BuildDetailContract(entity, steps, stepRoles, stepRoutes);
    }

    private static IReadOnlyList<WorkflowStepRouteEntity> BuildStepRoutes(
        IReadOnlyList<WorkflowStepRequest> requestSteps,
        IReadOnlyList<WorkflowStepEntity> stepEntities)
    {
        if (requestSteps.Count == 0 || stepEntities.Count == 0)
        {
            return [];
        }

        var stepIdByOrder = stepEntities.ToDictionary(step => step.DisplayOrder, step => step.Id);
        var routes = new List<WorkflowStepRouteEntity>();

        foreach (var requestStep in requestSteps)
        {
            if (requestStep.Routes is null || requestStep.Routes.Count == 0)
            {
                continue;
            }

            if (!stepIdByOrder.TryGetValue(requestStep.DisplayOrder, out var stepId))
            {
                continue;
            }

            foreach (var route in requestStep.Routes)
            {
                if (string.IsNullOrWhiteSpace(route.Action))
                {
                    continue;
                }

                routes.Add(new WorkflowStepRouteEntity
                {
                    Id = Guid.NewGuid(),
                    WorkflowStepId = stepId,
                    Action = route.Action.Trim().ToLowerInvariant(),
                    NextStepId = route.NextDisplayOrder.HasValue && stepIdByOrder.TryGetValue(route.NextDisplayOrder.Value, out var nextId)
                        ? nextId
                        : null,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
        }

        return routes;
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
        var stepRoutes = stepIds.Count == 0
            ? []
            : await dbContext.WorkflowStepRoutes
                .AsNoTracking()
                .Where(x => stepIds.Contains(x.WorkflowStepId))
                .ToListAsync(cancellationToken);
        entity.Status = status;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        var after = BuildDetailContract(entity, steps, stepRoles, stepRoutes);
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
        Guid? documentTemplateId,
        CancellationToken cancellationToken)
    {
        if (steps is null || steps.Count == 0)
        {
            return new WorkflowCommandResult(WorkflowCommandStatus.ValidationError, "Workflow steps are required.", ApiErrorCodes.RequestValidationFailed);
        }

        var displayOrders = new HashSet<int>();
        var roleIds = new HashSet<Guid>();
        var documentIds = new HashSet<Guid>();

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

            if (step.DocumentId.HasValue)
            {
                documentIds.Add(step.DocumentId.Value);
            }
        }

        if (documentIds.Count > 1)
        {
            return new WorkflowCommandResult(
                WorkflowCommandStatus.ValidationError,
                "Workflow steps must reference a single document.",
                ApiErrorCodes.RequestValidationFailed);
        }

        if (documentTemplateId.HasValue && documentIds.Count == 0)
        {
            return new WorkflowCommandResult(
                WorkflowCommandStatus.ValidationError,
                "Workflow document is required for the selected template.",
                ApiErrorCodes.RequestValidationFailed);
        }

        foreach (var step in steps)
        {
            var normalizedMinApprovals = NormalizeMinApprovals(step.StepType, step.MinApprovals);
            if (normalizedMinApprovals < 1)
            {
                return new WorkflowCommandResult(
                    WorkflowCommandStatus.ValidationError,
                    "Minimum approvals must be at least 1.",
                    ApiErrorCodes.RequestValidationFailed);
            }
        }

        if (documentIds.Count == 1)
        {
            var documentId = documentIds.First();
            var documentExists = await dbContext.Documents
                .AsNoTracking()
                .AnyAsync(x => x.Id == documentId && !x.IsDeleted, cancellationToken);
            if (!documentExists)
            {
                return new WorkflowCommandResult(
                    WorkflowCommandStatus.ValidationError,
                    "Workflow document does not exist.",
                    ApiErrorCodes.RequestValidationFailed);
            }

            if (steps.All(step => step.DocumentId.HasValue) && steps.First().DocumentId.HasValue)
            {
                // ok
            }
        }

        if (documentIds.Count == 1 && documentTemplateId.HasValue)
        {
            var documentId = documentIds.First();
            var inTemplate = await dbContext.DocumentTemplateItems
                .AsNoTracking()
                .AnyAsync(x => x.TemplateId == documentTemplateId.Value && x.DocumentId == documentId, cancellationToken);
            if (!inTemplate)
            {
                return new WorkflowCommandResult(
                    WorkflowCommandStatus.ValidationError,
                    "Workflow document is not in selected template.",
                    ApiErrorCodes.RequestValidationFailed);
            }
        }

        var validDisplayOrders = steps.Select(step => step.DisplayOrder).ToHashSet();
        foreach (var step in steps)
        {
            if (step.Routes is null || step.Routes.Count == 0)
            {
                continue;
            }

            var actionSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var route in step.Routes)
            {
                if (string.IsNullOrWhiteSpace(route.Action) || !AllowedStepTypes.Contains(route.Action.Trim()))
                {
                    return new WorkflowCommandResult(
                        WorkflowCommandStatus.ValidationError,
                        $"Invalid workflow route action: {route.Action}",
                        ApiErrorCodes.RequestValidationFailed);
                }

                if (!actionSet.Add(route.Action.Trim()))
                {
                    return new WorkflowCommandResult(
                        WorkflowCommandStatus.ValidationError,
                        "Workflow step routes must be unique per action.",
                        ApiErrorCodes.RequestValidationFailed);
                }

                if (route.NextDisplayOrder.HasValue)
                {
                    if (!validDisplayOrders.Contains(route.NextDisplayOrder.Value))
                    {
                        return new WorkflowCommandResult(
                            WorkflowCommandStatus.ValidationError,
                            "Workflow step route target does not exist.",
                            ApiErrorCodes.RequestValidationFailed);
                    }

                    if (route.NextDisplayOrder.Value == step.DisplayOrder)
                    {
                        return new WorkflowCommandResult(
                            WorkflowCommandStatus.ValidationError,
                            "Workflow step route cannot target the same step.",
                            ApiErrorCodes.RequestValidationFailed);
                    }
                }
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

    private static int NormalizeMinApprovals(string stepType, int minApprovals)
    {
        var normalized = minApprovals < 1 ? 1 : minApprovals;
        return normalized;
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
