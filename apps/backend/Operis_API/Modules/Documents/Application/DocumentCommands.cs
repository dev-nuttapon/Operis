using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Audits.Application;
using Operis_API.Modules.Documents.Contracts;
using Operis_API.Modules.Documents.Infrastructure;
using Operis_API.Modules.Workflows;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Documents.Application;

public sealed class DocumentCommands(
    OperisDbContext dbContext,
    IDocumentObjectStorage documentObjectStorage,
    IAuditLogWriter auditLogWriter,
    IBusinessAuditEventWriter businessAuditEventWriter,
    DocumentHistoryWriter historyWriter,
    IWorkflowInstanceCommands workflowInstanceCommands,
    IOptions<DocumentStorageOptions> optionsAccessor) : IDocumentCommands
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".doc",
        ".docx",
        ".xls",
        ".xlsx",
        ".ppt",
        ".pptx"
    };

    private static readonly HashSet<string> ReviewableDocumentStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "draft",
        "rejected"
    };

    private static readonly HashSet<string> ReviewableVersionStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "uploaded",
        "rejected"
    };

    public async Task<DocumentUploadResult> CreateDocumentAsync(DocumentCreateCommand request, CancellationToken cancellationToken)
    {
        if (!await ValidateDocumentTypeAsync(request.DocumentTypeId, cancellationToken))
        {
            return DocumentUploadResult.Fail(ApiErrorCodes.Documents.DocumentTypeNotFound, "Document type not found.");
        }

        if (!await ValidateProjectAsync(request.ProjectId, cancellationToken))
        {
            return DocumentUploadResult.Fail(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return DocumentUploadResult.Fail(ApiErrorCodes.Documents.NameRequired, "A document title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.PhaseCode) || string.IsNullOrWhiteSpace(request.OwnerUserId))
        {
            return DocumentUploadResult.Fail(ApiErrorCodes.RequestValidationFailed, "Phase code and owner user are required.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new DocumentEntity
        {
            Id = Guid.NewGuid(),
            DocumentTypeId = request.DocumentTypeId,
            ProjectId = request.ProjectId,
            PhaseCode = request.PhaseCode.Trim(),
            OwnerUserId = request.OwnerUserId.Trim(),
            Classification = NormalizeValue(request.Classification, "internal"),
            RetentionClass = NormalizeValue(request.RetentionClass, "standard"),
            Title = request.Title.Trim(),
            TagsJson = SerializeTags(request.Tags),
            Status = "draft",
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };

        dbContext.Documents.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("create", "document", entity.Id, StatusCodes.Status201Created, new { entity.Title, entity.Status }, cancellationToken);
        await TryAppendHistoryAsync(entity.Id, "create", null, new { entity.Title, entity.Status }, "Created document", null, null, cancellationToken);
        await TryAppendBusinessEventAsync("documents", "document.created", "document", entity.Id.ToString(), "Created document", null, new { entity.Title }, cancellationToken);

        var response = await BuildListItemAsync(entity.Id, cancellationToken)
            ?? new DocumentListItem(entity.Id, entity.Title, entity.DocumentTypeId, null, null, entity.ProjectId, null, entity.PhaseCode, entity.OwnerUserId, entity.Status, entity.Classification, entity.RetentionClass, null, null, null, null, null, entity.UpdatedAt);

        return DocumentUploadResult.Success(response);
    }

    public async Task<DocumentVersionCreateResult> CreateDocumentVersionAsync(DocumentVersionCreateCommand request, Stream content, CancellationToken cancellationToken)
    {
        var options = optionsAccessor.Value;
        if (request.DocumentId == Guid.Empty)
        {
            return DocumentVersionCreateResult.Fail(ApiErrorCodes.Documents.DocumentNotFound, "Document not found.");
        }

        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            return DocumentVersionCreateResult.Fail(ApiErrorCodes.Documents.FileRequired, "A file is required.");
        }

        if (request.Size <= 0)
        {
            return DocumentVersionCreateResult.Fail(ApiErrorCodes.Documents.FileEmpty, "The uploaded file is empty.");
        }

        if (request.Size > options.MaxFileSizeBytes)
        {
            return DocumentVersionCreateResult.Fail(ApiErrorCodes.Documents.FileTooLarge, "The uploaded file exceeds the configured size limit.");
        }

        var normalizedFileName = Path.GetFileName(request.FileName.Trim());
        var fileExtension = Path.GetExtension(normalizedFileName);
        if (string.IsNullOrWhiteSpace(fileExtension) || !AllowedExtensions.Contains(fileExtension))
        {
            return DocumentVersionCreateResult.Fail(ApiErrorCodes.Documents.FileTypeNotAllowed, "Only governed office document formats are allowed.");
        }

        var document = await dbContext.Documents.SingleOrDefaultAsync(x => x.Id == request.DocumentId && !x.IsDeleted, cancellationToken);
        if (document is null)
        {
            return DocumentVersionCreateResult.Fail(ApiErrorCodes.Documents.DocumentNotFound, "Document not found.");
        }

        var now = DateTimeOffset.UtcNow;
        var nextVersionNumber = (await dbContext.DocumentVersions
            .Where(x => x.DocumentId == request.DocumentId && !x.IsDeleted)
            .Select(x => (int?)x.VersionNumber)
            .MaxAsync(cancellationToken) ?? 0) + 1;

        var objectKey = $"documents/{request.DocumentId:N}/{now:yyyyMMddHHmmss}-{Guid.NewGuid():N}-{normalizedFileName}";
        var contentType = string.IsNullOrWhiteSpace(request.ContentType) ? "application/octet-stream" : request.ContentType.Trim();

        await documentObjectStorage.StoreAsync(objectKey, content, request.Size, contentType, cancellationToken);

        var previousCurrentVersionId = document.CurrentVersionId;
        var versionEntity = new DocumentVersionEntity
        {
            Id = Guid.NewGuid(),
            DocumentId = request.DocumentId,
            VersionNumber = nextVersionNumber,
            StorageKey = objectKey,
            FileName = normalizedFileName,
            FileSize = request.Size,
            MimeType = contentType,
            UploadedBy = request.UploadedByUserId ?? "unknown",
            UploadedAt = now,
            Status = "uploaded",
            IsDeleted = false
        };

        dbContext.DocumentVersions.Add(versionEntity);

        if (previousCurrentVersionId.HasValue)
        {
            var previousVersion = await dbContext.DocumentVersions.SingleOrDefaultAsync(x => x.Id == previousCurrentVersionId.Value && !x.IsDeleted, cancellationToken);
            if (previousVersion is not null && !string.Equals(previousVersion.Status, "superseded", StringComparison.OrdinalIgnoreCase))
            {
                dbContext.Entry(previousVersion).CurrentValues.SetValues(previousVersion with { Status = "superseded" });
            }
        }

        dbContext.Entry(document).CurrentValues.SetValues(document with
        {
            CurrentVersionId = versionEntity.Id,
            Status = "draft",
            UpdatedAt = now
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync(
            "create_version",
            "document_version",
            versionEntity.Id,
            StatusCodes.Status201Created,
            new { versionEntity.DocumentId, versionEntity.VersionNumber, versionEntity.FileName, versionEntity.Status },
            cancellationToken);
        await TryAppendHistoryAsync(
            document.Id,
            "create_version",
            null,
            new { versionEntity.VersionNumber, versionEntity.FileName, versionEntity.Status },
            "Added document version",
            null,
            null,
            cancellationToken);
        await TryAppendBusinessEventAsync(
            "documents",
            "document.version.created",
            "document",
            document.Id.ToString(),
            "Added document version",
            null,
            new { versionEntity.VersionNumber, versionEntity.FileName },
            cancellationToken);

        return DocumentVersionCreateResult.Success(new DocumentVersionListItem(
            versionEntity.Id,
            versionEntity.DocumentId,
            versionEntity.VersionNumber,
            versionEntity.FileName,
            versionEntity.MimeType,
            versionEntity.FileSize,
            versionEntity.UploadedBy,
            versionEntity.UploadedAt,
            versionEntity.Status));
    }

    public async Task<DocumentVersionDeleteResult> DeleteDocumentVersionAsync(DocumentVersionDeleteCommand request, CancellationToken cancellationToken)
    {
        if (request.DocumentId == Guid.Empty || request.VersionId == Guid.Empty)
        {
            return DocumentVersionDeleteResult.Fail(ApiErrorCodes.Documents.DocumentVersionNotFound, "Document version not found.");
        }

        var version = await dbContext.DocumentVersions.SingleOrDefaultAsync(
            x => x.Id == request.VersionId && x.DocumentId == request.DocumentId && !x.IsDeleted,
            cancellationToken);

        if (version is null)
        {
            return DocumentVersionDeleteResult.Fail(ApiErrorCodes.Documents.DocumentVersionNotFound, "Document version not found.");
        }

        var document = await dbContext.Documents.SingleOrDefaultAsync(x => x.Id == request.DocumentId && !x.IsDeleted, cancellationToken);
        if (document is null)
        {
            return DocumentVersionDeleteResult.Fail(ApiErrorCodes.Documents.DocumentNotFound, "Document not found.");
        }

        var deletedAt = DateTimeOffset.UtcNow;
        dbContext.Entry(version).CurrentValues.SetValues(version with
        {
            IsDeleted = true,
            DeletedAt = deletedAt,
            DeletedByUserId = request.DeletedByUserId
        });

        if (document.CurrentVersionId == version.Id)
        {
            var fallbackVersionId = await dbContext.DocumentVersions
                .Where(x => x.DocumentId == request.DocumentId && !x.IsDeleted && x.Id != version.Id)
                .OrderByDescending(x => x.VersionNumber)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            dbContext.Entry(document).CurrentValues.SetValues(document with
            {
                CurrentVersionId = fallbackVersionId,
                UpdatedAt = deletedAt
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("delete_version", "document_version", version.Id, StatusCodes.Status200OK, new { version.DocumentId, version.VersionNumber }, cancellationToken);
        await TryAppendHistoryAsync(document.Id, "delete_version", new { version.VersionNumber, version.FileName }, null, "Deleted document version", null, null, cancellationToken);

        return DocumentVersionDeleteResult.Success();
    }

    public async Task<DocumentUpdateResult> UpdateDocumentAsync(DocumentUpdateCommand request, CancellationToken cancellationToken)
    {
        if (request.DocumentId == Guid.Empty)
        {
            return DocumentUpdateResult.Fail(ApiErrorCodes.Documents.DocumentNotFound, "Document not found.");
        }

        if (!await ValidateDocumentTypeAsync(request.DocumentTypeId, cancellationToken))
        {
            return DocumentUpdateResult.Fail(ApiErrorCodes.Documents.DocumentTypeNotFound, "Document type not found.");
        }

        if (!await ValidateProjectAsync(request.ProjectId, cancellationToken))
        {
            return DocumentUpdateResult.Fail(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return DocumentUpdateResult.Fail(ApiErrorCodes.Documents.NameRequired, "A document title is required.");
        }

        var entity = await dbContext.Documents.SingleOrDefaultAsync(x => x.Id == request.DocumentId && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return DocumentUpdateResult.Fail(ApiErrorCodes.Documents.DocumentNotFound, "Document not found.");
        }

        var updated = entity with
        {
            DocumentTypeId = request.DocumentTypeId,
            ProjectId = request.ProjectId,
            PhaseCode = request.PhaseCode.Trim(),
            OwnerUserId = request.OwnerUserId.Trim(),
            Classification = NormalizeValue(request.Classification, entity.Classification),
            RetentionClass = NormalizeValue(request.RetentionClass, entity.RetentionClass),
            Title = request.Title.Trim(),
            TagsJson = SerializeTags(request.Tags),
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Entry(entity).CurrentValues.SetValues(updated);
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("update", "document", updated.Id, StatusCodes.Status200OK, new { updated.Title, updated.Status }, cancellationToken);
        await TryAppendHistoryAsync(updated.Id, "update", new { entity.Title, entity.Status }, new { updated.Title, updated.Status }, "Updated document metadata", null, null, cancellationToken);

        var response = await BuildListItemAsync(updated.Id, cancellationToken)
            ?? new DocumentListItem(updated.Id, updated.Title, updated.DocumentTypeId, null, null, updated.ProjectId, null, updated.PhaseCode, updated.OwnerUserId, updated.Status, updated.Classification, updated.RetentionClass, null, null, null, null, null, updated.UpdatedAt);
        return DocumentUpdateResult.Success(response);
    }

    public async Task<DocumentDeleteResult> DeleteDocumentAsync(DocumentDeleteCommand request, CancellationToken cancellationToken)
    {
        if (request.DocumentId == Guid.Empty)
        {
            return DocumentDeleteResult.Fail(ApiErrorCodes.Documents.DocumentNotFound, "Document not found.");
        }

        if (string.IsNullOrWhiteSpace(request.DeletedReason))
        {
            return DocumentDeleteResult.Fail(ApiErrorCodes.Documents.DeleteReasonRequired, "A delete reason is required.");
        }

        var document = await dbContext.Documents.SingleOrDefaultAsync(x => x.Id == request.DocumentId && !x.IsDeleted, cancellationToken);
        if (document is null)
        {
            return DocumentDeleteResult.Fail(ApiErrorCodes.Documents.DocumentNotFound, "Document not found.");
        }

        if (!string.Equals(document.Status, "draft", StringComparison.OrdinalIgnoreCase))
        {
            return DocumentDeleteResult.Fail(ApiErrorCodes.Documents.WorkflowTransitionNotAllowed, "Only draft documents can be deleted.");
        }

        var deletedAt = DateTimeOffset.UtcNow;
        var normalizedReason = request.DeletedReason.Trim();
        var versions = await dbContext.DocumentVersions.Where(x => x.DocumentId == request.DocumentId && !x.IsDeleted).ToListAsync(cancellationToken);

        foreach (var version in versions)
        {
            dbContext.Entry(version).CurrentValues.SetValues(version with
            {
                IsDeleted = true,
                DeletedAt = deletedAt,
                DeletedByUserId = request.DeletedByUserId,
                DeletedReason = normalizedReason
            });
        }

        dbContext.Entry(document).CurrentValues.SetValues(document with
        {
            IsDeleted = true,
            DeletedAt = deletedAt,
            DeletedByUserId = request.DeletedByUserId,
            DeletedReason = normalizedReason,
            UpdatedAt = deletedAt
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("delete", "document", document.Id, StatusCodes.Status200OK, null, cancellationToken, normalizedReason);
        await TryAppendHistoryAsync(document.Id, "delete", new { document.Title }, null, "Deleted document", normalizedReason, null, cancellationToken);
        await TryAppendBusinessEventAsync("documents", "document.deleted", "document", document.Id.ToString(), "Deleted document", normalizedReason, new { document.Title }, cancellationToken);

        return DocumentDeleteResult.Success();
    }

    public async Task<DocumentTypeCommandResult> CreateDocumentTypeAsync(DocumentTypeCreateCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.ModuleOwner))
        {
            return DocumentTypeCommandResult.Fail(ApiErrorCodes.RequestValidationFailed, "Code, name, and module owner are required.");
        }

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var exists = await dbContext.DocumentTypes.AsNoTracking().AnyAsync(x => x.Code == normalizedCode, cancellationToken);
        if (exists)
        {
            return DocumentTypeCommandResult.Fail(ApiErrorCodes.Documents.DocumentTypeCodeExists, "Document type code already exists.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new DocumentTypeEntity
        {
            Id = Guid.NewGuid(),
            Code = normalizedCode,
            Name = request.Name.Trim(),
            ModuleOwner = request.ModuleOwner.Trim(),
            ClassificationDefault = NormalizeValue(request.ClassificationDefault, "internal"),
            RetentionClassDefault = NormalizeValue(request.RetentionClassDefault, "standard"),
            ApprovalRequired = request.ApprovalRequired,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.DocumentTypes.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("create", "document_type", entity.Id, StatusCodes.Status201Created, new { entity.Code, entity.Status }, cancellationToken);

        return DocumentTypeCommandResult.Success(ToDocumentTypeResponse(entity));
    }

    public async Task<DocumentTypeCommandResult> UpdateDocumentTypeAsync(DocumentTypeUpdateCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.DocumentTypes.SingleOrDefaultAsync(x => x.Id == request.DocumentTypeId, cancellationToken);
        if (entity is null)
        {
            return DocumentTypeCommandResult.Fail(ApiErrorCodes.Documents.DocumentTypeNotFound, "Document type not found.");
        }

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var exists = await dbContext.DocumentTypes.AsNoTracking().AnyAsync(x => x.Code == normalizedCode && x.Id != request.DocumentTypeId, cancellationToken);
        if (exists)
        {
            return DocumentTypeCommandResult.Fail(ApiErrorCodes.Documents.DocumentTypeCodeExists, "Document type code already exists.");
        }

        var updated = entity with
        {
            Code = normalizedCode,
            Name = request.Name.Trim(),
            ModuleOwner = request.ModuleOwner.Trim(),
            ClassificationDefault = NormalizeValue(request.ClassificationDefault, entity.ClassificationDefault),
            RetentionClassDefault = NormalizeValue(request.RetentionClassDefault, entity.RetentionClassDefault),
            ApprovalRequired = request.ApprovalRequired,
            Status = NormalizeDocumentTypeStatus(request.Status),
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Entry(entity).CurrentValues.SetValues(updated);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("update", "document_type", updated.Id, StatusCodes.Status200OK, new { updated.Code, updated.Status }, cancellationToken);

        return DocumentTypeCommandResult.Success(ToDocumentTypeResponse(updated));
    }

    public Task<DocumentWorkflowResult> SubmitDocumentAsync(DocumentWorkflowCommand request, CancellationToken cancellationToken) =>
        ApplyWorkflowAsync(request, "review", "submitted", "submit", request.StepName, request.ReviewerUserId, request.Reason, cancellationToken);

    public Task<DocumentWorkflowResult> ApproveDocumentAsync(DocumentWorkflowCommand request, CancellationToken cancellationToken) =>
        ApplyWorkflowAsync(request, "approved", "approved", "approve", request.StepName, request.ReviewerUserId, request.Reason, cancellationToken);

    public Task<DocumentWorkflowResult> RejectDocumentAsync(DocumentWorkflowCommand request, CancellationToken cancellationToken) =>
        ApplyWorkflowAsync(request, "rejected", "rejected", "reject", request.StepName, request.ReviewerUserId, request.Reason, cancellationToken);

    public Task<DocumentWorkflowResult> BaselineDocumentAsync(DocumentWorkflowCommand request, CancellationToken cancellationToken) =>
        ApplyWorkflowAsync(request, "baseline", null, "baseline", request.StepName, request.ReviewerUserId, request.Reason, cancellationToken);

    public Task<DocumentWorkflowResult> ArchiveDocumentAsync(DocumentWorkflowCommand request, CancellationToken cancellationToken) =>
        ApplyWorkflowAsync(request, "archived", null, "archive", request.StepName, request.ReviewerUserId, request.Reason, cancellationToken);

    public async Task<DocumentLinkCommandResult> CreateDocumentLinkAsync(DocumentLinkCreateCommand request, CancellationToken cancellationToken)
    {
        if (request.DocumentId == Guid.Empty)
        {
            return DocumentLinkCommandResult.Fail(ApiErrorCodes.Documents.DocumentNotFound, "Document not found.");
        }

        if (string.IsNullOrWhiteSpace(request.TargetEntityType) || string.IsNullOrWhiteSpace(request.TargetEntityId) || string.IsNullOrWhiteSpace(request.LinkType))
        {
            return DocumentLinkCommandResult.Fail(ApiErrorCodes.RequestValidationFailed, "Link target and type are required.");
        }

        var exists = await dbContext.Documents.AsNoTracking().AnyAsync(x => x.Id == request.DocumentId && !x.IsDeleted, cancellationToken);
        if (!exists)
        {
            return DocumentLinkCommandResult.Fail(ApiErrorCodes.Documents.DocumentNotFound, "Document not found.");
        }

        var entity = new DocumentLinkEntity
        {
            Id = Guid.NewGuid(),
            SourceDocumentId = request.DocumentId,
            TargetEntityType = request.TargetEntityType.Trim(),
            TargetEntityId = request.TargetEntityId.Trim(),
            LinkType = request.LinkType.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.DocumentLinks.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("link", "document", request.DocumentId, StatusCodes.Status201Created, new { entity.TargetEntityType, entity.TargetEntityId, entity.LinkType }, cancellationToken);

        return DocumentLinkCommandResult.Success(new DocumentLinkItem(entity.Id, entity.SourceDocumentId, entity.TargetEntityType, entity.TargetEntityId, entity.LinkType));
    }

    private async Task<DocumentWorkflowResult> ApplyWorkflowAsync(
        DocumentWorkflowCommand request,
        string targetDocumentStatus,
        string? targetVersionStatus,
        string auditAction,
        string? stepName,
        string? reviewerUserId,
        string? reason,
        CancellationToken cancellationToken)
    {
        var document = await dbContext.Documents.SingleOrDefaultAsync(x => x.Id == request.DocumentId && !x.IsDeleted, cancellationToken);
        if (document is null)
        {
            return DocumentWorkflowResult.Fail(ApiErrorCodes.Documents.DocumentNotFound, "Document not found.");
        }

        if (document.CurrentVersionId is null)
        {
            return DocumentWorkflowResult.Fail(ApiErrorCodes.Documents.DocumentVersionNotFound, "Document does not have a current version.");
        }

        var version = await dbContext.DocumentVersions.SingleOrDefaultAsync(x => x.Id == document.CurrentVersionId.Value && !x.IsDeleted, cancellationToken);
        if (version is null)
        {
            return DocumentWorkflowResult.Fail(ApiErrorCodes.Documents.DocumentVersionNotFound, "Document version not found.");
        }

        if (string.Equals(auditAction, "submit", StringComparison.OrdinalIgnoreCase))
        {
            if (!ReviewableDocumentStatuses.Contains(document.Status) || !ReviewableVersionStatuses.Contains(version.Status))
            {
                return DocumentWorkflowResult.Fail(ApiErrorCodes.Documents.WorkflowTransitionNotAllowed, "Document cannot be submitted from its current state.");
            }
        }
        else if (string.Equals(auditAction, "approve", StringComparison.OrdinalIgnoreCase) || string.Equals(auditAction, "reject", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.Equals(document.Status, "review", StringComparison.OrdinalIgnoreCase) || !string.Equals(version.Status, "submitted", StringComparison.OrdinalIgnoreCase))
            {
                return DocumentWorkflowResult.Fail(ApiErrorCodes.Documents.WorkflowTransitionNotAllowed, "Document is not in review.");
            }
        }
        else if (string.Equals(auditAction, "baseline", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.Equals(document.Status, "approved", StringComparison.OrdinalIgnoreCase))
            {
                return DocumentWorkflowResult.Fail(ApiErrorCodes.Documents.WorkflowTransitionNotAllowed, "Only approved documents can be baselined.");
            }
        }
        else if (string.Equals(auditAction, "archive", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.Equals(document.Status, "baseline", StringComparison.OrdinalIgnoreCase))
            {
                return DocumentWorkflowResult.Fail(ApiErrorCodes.Documents.WorkflowTransitionNotAllowed, "Only baselined documents can be archived.");
            }
        }

        if (string.Equals(auditAction, "reject", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(reason))
        {
            return DocumentWorkflowResult.Fail(ApiErrorCodes.Documents.ApprovalReasonRequired, "A rejection reason is required.");
        }

        var now = DateTimeOffset.UtcNow;
        var updatedDocument = document with
        {
            Status = targetDocumentStatus,
            UpdatedAt = now
        };
        dbContext.Entry(document).CurrentValues.SetValues(updatedDocument);

        if (targetVersionStatus is not null && !string.Equals(version.Status, targetVersionStatus, StringComparison.OrdinalIgnoreCase))
        {
            dbContext.Entry(version).CurrentValues.SetValues(version with { Status = targetVersionStatus });
        }

        if (string.Equals(auditAction, "submit", StringComparison.OrdinalIgnoreCase)
            || string.Equals(auditAction, "approve", StringComparison.OrdinalIgnoreCase)
            || string.Equals(auditAction, "reject", StringComparison.OrdinalIgnoreCase))
        {
            dbContext.DocumentApprovals.Add(new DocumentApprovalEntity
            {
                Id = Guid.NewGuid(),
                DocumentVersionId = version.Id,
                StepName = string.IsNullOrWhiteSpace(stepName) ? "document_review" : stepName.Trim(),
                ReviewerUserId = string.IsNullOrWhiteSpace(reviewerUserId) ? (request.ActorUserId ?? "unknown") : reviewerUserId.Trim(),
                Decision = auditAction switch
                {
                    "submit" => "pending",
                    "approve" => "approved",
                    "reject" => "rejected",
                    _ => "pending"
                },
                DecisionReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
                DecidedAt = string.Equals(auditAction, "submit", StringComparison.OrdinalIgnoreCase) ? null : now,
                CreatedAt = now
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync(auditAction, "document", document.Id, StatusCodes.Status200OK, new { Before = document.Status, After = targetDocumentStatus, Version = version.VersionNumber }, cancellationToken, reason);
        await TryAppendHistoryAsync(
            document.Id,
            auditAction,
            new { DocumentStatus = document.Status, VersionStatus = version.Status },
            new { DocumentStatus = targetDocumentStatus, VersionStatus = targetVersionStatus ?? version.Status },
            $"Document {auditAction}",
            reason,
            null,
            cancellationToken);
        await TryAppendBusinessEventAsync("documents", $"document.{auditAction}", "document", document.Id.ToString(), $"Document {auditAction}", reason, new { document.Id, version.VersionNumber }, cancellationToken);

        if (string.Equals(auditAction, "approve", StringComparison.OrdinalIgnoreCase))
        {
            await TryStartWorkflowInstancesForPublishedDocumentAsync(document.Id, request.ActorUserId, cancellationToken);
        }

        var response = await BuildDetailResponseAsync(document.Id, cancellationToken);
        return response is null
            ? DocumentWorkflowResult.Fail(ApiErrorCodes.Documents.DocumentNotFound, "Document not found.")
            : DocumentWorkflowResult.Success(response);
    }

    private async Task<DocumentListItem?> BuildListItemAsync(Guid documentId, CancellationToken cancellationToken)
    {
        return await (
            from document in dbContext.Documents.AsNoTracking()
            where document.Id == documentId && !document.IsDeleted
            join documentType in dbContext.DocumentTypes.AsNoTracking() on document.DocumentTypeId equals documentType.Id into typeJoin
            from documentType in typeJoin.DefaultIfEmpty()
            join project in dbContext.Projects.AsNoTracking() on document.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            join version in dbContext.DocumentVersions.AsNoTracking() on document.CurrentVersionId equals version.Id into versionJoin
            from version in versionJoin.DefaultIfEmpty()
            select new DocumentListItem(
                document.Id,
                document.Title,
                document.DocumentTypeId,
                documentType != null ? documentType.Code : null,
                documentType != null ? documentType.Name : null,
                document.ProjectId,
                project != null ? project.Name : null,
                document.PhaseCode,
                document.OwnerUserId,
                document.Status,
                document.Classification,
                document.RetentionClass,
                version != null ? version.VersionNumber : null,
                version != null ? version.Status : null,
                version != null ? version.FileName : null,
                version != null ? version.MimeType : null,
                version != null ? version.FileSize : null,
                document.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private async Task<DocumentDetailResponse?> BuildDetailResponseAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var item = await (
            from document in dbContext.Documents.AsNoTracking()
            where document.Id == documentId && !document.IsDeleted
            join documentType in dbContext.DocumentTypes.AsNoTracking() on document.DocumentTypeId equals documentType.Id into typeJoin
            from documentType in typeJoin.DefaultIfEmpty()
            join project in dbContext.Projects.AsNoTracking() on document.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            select new
            {
                Document = document,
                DocumentTypeCode = documentType != null ? documentType.Code : null,
                DocumentTypeName = documentType != null ? documentType.Name : null,
                ProjectName = project != null ? project.Name : null
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return null;
        }

        var versions = await dbContext.DocumentVersions.AsNoTracking()
            .Where(x => x.DocumentId == documentId && !x.IsDeleted)
            .OrderByDescending(x => x.VersionNumber)
            .Select(x => new DocumentVersionListItem(x.Id, x.DocumentId, x.VersionNumber, x.FileName, x.MimeType, x.FileSize, x.UploadedBy, x.UploadedAt, x.Status))
            .ToListAsync(cancellationToken);

        var versionIds = versions.Select(x => x.Id).ToList();
        var approvals = await dbContext.DocumentApprovals.AsNoTracking()
            .Where(x => versionIds.Contains(x.DocumentVersionId))
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new DocumentApprovalItem(x.Id, x.DocumentVersionId, x.StepName, x.ReviewerUserId, x.Decision, x.DecisionReason, x.DecidedAt))
            .ToListAsync(cancellationToken);

        var links = await dbContext.DocumentLinks.AsNoTracking()
            .Where(x => x.SourceDocumentId == documentId)
            .OrderBy(x => x.LinkType)
            .Select(x => new DocumentLinkItem(x.Id, x.SourceDocumentId, x.TargetEntityType, x.TargetEntityId, x.LinkType))
            .ToListAsync(cancellationToken);

        return new DocumentDetailResponse(
            item.Document.Id,
            item.Document.Title,
            item.Document.DocumentTypeId,
            item.DocumentTypeCode,
            item.DocumentTypeName,
            item.Document.ProjectId,
            item.ProjectName,
            item.Document.PhaseCode,
            item.Document.OwnerUserId,
            item.Document.Status,
            item.Document.Classification,
            item.Document.RetentionClass,
            DeserializeTags(item.Document.TagsJson),
            item.Document.CurrentVersionId,
            versions,
            approvals,
            links,
            item.Document.CreatedAt,
            item.Document.UpdatedAt);
    }

    private async Task TryStartWorkflowInstancesForPublishedDocumentAsync(Guid documentId, string? actorUserId, CancellationToken cancellationToken)
    {
        var workflowDefinitionIds = await dbContext.WorkflowSteps
            .AsNoTracking()
            .Where(step => step.DocumentId == documentId)
            .Select(step => step.WorkflowDefinitionId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (workflowDefinitionIds.Count == 0)
        {
            return;
        }

        var projects = await dbContext.Projects
            .AsNoTracking()
            .Where(project => project.WorkflowDefinitionId.HasValue && project.DeletedAt == null && workflowDefinitionIds.Contains(project.WorkflowDefinitionId.Value))
            .Select(project => new { project.Id, project.WorkflowDefinitionId })
            .ToListAsync(cancellationToken);

        foreach (var project in projects)
        {
            if (!project.WorkflowDefinitionId.HasValue)
            {
                continue;
            }

            await workflowInstanceCommands.CreateInstanceAsync(new CreateWorkflowInstanceRequest(project.Id, documentId, project.WorkflowDefinitionId), actorUserId, null, null, cancellationToken);
        }
    }

    private static DocumentTypeResponse ToDocumentTypeResponse(DocumentTypeEntity entity) =>
        new(entity.Id, entity.Code, entity.Name, entity.ModuleOwner, entity.ClassificationDefault, entity.RetentionClassDefault, entity.Status, entity.ApprovalRequired, entity.CreatedAt, entity.UpdatedAt);

    private async Task<bool> ValidateDocumentTypeAsync(Guid documentTypeId, CancellationToken cancellationToken) =>
        documentTypeId != Guid.Empty
        && await dbContext.DocumentTypes.AsNoTracking().AnyAsync(x => x.Id == documentTypeId && x.Status == "active", cancellationToken);

    private async Task<bool> ValidateProjectAsync(Guid projectId, CancellationToken cancellationToken) =>
        projectId != Guid.Empty
        && await dbContext.Projects.AsNoTracking().AnyAsync(x => x.Id == projectId && x.DeletedAt == null, cancellationToken);

    private async Task AppendAuditAsync(
        string action,
        string entityType,
        Guid entityId,
        int statusCode,
        object? metadata,
        CancellationToken cancellationToken,
        string? reason = null)
    {
        auditLogWriter.Append(new AuditLogEntry(
            Module: "documents",
            Action: action,
            EntityType: entityType,
            EntityId: entityId.ToString(),
            StatusCode: statusCode,
            Reason: reason,
            Metadata: metadata));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task TryAppendHistoryAsync(
        Guid documentId,
        string eventType,
        object? before,
        object? after,
        string? summary,
        string? reason,
        object? metadata,
        CancellationToken cancellationToken)
    {
        try
        {
            await historyWriter.AppendAsync(documentId, eventType, before, after, summary, reason, metadata, cancellationToken);
        }
        catch
        {
            // Best-effort history; avoid failing the primary flow.
        }
    }

    private async Task TryAppendBusinessEventAsync(
        string module,
        string eventType,
        string entityType,
        string? entityId,
        string? summary,
        string? reason,
        object? metadata,
        CancellationToken cancellationToken)
    {
        try
        {
            await businessAuditEventWriter.AppendAsync(module, eventType, entityType, entityId, summary, reason, metadata, cancellationToken);
        }
        catch
        {
            // Best-effort business audit.
        }
    }

    private static string SerializeTags(IReadOnlyList<string> tags) =>
        JsonSerializer.Serialize(tags.Where(static tag => !string.IsNullOrWhiteSpace(tag)).Select(static tag => tag.Trim()).Distinct(StringComparer.OrdinalIgnoreCase), SerializerOptions);

    private static IReadOnlyList<string> DeserializeTags(string? json) =>
        string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<string>>(json, SerializerOptions) ?? [];

    private static string NormalizeValue(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string NormalizeDocumentTypeStatus(string? status) =>
        string.Equals(status, "deprecated", StringComparison.OrdinalIgnoreCase) ? "deprecated" : "active";
}
