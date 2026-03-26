using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Audits.Application;
using Operis_API.Modules.ChangeControl.Contracts;
using Operis_API.Modules.ChangeControl.Infrastructure;
using Operis_API.Modules.Governance.Infrastructure;
using Operis_API.Modules.Requirements.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.ChangeControl.Application;

public sealed class ChangeControlCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IBusinessAuditEventWriter businessAuditEventWriter,
    IChangeControlQueries queries) : IChangeControlCommands
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<ChangeControlCommandResult<ChangeRequestResponse>> CreateChangeRequestAsync(CreateChangeRequestRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var validation = await ValidateChangeRequestAsync(request, null, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        if (await dbContext.Set<ChangeRequestEntity>().AsNoTracking().AnyAsync(x => x.ProjectId == request.ProjectId && x.Code == normalizedCode, cancellationToken))
        {
            return Conflict<ChangeRequestResponse>(ApiErrorCodes.ChangeRequestCodeDuplicate, "Change request code already exists in the project.");
        }

        var now = DateTimeOffset.UtcNow;
        var changeRequest = new ChangeRequestEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Code = normalizedCode,
            Title = request.Title.Trim(),
            RequestedBy = request.RequestedBy.Trim(),
            Reason = request.Reason.Trim(),
            Status = "draft",
            Priority = request.Priority.Trim().ToLowerInvariant(),
            TargetBaselineId = request.TargetBaselineId,
            LinkedRequirementIdsJson = SerializeGuidList(request.LinkedRequirementIds),
            LinkedConfigurationItemIdsJson = SerializeGuidList(request.LinkedConfigurationItemIds),
            CreatedAt = now,
            UpdatedAt = now
        };

        var impact = new ChangeImpactEntity
        {
            Id = Guid.NewGuid(),
            ChangeRequestId = changeRequest.Id,
            ScopeImpact = request.Impact.ScopeImpact.Trim(),
            ScheduleImpact = request.Impact.ScheduleImpact.Trim(),
            QualityImpact = request.Impact.QualityImpact.Trim(),
            SecurityImpact = request.Impact.SecurityImpact.Trim(),
            PerformanceImpact = request.Impact.PerformanceImpact.Trim(),
            RiskImpact = request.Impact.RiskImpact.Trim()
        };

        dbContext.Add(changeRequest);
        dbContext.Add(impact);
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("create", "change_request", changeRequest.Id, StatusCodes.Status201Created, new { changeRequest.Code, changeRequest.Status }, cancellationToken);
        await AppendBusinessEventAsync("change_request_created", "change_request", changeRequest.Id, actorUserId, "Created change request", null, new { changeRequest.Code }, cancellationToken);
        return await SuccessChangeRequestAsync(changeRequest.Id, cancellationToken, StatusCodes.Status201Created);
    }

    public async Task<ChangeControlCommandResult<ChangeRequestResponse>> UpdateChangeRequestAsync(Guid changeRequestId, UpdateChangeRequestRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var changeRequest = await dbContext.Set<ChangeRequestEntity>().SingleOrDefaultAsync(x => x.Id == changeRequestId, cancellationToken);
        if (changeRequest is null)
        {
            return NotFound<ChangeRequestResponse>(ApiErrorCodes.ChangeRequestNotFound, "Change request not found.");
        }

        if (changeRequest.Status is not "draft" and not "submitted")
        {
            return Validation<ChangeRequestResponse>(ApiErrorCodes.ChangeRequestTransitionNotAllowed, "Only draft or submitted change requests can be updated.");
        }

        var validation = await ValidateChangeRequestAsync(
            new CreateChangeRequestRequest(changeRequest.ProjectId, changeRequest.Code, request.Title, request.RequestedBy, request.Reason, request.Priority, request.TargetBaselineId, request.Impact, request.LinkedRequirementIds, request.LinkedConfigurationItemIds),
            changeRequestId,
            cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var impact = await dbContext.Set<ChangeImpactEntity>().SingleOrDefaultAsync(x => x.ChangeRequestId == changeRequestId, cancellationToken);
        if (impact is null)
        {
            return NotFound<ChangeRequestResponse>(ApiErrorCodes.ChangeRequestImpactNotFound, "Change impact not found.");
        }

        var now = DateTimeOffset.UtcNow;
        dbContext.Entry(changeRequest).CurrentValues.SetValues(changeRequest with
        {
            Title = request.Title.Trim(),
            RequestedBy = request.RequestedBy.Trim(),
            Reason = request.Reason.Trim(),
            Priority = request.Priority.Trim().ToLowerInvariant(),
            TargetBaselineId = request.TargetBaselineId,
            LinkedRequirementIdsJson = SerializeGuidList(request.LinkedRequirementIds),
            LinkedConfigurationItemIdsJson = SerializeGuidList(request.LinkedConfigurationItemIds),
            UpdatedAt = now
        });
        dbContext.Entry(impact).CurrentValues.SetValues(impact with
        {
            ScopeImpact = request.Impact.ScopeImpact.Trim(),
            ScheduleImpact = request.Impact.ScheduleImpact.Trim(),
            QualityImpact = request.Impact.QualityImpact.Trim(),
            SecurityImpact = request.Impact.SecurityImpact.Trim(),
            PerformanceImpact = request.Impact.PerformanceImpact.Trim(),
            RiskImpact = request.Impact.RiskImpact.Trim()
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("update", "change_request", changeRequestId, StatusCodes.Status200OK, null, cancellationToken);
        await AppendBusinessEventAsync("change_request_updated", "change_request", changeRequestId, actorUserId, "Updated change request", null, null, cancellationToken);
        return await SuccessChangeRequestAsync(changeRequestId, cancellationToken);
    }

    public async Task<ChangeControlCommandResult<ChangeRequestResponse>> SubmitChangeRequestAsync(Guid changeRequestId, string? actorUserId, CancellationToken cancellationToken)
    {
        var changeRequest = await dbContext.Set<ChangeRequestEntity>().SingleOrDefaultAsync(x => x.Id == changeRequestId, cancellationToken);
        if (changeRequest is null)
        {
            return NotFound<ChangeRequestResponse>(ApiErrorCodes.ChangeRequestNotFound, "Change request not found.");
        }

        if (!string.Equals(changeRequest.Status, "draft", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<ChangeRequestResponse>(ApiErrorCodes.ChangeRequestTransitionNotAllowed, "Only draft change requests can be submitted.");
        }

        if (!await HasFullImpactAsync(changeRequestId, cancellationToken))
        {
            return Validation<ChangeRequestResponse>(ApiErrorCodes.RequestValidationFailed, "Full impact section is required before submission.");
        }

        dbContext.Entry(changeRequest).CurrentValues.SetValues(changeRequest with { Status = "submitted", UpdatedAt = DateTimeOffset.UtcNow });
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("submit", "change_request", changeRequestId, StatusCodes.Status200OK, null, cancellationToken);
        await AppendBusinessEventAsync("change_request_submitted", "change_request", changeRequestId, actorUserId, "Submitted change request", null, null, cancellationToken);
        return await SuccessChangeRequestAsync(changeRequestId, cancellationToken);
    }

    public async Task<ChangeControlCommandResult<ChangeRequestResponse>> ApproveChangeRequestAsync(Guid changeRequestId, ChangeDecisionRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return Validation<ChangeRequestResponse>(ApiErrorCodes.DecisionReasonRequired, "Decision reason is required.");
        }

        var changeRequest = await dbContext.Set<ChangeRequestEntity>().SingleOrDefaultAsync(x => x.Id == changeRequestId, cancellationToken);
        if (changeRequest is null)
        {
            return NotFound<ChangeRequestResponse>(ApiErrorCodes.ChangeRequestNotFound, "Change request not found.");
        }

        if (!string.Equals(changeRequest.Status, "submitted", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<ChangeRequestResponse>(ApiErrorCodes.ChangeRequestTransitionNotAllowed, "Only submitted change requests can be approved.");
        }

        var now = DateTimeOffset.UtcNow;
        dbContext.Entry(changeRequest).CurrentValues.SetValues(changeRequest with
        {
            Status = "approved",
            DecisionRationale = request.Reason.Trim(),
            ApprovedBy = actorUserId ?? "unknown",
            ApprovedAt = now,
            UpdatedAt = now
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("approve", "change_request", changeRequestId, StatusCodes.Status200OK, null, cancellationToken, request.Reason.Trim());
        await AppendBusinessEventAsync("change_request_approved", "change_request", changeRequestId, actorUserId, "Approved change request", request.Reason.Trim(), null, cancellationToken);
        return await SuccessChangeRequestAsync(changeRequestId, cancellationToken);
    }

    public async Task<ChangeControlCommandResult<ChangeRequestResponse>> RejectChangeRequestAsync(Guid changeRequestId, ChangeDecisionRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return Validation<ChangeRequestResponse>(ApiErrorCodes.DecisionReasonRequired, "Decision reason is required.");
        }

        var changeRequest = await dbContext.Set<ChangeRequestEntity>().SingleOrDefaultAsync(x => x.Id == changeRequestId, cancellationToken);
        if (changeRequest is null)
        {
            return NotFound<ChangeRequestResponse>(ApiErrorCodes.ChangeRequestNotFound, "Change request not found.");
        }

        if (!string.Equals(changeRequest.Status, "submitted", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<ChangeRequestResponse>(ApiErrorCodes.ChangeRequestTransitionNotAllowed, "Only submitted change requests can be rejected.");
        }

        dbContext.Entry(changeRequest).CurrentValues.SetValues(changeRequest with
        {
            Status = "rejected",
            DecisionRationale = request.Reason.Trim(),
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("reject", "change_request", changeRequestId, StatusCodes.Status200OK, null, cancellationToken, request.Reason.Trim());
        await AppendBusinessEventAsync("change_request_rejected", "change_request", changeRequestId, actorUserId, "Rejected change request", request.Reason.Trim(), null, cancellationToken);
        return await SuccessChangeRequestAsync(changeRequestId, cancellationToken);
    }

    public async Task<ChangeControlCommandResult<ChangeRequestResponse>> ImplementChangeRequestAsync(Guid changeRequestId, ChangeImplementationRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Summary))
        {
            return Validation<ChangeRequestResponse>(ApiErrorCodes.ImplementationSummaryRequired, "Implementation summary is required.");
        }

        var changeRequest = await dbContext.Set<ChangeRequestEntity>().SingleOrDefaultAsync(x => x.Id == changeRequestId, cancellationToken);
        if (changeRequest is null)
        {
            return NotFound<ChangeRequestResponse>(ApiErrorCodes.ChangeRequestNotFound, "Change request not found.");
        }

        if (!string.Equals(changeRequest.Status, "approved", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<ChangeRequestResponse>(ApiErrorCodes.ChangeRequestTransitionNotAllowed, "Only approved change requests can be implemented.");
        }

        if (!changeRequest.TargetBaselineId.HasValue && !HasLinkedConfigurationScope(changeRequest))
        {
            return Validation<ChangeRequestResponse>(ApiErrorCodes.RequestValidationFailed, "Implementation requires linked baseline or configuration item scope.");
        }

        dbContext.Entry(changeRequest).CurrentValues.SetValues(changeRequest with
        {
            Status = "implemented",
            ImplementationSummary = request.Summary.Trim(),
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("implement", "change_request", changeRequestId, StatusCodes.Status200OK, null, cancellationToken, request.Summary.Trim());
        await AppendBusinessEventAsync("change_request_implemented", "change_request", changeRequestId, actorUserId, "Implemented change request", request.Summary.Trim(), null, cancellationToken);
        return await SuccessChangeRequestAsync(changeRequestId, cancellationToken);
    }

    public async Task<ChangeControlCommandResult<ChangeRequestResponse>> CloseChangeRequestAsync(Guid changeRequestId, ChangeImplementationRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Summary))
        {
            return Validation<ChangeRequestResponse>(ApiErrorCodes.ImplementationSummaryRequired, "Implementation summary is required.");
        }

        var changeRequest = await dbContext.Set<ChangeRequestEntity>().SingleOrDefaultAsync(x => x.Id == changeRequestId, cancellationToken);
        if (changeRequest is null)
        {
            return NotFound<ChangeRequestResponse>(ApiErrorCodes.ChangeRequestNotFound, "Change request not found.");
        }

        if (!string.Equals(changeRequest.Status, "implemented", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<ChangeRequestResponse>(ApiErrorCodes.ChangeRequestTransitionNotAllowed, "Only implemented change requests can be closed.");
        }

        dbContext.Entry(changeRequest).CurrentValues.SetValues(changeRequest with
        {
            Status = "closed",
            ImplementationSummary = request.Summary.Trim(),
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("close", "change_request", changeRequestId, StatusCodes.Status200OK, null, cancellationToken, request.Summary.Trim());
        await AppendBusinessEventAsync("change_request_closed", "change_request", changeRequestId, actorUserId, "Closed change request", request.Summary.Trim(), null, cancellationToken);
        return await SuccessChangeRequestAsync(changeRequestId, cancellationToken);
    }

    public async Task<ChangeControlCommandResult<ConfigurationItemResponse>> CreateConfigurationItemAsync(CreateConfigurationItemRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var validation = await ValidateConfigurationItemAsync(request.ProjectId, request.Code, request.Name, request.ItemType, request.OwnerModule, null, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var now = DateTimeOffset.UtcNow;
        var entity = new ConfigurationItemEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Code = normalizedCode,
            Name = request.Name.Trim(),
            ItemType = request.ItemType.Trim(),
            OwnerModule = request.OwnerModule.Trim(),
            Status = "draft",
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("create", "configuration_item", entity.Id, StatusCodes.Status201Created, new { entity.Code }, cancellationToken);
        await AppendBusinessEventAsync("configuration_item_created", "configuration_item", entity.Id, actorUserId, "Created configuration item", null, new { entity.Code }, cancellationToken);
        return await SuccessConfigurationItemAsync(entity.Id, cancellationToken, StatusCodes.Status201Created);
    }

    public async Task<ChangeControlCommandResult<ConfigurationItemResponse>> UpdateConfigurationItemAsync(Guid configurationItemId, UpdateConfigurationItemRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<ConfigurationItemEntity>().SingleOrDefaultAsync(x => x.Id == configurationItemId, cancellationToken);
        if (entity is null)
        {
            return NotFound<ConfigurationItemResponse>(ApiErrorCodes.ConfigurationItemNotFound, "Configuration item not found.");
        }

        if (entity.Status is "baseline" or "superseded")
        {
            return Validation<ConfigurationItemResponse>(ApiErrorCodes.ConfigurationItemTransitionNotAllowed, "Baselined or superseded configuration items cannot be edited.");
        }

        var validation = await ValidateConfigurationItemAsync(entity.ProjectId, entity.Code, request.Name, request.ItemType, request.OwnerModule, configurationItemId, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Name = request.Name.Trim(),
            ItemType = request.ItemType.Trim(),
            OwnerModule = request.OwnerModule.Trim(),
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("update", "configuration_item", configurationItemId, StatusCodes.Status200OK, null, cancellationToken);
        await AppendBusinessEventAsync("configuration_item_updated", "configuration_item", configurationItemId, actorUserId, "Updated configuration item", null, null, cancellationToken);
        return await SuccessConfigurationItemAsync(configurationItemId, cancellationToken);
    }

    public async Task<ChangeControlCommandResult<ConfigurationItemResponse>> ApproveConfigurationItemAsync(Guid configurationItemId, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<ConfigurationItemEntity>().SingleOrDefaultAsync(x => x.Id == configurationItemId, cancellationToken);
        if (entity is null)
        {
            return NotFound<ConfigurationItemResponse>(ApiErrorCodes.ConfigurationItemNotFound, "Configuration item not found.");
        }

        if (!string.Equals(entity.Status, "draft", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<ConfigurationItemResponse>(ApiErrorCodes.ConfigurationItemTransitionNotAllowed, "Only draft configuration items can be approved.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with { Status = "approved", UpdatedAt = DateTimeOffset.UtcNow });
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("approve", "configuration_item", configurationItemId, StatusCodes.Status200OK, null, cancellationToken);
        await AppendBusinessEventAsync("configuration_item_approved", "configuration_item", configurationItemId, actorUserId, "Approved configuration item", null, null, cancellationToken);
        return await SuccessConfigurationItemAsync(configurationItemId, cancellationToken);
    }

    public async Task<ChangeControlCommandResult<BaselineRegistryResponse>> CreateBaselineRegistryAsync(CreateBaselineRegistryRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.BaselineName))
        {
            return Validation<BaselineRegistryResponse>(ApiErrorCodes.BaselineNameRequired, "Baseline name is required.");
        }

        if (!await dbContext.Projects.AsNoTracking().AnyAsync(x => x.Id == request.ProjectId, cancellationToken))
        {
            return NotFound<BaselineRegistryResponse>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        var changeRequest = await dbContext.Set<ChangeRequestEntity>().SingleOrDefaultAsync(x => x.Id == request.ChangeRequestId, cancellationToken);
        if (changeRequest is null || !string.Equals(changeRequest.Status, "approved", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<BaselineRegistryResponse>(ApiErrorCodes.ApprovedChangeRequestRequired, "An approved change request is required for baseline changes.");
        }

        if (changeRequest.ProjectId != request.ProjectId)
        {
            return Validation<BaselineRegistryResponse>(ApiErrorCodes.ApprovedChangeRequestRequired, "Change request must belong to the same project.");
        }

        var sourceValidation = await ValidateBaselineSourceAsync(request.SourceEntityType, request.SourceEntityId, request.ProjectId, cancellationToken);
        if (sourceValidation is not null)
        {
            return sourceValidation;
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new BaselineRegistryEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            BaselineName = request.BaselineName.Trim(),
            BaselineType = request.BaselineType.Trim(),
            SourceEntityType = request.SourceEntityType.Trim(),
            SourceEntityId = request.SourceEntityId.Trim(),
            Status = "proposed",
            ChangeRequestId = request.ChangeRequestId,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("create", "baseline_registry", entity.Id, StatusCodes.Status201Created, new { entity.BaselineName, entity.SourceEntityType }, cancellationToken);
        await AppendBusinessEventAsync("baseline_registry_created", "baseline_registry", entity.Id, actorUserId, "Created baseline registry record", null, new { entity.BaselineName }, cancellationToken);
        return await SuccessBaselineAsync(entity.Id, cancellationToken, StatusCodes.Status201Created);
    }

    public async Task<ChangeControlCommandResult<BaselineRegistryResponse>> ApproveBaselineRegistryAsync(Guid baselineRegistryId, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<BaselineRegistryEntity>().SingleOrDefaultAsync(x => x.Id == baselineRegistryId, cancellationToken);
        if (entity is null)
        {
            return NotFound<BaselineRegistryResponse>(ApiErrorCodes.BaselineRegistryNotFound, "Baseline registry record not found.");
        }

        if (!string.Equals(entity.Status, "proposed", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<BaselineRegistryResponse>(ApiErrorCodes.ChangeRequestTransitionNotAllowed, "Only proposed baseline records can be approved.");
        }

        var now = DateTimeOffset.UtcNow;
        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Status = "locked",
            ApprovedBy = actorUserId ?? "unknown",
            ApprovedAt = now,
            UpdatedAt = now
        });

        await ApplySourceBaselineAsync(entity.SourceEntityType, entity.SourceEntityId, entity.BaselineName, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("approve", "baseline_registry", baselineRegistryId, StatusCodes.Status200OK, null, cancellationToken);
        await AppendBusinessEventAsync("baseline_registry_approved", "baseline_registry", baselineRegistryId, actorUserId, "Approved baseline registry record", null, null, cancellationToken);
        return await SuccessBaselineAsync(baselineRegistryId, cancellationToken);
    }

    public async Task<ChangeControlCommandResult<BaselineRegistryResponse>> SupersedeBaselineRegistryAsync(Guid baselineRegistryId, BaselineOverrideRequest request, string? actorUserId, bool canEmergencyOverride, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<BaselineRegistryEntity>().SingleOrDefaultAsync(x => x.Id == baselineRegistryId, cancellationToken);
        if (entity is null)
        {
            return NotFound<BaselineRegistryResponse>(ApiErrorCodes.BaselineRegistryNotFound, "Baseline registry record not found.");
        }

        if (!string.Equals(entity.Status, "locked", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<BaselineRegistryResponse>(ApiErrorCodes.ChangeRequestTransitionNotAllowed, "Only locked baseline records can be superseded.");
        }

        if (request.EmergencyOverride)
        {
            if (!canEmergencyOverride)
            {
                return Validation<BaselineRegistryResponse>(ApiErrorCodes.ApprovedChangeRequestRequired, "Emergency override requires elevated permission.");
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return Validation<BaselineRegistryResponse>(ApiErrorCodes.EmergencyOverrideReasonRequired, "Emergency override reason is required.");
            }
        }

        BaselineRegistryEntity? replacementBaseline = null;
        if (request.SupersededByBaselineId.HasValue)
        {
            replacementBaseline = await dbContext.Set<BaselineRegistryEntity>().SingleOrDefaultAsync(x => x.Id == request.SupersededByBaselineId.Value, cancellationToken);
            if (replacementBaseline is null)
            {
                return NotFound<BaselineRegistryResponse>(ApiErrorCodes.BaselineRegistryNotFound, "Superseding baseline record not found.");
            }
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Status = "superseded",
            SupersededByBaselineId = replacementBaseline?.Id,
            OverrideReason = TrimOrNull(request.Reason),
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("supersede", "baseline_registry", baselineRegistryId, StatusCodes.Status200OK, null, cancellationToken, TrimOrNull(request.Reason));
        await AppendBusinessEventAsync("baseline_registry_superseded", "baseline_registry", baselineRegistryId, actorUserId, "Superseded baseline registry record", TrimOrNull(request.Reason), new { request.EmergencyOverride }, cancellationToken);
        return await SuccessBaselineAsync(baselineRegistryId, cancellationToken);
    }

    private async Task<ChangeControlCommandResult<ChangeRequestResponse>?> ValidateChangeRequestAsync(CreateChangeRequestRequest request, Guid? existingId, CancellationToken cancellationToken)
    {
        if (request.ProjectId == Guid.Empty || string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.RequestedBy) || string.IsNullOrWhiteSpace(request.Reason))
        {
            return Validation<ChangeRequestResponse>(ApiErrorCodes.RequestValidationFailed, "Project, code, title, requester, and reason are required.");
        }

        if (!await dbContext.Projects.AsNoTracking().AnyAsync(x => x.Id == request.ProjectId, cancellationToken))
        {
            return NotFound<ChangeRequestResponse>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        if (request.TargetBaselineId.HasValue
            && !await dbContext.Set<BaselineRegistryEntity>().AsNoTracking().AnyAsync(x => x.Id == request.TargetBaselineId.Value, cancellationToken))
        {
            return NotFound<ChangeRequestResponse>(ApiErrorCodes.BaselineNotFound, "Target baseline not found.");
        }

        if (!IsPriorityAllowed(request.Priority))
        {
            return Validation<ChangeRequestResponse>(ApiErrorCodes.RequestValidationFailed, "Priority is invalid.");
        }

        if (existingId is null && await dbContext.Set<ChangeRequestEntity>().AsNoTracking().AnyAsync(x => x.ProjectId == request.ProjectId && x.Code == request.Code.Trim().ToUpperInvariant(), cancellationToken))
        {
            return Conflict<ChangeRequestResponse>(ApiErrorCodes.ChangeRequestCodeDuplicate, "Change request code already exists in the project.");
        }

        if (request.LinkedConfigurationItemIds?.Count > 0)
        {
            var count = await dbContext.Set<ConfigurationItemEntity>().AsNoTracking().CountAsync(x => request.LinkedConfigurationItemIds.Contains(x.Id), cancellationToken);
            if (count != request.LinkedConfigurationItemIds.Distinct().Count())
            {
                return NotFound<ChangeRequestResponse>(ApiErrorCodes.ConfigurationItemNotFound, "One or more linked configuration items were not found.");
            }
        }

        if (request.LinkedRequirementIds?.Count > 0)
        {
            var count = await dbContext.Set<RequirementEntity>().AsNoTracking().CountAsync(x => request.LinkedRequirementIds.Contains(x.Id), cancellationToken);
            if (count != request.LinkedRequirementIds.Distinct().Count())
            {
                return NotFound<ChangeRequestResponse>(ApiErrorCodes.RequirementNotFound, "One or more linked requirements were not found.");
            }
        }

        if (!HasFullImpact(request.Impact))
        {
            return Validation<ChangeRequestResponse>(ApiErrorCodes.RequestValidationFailed, "Full impact analysis is required.");
        }

        return null;
    }

    private async Task<ChangeControlCommandResult<ConfigurationItemResponse>?> ValidateConfigurationItemAsync(Guid projectId, string code, string name, string itemType, string ownerModule, Guid? existingId, CancellationToken cancellationToken)
    {
        if (projectId == Guid.Empty || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(itemType) || string.IsNullOrWhiteSpace(ownerModule))
        {
            return Validation<ConfigurationItemResponse>(ApiErrorCodes.RequestValidationFailed, "Project, code, name, item type, and owner module are required.");
        }

        if (!await dbContext.Projects.AsNoTracking().AnyAsync(x => x.Id == projectId, cancellationToken))
        {
            return NotFound<ConfigurationItemResponse>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        var normalizedCode = code.Trim().ToUpperInvariant();
        var duplicate = await dbContext.Set<ConfigurationItemEntity>().AsNoTracking()
            .AnyAsync(x => x.ProjectId == projectId && x.Code == normalizedCode && (!existingId.HasValue || x.Id != existingId.Value), cancellationToken);
        if (duplicate)
        {
            return Conflict<ConfigurationItemResponse>(ApiErrorCodes.ConfigurationItemCodeDuplicate, "Configuration item code already exists in the project.");
        }

        return null;
    }

    private async Task<ChangeControlCommandResult<BaselineRegistryResponse>?> ValidateBaselineSourceAsync(string sourceEntityType, string sourceEntityId, Guid projectId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sourceEntityType) || string.IsNullOrWhiteSpace(sourceEntityId))
        {
            return Validation<BaselineRegistryResponse>(ApiErrorCodes.RequestValidationFailed, "Source entity type and id are required.");
        }

        var normalizedType = sourceEntityType.Trim().ToLowerInvariant();
        if (!Guid.TryParse(sourceEntityId.Trim(), out var sourceGuid))
        {
            return Validation<BaselineRegistryResponse>(ApiErrorCodes.RequestValidationFailed, "Source entity id must be a valid GUID.");
        }

        var exists = normalizedType switch
        {
            "configuration_item" => await dbContext.Set<ConfigurationItemEntity>().AsNoTracking().AnyAsync(x => x.Id == sourceGuid && x.ProjectId == projectId && x.Status == "approved", cancellationToken),
            "requirement_baseline" => await dbContext.Set<RequirementBaselineEntity>().AsNoTracking().AnyAsync(x => x.Id == sourceGuid && x.ProjectId == projectId && x.Status == "locked", cancellationToken),
            "project_plan" => await dbContext.Set<ProjectPlanEntity>().AsNoTracking().AnyAsync(x => x.Id == sourceGuid && x.ProjectId == projectId && x.Status == "baseline", cancellationToken),
            "document" => await dbContext.Set<Operis_API.Modules.Documents.Infrastructure.DocumentEntity>().AsNoTracking().AnyAsync(x => x.Id == sourceGuid && x.ProjectId == projectId && x.Status == "baseline", cancellationToken),
            _ => false
        };

        return exists ? null : NotFound<BaselineRegistryResponse>(ApiErrorCodes.BaselineNotFound, "Baseline source not found or not eligible.");
    }

    private async Task ApplySourceBaselineAsync(string sourceEntityType, string sourceEntityId, string baselineName, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(sourceEntityId, out var sourceGuid))
        {
            return;
        }

        if (!string.Equals(sourceEntityType, "configuration_item", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var configurationItem = await dbContext.Set<ConfigurationItemEntity>().SingleOrDefaultAsync(x => x.Id == sourceGuid, cancellationToken);
        if (configurationItem is null)
        {
            return;
        }

        dbContext.Entry(configurationItem).CurrentValues.SetValues(configurationItem with
        {
            Status = "baseline",
            BaselineRef = baselineName,
            UpdatedAt = DateTimeOffset.UtcNow
        });
    }

    private async Task<bool> HasFullImpactAsync(Guid changeRequestId, CancellationToken cancellationToken) =>
        await dbContext.Set<ChangeImpactEntity>().AsNoTracking()
            .Where(x => x.ChangeRequestId == changeRequestId)
            .AnyAsync(x =>
                !string.IsNullOrWhiteSpace(x.ScopeImpact)
                && !string.IsNullOrWhiteSpace(x.ScheduleImpact)
                && !string.IsNullOrWhiteSpace(x.QualityImpact)
                && !string.IsNullOrWhiteSpace(x.SecurityImpact)
                && !string.IsNullOrWhiteSpace(x.PerformanceImpact)
                && !string.IsNullOrWhiteSpace(x.RiskImpact),
                cancellationToken);

    private static bool HasFullImpact(ChangeImpactRequest impact) =>
        !string.IsNullOrWhiteSpace(impact.ScopeImpact)
        && !string.IsNullOrWhiteSpace(impact.ScheduleImpact)
        && !string.IsNullOrWhiteSpace(impact.QualityImpact)
        && !string.IsNullOrWhiteSpace(impact.SecurityImpact)
        && !string.IsNullOrWhiteSpace(impact.PerformanceImpact)
        && !string.IsNullOrWhiteSpace(impact.RiskImpact);

    private static bool IsPriorityAllowed(string priority) =>
        priority.Trim().ToLowerInvariant() is "low" or "medium" or "high" or "critical";

    private static bool HasLinkedConfigurationScope(ChangeRequestEntity changeRequest) =>
        DeserializeGuidList(changeRequest.LinkedConfigurationItemIdsJson).Count > 0;

    private static string SerializeGuidList(IReadOnlyList<Guid>? ids) =>
        JsonSerializer.Serialize((ids ?? []).Distinct().ToArray(), SerializerOptions);

    private static IReadOnlyList<Guid> DeserializeGuidList(string json) =>
        JsonSerializer.Deserialize<List<Guid>>(json, SerializerOptions) ?? [];

    private async Task<ChangeControlCommandResult<ChangeRequestResponse>> SuccessChangeRequestAsync(Guid changeRequestId, CancellationToken cancellationToken, int _ = StatusCodes.Status200OK)
    {
        var value = await queries.GetChangeRequestAsync(changeRequestId, cancellationToken);
        return new ChangeControlCommandResult<ChangeRequestResponse>(ChangeControlCommandStatus.Success, value);
    }

    private async Task<ChangeControlCommandResult<ConfigurationItemResponse>> SuccessConfigurationItemAsync(Guid configurationItemId, CancellationToken cancellationToken, int _ = StatusCodes.Status200OK)
    {
        var value = await queries.GetConfigurationItemAsync(configurationItemId, cancellationToken);
        return new ChangeControlCommandResult<ConfigurationItemResponse>(ChangeControlCommandStatus.Success, value);
    }

    private async Task<ChangeControlCommandResult<BaselineRegistryResponse>> SuccessBaselineAsync(Guid baselineRegistryId, CancellationToken cancellationToken, int _ = StatusCodes.Status200OK)
    {
        var value = await queries.GetBaselineRegistryAsync(baselineRegistryId, cancellationToken);
        return new ChangeControlCommandResult<BaselineRegistryResponse>(ChangeControlCommandStatus.Success, value);
    }

    private async Task AppendAuditAsync(string action, string entityType, Guid entityId, int statusCode, object? metadata, CancellationToken cancellationToken, string? reason = null)
    {
        auditLogWriter.Append(new AuditLogEntry(
            Module: "change_control",
            Action: action,
            EntityType: entityType,
            EntityId: entityId.ToString(),
            StatusCode: statusCode,
            Reason: reason,
            Metadata: metadata));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task AppendBusinessEventAsync(string eventType, string entityType, Guid entityId, string? actorUserId, string summary, string? reason, object? metadata, CancellationToken cancellationToken)
    {
        try
        {
            await businessAuditEventWriter.AppendAsync("change_control", eventType, entityType, entityId.ToString(), summary, reason, new { ActorUserId = actorUserId, Metadata = metadata }, cancellationToken);
        }
        catch
        {
            // Best-effort business audit.
        }
    }

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ChangeControlCommandResult<T> Validation<T>(string errorCode, string message) =>
        new(ChangeControlCommandStatus.ValidationError, default, message, errorCode);

    private static ChangeControlCommandResult<T> NotFound<T>(string errorCode, string message) =>
        new(ChangeControlCommandStatus.NotFound, default, message, errorCode);

    private static ChangeControlCommandResult<T> Conflict<T>(string errorCode, string message) =>
        new(ChangeControlCommandStatus.Conflict, default, message, errorCode);
}
