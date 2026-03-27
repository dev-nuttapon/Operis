using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Operations.Contracts;
using Operis_API.Modules.Operations.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Operations.Application;

public sealed class OperationsCommands(OperisDbContext dbContext, IAuditLogWriter auditLogWriter) : IOperationsCommands
{
    private static readonly string[] RecertificationStatuses = ["planned", "in_review", "approved", "completed"];
    private static readonly string[] RecertificationDecisions = ["kept", "revoked", "adjusted"];
    private static readonly string[] BackupEvidenceStatuses = ["planned", "completed", "verified", "archived"];
    private static readonly string[] RestoreVerificationStatuses = ["planned", "executed", "verified", "closed"];
    private static readonly string[] DrDrillStatuses = ["planned", "executed", "findings_issued", "closed"];
    private static readonly string[] LegalHoldStatuses = ["active", "released", "archived"];

    public async Task<OperationsCommandResult<AccessReviewResponse>> CreateAccessReviewAsync(CreateAccessReviewRequest request, string? actor, CancellationToken cancellationToken)
    {
        var scopeType = Required(request.ScopeType, 64);
        var scopeRef = Required(request.ScopeRef, 256);
        var reviewCycle = Required(request.ReviewCycle, 64);
        if (scopeType is null || scopeRef is null || reviewCycle is null)
        {
            return Validation<AccessReviewResponse>("Scope and review cycle are required.");
        }

        var entity = new AccessReviewEntity
        {
            Id = Guid.NewGuid(),
            ScopeType = scopeType,
            ScopeRef = scopeRef,
            ReviewCycle = reviewCycle,
            ReviewedBy = Optional(request.ReviewedBy, 64),
            Status = "Scheduled",
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.AccessReviews.Add(entity);
        AppendAudit("create", "access_review", entity.Id.ToString(), StatusCodes.Status201Created, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity), created: true);
    }

    public async Task<OperationsCommandResult<AccessReviewResponse>> UpdateAccessReviewAsync(Guid id, UpdateAccessReviewRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.AccessReviews.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound<AccessReviewResponse>();

        entity.ScopeType = Required(request.ScopeType, 64) ?? entity.ScopeType;
        entity.ScopeRef = Required(request.ScopeRef, 256) ?? entity.ScopeRef;
        entity.ReviewCycle = Required(request.ReviewCycle, 64) ?? entity.ReviewCycle;
        entity.ReviewedBy = Optional(request.ReviewedBy, 64);
        entity.Status = NormalizeAccessStatus(request.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        AppendAudit("update", "access_review", entity.Id.ToString(), StatusCodes.Status200OK, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity));
    }

    public async Task<OperationsCommandResult<AccessReviewResponse>> ApproveAccessReviewAsync(Guid id, ApproveAccessReviewRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.AccessReviews.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound<AccessReviewResponse>();

        var decision = Required(request.Decision, 64);
        var rationale = Required(request.DecisionRationale, 2000);
        if (decision is null) return Validation<AccessReviewResponse>("Decision is required.", ApiErrorCodes.AccessReviewDecisionRequired);
        if (rationale is null) return Validation<AccessReviewResponse>("Decision rationale is required.", ApiErrorCodes.AccessReviewRationaleRequired);
        if (entity.Status is not ("Scheduled" or "In Review")) return Validation<AccessReviewResponse>("Invalid workflow transition.", ApiErrorCodes.InvalidWorkflowTransition);

        entity.Decision = decision;
        entity.DecisionRationale = rationale;
        entity.Status = "Approved";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        AppendAudit("approve", "access_review", entity.Id.ToString(), StatusCodes.Status200OK, rationale, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity));
    }

    public async Task<OperationsCommandResult<SecurityReviewResponse>> CreateSecurityReviewAsync(CreateSecurityReviewRequest request, string? actor, CancellationToken cancellationToken)
    {
        var scopeType = Required(request.ScopeType, 64);
        var scopeRef = Required(request.ScopeRef, 256);
        var controlsReviewed = Required(request.ControlsReviewed, 2000);
        if (scopeType is null || scopeRef is null || controlsReviewed is null)
        {
            return Validation<SecurityReviewResponse>("Scope and controls reviewed are required.");
        }

        var entity = new SecurityReviewEntity
        {
            Id = Guid.NewGuid(),
            ScopeType = scopeType,
            ScopeRef = scopeRef,
            ControlsReviewed = controlsReviewed,
            FindingsSummary = Optional(request.FindingsSummary, 2000),
            Status = NormalizeSecurityStatus(request.Status),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.SecurityReviews.Add(entity);
        AppendAudit("create", "security_review", entity.Id.ToString(), StatusCodes.Status201Created, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity), created: true);
    }

    public async Task<OperationsCommandResult<SecurityReviewResponse>> UpdateSecurityReviewAsync(Guid id, UpdateSecurityReviewRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.SecurityReviews.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound<SecurityReviewResponse>();

        entity.ScopeType = Required(request.ScopeType, 64) ?? entity.ScopeType;
        entity.ScopeRef = Required(request.ScopeRef, 256) ?? entity.ScopeRef;
        entity.ControlsReviewed = Required(request.ControlsReviewed, 2000) ?? entity.ControlsReviewed;
        entity.FindingsSummary = Optional(request.FindingsSummary, 2000);
        entity.Status = NormalizeSecurityStatus(request.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        AppendAudit("update", "security_review", entity.Id.ToString(), StatusCodes.Status200OK, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity));
    }

    public async Task<OperationsCommandResult<ExternalDependencyResponse>> CreateExternalDependencyAsync(CreateExternalDependencyRequest request, string? actor, CancellationToken cancellationToken)
    {
        var name = Required(request.Name, 256);
        var dependencyType = Required(request.DependencyType, 128);
        var ownerUserId = Required(request.OwnerUserId, 64);
        var criticality = Required(request.Criticality, 32);
        if (name is null || dependencyType is null) return Validation<ExternalDependencyResponse>("Dependency name and type are required.");
        if (ownerUserId is null) return Validation<ExternalDependencyResponse>("Dependency owner is required.", ApiErrorCodes.DependencyOwnerRequired);
        if (criticality is null) return Validation<ExternalDependencyResponse>("Dependency criticality is required.", ApiErrorCodes.DependencyCriticalityRequired);
        if (request.SupplierId.HasValue && !await dbContext.Suppliers.AnyAsync(x => x.Id == request.SupplierId.Value, cancellationToken))
        {
            return NotFound<ExternalDependencyResponse>();
        }

        var entity = new ExternalDependencyEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            DependencyType = dependencyType,
            SupplierId = request.SupplierId,
            OwnerUserId = ownerUserId,
            Criticality = criticality.ToLowerInvariant(),
            ReviewDueAt = request.ReviewDueAt,
            Status = NormalizeDependencyStatus(request.Status),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.ExternalDependencies.Add(entity);
        AppendAudit("create", "external_dependency", entity.Id.ToString(), StatusCodes.Status201Created, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(await ToResponseAsync(entity, cancellationToken), created: true);
    }

    public async Task<OperationsCommandResult<ExternalDependencyResponse>> UpdateExternalDependencyAsync(Guid id, UpdateExternalDependencyRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ExternalDependencies.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound<ExternalDependencyResponse>();

        var ownerUserId = Required(request.OwnerUserId, 64);
        var criticality = Required(request.Criticality, 32);
        if (ownerUserId is null) return Validation<ExternalDependencyResponse>("Dependency owner is required.", ApiErrorCodes.DependencyOwnerRequired);
        if (criticality is null) return Validation<ExternalDependencyResponse>("Dependency criticality is required.", ApiErrorCodes.DependencyCriticalityRequired);
        if (request.SupplierId.HasValue && !await dbContext.Suppliers.AnyAsync(x => x.Id == request.SupplierId.Value, cancellationToken))
        {
            return NotFound<ExternalDependencyResponse>();
        }

        entity.Name = Required(request.Name, 256) ?? entity.Name;
        entity.DependencyType = Required(request.DependencyType, 128) ?? entity.DependencyType;
        entity.SupplierId = request.SupplierId;
        entity.OwnerUserId = ownerUserId;
        entity.Criticality = criticality.ToLowerInvariant();
        entity.ReviewDueAt = request.ReviewDueAt;
        entity.Status = NormalizeDependencyStatus(request.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        AppendAudit("update", "external_dependency", entity.Id.ToString(), StatusCodes.Status200OK, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(await ToResponseAsync(entity, cancellationToken));
    }

    public async Task<OperationsCommandResult<ConfigurationAuditResponse>> CreateConfigurationAuditAsync(CreateConfigurationAuditRequest request, string? actor, CancellationToken cancellationToken)
    {
        var scopeRef = Required(request.ScopeRef, 256);
        if (scopeRef is null) return Validation<ConfigurationAuditResponse>("Scope reference is required.");

        var entity = new ConfigurationAuditEntity
        {
            Id = Guid.NewGuid(),
            ScopeRef = scopeRef,
            PlannedAt = request.PlannedAt,
            Status = NormalizeAuditStatus(request.Status),
            FindingCount = Math.Max(0, request.FindingCount),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.ConfigurationAudits.Add(entity);
        AppendAudit("create", "configuration_audit", entity.Id.ToString(), StatusCodes.Status201Created, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity), created: true);
    }

    public async Task<OperationsCommandResult<SupplierResponse>> CreateSupplierAsync(CreateSupplierRequest request, string? actor, CancellationToken cancellationToken)
    {
        var name = Required(request.Name, 256);
        var supplierType = Required(request.SupplierType, 128);
        var ownerUserId = Required(request.OwnerUserId, 64);
        var criticality = Required(request.Criticality, 32);
        if (name is null || supplierType is null || ownerUserId is null || criticality is null)
        {
            return Validation<SupplierResponse>("Supplier name, type, owner, and criticality are required.");
        }

        var entity = new SupplierEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            SupplierType = supplierType,
            OwnerUserId = ownerUserId,
            Criticality = criticality.ToLowerInvariant(),
            ReviewDueAt = request.ReviewDueAt,
            Status = NormalizeSupplierStatus(request.Status),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Suppliers.Add(entity);
        AppendAudit("create", "supplier", entity.Id.ToString(), StatusCodes.Status201Created, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity, 0), created: true);
    }

    public async Task<OperationsCommandResult<SupplierResponse>> UpdateSupplierAsync(Guid id, UpdateSupplierRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Suppliers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound<SupplierResponse>();

        var ownerUserId = Required(request.OwnerUserId, 64);
        var criticality = Required(request.Criticality, 32);
        if (ownerUserId is null) return Validation<SupplierResponse>("Supplier owner is required.");
        if (criticality is null) return Validation<SupplierResponse>("Supplier criticality is required.");

        var nextStatus = NormalizeSupplierStatus(request.Status);
        if (nextStatus == "Archived")
        {
            var hasActiveAgreements = await dbContext.SupplierAgreements.AnyAsync(
                x => x.SupplierId == id && x.Status == "Active",
                cancellationToken);

            if (hasActiveAgreements)
            {
                return Validation<SupplierResponse>(
                    "Supplier cannot be archived while an active agreement exists.",
                    ApiErrorCodes.SupplierActiveAgreementExists);
            }
        }

        entity.Name = Required(request.Name, 256) ?? entity.Name;
        entity.SupplierType = Required(request.SupplierType, 128) ?? entity.SupplierType;
        entity.OwnerUserId = ownerUserId;
        entity.Criticality = criticality.ToLowerInvariant();
        entity.ReviewDueAt = request.ReviewDueAt;
        entity.Status = nextStatus;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        var activeAgreementCount = await dbContext.SupplierAgreements.CountAsync(x => x.SupplierId == id && x.Status == "Active", cancellationToken);
        AppendAudit("update", "supplier", entity.Id.ToString(), StatusCodes.Status200OK, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity, activeAgreementCount));
    }

    public async Task<OperationsCommandResult<SupplierAgreementResponse>> CreateSupplierAgreementAsync(CreateSupplierAgreementRequest request, string? actor, CancellationToken cancellationToken)
    {
        var supplier = await dbContext.Suppliers.FirstOrDefaultAsync(x => x.Id == request.SupplierId, cancellationToken);
        if (supplier is null) return NotFound<SupplierAgreementResponse>();

        var validation = ValidateAgreement(request.EffectiveFrom, request.EffectiveTo, request.EvidenceRef);
        if (validation is not null) return validation;

        var entity = new SupplierAgreementEntity
        {
            Id = Guid.NewGuid(),
            SupplierId = request.SupplierId,
            AgreementType = Required(request.AgreementType, 128) ?? "contract",
            EffectiveFrom = request.EffectiveFrom!.Value,
            EffectiveTo = request.EffectiveTo,
            SlaTerms = Required(request.SlaTerms, 2000) ?? string.Empty,
            EvidenceRef = Required(request.EvidenceRef, 512)!,
            Status = NormalizeAgreementStatus(request.Status),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.SupplierAgreements.Add(entity);
        AppendAudit("create", "supplier_agreement", entity.Id.ToString(), StatusCodes.Status201Created, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity, supplier.Name), created: true);
    }

    public async Task<OperationsCommandResult<SupplierAgreementResponse>> UpdateSupplierAgreementAsync(Guid id, UpdateSupplierAgreementRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.SupplierAgreements.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound<SupplierAgreementResponse>();

        var supplier = await dbContext.Suppliers.FirstOrDefaultAsync(x => x.Id == request.SupplierId, cancellationToken);
        if (supplier is null) return NotFound<SupplierAgreementResponse>();

        var validation = ValidateAgreement(request.EffectiveFrom, request.EffectiveTo, request.EvidenceRef);
        if (validation is not null) return validation;

        entity.SupplierId = request.SupplierId;
        entity.AgreementType = Required(request.AgreementType, 128) ?? entity.AgreementType;
        entity.EffectiveFrom = request.EffectiveFrom!.Value;
        entity.EffectiveTo = request.EffectiveTo;
        entity.SlaTerms = Required(request.SlaTerms, 2000) ?? string.Empty;
        entity.EvidenceRef = Required(request.EvidenceRef, 512)!;
        entity.Status = NormalizeAgreementStatus(request.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        AppendAudit("update", "supplier_agreement", entity.Id.ToString(), StatusCodes.Status200OK, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity, supplier.Name));
    }

    public async Task<OperationsCommandResult<AccessRecertificationResponse>> CreateAccessRecertificationAsync(CreateAccessRecertificationRequest request, string? actor, CancellationToken cancellationToken)
    {
        var scopeType = Required(request.ScopeType, 64);
        var scopeRef = Required(request.ScopeRef, 256);
        var reviewOwnerUserId = Required(request.ReviewOwnerUserId, 128);
        if (scopeType is null || scopeRef is null || reviewOwnerUserId is null)
        {
            return Validation<AccessRecertificationResponse>("Scope and review owner are required.");
        }

        var entity = new AccessRecertificationScheduleEntity
        {
            Id = Guid.NewGuid(),
            ScopeType = scopeType.ToLowerInvariant(),
            ScopeRef = scopeRef,
            PlannedAt = request.PlannedAt,
            ReviewOwnerUserId = reviewOwnerUserId,
            Status = "planned",
            SubjectUsersJson = SerializeSubjects(request.SubjectUserIds),
            ExceptionNotes = Optional(request.ExceptionNotes, 2000),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.AccessRecertificationSchedules.Add(entity);
        AppendAudit("create", "access_recertification_schedule", entity.Id.ToString(), StatusCodes.Status201Created, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success((await GetAccessRecertificationAsync(entity.Id, cancellationToken))!, created: true);
    }

    public async Task<OperationsCommandResult<AccessRecertificationResponse>> UpdateAccessRecertificationAsync(Guid id, UpdateAccessRecertificationRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.AccessRecertificationSchedules.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound<AccessRecertificationResponse>();

        var scopeType = Required(request.ScopeType, 64);
        var scopeRef = Required(request.ScopeRef, 256);
        var reviewOwnerUserId = Required(request.ReviewOwnerUserId, 128);
        var nextStatus = NormalizeRecertificationStatus(request.Status);
        if (scopeType is null || scopeRef is null || reviewOwnerUserId is null)
        {
            return Validation<AccessRecertificationResponse>("Scope and review owner are required.");
        }

        if (!IsValidRecertificationTransition(entity.Status, nextStatus))
        {
            return Validation<AccessRecertificationResponse>("Invalid workflow transition.", ApiErrorCodes.InvalidWorkflowTransition);
        }

        var subjectUserIds = NormalizeSubjects(request.SubjectUserIds);
        var decisionsCount = await dbContext.AccessRecertificationDecisions.CountAsync(x => x.ScheduleId == id, cancellationToken);
        if ((nextStatus is "approved" or "completed") && subjectUserIds.Count > decisionsCount)
        {
            return Validation<AccessRecertificationResponse>(
                "Schedule cannot complete while pending decisions remain.",
                ApiErrorCodes.AccessRecertificationPendingDecisions);
        }

        entity.ScopeType = scopeType.ToLowerInvariant();
        entity.ScopeRef = scopeRef;
        entity.PlannedAt = request.PlannedAt;
        entity.ReviewOwnerUserId = reviewOwnerUserId;
        entity.Status = nextStatus;
        entity.SubjectUsersJson = SerializeSubjects(subjectUserIds);
        entity.ExceptionNotes = Optional(request.ExceptionNotes, 2000);
        entity.CompletedAt = nextStatus == "completed" ? entity.CompletedAt ?? DateTimeOffset.UtcNow : null;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        AppendAudit("update", "access_recertification_schedule", entity.Id.ToString(), StatusCodes.Status200OK, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success((await GetAccessRecertificationAsync(entity.Id, cancellationToken))!);
    }

    public async Task<OperationsCommandResult<AccessRecertificationDecisionResponse>> AddAccessRecertificationDecisionAsync(Guid id, AddAccessRecertificationDecisionRequest request, string? actor, CancellationToken cancellationToken)
    {
        var schedule = await dbContext.AccessRecertificationSchedules.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (schedule is null) return NotFound<AccessRecertificationDecisionResponse>();

        if (schedule.Status is "approved" or "completed")
        {
            return Validation<AccessRecertificationDecisionResponse>("Invalid workflow transition.", ApiErrorCodes.InvalidWorkflowTransition);
        }

        var subjectUserId = Required(request.SubjectUserId, 128);
        var decision = NormalizeRecertificationDecision(request.Decision);
        var reason = Required(request.Reason, 2000);
        if (subjectUserId is null || decision is null)
        {
            return Validation<AccessRecertificationDecisionResponse>("Subject and decision are required.");
        }

        var subjects = DeserializeSubjects(schedule.SubjectUsersJson);
        if (subjects.Count > 0 && !subjects.Contains(subjectUserId, StringComparer.OrdinalIgnoreCase))
        {
            return Validation<AccessRecertificationDecisionResponse>("Subject is outside the recertification scope.");
        }

        if (reason is null)
        {
            return Validation<AccessRecertificationDecisionResponse>(
                "Decision rationale is required.",
                ApiErrorCodes.AccessRecertificationDecisionRationaleRequired);
        }

        var entity = await dbContext.AccessRecertificationDecisions.FirstOrDefaultAsync(
            x => x.ScheduleId == id && x.SubjectUserId == subjectUserId,
            cancellationToken);

        if (entity is null)
        {
            entity = new AccessRecertificationDecisionEntity
            {
                Id = Guid.NewGuid(),
                ScheduleId = id,
                SubjectUserId = subjectUserId,
                Decision = decision,
                Reason = reason,
                DecidedBy = actor ?? "system",
                DecidedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow
            };
            dbContext.AccessRecertificationDecisions.Add(entity);
        }
        else
        {
            entity.Decision = decision;
            entity.Reason = reason;
            entity.DecidedBy = actor ?? "system";
            entity.DecidedAt = DateTimeOffset.UtcNow;
        }

        if (schedule.Status == "planned")
        {
            schedule.Status = "in_review";
            schedule.UpdatedAt = DateTimeOffset.UtcNow;
        }

        AppendAudit("decision", "access_recertification_decision", entity.Id.ToString(), StatusCodes.Status200OK, reason, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity));
    }

    public async Task<OperationsCommandResult<AccessRecertificationResponse>> CompleteAccessRecertificationAsync(Guid id, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.AccessRecertificationSchedules.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound<AccessRecertificationResponse>();

        if (entity.Status != "approved")
        {
            return Validation<AccessRecertificationResponse>("Invalid workflow transition.", ApiErrorCodes.InvalidWorkflowTransition);
        }

        var subjects = DeserializeSubjects(entity.SubjectUsersJson);
        var decisionsCount = await dbContext.AccessRecertificationDecisions.CountAsync(x => x.ScheduleId == id, cancellationToken);
        if (subjects.Count > decisionsCount)
        {
            return Validation<AccessRecertificationResponse>(
                "Schedule cannot complete while pending decisions remain.",
                ApiErrorCodes.AccessRecertificationPendingDecisions);
        }

        entity.Status = "completed";
        entity.CompletedAt = entity.CompletedAt ?? DateTimeOffset.UtcNow;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        AppendAudit("complete", "access_recertification_schedule", entity.Id.ToString(), StatusCodes.Status200OK, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success((await GetAccessRecertificationAsync(entity.Id, cancellationToken))!);
    }

    private OperationsCommandResult<SupplierAgreementResponse>? ValidateAgreement(DateOnly? effectiveFrom, DateOnly? effectiveTo, string? evidenceRef)
    {
        if (!effectiveFrom.HasValue)
        {
            return Validation<SupplierAgreementResponse>(
                "Agreement effective dates are required.",
                ApiErrorCodes.SupplierAgreementEffectiveDatesRequired);
        }

        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom.Value)
        {
            return Validation<SupplierAgreementResponse>(
                "Agreement effective dates are invalid.",
                ApiErrorCodes.SupplierAgreementEffectiveDatesRequired);
        }

        if (Required(evidenceRef, 512) is null)
        {
            return Validation<SupplierAgreementResponse>(
                "Agreement evidence is required.",
                ApiErrorCodes.SupplierAgreementEvidenceRequired);
        }

        return null;
    }

    private void AppendAudit(string action, string entityType, string entityId, int statusCode, string? reason = null, object? after = null)
    {
        auditLogWriter.Append(new AuditLogEntry(
            Module: "operations",
            Action: action,
            EntityType: entityType,
            EntityId: entityId,
            StatusCode: statusCode,
            Reason: reason,
            After: after));
    }

    public async Task<OperationsCommandResult<SecurityIncidentResponse>> CreateSecurityIncidentAsync(CreateSecurityIncidentRequest request, string? actor, CancellationToken cancellationToken)
    {
        var code = Required(request.Code, 128);
        var title = Required(request.Title, 512);
        var severity = NormalizeIncidentSeverity(request.Severity);
        var ownerUserId = Required(request.OwnerUserId, 128);
        if (code is null || title is null || severity is null || ownerUserId is null)
        {
            return Validation<SecurityIncidentResponse>("Code, title, severity, and owner are required.");
        }

        if (await dbContext.SecurityIncidents.AnyAsync(x => x.Code == code, cancellationToken))
        {
            return Validation<SecurityIncidentResponse>("Security incident code already exists.", ApiErrorCodes.SecurityIncidentCodeDuplicate);
        }

        if (request.ProjectId.HasValue && !await dbContext.Projects.AnyAsync(x => x.Id == request.ProjectId.Value, cancellationToken))
        {
            return NotFound<SecurityIncidentResponse>();
        }

        var entity = new SecurityIncidentEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Code = code,
            Title = title,
            Severity = severity,
            ReportedAt = request.ReportedAt,
            OwnerUserId = ownerUserId,
            Status = NormalizeIncidentStatus(request.Status),
            ResolutionSummary = Optional(request.ResolutionSummary, 4000),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.SecurityIncidents.Add(entity);
        AppendAudit("create", "security_incident", entity.Id.ToString(), StatusCodes.Status201Created, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(await ToResponseAsync(entity, cancellationToken), created: true);
    }

    public async Task<OperationsCommandResult<SecurityIncidentResponse>> UpdateSecurityIncidentAsync(Guid id, UpdateSecurityIncidentRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.SecurityIncidents.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound<SecurityIncidentResponse>();

        var code = Required(request.Code, 128);
        var title = Required(request.Title, 512);
        var severity = NormalizeIncidentSeverity(request.Severity);
        var ownerUserId = Required(request.OwnerUserId, 128);
        if (code is null || title is null || severity is null || ownerUserId is null)
        {
            return Validation<SecurityIncidentResponse>("Code, title, severity, and owner are required.");
        }

        if (await dbContext.SecurityIncidents.AnyAsync(x => x.Id != id && x.Code == code, cancellationToken))
        {
            return Validation<SecurityIncidentResponse>("Security incident code already exists.", ApiErrorCodes.SecurityIncidentCodeDuplicate);
        }

        if (request.ProjectId.HasValue && !await dbContext.Projects.AnyAsync(x => x.Id == request.ProjectId.Value, cancellationToken))
        {
            return NotFound<SecurityIncidentResponse>();
        }

        var nextStatus = NormalizeIncidentStatus(request.Status);
        if (!IsValidIncidentTransition(entity.Status, nextStatus))
        {
            return Validation<SecurityIncidentResponse>("Invalid workflow transition.", ApiErrorCodes.InvalidWorkflowTransition);
        }

        var resolutionSummary = Optional(request.ResolutionSummary, 4000);
        if (nextStatus == "closed" && resolutionSummary is null)
        {
            return Validation<SecurityIncidentResponse>("Security incident closure requires resolution summary.", ApiErrorCodes.SecurityIncidentResolutionRequired);
        }

        entity.ProjectId = request.ProjectId;
        entity.Code = code;
        entity.Title = title;
        entity.Severity = severity;
        entity.ReportedAt = request.ReportedAt;
        entity.OwnerUserId = ownerUserId;
        entity.Status = nextStatus;
        entity.ResolutionSummary = resolutionSummary;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        AppendAudit("update", "security_incident", entity.Id.ToString(), StatusCodes.Status200OK, resolutionSummary, entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(await ToResponseAsync(entity, cancellationToken));
    }

    public async Task<OperationsCommandResult<VulnerabilityResponse>> CreateVulnerabilityAsync(CreateVulnerabilityRequest request, string? actor, CancellationToken cancellationToken)
    {
        var assetRef = Required(request.AssetRef, 256);
        var title = Required(request.Title, 512);
        var severity = NormalizeIncidentSeverity(request.Severity);
        var ownerUserId = Required(request.OwnerUserId, 128);
        if (assetRef is null || title is null || severity is null || ownerUserId is null)
        {
            return Validation<VulnerabilityResponse>("Asset, title, severity, and owner are required.");
        }

        var entity = new VulnerabilityRecordEntity
        {
            Id = Guid.NewGuid(),
            AssetRef = assetRef,
            Title = title,
            Severity = severity,
            IdentifiedAt = request.IdentifiedAt,
            PatchDueAt = request.PatchDueAt,
            OwnerUserId = ownerUserId,
            Status = NormalizeVulnerabilityStatus(request.Status),
            VerificationSummary = Optional(request.VerificationSummary, 4000),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.VulnerabilityRecords.Add(entity);
        AppendAudit("create", "vulnerability_record", entity.Id.ToString(), StatusCodes.Status201Created, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity), created: true);
    }

    public async Task<OperationsCommandResult<VulnerabilityResponse>> UpdateVulnerabilityAsync(Guid id, UpdateVulnerabilityRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.VulnerabilityRecords.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound<VulnerabilityResponse>();

        var assetRef = Required(request.AssetRef, 256);
        var title = Required(request.Title, 512);
        var severity = NormalizeIncidentSeverity(request.Severity);
        var ownerUserId = Required(request.OwnerUserId, 128);
        if (assetRef is null || title is null || severity is null || ownerUserId is null)
        {
            return Validation<VulnerabilityResponse>("Asset, title, severity, and owner are required.");
        }

        var nextStatus = NormalizeVulnerabilityStatus(request.Status);
        if (!IsValidVulnerabilityTransition(entity.Status, nextStatus))
        {
            return Validation<VulnerabilityResponse>("Invalid workflow transition.", ApiErrorCodes.InvalidWorkflowTransition);
        }

        entity.AssetRef = assetRef;
        entity.Title = title;
        entity.Severity = severity;
        entity.IdentifiedAt = request.IdentifiedAt;
        entity.PatchDueAt = request.PatchDueAt;
        entity.OwnerUserId = ownerUserId;
        entity.Status = nextStatus;
        entity.VerificationSummary = Optional(request.VerificationSummary, 4000);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        AppendAudit("update", "vulnerability_record", entity.Id.ToString(), StatusCodes.Status200OK, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity));
    }

    public async Task<OperationsCommandResult<SecretRotationResponse>> CreateSecretRotationAsync(CreateSecretRotationRequest request, string? actor, CancellationToken cancellationToken)
    {
        var secretScope = Required(request.SecretScope, 256);
        if (secretScope is null)
        {
            return Validation<SecretRotationResponse>("Secret scope is required.");
        }

        var status = NormalizeSecretRotationStatus(request.Status);
        if (status == "verified" && (Required(request.VerifiedBy, 128) is null || request.VerifiedAt is null || request.RotatedAt is null))
        {
            return Validation<SecretRotationResponse>("Secret rotation verification requires verifier and verification time.", ApiErrorCodes.SecretRotationVerificationRequired);
        }

        var entity = new SecretRotationEntity
        {
            Id = Guid.NewGuid(),
            SecretScope = secretScope,
            PlannedAt = request.PlannedAt,
            RotatedAt = request.RotatedAt,
            VerifiedBy = Optional(request.VerifiedBy, 128),
            VerifiedAt = request.VerifiedAt,
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.SecretRotations.Add(entity);
        AppendAudit("create", "secret_rotation", entity.Id.ToString(), StatusCodes.Status201Created, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity), created: true);
    }

    public async Task<OperationsCommandResult<SecretRotationResponse>> UpdateSecretRotationAsync(Guid id, UpdateSecretRotationRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.SecretRotations.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound<SecretRotationResponse>();

        var secretScope = Required(request.SecretScope, 256);
        if (secretScope is null)
        {
            return Validation<SecretRotationResponse>("Secret scope is required.");
        }

        var nextStatus = NormalizeSecretRotationStatus(request.Status);
        if (!IsValidSecretRotationTransition(entity.Status, nextStatus))
        {
            return Validation<SecretRotationResponse>("Invalid workflow transition.", ApiErrorCodes.InvalidWorkflowTransition);
        }

        var verifiedBy = Optional(request.VerifiedBy, 128);
        if (nextStatus == "verified" && (verifiedBy is null || request.VerifiedAt is null || request.RotatedAt is null))
        {
            return Validation<SecretRotationResponse>("Secret rotation verification requires verifier and verification time.", ApiErrorCodes.SecretRotationVerificationRequired);
        }

        entity.SecretScope = secretScope;
        entity.PlannedAt = request.PlannedAt;
        entity.RotatedAt = request.RotatedAt;
        entity.VerifiedBy = verifiedBy;
        entity.VerifiedAt = request.VerifiedAt;
        entity.Status = nextStatus;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        AppendAudit("update", "secret_rotation", entity.Id.ToString(), StatusCodes.Status200OK, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity));
    }

    public async Task<OperationsCommandResult<PrivilegedAccessEventResponse>> CreatePrivilegedAccessEventAsync(CreatePrivilegedAccessEventRequest request, string? actor, CancellationToken cancellationToken)
    {
        var requestedBy = Required(request.RequestedBy, 128);
        var reason = Required(request.Reason, 2000);
        if (requestedBy is null || reason is null)
        {
            return Validation<PrivilegedAccessEventResponse>("Requester and reason are required.");
        }

        var status = NormalizePrivilegedAccessStatus(request.Status);
        if (status is "used" or "reviewed" or "closed" && Required(request.ApprovedBy, 128) is null)
        {
            return Validation<PrivilegedAccessEventResponse>("Privileged access use requires approved request.", ApiErrorCodes.PrivilegedAccessApprovalRequired);
        }

        var entity = new PrivilegedAccessEventEntity
        {
            Id = Guid.NewGuid(),
            RequestedBy = requestedBy,
            ApprovedBy = Optional(request.ApprovedBy, 128),
            UsedBy = Optional(request.UsedBy, 128),
            RequestedAt = request.RequestedAt,
            ApprovedAt = request.ApprovedAt,
            UsedAt = request.UsedAt,
            ReviewedAt = request.ReviewedAt,
            Status = status,
            Reason = reason,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.PrivilegedAccessEvents.Add(entity);
        AppendAudit("create", "privileged_access_event", entity.Id.ToString(), StatusCodes.Status201Created, reason, entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity), created: true);
    }

    public async Task<OperationsCommandResult<PrivilegedAccessEventResponse>> UpdatePrivilegedAccessEventAsync(Guid id, UpdatePrivilegedAccessEventRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.PrivilegedAccessEvents.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound<PrivilegedAccessEventResponse>();

        var requestedBy = Required(request.RequestedBy, 128);
        var reason = Required(request.Reason, 2000);
        if (requestedBy is null || reason is null)
        {
            return Validation<PrivilegedAccessEventResponse>("Requester and reason are required.");
        }

        var nextStatus = NormalizePrivilegedAccessStatus(request.Status);
        if (nextStatus is "used" or "reviewed" or "closed" && Required(request.ApprovedBy, 128) is null)
        {
            return Validation<PrivilegedAccessEventResponse>("Privileged access use requires approved request.", ApiErrorCodes.PrivilegedAccessApprovalRequired);
        }

        if (!IsValidPrivilegedAccessTransition(entity.Status, nextStatus))
        {
            return Validation<PrivilegedAccessEventResponse>("Invalid workflow transition.", ApiErrorCodes.InvalidWorkflowTransition);
        }

        entity.RequestedBy = requestedBy;
        entity.ApprovedBy = Optional(request.ApprovedBy, 128);
        entity.UsedBy = Optional(request.UsedBy, 128);
        entity.RequestedAt = request.RequestedAt;
        entity.ApprovedAt = request.ApprovedAt;
        entity.UsedAt = request.UsedAt;
        entity.ReviewedAt = request.ReviewedAt;
        entity.Status = nextStatus;
        entity.Reason = reason;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        AppendAudit("update", "privileged_access_event", entity.Id.ToString(), StatusCodes.Status200OK, reason, entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity));
    }

    public async Task<OperationsCommandResult<ClassificationPolicyResponse>> CreateClassificationPolicyAsync(CreateClassificationPolicyRequest request, string? actor, CancellationToken cancellationToken)
    {
        var policyCode = Required(request.PolicyCode, 128);
        var classificationLevel = Required(request.ClassificationLevel, 64);
        var scope = Required(request.Scope, 256);
        if (policyCode is null || classificationLevel is null || scope is null)
        {
            return Validation<ClassificationPolicyResponse>("Policy code, classification level, and scope are required.");
        }

        var entity = new DataClassificationPolicyEntity
        {
            Id = Guid.NewGuid(),
            PolicyCode = policyCode,
            ClassificationLevel = classificationLevel.ToLowerInvariant(),
            Scope = scope,
            Status = NormalizeClassificationPolicyStatus(request.Status),
            HandlingRule = Optional(request.HandlingRule, 2000),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.DataClassificationPolicies.Add(entity);
        AppendAudit("create", "classification_policy", entity.Id.ToString(), StatusCodes.Status201Created, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity), created: true);
    }

    public async Task<OperationsCommandResult<ClassificationPolicyResponse>> UpdateClassificationPolicyAsync(Guid id, UpdateClassificationPolicyRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.DataClassificationPolicies.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound<ClassificationPolicyResponse>();

        var policyCode = Required(request.PolicyCode, 128);
        var classificationLevel = Required(request.ClassificationLevel, 64);
        var scope = Required(request.Scope, 256);
        if (policyCode is null || classificationLevel is null || scope is null)
        {
            return Validation<ClassificationPolicyResponse>("Policy code, classification level, and scope are required.");
        }

        var nextStatus = NormalizeClassificationPolicyStatus(request.Status);
        if (!IsValidClassificationPolicyTransition(entity.Status, nextStatus))
        {
            return Validation<ClassificationPolicyResponse>("Invalid workflow transition.", ApiErrorCodes.InvalidWorkflowTransition);
        }

        entity.PolicyCode = policyCode;
        entity.ClassificationLevel = classificationLevel.ToLowerInvariant();
        entity.Scope = scope;
        entity.Status = nextStatus;
        entity.HandlingRule = Optional(request.HandlingRule, 2000);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        AppendAudit("update", "classification_policy", entity.Id.ToString(), StatusCodes.Status200OK, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity));
    }

    public async Task<OperationsCommandResult<BackupEvidenceResponse>> CreateBackupEvidenceAsync(CreateBackupEvidenceRequest request, string? actor, CancellationToken cancellationToken)
    {
        var backupScope = Required(request.BackupScope, 128);
        var executedBy = Required(request.ExecutedBy, 128);
        if (backupScope is null || executedBy is null)
        {
            return Validation<BackupEvidenceResponse>("Backup scope and operator are required.");
        }

        var entity = new BackupEvidenceEntity
        {
            Id = Guid.NewGuid(),
            BackupScope = backupScope,
            ExecutedAt = request.ExecutedAt,
            ExecutedBy = executedBy,
            Status = NormalizeBackupEvidenceStatus(request.Status),
            EvidenceRef = Optional(request.EvidenceRef, 512),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.BackupEvidence.Add(entity);
        AppendAudit("create", "backup_evidence", entity.Id.ToString(), StatusCodes.Status201Created, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity), created: true);
    }

    public async Task<OperationsCommandResult<RestoreVerificationResponse>> CreateRestoreVerificationAsync(CreateRestoreVerificationRequest request, string? actor, CancellationToken cancellationToken)
    {
        if (!request.BackupEvidenceId.HasValue)
        {
            return Validation<RestoreVerificationResponse>("Restore verification requires backup reference.", ApiErrorCodes.RestoreBackupReferenceRequired);
        }

        var backup = await dbContext.BackupEvidence.FirstOrDefaultAsync(x => x.Id == request.BackupEvidenceId.Value, cancellationToken);
        if (backup is null)
        {
            return Validation<RestoreVerificationResponse>("Restore verification requires backup reference.", ApiErrorCodes.RestoreBackupReferenceRequired);
        }

        var executedBy = Required(request.ExecutedBy, 128);
        var resultSummary = Required(request.ResultSummary, 4000);
        if (executedBy is null || resultSummary is null)
        {
            return Validation<RestoreVerificationResponse>("Operator and result summary are required.");
        }

        var entity = new RestoreVerificationEntity
        {
            Id = Guid.NewGuid(),
            BackupEvidenceId = backup.Id,
            ExecutedAt = request.ExecutedAt,
            ExecutedBy = executedBy,
            Status = NormalizeRestoreVerificationStatus(request.Status),
            ResultSummary = resultSummary,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.RestoreVerifications.Add(entity);
        AppendAudit("create", "restore_verification", entity.Id.ToString(), StatusCodes.Status201Created, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity, backup.BackupScope), created: true);
    }

    public async Task<OperationsCommandResult<DrDrillResponse>> CreateDrDrillAsync(CreateDrDrillRequest request, string? actor, CancellationToken cancellationToken)
    {
        var scopeRef = Required(request.ScopeRef, 256);
        if (scopeRef is null)
        {
            return Validation<DrDrillResponse>("Scope reference is required.");
        }

        var entity = new DrDrillEntity
        {
            Id = Guid.NewGuid(),
            ScopeRef = scopeRef,
            PlannedAt = request.PlannedAt,
            ExecutedAt = request.ExecutedAt,
            Status = NormalizeDrDrillStatus(request.Status),
            FindingCount = Math.Max(0, request.FindingCount),
            Summary = Optional(request.Summary, 4000),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.DrDrills.Add(entity);
        AppendAudit("create", "dr_drill", entity.Id.ToString(), StatusCodes.Status201Created, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity), created: true);
    }

    public async Task<OperationsCommandResult<DrDrillResponse>> UpdateDrDrillAsync(Guid id, UpdateDrDrillRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.DrDrills.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound<DrDrillResponse>();

        var scopeRef = Required(request.ScopeRef, 256);
        if (scopeRef is null)
        {
            return Validation<DrDrillResponse>("Scope reference is required.");
        }

        var nextStatus = NormalizeDrDrillStatus(request.Status);
        if (!IsValidLinearTransition(entity.Status, nextStatus, DrDrillStatuses))
        {
            return Validation<DrDrillResponse>("Invalid workflow transition.", ApiErrorCodes.InvalidWorkflowTransition);
        }

        entity.ScopeRef = scopeRef;
        entity.PlannedAt = request.PlannedAt;
        entity.ExecutedAt = request.ExecutedAt;
        entity.Status = nextStatus;
        entity.FindingCount = Math.Max(0, request.FindingCount);
        entity.Summary = Optional(request.Summary, 4000);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        AppendAudit("update", "dr_drill", entity.Id.ToString(), StatusCodes.Status200OK, after: entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity));
    }

    public async Task<OperationsCommandResult<LegalHoldResponse>> CreateLegalHoldAsync(CreateLegalHoldRequest request, string? actor, CancellationToken cancellationToken)
    {
        var scopeType = Required(request.ScopeType, 64);
        var scopeRef = Required(request.ScopeRef, 256);
        var reason = Required(request.Reason, 2000);
        if (scopeType is null || scopeRef is null || reason is null)
        {
            return Validation<LegalHoldResponse>("Scope and reason are required.");
        }

        var entity = new LegalHoldEntity
        {
            Id = Guid.NewGuid(),
            ScopeType = scopeType.ToLowerInvariant(),
            ScopeRef = scopeRef,
            PlacedAt = DateTimeOffset.UtcNow,
            PlacedBy = actor ?? "system",
            Status = "active",
            Reason = reason,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.LegalHolds.Add(entity);
        AppendAudit("create", "legal_hold", entity.Id.ToString(), StatusCodes.Status201Created, reason, entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity), created: true);
    }

    public async Task<OperationsCommandResult<LegalHoldResponse>> ReleaseLegalHoldAsync(Guid id, ReleaseLegalHoldRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.LegalHolds.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound<LegalHoldResponse>();

        var reason = Required(request.Reason, 2000);
        if (reason is null)
        {
            return Validation<LegalHoldResponse>("Legal hold release requires rationale.", ApiErrorCodes.LegalHoldReleaseReasonRequired);
        }

        if (!IsValidLinearTransition(entity.Status, "released", LegalHoldStatuses))
        {
            return Validation<LegalHoldResponse>("Invalid workflow transition.", ApiErrorCodes.InvalidWorkflowTransition);
        }

        entity.Status = "released";
        entity.ReleasedAt = DateTimeOffset.UtcNow;
        entity.ReleasedBy = actor ?? "system";
        entity.ReleaseReason = reason;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        AppendAudit("release", "legal_hold", entity.Id.ToString(), StatusCodes.Status200OK, reason, entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity));
    }

    private static string? NormalizeIncidentSeverity(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "low" => "low",
        "medium" => "medium",
        "high" => "high",
        "critical" => "critical",
        _ => null
    };

    private static string NormalizeIncidentStatus(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "assessed" => "assessed",
        "contained" => "contained",
        "resolved" => "resolved",
        "closed" => "closed",
        _ => "reported"
    };

    private static string NormalizeVulnerabilityStatus(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "assessed" => "assessed",
        "scheduled" => "scheduled",
        "patched" => "patched",
        "verified" => "verified",
        "closed" => "closed",
        _ => "open"
    };

    private static string NormalizeSecretRotationStatus(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "rotated" => "rotated",
        "verified" => "verified",
        "archived" => "archived",
        _ => "planned"
    };

    private static string NormalizePrivilegedAccessStatus(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "approved" => "approved",
        "used" => "used",
        "reviewed" => "reviewed",
        "closed" => "closed",
        _ => "requested"
    };

    private static string NormalizeClassificationPolicyStatus(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "approved" => "approved",
        "active" => "active",
        "archived" => "archived",
        _ => "draft"
    };
    private static string NormalizeBackupEvidenceStatus(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "completed" => "completed",
        "verified" => "verified",
        "archived" => "archived",
        _ => "planned"
    };

    private static string NormalizeRestoreVerificationStatus(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "executed" => "executed",
        "verified" => "verified",
        "closed" => "closed",
        _ => "planned"
    };

    private static string NormalizeDrDrillStatus(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "executed" => "executed",
        "findings_issued" => "findings_issued",
        "closed" => "closed",
        _ => "planned"
    };

    private static OperationsCommandResult<T> Success<T>(T value, bool created = false) => new(OperationsCommandStatus.Success, value);
    private static OperationsCommandResult<T> NotFound<T>() => new(OperationsCommandStatus.NotFound, default, "Resource not found.", ApiErrorCodes.ResourceNotFound);
    private static OperationsCommandResult<T> Validation<T>(string message, string? code = null) => new(OperationsCommandStatus.ValidationError, default, message, code ?? ApiErrorCodes.RequestValidationFailed);
    private static string? Required(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : (value.Trim().Length > max ? value.Trim()[..max] : value.Trim());
    private static string? Optional(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : (value.Trim().Length > max ? value.Trim()[..max] : value.Trim());
    private static string NormalizeAccessStatus(string? value) => value?.Trim().ToLowerInvariant() switch { "in_review" => "In Review", "approved" => "Approved", "archived" => "Archived", _ => "Scheduled" };
    private static string NormalizeSecurityStatus(string? value) => value?.Trim().ToLowerInvariant() switch { "in_review" => "In Review", "findings_issued" => "Findings Issued", "closed" => "Closed", _ => "Planned" };
    private static string NormalizeDependencyStatus(string? value) => value?.Trim().ToLowerInvariant() switch { "review_due" => "Review Due", "updated" => "Updated", "archived" => "Archived", _ => "Active" };
    private static string NormalizeAuditStatus(string? value) => value?.Trim().ToLowerInvariant() switch { "in_review" => "In Review", "findings_issued" => "Findings Issued", "closed" => "Closed", _ => "Planned" };
    private static string NormalizeSupplierStatus(string? value) => value?.Trim().ToLowerInvariant() switch { "review_due" => "Review Due", "updated" => "Updated", "archived" => "Archived", _ => "Active" };
    private static string NormalizeAgreementStatus(string? value) => value?.Trim().ToLowerInvariant() switch { "approved" => "Approved", "active" => "Active", "archived" => "Archived", _ => "Draft" };
    private static string NormalizeRecertificationStatus(string? value) => value?.Trim().ToLowerInvariant() switch { "in_review" => "in_review", "approved" => "approved", "completed" => "completed", _ => "planned" };
    private static string? NormalizeRecertificationDecision(string? value) => value?.Trim().ToLowerInvariant() switch { "kept" => "kept", "revoked" => "revoked", "adjusted" => "adjusted", _ => null };
    private static AccessReviewResponse ToResponse(AccessReviewEntity x) => new(x.Id, x.ScopeType, x.ScopeRef, x.ReviewCycle, x.ReviewedBy, x.Status, x.Decision, x.DecisionRationale, x.CreatedAt, x.UpdatedAt);
    private static SecurityReviewResponse ToResponse(SecurityReviewEntity x) => new(x.Id, x.ScopeType, x.ScopeRef, x.ControlsReviewed, x.FindingsSummary, x.Status, x.CreatedAt, x.UpdatedAt);
    private static AccessRecertificationDecisionResponse ToResponse(AccessRecertificationDecisionEntity x) => new(x.Id, x.ScheduleId, x.SubjectUserId, x.Decision, x.Reason, x.DecidedBy, x.DecidedAt);
    private static VulnerabilityResponse ToResponse(VulnerabilityRecordEntity x) => new(x.Id, x.AssetRef, x.Title, x.Severity, x.IdentifiedAt, x.PatchDueAt, x.OwnerUserId, x.Status, x.VerificationSummary, x.CreatedAt, x.UpdatedAt);
    private static SecretRotationResponse ToResponse(SecretRotationEntity x) => new(x.Id, x.SecretScope, x.PlannedAt, x.RotatedAt, x.VerifiedBy, x.VerifiedAt, x.Status, x.CreatedAt, x.UpdatedAt);
    private static PrivilegedAccessEventResponse ToResponse(PrivilegedAccessEventEntity x) => new(x.Id, x.RequestedBy, x.ApprovedBy, x.UsedBy, x.RequestedAt, x.ApprovedAt, x.UsedAt, x.ReviewedAt, x.Status, x.Reason, x.CreatedAt, x.UpdatedAt);
    private static ClassificationPolicyResponse ToResponse(DataClassificationPolicyEntity x) => new(x.Id, x.PolicyCode, x.ClassificationLevel, x.Scope, x.Status, x.HandlingRule, x.CreatedAt, x.UpdatedAt);
    private static BackupEvidenceResponse ToResponse(BackupEvidenceEntity x) => new(x.Id, x.BackupScope, x.ExecutedAt, x.ExecutedBy, x.Status, x.EvidenceRef, x.CreatedAt);
    private static RestoreVerificationResponse ToResponse(RestoreVerificationEntity x, string backupScope) => new(x.Id, x.BackupEvidenceId, backupScope, x.ExecutedAt, x.ExecutedBy, x.Status, x.ResultSummary, x.CreatedAt);
    private static DrDrillResponse ToResponse(DrDrillEntity x) => new(x.Id, x.ScopeRef, x.PlannedAt, x.ExecutedAt, x.Status, x.FindingCount, x.Summary, x.CreatedAt, x.UpdatedAt);
    private static LegalHoldResponse ToResponse(LegalHoldEntity x) => new(x.Id, x.ScopeType, x.ScopeRef, x.PlacedAt, x.PlacedBy, x.Status, x.Reason, x.ReleasedAt, x.ReleasedBy, x.ReleaseReason, x.CreatedAt, x.UpdatedAt);
    private async Task<ExternalDependencyResponse> ToResponseAsync(ExternalDependencyEntity x, CancellationToken cancellationToken)
    {
        string? supplierName = null;
        if (x.SupplierId.HasValue)
        {
            supplierName = await dbContext.Suppliers
                .Where(y => y.Id == x.SupplierId.Value)
                .Select(y => y.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new ExternalDependencyResponse(x.Id, x.Name, x.DependencyType, x.SupplierId, supplierName, x.OwnerUserId, x.Criticality, x.Status, x.ReviewDueAt, x.CreatedAt, x.UpdatedAt);
    }

    private async Task<SecurityIncidentResponse> ToResponseAsync(SecurityIncidentEntity x, CancellationToken cancellationToken)
    {
        string? projectName = null;
        if (x.ProjectId.HasValue)
        {
            projectName = await dbContext.Projects
                .Where(y => y.Id == x.ProjectId.Value)
                .Select(y => y.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new SecurityIncidentResponse(
            x.Id,
            x.ProjectId,
            projectName,
            x.Code,
            x.Title,
            x.Severity,
            x.ReportedAt,
            x.OwnerUserId,
            x.Status,
            x.ResolutionSummary,
            x.CreatedAt,
            x.UpdatedAt);
    }

    private static ConfigurationAuditResponse ToResponse(ConfigurationAuditEntity x) => new(x.Id, x.ScopeRef, x.PlannedAt, x.Status, x.FindingCount, x.CreatedAt, x.UpdatedAt);
    private static SupplierResponse ToResponse(SupplierEntity x, int activeAgreementCount) => new(x.Id, x.Name, x.SupplierType, x.OwnerUserId, x.Criticality, x.Status, x.ReviewDueAt, activeAgreementCount, x.CreatedAt, x.UpdatedAt);
    private static SupplierAgreementResponse ToResponse(SupplierAgreementEntity x, string supplierName) => new(x.Id, x.SupplierId, supplierName, x.AgreementType, x.EffectiveFrom, x.EffectiveTo, x.SlaTerms, x.EvidenceRef, x.Status, x.CreatedAt, x.UpdatedAt);

    private async Task<AccessRecertificationResponse?> GetAccessRecertificationAsync(Guid id, CancellationToken cancellationToken)
    {
        var schedule = await dbContext.AccessRecertificationSchedules.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (schedule is null)
        {
            return null;
        }

        var decisions = await dbContext.AccessRecertificationDecisions.AsNoTracking()
            .Where(x => x.ScheduleId == id)
            .OrderBy(x => x.SubjectUserId)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);

        var subjects = DeserializeSubjects(schedule.SubjectUsersJson);
        return ToResponse(schedule, decisions, subjects);
    }

    private static AccessRecertificationResponse ToResponse(AccessRecertificationScheduleEntity schedule, IReadOnlyList<AccessRecertificationDecisionResponse> decisions, IReadOnlyList<string> subjects)
    {
        var completedCount = decisions.Count;
        var pendingCount = Math.Max(0, subjects.Count - completedCount);
        return new AccessRecertificationResponse(
            schedule.Id,
            schedule.ScopeType,
            schedule.ScopeRef,
            schedule.PlannedAt,
            schedule.ReviewOwnerUserId,
            schedule.Status,
            subjects,
            decisions,
            schedule.ExceptionNotes,
            completedCount,
            pendingCount,
            schedule.CreatedAt,
            schedule.UpdatedAt,
            schedule.CompletedAt);
    }

    private static bool IsValidRecertificationTransition(string current, string next) =>
        (current, next) switch
        {
            ("planned", "planned") => true,
            ("planned", "in_review") => true,
            ("in_review", "in_review") => true,
            ("in_review", "approved") => true,
            ("approved", "approved") => true,
            ("approved", "completed") => true,
            ("completed", "completed") => true,
            _ => false
        };

    private static bool IsValidIncidentTransition(string current, string next) =>
        (current, next) switch
        {
            ("reported", "reported") => true,
            ("reported", "assessed") => true,
            ("assessed", "assessed") => true,
            ("assessed", "contained") => true,
            ("contained", "contained") => true,
            ("contained", "resolved") => true,
            ("resolved", "resolved") => true,
            ("resolved", "closed") => true,
            ("closed", "closed") => true,
            _ => false
        };

    private static bool IsValidVulnerabilityTransition(string current, string next) =>
        (current, next) switch
        {
            ("open", "open") => true,
            ("open", "assessed") => true,
            ("assessed", "assessed") => true,
            ("assessed", "scheduled") => true,
            ("scheduled", "scheduled") => true,
            ("scheduled", "patched") => true,
            ("patched", "patched") => true,
            ("patched", "verified") => true,
            ("verified", "verified") => true,
            ("verified", "closed") => true,
            ("closed", "closed") => true,
            _ => false
        };

    private static bool IsValidSecretRotationTransition(string current, string next) =>
        (current, next) switch
        {
            ("planned", "planned") => true,
            ("planned", "rotated") => true,
            ("rotated", "rotated") => true,
            ("rotated", "verified") => true,
            ("verified", "verified") => true,
            ("verified", "archived") => true,
            ("archived", "archived") => true,
            _ => false
        };

    private static bool IsValidPrivilegedAccessTransition(string current, string next) =>
        (current, next) switch
        {
            ("requested", "requested") => true,
            ("requested", "approved") => true,
            ("approved", "approved") => true,
            ("approved", "used") => true,
            ("used", "used") => true,
            ("used", "reviewed") => true,
            ("reviewed", "reviewed") => true,
            ("reviewed", "closed") => true,
            ("closed", "closed") => true,
            _ => false
        };

    private static bool IsValidClassificationPolicyTransition(string current, string next) =>
        (current, next) switch
        {
            ("draft", "draft") => true,
            ("draft", "approved") => true,
            ("approved", "approved") => true,
            ("approved", "active") => true,
            ("active", "active") => true,
            ("active", "archived") => true,
            ("archived", "archived") => true,
            _ => false
        };

    private static bool IsValidLinearTransition(string current, string next, IReadOnlyList<string> statuses)
    {
        var currentIndex = Array.IndexOf(statuses.ToArray(), current);
        var nextIndex = Array.IndexOf(statuses.ToArray(), next);
        return currentIndex >= 0 && nextIndex >= 0 && nextIndex >= currentIndex && nextIndex - currentIndex <= 1;
    }

    private static string? SerializeSubjects(IEnumerable<string>? subjectUserIds)
    {
        var subjects = NormalizeSubjects(subjectUserIds);
        return JsonSerializer.Serialize(subjects);
    }

    private static List<string> DeserializeSubjects(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return NormalizeSubjects(JsonSerializer.Deserialize<List<string>>(json));
        }
        catch
        {
            return [];
        }
    }

    private static List<string> NormalizeSubjects(IEnumerable<string>? subjectUserIds) =>
        (subjectUserIds ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
}
