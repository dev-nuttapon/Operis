using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Documents.Contracts;
using Operis_API.Modules.Documents.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Documents.Application;

public sealed class DocumentCommands(
    OperisDbContext dbContext,
    IDocumentObjectStorage documentObjectStorage,
    IAuditLogWriter auditLogWriter,
    IOptions<DocumentStorageOptions> optionsAccessor) : IDocumentCommands
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".doc",
        ".docx",
        ".xls",
        ".xlsx"
    };

    public async Task<DocumentUploadResult> CreateDocumentAsync(DocumentCreateCommand request, CancellationToken cancellationToken)
    {
        var options = optionsAccessor.Value;

        if (string.IsNullOrWhiteSpace(request.DocumentName))
        {
            return DocumentUploadResult.Fail(ApiErrorCodes.Documents.NameRequired, "A document name is required.");
        }

        var normalizedDocumentName = request.DocumentName.Trim();

        var entity = new DocumentEntity
        {
            Id = Guid.NewGuid(),
            DocumentName = normalizedDocumentName,
            UploadedByUserId = request.CreatedByUserId,
            UploadedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };

        dbContext.Documents.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new DocumentListItem(
            entity.Id,
            entity.DocumentName,
            string.Empty,
            "application/octet-stream",
            0,
            entity.UploadedByUserId,
            entity.UploadedAt,
            null,
            null,
            null,
            null);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "documents",
            Action: "create",
            EntityType: "document",
            EntityId: entity.Id.ToString(),
            StatusCode: StatusCodes.Status201Created,
            After: new
            {
                entity.Id,
            entity.DocumentName,
            entity.UploadedByUserId,
            entity.UploadedAt
        }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return DocumentUploadResult.Success(response);
    }

    public async Task<DocumentVersionCreateResult> CreateDocumentVersionAsync(
        DocumentVersionCreateCommand request,
        Stream content,
        CancellationToken cancellationToken)
    {
        var options = optionsAccessor.Value;

        if (request.DocumentId == Guid.Empty)
        {
            return DocumentVersionCreateResult.Fail(ApiErrorCodes.Documents.DocumentNotFound, "Document not found.");
        }

        if (string.IsNullOrWhiteSpace(request.VersionCode))
        {
            return DocumentVersionCreateResult.Fail(ApiErrorCodes.Documents.VersionCodeRequired, "A version code is required.");
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

        var document = await dbContext.Documents
            .SingleOrDefaultAsync(x => x.Id == request.DocumentId && !x.IsDeleted, cancellationToken);

        if (document is null)
        {
            return DocumentVersionCreateResult.Fail(ApiErrorCodes.Documents.DocumentNotFound, "Document not found.");
        }

        var normalizedVersionCode = request.VersionCode.Trim();
        var normalizedFileName = Path.GetFileName(request.FileName.Trim());
        var fileExtension = Path.GetExtension(normalizedFileName);
        if (string.IsNullOrWhiteSpace(fileExtension) || !AllowedExtensions.Contains(fileExtension))
        {
            return DocumentVersionCreateResult.Fail(
                ApiErrorCodes.Documents.FileTypeNotAllowed,
                "Only PDF, Word, and Excel documents are allowed.");
        }

        var versionCodeExists = await dbContext.DocumentVersions
            .AsNoTracking()
            .AnyAsync(x => x.DocumentId == request.DocumentId && x.VersionCode == normalizedVersionCode && !x.IsDeleted, cancellationToken);

        if (versionCodeExists)
        {
            return DocumentVersionCreateResult.Fail(ApiErrorCodes.Documents.VersionCodeExists, "Version code already exists.");
        }

        var currentRevision = await dbContext.DocumentVersions
            .Where(x => x.DocumentId == request.DocumentId && !x.IsDeleted)
            .Select(x => (int?)x.Revision)
            .MaxAsync(cancellationToken);

        var nextRevision = (currentRevision ?? 0) + 1;
        var objectKey = $"documents/{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid():N}-{normalizedFileName}";
        var contentType = string.IsNullOrWhiteSpace(request.ContentType) ? "application/octet-stream" : request.ContentType.Trim();

        await documentObjectStorage.StoreAsync(objectKey, content, request.Size, contentType, cancellationToken);

        var versionEntity = new DocumentVersionEntity
        {
            Id = Guid.NewGuid(),
            DocumentId = request.DocumentId,
            Revision = nextRevision,
            VersionCode = normalizedVersionCode,
            FileName = normalizedFileName,
            ObjectKey = objectKey,
            BucketName = options.BucketName,
            ContentType = contentType,
            SizeBytes = request.Size,
            UploadedByUserId = request.UploadedByUserId,
            UploadedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };

        dbContext.DocumentVersions.Add(versionEntity);

        if (document.PublishedVersionId is null)
        {
            dbContext.Entry(document).CurrentValues.SetValues(document with { PublishedVersionId = versionEntity.Id });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new DocumentVersionListItem(
            versionEntity.Id,
            versionEntity.DocumentId,
            versionEntity.Revision,
            versionEntity.VersionCode,
            versionEntity.FileName,
            versionEntity.ContentType,
            versionEntity.SizeBytes,
            versionEntity.UploadedByUserId,
            versionEntity.UploadedAt,
            document.PublishedVersionId == versionEntity.Id);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "documents",
            Action: "create_version",
            EntityType: "document_version",
            EntityId: versionEntity.Id.ToString(),
            StatusCode: StatusCodes.Status201Created,
            After: new
            {
                versionEntity.Id,
                versionEntity.DocumentId,
                versionEntity.Revision,
                versionEntity.VersionCode,
                versionEntity.FileName,
                versionEntity.ContentType,
                versionEntity.SizeBytes,
                versionEntity.UploadedByUserId,
                versionEntity.UploadedAt
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return DocumentVersionCreateResult.Success(response);
    }

    public async Task<DocumentVersionDeleteResult> DeleteDocumentVersionAsync(DocumentVersionDeleteCommand request, CancellationToken cancellationToken)
    {
        if (request.DocumentId == Guid.Empty || request.VersionId == Guid.Empty)
        {
            return DocumentVersionDeleteResult.Fail(ApiErrorCodes.Documents.DocumentVersionNotFound, "Document version not found.");
        }

        var version = await dbContext.DocumentVersions
            .SingleOrDefaultAsync(x => x.Id == request.VersionId && x.DocumentId == request.DocumentId && !x.IsDeleted, cancellationToken);

        if (version is null)
        {
            return DocumentVersionDeleteResult.Fail(ApiErrorCodes.Documents.DocumentVersionNotFound, "Document version not found.");
        }

        var deletedAt = DateTimeOffset.UtcNow;
        dbContext.Entry(version).CurrentValues.SetValues(version with
        {
            IsDeleted = true,
            DeletedAt = deletedAt,
            DeletedByUserId = request.DeletedByUserId
        });

        var document = await dbContext.Documents
            .SingleOrDefaultAsync(x => x.Id == request.DocumentId && !x.IsDeleted, cancellationToken);

        if (document is not null && document.PublishedVersionId == version.Id)
        {
            dbContext.Entry(document).CurrentValues.SetValues(document with { PublishedVersionId = null });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "documents",
            Action: "delete_version",
            EntityType: "document_version",
            EntityId: version.Id.ToString(),
            StatusCode: StatusCodes.Status200OK));
        await dbContext.SaveChangesAsync(cancellationToken);

        return DocumentVersionDeleteResult.Success();
    }

    public async Task<DocumentUpdateResult> UpdateDocumentAsync(DocumentUpdateCommand request, CancellationToken cancellationToken)
    {
        if (request.DocumentId == Guid.Empty)
        {
            return DocumentUpdateResult.Fail(ApiErrorCodes.Documents.DocumentNotFound, "Document not found.");
        }

        if (string.IsNullOrWhiteSpace(request.DocumentName))
        {
            return DocumentUpdateResult.Fail(ApiErrorCodes.Documents.NameRequired, "A document name is required.");
        }

        var normalizedName = request.DocumentName.Trim();
        var exists = await dbContext.Documents
            .AsNoTracking()
            .AnyAsync(x => x.DocumentName == normalizedName && x.Id != request.DocumentId, cancellationToken);

        if (exists)
        {
            return DocumentUpdateResult.Fail(ApiErrorCodes.Documents.DocumentNameExists, "Document name already exists.");
        }

        var entity = await dbContext.Documents
            .SingleOrDefaultAsync(x => x.Id == request.DocumentId && !x.IsDeleted, cancellationToken);

        if (entity is null)
        {
            return DocumentUpdateResult.Fail(ApiErrorCodes.Documents.DocumentNotFound, "Document not found.");
        }

        var updated = entity with { DocumentName = normalizedName };
        dbContext.Entry(entity).CurrentValues.SetValues(updated);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new DocumentListItem(
            updated.Id,
            updated.DocumentName,
            string.Empty,
            "application/octet-stream",
            0,
            updated.UploadedByUserId,
            updated.UploadedAt,
            null,
            null,
            null,
            null);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "documents",
            Action: "update",
            EntityType: "document",
            EntityId: updated.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            After: new
            {
                updated.Id,
                updated.DocumentName,
                updated.UploadedByUserId,
                updated.UploadedAt
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

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

        var document = await dbContext.Documents
            .SingleOrDefaultAsync(x => x.Id == request.DocumentId && !x.IsDeleted, cancellationToken);

        if (document is null)
        {
            return DocumentDeleteResult.Fail(ApiErrorCodes.Documents.DocumentNotFound, "Document not found.");
        }

        var versions = await dbContext.DocumentVersions
            .Where(x => x.DocumentId == request.DocumentId && !x.IsDeleted)
            .ToListAsync(cancellationToken);
        var deletedAt = DateTimeOffset.UtcNow;
        var normalizedReason = request.DeletedReason.Trim();
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

        var deletedDocument = document with
        {
            IsDeleted = true,
            DeletedAt = deletedAt,
            DeletedByUserId = request.DeletedByUserId,
            DeletedReason = normalizedReason
        };

        dbContext.Entry(document).CurrentValues.SetValues(deletedDocument);
        await dbContext.SaveChangesAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "documents",
            Action: "delete",
            EntityType: "document",
            EntityId: request.DocumentId.ToString(),
            StatusCode: StatusCodes.Status200OK,
            Reason: normalizedReason));
        await dbContext.SaveChangesAsync(cancellationToken);

        return DocumentDeleteResult.Success();
    }
}
