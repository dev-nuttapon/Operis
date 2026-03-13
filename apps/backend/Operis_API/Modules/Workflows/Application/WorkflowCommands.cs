using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Workflows.Infrastructure;
using Operis_API.Shared.Auditing;
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
            return new WorkflowCommandResult(WorkflowCommandStatus.ValidationError, "Workflow definition name is required.");
        }

        var code = ToCode(name);
        if (string.IsNullOrWhiteSpace(code))
        {
            return new WorkflowCommandResult(WorkflowCommandStatus.ValidationError, "Workflow definition name is required.");
        }

        var exists = await dbContext.WorkflowDefinitions
            .AnyAsync(x => x.Code == code, cancellationToken);
        if (exists)
        {
            return new WorkflowCommandResult(WorkflowCommandStatus.Conflict, "Workflow definition already exists.");
        }

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
}
