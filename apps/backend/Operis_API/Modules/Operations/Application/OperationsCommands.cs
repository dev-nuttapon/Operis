using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Operations.Contracts;
using Operis_API.Modules.Operations.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Operations.Application;

public sealed class OperationsCommands(OperisDbContext dbContext, IAuditLogWriter auditLogWriter) : IOperationsCommands
{
    public async Task<OperationsCommandResult<AccessReviewResponse>> CreateAccessReviewAsync(CreateAccessReviewRequest request, string? actor, CancellationToken cancellationToken)
    {
        var scopeType = Required(request.ScopeType, 64);
        var scopeRef = Required(request.ScopeRef, 256);
        var reviewCycle = Required(request.ReviewCycle, 64);
        if (scopeType is null || scopeRef is null || reviewCycle is null)
        {
            return Validation<AccessReviewResponse>("Scope and review cycle are required.");
        }

        var entity = new AccessReviewEntity { Id = Guid.NewGuid(), ScopeType = scopeType, ScopeRef = scopeRef, ReviewCycle = reviewCycle, ReviewedBy = Optional(request.ReviewedBy, 64), Status = "Scheduled", CreatedAt = DateTimeOffset.UtcNow };
        dbContext.AccessReviews.Add(entity);
        auditLogWriter.Append(new AuditLogEntry(Module: "operations", Action: "create", EntityType: "access_review", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status201Created, After: entity));
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
        auditLogWriter.Append(new AuditLogEntry(Module: "operations", Action: "update", EntityType: "access_review", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status200OK, After: entity));
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
        auditLogWriter.Append(new AuditLogEntry(Module: "operations", Action: "approve", EntityType: "access_review", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status200OK, Reason: rationale, After: entity));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity));
    }

    public async Task<OperationsCommandResult<SecurityReviewResponse>> CreateSecurityReviewAsync(CreateSecurityReviewRequest request, string? actor, CancellationToken cancellationToken)
    {
        var scopeType = Required(request.ScopeType, 64);
        var scopeRef = Required(request.ScopeRef, 256);
        var controlsReviewed = Required(request.ControlsReviewed, 2000);
        if (scopeType is null || scopeRef is null || controlsReviewed is null) return Validation<SecurityReviewResponse>("Scope and controls reviewed are required.");
        var entity = new SecurityReviewEntity { Id = Guid.NewGuid(), ScopeType = scopeType, ScopeRef = scopeRef, ControlsReviewed = controlsReviewed, FindingsSummary = Optional(request.FindingsSummary, 2000), Status = NormalizeSecurityStatus(request.Status), CreatedAt = DateTimeOffset.UtcNow };
        dbContext.SecurityReviews.Add(entity);
        auditLogWriter.Append(new AuditLogEntry(Module: "operations", Action: "create", EntityType: "security_review", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status201Created, After: entity));
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
        auditLogWriter.Append(new AuditLogEntry(Module: "operations", Action: "update", EntityType: "security_review", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status200OK, After: entity));
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
        var entity = new ExternalDependencyEntity { Id = Guid.NewGuid(), Name = name, DependencyType = dependencyType, OwnerUserId = ownerUserId, Criticality = criticality.ToLowerInvariant(), ReviewDueAt = request.ReviewDueAt, Status = NormalizeDependencyStatus(request.Status), CreatedAt = DateTimeOffset.UtcNow };
        dbContext.ExternalDependencies.Add(entity);
        auditLogWriter.Append(new AuditLogEntry(Module: "operations", Action: "create", EntityType: "external_dependency", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status201Created, After: entity));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity), created: true);
    }

    public async Task<OperationsCommandResult<ExternalDependencyResponse>> UpdateExternalDependencyAsync(Guid id, UpdateExternalDependencyRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ExternalDependencies.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return NotFound<ExternalDependencyResponse>();
        var ownerUserId = Required(request.OwnerUserId, 64);
        var criticality = Required(request.Criticality, 32);
        if (ownerUserId is null) return Validation<ExternalDependencyResponse>("Dependency owner is required.", ApiErrorCodes.DependencyOwnerRequired);
        if (criticality is null) return Validation<ExternalDependencyResponse>("Dependency criticality is required.", ApiErrorCodes.DependencyCriticalityRequired);
        entity.Name = Required(request.Name, 256) ?? entity.Name;
        entity.DependencyType = Required(request.DependencyType, 128) ?? entity.DependencyType;
        entity.OwnerUserId = ownerUserId;
        entity.Criticality = criticality.ToLowerInvariant();
        entity.ReviewDueAt = request.ReviewDueAt;
        entity.Status = NormalizeDependencyStatus(request.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        auditLogWriter.Append(new AuditLogEntry(Module: "operations", Action: "update", EntityType: "external_dependency", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status200OK, After: entity));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity));
    }

    public async Task<OperationsCommandResult<ConfigurationAuditResponse>> CreateConfigurationAuditAsync(CreateConfigurationAuditRequest request, string? actor, CancellationToken cancellationToken)
    {
        var scopeRef = Required(request.ScopeRef, 256);
        if (scopeRef is null) return Validation<ConfigurationAuditResponse>("Scope reference is required.");
        var entity = new ConfigurationAuditEntity { Id = Guid.NewGuid(), ScopeRef = scopeRef, PlannedAt = request.PlannedAt, Status = NormalizeAuditStatus(request.Status), FindingCount = Math.Max(0, request.FindingCount), CreatedAt = DateTimeOffset.UtcNow };
        dbContext.ConfigurationAudits.Add(entity);
        auditLogWriter.Append(new AuditLogEntry(Module: "operations", Action: "create", EntityType: "configuration_audit", EntityId: entity.Id.ToString(), StatusCode: StatusCodes.Status201Created, After: entity));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Success(ToResponse(entity), created: true);
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
    private static AccessReviewResponse ToResponse(AccessReviewEntity x) => new(x.Id, x.ScopeType, x.ScopeRef, x.ReviewCycle, x.ReviewedBy, x.Status, x.Decision, x.DecisionRationale, x.CreatedAt, x.UpdatedAt);
    private static SecurityReviewResponse ToResponse(SecurityReviewEntity x) => new(x.Id, x.ScopeType, x.ScopeRef, x.ControlsReviewed, x.FindingsSummary, x.Status, x.CreatedAt, x.UpdatedAt);
    private static ExternalDependencyResponse ToResponse(ExternalDependencyEntity x) => new(x.Id, x.Name, x.DependencyType, x.OwnerUserId, x.Criticality, x.Status, x.ReviewDueAt, x.CreatedAt, x.UpdatedAt);
    private static ConfigurationAuditResponse ToResponse(ConfigurationAuditEntity x) => new(x.Id, x.ScopeRef, x.PlannedAt, x.Status, x.FindingCount, x.CreatedAt, x.UpdatedAt);
}
