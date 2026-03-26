using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Audits.Application;
using Operis_API.Modules.Requirements.Contracts;
using Operis_API.Modules.Requirements.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Requirements.Application;

public sealed class RequirementCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IBusinessAuditEventWriter businessAuditEventWriter,
    IRequirementQueries queries) : IRequirementCommands
{
    private static readonly string[] RequiredTargetTypes = ["document", "test"];
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<RequirementCommandResult<RequirementDetailResponse>> CreateRequirementAsync(CreateRequirementRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var validationError = await ValidateCreateRequestAsync(request, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var now = DateTimeOffset.UtcNow;
        var requirementId = Guid.NewGuid();
        var versionId = Guid.NewGuid();

        var requirement = new RequirementEntity
        {
            Id = requirementId,
            ProjectId = request.ProjectId,
            Code = normalizedCode,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Priority = request.Priority.Trim().ToLowerInvariant(),
            OwnerUserId = request.OwnerUserId.Trim(),
            Status = "draft",
            CurrentVersionId = versionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        var version = new RequirementVersionEntity
        {
            Id = versionId,
            RequirementId = requirementId,
            VersionNumber = 1,
            BusinessReason = request.BusinessReason.Trim(),
            AcceptanceCriteria = request.AcceptanceCriteria.Trim(),
            SecurityImpact = NormalizeOptional(request.SecurityImpact),
            PerformanceImpact = NormalizeOptional(request.PerformanceImpact),
            Status = "draft",
            CreatedAt = now
        };

        dbContext.Add(requirement);
        dbContext.Add(version);
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("create", "requirement", requirementId, StatusCodes.Status201Created, new { requirement.Code, requirement.Status }, cancellationToken);
        await TryAppendBusinessEventAsync("requirement.created", "requirement", requirementId, actorUserId, "Created requirement", null, new { requirement.Code, requirement.Title }, cancellationToken);
        return await SuccessAsync(requirementId, cancellationToken, StatusCodes.Status201Created);
    }

    public async Task<RequirementCommandResult<RequirementDetailResponse>> UpdateRequirementAsync(Guid requirementId, UpdateRequirementRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var requirement = await dbContext.Set<RequirementEntity>().SingleOrDefaultAsync(x => x.Id == requirementId, cancellationToken);
        if (requirement is null)
        {
            return NotFound<RequirementDetailResponse>(ApiErrorCodes.RequirementNotFound, "Requirement not found.");
        }

        if (string.Equals(requirement.Status, "baselined", StringComparison.OrdinalIgnoreCase)
            || string.Equals(requirement.Status, "superseded", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<RequirementDetailResponse>(ApiErrorCodes.RequirementTransitionNotAllowed, "Baselined or superseded requirements cannot be edited directly.");
        }

        var currentVersion = await GetCurrentVersionAsync(requirement, cancellationToken);
        if (currentVersion is null)
        {
            return NotFound<RequirementDetailResponse>(ApiErrorCodes.RequirementVersionNotFound, "Current requirement version not found.");
        }

        var nextVersionNumber = await dbContext.Set<RequirementVersionEntity>()
            .Where(x => x.RequirementId == requirementId)
            .Select(x => (int?)x.VersionNumber)
            .MaxAsync(cancellationToken) ?? 0;

        dbContext.Entry(currentVersion).CurrentValues.SetValues(currentVersion with { Status = "superseded" });

        var now = DateTimeOffset.UtcNow;
        var replacementVersion = new RequirementVersionEntity
        {
            Id = Guid.NewGuid(),
            RequirementId = requirementId,
            VersionNumber = nextVersionNumber + 1,
            BusinessReason = request.BusinessReason.Trim(),
            AcceptanceCriteria = request.AcceptanceCriteria.Trim(),
            SecurityImpact = NormalizeOptional(request.SecurityImpact),
            PerformanceImpact = NormalizeOptional(request.PerformanceImpact),
            Status = "draft",
            CreatedAt = now
        };

        dbContext.Add(replacementVersion);
        dbContext.Entry(requirement).CurrentValues.SetValues(requirement with
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Priority = request.Priority.Trim().ToLowerInvariant(),
            OwnerUserId = request.OwnerUserId.Trim(),
            CurrentVersionId = replacementVersion.Id,
            Status = "draft",
            UpdatedAt = now
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("update", "requirement", requirementId, StatusCodes.Status200OK, new { requirement.Code, Version = replacementVersion.VersionNumber }, cancellationToken);
        await TryAppendBusinessEventAsync("requirement.updated", "requirement", requirementId, actorUserId, "Updated requirement", null, new { replacementVersion.VersionNumber }, cancellationToken);
        return await SuccessAsync(requirementId, cancellationToken);
    }

    public async Task<RequirementCommandResult<RequirementDetailResponse>> SubmitRequirementAsync(Guid requirementId, string? actorUserId, CancellationToken cancellationToken)
    {
        var requirement = await dbContext.Set<RequirementEntity>().SingleOrDefaultAsync(x => x.Id == requirementId, cancellationToken);
        if (requirement is null)
        {
            return NotFound<RequirementDetailResponse>(ApiErrorCodes.RequirementNotFound, "Requirement not found.");
        }

        var version = await GetCurrentVersionAsync(requirement, cancellationToken);
        if (version is null)
        {
            return NotFound<RequirementDetailResponse>(ApiErrorCodes.RequirementVersionNotFound, "Current requirement version not found.");
        }

        if (string.IsNullOrWhiteSpace(version.AcceptanceCriteria))
        {
            return Validation<RequirementDetailResponse>(ApiErrorCodes.RequestValidationFailed, "Acceptance criteria are required before submission.");
        }

        if (!string.Equals(requirement.Status, "draft", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<RequirementDetailResponse>(ApiErrorCodes.RequirementTransitionNotAllowed, "Only draft requirements can be submitted.");
        }

        dbContext.Entry(requirement).CurrentValues.SetValues(requirement with { Status = "review", UpdatedAt = DateTimeOffset.UtcNow });
        dbContext.Entry(version).CurrentValues.SetValues(version with { Status = "submitted" });
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("submit", "requirement", requirementId, StatusCodes.Status200OK, null, cancellationToken);
        await TryAppendBusinessEventAsync("requirement.submitted", "requirement", requirementId, actorUserId, "Submitted requirement", null, null, cancellationToken);
        return await SuccessAsync(requirementId, cancellationToken);
    }

    public async Task<RequirementCommandResult<RequirementDetailResponse>> ApproveRequirementAsync(Guid requirementId, RequirementDecisionRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return Validation<RequirementDetailResponse>(ApiErrorCodes.ApprovalReasonRequired, "Approval reason is required.");
        }

        var requirement = await dbContext.Set<RequirementEntity>().SingleOrDefaultAsync(x => x.Id == requirementId, cancellationToken);
        if (requirement is null)
        {
            return NotFound<RequirementDetailResponse>(ApiErrorCodes.RequirementNotFound, "Requirement not found.");
        }

        var version = await GetCurrentVersionAsync(requirement, cancellationToken);
        if (version is null)
        {
            return NotFound<RequirementDetailResponse>(ApiErrorCodes.RequirementVersionNotFound, "Current requirement version not found.");
        }

        if (!string.Equals(requirement.Status, "review", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<RequirementDetailResponse>(ApiErrorCodes.RequirementTransitionNotAllowed, "Only requirements in review can be approved.");
        }

        dbContext.Entry(requirement).CurrentValues.SetValues(requirement with { Status = "approved", UpdatedAt = DateTimeOffset.UtcNow });
        dbContext.Entry(version).CurrentValues.SetValues(version with { Status = "approved" });
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("approve", "requirement", requirementId, StatusCodes.Status200OK, null, cancellationToken, request.Reason.Trim());
        await TryAppendBusinessEventAsync("requirement.approved", "requirement", requirementId, actorUserId, "Approved requirement", request.Reason.Trim(), null, cancellationToken);
        return await SuccessAsync(requirementId, cancellationToken);
    }

    public async Task<RequirementCommandResult<RequirementDetailResponse>> BaselineRequirementAsync(Guid requirementId, string? actorUserId, CancellationToken cancellationToken)
    {
        var requirement = await dbContext.Set<RequirementEntity>().SingleOrDefaultAsync(x => x.Id == requirementId, cancellationToken);
        if (requirement is null)
        {
            return NotFound<RequirementDetailResponse>(ApiErrorCodes.RequirementNotFound, "Requirement not found.");
        }

        if (!string.Equals(requirement.Status, "approved", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<RequirementDetailResponse>(ApiErrorCodes.RequirementTransitionNotAllowed, "Only approved requirements can be baselined.");
        }

        var traceability = await LoadTraceabilityAsync(requirementId, cancellationToken);
        var missing = MissingTargetTypes(traceability);
        if (missing.Count > 0)
        {
            return Validation<RequirementDetailResponse>(ApiErrorCodes.TraceabilityIncomplete, $"Missing required traceability links: {string.Join(", ", missing)}.");
        }

        dbContext.Entry(requirement).CurrentValues.SetValues(requirement with { Status = "baselined", UpdatedAt = DateTimeOffset.UtcNow });
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("baseline", "requirement", requirementId, StatusCodes.Status200OK, null, cancellationToken);
        await TryAppendBusinessEventAsync("requirement.baselined", "requirement", requirementId, actorUserId, "Baselined requirement", null, null, cancellationToken);
        return await SuccessAsync(requirementId, cancellationToken);
    }

    public async Task<RequirementCommandResult<RequirementDetailResponse>> SupersedeRequirementAsync(Guid requirementId, RequirementDecisionRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return Validation<RequirementDetailResponse>(ApiErrorCodes.ReasonRequired, "Supersede reason is required.");
        }

        var requirement = await dbContext.Set<RequirementEntity>().SingleOrDefaultAsync(x => x.Id == requirementId, cancellationToken);
        if (requirement is null)
        {
            return NotFound<RequirementDetailResponse>(ApiErrorCodes.RequirementNotFound, "Requirement not found.");
        }

        dbContext.Entry(requirement).CurrentValues.SetValues(requirement with { Status = "superseded", UpdatedAt = DateTimeOffset.UtcNow });
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("supersede", "requirement", requirementId, StatusCodes.Status200OK, null, cancellationToken, request.Reason.Trim());
        await TryAppendBusinessEventAsync("requirement.superseded", "requirement", requirementId, actorUserId, "Superseded requirement", request.Reason.Trim(), null, cancellationToken);
        return await SuccessAsync(requirementId, cancellationToken);
    }

    public async Task<RequirementCommandResult<RequirementBaselineItem>> CreateBaselineAsync(CreateRequirementBaselineRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.BaselineName))
        {
            return Validation<RequirementBaselineItem>(ApiErrorCodes.BaselineNameRequired, "Baseline name is required.");
        }

        if (request.RequirementIds.Count == 0)
        {
            return Validation<RequirementBaselineItem>(ApiErrorCodes.RequestValidationFailed, "At least one requirement is required.");
        }

        var projectExists = await dbContext.Projects.AsNoTracking().AnyAsync(x => x.Id == request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            return NotFound<RequirementBaselineItem>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        var requirements = await dbContext.Set<RequirementEntity>()
            .Where(x => request.RequirementIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (requirements.Count != request.RequirementIds.Count)
        {
            return NotFound<RequirementBaselineItem>(ApiErrorCodes.RequirementNotFound, "One or more requirements were not found.");
        }

        var invalid = requirements.Where(x => !string.Equals(x.Status, "approved", StringComparison.OrdinalIgnoreCase)).Select(x => x.Code).ToArray();
        if (invalid.Length > 0)
        {
            return Validation<RequirementBaselineItem>(ApiErrorCodes.RequirementNotApproved, $"Requirements must be approved before baseline: {string.Join(", ", invalid)}.");
        }

        foreach (var requirement in requirements)
        {
            var missing = MissingTargetTypes(await LoadTraceabilityAsync(requirement.Id, cancellationToken));
            if (missing.Count > 0)
            {
                return Validation<RequirementBaselineItem>(ApiErrorCodes.TraceabilityIncomplete, $"Requirement {requirement.Code} is missing required traceability links: {string.Join(", ", missing)}.");
            }
        }

        var now = DateTimeOffset.UtcNow;
        var baseline = new RequirementBaselineEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            BaselineName = request.BaselineName.Trim(),
            RequirementIdsJson = JsonSerializer.Serialize(request.RequirementIds.Distinct(), SerializerOptions),
            Reason = request.Reason.Trim(),
            ApprovedBy = actorUserId ?? "unknown",
            ApprovedAt = now,
            Status = "locked"
        };

        dbContext.Add(baseline);
        foreach (var requirement in requirements)
        {
            dbContext.Entry(requirement).CurrentValues.SetValues(requirement with { Status = "baselined", UpdatedAt = now });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("baseline_create", "requirement_baseline", baseline.Id, StatusCodes.Status201Created, new { baseline.ProjectId, Count = request.RequirementIds.Count }, cancellationToken, request.Reason.Trim());
        await TryAppendBusinessEventAsync("requirement.baseline.created", "requirement_baseline", baseline.Id, actorUserId, "Created requirement baseline", request.Reason.Trim(), new { request.RequirementIds.Count }, cancellationToken);

        var projectName = await dbContext.Projects.AsNoTracking().Where(x => x.Id == request.ProjectId).Select(x => x.Name).SingleAsync(cancellationToken);
        return new RequirementCommandResult<RequirementBaselineItem>(
            RequirementCommandStatus.Success,
            new RequirementBaselineItem(baseline.Id, baseline.ProjectId, projectName, baseline.BaselineName, request.RequirementIds.Distinct().ToList(), baseline.Status, baseline.ApprovedBy, baseline.ApprovedAt));
    }

    public async Task<RequirementCommandResult<TraceabilityLinkItem>> CreateTraceabilityLinkAsync(CreateTraceabilityLinkRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SourceType)
            || string.IsNullOrWhiteSpace(request.SourceId)
            || string.IsNullOrWhiteSpace(request.TargetType)
            || string.IsNullOrWhiteSpace(request.TargetId)
            || string.IsNullOrWhiteSpace(request.LinkRule))
        {
            return Validation<TraceabilityLinkItem>(ApiErrorCodes.RequestValidationFailed, "Traceability source, target, and rule are required.");
        }

        var sourceRequirementExists = request.SourceType.Trim().Equals("requirement", StringComparison.OrdinalIgnoreCase)
            && Guid.TryParse(request.SourceId, out var requirementId)
            && await dbContext.Set<RequirementEntity>().AsNoTracking().AnyAsync(x => x.Id == requirementId, cancellationToken);

        if (!sourceRequirementExists)
        {
            return NotFound<TraceabilityLinkItem>(ApiErrorCodes.RequirementNotFound, "Source requirement not found.");
        }

        var exists = await dbContext.Set<TraceabilityLinkEntity>().AsNoTracking().AnyAsync(
            x => x.SourceType == request.SourceType.Trim()
                && x.SourceId == request.SourceId.Trim()
                && x.TargetType == request.TargetType.Trim()
                && x.TargetId == request.TargetId.Trim()
                && x.LinkRule == request.LinkRule.Trim(),
            cancellationToken);

        if (exists)
        {
            return Conflict<TraceabilityLinkItem>(ApiErrorCodes.TraceabilityLinkExists, "Traceability link already exists.");
        }

        var entity = new TraceabilityLinkEntity
        {
            Id = Guid.NewGuid(),
            SourceType = request.SourceType.Trim(),
            SourceId = request.SourceId.Trim(),
            TargetType = request.TargetType.Trim(),
            TargetId = request.TargetId.Trim(),
            LinkRule = request.LinkRule.Trim(),
            Status = "created",
            CreatedBy = actorUserId ?? "unknown",
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("traceability_create", "traceability_link", entity.Id, StatusCodes.Status201Created, new { entity.SourceId, entity.TargetType, entity.TargetId }, cancellationToken);
        await TryAppendBusinessEventAsync("traceability.created", "traceability_link", entity.Id, actorUserId, "Created traceability link", null, new { entity.SourceId, entity.TargetType, entity.TargetId }, cancellationToken);

        return new RequirementCommandResult<TraceabilityLinkItem>(
            RequirementCommandStatus.Success,
            new TraceabilityLinkItem(entity.Id, entity.SourceType, entity.SourceId, entity.TargetType, entity.TargetId, entity.LinkRule, entity.Status, entity.CreatedBy, entity.CreatedAt));
    }

    public async Task<RequirementCommandResult<bool>> DeleteTraceabilityLinkAsync(Guid linkId, string? actorUserId, CancellationToken cancellationToken)
    {
        var link = await dbContext.Set<TraceabilityLinkEntity>().SingleOrDefaultAsync(x => x.Id == linkId, cancellationToken);
        if (link is null)
        {
            return NotFound<bool>(ApiErrorCodes.TraceabilityLinkNotFound, "Traceability link not found.");
        }

        dbContext.Remove(link);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("traceability_delete", "traceability_link", linkId, StatusCodes.Status200OK, null, cancellationToken);
        await TryAppendBusinessEventAsync("traceability.deleted", "traceability_link", linkId, actorUserId, "Deleted traceability link", null, new { link.SourceId, link.TargetType, link.TargetId }, cancellationToken);
        return new RequirementCommandResult<bool>(RequirementCommandStatus.Success, true);
    }

    private async Task<RequirementCommandResult<RequirementDetailResponse>?> ValidateCreateRequestAsync(CreateRequirementRequest request, CancellationToken cancellationToken)
    {
        if (request.ProjectId == Guid.Empty)
        {
            return Validation<RequirementDetailResponse>(ApiErrorCodes.ProjectNotFound, "Project is required.");
        }

        var projectExists = await dbContext.Projects.AsNoTracking().AnyAsync(x => x.Id == request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            return NotFound<RequirementDetailResponse>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        if (string.IsNullOrWhiteSpace(request.Code)
            || string.IsNullOrWhiteSpace(request.Title)
            || string.IsNullOrWhiteSpace(request.Description)
            || string.IsNullOrWhiteSpace(request.BusinessReason)
            || string.IsNullOrWhiteSpace(request.AcceptanceCriteria)
            || string.IsNullOrWhiteSpace(request.OwnerUserId))
        {
            return Validation<RequirementDetailResponse>(ApiErrorCodes.RequestValidationFailed, "Code, title, description, owner, business reason, and acceptance criteria are required.");
        }

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var duplicate = await dbContext.Set<RequirementEntity>().AsNoTracking()
            .AnyAsync(x => x.ProjectId == request.ProjectId && x.Code == normalizedCode, cancellationToken);
        if (duplicate)
        {
            return Conflict<RequirementDetailResponse>(ApiErrorCodes.RequirementCodeDuplicate, "Requirement code already exists in the project.");
        }

        return null;
    }

    private async Task<RequirementVersionEntity?> GetCurrentVersionAsync(RequirementEntity requirement, CancellationToken cancellationToken)
    {
        if (!requirement.CurrentVersionId.HasValue)
        {
            return null;
        }

        return await dbContext.Set<RequirementVersionEntity>()
            .SingleOrDefaultAsync(x => x.Id == requirement.CurrentVersionId.Value && x.RequirementId == requirement.Id, cancellationToken);
    }

    private async Task<IReadOnlyList<TraceabilityLinkEntity>> LoadTraceabilityAsync(Guid requirementId, CancellationToken cancellationToken) =>
        await dbContext.Set<TraceabilityLinkEntity>().AsNoTracking()
            .Where(x => x.SourceType == "requirement" && x.SourceId == requirementId.ToString() && x.Status != "broken")
            .ToListAsync(cancellationToken);

    private static IReadOnlyList<string> MissingTargetTypes(IReadOnlyList<TraceabilityLinkEntity> links)
    {
        var targetTypes = links.Select(x => x.TargetType).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return RequiredTargetTypes.Where(targetType => !targetTypes.Contains(targetType)).ToArray();
    }

    private async Task<RequirementCommandResult<RequirementDetailResponse>> SuccessAsync(Guid requirementId, CancellationToken cancellationToken, int _ = StatusCodes.Status200OK)
    {
        var detail = await queries.GetRequirementAsync(requirementId, cancellationToken);
        return new RequirementCommandResult<RequirementDetailResponse>(RequirementCommandStatus.Success, detail);
    }

    private async Task AppendAuditAsync(string action, string entityType, Guid entityId, int statusCode, object? metadata, CancellationToken cancellationToken, string? reason = null)
    {
        auditLogWriter.Append(new AuditLogEntry(
            Module: "requirements",
            Action: action,
            EntityType: entityType,
            EntityId: entityId.ToString(),
            StatusCode: statusCode,
            Reason: reason,
            Metadata: metadata));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task TryAppendBusinessEventAsync(string eventType, string entityType, Guid entityId, string? actorUserId, string summary, string? reason, object? metadata, CancellationToken cancellationToken)
    {
        try
        {
            await businessAuditEventWriter.AppendAsync(
                "requirements",
                eventType,
                entityType,
                entityId.ToString(),
                summary,
                reason,
                new { ActorUserId = actorUserId, Metadata = metadata },
                cancellationToken);
        }
        catch
        {
            // Best-effort business audit.
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static RequirementCommandResult<T> Validation<T>(string errorCode, string message) =>
        new(RequirementCommandStatus.ValidationError, default, message, errorCode);

    private static RequirementCommandResult<T> NotFound<T>(string errorCode, string message) =>
        new(RequirementCommandStatus.NotFound, default, message, errorCode);

    private static RequirementCommandResult<T> Conflict<T>(string errorCode, string message) =>
        new(RequirementCommandStatus.Conflict, default, message, errorCode);
}
