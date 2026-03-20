using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Modules.Workflows.Infrastructure;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Workflows;

public sealed class WorkflowInstanceCommands(
    OperisDbContext dbContext,
    IWorkflowInstanceQueries queries) : IWorkflowInstanceCommands
{
    public async Task<(bool Success, string? Error, string? ErrorCode, WorkflowInstanceDetailContract? Response)> CreateInstanceAsync(
        CreateWorkflowInstanceRequest request,
        string? actorUserId,
        string? actorDisplayName,
        string? actorEmail,
        CancellationToken cancellationToken)
    {
        var workflowDefinitionId = request.WorkflowDefinitionId;
        var project = await dbContext.Projects.FirstOrDefaultAsync(x => x.Id == request.ProjectId && x.DeletedAt == null, cancellationToken);
        if (project is null)
        {
            return (false, "Project does not exist.", ApiErrorCodes.RequestValidationFailed, null);
        }

        if (workflowDefinitionId is null)
        {
            workflowDefinitionId = project.WorkflowDefinitionId;
        }

        if (workflowDefinitionId is null)
        {
            return (false, "Workflow definition is required.", ApiErrorCodes.RequestValidationFailed, null);
        }

        var workflowDefinition = await dbContext.WorkflowDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == workflowDefinitionId.Value, cancellationToken);

        if (workflowDefinition is null)
        {
            return (false, "Workflow definition does not exist.", ApiErrorCodes.RequestValidationFailed, null);
        }

        if (string.Equals(workflowDefinition.Status, "archived", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Workflow definition is archived.", ApiErrorCodes.RequestValidationFailed, null);
        }

        var documentExists = await dbContext.Documents.AnyAsync(x => x.Id == request.DocumentId && !x.IsDeleted, cancellationToken);
        if (!documentExists)
        {
            return (false, "Document does not exist.", ApiErrorCodes.RequestValidationFailed, null);
        }

        var activeInstanceExists = await dbContext.WorkflowInstances
            .AnyAsync(x =>
                x.DocumentId == request.DocumentId &&
                x.ProjectId == request.ProjectId &&
                x.Status == "in_progress",
                cancellationToken);
        if (activeInstanceExists)
        {
            return (false, "Workflow instance already exists for this document.", ApiErrorCodes.RequestValidationFailed, null);
        }

        var workflowSteps = await dbContext.WorkflowSteps
            .AsNoTracking()
            .Where(x => x.WorkflowDefinitionId == workflowDefinitionId.Value)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);

        if (workflowSteps.Count == 0)
        {
            return (false, "Workflow definition has no steps.", ApiErrorCodes.RequestValidationFailed, null);
        }

        var stepDocumentIds = workflowSteps
            .Where(step => step.DocumentId.HasValue)
            .Select(step => step.DocumentId!.Value)
            .Distinct()
            .ToList();

        if (stepDocumentIds.Count > 0 && stepDocumentIds.Any(id => id != request.DocumentId))
        {
            return (false, "Workflow definition does not match selected document.", ApiErrorCodes.RequestValidationFailed, null);
        }

        var instance = new WorkflowInstanceEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            DocumentId = request.DocumentId,
            WorkflowDefinitionId = workflowDefinitionId.Value,
            Status = "in_progress",
            CurrentStepOrder = workflowSteps.First().DisplayOrder,
            StartedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.WorkflowInstances.Add(instance);

        var instanceSteps = workflowSteps.Select((step, index) => new WorkflowInstanceStepEntity
        {
            Id = Guid.NewGuid(),
            WorkflowInstanceId = instance.Id,
            WorkflowStepId = step.Id,
            StepType = step.StepType,
            DisplayOrder = step.DisplayOrder,
            IsRequired = step.IsRequired,
            Status = index == 0 ? "in_progress" : "pending",
            StartedAt = index == 0 ? DateTimeOffset.UtcNow : null,
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();

        dbContext.WorkflowInstanceSteps.AddRange(instanceSteps);

        await dbContext.SaveChangesAsync(cancellationToken);

        var detail = await queries.GetInstanceAsync(instance.Id, cancellationToken);
        return (true, null, null, detail);
    }

    public async Task<(bool Success, string? Error, string? ErrorCode, WorkflowInstanceDetailContract? Response, bool NotFound)> ApplyStepActionAsync(
        Guid workflowInstanceId,
        Guid workflowInstanceStepId,
        WorkflowStepActionRequest request,
        string? actorUserId,
        string? actorDisplayName,
        string? actorEmail,
        CancellationToken cancellationToken)
    {
        var instance = await dbContext.WorkflowInstances.FirstOrDefaultAsync(x => x.Id == workflowInstanceId, cancellationToken);
        if (instance is null)
        {
            return (false, null, null, null, true);
        }

        var step = await dbContext.WorkflowInstanceSteps.FirstOrDefaultAsync(
            x => x.Id == workflowInstanceStepId && x.WorkflowInstanceId == workflowInstanceId,
            cancellationToken);

        if (step is null)
        {
            return (false, "Workflow step does not exist.", ApiErrorCodes.RequestValidationFailed, null, false);
        }

        var workflowStep = await dbContext.WorkflowSteps
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == step.WorkflowStepId, cancellationToken);
        if (workflowStep is null)
        {
            return (false, "Workflow step does not exist.", ApiErrorCodes.RequestValidationFailed, null, false);
        }

        if (!string.Equals(instance.Status, "in_progress", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Workflow instance is not active.", ApiErrorCodes.RequestValidationFailed, null, false);
        }

        if (!string.Equals(step.Status, "in_progress", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Workflow step is not active.", ApiErrorCodes.RequestValidationFailed, null, false);
        }

        if (!IsSupportedAction(request.Action))
        {
            return (false, $"Unsupported action: {request.Action}.", ApiErrorCodes.RequestValidationFailed, null, false);
        }

        var stepRoutes = await dbContext.WorkflowStepRoutes
            .AsNoTracking()
            .Where(x => x.WorkflowStepId == step.WorkflowStepId)
            .ToListAsync(cancellationToken);

        if (stepRoutes.Count == 0)
        {
            if (!string.Equals(workflowStep.StepType, request.Action, StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Action does not match step type.", ApiErrorCodes.RequestValidationFailed, null, false);
            }
        }
        else
        {
            var routeExists = stepRoutes.Any(route => string.Equals(route.Action, request.Action, StringComparison.OrdinalIgnoreCase));
            if (!routeExists)
            {
                return (false, "Action is not allowed for this step.", ApiErrorCodes.RequestValidationFailed, null, false);
            }
        }

        if (!await HasRolePermissionAsync(instance.ProjectId, step.WorkflowStepId, actorUserId, cancellationToken))
        {
            return (false, "User does not have permission for this workflow step.", ApiErrorCodes.RequestValidationFailed, null, false);
        }

        var normalizedAction = request.Action.Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(actorUserId))
        {
            var alreadyActed = await dbContext.WorkflowInstanceActions
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkflowInstanceStepId == step.Id &&
                    x.ActorUserId == actorUserId &&
                    x.Action == normalizedAction,
                    cancellationToken);
            if (alreadyActed)
            {
                return (false, "User already completed this step.", ApiErrorCodes.RequestValidationFailed, null, false);
            }
        }

        var actionEntry = new WorkflowInstanceActionEntity
        {
            Id = Guid.NewGuid(),
            WorkflowInstanceStepId = step.Id,
            Action = normalizedAction,
            ActorUserId = actorUserId,
            ActorEmail = actorEmail,
            ActorDisplayName = actorDisplayName,
            Comment = NormalizeOptional(request.Comment, 512),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.WorkflowInstanceActions.Add(actionEntry);

        var shouldCompleteStep = true;
        if (string.Equals(workflowStep.StepType, "peer_review", StringComparison.OrdinalIgnoreCase) && workflowStep.MinApprovals > 1)
        {
            var approvals = await dbContext.WorkflowInstanceActions
                .AsNoTracking()
                .Where(x => x.WorkflowInstanceStepId == step.Id && x.Action == normalizedAction)
                .Select(x => x.ActorUserId)
                .Distinct()
                .CountAsync(cancellationToken);
            shouldCompleteStep = approvals + 1 >= workflowStep.MinApprovals;
        }

        if (shouldCompleteStep)
        {
            step.Status = "completed";
            step.CompletedAt = DateTimeOffset.UtcNow;
            step.UpdatedAt = DateTimeOffset.UtcNow;
        }

        WorkflowInstanceStepEntity? nextStep = null;
        if (stepRoutes.Count == 0)
        {
            if (shouldCompleteStep)
            {
                nextStep = await dbContext.WorkflowInstanceSteps
                    .Where(x => x.WorkflowInstanceId == workflowInstanceId && x.DisplayOrder > step.DisplayOrder)
                    .OrderBy(x => x.DisplayOrder)
                    .FirstOrDefaultAsync(cancellationToken);
            }
        }
        else
        {
            var matchedRoute = stepRoutes.First(route => string.Equals(route.Action, request.Action, StringComparison.OrdinalIgnoreCase));
            if (shouldCompleteStep && matchedRoute.NextStepId.HasValue)
            {
                nextStep = await dbContext.WorkflowInstanceSteps
                    .FirstOrDefaultAsync(
                        x => x.WorkflowInstanceId == workflowInstanceId && x.WorkflowStepId == matchedRoute.NextStepId.Value,
                        cancellationToken);
                if (nextStep is null)
                {
                    return (false, "Workflow route target was not found in instance.", ApiErrorCodes.RequestValidationFailed, null, false);
                }
            }
        }

        if (nextStep is null)
        {
            if (shouldCompleteStep)
            {
                instance.Status = "completed";
                instance.CompletedAt = DateTimeOffset.UtcNow;
            }
        }
        else
        {
            nextStep.Status = "in_progress";
            nextStep.StartedAt = DateTimeOffset.UtcNow;
            nextStep.UpdatedAt = DateTimeOffset.UtcNow;
            instance.CurrentStepOrder = nextStep.DisplayOrder;
        }

        instance.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var detail = await queries.GetInstanceAsync(workflowInstanceId, cancellationToken);
        return (true, null, null, detail, false);
    }

    private async Task<bool> HasRolePermissionAsync(Guid projectId, Guid workflowStepId, string? actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(actorUserId))
        {
            return false;
        }

        var roleIds = await dbContext.WorkflowStepRoles
            .AsNoTracking()
            .Where(x => x.WorkflowStepId == workflowStepId)
            .Select(x => x.ProjectRoleId)
            .ToListAsync(cancellationToken);

        if (roleIds.Count == 0)
        {
            return false;
        }

        return await dbContext.UserProjectAssignments
            .AsNoTracking()
            .AnyAsync(x =>
                x.ProjectId == projectId &&
                x.UserId == actorUserId &&
                x.Status == "Active" &&
                roleIds.Contains(x.ProjectRoleId),
                cancellationToken);
    }

    private static bool IsSupportedAction(string? action)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            return false;
        }

        var normalized = action.Trim().ToLowerInvariant();
        return normalized is "submit" or "peer_review" or "review" or "approve";
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
}
