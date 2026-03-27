using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Governance.Contracts;
using Operis_API.Modules.Governance.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Governance.Application;

public sealed class GovernanceOperationsCommands(OperisDbContext dbContext, IAuditLogWriter auditLogWriter) : IGovernanceOperationsCommands
{
    public async Task<GovernanceCommandResult<ComplianceDashboardPreferenceResponse>> UpdateComplianceDashboardPreferencesAsync(UpdateComplianceDashboardPreferencesRequest request, string userId, string? actor, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Validation<ComplianceDashboardPreferenceResponse>("Dashboard scope is required.");
        }

        if (request.DefaultPeriodDays is < 7 or > 365)
        {
            return Validation<ComplianceDashboardPreferenceResponse>("Dashboard period is invalid.", "compliance_dashboard_period_invalid");
        }

        if (!string.IsNullOrWhiteSpace(request.DefaultProcessArea) && !IsSupportedComplianceProcessArea(request.DefaultProcessArea))
        {
            return Validation<ComplianceDashboardPreferenceResponse>("Process area is invalid.", "compliance_dashboard_process_area_invalid");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = await dbContext.ComplianceDashboardPreferences.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (entity is null)
        {
            entity = new ComplianceDashboardPreferenceEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = now,
                UpdatedAt = now
            };
            dbContext.ComplianceDashboardPreferences.Add(entity);
        }

        entity.DefaultProjectId = request.DefaultProjectId;
        entity.DefaultProcessArea = Optional(request.DefaultProcessArea, 128);
        entity.DefaultPeriodDays = request.DefaultPeriodDays;
        entity.DefaultShowOnlyAtRisk = request.DefaultShowOnlyAtRisk;
        entity.UpdatedAt = now;

        auditLogWriter.Append(new AuditLogEntry(
            Module: "governance",
            Action: "update_preferences",
            EntityType: "compliance_dashboard_preference",
            EntityId: entity.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            ActorUserId: userId,
            ActorEmail: actor,
            After: ToResponse(entity)));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity));
    }

    public async Task<GovernanceCommandResult<ManagementReviewDetailResponse>> CreateManagementReviewAsync(CreateManagementReviewRequest request, string? actor, CancellationToken cancellationToken)
    {
        var validationError = await ValidateManagementReviewRequestAsync(
            request.ProjectId,
            request.ReviewCode,
            request.Title,
            request.ReviewPeriod,
            request.ScheduledAt,
            request.FacilitatorUserId,
            request.Items,
            request.Actions,
            cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var reviewCode = Required(request.ReviewCode, 128)!;
        if (await dbContext.ManagementReviews.AnyAsync(x => x.ReviewCode == reviewCode, cancellationToken))
        {
            return new GovernanceCommandResult<ManagementReviewDetailResponse>(
                GovernanceCommandStatus.Conflict,
                default,
                "Management review code already exists.",
                ApiErrorCodes.ManagementReviewCodeDuplicate);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new ManagementReviewEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            ReviewCode = reviewCode,
            Title = Required(request.Title, 512)!,
            ReviewPeriod = Required(request.ReviewPeriod, 128)!,
            ScheduledAt = request.ScheduledAt,
            FacilitatorUserId = Required(request.FacilitatorUserId, 128)!,
            Status = "draft",
            AgendaSummary = Optional(request.AgendaSummary, 4000),
            MinutesSummary = Optional(request.MinutesSummary, 4000),
            DecisionSummary = Optional(request.DecisionSummary, 4000),
            EscalationEntityType = Optional(request.EscalationEntityType, 128),
            EscalationEntityId = Optional(request.EscalationEntityId, 128),
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.ManagementReviews.Add(entity);
        ReplaceManagementReviewItems(entity.Id, request.Items, now);
        ReplaceManagementReviewActions(entity.Id, request.Actions, now);
        await PersistWithAuditAsync("create", "management_review", entity.Id.ToString(), StatusCodes.Status201Created, entity, cancellationToken);
        return await SuccessManagementReviewAsync(entity.Id, cancellationToken);
    }

    public async Task<GovernanceCommandResult<ManagementReviewDetailResponse>> UpdateManagementReviewAsync(Guid id, UpdateManagementReviewRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ManagementReviews.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return new GovernanceCommandResult<ManagementReviewDetailResponse>(
                GovernanceCommandStatus.NotFound,
                default,
                "Management review not found.",
                ApiErrorCodes.ManagementReviewNotFound);
        }

        if (string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase) || string.Equals(entity.Status, "archived", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<ManagementReviewDetailResponse>("Closed or archived management reviews cannot be edited.", ApiErrorCodes.InvalidWorkflowTransition);
        }

        var validationError = await ValidateManagementReviewRequestAsync(
            request.ProjectId,
            request.ReviewCode,
            request.Title,
            request.ReviewPeriod,
            request.ScheduledAt,
            request.FacilitatorUserId,
            request.Items,
            request.Actions,
            cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var reviewCode = Required(request.ReviewCode, 128)!;
        if (!string.Equals(entity.ReviewCode, reviewCode, StringComparison.OrdinalIgnoreCase)
            && await dbContext.ManagementReviews.AnyAsync(x => x.ReviewCode == reviewCode, cancellationToken))
        {
            return new GovernanceCommandResult<ManagementReviewDetailResponse>(
                GovernanceCommandStatus.Conflict,
                default,
                "Management review code already exists.",
                ApiErrorCodes.ManagementReviewCodeDuplicate);
        }

        entity.ProjectId = request.ProjectId;
        entity.ReviewCode = reviewCode;
        entity.Title = Required(request.Title, 512)!;
        entity.ReviewPeriod = Required(request.ReviewPeriod, 128)!;
        entity.ScheduledAt = request.ScheduledAt;
        entity.FacilitatorUserId = Required(request.FacilitatorUserId, 128)!;
        entity.AgendaSummary = Optional(request.AgendaSummary, 4000);
        entity.MinutesSummary = Optional(request.MinutesSummary, 4000);
        entity.DecisionSummary = Optional(request.DecisionSummary, 4000);
        entity.EscalationEntityType = Optional(request.EscalationEntityType, 128);
        entity.EscalationEntityId = Optional(request.EscalationEntityId, 128);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        ReplaceManagementReviewItems(id, request.Items, entity.UpdatedAt);
        ReplaceManagementReviewActions(id, request.Actions, entity.UpdatedAt);
        await PersistWithAuditAsync("update", "management_review", entity.Id.ToString(), StatusCodes.Status200OK, entity, cancellationToken);
        return await SuccessManagementReviewAsync(entity.Id, cancellationToken);
    }

    public async Task<GovernanceCommandResult<ManagementReviewDetailResponse>> TransitionManagementReviewAsync(Guid id, TransitionManagementReviewRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ManagementReviews.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return new GovernanceCommandResult<ManagementReviewDetailResponse>(
                GovernanceCommandStatus.NotFound,
                default,
                "Management review not found.",
                ApiErrorCodes.ManagementReviewNotFound);
        }

        var targetStatus = NormalizeManagementReviewStatus(request.TargetStatus);
        if (!IsAllowedManagementReviewTransition(entity.Status, targetStatus))
        {
            return Validation<ManagementReviewDetailResponse>("Invalid workflow transition.", ApiErrorCodes.InvalidWorkflowTransition);
        }

        if (targetStatus is "scheduled" or "in_review" && entity.ScheduledAt == default)
        {
            return Validation<ManagementReviewDetailResponse>(
                "Management review schedule is required.",
                ApiErrorCodes.ManagementReviewScheduleRequired);
        }

        if (targetStatus == "closed")
        {
            if (string.IsNullOrWhiteSpace(entity.MinutesSummary) || string.IsNullOrWhiteSpace(entity.DecisionSummary))
            {
                return Validation<ManagementReviewDetailResponse>(
                    "Management review minutes and decision summary are required before close.",
                    ApiErrorCodes.ManagementReviewMinutesRequired);
            }

            var openMandatoryActions = await dbContext.ManagementReviewActions
                .AsNoTracking()
                .CountAsync(
                    x => x.ReviewId == id
                         && x.IsMandatory
                         && !string.Equals(x.Status, "closed", StringComparison.OrdinalIgnoreCase),
                    cancellationToken);
            if (openMandatoryActions > 0)
            {
                return Validation<ManagementReviewDetailResponse>(
                    "Management review cannot close while mandatory actions remain open.",
                    ApiErrorCodes.ManagementReviewOpenActionsBlockClose);
            }
        }

        entity.Status = targetStatus;
        entity.ClosedBy = targetStatus == "closed" ? actor ?? "unknown" : null;
        entity.ClosedAt = targetStatus == "closed" ? DateTimeOffset.UtcNow : null;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        WriteApprovalEvidenceIfNeeded("management_review", entity.Id, targetStatus, actor, request.Reason);
        await PersistWithAuditAsync("transition", "management_review", entity.Id.ToString(), StatusCodes.Status200OK, entity, cancellationToken);
        return await SuccessManagementReviewAsync(entity.Id, cancellationToken);
    }

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

    private async Task<GovernanceCommandResult<ManagementReviewDetailResponse>?> ValidateManagementReviewRequestAsync(
        Guid? projectId,
        string? reviewCode,
        string? title,
        string? reviewPeriod,
        DateTimeOffset scheduledAt,
        string? facilitatorUserId,
        IReadOnlyList<ManagementReviewItemInput>? items,
        IReadOnlyList<ManagementReviewActionInput>? actions,
        CancellationToken cancellationToken)
    {
        if (Required(reviewCode, 128) is null
            || Required(title, 512) is null
            || Required(reviewPeriod, 128) is null
            || Required(facilitatorUserId, 128) is null)
        {
            return Validation<ManagementReviewDetailResponse>("Review code, title, review period, and facilitator are required.");
        }

        if (scheduledAt == default)
        {
            return Validation<ManagementReviewDetailResponse>(
                "Management review schedule is required.",
                ApiErrorCodes.ManagementReviewScheduleRequired);
        }

        if (projectId.HasValue)
        {
            var exists = await dbContext.Projects.AsNoTracking().AnyAsync(x => x.Id == projectId.Value, cancellationToken);
            if (!exists)
            {
                return NotFound<ManagementReviewDetailResponse>();
            }
        }

        if (items is not null && items.Any(item => Required(item.Title, 512) is null))
        {
            return Validation<ManagementReviewDetailResponse>("Management review item title is required.");
        }

        if (actions is not null && actions.Any(action => Required(action.Title, 512) is null || Required(action.OwnerUserId, 128) is null))
        {
            return Validation<ManagementReviewDetailResponse>("Management review actions require title and owner.");
        }

        return null;
    }

    private void ReplaceManagementReviewItems(Guid reviewId, IReadOnlyList<ManagementReviewItemInput>? items, DateTimeOffset now)
    {
        var existing = dbContext.ManagementReviewItems.Where(x => x.ReviewId == reviewId);
        dbContext.ManagementReviewItems.RemoveRange(existing);

        if (items is null)
        {
            return;
        }

        foreach (var item in items)
        {
            dbContext.ManagementReviewItems.Add(new ManagementReviewItemEntity
            {
                Id = Guid.NewGuid(),
                ReviewId = reviewId,
                ItemType = NormalizeManagementReviewItemType(item.ItemType),
                Title = Required(item.Title, 512)!,
                Summary = Optional(item.Summary, 4000),
                Decision = Optional(item.Decision, 2000),
                OwnerUserId = Optional(item.OwnerUserId, 128),
                DueAt = item.DueAt,
                Status = NormalizeManagementReviewItemStatus(item.Status),
                CreatedAt = now,
                UpdatedAt = now
            });
        }
    }

    private void ReplaceManagementReviewActions(Guid reviewId, IReadOnlyList<ManagementReviewActionInput>? actions, DateTimeOffset now)
    {
        var existing = dbContext.ManagementReviewActions.Where(x => x.ReviewId == reviewId);
        dbContext.ManagementReviewActions.RemoveRange(existing);

        if (actions is null)
        {
            return;
        }

        foreach (var action in actions)
        {
            var status = NormalizeManagementReviewActionStatus(action.Status);
            dbContext.ManagementReviewActions.Add(new ManagementReviewActionEntity
            {
                Id = Guid.NewGuid(),
                ReviewId = reviewId,
                Title = Required(action.Title, 512)!,
                Description = Optional(action.Description, 4000),
                OwnerUserId = Required(action.OwnerUserId, 128)!,
                DueAt = action.DueAt,
                Status = status,
                IsMandatory = action.IsMandatory,
                LinkedEntityType = Optional(action.LinkedEntityType, 128),
                LinkedEntityId = Optional(action.LinkedEntityId, 128),
                ClosedAt = status == "closed" ? now : null,
                CreatedAt = now,
                UpdatedAt = now
            });
        }
    }

    private async Task<GovernanceCommandResult<ManagementReviewDetailResponse>> SuccessManagementReviewAsync(Guid reviewId, CancellationToken cancellationToken)
    {
        var detail = await BuildManagementReviewDetailAsync(reviewId, cancellationToken);
        return detail is null
            ? new GovernanceCommandResult<ManagementReviewDetailResponse>(GovernanceCommandStatus.NotFound, default, "Management review not found.", ApiErrorCodes.ManagementReviewNotFound)
            : Success(detail);
    }

    private async Task<ManagementReviewDetailResponse?> BuildManagementReviewDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        var review = await (
            from managementReview in dbContext.ManagementReviews.AsNoTracking()
            where managementReview.Id == id
            join project in dbContext.Projects.AsNoTracking() on managementReview.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            select new
            {
                Review = managementReview,
                ProjectName = project == null ? null : project.Name
            }).SingleOrDefaultAsync(cancellationToken);

        if (review is null)
        {
            return null;
        }

        var items = await dbContext.ManagementReviewItems.AsNoTracking()
            .Where(x => x.ReviewId == id)
            .OrderBy(x => x.ItemType)
            .ThenBy(x => x.Title)
            .Select(x => new ManagementReviewItemResponse(x.Id, x.ItemType, x.Title, x.Summary, x.Decision, x.OwnerUserId, x.DueAt, x.Status, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var actions = await dbContext.ManagementReviewActions.AsNoTracking()
            .Where(x => x.ReviewId == id)
            .OrderBy(x => x.Status)
            .ThenBy(x => x.DueAt)
            .ThenBy(x => x.Title)
            .Select(x => new ManagementReviewActionResponse(x.Id, x.Title, x.Description, x.OwnerUserId, x.DueAt, x.Status, x.IsMandatory, x.LinkedEntityType, x.LinkedEntityId, x.ClosedAt, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var history = await dbContext.BusinessAuditEvents.AsNoTracking()
            .Where(x => x.EntityType == "management_review" && x.EntityId == id.ToString())
            .OrderByDescending(x => x.OccurredAt)
            .Select(x => new WorkflowOverrideLogResponse(x.Id, x.EntityType, x.EntityId ?? string.Empty, x.ActorUserId ?? "unknown", x.ActorUserId ?? "unknown", x.Reason ?? x.Summary ?? string.Empty, x.OccurredAt))
            .ToListAsync(cancellationToken);

        return new ManagementReviewDetailResponse(
            review.Review.Id,
            review.Review.ProjectId,
            review.ProjectName,
            review.Review.ReviewCode,
            review.Review.Title,
            review.Review.ReviewPeriod,
            review.Review.ScheduledAt,
            review.Review.FacilitatorUserId,
            review.Review.Status,
            review.Review.AgendaSummary,
            review.Review.MinutesSummary,
            review.Review.DecisionSummary,
            review.Review.EscalationEntityType,
            review.Review.EscalationEntityId,
            review.Review.ClosedBy,
            review.Review.ClosedAt,
            items,
            actions,
            history,
            review.Review.CreatedAt,
            review.Review.UpdatedAt);
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

    private static bool IsAllowedManagementReviewTransition(string currentStatus, string nextStatus)
    {
        if (string.Equals(currentStatus, nextStatus, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return currentStatus switch
        {
            "draft" => nextStatus == "scheduled",
            "scheduled" => nextStatus == "in_review",
            "in_review" => nextStatus == "closed",
            "closed" => nextStatus == "archived",
            _ => false
        };
    }

    private static bool IsSupportedComplianceProcessArea(string value) => value.Trim().ToLowerInvariant() is
        "process-assets-planning" or
        "requirements-traceability" or
        "document-governance" or
        "change-configuration" or
        "verification-release" or
        "audit-capa" or
        "security-resilience";

    private static string? Required(string? value, int maxLength) => string.IsNullOrWhiteSpace(value) ? null : Trim(value, maxLength);
    private static string? Optional(string? value, int maxLength) => string.IsNullOrWhiteSpace(value) ? null : Trim(value, maxLength);
    private static string Trim(string value, int maxLength) => value.Trim().Length > maxLength ? value.Trim()[..maxLength] : value.Trim();
    private static string? NormalizeResponsibility(string? value) => value?.Trim().ToUpperInvariant() switch { "R" or "A" or "C" or "I" => value.Trim().ToUpperInvariant(), _ => null };
    private static string NormalizeGovernanceStatus(string? value) => value?.Trim().ToLowerInvariant() switch { "approved" => "approved", "active" => "active", "archived" => "archived", _ => "draft" };
    private static string NormalizeArchitectureStatus(string? value) => value?.Trim().ToLowerInvariant() switch { "reviewed" => "reviewed", "approved" => "approved", "active" => "active", "superseded" => "superseded", _ => "draft" };
    private static string NormalizeDesignReviewStatus(string? value) => value?.Trim().ToLowerInvariant() switch { "in_review" => "in_review", "approved" => "approved", "rejected" => "rejected", "baseline" => "baseline", _ => "draft" };
    private static string NormalizeIntegrationReviewStatus(string? value) => value?.Trim().ToLowerInvariant() switch { "in_review" => "in_review", "approved" => "approved", "rejected" => "rejected", "applied" => "applied", _ => "draft" };
    private static string NormalizeManagementReviewStatus(string? value) => value?.Trim().ToLowerInvariant() switch { "scheduled" => "scheduled", "in_review" => "in_review", "closed" => "closed", "archived" => "archived", _ => "draft" };
    private static string NormalizeManagementReviewItemType(string? value) => value?.Trim().ToLowerInvariant() switch { "decision" => "decision", "risk" => "risk", "issue" => "issue", _ => "agenda" };
    private static string NormalizeManagementReviewItemStatus(string? value) => value?.Trim().ToLowerInvariant() switch { "closed" => "closed", "noted" => "noted", _ => "open" };
    private static string NormalizeManagementReviewActionStatus(string? value) => value?.Trim().ToLowerInvariant() switch { "in_progress" => "in_progress", "closed" => "closed", _ => "open" };
    private static bool RequiresSecurityEvidence(string architectureType) => architectureType.Contains("security", StringComparison.OrdinalIgnoreCase);
    private static GovernanceCommandResult<T> Success<T>(T value) => new(GovernanceCommandStatus.Success, value);
    private static GovernanceCommandResult<T> Validation<T>(string message, string? code = null) => new(GovernanceCommandStatus.ValidationError, default, message, code ?? ApiErrorCodes.RequestValidationFailed);
    private static GovernanceCommandResult<T> NotFound<T>() => new(GovernanceCommandStatus.NotFound, default, "Resource not found.", ApiErrorCodes.ResourceNotFound);
    private static GovernanceCommandResult<T> Conflict<T>(string message) => new(GovernanceCommandStatus.Conflict, default, message, ApiErrorCodes.RequestValidationFailed);
    private static RaciMapResponse ToResponse(RaciMapEntity entity) => new(entity.Id, entity.ProcessCode, entity.RoleName, entity.ResponsibilityType, entity.Status, entity.CreatedAt, entity.UpdatedAt);
    private static SlaRuleResponse ToResponse(SlaRuleEntity entity) => new(entity.Id, entity.ScopeType, entity.ScopeRef, entity.TargetDurationHours, entity.EscalationPolicyId, entity.Status, entity.CreatedAt, entity.UpdatedAt);
    private static RetentionPolicyResponse ToResponse(RetentionPolicyEntity entity) => new(entity.Id, entity.PolicyCode, entity.AppliesTo, entity.RetentionPeriodDays, entity.ArchiveRule, entity.Status, entity.CreatedAt, entity.UpdatedAt);
    private static ComplianceDashboardPreferenceResponse ToResponse(ComplianceDashboardPreferenceEntity entity) => new(entity.Id, entity.UserId, entity.DefaultProjectId, entity.DefaultProcessArea, entity.DefaultPeriodDays, entity.DefaultShowOnlyAtRisk, entity.UpdatedAt);
    private static ArchitectureRecordResponse ToResponse(ArchitectureRecordEntity entity, string? projectName) => new(entity.Id, entity.ProjectId, projectName, entity.Title, entity.ArchitectureType, entity.OwnerUserId, entity.Status, entity.CurrentVersionId, entity.Summary, entity.SecurityImpact, entity.EvidenceRef, entity.ApprovedBy, entity.ApprovedAt, entity.CreatedAt, entity.UpdatedAt);
    private static DesignReviewResponse ToResponse(DesignReviewEntity entity, string? architectureTitle) => new(entity.Id, entity.ArchitectureRecordId, architectureTitle, entity.ReviewType, entity.ReviewedBy, entity.Status, entity.DecisionReason, entity.DesignSummary, entity.Concerns, entity.EvidenceRef, entity.DecidedAt, entity.CreatedAt, entity.UpdatedAt);
    private static IntegrationReviewResponse ToResponse(IntegrationReviewEntity entity) => new(entity.Id, entity.ScopeRef, entity.IntegrationType, entity.ReviewedBy, entity.Status, entity.DecisionReason, entity.Risks, entity.DependencyImpact, entity.EvidenceRef, entity.DecidedAt, entity.AppliedAt, entity.CreatedAt, entity.UpdatedAt);
}
