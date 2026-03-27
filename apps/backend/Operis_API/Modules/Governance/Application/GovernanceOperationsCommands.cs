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

    public async Task<GovernanceCommandResult<ArchitectureRecordResponse>> CreateArchitectureRecordAsync(CreateArchitectureRecordRequest request, string? actor, CancellationToken cancellationToken)
    {
        var title = Required(request.Title, 512);
        var architectureType = Required(request.ArchitectureType, 128);
        var ownerUserId = Required(request.OwnerUserId, 128);
        if (title is null || architectureType is null || ownerUserId is null)
        {
            return Validation<ArchitectureRecordResponse>("Project, title, architecture type, and owner are required.");
        }

        var project = await dbContext.Projects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ProjectId, cancellationToken);
        if (project is null)
        {
            return NotFound<ArchitectureRecordResponse>();
        }

        var status = NormalizeArchitectureStatus(request.Status);
        var evidenceRef = Optional(request.EvidenceRef, 512);
        if (RequiresSecurityEvidence(architectureType) && status is "approved" or "active" && evidenceRef is null)
        {
            return Validation<ArchitectureRecordResponse>("Security-sensitive architecture changes require evidence.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new ArchitectureRecordEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Title = title,
            ArchitectureType = architectureType.ToLowerInvariant(),
            OwnerUserId = ownerUserId,
            Status = status,
            CurrentVersionId = Optional(request.CurrentVersionId, 128),
            Summary = Optional(request.Summary, 4000),
            SecurityImpact = Optional(request.SecurityImpact, 4000),
            EvidenceRef = evidenceRef,
            ApprovedBy = status is "approved" or "active" or "superseded" ? actor ?? "unknown" : null,
            ApprovedAt = status is "approved" or "active" or "superseded" ? now : null,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.ArchitectureRecords.Add(entity);
        WriteApprovalEvidenceIfNeeded("architecture_record", entity.Id, entity.Status, actor, entity.Summary);
        await PersistWithAuditAsync("create", "architecture_record", entity.Id.ToString(), StatusCodes.Status201Created, entity, cancellationToken);
        return Success(ToResponse(entity, project.Name));
    }

    public async Task<GovernanceCommandResult<ArchitectureRecordResponse>> UpdateArchitectureRecordAsync(Guid id, UpdateArchitectureRecordRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ArchitectureRecords.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound<ArchitectureRecordResponse>();

        var title = Required(request.Title, 512);
        var architectureType = Required(request.ArchitectureType, 128);
        var ownerUserId = Required(request.OwnerUserId, 128);
        if (title is null || architectureType is null || ownerUserId is null)
        {
            return Validation<ArchitectureRecordResponse>("Project, title, architecture type, and owner are required.");
        }

        var project = await dbContext.Projects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ProjectId, cancellationToken);
        if (project is null)
        {
            return NotFound<ArchitectureRecordResponse>();
        }

        var nextStatus = NormalizeArchitectureStatus(request.Status);
        if (!IsAllowedArchitectureTransition(entity.Status, nextStatus))
        {
            return Validation<ArchitectureRecordResponse>("Invalid workflow transition.", ApiErrorCodes.InvalidWorkflowTransition);
        }

        var evidenceRef = Optional(request.EvidenceRef, 512);
        if (RequiresSecurityEvidence(architectureType) && nextStatus is "approved" or "active" && evidenceRef is null)
        {
            return Validation<ArchitectureRecordResponse>("Security-sensitive architecture changes require evidence.");
        }

        entity.ProjectId = request.ProjectId;
        entity.Title = title;
        entity.ArchitectureType = architectureType.ToLowerInvariant();
        entity.OwnerUserId = ownerUserId;
        entity.Status = nextStatus;
        entity.CurrentVersionId = Optional(request.CurrentVersionId, 128);
        entity.Summary = Optional(request.Summary, 4000);
        entity.SecurityImpact = Optional(request.SecurityImpact, 4000);
        entity.EvidenceRef = evidenceRef;
        entity.ApprovedBy = nextStatus is "approved" or "active" or "superseded" ? actor ?? entity.ApprovedBy ?? "unknown" : null;
        entity.ApprovedAt = nextStatus is "approved" or "active" or "superseded" ? entity.ApprovedAt ?? DateTimeOffset.UtcNow : null;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        WriteApprovalEvidenceIfNeeded("architecture_record", entity.Id, entity.Status, actor, entity.Summary);
        await PersistWithAuditAsync("update", "architecture_record", entity.Id.ToString(), StatusCodes.Status200OK, entity, cancellationToken);
        return Success(ToResponse(entity, project.Name));
    }

    public async Task<GovernanceCommandResult<DesignReviewResponse>> CreateDesignReviewAsync(CreateDesignReviewRequest request, string? actor, CancellationToken cancellationToken)
    {
        var reviewType = Required(request.ReviewType, 128);
        if (reviewType is null)
        {
            return Validation<DesignReviewResponse>("Architecture record and review type are required.");
        }

        var architecture = await dbContext.ArchitectureRecords.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ArchitectureRecordId, cancellationToken);
        if (architecture is null)
        {
            return NotFound<DesignReviewResponse>();
        }

        var status = NormalizeDesignReviewStatus(request.Status);
        var decisionReason = Optional(request.DecisionReason, 2000);
        if (status is "approved" or "rejected" && decisionReason is null)
        {
            return Validation<DesignReviewResponse>(
                "Design review decision reason is required.",
                ApiErrorCodes.DesignReviewDecisionReasonRequired);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new DesignReviewEntity
        {
            Id = Guid.NewGuid(),
            ArchitectureRecordId = request.ArchitectureRecordId,
            ReviewType = reviewType.ToLowerInvariant(),
            ReviewedBy = Optional(request.ReviewedBy, 128),
            Status = status,
            DecisionReason = decisionReason,
            DesignSummary = Optional(request.DesignSummary, 4000),
            Concerns = Optional(request.Concerns, 4000),
            EvidenceRef = Optional(request.EvidenceRef, 512),
            DecidedAt = status is "approved" or "rejected" or "baseline" ? now : null,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.DesignReviews.Add(entity);
        WriteApprovalEvidenceIfNeeded("design_review", entity.Id, entity.Status, actor, entity.DecisionReason);
        await PersistWithAuditAsync("create", "design_review", entity.Id.ToString(), StatusCodes.Status201Created, entity, cancellationToken);
        return Success(ToResponse(entity, architecture.Title));
    }

    public async Task<GovernanceCommandResult<DesignReviewResponse>> UpdateDesignReviewAsync(Guid id, UpdateDesignReviewRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.DesignReviews.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound<DesignReviewResponse>();

        var reviewType = Required(request.ReviewType, 128);
        if (reviewType is null)
        {
            return Validation<DesignReviewResponse>("Architecture record and review type are required.");
        }

        var architecture = await dbContext.ArchitectureRecords.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ArchitectureRecordId, cancellationToken);
        if (architecture is null)
        {
            return NotFound<DesignReviewResponse>();
        }

        var nextStatus = NormalizeDesignReviewStatus(request.Status);
        if (!IsAllowedDesignReviewTransition(entity.Status, nextStatus))
        {
            return Validation<DesignReviewResponse>("Invalid workflow transition.", ApiErrorCodes.InvalidWorkflowTransition);
        }

        var decisionReason = Optional(request.DecisionReason, 2000);
        if (nextStatus is "approved" or "rejected" && decisionReason is null)
        {
            return Validation<DesignReviewResponse>(
                "Design review decision reason is required.",
                ApiErrorCodes.DesignReviewDecisionReasonRequired);
        }

        entity.ArchitectureRecordId = request.ArchitectureRecordId;
        entity.ReviewType = reviewType.ToLowerInvariant();
        entity.ReviewedBy = Optional(request.ReviewedBy, 128);
        entity.Status = nextStatus;
        entity.DecisionReason = decisionReason;
        entity.DesignSummary = Optional(request.DesignSummary, 4000);
        entity.Concerns = Optional(request.Concerns, 4000);
        entity.EvidenceRef = Optional(request.EvidenceRef, 512);
        entity.DecidedAt = nextStatus is "approved" or "rejected" or "baseline" ? DateTimeOffset.UtcNow : null;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        WriteApprovalEvidenceIfNeeded("design_review", entity.Id, entity.Status, actor, entity.DecisionReason);
        await PersistWithAuditAsync("update", "design_review", entity.Id.ToString(), StatusCodes.Status200OK, entity, cancellationToken);
        return Success(ToResponse(entity, architecture.Title));
    }

    public async Task<GovernanceCommandResult<IntegrationReviewResponse>> CreateIntegrationReviewAsync(CreateIntegrationReviewRequest request, string? actor, CancellationToken cancellationToken)
    {
        var scopeRef = Required(request.ScopeRef, 256);
        var integrationType = Required(request.IntegrationType, 128);
        if (scopeRef is null || integrationType is null)
        {
            return Validation<IntegrationReviewResponse>("Scope and integration type are required.");
        }

        var status = NormalizeIntegrationReviewStatus(request.Status);
        if (status == "applied")
        {
            return Validation<IntegrationReviewResponse>(
                "Integration review requires an approved decision before apply.",
                ApiErrorCodes.IntegrationReviewApprovalRequired);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new IntegrationReviewEntity
        {
            Id = Guid.NewGuid(),
            ScopeRef = scopeRef,
            IntegrationType = integrationType.ToLowerInvariant(),
            ReviewedBy = Optional(request.ReviewedBy, 128),
            Status = status,
            DecisionReason = Optional(request.DecisionReason, 2000),
            Risks = Optional(request.Risks, 4000),
            DependencyImpact = Optional(request.DependencyImpact, 4000),
            EvidenceRef = Optional(request.EvidenceRef, 512),
            DecidedAt = status is "approved" or "rejected" ? now : null,
            AppliedAt = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.IntegrationReviews.Add(entity);
        WriteApprovalEvidenceIfNeeded("integration_review", entity.Id, entity.Status, actor, entity.DecisionReason);
        await PersistWithAuditAsync("create", "integration_review", entity.Id.ToString(), StatusCodes.Status201Created, entity, cancellationToken);
        return Success(ToResponse(entity));
    }

    public async Task<GovernanceCommandResult<IntegrationReviewResponse>> UpdateIntegrationReviewAsync(Guid id, UpdateIntegrationReviewRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.IntegrationReviews.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound<IntegrationReviewResponse>();

        var scopeRef = Required(request.ScopeRef, 256);
        var integrationType = Required(request.IntegrationType, 128);
        if (scopeRef is null || integrationType is null)
        {
            return Validation<IntegrationReviewResponse>("Scope and integration type are required.");
        }

        var nextStatus = NormalizeIntegrationReviewStatus(request.Status);
        if (nextStatus == "applied" && entity.Status != "approved")
        {
            return Validation<IntegrationReviewResponse>(
                "Integration review requires an approved decision before apply.",
                ApiErrorCodes.IntegrationReviewApprovalRequired);
        }

        if (!IsAllowedIntegrationReviewTransition(entity.Status, nextStatus))
        {
            return Validation<IntegrationReviewResponse>("Invalid workflow transition.", ApiErrorCodes.InvalidWorkflowTransition);
        }

        entity.ScopeRef = scopeRef;
        entity.IntegrationType = integrationType.ToLowerInvariant();
        entity.ReviewedBy = Optional(request.ReviewedBy, 128);
        entity.Status = nextStatus;
        entity.DecisionReason = Optional(request.DecisionReason, 2000);
        entity.Risks = Optional(request.Risks, 4000);
        entity.DependencyImpact = Optional(request.DependencyImpact, 4000);
        entity.EvidenceRef = Optional(request.EvidenceRef, 512);
        entity.DecidedAt = nextStatus is "approved" or "rejected" ? DateTimeOffset.UtcNow : entity.DecidedAt;
        entity.AppliedAt = nextStatus == "applied" ? DateTimeOffset.UtcNow : null;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        WriteApprovalEvidenceIfNeeded("integration_review", entity.Id, entity.Status, actor, entity.DecisionReason);
        await PersistWithAuditAsync("update", "integration_review", entity.Id.ToString(), StatusCodes.Status200OK, entity, cancellationToken);
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

    private static bool IsAllowedArchitectureTransition(string currentStatus, string nextStatus)
    {
        if (string.Equals(currentStatus, nextStatus, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return currentStatus switch
        {
            "draft" => nextStatus == "reviewed",
            "reviewed" => nextStatus == "approved",
            "approved" => nextStatus == "active",
            "active" => nextStatus == "superseded",
            _ => false
        };
    }

    private static bool IsAllowedDesignReviewTransition(string currentStatus, string nextStatus)
    {
        if (string.Equals(currentStatus, nextStatus, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return currentStatus switch
        {
            "draft" => nextStatus == "in_review",
            "in_review" => nextStatus is "approved" or "rejected",
            "approved" or "rejected" => nextStatus == "baseline",
            _ => false
        };
    }

    private static bool IsAllowedIntegrationReviewTransition(string currentStatus, string nextStatus)
    {
        if (string.Equals(currentStatus, nextStatus, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return currentStatus switch
        {
            "draft" => nextStatus == "in_review",
            "in_review" => nextStatus is "approved" or "rejected",
            "approved" => nextStatus == "applied",
            _ => false
        };
    }

    private static string? Required(string? value, int maxLength) => string.IsNullOrWhiteSpace(value) ? null : Trim(value, maxLength);
    private static string? Optional(string? value, int maxLength) => string.IsNullOrWhiteSpace(value) ? null : Trim(value, maxLength);
    private static string Trim(string value, int maxLength) => value.Trim().Length > maxLength ? value.Trim()[..maxLength] : value.Trim();
    private static string? NormalizeResponsibility(string? value) => value?.Trim().ToUpperInvariant() switch { "R" or "A" or "C" or "I" => value.Trim().ToUpperInvariant(), _ => null };
    private static string NormalizeGovernanceStatus(string? value) => value?.Trim().ToLowerInvariant() switch { "approved" => "approved", "active" => "active", "archived" => "archived", _ => "draft" };
    private static string NormalizeArchitectureStatus(string? value) => value?.Trim().ToLowerInvariant() switch { "reviewed" => "reviewed", "approved" => "approved", "active" => "active", "superseded" => "superseded", _ => "draft" };
    private static string NormalizeDesignReviewStatus(string? value) => value?.Trim().ToLowerInvariant() switch { "in_review" => "in_review", "approved" => "approved", "rejected" => "rejected", "baseline" => "baseline", _ => "draft" };
    private static string NormalizeIntegrationReviewStatus(string? value) => value?.Trim().ToLowerInvariant() switch { "in_review" => "in_review", "approved" => "approved", "rejected" => "rejected", "applied" => "applied", _ => "draft" };
    private static bool RequiresSecurityEvidence(string architectureType) => architectureType.Contains("security", StringComparison.OrdinalIgnoreCase);
    private static GovernanceCommandResult<T> Success<T>(T value) => new(GovernanceCommandStatus.Success, value);
    private static GovernanceCommandResult<T> Validation<T>(string message, string? code = null) => new(GovernanceCommandStatus.ValidationError, default, message, code ?? ApiErrorCodes.RequestValidationFailed);
    private static GovernanceCommandResult<T> NotFound<T>() => new(GovernanceCommandStatus.NotFound, default, "Resource not found.", ApiErrorCodes.ResourceNotFound);
    private static GovernanceCommandResult<T> Conflict<T>(string message) => new(GovernanceCommandStatus.Conflict, default, message, ApiErrorCodes.RequestValidationFailed);
    private static RaciMapResponse ToResponse(RaciMapEntity entity) => new(entity.Id, entity.ProcessCode, entity.RoleName, entity.ResponsibilityType, entity.Status, entity.CreatedAt, entity.UpdatedAt);
    private static SlaRuleResponse ToResponse(SlaRuleEntity entity) => new(entity.Id, entity.ScopeType, entity.ScopeRef, entity.TargetDurationHours, entity.EscalationPolicyId, entity.Status, entity.CreatedAt, entity.UpdatedAt);
    private static RetentionPolicyResponse ToResponse(RetentionPolicyEntity entity) => new(entity.Id, entity.PolicyCode, entity.AppliesTo, entity.RetentionPeriodDays, entity.ArchiveRule, entity.Status, entity.CreatedAt, entity.UpdatedAt);
    private static ArchitectureRecordResponse ToResponse(ArchitectureRecordEntity entity, string? projectName) => new(entity.Id, entity.ProjectId, projectName, entity.Title, entity.ArchitectureType, entity.OwnerUserId, entity.Status, entity.CurrentVersionId, entity.Summary, entity.SecurityImpact, entity.EvidenceRef, entity.ApprovedBy, entity.ApprovedAt, entity.CreatedAt, entity.UpdatedAt);
    private static DesignReviewResponse ToResponse(DesignReviewEntity entity, string? architectureTitle) => new(entity.Id, entity.ArchitectureRecordId, architectureTitle, entity.ReviewType, entity.ReviewedBy, entity.Status, entity.DecisionReason, entity.DesignSummary, entity.Concerns, entity.EvidenceRef, entity.DecidedAt, entity.CreatedAt, entity.UpdatedAt);
    private static IntegrationReviewResponse ToResponse(IntegrationReviewEntity entity) => new(entity.Id, entity.ScopeRef, entity.IntegrationType, entity.ReviewedBy, entity.Status, entity.DecisionReason, entity.Risks, entity.DependencyImpact, entity.EvidenceRef, entity.DecidedAt, entity.AppliedAt, entity.CreatedAt, entity.UpdatedAt);
}
