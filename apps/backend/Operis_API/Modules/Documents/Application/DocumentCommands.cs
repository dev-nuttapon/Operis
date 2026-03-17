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
            FileName = string.Empty,
            ObjectKey = null,
            BucketName = options.BucketName,
            ContentType = "application/octet-stream",
            SizeBytes = 0,
            UploadedByUserId = request.CreatedByUserId,
            UploadedAt = DateTimeOffset.UtcNow
        };

        dbContext.Documents.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new DocumentListItem(
            entity.Id,
            entity.DocumentName,
            entity.FileName,
            entity.ContentType,
            entity.SizeBytes,
            entity.UploadedByUserId,
            entity.UploadedAt,
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
                entity.FileName,
                entity.ContentType,
                entity.SizeBytes,
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

        var documentExists = await dbContext.Documents
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.DocumentId, cancellationToken);

        if (!documentExists)
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
            .AnyAsync(x => x.DocumentId == request.DocumentId && x.VersionCode == normalizedVersionCode, cancellationToken);

        if (versionCodeExists)
        {
            return DocumentVersionCreateResult.Fail(ApiErrorCodes.Documents.VersionCodeExists, "Version code already exists.");
        }

        var currentRevision = await dbContext.DocumentVersions
            .Where(x => x.DocumentId == request.DocumentId)
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
            UploadedAt = DateTimeOffset.UtcNow
        };

        dbContext.DocumentVersions.Add(versionEntity);
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
            versionEntity.UploadedAt);

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
}
