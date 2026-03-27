using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Governance.Contracts;
using Operis_API.Modules.Governance.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Governance.Application;

public sealed class GovernanceOperationsCommands(OperisDbContext dbContext, IAuditLogWriter auditLogWriter) : IGovernanceOperationsCommands
{
    public async Task<GovernanceCommandResult<RaciMapResponse>> CreateRaciMapAsync(CreateRaciMapRequest request, string? actor, CancellationToken cancellationToken)
    {
        var processCode = Required(request.ProcessCode, 128);
        var roleName = Required(request.RoleName, 256);
        var responsibilityType = NormalizeResponsibility(request.ResponsibilityType);
        if (processCode is null || roleName is null || responsibilityType is null)
        {
            return Validation<RaciMapResponse>("Process, role, and responsibility type are required.");
        }

        if (await dbContext.RaciMaps.AnyAsync(x => x.ProcessCode == processCode && x.RoleName == roleName && x.ResponsibilityType == responsibilityType, cancellationToken))
        {
            return Conflict<RaciMapResponse>("RACI entry already exists.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new RaciMapEntity
        {
            Id = Guid.NewGuid(),
            ProcessCode = processCode,
            RoleName = roleName,
            ResponsibilityType = responsibilityType,
            Status = NormalizeGovernanceStatus(request.Status),
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.RaciMaps.Add(entity);
        WriteApprovalEvidenceIfNeeded("raci_map", entity.Id, entity.Status, actor, request.Reason);
        await PersistWithAuditAsync("create", "raci_map", entity.Id.ToString(), StatusCodes.Status201Created, entity, cancellationToken);
        return Success(ToResponse(entity));
    }

    public async Task<GovernanceCommandResult<RaciMapResponse>> UpdateRaciMapAsync(Guid id, UpdateRaciMapRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.RaciMaps.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound<RaciMapResponse>();

        var processCode = Required(request.ProcessCode, 128);
        var roleName = Required(request.RoleName, 256);
        var responsibilityType = NormalizeResponsibility(request.ResponsibilityType);
        if (processCode is null || roleName is null || responsibilityType is null)
        {
            return Validation<RaciMapResponse>("Process, role, and responsibility type are required.");
        }

        var status = NormalizeGovernanceStatus(request.Status);
        if (!IsAllowedTransition(entity.Status, status))
        {
            return Validation<RaciMapResponse>("Invalid workflow transition.", ApiErrorCodes.InvalidWorkflowTransition);
        }

        entity.ProcessCode = processCode;
        entity.RoleName = roleName;
        entity.ResponsibilityType = responsibilityType;
        entity.Status = status;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        WriteApprovalEvidenceIfNeeded("raci_map", entity.Id, entity.Status, actor, request.Reason);
        await PersistWithAuditAsync("update", "raci_map", entity.Id.ToString(), StatusCodes.Status200OK, entity, cancellationToken);
        return Success(ToResponse(entity));
    }

    public async Task<GovernanceCommandResult<SlaRuleResponse>> CreateSlaRuleAsync(CreateSlaRuleRequest request, string? actor, CancellationToken cancellationToken)
    {
        var scopeType = Required(request.ScopeType, 64);
        var scopeRef = Required(request.ScopeRef, 256);
        var escalationPolicyId = Required(request.EscalationPolicyId, 128);
        if (scopeType is null || scopeRef is null)
        {
            return Validation<SlaRuleResponse>("Scope type and scope reference are required.");
        }

        if (request.TargetDurationHours < 1)
        {
            return Validation<SlaRuleResponse>("Target duration is required.", ApiErrorCodes.SlaTargetRequired);
        }

        if (escalationPolicyId is null)
        {
            return Validation<SlaRuleResponse>("Escalation policy is required.", ApiErrorCodes.SlaEscalationPolicyRequired);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new SlaRuleEntity
        {
            Id = Guid.NewGuid(),
            ScopeType = scopeType,
            ScopeRef = scopeRef,
            TargetDurationHours = request.TargetDurationHours,
            EscalationPolicyId = escalationPolicyId,
            Status = NormalizeGovernanceStatus(request.Status),
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.SlaRules.Add(entity);
        WriteApprovalEvidenceIfNeeded("sla_rule", entity.Id, entity.Status, actor, request.Reason);
        await PersistWithAuditAsync("create", "sla_rule", entity.Id.ToString(), StatusCodes.Status201Created, entity, cancellationToken);
        return Success(ToResponse(entity));
    }

    public async Task<GovernanceCommandResult<SlaRuleResponse>> UpdateSlaRuleAsync(Guid id, UpdateSlaRuleRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.SlaRules.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound<SlaRuleResponse>();

        var scopeType = Required(request.ScopeType, 64);
        var scopeRef = Required(request.ScopeRef, 256);
        var escalationPolicyId = Required(request.EscalationPolicyId, 128);
        if (scopeType is null || scopeRef is null)
        {
            return Validation<SlaRuleResponse>("Scope type and scope reference are required.");
        }

        if (request.TargetDurationHours < 1)
        {
            return Validation<SlaRuleResponse>("Target duration is required.", ApiErrorCodes.SlaTargetRequired);
        }

        if (escalationPolicyId is null)
        {
            return Validation<SlaRuleResponse>("Escalation policy is required.", ApiErrorCodes.SlaEscalationPolicyRequired);
        }

        var status = NormalizeGovernanceStatus(request.Status);
        if (!IsAllowedTransition(entity.Status, status))
        {
            return Validation<SlaRuleResponse>("Invalid workflow transition.", ApiErrorCodes.InvalidWorkflowTransition);
        }

        entity.ScopeType = scopeType;
        entity.ScopeRef = scopeRef;
        entity.TargetDurationHours = request.TargetDurationHours;
        entity.EscalationPolicyId = escalationPolicyId;
        entity.Status = status;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        WriteApprovalEvidenceIfNeeded("sla_rule", entity.Id, entity.Status, actor, request.Reason);
        await PersistWithAuditAsync("update", "sla_rule", entity.Id.ToString(), StatusCodes.Status200OK, entity, cancellationToken);
        return Success(ToResponse(entity));
    }

    public async Task<GovernanceCommandResult<RetentionPolicyResponse>> CreateRetentionPolicyAsync(CreateRetentionPolicyRequest request, string? actor, CancellationToken cancellationToken)
    {
        var policyCode = Required(request.PolicyCode, 128);
        var appliesTo = Required(request.AppliesTo, 128);
        if (policyCode is null || appliesTo is null || request.RetentionPeriodDays < 1)
        {
            return Validation<RetentionPolicyResponse>("Policy code, scope, and retention period are required.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new RetentionPolicyEntity
        {
            Id = Guid.NewGuid(),
            PolicyCode = policyCode,
            AppliesTo = appliesTo,
            RetentionPeriodDays = request.RetentionPeriodDays,
            ArchiveRule = Optional(request.ArchiveRule, 512),
            Status = NormalizeGovernanceStatus(request.Status),
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.RetentionPolicies.Add(entity);
        WriteApprovalEvidenceIfNeeded("retention_policy", entity.Id, entity.Status, actor, request.Reason);
        await PersistWithAuditAsync("create", "retention_policy", entity.Id.ToString(), StatusCodes.Status201Created, entity, cancellationToken);
        return Success(ToResponse(entity));
    }

    public async Task<GovernanceCommandResult<RetentionPolicyResponse>> UpdateRetentionPolicyAsync(Guid id, UpdateRetentionPolicyRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.RetentionPolicies.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound<RetentionPolicyResponse>();

        var policyCode = Required(request.PolicyCode, 128);
        var appliesTo = Required(request.AppliesTo, 128);
        if (policyCode is null || appliesTo is null || request.RetentionPeriodDays < 1)
        {
            return Validation<RetentionPolicyResponse>("Policy code, scope, and retention period are required.");
        }

        var status = NormalizeGovernanceStatus(request.Status);
        if (!IsAllowedTransition(entity.Status, status))
        {
            return Validation<RetentionPolicyResponse>("Invalid workflow transition.", ApiErrorCodes.InvalidWorkflowTransition);
        }

        entity.PolicyCode = policyCode;
        entity.AppliesTo = appliesTo;
        entity.RetentionPeriodDays = request.RetentionPeriodDays;
        entity.ArchiveRule = Optional(request.ArchiveRule, 512);
        entity.Status = status;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        WriteApprovalEvidenceIfNeeded("retention_policy", entity.Id, entity.Status, actor, request.Reason);
        await PersistWithAuditAsync("update", "retention_policy", entity.Id.ToString(), StatusCodes.Status200OK, entity, cancellationToken);
        return Success(ToResponse(entity));
    }

    private void WriteApprovalEvidenceIfNeeded(string entityType, Guid entityId, string status, string? actor, string? reason)
    {
        if (!string.Equals(status, "approved", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        dbContext.ApprovalEvidenceLogs.Add(new ApprovalEvidenceLogEntity
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId.ToString(),
            ApproverUserId = actor ?? "unknown",
            ApprovedAt = DateTimeOffset.UtcNow,
            Reason = Optional(reason, 2000) ?? $"Approved {entityType.Replace('_', ' ')}.",
            Outcome = "approved"
        });
    }

    private async Task PersistWithAuditAsync(string action, string entityType, string entityId, int statusCode, object after, CancellationToken cancellationToken)
    {
        auditLogWriter.Append(new AuditLogEntry(Module: "governance", Action: action, EntityType: entityType, EntityId: entityId, StatusCode: statusCode, After: after));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool IsAllowedTransition(string currentStatus, string nextStatus)
    {
        if (string.Equals(currentStatus, nextStatus, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return currentStatus switch
        {
            "draft" => nextStatus == "approved",
            "approved" => nextStatus == "active",
            "active" => nextStatus == "archived",
            _ => false
        };
    }

    private static string? Required(string? value, int maxLength) => string.IsNullOrWhiteSpace(value) ? null : Trim(value, maxLength);
    private static string? Optional(string? value, int maxLength) => string.IsNullOrWhiteSpace(value) ? null : Trim(value, maxLength);
    private static string Trim(string value, int maxLength) => value.Trim().Length > maxLength ? value.Trim()[..maxLength] : value.Trim();
    private static string? NormalizeResponsibility(string? value) => value?.Trim().ToUpperInvariant() switch { "R" or "A" or "C" or "I" => value.Trim().ToUpperInvariant(), _ => null };
    private static string NormalizeGovernanceStatus(string? value) => value?.Trim().ToLowerInvariant() switch { "approved" => "approved", "active" => "active", "archived" => "archived", _ => "draft" };
    private static GovernanceCommandResult<T> Success<T>(T value) => new(GovernanceCommandStatus.Success, value);
    private static GovernanceCommandResult<T> Validation<T>(string message, string? code = null) => new(GovernanceCommandStatus.ValidationError, default, message, code ?? ApiErrorCodes.RequestValidationFailed);
    private static GovernanceCommandResult<T> NotFound<T>() => new(GovernanceCommandStatus.NotFound, default, "Resource not found.", ApiErrorCodes.ResourceNotFound);
    private static GovernanceCommandResult<T> Conflict<T>(string message) => new(GovernanceCommandStatus.Conflict, default, message, ApiErrorCodes.RequestValidationFailed);
    private static RaciMapResponse ToResponse(RaciMapEntity entity) => new(entity.Id, entity.ProcessCode, entity.RoleName, entity.ResponsibilityType, entity.Status, entity.CreatedAt, entity.UpdatedAt);
    private static SlaRuleResponse ToResponse(SlaRuleEntity entity) => new(entity.Id, entity.ScopeType, entity.ScopeRef, entity.TargetDurationHours, entity.EscalationPolicyId, entity.Status, entity.CreatedAt, entity.UpdatedAt);
    private static RetentionPolicyResponse ToResponse(RetentionPolicyEntity entity) => new(entity.Id, entity.PolicyCode, entity.AppliesTo, entity.RetentionPeriodDays, entity.ArchiveRule, entity.Status, entity.CreatedAt, entity.UpdatedAt);
}
