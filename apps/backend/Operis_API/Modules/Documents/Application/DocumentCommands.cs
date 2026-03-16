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

    public async Task<DocumentUploadResult> UploadDocumentAsync(DocumentUploadRequest request, Stream content, CancellationToken cancellationToken)
    {
        var options = optionsAccessor.Value;

        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            return DocumentUploadResult.Fail(ApiErrorCodes.Documents.FileRequired, "A file is required.");
        }

        if (request.Size <= 0)
        {
            return DocumentUploadResult.Fail(ApiErrorCodes.Documents.FileEmpty, "The uploaded file is empty.");
        }

        if (request.Size > options.MaxFileSizeBytes)
        {
            return DocumentUploadResult.Fail(ApiErrorCodes.Documents.FileTooLarge, "The uploaded file exceeds the configured size limit.");
        }

        var normalizedFileName = Path.GetFileName(request.FileName.Trim());
        var fileExtension = Path.GetExtension(normalizedFileName);
        if (string.IsNullOrWhiteSpace(fileExtension) || !AllowedExtensions.Contains(fileExtension))
        {
            return DocumentUploadResult.Fail(
                ApiErrorCodes.Documents.FileTypeNotAllowed,
                "Only PDF, Word, and Excel documents are allowed.");
        }

        var objectKey = $"documents/{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid():N}-{normalizedFileName}";
        var contentType = string.IsNullOrWhiteSpace(request.ContentType) ? "application/octet-stream" : request.ContentType.Trim();

        await documentObjectStorage.StoreAsync(objectKey, content, request.Size, contentType, cancellationToken);

        var entity = new DocumentEntity
        {
            Id = Guid.NewGuid(),
            FileName = normalizedFileName,
            ObjectKey = objectKey,
            BucketName = options.BucketName,
            ContentType = contentType,
            SizeBytes = request.Size,
            UploadedByUserId = request.UploadedByUserId,
            UploadedAt = DateTimeOffset.UtcNow
        };

        dbContext.Documents.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new DocumentListItem(entity.Id, entity.FileName, entity.ContentType, entity.SizeBytes, entity.UploadedByUserId, entity.UploadedAt);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "documents",
            Action: "create",
            EntityType: "document",
            EntityId: entity.Id.ToString(),
            StatusCode: StatusCodes.Status201Created,
            After: new
            {
                entity.Id,
                entity.FileName,
                entity.ContentType,
                entity.SizeBytes,
                entity.UploadedByUserId,
                entity.UploadedAt
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return DocumentUploadResult.Success(response);
    }
}
