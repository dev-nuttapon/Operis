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
