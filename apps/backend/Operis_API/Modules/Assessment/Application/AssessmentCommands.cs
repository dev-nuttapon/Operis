using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Assessment.Contracts;
using Operis_API.Modules.Assessment.Infrastructure;
using Operis_API.Modules.Audits.Infrastructure;
using Operis_API.Modules.ChangeControl.Infrastructure;
using Operis_API.Modules.Documents.Infrastructure;
using Operis_API.Modules.Governance.Infrastructure;
using Operis_API.Modules.Operations.Infrastructure;
using Operis_API.Modules.Requirements.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Modules.Verification.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Assessment.Application;

public sealed class AssessmentCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IAssessmentQueries queries) : IAssessmentCommands
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly string[] PackageStates = ["draft", "prepared", "shared", "archived"];
    private static readonly string[] FindingStates = ["open", "accepted", "closed"];

    public async Task<AssessmentCommandResult<AssessmentPackageDetailResponse>> CreatePackageAsync(CreateAssessmentPackageRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (!request.ProjectId.HasValue && string.IsNullOrWhiteSpace(request.ProcessArea))
        {
            return Validation<AssessmentPackageDetailResponse>(ApiErrorCodes.AssessmentPackageScopeRequired, "Package scope requires a project or process area.");
        }

        if (string.IsNullOrWhiteSpace(request.ScopeSummary))
        {
            return Validation<AssessmentPackageDetailResponse>(ApiErrorCodes.AssessmentPackageScopeRequired, "Package scope summary is required.");
        }

        ProjectEntity? project = null;
        if (request.ProjectId.HasValue)
        {
            project = await dbContext.Projects.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.ProjectId.Value, cancellationToken);
            if (project is null)
            {
                return NotFound<AssessmentPackageDetailResponse>(ApiErrorCodes.ProjectNotFound, "Project not found.");
            }
        }

        var now = DateTimeOffset.UtcNow;
        var evidenceReferences = await BuildEvidenceReferencesAsync(request.ProjectId, NormalizeOrNull(request.ProcessArea), cancellationToken);
        var packageCode = await GeneratePackageCodeAsync(request.ProjectId, cancellationToken);

        var entity = new AssessmentPackageEntity
        {
            Id = Guid.NewGuid(),
            PackageCode = packageCode,
            ProjectId = request.ProjectId,
            ProcessArea = NormalizeOrNull(request.ProcessArea),
            ScopeSummary = request.ScopeSummary.Trim(),
            Status = "draft",
            EvidenceReferencesJson = JsonSerializer.Serialize(evidenceReferences, SerializerOptions),
            CreatedByUserId = actorUserId ?? "system",
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.AssessmentPackages.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("create", "assessment_package", entity.Id, StatusCodes.Status201Created, new { entity.PackageCode, entity.ProjectId, entity.ProcessArea, EvidenceCount = evidenceReferences.Count });
        return Success((await queries.GetPackageAsync(entity.Id, cancellationToken))!);
    }

    public async Task<AssessmentCommandResult<AssessmentPackageDetailResponse>> TransitionPackageAsync(Guid packageId, TransitionAssessmentPackageRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.AssessmentPackages.SingleOrDefaultAsync(x => x.Id == packageId, cancellationToken);
        if (entity is null)
        {
            return NotFound<AssessmentPackageDetailResponse>(ApiErrorCodes.AssessmentPackageNotFound, "Assessment package not found.");
        }

        var target = NormalizeOrNull(request.TargetStatus);
        if (target is null || !PackageStates.Contains(target) || !IsValidPackageTransition(entity.Status, target))
        {
            return Validation<AssessmentPackageDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Assessment package transition is invalid.");
        }

        if (target == "shared" && !await dbContext.AssessmentFindings.AsNoTracking().AnyAsync(x => x.PackageId == packageId, cancellationToken))
        {
            return Validation<AssessmentPackageDetailResponse>(ApiErrorCodes.AssessmentPackageSharingRequiresFinding, "A shared package must contain at least one finding.");
        }

        var now = DateTimeOffset.UtcNow;
        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Status = target,
            PreparedAt = target == "prepared" ? now : entity.PreparedAt,
            PreparedByUserId = target == "prepared" ? actorUserId : entity.PreparedByUserId,
            SharedAt = target == "shared" ? now : entity.SharedAt,
            SharedByUserId = target == "shared" ? actorUserId : entity.SharedByUserId,
            ArchivedAt = target == "archived" ? now : entity.ArchivedAt,
            ArchivedByUserId = target == "archived" ? actorUserId : entity.ArchivedByUserId,
            UpdatedAt = now
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("transition", "assessment_package", entity.Id, StatusCodes.Status200OK, new { From = entity.Status, To = target }, request.Reason);
        return Success((await queries.GetPackageAsync(packageId, cancellationToken))!);
    }

    public async Task<AssessmentCommandResult<AssessmentPackageNoteResponse>> AddPackageNoteAsync(Guid packageId, CreateAssessmentNoteRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (!await dbContext.AssessmentPackages.AnyAsync(x => x.Id == packageId, cancellationToken))
        {
            return NotFound<AssessmentPackageNoteResponse>(ApiErrorCodes.AssessmentPackageNotFound, "Assessment package not found.");
        }

        if (string.IsNullOrWhiteSpace(request.Note))
        {
            return Validation<AssessmentPackageNoteResponse>(ApiErrorCodes.RequestValidationFailed, "Assessment note is required.");
        }

        var now = DateTimeOffset.UtcNow;
        var note = new AssessmentNoteEntity
        {
            Id = Guid.NewGuid(),
            PackageId = packageId,
            NoteType = string.IsNullOrWhiteSpace(request.NoteType) ? "assessor_note" : request.NoteType.Trim().ToLowerInvariant(),
            Note = request.Note.Trim(),
            CreatedByUserId = actorUserId ?? "system",
            CreatedAt = now
        };

        dbContext.AssessmentNotes.Add(note);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("add_note", "assessment_package", packageId, StatusCodes.Status201Created, new { note.NoteType });
        return Success(new AssessmentPackageNoteResponse(note.Id, note.NoteType, note.Note, note.CreatedByUserId, note.CreatedAt));
    }

    public async Task<AssessmentCommandResult<AssessmentFindingDetailResponse>> CreateFindingAsync(CreateAssessmentFindingRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Validation<AssessmentFindingDetailResponse>(ApiErrorCodes.AssessmentFindingTitleRequired, "Assessment finding title is required.");
        }

        var package = await dbContext.AssessmentPackages.SingleOrDefaultAsync(x => x.Id == request.PackageId, cancellationToken);
        if (package is null)
        {
            return NotFound<AssessmentFindingDetailResponse>(ApiErrorCodes.AssessmentPackageNotFound, "Assessment package not found.");
        }

        var evidenceRefs = AssessmentQueries.DeserializeEvidenceRefs(package.EvidenceReferencesJson);
        var evidenceRef = evidenceRefs.FirstOrDefault(x =>
            string.Equals(x.EntityType, request.EvidenceEntityType.Trim(), StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.EntityId, request.EvidenceEntityId.Trim(), StringComparison.OrdinalIgnoreCase));
        if (evidenceRef is null)
        {
            return Validation<AssessmentFindingDetailResponse>(ApiErrorCodes.AssessmentFindingEvidenceReferenceRequired, "Assessment finding must reference evidence from the package.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new AssessmentFindingEntity
        {
            Id = Guid.NewGuid(),
            PackageId = request.PackageId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = string.IsNullOrWhiteSpace(request.Severity) ? "medium" : request.Severity.Trim().ToLowerInvariant(),
            Status = "open",
            EvidenceEntityType = request.EvidenceEntityType.Trim(),
            EvidenceEntityId = request.EvidenceEntityId.Trim(),
            EvidenceRoute = string.IsNullOrWhiteSpace(request.EvidenceRoute) ? evidenceRef.Route : request.EvidenceRoute.Trim(),
            OwnerUserId = NormalizeOrNull(request.OwnerUserId),
            CreatedByUserId = actorUserId ?? "system",
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.AssessmentFindings.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("create", "assessment_finding", entity.Id, StatusCodes.Status201Created, new { entity.PackageId, entity.EvidenceEntityType, entity.EvidenceEntityId });
        return Success((await queries.GetFindingAsync(entity.Id, cancellationToken))!);
    }

    public async Task<AssessmentCommandResult<AssessmentFindingDetailResponse>> TransitionFindingAsync(Guid findingId, TransitionAssessmentFindingRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.AssessmentFindings.SingleOrDefaultAsync(x => x.Id == findingId, cancellationToken);
        if (entity is null)
        {
            return NotFound<AssessmentFindingDetailResponse>(ApiErrorCodes.AssessmentFindingNotFound, "Assessment finding not found.");
        }

        var target = NormalizeOrNull(request.TargetStatus);
        if (target is null || !FindingStates.Contains(target) || !IsValidFindingTransition(entity.Status, target))
        {
            return Validation<AssessmentFindingDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Assessment finding transition is invalid.");
        }

        if ((target == "accepted" || target == "closed") && string.IsNullOrWhiteSpace(request.Summary))
        {
            return Validation<AssessmentFindingDetailResponse>(ApiErrorCodes.ReasonRequired, "Transition summary is required.");
        }

        var now = DateTimeOffset.UtcNow;
        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Status = target,
            AcceptanceSummary = target == "accepted" ? request.Summary?.Trim() : entity.AcceptanceSummary,
            ClosureSummary = target == "closed" ? request.Summary?.Trim() : entity.ClosureSummary,
            AcceptedAt = target == "accepted" ? now : entity.AcceptedAt,
            AcceptedByUserId = target == "accepted" ? actorUserId : entity.AcceptedByUserId,
            ClosedAt = target == "closed" ? now : entity.ClosedAt,
            ClosedByUserId = target == "closed" ? actorUserId : entity.ClosedByUserId,
            UpdatedAt = now
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("transition", "assessment_finding", entity.Id, StatusCodes.Status200OK, new { From = entity.Status, To = target }, request.Summary);
        return Success((await queries.GetFindingAsync(entity.Id, cancellationToken))!);
    }

    public async Task<AssessmentCommandResult<ControlCatalogItemResponse>> CreateControlCatalogItemAsync(CreateControlCatalogItemRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var validation = await ValidateControlAsync(request.ControlCode, request.Title, request.ControlSet, request.ProjectId, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var code = request.ControlCode.Trim().ToUpperInvariant();
        if (await dbContext.ControlCatalog.AnyAsync(x => x.ControlCode == code, cancellationToken))
        {
            return Conflict<ControlCatalogItemResponse>(ApiErrorCodes.ControlCodeRequired, "Control code already exists.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new ControlCatalogEntity
        {
            Id = Guid.NewGuid(),
            ControlCode = code,
            Title = request.Title.Trim(),
            ControlSet = request.ControlSet.Trim().ToLowerInvariant(),
            ProcessArea = NormalizeOrNull(request.ProcessArea),
            Status = "draft",
            Description = TrimOrNull(request.Description, 4000),
            ProjectId = request.ProjectId,
            CreatedByUserId = actorUserId ?? "system",
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.ControlCatalog.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("create", "control_catalog", entity.Id, StatusCodes.Status201Created, new { entity.ControlCode, entity.ControlSet, entity.ProcessArea });
        return Success((await queries.GetControlCatalogItemAsync(entity.Id, cancellationToken))!);
    }

    public async Task<AssessmentCommandResult<ControlCatalogItemResponse>> UpdateControlCatalogItemAsync(Guid controlId, UpdateControlCatalogItemRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ControlCatalog.SingleOrDefaultAsync(x => x.Id == controlId, cancellationToken);
        if (entity is null)
        {
            return NotFound<ControlCatalogItemResponse>(ApiErrorCodes.ResourceNotFound, "Control catalog item not found.");
        }

        var validation = await ValidateControlAsync(request.ControlCode, request.Title, request.ControlSet, request.ProjectId, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var code = request.ControlCode.Trim().ToUpperInvariant();
        if (await dbContext.ControlCatalog.AnyAsync(x => x.Id != controlId && x.ControlCode == code, cancellationToken))
        {
            return Conflict<ControlCatalogItemResponse>(ApiErrorCodes.ControlCodeRequired, "Control code already exists.");
        }

        var status = string.IsNullOrWhiteSpace(request.Status) ? entity.Status : request.Status.Trim().ToLowerInvariant();
        var now = DateTimeOffset.UtcNow;
        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            ControlCode = code,
            Title = request.Title.Trim(),
            ControlSet = request.ControlSet.Trim().ToLowerInvariant(),
            ProcessArea = NormalizeOrNull(request.ProcessArea),
            Status = status,
            Description = TrimOrNull(request.Description, 4000),
            ProjectId = request.ProjectId,
            UpdatedAt = now
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("update", "control_catalog", entity.Id, StatusCodes.Status200OK, new { code, status });
        return Success((await queries.GetControlCatalogItemAsync(controlId, cancellationToken))!);
    }

    public async Task<AssessmentCommandResult<ControlMappingDetailResponse>> CreateControlMappingAsync(CreateControlMappingRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TargetModule) || string.IsNullOrWhiteSpace(request.TargetEntityType) || string.IsNullOrWhiteSpace(request.TargetEntityId) || string.IsNullOrWhiteSpace(request.TargetRoute))
        {
            return Validation<ControlMappingDetailResponse>(ApiErrorCodes.ControlMappingTargetRequired, "Control mapping target is required.");
        }

        var control = await dbContext.ControlCatalog.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.ControlId, cancellationToken);
        if (control is null)
        {
            return NotFound<ControlMappingDetailResponse>(ApiErrorCodes.ResourceNotFound, "Control catalog item not found.");
        }

        if (request.ProjectId.HasValue && !await dbContext.Projects.AnyAsync(x => x.Id == request.ProjectId.Value, cancellationToken))
        {
            return NotFound<ControlMappingDetailResponse>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new ControlMappingEntity
        {
            Id = Guid.NewGuid(),
            ControlId = request.ControlId,
            ProjectId = request.ProjectId,
            TargetModule = request.TargetModule.Trim().ToLowerInvariant(),
            TargetEntityType = request.TargetEntityType.Trim().ToLowerInvariant(),
            TargetEntityId = request.TargetEntityId.Trim(),
            TargetRoute = request.TargetRoute.Trim(),
            EvidenceStatus = string.IsNullOrWhiteSpace(request.EvidenceStatus) ? "referenced" : request.EvidenceStatus.Trim().ToLowerInvariant(),
            Status = "draft",
            Notes = TrimOrNull(request.Notes, 2000),
            CreatedByUserId = actorUserId ?? "system",
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.ControlMappings.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("create", "control_mapping", entity.Id, StatusCodes.Status201Created, new { entity.ControlId, entity.TargetModule, entity.TargetEntityType, entity.TargetEntityId });
        return Success(await BuildControlMappingDetailAsync(entity.Id, cancellationToken));
    }

    public async Task<AssessmentCommandResult<ControlMappingDetailResponse>> TransitionControlMappingAsync(Guid mappingId, TransitionControlMappingRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ControlMappings.SingleOrDefaultAsync(x => x.Id == mappingId, cancellationToken);
        if (entity is null)
        {
            return NotFound<ControlMappingDetailResponse>(ApiErrorCodes.ResourceNotFound, "Control mapping not found.");
        }

        var target = NormalizeOrNull(request.TargetStatus);
        if (target is null || target is not ("draft" or "active" or "retired"))
        {
            return Validation<ControlMappingDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Control mapping transition is invalid.");
        }

        if (!IsValidMappingTransition(entity.Status, target))
        {
            return Validation<ControlMappingDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Control mapping transition is invalid.");
        }

        var now = DateTimeOffset.UtcNow;
        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Status = target,
            ActivatedAt = target == "active" ? now : entity.ActivatedAt,
            RetiredAt = target == "retired" ? now : entity.RetiredAt,
            Notes = string.IsNullOrWhiteSpace(request.Reason) ? entity.Notes : request.Reason.Trim(),
            UpdatedAt = now
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("transition", "control_mapping", entity.Id, StatusCodes.Status200OK, new { From = entity.Status, To = target }, request.Reason);
        return Success(await BuildControlMappingDetailAsync(entity.Id, cancellationToken));
    }

    private async Task<IReadOnlyList<AssessmentEvidenceReferenceResponse>> BuildEvidenceReferencesAsync(Guid? projectId, string? processArea, CancellationToken cancellationToken)
    {
        var references = new List<AssessmentEvidenceReferenceResponse>();

        static bool Include(string? requested, params string[] areas) =>
            requested is null || areas.Any(area => string.Equals(area, requested, StringComparison.OrdinalIgnoreCase));

        if (Include(processArea, "document-governance"))
        {
            var documents = dbContext.Documents.AsNoTracking().AsQueryable();
            if (projectId.HasValue)
            {
                documents = documents.Where(x => x.ProjectId == projectId.Value);
            }

            references.AddRange(await documents
                .OrderByDescending(x => x.UpdatedAt)
                .Take(25)
                .Select(x => new AssessmentEvidenceReferenceResponse(
                    "documents",
                    "document",
                    x.Id.ToString(),
                    x.Title,
                    x.Status,
                    "document-governance",
                    $"/app/documents/{x.Id}/versions",
                    x.UpdatedAt,
                    x.PhaseCode))
                .ToListAsync(cancellationToken));
        }

        if (Include(processArea, "requirements-traceability"))
        {
            var requirements = dbContext.Requirements.AsNoTracking().AsQueryable();
            if (projectId.HasValue)
            {
                requirements = requirements.Where(x => x.ProjectId == projectId.Value);
            }

            references.AddRange(await requirements
                .OrderByDescending(x => x.UpdatedAt)
                .Take(25)
                .Select(x => new AssessmentEvidenceReferenceResponse(
                    "requirements",
                    "requirement",
                    x.Id.ToString(),
                    x.Title,
                    x.Status,
                    "requirements-traceability",
                    $"/app/requirements/{x.Id}",
                    x.UpdatedAt,
                    x.Code))
                .ToListAsync(cancellationToken));
        }

        if (Include(processArea, "change-configuration"))
        {
            var changeRequests = dbContext.ChangeRequests.AsNoTracking().AsQueryable();
            if (projectId.HasValue)
            {
                changeRequests = changeRequests.Where(x => x.ProjectId == projectId.Value);
            }

            references.AddRange(await changeRequests
                .OrderByDescending(x => x.UpdatedAt)
                .Take(25)
                .Select(x => new AssessmentEvidenceReferenceResponse(
                    "change_control",
                    "change_request",
                    x.Id.ToString(),
                    x.Title,
                    x.Status,
                    "change-configuration",
                    $"/app/change-control/change-requests/{x.Id}",
                    x.UpdatedAt,
                    x.Code))
                .ToListAsync(cancellationToken));
        }

        if (Include(processArea, "verification-release"))
        {
            var testPlans = dbContext.TestPlans.AsNoTracking().AsQueryable();
            if (projectId.HasValue)
            {
                testPlans = testPlans.Where(x => x.ProjectId == projectId.Value);
            }

            references.AddRange(await testPlans
                .OrderByDescending(x => x.UpdatedAt)
                .Take(25)
                .Select(x => new AssessmentEvidenceReferenceResponse(
                    "verification",
                    "test_plan",
                    x.Id.ToString(),
                    x.Title,
                    x.Status,
                    "verification-release",
                    "/app/verification/test-plans",
                    x.UpdatedAt,
                    x.Code))
                .ToListAsync(cancellationToken));
        }

        if (Include(processArea, "audit-capa"))
        {
            var auditPlans = dbContext.AuditPlans.AsNoTracking().AsQueryable();
            if (projectId.HasValue)
            {
                auditPlans = auditPlans.Where(x => x.ProjectId == projectId.Value);
            }

            references.AddRange(await auditPlans
                .OrderByDescending(x => x.UpdatedAt)
                .Take(20)
                .Select(x => new AssessmentEvidenceReferenceResponse(
                    "audits",
                    "audit_plan",
                    x.Id.ToString(),
                    x.Title,
                    x.Status,
                    "audit-capa",
                    "/app/audits/plans",
                    x.UpdatedAt,
                    x.Scope))
                .ToListAsync(cancellationToken));

            references.AddRange(await dbContext.CapaRecords.AsNoTracking()
                .OrderByDescending(x => x.UpdatedAt)
                .Take(20)
                .Select(x => new AssessmentEvidenceReferenceResponse(
                    "operations",
                    "capa",
                    x.Id.ToString(),
                    x.Title,
                    x.Status,
                    "audit-capa",
                    "/app/operations/capa",
                    x.UpdatedAt,
                    x.SourceRef))
                .ToListAsync(cancellationToken));
        }

        if (Include(processArea, "security-resilience"))
        {
            references.AddRange(await dbContext.SecurityReviews.AsNoTracking()
                .OrderByDescending(x => x.UpdatedAt)
                .Take(20)
                .Select(x => new AssessmentEvidenceReferenceResponse(
                    "operations",
                    "security_review",
                    x.Id.ToString(),
                    x.ScopeRef,
                    x.Status,
                    "security-resilience",
                    "/app/operations/security-reviews",
                    x.UpdatedAt,
                    x.ControlsReviewed))
                .ToListAsync(cancellationToken));
        }

        if (Include(processArea, "process-assets-planning"))
        {
            var assets = dbContext.ProcessAssets.AsNoTracking().AsQueryable();
            references.AddRange(await assets
                .OrderByDescending(x => x.UpdatedAt)
                .Take(20)
                .Select(x => new AssessmentEvidenceReferenceResponse(
                    "governance",
                    "process_asset",
                    x.Id.ToString(),
                    x.Name,
                    x.Status,
                    "process-assets-planning",
                    "/app/process-library",
                    x.UpdatedAt,
                    x.Code))
                .ToListAsync(cancellationToken));

            var reviews = dbContext.ManagementReviews.AsNoTracking().AsQueryable();
            if (projectId.HasValue)
            {
                reviews = reviews.Where(x => x.ProjectId == projectId.Value);
            }

            references.AddRange(await reviews
                .OrderByDescending(x => x.UpdatedAt)
                .Take(20)
                .Select(x => new AssessmentEvidenceReferenceResponse(
                    "governance",
                    "management_review",
                    x.Id.ToString(),
                    x.Title,
                    x.Status,
                    "process-assets-planning",
                    "/app/governance/management-reviews",
                    x.UpdatedAt,
                    x.ReviewCode))
                .ToListAsync(cancellationToken));
        }

        return references
            .DistinctBy(x => $"{x.EntityType}:{x.EntityId}")
            .OrderBy(x => x.ProcessArea)
            .ThenByDescending(x => x.CapturedAt)
            .ToList();
    }

    private async Task<string> GeneratePackageCodeAsync(Guid? projectId, CancellationToken cancellationToken)
    {
        var prefix = projectId.HasValue
            ? await dbContext.Projects.AsNoTracking().Where(x => x.Id == projectId.Value).Select(x => x.Code).SingleAsync(cancellationToken)
            : "ORG";
        var today = DateTime.UtcNow;
        var count = await dbContext.AssessmentPackages.CountAsync(cancellationToken) + 1;
        return $"APK-{prefix}-{today:yyyyMMdd}-{count:000}";
    }

    private void AppendAudit(string action, string entityType, Guid entityId, int statusCode, object? metadata, string? reason = null) =>
        auditLogWriter.Append(new AuditLogEntry("assessment", action, entityType, entityId.ToString(), StatusCode: statusCode, Reason: reason, Metadata: metadata, Audience: LogAudience.AuditOnly));

    private async Task<AssessmentCommandResult<ControlCatalogItemResponse>?> ValidateControlAsync(string controlCode, string title, string controlSet, Guid? projectId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(controlCode))
        {
            return Validation<ControlCatalogItemResponse>(ApiErrorCodes.ControlCodeRequired, "Control code is required.");
        }

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(controlSet))
        {
            return Validation<ControlCatalogItemResponse>(ApiErrorCodes.RequestValidationFailed, "Control title and control set are required.");
        }

        if (projectId.HasValue && !await dbContext.Projects.AnyAsync(x => x.Id == projectId.Value, cancellationToken))
        {
            return NotFound<ControlCatalogItemResponse>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        return null;
    }

    private async Task<ControlMappingDetailResponse> BuildControlMappingDetailAsync(Guid mappingId, CancellationToken cancellationToken)
    {
        var row = await (
            from mapping in dbContext.ControlMappings.AsNoTracking()
            where mapping.Id == mappingId
            join control in dbContext.ControlCatalog.AsNoTracking() on mapping.ControlId equals control.Id
            join project in dbContext.Projects.AsNoTracking() on mapping.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            select new { Mapping = mapping, Control = control, ProjectName = project == null ? null : project.Name })
            .SingleAsync(cancellationToken);

        return new ControlMappingDetailResponse(
            row.Mapping.Id,
            row.Mapping.ControlId,
            row.Control.ControlCode,
            row.Control.Title,
            row.Mapping.ProjectId,
            row.ProjectName,
            row.Mapping.TargetModule,
            row.Mapping.TargetEntityType,
            row.Mapping.TargetEntityId,
            row.Mapping.TargetRoute,
            row.Mapping.EvidenceStatus,
            row.Mapping.Status,
            row.Mapping.Notes,
            row.Mapping.CreatedAt,
            row.Mapping.UpdatedAt);
    }

    private static bool IsValidPackageTransition(string current, string next)
    {
        current = current.Trim().ToLowerInvariant();
        next = next.Trim().ToLowerInvariant();
        return current switch
        {
            "draft" => next == "prepared",
            "prepared" => next is "shared" or "archived",
            "shared" => next == "archived",
            _ => false
        };
    }

    private static bool IsValidFindingTransition(string current, string next)
    {
        current = current.Trim().ToLowerInvariant();
        next = next.Trim().ToLowerInvariant();
        return current switch
        {
            "open" => next == "accepted",
            "accepted" => next == "closed",
            _ => false
        };
    }

    private static bool IsValidMappingTransition(string current, string next)
    {
        current = current.Trim().ToLowerInvariant();
        next = next.Trim().ToLowerInvariant();
        return current switch
        {
            "draft" => next == "active",
            "active" => next == "retired",
            _ => false
        };
    }

    private static string? NormalizeOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static string? TrimOrNull(string? value, int maxLength = 2000) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().Length <= maxLength ? value.Trim() : value.Trim()[..maxLength];

    private static AssessmentCommandResult<T> Success<T>(T value) => new(AssessmentCommandStatus.Success, value);
    private static AssessmentCommandResult<T> Validation<T>(string code, string message) => new(AssessmentCommandStatus.ValidationError, default, message, code);
    private static AssessmentCommandResult<T> NotFound<T>(string code, string message) => new(AssessmentCommandStatus.NotFound, default, message, code);
    private static AssessmentCommandResult<T> Conflict<T>(string code, string message) => new(AssessmentCommandStatus.Conflict, default, message, code);
}
