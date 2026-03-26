using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Audits.Application;
using Operis_API.Modules.Governance.Contracts;
using Operis_API.Modules.Governance.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Governance.Application;

public sealed class GovernanceCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IBusinessAuditEventWriter businessAuditEventWriter) : IGovernanceCommands
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<GovernanceCommandResult<ProcessAssetResponse>> CreateProcessAssetAsync(CreateProcessAssetRequest request, CancellationToken cancellationToken)
    {
        var validation = ValidateProcessAssetRequest(request.Code, request.Name, request.Category, request.OwnerUserId, request.InitialVersionTitle, request.InitialVersionSummary);
        if (validation is not null)
        {
            return validation;
        }

        if (await dbContext.Set<ProcessAssetEntity>().AnyAsync(x => x.Code == request.Code.Trim(), cancellationToken))
        {
            return ValidationError<ProcessAssetResponse>("Process asset code already exists.", ApiErrorCodes.ChangeSummaryRequired);
        }

        var now = DateTimeOffset.UtcNow;
        var asset = new ProcessAssetEntity
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Category = request.Category.Trim(),
            Status = "draft",
            OwnerUserId = request.OwnerUserId.Trim(),
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            CreatedAt = now,
            UpdatedAt = now
        };

        var version = new ProcessAssetVersionEntity
        {
            Id = Guid.NewGuid(),
            ProcessAssetId = asset.Id,
            VersionNumber = 1,
            Title = request.InitialVersionTitle.Trim(),
            Summary = request.InitialVersionSummary.Trim(),
            ContentRef = TrimOrNull(request.InitialContentRef),
            Status = "draft",
            CreatedAt = now,
            UpdatedAt = now
        };

        asset.CurrentVersionId = version.Id;
        dbContext.Add(asset);
        dbContext.Add(version);
        await dbContext.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync("create", "process_asset", asset.Id.ToString(), StatusCodes.Status201Created, null, new { asset.Code, version.VersionNumber }, cancellationToken);
        await businessAuditEventWriter.AppendAsync("governance", "process_asset_created", "process_asset", asset.Id.ToString(), asset.Name, null, new { asset.Code, version.VersionNumber }, cancellationToken);

        return new GovernanceCommandResult<ProcessAssetResponse>(GovernanceCommandStatus.Success, await LoadProcessAssetAsync(asset.Id, cancellationToken));
    }

    public async Task<GovernanceCommandResult<ProcessAssetResponse>> UpdateProcessAssetAsync(Guid processAssetId, UpdateProcessAssetRequest request, CancellationToken cancellationToken)
    {
        var asset = await dbContext.Set<ProcessAssetEntity>().SingleOrDefaultAsync(x => x.Id == processAssetId, cancellationToken);
        if (asset is null)
        {
            return NotFound<ProcessAssetResponse>("Process asset not found.", ApiErrorCodes.ProcessAssetNotFound);
        }

        var validation = ValidateProcessAssetRequest(request.Code, request.Name, request.Category, request.OwnerUserId, "v", "v");
        if (validation is not null)
        {
            return validation;
        }

        if (await dbContext.Set<ProcessAssetEntity>().AnyAsync(x => x.Id != processAssetId && x.Code == request.Code.Trim(), cancellationToken))
        {
            return ValidationError<ProcessAssetResponse>("Process asset code already exists.", ApiErrorCodes.ChangeSummaryRequired);
        }

        var before = new { asset.Code, asset.Name, asset.Category, asset.Status, asset.OwnerUserId, asset.EffectiveFrom, asset.EffectiveTo };
        asset.Code = request.Code.Trim();
        asset.Name = request.Name.Trim();
        asset.Category = request.Category.Trim();
        asset.OwnerUserId = request.OwnerUserId.Trim();
        asset.EffectiveFrom = request.EffectiveFrom;
        asset.EffectiveTo = request.EffectiveTo;
        asset.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync("update", "process_asset", asset.Id.ToString(), StatusCodes.Status200OK, before, new { asset.Code, asset.Name, asset.Category, asset.OwnerUserId }, cancellationToken);
        return new GovernanceCommandResult<ProcessAssetResponse>(GovernanceCommandStatus.Success, await LoadProcessAssetAsync(asset.Id, cancellationToken));
    }

    public async Task<GovernanceCommandResult<ProcessAssetResponse>> CreateProcessAssetVersionAsync(Guid processAssetId, CreateProcessAssetVersionRequest request, CancellationToken cancellationToken)
    {
        var asset = await dbContext.Set<ProcessAssetEntity>().SingleOrDefaultAsync(x => x.Id == processAssetId, cancellationToken);
        if (asset is null)
        {
            return NotFound<ProcessAssetResponse>("Process asset not found.", ApiErrorCodes.ProcessAssetNotFound);
        }

        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Summary))
        {
            return ValidationError<ProcessAssetResponse>("Version title and summary are required.", ApiErrorCodes.RequestValidationFailed);
        }

        var nextVersion = (await dbContext.Set<ProcessAssetVersionEntity>()
            .Where(x => x.ProcessAssetId == processAssetId)
            .MaxAsync(x => (int?)x.VersionNumber, cancellationToken) ?? 0) + 1;

        var now = DateTimeOffset.UtcNow;
        var version = new ProcessAssetVersionEntity
        {
            Id = Guid.NewGuid(),
            ProcessAssetId = processAssetId,
            VersionNumber = nextVersion,
            Title = request.Title.Trim(),
            Summary = request.Summary.Trim(),
            ContentRef = TrimOrNull(request.ContentRef),
            Status = "draft",
            ChangeSummary = TrimOrNull(request.ChangeSummary),
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Add(version);
        asset.CurrentVersionId = version.Id;
        asset.Status = version.Status;
        asset.UpdatedAt = now;
        await dbContext.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync("create", "process_asset_version", version.Id.ToString(), StatusCodes.Status201Created, null, new { processAssetId, version.VersionNumber }, cancellationToken);
        return new GovernanceCommandResult<ProcessAssetResponse>(GovernanceCommandStatus.Success, await LoadProcessAssetAsync(processAssetId, cancellationToken));
    }

    public async Task<GovernanceCommandResult<ProcessAssetResponse>> UpdateProcessAssetVersionAsync(Guid processAssetId, Guid versionId, UpdateProcessAssetVersionRequest request, CancellationToken cancellationToken)
    {
        var version = await LoadProcessAssetVersionAsync(processAssetId, versionId, cancellationToken);
        if (version is null)
        {
            return NotFound<ProcessAssetResponse>("Process asset version not found.", ApiErrorCodes.ProcessAssetVersionNotFound);
        }

        if (version.Status is not "draft" and not "reviewed")
        {
            return Conflict<ProcessAssetResponse>("Version can only be edited in draft or reviewed status.", ApiErrorCodes.GovernanceTransitionNotAllowed);
        }

        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Summary))
        {
            return ValidationError<ProcessAssetResponse>("Version title and summary are required.", ApiErrorCodes.RequestValidationFailed);
        }

        version.Title = request.Title.Trim();
        version.Summary = request.Summary.Trim();
        version.ContentRef = TrimOrNull(request.ContentRef);
        version.ChangeSummary = TrimOrNull(request.ChangeSummary);
        version.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync("update", "process_asset_version", version.Id.ToString(), StatusCodes.Status200OK, null, new { version.Status, version.Title }, cancellationToken);
        return new GovernanceCommandResult<ProcessAssetResponse>(GovernanceCommandStatus.Success, await LoadProcessAssetAsync(processAssetId, cancellationToken));
    }

    public async Task<GovernanceCommandResult<GovernanceMutationResponse>> SubmitProcessAssetVersionReviewAsync(Guid processAssetId, Guid versionId, CancellationToken cancellationToken)
    {
        var version = await LoadProcessAssetVersionAsync(processAssetId, versionId, cancellationToken);
        if (version is null)
        {
            return NotFound<GovernanceMutationResponse>("Process asset version not found.", ApiErrorCodes.ProcessAssetVersionNotFound);
        }

        if (version.Status != "draft")
        {
            return Conflict<GovernanceMutationResponse>("Only draft versions can be submitted for review.", ApiErrorCodes.GovernanceTransitionNotAllowed);
        }

        version.Status = "reviewed";
        version.UpdatedAt = DateTimeOffset.UtcNow;
        await SyncProcessAssetStatusAsync(processAssetId, version.Id, version.Status, cancellationToken);

        await WriteAuditAsync("submit_review", "process_asset_version", version.Id.ToString(), StatusCodes.Status200OK, null, new { version.Status }, cancellationToken);
        await businessAuditEventWriter.AppendAsync("governance", "process_asset_version_reviewed", "process_asset_version", version.Id.ToString(), version.Title, null, new { processAssetId }, cancellationToken);
        return Success(version);
    }

    public async Task<GovernanceCommandResult<GovernanceMutationResponse>> ApproveProcessAssetVersionAsync(Guid processAssetId, Guid versionId, string actor, ProcessAssetApprovalRequest request, CancellationToken cancellationToken)
    {
        var version = await LoadProcessAssetVersionAsync(processAssetId, versionId, cancellationToken);
        if (version is null)
        {
            return NotFound<GovernanceMutationResponse>("Process asset version not found.", ApiErrorCodes.ProcessAssetVersionNotFound);
        }

        if (version.Status != "reviewed")
        {
            return Conflict<GovernanceMutationResponse>("Only reviewed versions can be approved.", ApiErrorCodes.GovernanceTransitionNotAllowed);
        }

        var changeSummary = TrimOrNull(request.ChangeSummary);
        if (string.IsNullOrWhiteSpace(changeSummary))
        {
            return ValidationError<GovernanceMutationResponse>("Change summary is required.", ApiErrorCodes.ChangeSummaryRequired);
        }

        version.Status = "approved";
        version.ChangeSummary = changeSummary;
        version.ApprovedBy = actor;
        version.ApprovedAt = DateTimeOffset.UtcNow;
        version.UpdatedAt = version.ApprovedAt.Value;
        await SyncProcessAssetStatusAsync(processAssetId, version.Id, version.Status, cancellationToken);

        await WriteAuditAsync("approve", "process_asset_version", version.Id.ToString(), StatusCodes.Status200OK, null, new { version.Status, version.ApprovedBy, version.ChangeSummary }, cancellationToken, changeSummary);
        await businessAuditEventWriter.AppendAsync("governance", "process_asset_version_approved", "process_asset_version", version.Id.ToString(), version.Title, changeSummary, new { processAssetId, approver = actor }, cancellationToken);
        return Success(version);
    }

    public async Task<GovernanceCommandResult<GovernanceMutationResponse>> ActivateProcessAssetVersionAsync(Guid processAssetId, Guid versionId, CancellationToken cancellationToken)
    {
        var version = await LoadProcessAssetVersionAsync(processAssetId, versionId, cancellationToken);
        if (version is null)
        {
            return NotFound<GovernanceMutationResponse>("Process asset version not found.", ApiErrorCodes.ProcessAssetVersionNotFound);
        }

        if (version.Status != "approved")
        {
            return Conflict<GovernanceMutationResponse>("Only approved versions can be activated.", ApiErrorCodes.GovernanceTransitionNotAllowed);
        }

        version.Status = "active";
        version.UpdatedAt = DateTimeOffset.UtcNow;
        await SyncProcessAssetStatusAsync(processAssetId, version.Id, version.Status, cancellationToken);

        await WriteAuditAsync("activate", "process_asset_version", version.Id.ToString(), StatusCodes.Status200OK, null, new { version.Status }, cancellationToken);
        await businessAuditEventWriter.AppendAsync("governance", "process_asset_version_activated", "process_asset_version", version.Id.ToString(), version.Title, null, new { processAssetId }, cancellationToken);
        return Success(version);
    }

    public async Task<GovernanceCommandResult<GovernanceMutationResponse>> DeprecateProcessAssetAsync(Guid processAssetId, CancellationToken cancellationToken)
    {
        var asset = await dbContext.Set<ProcessAssetEntity>().SingleOrDefaultAsync(x => x.Id == processAssetId, cancellationToken);
        if (asset is null)
        {
            return NotFound<GovernanceMutationResponse>("Process asset not found.", ApiErrorCodes.ProcessAssetNotFound);
        }

        if (asset.Status != "active")
        {
            return Conflict<GovernanceMutationResponse>("Only active assets can be deprecated.", ApiErrorCodes.GovernanceTransitionNotAllowed);
        }

        asset.Status = "deprecated";
        asset.UpdatedAt = DateTimeOffset.UtcNow;
        if (asset.CurrentVersionId.HasValue)
        {
            var version = await dbContext.Set<ProcessAssetVersionEntity>().SingleAsync(x => x.Id == asset.CurrentVersionId.Value, cancellationToken);
            version.Status = "deprecated";
            version.UpdatedAt = asset.UpdatedAt;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("deprecate", "process_asset", asset.Id.ToString(), StatusCodes.Status200OK, null, new { asset.Status }, cancellationToken);
        return new GovernanceCommandResult<GovernanceMutationResponse>(GovernanceCommandStatus.Success, new GovernanceMutationResponse(asset.Id, asset.Status, asset.UpdatedAt));
    }

    public async Task<GovernanceCommandResult<QaChecklistResponse>> CreateQaChecklistAsync(CreateQaChecklistRequest request, CancellationToken cancellationToken)
    {
        var validation = ValidateChecklistRequest(request.Code, request.Name, request.Scope, request.OwnerUserId, request.Items);
        if (validation is not null)
        {
            return validation;
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new QaChecklistEntity
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Scope = request.Scope.Trim(),
            OwnerUserId = request.OwnerUserId.Trim(),
            Status = "draft",
            ItemsJson = SerializeChecklistItems(request.Items),
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("create", "qa_checklist", entity.Id.ToString(), StatusCodes.Status201Created, null, new { entity.Code, entity.Status }, cancellationToken);

        return new GovernanceCommandResult<QaChecklistResponse>(GovernanceCommandStatus.Success, GovernanceQueries.ToQaChecklistResponse(entity));
    }

    public async Task<GovernanceCommandResult<QaChecklistResponse>> UpdateQaChecklistAsync(Guid qaChecklistId, UpdateQaChecklistRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<QaChecklistEntity>().SingleOrDefaultAsync(x => x.Id == qaChecklistId, cancellationToken);
        if (entity is null)
        {
            return NotFound<QaChecklistResponse>("QA checklist not found.", ApiErrorCodes.QaChecklistNotFound);
        }

        var validation = ValidateChecklistRequest(request.Code, request.Name, request.Scope, request.OwnerUserId, request.Items);
        if (validation is not null)
        {
            return validation;
        }

        entity.Code = request.Code.Trim();
        entity.Name = request.Name.Trim();
        entity.Scope = request.Scope.Trim();
        entity.OwnerUserId = request.OwnerUserId.Trim();
        entity.ItemsJson = SerializeChecklistItems(request.Items);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync("update", "qa_checklist", entity.Id.ToString(), StatusCodes.Status200OK, null, new { entity.Code, entity.Status }, cancellationToken);
        return new GovernanceCommandResult<QaChecklistResponse>(GovernanceCommandStatus.Success, GovernanceQueries.ToQaChecklistResponse(entity));
    }

    public async Task<GovernanceCommandResult<GovernanceMutationResponse>> ApproveQaChecklistAsync(Guid qaChecklistId, CancellationToken cancellationToken)
    {
        return await TransitionChecklistAsync(qaChecklistId, "draft", "approved", "approve", cancellationToken);
    }

    public async Task<GovernanceCommandResult<GovernanceMutationResponse>> ActivateQaChecklistAsync(Guid qaChecklistId, CancellationToken cancellationToken)
    {
        return await TransitionChecklistAsync(qaChecklistId, "approved", "active", "activate", cancellationToken);
    }

    public async Task<GovernanceCommandResult<GovernanceMutationResponse>> DeprecateQaChecklistAsync(Guid qaChecklistId, CancellationToken cancellationToken)
    {
        return await TransitionChecklistAsync(qaChecklistId, "active", "deprecated", "deprecate", cancellationToken);
    }

    public async Task<GovernanceCommandResult<ProjectPlanResponse>> CreateProjectPlanAsync(CreateProjectPlanRequest request, CancellationToken cancellationToken)
    {
        if (!await dbContext.Projects.AnyAsync(x => x.Id == request.ProjectId && x.DeletedAt == null, cancellationToken))
        {
            return NotFound<ProjectPlanResponse>("Project not found.", ApiErrorCodes.ProjectNotFound);
        }

        var validation = ValidateProjectPlanRequest(request.Name, request.ScopeSummary, request.LifecycleModel, request.OwnerUserId, request.StartDate, request.TargetEndDate);
        if (validation is not null)
        {
            return validation;
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new ProjectPlanEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Name = request.Name.Trim(),
            ScopeSummary = request.ScopeSummary.Trim(),
            LifecycleModel = request.LifecycleModel.Trim(),
            StartDate = request.StartDate,
            TargetEndDate = request.TargetEndDate,
            OwnerUserId = request.OwnerUserId.Trim(),
            Status = "draft",
            MilestonesJson = SerializeStrings(request.Milestones),
            RolesJson = SerializeStrings(request.Roles),
            RiskApproach = request.RiskApproach.Trim(),
            QualityApproach = request.QualityApproach.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("create", "project_plan", entity.Id.ToString(), StatusCodes.Status201Created, null, new { entity.ProjectId, entity.Status }, cancellationToken);
        return new GovernanceCommandResult<ProjectPlanResponse>(GovernanceCommandStatus.Success, GovernanceQueries.ToProjectPlanResponse(entity));
    }

    public async Task<GovernanceCommandResult<ProjectPlanResponse>> UpdateProjectPlanAsync(Guid projectPlanId, UpdateProjectPlanRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<ProjectPlanEntity>().SingleOrDefaultAsync(x => x.Id == projectPlanId, cancellationToken);
        if (entity is null)
        {
            return NotFound<ProjectPlanResponse>("Project plan not found.", ApiErrorCodes.ProjectPlanNotFound);
        }

        var validation = ValidateProjectPlanRequest(request.Name, request.ScopeSummary, request.LifecycleModel, request.OwnerUserId, request.StartDate, request.TargetEndDate);
        if (validation is not null)
        {
            return validation;
        }

        entity.Name = request.Name.Trim();
        entity.ScopeSummary = request.ScopeSummary.Trim();
        entity.LifecycleModel = request.LifecycleModel.Trim();
        entity.StartDate = request.StartDate;
        entity.TargetEndDate = request.TargetEndDate;
        entity.OwnerUserId = request.OwnerUserId.Trim();
        entity.MilestonesJson = SerializeStrings(request.Milestones);
        entity.RolesJson = SerializeStrings(request.Roles);
        entity.RiskApproach = request.RiskApproach.Trim();
        entity.QualityApproach = request.QualityApproach.Trim();
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync("update", "project_plan", entity.Id.ToString(), StatusCodes.Status200OK, null, new { entity.Status, entity.Name }, cancellationToken);
        return new GovernanceCommandResult<ProjectPlanResponse>(GovernanceCommandStatus.Success, GovernanceQueries.ToProjectPlanResponse(entity));
    }

    public async Task<GovernanceCommandResult<GovernanceMutationResponse>> SubmitProjectPlanReviewAsync(Guid projectPlanId, CancellationToken cancellationToken)
    {
        return await TransitionProjectPlanAsync(projectPlanId, "draft", "review", "submit_review", null, null, cancellationToken);
    }

    public async Task<GovernanceCommandResult<GovernanceMutationResponse>> ApproveProjectPlanAsync(Guid projectPlanId, string actor, ProjectPlanApprovalRequest request, CancellationToken cancellationToken)
    {
        var reason = TrimOrNull(request.Reason);
        if (string.IsNullOrWhiteSpace(reason))
        {
            return ValidationError<GovernanceMutationResponse>("Approval reason is required.", ApiErrorCodes.ApprovalReasonRequired);
        }

        return await TransitionProjectPlanAsync(projectPlanId, "review", "approved", "approve", actor, reason, cancellationToken);
    }

    public async Task<GovernanceCommandResult<GovernanceMutationResponse>> BaselineProjectPlanAsync(Guid projectPlanId, CancellationToken cancellationToken)
    {
        return await TransitionProjectPlanAsync(projectPlanId, "approved", "baseline", "baseline", null, null, cancellationToken);
    }

    public async Task<GovernanceCommandResult<GovernanceMutationResponse>> SupersedeProjectPlanAsync(Guid projectPlanId, string actor, ProjectPlanApprovalRequest request, CancellationToken cancellationToken)
    {
        var reason = TrimOrNull(request.Reason);
        if (string.IsNullOrWhiteSpace(reason))
        {
            return ValidationError<GovernanceMutationResponse>("Approval reason is required.", ApiErrorCodes.ApprovalReasonRequired);
        }

        return await TransitionProjectPlanAsync(projectPlanId, "baseline", "superseded", "supersede", actor, reason, cancellationToken);
    }

    public async Task<GovernanceCommandResult<StakeholderResponse>> CreateStakeholderAsync(CreateStakeholderRequest request, CancellationToken cancellationToken)
    {
        if (!await dbContext.Projects.AnyAsync(x => x.Id == request.ProjectId && x.DeletedAt == null, cancellationToken))
        {
            return NotFound<StakeholderResponse>("Project not found.", ApiErrorCodes.ProjectNotFound);
        }

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.RoleName))
        {
            return ValidationError<StakeholderResponse>("Stakeholder name and role are required.", ApiErrorCodes.RequestValidationFailed);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new StakeholderEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Name = request.Name.Trim(),
            RoleName = request.RoleName.Trim(),
            InfluenceLevel = request.InfluenceLevel.Trim(),
            ContactChannel = request.ContactChannel.Trim(),
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("create", "stakeholder", entity.Id.ToString(), StatusCodes.Status201Created, null, new { entity.ProjectId, entity.Name }, cancellationToken);
        return new GovernanceCommandResult<StakeholderResponse>(GovernanceCommandStatus.Success, await LoadStakeholderAsync(entity.Id, cancellationToken));
    }

    public async Task<GovernanceCommandResult<StakeholderResponse>> UpdateStakeholderAsync(Guid stakeholderId, UpdateStakeholderRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<StakeholderEntity>().SingleOrDefaultAsync(x => x.Id == stakeholderId, cancellationToken);
        if (entity is null)
        {
            return NotFound<StakeholderResponse>("Stakeholder not found.", ApiErrorCodes.StakeholderNotFound);
        }

        entity.Name = request.Name.Trim();
        entity.RoleName = request.RoleName.Trim();
        entity.InfluenceLevel = request.InfluenceLevel.Trim();
        entity.ContactChannel = request.ContactChannel.Trim();
        entity.Status = request.Status.Trim().ToLowerInvariant();
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync("update", "stakeholder", entity.Id.ToString(), StatusCodes.Status200OK, null, new { entity.Name, entity.Status }, cancellationToken);
        return new GovernanceCommandResult<StakeholderResponse>(GovernanceCommandStatus.Success, await LoadStakeholderAsync(entity.Id, cancellationToken));
    }

    public async Task<GovernanceCommandResult<GovernanceMutationResponse>> ArchiveStakeholderAsync(Guid stakeholderId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<StakeholderEntity>().SingleOrDefaultAsync(x => x.Id == stakeholderId, cancellationToken);
        if (entity is null)
        {
            return NotFound<GovernanceMutationResponse>("Stakeholder not found.", ApiErrorCodes.StakeholderNotFound);
        }

        entity.Status = "archived";
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("archive", "stakeholder", entity.Id.ToString(), StatusCodes.Status200OK, null, new { entity.Status }, cancellationToken);
        return new GovernanceCommandResult<GovernanceMutationResponse>(GovernanceCommandStatus.Success, new GovernanceMutationResponse(entity.Id, entity.Status, entity.UpdatedAt));
    }

    public async Task<GovernanceCommandResult<TailoringRecordResponse>> CreateTailoringRecordAsync(CreateTailoringRecordRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateTailoringRequestAsync(request.ProjectId, request.RequestedChange, request.Reason, request.ImpactSummary, request.ImpactedProcessAssetId, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new TailoringRecordEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            RequesterUserId = request.RequesterUserId.Trim(),
            RequestedChange = request.RequestedChange.Trim(),
            Reason = request.Reason.Trim(),
            ImpactSummary = request.ImpactSummary.Trim(),
            ImpactedProcessAssetId = request.ImpactedProcessAssetId,
            Status = "draft",
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("create", "tailoring_record", entity.Id.ToString(), StatusCodes.Status201Created, null, new { entity.ProjectId, entity.Status }, cancellationToken, entity.Reason);
        return new GovernanceCommandResult<TailoringRecordResponse>(GovernanceCommandStatus.Success, await LoadTailoringAsync(entity.Id, cancellationToken));
    }

    public async Task<GovernanceCommandResult<TailoringRecordResponse>> UpdateTailoringRecordAsync(Guid tailoringRecordId, UpdateTailoringRecordRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<TailoringRecordEntity>().SingleOrDefaultAsync(x => x.Id == tailoringRecordId, cancellationToken);
        if (entity is null)
        {
            return NotFound<TailoringRecordResponse>("Tailoring record not found.", ApiErrorCodes.TailoringRecordNotFound);
        }

        var validation = await ValidateTailoringRequestAsync(entity.ProjectId, request.RequestedChange, request.Reason, request.ImpactSummary, request.ImpactedProcessAssetId, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        entity.RequestedChange = request.RequestedChange.Trim();
        entity.Reason = request.Reason.Trim();
        entity.ImpactSummary = request.ImpactSummary.Trim();
        entity.ImpactedProcessAssetId = request.ImpactedProcessAssetId;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync("update", "tailoring_record", entity.Id.ToString(), StatusCodes.Status200OK, null, new { entity.Status }, cancellationToken, entity.Reason);
        return new GovernanceCommandResult<TailoringRecordResponse>(GovernanceCommandStatus.Success, await LoadTailoringAsync(entity.Id, cancellationToken));
    }

    public async Task<GovernanceCommandResult<GovernanceMutationResponse>> SubmitTailoringRecordAsync(Guid tailoringRecordId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<TailoringRecordEntity>().SingleOrDefaultAsync(x => x.Id == tailoringRecordId, cancellationToken);
        if (entity is null)
        {
            return NotFound<GovernanceMutationResponse>("Tailoring record not found.", ApiErrorCodes.TailoringRecordNotFound);
        }

        if (entity.Status != "draft")
        {
            return Conflict<GovernanceMutationResponse>("Only draft tailoring records can be submitted.", ApiErrorCodes.GovernanceTransitionNotAllowed);
        }

        entity.Status = "submitted";
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync("submit", "tailoring_record", entity.Id.ToString(), StatusCodes.Status200OK, null, new { entity.Status }, cancellationToken, entity.Reason);
        await businessAuditEventWriter.AppendAsync("governance", "tailoring_submitted", "tailoring_record", entity.Id.ToString(), entity.RequestedChange, entity.Reason, new { entity.ProjectId }, cancellationToken);
        return new GovernanceCommandResult<GovernanceMutationResponse>(GovernanceCommandStatus.Success, new GovernanceMutationResponse(entity.Id, entity.Status, entity.UpdatedAt));
    }

    public async Task<GovernanceCommandResult<GovernanceMutationResponse>> ApproveTailoringRecordAsync(Guid tailoringRecordId, string actor, TailoringDecisionRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<TailoringRecordEntity>().SingleOrDefaultAsync(x => x.Id == tailoringRecordId, cancellationToken);
        if (entity is null)
        {
            return NotFound<GovernanceMutationResponse>("Tailoring record not found.", ApiErrorCodes.TailoringRecordNotFound);
        }

        if (entity.Status != "submitted")
        {
            return Conflict<GovernanceMutationResponse>("Only submitted tailoring records can be approved or rejected.", ApiErrorCodes.GovernanceTransitionNotAllowed);
        }

        var decision = request.Decision.Trim().ToLowerInvariant();
        var reason = TrimOrNull(request.Reason);
        if (string.IsNullOrWhiteSpace(reason))
        {
            return ValidationError<GovernanceMutationResponse>("Reason is required.", ApiErrorCodes.TailoringReasonRequired);
        }

        if (decision is not "approved" and not "rejected")
        {
            return ValidationError<GovernanceMutationResponse>("Decision must be approved or rejected.", ApiErrorCodes.RequestValidationFailed);
        }

        entity.Status = decision;
        entity.ApproverUserId = actor;
        entity.ApprovedAt = DateTimeOffset.UtcNow;
        entity.ApprovalRationale = reason;
        entity.UpdatedAt = entity.ApprovedAt.Value;
        await dbContext.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync("approve", "tailoring_record", entity.Id.ToString(), StatusCodes.Status200OK, null, new { entity.Status, entity.ApproverUserId }, cancellationToken, reason);
        await businessAuditEventWriter.AppendAsync("governance", $"tailoring_{decision}", "tailoring_record", entity.Id.ToString(), entity.RequestedChange, reason, new { entity.ProjectId, approver = actor }, cancellationToken);
        return new GovernanceCommandResult<GovernanceMutationResponse>(GovernanceCommandStatus.Success, new GovernanceMutationResponse(entity.Id, entity.Status, entity.UpdatedAt, entity.ApproverUserId, entity.ApprovedAt));
    }

    public async Task<GovernanceCommandResult<GovernanceMutationResponse>> ApplyTailoringRecordAsync(Guid tailoringRecordId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<TailoringRecordEntity>().SingleOrDefaultAsync(x => x.Id == tailoringRecordId, cancellationToken);
        if (entity is null)
        {
            return NotFound<GovernanceMutationResponse>("Tailoring record not found.", ApiErrorCodes.TailoringRecordNotFound);
        }

        if (entity.Status != "approved")
        {
            return Conflict<GovernanceMutationResponse>("Only approved tailoring records can be applied.", ApiErrorCodes.GovernanceTransitionNotAllowed);
        }

        entity.Status = "applied";
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("apply", "tailoring_record", entity.Id.ToString(), StatusCodes.Status200OK, null, new { entity.Status }, cancellationToken);
        await businessAuditEventWriter.AppendAsync("governance", "tailoring_applied", "tailoring_record", entity.Id.ToString(), entity.RequestedChange, null, new { entity.ProjectId }, cancellationToken);
        return new GovernanceCommandResult<GovernanceMutationResponse>(GovernanceCommandStatus.Success, new GovernanceMutationResponse(entity.Id, entity.Status, entity.UpdatedAt, entity.ApproverUserId, entity.ApprovedAt));
    }

    public async Task<GovernanceCommandResult<GovernanceMutationResponse>> ArchiveTailoringRecordAsync(Guid tailoringRecordId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<TailoringRecordEntity>().SingleOrDefaultAsync(x => x.Id == tailoringRecordId, cancellationToken);
        if (entity is null)
        {
            return NotFound<GovernanceMutationResponse>("Tailoring record not found.", ApiErrorCodes.TailoringRecordNotFound);
        }

        if (entity.Status is not "applied" and not "rejected")
        {
            return Conflict<GovernanceMutationResponse>("Only applied or rejected tailoring records can be archived.", ApiErrorCodes.GovernanceTransitionNotAllowed);
        }

        entity.Status = "archived";
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("archive", "tailoring_record", entity.Id.ToString(), StatusCodes.Status200OK, null, new { entity.Status }, cancellationToken);
        return new GovernanceCommandResult<GovernanceMutationResponse>(GovernanceCommandStatus.Success, new GovernanceMutationResponse(entity.Id, entity.Status, entity.UpdatedAt, entity.ApproverUserId, entity.ApprovedAt));
    }

    private async Task<GovernanceCommandResult<GovernanceMutationResponse>> TransitionChecklistAsync(Guid qaChecklistId, string fromStatus, string toStatus, string action, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<QaChecklistEntity>().SingleOrDefaultAsync(x => x.Id == qaChecklistId, cancellationToken);
        if (entity is null)
        {
            return NotFound<GovernanceMutationResponse>("QA checklist not found.", ApiErrorCodes.QaChecklistNotFound);
        }

        if (entity.Status != fromStatus)
        {
            return Conflict<GovernanceMutationResponse>("Checklist transition is not allowed.", ApiErrorCodes.GovernanceTransitionNotAllowed);
        }

        entity.Status = toStatus;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(action, "qa_checklist", entity.Id.ToString(), StatusCodes.Status200OK, null, new { entity.Status }, cancellationToken);
        return new GovernanceCommandResult<GovernanceMutationResponse>(GovernanceCommandStatus.Success, new GovernanceMutationResponse(entity.Id, entity.Status, entity.UpdatedAt));
    }

    private async Task<GovernanceCommandResult<GovernanceMutationResponse>> TransitionProjectPlanAsync(Guid projectPlanId, string fromStatus, string toStatus, string action, string? actor, string? reason, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<ProjectPlanEntity>().SingleOrDefaultAsync(x => x.Id == projectPlanId, cancellationToken);
        if (entity is null)
        {
            return NotFound<GovernanceMutationResponse>("Project plan not found.", ApiErrorCodes.ProjectPlanNotFound);
        }

        if (entity.Status != fromStatus)
        {
            return Conflict<GovernanceMutationResponse>("Project plan transition is not allowed.", ApiErrorCodes.GovernanceTransitionNotAllowed);
        }

        entity.Status = toStatus;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        if (!string.IsNullOrWhiteSpace(actor))
        {
            entity.ApprovalReason = reason;
            entity.ApprovedBy = actor;
            entity.ApprovedAt = entity.UpdatedAt;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(action, "project_plan", entity.Id.ToString(), StatusCodes.Status200OK, null, new { entity.Status, entity.ApprovedBy }, cancellationToken, reason);
        await businessAuditEventWriter.AppendAsync("governance", $"project_plan_{action}", "project_plan", entity.Id.ToString(), entity.Name, reason, new { entity.ProjectId, approver = actor }, cancellationToken);
        return new GovernanceCommandResult<GovernanceMutationResponse>(GovernanceCommandStatus.Success, new GovernanceMutationResponse(entity.Id, entity.Status, entity.UpdatedAt, entity.ApprovedBy, entity.ApprovedAt));
    }

    private async Task SyncProcessAssetStatusAsync(Guid processAssetId, Guid versionId, string status, CancellationToken cancellationToken)
    {
        var asset = await dbContext.Set<ProcessAssetEntity>().SingleAsync(x => x.Id == processAssetId, cancellationToken);
        asset.Status = status;
        asset.CurrentVersionId = versionId;
        asset.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<ProcessAssetVersionEntity?> LoadProcessAssetVersionAsync(Guid processAssetId, Guid versionId, CancellationToken cancellationToken) =>
        await dbContext.Set<ProcessAssetVersionEntity>()
            .SingleOrDefaultAsync(x => x.Id == versionId && x.ProcessAssetId == processAssetId, cancellationToken);

    private async Task<ProcessAssetResponse> LoadProcessAssetAsync(Guid processAssetId, CancellationToken cancellationToken) =>
        await new GovernanceQueries(dbContext, auditLogWriter).GetProcessAssetAsync(processAssetId, cancellationToken)
        ?? throw new InvalidOperationException("Expected process asset to exist.");

    private async Task<StakeholderResponse> LoadStakeholderAsync(Guid stakeholderId, CancellationToken cancellationToken) =>
        await new GovernanceQueries(dbContext, auditLogWriter).GetStakeholderAsync(stakeholderId, cancellationToken)
        ?? throw new InvalidOperationException("Expected stakeholder to exist.");

    private async Task<TailoringRecordResponse> LoadTailoringAsync(Guid tailoringId, CancellationToken cancellationToken) =>
        await new GovernanceQueries(dbContext, auditLogWriter).GetTailoringRecordAsync(tailoringId, cancellationToken)
        ?? throw new InvalidOperationException("Expected tailoring record to exist.");

    private async Task<GovernanceCommandResult<TailoringRecordResponse>?> ValidateTailoringRequestAsync(Guid projectId, string requestedChange, string reason, string impactSummary, Guid? impactedProcessAssetId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Projects.AnyAsync(x => x.Id == projectId && x.DeletedAt == null, cancellationToken))
        {
            return NotFound<TailoringRecordResponse>("Project not found.", ApiErrorCodes.ProjectNotFound);
        }

        if (string.IsNullOrWhiteSpace(requestedChange) || string.IsNullOrWhiteSpace(reason) || string.IsNullOrWhiteSpace(impactSummary))
        {
            return ValidationError<TailoringRecordResponse>("Requested change, reason, and impact summary are required.", ApiErrorCodes.RequestValidationFailed);
        }

        if (impactedProcessAssetId.HasValue && !await dbContext.Set<ProcessAssetEntity>().AnyAsync(x => x.Id == impactedProcessAssetId.Value, cancellationToken))
        {
            return NotFound<TailoringRecordResponse>("Process asset not found.", ApiErrorCodes.ProcessAssetNotFound);
        }

        return null;
    }

    private static GovernanceCommandResult<ProcessAssetResponse>? ValidateProcessAssetRequest(string code, string name, string category, string ownerUserId, string versionTitle, string versionSummary)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(ownerUserId))
        {
            return ValidationError<ProcessAssetResponse>("Code, name, category, and owner are required.", ApiErrorCodes.RequestValidationFailed);
        }

        if (string.IsNullOrWhiteSpace(versionTitle) || string.IsNullOrWhiteSpace(versionSummary))
        {
            return ValidationError<ProcessAssetResponse>("Initial version title and summary are required.", ApiErrorCodes.RequestValidationFailed);
        }

        return null;
    }

    private static GovernanceCommandResult<QaChecklistResponse>? ValidateChecklistRequest(string code, string name, string scope, string ownerUserId, IReadOnlyList<QaChecklistItemRequest> items)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(scope) || string.IsNullOrWhiteSpace(ownerUserId))
        {
            return ValidationError<QaChecklistResponse>("Code, name, scope, and owner are required.", ApiErrorCodes.RequestValidationFailed);
        }

        if (items.Count == 0 || items.Any(item => string.IsNullOrWhiteSpace(item.ItemText)))
        {
            return ValidationError<QaChecklistResponse>("At least one checklist item is required.", ApiErrorCodes.RequestValidationFailed);
        }

        return null;
    }

    private static GovernanceCommandResult<ProjectPlanResponse>? ValidateProjectPlanRequest(string name, string scopeSummary, string lifecycleModel, string ownerUserId, DateOnly startDate, DateOnly targetEndDate)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(scopeSummary) || string.IsNullOrWhiteSpace(lifecycleModel) || string.IsNullOrWhiteSpace(ownerUserId))
        {
            return ValidationError<ProjectPlanResponse>("Name, scope, lifecycle model, and owner are required.", ApiErrorCodes.RequestValidationFailed);
        }

        if (targetEndDate < startDate)
        {
            return ValidationError<ProjectPlanResponse>("Target end date must be on or after start date.", ApiErrorCodes.RequestValidationFailed);
        }

        return null;
    }

    private static string SerializeChecklistItems(IReadOnlyList<QaChecklistItemRequest> items) =>
        JsonSerializer.Serialize(
            items.Select(x => new QaChecklistItemResponse(x.ItemText.Trim(), x.Mandatory, x.ApplicablePhase.Trim(), x.EvidenceRule.Trim())).ToArray(),
            SerializerOptions);

    private static string SerializeStrings(IReadOnlyList<string> items) =>
        JsonSerializer.Serialize(items.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray(), SerializerOptions);

    private async Task WriteAuditAsync(string action, string entityType, string? entityId, int statusCode, object? before, object? after, CancellationToken cancellationToken, string? reason = null)
    {
        auditLogWriter.Append(new AuditLogEntry(
            Module: "governance",
            Action: action,
            EntityType: entityType,
            EntityId: entityId,
            StatusCode: statusCode,
            Reason: reason,
            Before: before,
            After: after));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private GovernanceCommandResult<GovernanceMutationResponse> Success(ProcessAssetVersionEntity version) =>
        new(GovernanceCommandStatus.Success, new GovernanceMutationResponse(version.Id, version.Status, version.UpdatedAt, version.ApprovedBy, version.ApprovedAt));

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static GovernanceCommandResult<T> NotFound<T>(string message, string code) => new(GovernanceCommandStatus.NotFound, ErrorMessage: message, ErrorCode: code);
    private static GovernanceCommandResult<T> ValidationError<T>(string message, string code) => new(GovernanceCommandStatus.ValidationError, ErrorMessage: message, ErrorCode: code);
    private static GovernanceCommandResult<T> Conflict<T>(string message, string code) => new(GovernanceCommandStatus.Conflict, ErrorMessage: message, ErrorCode: code);
}
